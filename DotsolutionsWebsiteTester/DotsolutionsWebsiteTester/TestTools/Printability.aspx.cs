using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class Printability : System.Web.UI.Page
    {
        private List<string> sitemap;
        private List<Thread> ThreadList = new List<Thread>();
        int printablePages = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Session["MainUrl"].ToString();
            }
            catch (NullReferenceException)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }
            this.sitemap = (List<string>)Session["selectedSites"];


            foreach (string url in sitemap)
            {
                ThreadStart ths = new ThreadStart(() => TestPrintability(url));
                Thread th = new Thread(ths);
                ThreadList.Add(th);
                th.Start();
            }

            foreach (Thread th in ThreadList)
            {
                th.Join();
            }

            Debug.WriteLine("Hier kom ik nog wel!");
            Debug.WriteLine("printablePages =======> " + printablePages);

            if (printablePages >= sitemap.Count)
                PrintResults.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span>Er is rekening gehouden met de printbaarheid van de webiste.</span></div>";

            if (printablePages < sitemap.Count)
                PrintResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg'></i>"
                    + "<span>Er is geen rekening gehouden met de printbaarheid van de webiste in de aangetroffen CSS op de volgende pagina's:</span></div>";



            var sb = new System.Text.StringBuilder();
            PrintabilitySession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Printability"] = htmlstring;
        }

        /// <summary>
        /// Test a single page
        /// </summary>
        /// <param name="url"></param>
        private void TestPrintability(string url)
        {
            Debug.WriteLine("Printbaarheid test op ---> " + url);
            
            List<Thread> SubThreadList = new List<Thread>();
            List<string> cssList = new List<string>();
            var Webget = new HtmlWeb();
            var doc = Webget.Load(url);
            string stylesheet = "";
            bool found = false;

            if (doc.DocumentNode.SelectNodes("//link[@rel]") != null)
            {
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//link[@rel]"))
                {
                    if (node.Attributes["rel"].Value == "stylesheet")
                    {
                        if (!node.Attributes["href"].Value.Contains("https://") && !node.Attributes["href"].Value.Contains("http://"))
                        {
                            stylesheet = Session["MainUrl"].ToString() + node.Attributes["href"].Value;
                            //System.Diagnostics.Debug.WriteLine(Session["MainUrl"].ToString() + node.Attributes["href"].Value);
                        }
                        if (!cssList.Contains(stylesheet) && stylesheet.Length > 0)
                        {
                            cssList.Add(stylesheet);
                            found = true;
                        }
                    }
                }
            }

            if (!found)
            {
                AddToTable("Geen CSS aangetroffen en er wordt hierdoor waarschijnlijk geen rekening gehouden met de printbaarheid van de pagina.", url, "-");
            }
            else
            {
                // Check if CSS has option for @media print
                foreach (string cssUrl in cssList)
                {
                    ThreadStart ths = new ThreadStart(() => TestCss(url, cssUrl));
                    Thread th = new Thread(ths);
                    SubThreadList.Add(th);
                    th.Start();
                }

                Debug.WriteLine("ergens in een thread");

                foreach (Thread th in SubThreadList)
                {
                    th.Join();
                }
                Debug.WriteLine("ergens na een thread");
            }
        }

        private void TestCss(string url, string cssUrl)
        {
            Debug.WriteLine("Getting content of: " + cssUrl + " from: " + url);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cssUrl);
            request.UserAgent = Session["userAgent"].ToString();
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content. 
            string responseFromServer = reader.ReadToEnd();

            if (!responseFromServer.Contains("@media print"))
            {
                AddToTable("Er is geen rekening gehouden met de printbaarheid van de webiste in de aangetroffen CSS.", url, cssUrl);
            }
            else
            {
                printablePages++;
            }
        }

        private void AddToTable(string msg, string url, string cssUrl)
        {
            TableRow tRow = new TableRow();

            TableCell tCellMsg = new TableCell();
            tCellMsg.Text = msg;
            tRow.Cells.Add(tCellMsg);

            TableCell tCellUrl = new TableCell();
            tCellUrl.Text = url;
            tRow.Cells.Add(tCellUrl);

            TableCell tCellCssUrl = new TableCell();
            tCellCssUrl.Text = cssUrl;
            tRow.Cells.Add(tCellCssUrl);

            PrintabilityTable.Rows.Add(tRow);
            PrintabilityTableHidden.Attributes.Remove("class");
        }
    }
}
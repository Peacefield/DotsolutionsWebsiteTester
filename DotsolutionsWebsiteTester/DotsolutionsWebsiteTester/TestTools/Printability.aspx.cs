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
        private List<string> printable = new List<string>();
        private List<string> notPrintable = new List<string>();
        private List<Thread> threadList = new List<Thread>();
        private int printablePages = 0;

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

            return;


            this.sitemap = (List<string>)Session["selectedSites"];

            foreach (var url in sitemap)
            {
                var ths = new ThreadStart(() => TestPrintability(url));
                var th = new Thread(ths);
                threadList.Add(th);
                th.Start();
            }

            foreach (var th in threadList)
            {
                th.Join();
            }

            if (printable.Count > 0)
            {
                string printablelist = "";
                foreach (string item in printable)
                {
                    printablelist += "<li>" + item + "</li>";
                }
                if (printable.Count < sitemap.Count)
                {
                    PrintResults.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                        + "<span>Er is rekening gehouden met de printbaarheid van de volgende pagina's:</span>"
                        + "<ul>" + printablelist + "</ul></div>";
                }
                else
                    PrintResults.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                        + "<span>Er is rekening gehouden met de printbaarheid op alle geteste pagina's</span></div>";
            }

            if (printablePages < sitemap.Count)
            {
                string notprintablelist = "";
                foreach (var item in notPrintable)
                {
                    notprintablelist += "<li>" + item + "</li>";
                }
                string amount = "";
                if ((sitemap.Count - printablePages) > 1)
                    amount = "bevatten " + (sitemap.Count - printablePages) + " pagina's";
                else
                    amount = "bevat " + (sitemap.Count - printablePages) + " pagina";

                PrintResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg'></i>"
                    + "<span>Van de " + sitemap.Count + " geteste pagina's " + amount + " geen CSS die rekening houdt met de printbaarheid:</span>"
                    + "<ul>" + notprintablelist + "</ul></div>";
            }

            var sb = new System.Text.StringBuilder();
            PrintabilitySession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Printability"] = htmlstring;
        }

        /// <summary>
        /// Test a single page on printability
        /// </summary>
        /// <param name="url"></param>
        private void TestPrintability(string url)
        {
            Debug.WriteLine("Printbaarheid test op ---> " + url);

            var subThreadList = new List<Thread>();
            var cssList = new List<string>();
            var webget = new HtmlWeb();
            var doc = webget.Load(url);
            string stylesheet = "";
            bool found = false;

            if (doc.DocumentNode.SelectNodes("//link[@rel]") != null)
            {
                foreach (var node in doc.DocumentNode.SelectNodes("//link[@rel]"))
                {
                    if (node.Attributes["rel"].Value == "stylesheet")
                    {
                        string hrefstring = node.Attributes["href"].Value;

                        Uri uri = new Uri(Session["MainUrl"].ToString());
                        string scheme = uri.Scheme;
                        string host = uri.Host;
                        string baseUrl = scheme + "://" + host + "/";

                        if (hrefstring.Contains("//"))
                        {
                            if (hrefstring.Contains("https://") || hrefstring.Contains("http://"))
                            {
                                stylesheet = node.Attributes["href"].Value;
                            }
                            else if (!hrefstring.Contains("https://") && !hrefstring.Contains("http://"))
                            {
                                try
                                {
                                    Debug.WriteLine("Proberen in TestPrintability ---> " + hrefstring);
                                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(hrefstring);
                                    stylesheet = hrefstring;
                                }
                                catch (InvalidCastException icex)
                                {
                                    Debug.WriteLine("InvalidCastException in TestPrintability ---> " + icex.Message);
                                    //add baseurl since it apparently is not external
                                    stylesheet = hrefstring.Replace("//", scheme + "://");
                                    Debug.WriteLine("stylesheet in TestPrintability ---> " + stylesheet);
                                }
                            }
                        }
                        else
                        {
                            stylesheet = baseUrl + hrefstring;
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
                notPrintable.Add(url);
            }
            else
            {
                // Check if CSS has option for @media print
                foreach (var cssUrl in cssList)
                {
                    var ths = new ThreadStart(() => TestCss(url, cssUrl));
                    var th = new Thread(ths);
                    subThreadList.Add(th);
                    th.Start();
                }


                foreach (Thread th in subThreadList)
                {
                    th.Join();
                }
            }
        }

        private void TestCss(string url, string cssUrl)
        {
            Debug.WriteLine("Getting content of: " + cssUrl + " from: " + url);
            //string encoded = WebUtility.UrlEncode(cssUrl);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cssUrl);
            request.UserAgent = Session["userAgent"].ToString();
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            var reader = new StreamReader(dataStream);
            // Read the content. 
            string responseFromServer = reader.ReadToEnd();

            if (!responseFromServer.Contains("@media print"))
            {
                AddToTable("Er is geen rekening gehouden met de printbaarheid in de aangetroffen CSS.", url, cssUrl);
                if (!printable.Contains(url))
                    notPrintable.Add(url);
            }
            else
            {
                printablePages++;
                if (!printable.Contains(url))
                    printable.Add(url);

                if (notPrintable.Contains(url))
                    notPrintable.Remove(url);

            }
        }

        private void AddToTable(string msg, string url, string cssUrl)
        {
            var tRow = new TableRow();

            var tCellMsg = new TableCell();
            tCellMsg.Text = msg;
            tRow.Cells.Add(tCellMsg);

            var tCellUrl = new TableCell();
            tCellUrl.Text = url;
            tRow.Cells.Add(tCellUrl);

            var tCellCssUrl = new TableCell();
            tCellCssUrl.Text = "<a href='" + cssUrl + "' target='_blank' >" + cssUrl + "</a>";
            tRow.Cells.Add(tCellCssUrl);

            PrintabilityTable.Rows.Add(tRow);
            PrintabilityTableHidden.Attributes.Remove("class");
        }
    }
}
﻿using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class CodeQuality : System.Web.UI.Page
    {
        private List<string> sitemap;
        private int errorCnt = 0;
        private int warningCnt = 0;

        private List<System.Threading.Thread> ThreadList = new List<System.Threading.Thread>();

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

            System.Diagnostics.Debug.WriteLine(">>>> CodeQuality");

            System.Threading.ThreadStart ths = new System.Threading.ThreadStart(TestCodeQuality);
            System.Threading.Thread th = new System.Threading.Thread(ths);
            th.Start();

            th.Join();

            var sb = new System.Text.StringBuilder();
            CodeQualitySession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["CodeQuality"] = htmlstring;
        }

        private void TestCodeQuality()
        {
            List<string> TableLayOutList = new List<string>();
            List<string> NoSemanticList = new List<string>();

            foreach (string url in this.sitemap)
            {
                System.Threading.ThreadStart ths = new System.Threading.ThreadStart(() => W3CValidate(url));
                System.Threading.Thread th = new System.Threading.Thread(ths);
                ThreadList.Add(th);
                th.Start();
                
                HtmlWeb Webget = new HtmlWeb();
                HtmlDocument doc = Webget.Load(url);

                if (IsTableLayout(url, doc))
                {
                    w3ErrorsFound.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg'></i>"
                        + "<span>Pagina " + url + " gebruikt misschien een tabel voor layout. Dit wordt over het algemeen beschouwd als bad practice.</span></div>";

                    TableLayOutList.Add(url);
                }

                if (!IsUsingSemantics(url, doc))
                {
                    NoSemanticList.Add(url);
                }
            }

            if (TableLayOutList.Count == 0)
            {
                w3ErrorsFound.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span>Er wordt op alle pagina's waarschijnlijk geen tabel gebruikt voor layout.</span></div>";
            }

            if (NoSemanticList.Count == 0)
            {
                w3ErrorsFound.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span>Er wordt op alle pagina's gebruik gemaakt van semantic HTML elements</span></div>";
            }
            else
            {
                string unorderedlist = "<ul>";

                foreach (string item in NoSemanticList)
                {
                    unorderedlist += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";
                }

                unorderedlist += "</ul>";

                w3ErrorsFound.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg'></i>"
                    + "<span>De volgende pagina's gebruiken geen semantic HTML elements</span> " + unorderedlist + "</div>";
            }

            // Join Threads
            foreach (System.Threading.Thread thread in ThreadList)
                thread.Join();

            // Show table when errors are found 
            if (errorCnt > 0 || warningCnt > 0)
            {
                W3ResultsTableHidden.Attributes.Remove("class");
                w3ErrorsFound.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg'></i>"
                    + "<span>" + errorCnt + " errors en " + warningCnt + " waarschuwingen gevonden.</span></div>";
            }
            else
            {
                w3ErrorsFound.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span> " + errorCnt + " W3C meldingen gevonden.</span></div>";
            }
        }

        private void W3CValidate(string url)
        {
            System.Diagnostics.Debug.WriteLine("W3CValidate <<<<<");
            System.Diagnostics.Debug.WriteLine("Performing code quality check on: " + url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://validator.w3.org/check?uri=" + url + "&output=json");
            request.UserAgent = Session["userAgent"].ToString();
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content. 
            string responseFromServer = reader.ReadToEnd();

            JObject w3Validate = JObject.Parse(responseFromServer);
            IList<JToken> messages = w3Validate["messages"].Children().ToList();

            foreach (JToken item in messages)
            {
                if (item["type"].ToString() == "error")
                {
                    errorCnt++;
                    try
                    {
                        // add error message to table
                        AddToTable(url, item["type"].ToString(), item["lastLine"].ToString(), item["lastColumn"].ToString(), item["message"].ToString());
                    }
                    catch (NullReferenceException nre)
                    {
                        System.Diagnostics.Debug.WriteLine("W3CValidate nullreference exception");
                        System.Diagnostics.Debug.WriteLine(nre.Message);
                    }
                }
                else if (item["type"].ToString() == "info")
                {
                    try
                    {
                        if (item["subType"].ToString() == "warning")
                        {
                            warningCnt++;
                            try
                            {
                                // add warning message to table
                                AddToTable(url, item["subType"].ToString(), "", "", item["message"].ToString());
                            }
                            catch (NullReferenceException nrex)
                            {
                                System.Diagnostics.Debug.WriteLine("Could not add to table or list<keyvaluepair> due to: " + nrex.Message);
                            }
                        }
                    }
                    catch (NullReferenceException nre)
                    {
                        System.Diagnostics.Debug.WriteLine("Just an info message, not a warning so subType is not available: " + nre.Message);
                    }
                }
            }

            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
        }

        /// <summary>
        /// Add the error/warning message to the table
        /// </summary>
        /// <param name="url">URL of origin of the message</param>
        /// <param name="type">Error or warning</param>
        /// <param name="line">Last Line</param>
        /// <param name="column">Last Column</param>
        /// <param name="msg">Additional error/warning message</param>
        private void AddToTable(string url, string type, string line, string column, string msg)
        {
            TableRow tRow = new TableRow();

            TableCell tCellUrl = new TableCell();
            tCellUrl.Text = url;
            tRow.Cells.Add(tCellUrl);

            TableCell tCellType = new TableCell();
            tCellType.Text = type;
            tRow.Cells.Add(tCellType);

            TableCell tCellLine = new TableCell();
            tCellLine.Text = line;
            tRow.Cells.Add(tCellLine);

            TableCell tCellClmn = new TableCell();
            tCellClmn.Text = column;
            tRow.Cells.Add(tCellClmn);

            TableCell tCellMsg = new TableCell();
            tCellMsg.Text = msg;
            tRow.Cells.Add(tCellMsg);

            table.Rows.Add(tRow);
        }

        private bool IsTableLayout(string url, HtmlDocument doc)
        {
            // Check every url in sitemap for analytics software for the current analytictype
            if (doc.DocumentNode.SelectNodes("//table[not(ancestor::table)]") != null)
            {
                if (doc.DocumentNode.SelectNodes("//table[not(ancestor::table)]").Count == 1)
                    return true; // Only one table found at upper level. May be used for layout
                else
                    return false; // Not null so tables are used at upper level, but multiple tables are used separately so it's probably proper use
            }
            else
                return false; // No table found in upper level
        }


        private bool IsUsingSemantics(string url, HtmlDocument doc)
        {
            System.Diagnostics.Debug.WriteLine("IsUsingSemantics <<<<<");

            List<string> semantics = new List<string>()
            {
                "article>", "aside>", "details>", "figcaption>", "figure>", "footer>", "form>", "header>", "img>", "main>", "mark>", "nav>", "section>", "summary>", "table>", "time>"
            };

            if (doc.DocumentNode.SelectSingleNode("//body") != null)
            {
                foreach (string item in semantics)
                {
                    if (doc.DocumentNode.SelectSingleNode("//body").InnerHtml.Contains(item))
                        return true;
                }
            }
            return false;
        }
    }
}
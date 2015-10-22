using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class CodeQuality : System.Web.UI.Page
    {
        private List<string> sitemap;
        private List<string> notW3cCompliant = new List<string>();
        private int errorCnt = 0;
        private int warningCnt = 0;
        private bool reqSuccess = true;

        private List<Thread> threadList = new List<Thread>();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Session["MainUrl"].ToString();
            }
            catch (NullReferenceException)
            {
                Response.Redirect("~/");
                return;
            }
            Debug.WriteLine(">>>> CodeQuality");

            this.sitemap = (List<string>)Session["selectedSites"];

            var ths = new ThreadStart(TestCodeQuality);
            var th = new Thread(ths);
            th.Start();

            th.Join();

            var sb = new System.Text.StringBuilder();
            CodeQualitySession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["CodeQuality"] = htmlstring;
        }

        /// <summary>
        /// Initiate tests regarding Code Quality on all found pages
        /// </summary>
        private void TestCodeQuality()
        {
            var tableLayOutList = new List<string>();
            var noSemanticList = new List<string>();
            decimal rating = 10.0m;

            foreach (string url in this.sitemap)
            {
                var ths = new ThreadStart(() => W3CValidate(url));
                var th = new Thread(ths);
                threadList.Add(th);
                th.Start();

                var Webget = new HtmlWeb();
                var doc = Webget.Load(url);

                if (IsTableLayout(doc))
                    tableLayOutList.Add(url);

                if (!IsUsingSemantics(doc))
                    noSemanticList.Add(url);
            }

            // Show results from IsTableLayout()
            if (tableLayOutList.Count == 0)
            {
                w3ErrorsFound.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span> Er wordt op alle pagina's waarschijnlijk geen tabel gebruikt voor lay-out.</span></div>";
            }
            else
            {
                rating = rating - 2;
                var unorderedlist = "<ul>";
                foreach (var url in tableLayOutList)
                    unorderedlist += "<li><a href='" + url + "' target='_blank'>" + url + "</a></li>";
                unorderedlist += "</ul>";

                w3ErrorsFound.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> De volgende pagina's gebruiken misschien een tabel voor lay-out. Dit wordt over het algemeen beschouwd als bad practice.</span>" + unorderedlist + "</div>";
            }

            // Show results from IsUsingSemantics()
            if (noSemanticList.Count == 0)
            {
                w3ErrorsFound.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span> Er wordt op alle pagina's gebruik gemaakt van semantic HTML elements</span></div>";
            }
            else
            {
                rating = rating - 2;
                var unorderedlist = "<ul>";

                foreach (var item in noSemanticList)
                {
                    unorderedlist += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";
                }

                unorderedlist += "</ul>";

                w3ErrorsFound.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> De volgende pagina's gebruiken geen semantic HTML elements</span> " + unorderedlist + "</div>";
            }

            // Join Threads
            foreach (var thread in threadList)
                thread.Join();

            if (reqSuccess)
            {
                // Show results from W3CValidate()
                if (notW3cCompliant.Count == 0)
                {
                    w3ErrorsFound.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                        + "<span> Alle geteste pagina's zijn W3C compliant</span></div>";
                }
                else
                {
                    var unorderedlist = "<ul>";
                    foreach (var url in notW3cCompliant)
                        unorderedlist += "<li><a href='" + url + "' target='_blank'>" + url + "</a></li>";
                    unorderedlist += "</ul>";

                    w3ErrorsFound.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                        + "<span> De volgende pagina's zijn niet W3C compliant.</span>" + unorderedlist + "</div>";
                }
            }

            if (errorCnt > 0 || warningCnt > 0)
            {
                // Maximum reduction of 6 because of previous tests. Margin dependent of amount of sites found
                var margin = 6m / (decimal)sitemap.Count;
                rating = rating - ((decimal)notW3cCompliant.Count * margin);
                if (rating < 0)
                    rating = 0.0m;

                // Show table when W3C notifications are encountered 
                W3ResultsTableHidden.Attributes.Remove("class");
                var errorString = "";
                var warningString = "";

                // Uphold grammar
                if (errorCnt == 1)
                    errorString = errorCnt + " error";
                else
                    errorString = errorCnt + " errors";
                if (warningCnt == 1)
                    warningString = warningCnt + " waarschuwing";
                else
                    warningString = warningCnt + " waarschuwingen";

                w3ErrorsFound.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> " + errorString + " en " + warningString + " gevonden.</span></div>";
            }

            decimal rounded = decimal.Round(rating, 1);
            CodeQualityRating.InnerHtml = rounded.ToString();

            var temp = (decimal)Session["RatingAccess"];
            Session["RatingAccess"] = temp + rounded;

            temp = (decimal)Session["RatingTech"];
            Session["RatingTech"] = temp + rounded;
            Session["CodeQualityRating"] = rounded;
            SetRatingDisplay(rating);
        }

        /// <summary>
        /// Get W3C validation results of a single page
        /// </summary>
        /// <param name="url">To be tested URL</param>
        private void W3CValidate(string url)
        {
            Debug.WriteLine("Performing code quality check on: " + url);

            var encoded = WebUtility.UrlEncode(url);
            var currentCnt = 0;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://validator.w3.org/check?uri=" + encoded + "&output=json");
                request.UserAgent = Session["userAgent"].ToString();
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Timeout = 30000;
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                var reader = new StreamReader(dataStream);
                // Read the content. 
                string responseFromServer = reader.ReadToEnd();

                try
                {
                    JObject w3Validate = JObject.Parse(responseFromServer);
                    IList<JToken> messages = w3Validate["messages"].Children().ToList();

                    foreach (JToken item in messages)
                    {
                        if (item["type"].ToString() == "error")
                        {
                            errorCnt++;
                            currentCnt++;
                            if (!notW3cCompliant.Contains(url))
                                notW3cCompliant.Add(url);
                            if (currentCnt <= 5)
                            {

                                try
                                {
                                    // add error message to table
                                    if (item["lastLine"] != null)
                                    {
                                        AddToTable(url, item["type"].ToString(), item["lastLine"].ToString(), item["lastColumn"].ToString(), item["message"].ToString());
                                    }
                                    else
                                    {
                                        AddToTable(url, item["type"].ToString(), "", "", item["message"].ToString());
                                    }
                                }
                                catch (NullReferenceException nre)
                                {
                                    Debug.WriteLine("W3CValidate nullreference exception");
                                    Debug.WriteLine(nre.Message);
                                }
                            }
                        }
                        else if (item["type"].ToString() == "info")
                        {
                            if (item["subType"] != null)
                            {
                                if (item["subType"].ToString() == "warning")
                                {
                                    warningCnt++;
                                    currentCnt++;
                                    if (!notW3cCompliant.Contains(url))
                                        notW3cCompliant.Add(url);
                                    if (currentCnt <= 5)
                                    {
                                        try
                                        {
                                            if (item["subType"] == null)
                                            {
                                                Debug.WriteLine("item[subType] is null");
                                            }
                                            if (item["message"] == null)
                                            {
                                                Debug.WriteLine("item[message] is null");
                                            }
                                            // add warning message to table
                                            AddToTable(url, item["subType"].ToString(), "", "", item["message"].ToString());
                                        }
                                        catch (NullReferenceException nrex)
                                        {
                                            Debug.WriteLine("Could not add to table due to: " + nrex.Message);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (currentCnt > 10)
                        AddToTable(url, "...", "...", "...", "<a href='https://validator.w3.org/nu/?doc=" + url + "' target='_blank'>Volledig verslag met " + currentCnt + " errors en/of waarschuwingen</a>");

                    // Cleanup the streams and the response.
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            catch (WebException e)
            {
                reqSuccess = false;
                w3ErrorsFound.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span>W3C Validatie niet uit kunnen voeren voor " + url + ": " + e.Message + "</span></div>";
                Debug.WriteLine(e.Message);
            }
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
            var tRow = new TableRow();

            var tCellUrl = new TableCell();
            tCellUrl.Text = "<a href='" + url + "' target='_blank'>" + url + "</a>";
            tRow.Cells.Add(tCellUrl);

            var tCellType = new TableCell();
            tCellType.Text = type;
            tRow.Cells.Add(tCellType);

            var tCellLine = new TableCell();
            tCellLine.Text = line;
            tRow.Cells.Add(tCellLine);

            var tCellClmn = new TableCell();
            tCellClmn.Text = column;
            tRow.Cells.Add(tCellClmn);

            var tCellMsg = new TableCell();
            tCellMsg.Text = msg;
            tRow.Cells.Add(tCellMsg);

            table.Rows.Add(tRow);
        }

        /// <summary>
        /// Test if page is using table for lay-out
        /// </summary>
        /// <param name="doc">HtmlDocument</param>
        /// <returns>false if no table is being used for lay-out</returns>
        private bool IsTableLayout(HtmlDocument doc)
        {
            // Check every url in sitemap for analytics software for the current analytictype
            if (doc.DocumentNode.SelectNodes("//table[parent::body][not(ancestor::table)]") != null)
            {
                if (doc.DocumentNode.SelectNodes("//table[parent::body][not(ancestor::table)]").Count == 1)
                    return true; // Only one table found at upper level. May be used for layout
                else
                    return false; // Not null so tables are used at upper level, but multiple tables are used separately so it's probably proper use
            }
            else
            {
                return false; // No table found in upper level
            }
        }

        /// <summary>
        /// Test if page is using semantic HTML elements
        /// </summary>
        /// <param name="doc">HtmlDocument</param>
        /// <returns>false if not using semantic HTML elements</returns>
        private bool IsUsingSemantics(HtmlDocument doc)
        {
            var semantics = new List<string>()
            {
                "</article>", "</aside>", "</details>", "</figcaption>", "</figure>", "</footer>", "</form>", "</header>", "</img>", "</main>", "</mark>", "</nav>", "</section>", "</summary>", "</table>", "</time>"
            };

            if (doc.DocumentNode.SelectSingleNode("//body") != null)
            {
                foreach (var item in semantics)
                {
                    if (doc.DocumentNode.SelectSingleNode("//body").InnerHtml.Contains(item))
                        return true;
                }
            }
            return false;
        }

        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 6m)
                CodeQualityRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                CodeQualityRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                CodeQualityRating.Attributes.Add("class", "excellentScore ratingCircle");
        }
    }
}
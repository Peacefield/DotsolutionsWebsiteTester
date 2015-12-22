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
            // Het gebruik van een tabel voor lay-out zorgt voor aftrek van 1/{aantal geteste pagina's} van 5.
            // 1 pagina met 5 geteste pagina's levert dus 1/5*5 op, oftewel 1 punt aftrek.

            // Het niet gebruiken van semantische elementen hanteert dezelfde beoordeling als hierboven.

            // Het niet kunnen W3C valideren van een pagina levert maximaal een aftrek op van 8 punten.
            // Per pagina is er een aftrek mogelijk van 1/{aantal pagina's} van 8.
            // 1 pagina met 5 geteste pagina's levert dus 1/5*8 op, oftewel 1.6 punt aftrek.

            var tableLayOutList = new List<string>();
            var noSemanticList = new List<string>();
            decimal rating = 10.0m;
            var message = "";
            var isDetailed = (bool)Session["IsDetailedTest"];

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
                message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-ok glyphicons-lg messageIcon'></i>"
                        + "<span class='messageText'> Er wordt op alle geteste pagina's waarschijnlijk geen tabel gebruikt voor lay-out. Dit is uitstekend doordat de lay-out geregeld moet door middel van div-elementen met styling via de CSS.</span></div>";
            }
            else
            {
                rating = rating - ((1m/(decimal)sitemap.Count) * 5);
                var unorderedlist = "<ul>";
                foreach (var url in tableLayOutList)
                    unorderedlist += "<li><a href='" + url + "' target='_blank'>" + url + "</a></li>";
                unorderedlist += "</ul>";

                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                        + "<span class='messageText'> De volgende pagina's gebruiken misschien een tabel voor lay-out. Dit is slecht, de lay-out moet worden geregeld door middel van div-elementen met styling via de CSS." + unorderedlist + "</span></div>";
            }

            // Show results from IsUsingSemantics()
            if (noSemanticList.Count == 0)
            {
                message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er wordt op alle pagina's gebruik gemaakt van semantische HTML5 elementen. Dit is uitstekend aangezien zoekmachines en browsers weten wat ze kunnen verwachten binnen deze elementen.</span></div>";

            }
            else
            {
                rating = rating - ((1m / (decimal)sitemap.Count) * 5);
                var unorderedlist = "<ul>";

                foreach (var item in noSemanticList)
                {
                    unorderedlist += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";
                }

                unorderedlist += "</ul>";

                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                        + "<span class='messageText'> De volgende pagina's gebruiken geen semantische HTML5 elementen. Dit is slecht doordat zoekmachines en browsers niet kunnen voorspellen wat er op een pagina kan staan." + unorderedlist + "</span></div>";

            }

            // Join Threads
            foreach (var thread in threadList)
                thread.Join();

            if (reqSuccess)
            {
                // Show results from W3CValidate()
                if (notW3cCompliant.Count == 0)
                {
                    message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                            + "<i class='glyphicon glyphicon-ok glyphicons-lg messageIcon'></i>"
                            + "<span class='messageText'> Alle geteste pagina's zijn W3C gevalideerd. Dit is uitstekend en betekend dat de website kan worden gelezen door het maximaal mogelijk aantal applicaties.</span></div>";

                }
                else
                {
                    var unorderedlist = "<ul>";
                    foreach (var url in notW3cCompliant)
                        unorderedlist += "<li><a href='https://validator.w3.org/nu/?doc=" + url + "' target='_blank'>" + url + "</a></li>";
                    unorderedlist += "</ul>";

                    message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> De volgende pagina's zijn niet W3C gevalideerd. Hierdoor is het mogelijk dat de website niet gelezen kan worden door sommige browsers of niet goed wordt weergegeven. "
                    + "Om dit op te lossen dienen de gevonden errors worden opgelost." + unorderedlist + "</span></div>";
                }
            }

            if (errorCnt > 0 || warningCnt > 0)
            {
                // Maximum reduction of 8 because of importance. Margin dependent of amount of sites found
                var margin = 8m / (decimal)sitemap.Count;
                rating = rating - ((decimal)notW3cCompliant.Count * margin);
                if (rating < 0)
                    rating = 0.0m;

                // Show table when W3C notifications are encountered 
                var errorString = "";
                var warningString = "";

                // Uphold grammar
                if (errorCnt == 1)
                    errorString = errorCnt.ToString("#,##0") + " error";
                else
                    errorString = errorCnt.ToString("#,##0") + " errors";
                if (warningCnt == 1)
                    warningString = warningCnt.ToString("#,##0") + " waarschuwing";
                else
                    warningString = warningCnt.ToString("#,##0") + " waarschuwingen";

                if (isDetailed)
                {
                    W3ResultsTableHidden.Attributes.Remove("class");

                    message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> " + errorString + " en " + warningString + " gevonden.</span></div>";
                }
                if (!isDetailed)
                    w3Table.Rows.Clear();
            }

            var usingSemanticsIcon = "";
            var usingSemantics = "";
            var w3cValidatedIcon = "";
            var w3cValidated = "";


            if (noSemanticList.Count == 0)
            {
                usingSemanticsIcon = "<i class='fa fa-html5 fa-3x'></i>";

                usingSemantics = "HTML5";
            }
            else
            {
                usingSemanticsIcon = "<span class='fa-stack fa-2x'>"
                    + "<i class='fa fa-ban fa-stack-2x'></i>"
                    + "<i class='fa fa-html5 fa-stack-1x'></i></span>";

                usingSemantics = "Geen gebruik van HTML5";
            }

            if (notW3cCompliant.Count == 0)
            {
                w3cValidatedIcon = "fa-check-circle";
                w3cValidated = "W3C gevalideerd";
            }
            else
            {
                w3cValidatedIcon = "fa-times-circle";
                w3cValidated = "Niet W3C gevalideerd";
            }

            message = "<div class='well well-lg resultWell text-center'>"
                + usingSemanticsIcon
                + "<br/>"
                + "<span>" + usingSemantics + "</span></div>"

                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg resultWell text-center'>"
                + "<i class='fa " + w3cValidatedIcon + " fa-3x'></i><br/>"
                + "<span>" + w3cValidated + "</span></div>"
                + message;


            w3ErrorsFound.InnerHtml = message;

            if (rating == 10.0m)
                rating = 10m;

            if (rating < 0m)
                rating = 0.0m;

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
                                    // Warnings aren't counted as non-compliant
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

                    if (currentCnt > 5)
                        AddToTable(url, "...", "...", "...", "<a href='https://validator.w3.org/nu/?doc=" + url + "' target='_blank'>Volledig verslag voor " + url + "</a>");

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
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'>W3C Validatie niet uit kunnen voeren voor <a href='https://validator.w3.org/nu/?doc=" + url + "' target='_blank'>" + url + "</a>: " + e.Message + "</span></div>";
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

            w3Table.Rows.Add(tRow);
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

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
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
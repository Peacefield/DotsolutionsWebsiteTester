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
        private List<string> notPrintableCss = new List<string>();
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

            ShowPrintability();

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


            if (doc.DocumentNode.SelectNodes("//link[@media]") != null)
            {
                foreach (var node in doc.DocumentNode.SelectNodes("//link[@media]"))
                {
                    if (node.Attributes["media"].Value == "print")
                    {
                        found = true;
                        printable.Add(url);
                    }
                }
            }

            if (doc.DocumentNode.SelectNodes("//link[@rel]") != null && !found)
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
                notPrintableCss.Add("-");
            }
            else
            {
                if (cssList.Count > 0)
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
        }

        /// <summary>
        /// Check if found CSS is compatible for printing
        /// </summary>
        /// <param name="url">URL where CSS was found</param>
        /// <param name="cssUrl">URL of found CSS</param>
        private void TestCss(string url, string cssUrl)
        {
            Debug.WriteLine("Getting content of: " + cssUrl + " from: " + url);
            //string encoded = WebUtility.UrlEncode(cssUrl);
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cssUrl);
                request.UserAgent = Session["userAgent"].ToString();
                request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");
                request.Credentials = CredentialCache.DefaultCredentials;
                

                //// Create new stopwatch.
                //Stopwatch stopwatch = new Stopwatch();
                //// Begin timing.
                //stopwatch.Start();
                

                // Get the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                var reader = new StreamReader(dataStream);
                // Read the content. 
                string responseFromServer = reader.ReadToEnd();


                //// Stop timing.
                //stopwatch.Stop();
                //// Write result.
                //Debug.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);



                if (!responseFromServer.Contains("@media print"))
                {
                    if (!printable.Contains(url) && !notPrintable.Contains(url))
                    {
                        notPrintable.Add(url);
                        notPrintableCss.Add(cssUrl);
                    }
                }
                else
                {
                    if (!printable.Contains(url))
                        printable.Add(url);

                    if (notPrintable.Contains(url))
                    {
                        var index = notPrintable.IndexOf(url);
                        notPrintableCss.RemoveAt(index);

                        notPrintable.Remove(url);
                    }
                }
            }
            catch (WebException)
            {
                Debug.WriteLine("Could not fetch CSS");
            }
        }

        /// <summary>
        /// Add to table
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="url">URL of origin</param>
        /// <param name="cssUrl">URL of found CSS</param>
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
        }

        /// <summary>
        /// Show resultmessages
        /// </summary>
        private void ShowPrintability()
        {
            // De beoordeling voor dit onderdeel wordt berekend door per pagina die niet printbaar is
            // een aftrek te hanteren van 1/{aantal pagina's} * 10.
            // 1 niet-printbare pagina van 5 geteste pagina's resulteert in een aftrek van 1/5*10 = 2 punten.

            var rating = 10.0m;
            var message = "";
            var isDetailed = (bool)Session["IsDetailedTest"];

            // Er zijn printbare pagina's
            if (printable.Count >= sitemap.Count)
            {
                var percentage = ((decimal)printable.Count / (decimal)sitemap.Count) * 100m;
                if (percentage > 100m)
                    percentage = 100m;
                message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12 text-center' role='alert'>"
                    + "<i class='fa fa-print fa-3x'></i><br/>"
                    + "<span class='messageText'>" + percentage.ToString("#,0") + "% van de geteste pagina's is printbaar.</span></div>";

            }
            // Less pages printable than there were pages tested
            else
            {
                if (isDetailed)
                    PrintabilityTableHidden.Attributes.Remove("class");

                if (!isDetailed)
                    PrintabilityTable.Rows.Clear();

                var notprintable = sitemap.Count - printable.Count;

                var percentage = ((decimal)notprintable / (decimal)sitemap.Count) * 100m;
                
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12 text-center' role='alert'>"
                    + "<span class='fa-stack fa-2x'>"
                    + "<i class='fa fa-ban fa-stack-2x'></i>"
                    + "<i class='fa fa-print fa-stack-1x'></i></span>"
                    + "<br/>"
                    + "<span class='messageText'>" + percentage.ToString("#,0") + "% van de geteste pagina's is niet printbaar.</span></div>";
            }

            // Add every page that's not printable
            if (notPrintable.Count > 0)
            {
                foreach (var url in notPrintable)
                {
                    if (!printable.Contains(url))
                    {
                        rating = rating - ((1m / (decimal)sitemap.Count) * 10m);
                        var index = notPrintable.IndexOf(url);
                        var cssUrl = notPrintableCss[index].ToString();
                        if (cssUrl != "-")
                            AddToTable("Er is geen rekening gehouden met de printbaarheid in de aangetroffen CSS.", url, cssUrl);
                    }
                }
            }

            PrintResults.InnerHtml = message;

            if (rating == 10.0m)
                rating = 10m;
            var rounded = decimal.Round(rating, 1);
            Session["PrintabilityRating"] = rounded;
            PrintabilityRating.InnerHtml = rounded.ToString();

            var temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = temp + rounded;

            temp = (decimal)Session["RatingTech"];
            Session["RatingTech"] = temp + rounded;
            SetRatingDisplay(rating);
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 6m)
                PrintabilityRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                PrintabilityRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                PrintabilityRating.Attributes.Add("class", "excellentScore ratingCircle");
        }
    }
}
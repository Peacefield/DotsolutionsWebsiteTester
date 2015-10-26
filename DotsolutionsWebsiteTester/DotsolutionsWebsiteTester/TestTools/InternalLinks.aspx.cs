using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class InternalLinks : System.Web.UI.Page
    {
        private int errorCnt = 0;

        private int lengthCnt = 0;
        private int imageCnt = 0;
        private int brokenCnt = 0;
        private List<string> robots;
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

            this.robots = (List<string>)Session["robotsTxt"];
            var ths = new ThreadStart(TestInternalLinks);
            var th = new Thread(ths);
            th.Start();

            th.Join();

            var sb = new System.Text.StringBuilder();
            InternalLinksSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["InternalLinks"] = htmlstring;
        }

        /// <summary>
        /// Check per page the links that are declared
        /// Checks for length and initiates a check to see if the link is working
        /// </summary>
        private void TestInternalLinks()
        {
            var sitemap = (List<string>)Session["selectedSites"];
            var threadList = new List<Thread>();
            foreach (var url in sitemap)
            {
                Debug.WriteLine("Link check op -> " + url);

                // Reset counters per page
                lengthCnt = 0;
                imageCnt = 0;
                brokenCnt = 0;

                var Webget = new HtmlWeb();
                var doc = Webget.Load(url);

                // Test every internal link on current page from sitmap
                if (doc.DocumentNode.SelectNodes("//a[@href]") != null)
                {
                    foreach (var link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        var ths = new ThreadStart(() => TestLink(link, url));
                        var th = new Thread(ths);
                        threadList.Add(th);
                        th.Start();
                    }
                }
                // Join Threads that were executing TestLink
                foreach (var thread in threadList)
                {
                    thread.Join();
                }

                if (lengthCnt >= 5)
                {
                    AddToTable("<strong>" + (lengthCnt - 4) + " overige links gevonden met een te lange beschrijvende tekst</strong>", "...", url);
                }
                if (imageCnt >= 5)
                {
                    AddToTable("<strong>" + (imageCnt - 4) + " overige afbeeldinglinks gevonden zonder title/alt attribuut</strong>", "...", url);
                }
                if (brokenCnt >= 5)
                {
                    AddToTable("<strong>" + (brokenCnt - 4) + " overige links gevonden die niet werken</strong>", "...", url);
                }
            }

            // Show message with findings
            if (errorCnt > 0)
            {
                // Show table when errors are found 
                IntLinksHiddenTable.Attributes.Remove("class");
                internalLinksErrorsFound.InnerHtml = "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg'></i>"
                    + "<span> " + errorCnt + " meldingen gevonden.</span></div>";
            }
            else
            {
                internalLinksErrorsFound.InnerHtml = "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span> " + errorCnt + " meldingen gevonden.</span></div>";
            }

            var rating = 10m - ((decimal)errorCnt / 5m);

            if (rating < 0)
                rating = 0.0m;

            decimal rounded = decimal.Round(rating, 1);
            InternalLinksRating.InnerHtml = rounded.ToString();

            var temp = (decimal)Session["RatingAccess"];
            Session["RatingAccess"] = temp + rounded;

            temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = temp + rounded;

            temp = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = temp + rounded;

            temp = (decimal)Session["RatingTech"];
            Session["RatingTech"] = temp + rounded;

            Session["CodeQualityRating"] = rounded;
            SetRatingDisplay(rating);
        }

        /// <summary>
        /// Test individual link
        /// </summary>
        /// <param name="link"></param>
        /// <param name="url"></param>
        private void TestLink(HtmlNode link, string url)
        {
            string mainUrl = Session["MainUrl"].ToString();
            string internalLink = link.Attributes["href"].Value;

            // Making sure we only test urls, instead of also including mailto: tel: javascript: intent: etc.
            if (!internalLink.Contains("/") || internalLink.Contains("intent://"))
                return;

            // Check that there is a description
            if (link.InnerText != "")
            {
                // Check if the description is not too long
                string[] words = link.InnerText.Split(new char[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (words.Length > 25)
                {
                    if (lengthCnt < 5)
                    {
                        AddToTable(internalLink, "Beschrijvende tekst is te lang (" + words.Length + " woorden)", url);
                    }

                    lengthCnt++;
                    errorCnt++;
                }
            }
            else
            {
                // If the link is an image it has to containt a alt attribute
                if (link.InnerHtml.Contains("<img") || link.InnerHtml.Contains("figure"))
                {
                    if (link.InnerHtml.Contains("alt") == false)
                    {
                        if (imageCnt < 5)
                        {
                            AddToTable(internalLink, "Afbeelding bevat geen 'alt' attribuut", url);
                        }
                        imageCnt++;
                        errorCnt++;
                    }
                }

                // If the link is an icon it has to contain a title attribute
                if ((link.InnerHtml.Contains("</i>") || link.InnerHtml.Contains("</span>")))
                {
                    if (link.InnerHtml.Contains("title") == false)
                    {
                        if (imageCnt < 5)
                        {
                            AddToTable(internalLink, "i element bevat geen 'title' attribuut", url);
                        }
                        imageCnt++;
                        errorCnt++;
                    }
                }
            }

            // Test if the link does not return an errorcode
            var testLink = "";
            if (internalLink.Contains("http://") || internalLink.Contains("https://"))
            {
                testLink = internalLink;
            }
            else if (internalLink.Contains("//www."))
            {
                testLink = "http:" + internalLink;
            }
            else if (internalLink.Contains("www."))
            {
                testLink = "http://" + internalLink;
            }
            else
            {
                if (mainUrl.EndsWith("/"))
                    testLink = mainUrl.Remove(mainUrl.Length - 1) + internalLink;
                else
                    testLink = mainUrl + internalLink;
            }

            if (link.Attributes["rel"] != null)
            {
                if (link.Attributes["rel"].Value.ToLower() == "nofollow")
                {
                    return;
                }
            }

            foreach (var item in robots)
            {
                if (testLink.Contains(item))
                {
                    Debug.WriteLine("Link zit in disallow van robots.txt");
                    return;
                }
                else if (item.Contains("*"))
                {
                    var split = item.Split('*');
                    foreach (var part in split)
                    {
                        if (part != "")
                        {
                            if (testLink.Contains(item))
                            {
                                Debug.WriteLine("Link zit in disallow van robots.txt");
                                return;
                            }
                        }
                    }
                }
            }

            int httpcode = LinkWorks(mainUrl, testLink);
            if (httpcode != 200)
            {
                if (brokenCnt < 5)
                {
                    string tablelink = "<a href='" + testLink + "' target='_blank'>" + testLink + "</a>";

                    // add message to table
                    if (httpcode > 0)
                        AddToTable(tablelink, "Link werkt niet (HTTP Status Code: " + httpcode + ")", url);
                    else if (httpcode == -1)
                        AddToTable(tablelink, "Link werkt niet (Timeout)", url);
                    else if (httpcode == 0)
                        AddToTable(tablelink, "Link werkt niet", url);
                }

                brokenCnt++;
                errorCnt++;
            }

        }

        /// <summary>
        /// Test if a link is broken, a.k.a. returns anything other than statuscode 200 OK or Redirect
        /// </summary>
        /// <param name="url">URL to be tested</param>
        /// <returns></returns>
        private int LinkWorks(string mainurl, string link)
        {
            try
            {
                if (link == "")
                    return 0;

                string encoded = WebUtility.UrlEncode(link);

                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(link) as HttpWebRequest;
                request.UserAgent = Session["userAgent"].ToString();
                request.Timeout = 10000; // Set timout of 10 seconds so to not waste time
                request.Method = "GET";
                //request.Credentials = CredentialCache.DefaultCredentials;
                request.UnsafeAuthenticatedConnectionSharing = false;
                request.UseDefaultCredentials = true;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                int httpcode = (int)response.StatusCode;
                response.Dispose();
                return httpcode;
            }
            catch (WebException we)
            {
                //Any webexception will return true unless it's a 404.
                Debug.WriteLine("WebException " + we.Message + " <><><><><> met link: " + link + " en Status: " + we.Status);

                HttpWebResponse response = we.Response as HttpWebResponse;
                if (response != null)
                {
                    int httpcode = (int)response.StatusCode;
                    Debug.WriteLine("HTTP Status Code: " + httpcode);
                    return httpcode;
                }
                else if (we.Status == WebExceptionStatus.Timeout)
                {
                    // no http status code available
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                //Any exception will return 0
                //Debug.WriteLine("Algemene fout " + e.Message);
                return 0;
            }
        }

        /// <summary>
        /// Adds result to table IntLinksTable
        /// </summary>
        /// <param name="link">Tested link</param>
        /// <param name="text">Description of the error</param>
        /// <param name="page">Page of origin</param>
        private void AddToTable(string link, string text, string page)
        {
            var tRow = new TableRow();

            var tCellLink = new TableCell();
            tCellLink.Text = link;
            tRow.Cells.Add(tCellLink);

            var tCellMsg = new TableCell();
            tCellMsg.Text = text;
            tRow.Cells.Add(tCellMsg);

            var tCellPage = new TableCell();
            tCellPage.Text = "<a href='" + page + "' target='_blank'>" + page + "</a>";
            tRow.Cells.Add(tCellPage);

            IntLinksTable.Rows.Add(tRow);
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 6m)
                InternalLinksRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                InternalLinksRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                InternalLinksRating.Attributes.Add("class", "excellentScore ratingCircle");
        }
    }
}
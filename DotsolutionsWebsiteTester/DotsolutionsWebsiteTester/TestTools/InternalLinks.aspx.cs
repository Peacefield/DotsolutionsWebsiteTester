﻿using HtmlAgilityPack;
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
        private int threadCnt = 0;

        private List<string> robots;
        private bool unavailable = false;

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
            // Get Robots.txt as list containing only pages affecting this bot
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
            // De beoordeling is per pagina afhankelijk van het aantal gevonden links tegenover het aantal foutieve links
            // De formule hiervoor is: {aantal foutieve links}/{totaal aantal links} * 10.
            // De uitkomst wordt per pagina afgetrokken van het totaal.

            var sitemap = (List<string>)Session["selectedSites"];
            var message = "";
            var isDetailed = (bool)Session["IsDetailedTest"];
            var totalLinkCnt = 0;
            foreach (var url in sitemap)
            {
                Debug.WriteLine("Link check op -> " + url);
                this.threadCnt = 0;

                var threadList = new List<Thread>();
                var Webget = new HtmlWeb();
                var doc = Webget.Load(url);

                // Test every internal link on current page from sitmap
                if (doc.DocumentNode.SelectNodes("//a[@href]") != null)
                {
                    totalLinkCnt += doc.DocumentNode.SelectNodes("//a[@href]").Count;
                    Debug.WriteLine("totalLinkCnt: " + totalLinkCnt);
                    foreach (var link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        var ths = new ThreadStart(() => TestLink(link, url));
                        var th = new Thread(ths);
                        threadList.Add(th);
                    }
                }

                // Start Threads some time apart to prevent spamming/getting blacklisted
                foreach (var thread in threadList)
                {
                    thread.Start();
                    Thread.Sleep(10);
                }

                // Join Threads that were executing TestLink
                foreach (var thread in threadList)
                {
                    thread.Join();
                }
            }

            // Show message with findings
            if (errorCnt > 0)
            {
                // Show table when errors are found and is a detailed test
                if (isDetailed)
                    IntLinksHiddenTable.Attributes.Remove("class");

                Debug.WriteLine("IntLinksTable.Rows.Count = " + IntLinksTable.Rows.Count);

                if (!isDetailed)
                {
                    try
                    {
                        IntLinksTable.Rows.Clear();
                    }
                    catch (NullReferenceException)
                    {

                    }
                }

                var errorCntString = "zijn " + errorCnt.ToString("#,##0") + " meldingen";
                if (errorCnt == 1)
                {
                    errorCntString = "is 1 melding";
                }

                message = "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12 text-center' role='alert'>"
                    + "<i class='fa fa-globe fa-3x'></i><br/>"
                    + "<span>Er " + errorCntString + " gevonden van linken die te algemeen beschreven zijn, te lang zijn en/of niet werken.</span></div>";
            }
            else
            {
                message = "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12 text-center' role='alert'>"
                    + "<i class='fa fa-globe fa-3x'></i><br/>"
                    + "<span class='messageText'> Alle gevonden linken hebben een goede beschrijving, zijn niet te lang en zijn werkend.</span></div>";
            }

            internalLinksErrorsFound.InnerHtml = message;

            //var rating = 10.0m - ((decimal)errorCnt / 5m);
            var rating = 10m;
            if (totalLinkCnt > 0)
                rating = 10.0m - (((decimal)errorCnt / (decimal)totalLinkCnt) * 10m);

            if (rating < 0)
                rating = 0.0m;
            if (rating == 10.0m)
                rating = 10m;

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

            Session["InternalLinksRating"] = rounded;
            SetRatingDisplay(rating);
        }

        /// <summary>
        /// Test individual link
        /// </summary>
        /// <param name="link">Found HtmlNode containing href</param>
        /// <param name="url">Page of origin</param>
        private void TestLink(HtmlNode link, string url)
        {
            string mainUrl = Session["MainUrl"].ToString();
            string internalLink = link.Attributes["href"].Value;
            string[] badDesc = { "link", "klik", "hier", "klik hier", "lees meer", "lees verder" };
            // Making sure we only test urls, instead of also including mailto: tel: javascript: intent: etc.
            if (!internalLink.Contains("/") || internalLink.Contains("intent://"))
                return;

            // Check that there is a description
            if (link.InnerText != "")
            {
                // Check if the description is not too long
                //string[] words = link.InnerText.Split(new char[] { ' ', ',', '.', '&', ':', '©', '\'', '+' }, StringSplitOptions.RemoveEmptyEntries);
                //string[] splitLink = link.InnerText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                string[] splitLink = link.InnerText.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                var words = new List<string>();

                foreach (var item in splitLink)
                {
                    if (item is string && item.Length > 0)
                    {
                        words.Add(item);
                    }
                }

                if (words.Count > 25)
                {
                    var placeholder = "";
                    foreach (var item in words)
                    {
                        placeholder += item;
                    }

                    Debug.WriteLine("Zin > 25 woorden = " + placeholder);

                    AddToTable(internalLink, "Beschrijvende tekst is te lang (" + words.Count + " woorden)", url);

                    lengthCnt++;
                    errorCnt++;
                }

                foreach (var item in badDesc)
                {
                    if (link.InnerText.ToLower() == item)
                    {
                        AddToTable(internalLink, "Slechte beschrijving (" + item + ")", url);
                        break;
                    }
                }
            }
            else
            {
                // If the link is an image it has to contain a alt attribute
                if (link.InnerHtml.Contains("<img") || link.InnerHtml.Contains("figure"))
                {
                    if (!link.InnerHtml.Contains("alt="))
                    {
                        AddToTable(internalLink, "Afbeelding bevat geen 'alt' attribuut", url);
                        imageCnt++;
                        errorCnt++;
                    }
                }

                // If the link is an icon it still has to contain a title attribute
                if ((link.InnerHtml.Contains("</i>") || link.InnerHtml.Contains("</span>")))
                {
                    if (link.InnerHtml.Contains("title") == false)
                    {
                        AddToTable(internalLink, "i of span element bevat geen 'title' attribuut", url);
                        imageCnt++;
                        errorCnt++;
                    }
                }
            }
            #region Broken links
            // Limit amount of links per page checked to prevent spamming/getting blacklisted when checking for broken links
            if (threadCnt < 200)
            {
                threadCnt++;

                // Test if the link does not return an errorcode
                var testLink = "";
                if (internalLink.Contains("http:/") || internalLink.Contains("https:/"))
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
                    if (mainUrl.EndsWith("/") && internalLink.StartsWith("/"))
                        testLink = mainUrl.Remove(mainUrl.Length - 1) + internalLink;
                    else if ((mainUrl.EndsWith("/") && !internalLink.StartsWith("/")) || (!mainUrl.EndsWith("/") && internalLink.StartsWith("/")))
                        testLink = mainUrl + internalLink;
                    else if (!mainUrl.EndsWith("/") && !internalLink.StartsWith("/"))
                        testLink = mainUrl + "/" + internalLink;
                }

                var nofollow = false;
                if (link.Attributes["rel"] != null)
                {
                    if (link.Attributes["rel"].Value.ToLower() == "nofollow")
                    {
                        nofollow = true;
                    }
                }

                foreach (var item in robots)
                {
                    if (testLink.Contains(item))
                    {
                        Debug.WriteLine(testLink + " zit in disallow van robots.txt: " + item);
                        nofollow = true;
                    }
                    else if (item.Contains("*"))
                    {
                        var split = item.Split('*');
                        var count = 0;
                        foreach (var part in split)
                        {
                            if (part != "")
                            {
                                if (testLink.Contains(part))
                                {
                                    count++;
                                }
                            }
                            else
                                count++;
                        }
                        if (count == split.Length)
                        {
                            Debug.WriteLine(testLink + " zit in disallow van robots.txt: " + item);
                            nofollow = true;
                        }
                    }
                }
                if (!unavailable && !nofollow)
                {
                    int httpcode = LinkWorks(mainUrl, testLink);
                    // 405 means request method: HEAD is not allowed, but the URL probably works if this gets returned
                    if (httpcode != 200 && httpcode != 405)
                    {
                        string tablelink = "<a href='" + testLink + "' target='_blank'>" + testLink + "</a>";

                        // add message to table
                        if (httpcode > 0)
                            AddToTable(tablelink, "Link werkt niet (HTTP Status Code: " + httpcode + ")", url);
                        else if (httpcode == -1)
                            AddToTable(tablelink, "Link werkt niet (Timeout)", url);
                        else if (httpcode == 0)
                            AddToTable(tablelink, "Link werkt niet", url);

                        brokenCnt++;
                        errorCnt++;
                        // Emergency break
                        if (httpcode == 503)
                            unavailable = true;
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Test if a link is broken, a.k.a. returns anything other than statuscode 200 OK or Redirect
        /// </summary>
        /// <param name="url">URL to be tested</param>
        /// <returns>int httpcode</returns>
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
                request.Method = "HEAD";
                //request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");
                //request.Credentials = CredentialCache.DefaultCredentials;
                request.UnsafeAuthenticatedConnectionSharing = true;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                int httpcode = (int)response.StatusCode;
                response.Dispose();
                return httpcode;
            }
            catch (WebException we)
            {
                //Debug.WriteLine("WebException " + we.Message + " \n\tmet link: " + link + " en Status: " + we.Status);

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
            var element = InternalLinksRating;

            if (rating == 10m)
                element.Attributes.Add("class", "score-10 ratingCircle");
            else if (rating >= 9m)
                element.Attributes.Add("class", "score-9 ratingCircle");
            else if (rating >= 8m)
                element.Attributes.Add("class", "score-8 ratingCircle");
            else if (rating >= 7m)
                element.Attributes.Add("class", "score-7 ratingCircle");
            else if (rating >= 6m)
                element.Attributes.Add("class", "score-6 ratingCircle");
            else if (rating >= 5m)
                element.Attributes.Add("class", "score-5 ratingCircle");
            else if (rating >= 4m)
                element.Attributes.Add("class", "score-4 ratingCircle");
            else if (rating >= 3m)
                element.Attributes.Add("class", "score-3 ratingCircle");
            else if (rating >= 2m)
                element.Attributes.Add("class", "score-2 ratingCircle");
            else if (rating >= 1m)
                element.Attributes.Add("class", "score-1 ratingCircle");
            else
                element.Attributes.Add("class", "score-0 ratingCircle");
        }
    }
}
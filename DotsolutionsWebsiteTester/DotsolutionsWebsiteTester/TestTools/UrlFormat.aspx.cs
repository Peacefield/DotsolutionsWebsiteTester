﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class UrlFormat : System.Web.UI.Page
    {
        decimal rating = 10.0m;
        List<string> sitemap;
        List<string> longUrl = new List<string>();
        List<string> dirtyUrl = new List<string>();

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
            var ths = new ThreadStart(GetUrlFormat);
            var th = new Thread(ths);
            th.Start();

            th.Join();

            var sb = new System.Text.StringBuilder();
            UrlFormatSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["UrlFormat"] = htmlstring;
        }

        /// <summary>
        /// Rate the URLs on the pages in Session["selectedSites"] 
        /// </summary>
        private void GetUrlFormat()
        {
            // De beoordeling is afhankelijk van de lengte en opbouw van de URL’s
            // Per pagina wordt de beoordeling berekend in verhouding tot het totaal aantal gebruikte URL's op die pagina
            // De formule hiervoor is {aantal foutieve URL's}/{totaal aantal URL's}*10. 
            // De uitkomst hiervan wordt van de huidige beoordeling afgetrokken.

            var totalCount = 0;
            var message = "";
            var isDetailed = (bool)Session["IsDetailedTest"];
            var totalLinkCnt = 0;
            foreach (var page in sitemap)
            {
                var threadList = new List<Thread>();
                var Webget = new HtmlWeb();
                var doc = Webget.Load(page);
                if (doc.DocumentNode.SelectNodes("//a[@href]") != null)
                {
                    var pageLinkCount = doc.DocumentNode.SelectNodes("//a[@href]").Count;
                    totalLinkCnt += pageLinkCount;
                    Debug.WriteLine("pageLinkCount: " + pageLinkCount);
                    Debug.WriteLine("totalLinkCnt: " + totalLinkCnt);
                    foreach (var link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        var ths = new ThreadStart(() => TestFormat(link.Attributes["href"].Value, page, pageLinkCount));
                        var th = new Thread(ths);
                        threadList.Add(th);
                        th.Start();
                    }
                }
                foreach (var th in threadList)
                {
                    th.Join();
                }

                if (longUrl.Count > 0)
                {

                    var count = 0;
                    foreach (var item in longUrl)
                    {
                        if (count <= 5)
                        {
                            AddToTable(item, "URL is te lang", page);
                        }
                        count++;
                    }
                    if (count > 5)
                    {
                        AddToTable("...", "<strong>" + (count - 4) + " overige te lange URLs gevonden</strong>", page);
                    }
                    totalCount += count;

                    if (isDetailed)
                        UrlFormatHiddenTable.Attributes.Remove("class");
                    if (!isDetailed)
                        UrlFormatTable.Rows.Clear();
                }

                if (dirtyUrl.Count > 0)
                {
                    var count = 0;
                    foreach (var item in dirtyUrl)
                    {
                        if (count <= 5)
                        {
                            AddToTable(item, "URL is niet gebruiksvriendelijk", page);
                        }
                        count++;
                    }
                    if (count > 5)
                    {
                        AddToTable("...", "<strong>" + (count - 4) + " overige niet gebruiksvriendelijke URLs gevonden</strong>", page);
                    }
                    totalCount += count;

                    if (isDetailed)
                        UrlFormatHiddenTable.Attributes.Remove("class");
                    if (!isDetailed)
                        UrlFormatTable.Rows.Clear();
                }

                longUrl.Clear();
                dirtyUrl.Clear();
            }

            var resultMessage = "";
            if (totalCount == 0)
            {
                resultMessage = "<div class='alert alert-success well-lg resultWell text-center'>"
                    + "<i class='fa fa-link fa-3x'></i><br/>"
                    + "<span class='messageText'> Alle URLs zijn schoon en gebruiksvriendelijk.<br/>Dit houdt in dat de gebruikte URL's niet te lang zijn en geen rare tekens bevatten.</span></div>";
            }
            else
            {
                resultMessage = "<div class='alert alert-danger well-lg resultWell text-center'>"
                    + "<i class='fa fa-chain-broken fa-3x'></i><br/>"
                    + "<span class='messageText'> " + totalCount.ToString("#,##0") + " foutieve URLs gevonden.<br/>Dit houdt in dat de gebruikte URL's te lang zijn en/of rare tekens bevatten.</span></div>";
            }

            message = resultMessage
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg resultWell text-center'>"
                + "<span class='largetext'>" + totalLinkCnt.ToString("#,##0") + "</span><br/>"
                + "<span>URL's getest</span></div>";

            UrlFormatNotifications.InnerHtml = message;

            if (rating <= 0m)
                rating = 0.0m;
            if (rating == 10.0m)
                rating = 10m;

            decimal rounded = decimal.Round(rating, 1);

            var temp = (decimal)Session["RatingAccess"];
            Session["RatingAccess"] = temp + rounded;

            temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = temp + rounded;

            temp = (decimal)Session["RatingTech"];
            Session["RatingTech"] = temp + rounded;

            UrlFormatRating.InnerHtml = rounded.ToString();
            Session["UrlFormatRating"] = rounded;

            SetRatingDisplay(rounded);
        }

        /// <summary>
        /// Test a found URL on a page
        /// </summary>
        /// <param name="link">Found URL that gets tested</param>
        /// <param name="page">Page of origin where the URL was found</param>
        /// <param name="linkCount">Amount of links on the page of origin</param>
        private void TestFormat(string link, string page, int linkCount)
        {
            var uri = new Uri(page);
            if (link.Contains("/"))
            {
                var path = link;
                if (link.Contains("http"))
                {
                    if (link.StartsWith(uri.Scheme + "://" + uri.Host + "/"))
                        path = link.Replace(uri.Scheme + "://" + uri.Host + "/", "");
                    else
                        return;
                }

                if (IsLong(path))
                {
                    rating = rating - (1m / (decimal)linkCount * 10m);
                    longUrl.Add(path);
                }

                if (IsDirty(path))
                {
                    rating = rating - (1m / (decimal)linkCount * 10m);
                    dirtyUrl.Add(path);
                }
            }
        }

        /// <summary>
        /// Check if URL is longer than 15 words when split on '-' and '_'
        /// </summary>
        /// <param name="path">URL path</param>
        /// <returns>true if URL is longer than 15 words</returns>
        private bool IsLong(string path)
        {
            var parts = path.Split('/');
            foreach (var part in parts)
            {
                var words = part.Split('-', '_');
                if (words.Length > 15)
                {
                    Debug.WriteLine(" >>>>> " + path + " IsLong()");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if URL is userfriendly by checking for several characters
        /// </summary>
        /// <param name="path">URL path</param>
        /// <returns>true if URL contains any of the 'dirty' characters</returns>
        private bool IsDirty(string path)
        {
            string[] dirtyChars = { "?", "_", "%20", "!", "$", "&", "+", ":", "=", "@" };

            foreach (var item in dirtyChars)
            {
                if (path.Contains(item))
                {
                    Debug.WriteLine(" >>>>> " + path + " contains: " + item);
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
            var element = UrlFormatRating;

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

        /// <summary>
        /// Adds result to table UrlFormatTable
        /// </summary>
        /// <param name="link">Tested link</param>
        /// <param name="text">Description of the notification</param>
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

            UrlFormatTable.Rows.Add(tRow);
        }
    }
}
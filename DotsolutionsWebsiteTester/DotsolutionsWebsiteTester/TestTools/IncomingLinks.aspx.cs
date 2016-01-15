using Scoop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class IncomingLinks : System.Web.UI.Page
    {
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
            GetIncomingLinks();

            var sb = new System.Text.StringBuilder();
            IncomingLinksSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["IncomingLinks"] = htmlstring;
        }

        /// <summary>
        /// Get and show results from Mozscape API
        /// </summary>
        private void GetIncomingLinks()
        {
            // De beoordeling voor dit onderdeel is gelijk aan de score die de website krijgt van Moz, de Mozrank.

            var rating = 10m;
            var sitemap = (List<string>)Session["selectedSites"];
            var message = "";
            // Start setting up MozscapeAPI
            var strAccessID = System.Web.Configuration.WebConfigurationManager.AppSettings["MozscapeAccessId"];
            var strPrivateKey = System.Web.Configuration.WebConfigurationManager.AppSettings["MozscapeSecretKey"];
            var mozAPI = new MozscapeAPI();
            // End setting up MozscapeAPI
            var totalLinks = 0;
            var totalRating = 0.0m;
            var isDetailed = (bool)Session["IsDetailedTest"];

            foreach (var page in sitemap)
            {
                var strAPIURL = mozAPI.CreateAPIURL(strAccessID, strPrivateKey, 1, "url metrics", page, "");
                var strResults = mozAPI.FetchResults(strAPIURL);
                var msURLMetrics = mozAPI.ParseURLMetrics(strResults);
                var strBackLinks = msURLMetrics.uid;
                var strMozRankUrl = msURLMetrics.umrp;
                var strMozRankCrawled = msURLMetrics.ulc;

                var strMozRankCrawledDate = UnixTimeStampToDateTime(strMozRankCrawled);

                totalLinks += Int32.Parse(strBackLinks);
                totalRating += decimal.Parse(strMozRankUrl, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

                if (strMozRankCrawled == "0")
                    strMozRankCrawledDate = "Niet bekend";
                var strMozRankUrlRounded = decimal.Round(decimal.Parse(strMozRankUrl, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture), 1).ToString();
                var intExternalLinks = Int32.Parse(strBackLinks);
                AddToTable(page, intExternalLinks.ToString("#,##0"), strMozRankUrlRounded, strMozRankCrawledDate);
            }

            if (isDetailed)
                IncomingLinksTableHidden.Attributes.Remove("class");

            if (!isDetailed)
                IncomingLinksTable.Rows.Clear();

            totalRating = decimal.Round(totalRating / sitemap.Count, 1);

            message += "<div class='well well-lg resultWell text-center'>"
                + "<span class='largetext'>" + totalLinks.ToString("#,##0") + "</span><br/>"
                + "<span>links gevonden die naar de geteste pagina's verwijzen</span></div>"
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg resultWell text-center'>"
                + "<span class='largetext'>" + totalRating + "/10</span><br/>"
                + "<span>Gemiddelde MozRank score</span></div>";

            message += "<div class='alert alert-info col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg messageIcon'></i>"
                + "<span class='messageText'>De hoeveelheid links die verwijzen naar een pagina worden door zoekmachines gezien als <i>stemmen</i> die verantwoordelijk zijn voor de positie in de zoekresultaten.</span></div>";

            IncomingLinksResults.InnerHtml = message;

            rating = totalRating;
            var rounded = decimal.Round(rating, 1);
            IncomingLinksRating.InnerHtml = rounded.ToString();
            SetRatingDisplay(rounded);
            var temp = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = temp + rounded;

            Session["IncomingLinksRating"] = rounded;
        }

        // https://moz.com/help/guides/moz-api/mozscape/api-reference/url-metrics
        // https://moz.com/help/guides/moz-api/mozscape/getting-started-with-mozscape
        // https://moz.com/help/guides/moz-api/mozscape/getting-started-with-mozscape/anatomy-of-a-mozscape-api-call

        // http://uk.queryclick.com/seo-news/using-mozscape-api-c-net/
        // https://github.com/QueryClick/MozscapeAPI/blob/master/MozscapeAPI.cs#L93
        
        /// <summary>
        /// Add a 0 to the start of an integer if it's less than 10 to improve readability
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private string AddZero(int date)
        {
            string temp = date.ToString();
            if (date < 10)
                temp = "0" + date;
            return temp;
        }

        /// <summary>
        /// Convert Unix Timestamp to DateTime
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        private string UnixTimeStampToDateTime(string unixTimeStamp)
        {
            var unix = Convert.ToDouble(unixTimeStamp);
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unix).ToLocalTime();

            var result = dtDateTime.Year + "/" + AddZero(dtDateTime.Month) + "/" + AddZero(dtDateTime.Day);
            return result;
        }

        /// <summary>
        /// Add the found MozRank results to the table
        /// </summary>
        /// <param name="page">Page of origin</param>
        /// <param name="linkAmount">amount of equity  and non-equity links found</param>
        /// <param name="mozRank">MozRank score</param>
        /// <param name="lastCrawled">Last time Mozscape crawled the URL</param>
        private void AddToTable(string page, string linkAmount, string mozRank, string lastCrawled)
        {
            var tRow = new TableRow();

            var tCellPage = new TableCell();
            tCellPage.Text = "<a href='" + page + "' target='_blank'>" + page + "</a>";
            tRow.Cells.Add(tCellPage);

            var tCellLink = new TableCell();
            tCellLink.Text = linkAmount;
            tRow.Cells.Add(tCellLink);

            var tCellMozRank = new TableCell();
            tCellMozRank.Text = mozRank;
            tRow.Cells.Add(tCellMozRank);

            var tCellLastCrawled = new TableCell();
            tCellLastCrawled.Text = lastCrawled;
            tRow.Cells.Add(tCellLastCrawled);

            IncomingLinksTable.Rows.Add(tRow);
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating == 10m)
                IncomingLinksRating.Attributes.Add("class", "score-10 ratingCircle");
            else if (rating > 9m)
                IncomingLinksRating.Attributes.Add("class", "score-9 ratingCircle");
            else if (rating > 8m)
                IncomingLinksRating.Attributes.Add("class", "score-8 ratingCircle");
            else if (rating > 7m)
                IncomingLinksRating.Attributes.Add("class", "score-7 ratingCircle");
            else if (rating > 6m)
                IncomingLinksRating.Attributes.Add("class", "score-6 ratingCircle");
            else if (rating > 5m)
                IncomingLinksRating.Attributes.Add("class", "score-5 ratingCircle");
            else if (rating > 4m)
                IncomingLinksRating.Attributes.Add("class", "score-4 ratingCircle");
            else if (rating > 3m)
                IncomingLinksRating.Attributes.Add("class", "score-3 ratingCircle");
            else if (rating > 2m)
                IncomingLinksRating.Attributes.Add("class", "score-2 ratingCircle");
            else if (rating > 1m)
                IncomingLinksRating.Attributes.Add("class", "score-1 ratingCircle");
            else
                IncomingLinksRating.Attributes.Add("class", "score-0 ratingCircle");
        }
    }
}
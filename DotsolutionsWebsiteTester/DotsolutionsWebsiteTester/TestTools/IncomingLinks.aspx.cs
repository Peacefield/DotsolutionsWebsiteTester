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

        private void GetIncomingLinks()
        {
            var rating = 10m;
            var sitemap = (List<string>)Session["selectedSites"];
            var message = "";
            // Start setting up MozscapeAPI
            var strAccessID = GetFromApiKeys("MozscapeAccessId");
            var strPrivateKey = GetFromApiKeys("MozscapeSecretKey");
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
                var strExternalLinks = msURLMetrics.ueid;
                var strMozRankUrl = msURLMetrics.umrp;
                var strMozRankCrawled = msURLMetrics.ulc;

                var strMozRankCrawledDate = UnixTimeStampToDateTime(strMozRankCrawled);

                totalLinks += Int32.Parse(strExternalLinks);
                totalRating += decimal.Parse(strMozRankUrl, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

                if (strMozRankCrawled == "0")
                {
                    strMozRankCrawledDate = "Niet bekend";
                }
                var strMozRankUrlRounded = decimal.Round(decimal.Parse(strMozRankUrl, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture), 1).ToString();
                var intExternalLinks = Int32.Parse(strExternalLinks);
                AddToTable(page, intExternalLinks.ToString("#,##0"), strMozRankUrlRounded, strMozRankCrawledDate);
            }

            if (isDetailed)
                IncomingLinksTableHidden.Attributes.Remove("class");

            totalRating = decimal.Round(totalRating / sitemap.Count, 1);

            message += "<div class='well well-lg resultWell text-center'>"
                + "<span class='largetext'>" + totalLinks.ToString("#,##0") + "</span><br/>"
                + "<span>links gevonden die naar de geteste pagina's verwijzen</span></div>"
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg resultWell text-center'>"
                + "<span class='largetext'>" + totalRating + "</span><br/>"
                + "<span>Gemiddelde MozRank score</span></div>";

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
        /// Get ApiKey from Session["ApiKeys"]
        /// </summary>
        /// <param name="key">ApiKey</param>
        /// <returns>ApiKey Value</returns>
        private string GetFromApiKeys(string key)
        {
            var list = (List<KeyValuePair<string, string>>)Session["ApiKeys"];
            foreach (var element in list)
                if (element.Key == key)
                    return element.Value;
            return "";
        }

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
        /// <param name="linkAmount">amount of equity links found</param>
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
            if (rating < 6m)
                IncomingLinksRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                IncomingLinksRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                IncomingLinksRating.Attributes.Add("class", "excellentScore ratingCircle");
        }
    }
}
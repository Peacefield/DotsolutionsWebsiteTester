using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class AmountOfContent : System.Web.UI.Page
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

            GetAmountResults();

            var sb = new System.Text.StringBuilder();
            AmountOfContentSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["AmountOfContent"] = htmlstring;
        }
        // http://www.wordstream.com/blog/ws/2010/05/05/word-count-for-seo raadt 500 woorden aan, minimaal.
        // http://seocopywriting.com/whats-the-best-word-count-for-seo-copywriting/ Kwaliteit over kwantiteit
        // http://www.socialmediatoday.com/content/longer-better-blog-content-truth-or-myth 1500 tot 2000 leidde tot positieve resultaten

        /// <summary>
        /// Start tests to get a rating for the Amount of Content available on found pages
        /// </summary>
        private void GetAmountResults()
        {
            var rating = 10.0m;
            var sitemap = (List<string>)Session["selectedSites"];
            var isDetailed = (bool)Session["IsDetailedTest"];
            var lowContentPageCnt = 0;
            var totalContentCount = 0;
            foreach (var page in sitemap)
            {
                var wordCount = GetContentCount(page);

                var icon = "<i class='fa fa-check fa-check-green'></i>";
                if (wordCount < 500)
                {
                    // te weinig
                    rating = rating - ((1m / (decimal)sitemap.Count) * 10m);
                    lowContentPageCnt++;
                    icon = "<i class='fa fa-times fa-times-red'></i>";
                }

                AddToTable(icon, page, wordCount.ToString("#,##0"));
                totalContentCount += wordCount;
            }
            var lowContentPageCntPercentage = ((decimal)lowContentPageCnt / (decimal)sitemap.Count) * 100m;
            var averageCount = totalContentCount / sitemap.Count;

            var message = "<div class='well well-lg resultWell text-center'>"
                + "<span class='largetext'>" + lowContentPageCntPercentage + "%</span><br/>"
                + "<span>pagina's met te weinig content</span></div>"
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg resultWell text-center'>"
                + "<i class='fa fa-font fa-3x'></i><br/>"
                + "<span>Gemiddeld " + averageCount.ToString("#,##0") + " woorden per pagina</span></div>";

            if (lowContentPageCnt > 0)
            {
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> " + lowContentPageCnt + " pagina's met te weinig content gevonden. "
                    + "Doordat er bij minder inhoudelijk content minder kans is dat de website verschijnt in gerelateerde zoekopdrachten is dit slecht. "
                    + "Er wordt dan ook een minimum van 500 woorden per pagina aangeraden.</span></div>";
            }
            else
            {
                message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Op alle pagina's is genoeg content gevonden. "
                    + "Doordat er bij meer inhoudelijk content meer kans is dat de website verschijnt in gerelateerde zoekopdrachten is dit uitstekend.</span></div>";
            }
            
            // Show table when detailed test is selected and something was added to table
            if (isDetailed /*&& lowContentPageCnt > 0*/)
                AmountOfContentTableHidden.Attributes.Remove("class");

            AmountOfContentResults.InnerHtml = message;

            // Set rating
            if (rating == 10.0m)
                rating = 10m;
            if (rating < 0.0m)
                rating = 0.0m;
            var rounded = decimal.Round(rating, 1);
            Session["AmountOfContentRating"] = rounded;
            AmountOfContentRating.InnerHtml = rounded.ToString();
            SetRatingDisplay(rounded);
            // Set sessions
            var temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = temp + rounded;

            temp = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = temp + rounded;
        }

        /// <summary>
        /// Get amount of content of a page within p and/or a elements
        /// </summary>
        /// <param name="page">Page to be tested</param>
        /// <returns>int amount of words</returns>
        private int GetContentCount(string page)
        {
            Debug.WriteLine(" ----- Testing " + page + " ----- ");

            var webget = new HtmlWeb();
            var doc = webget.Load(page);
            var bodycontent = doc.DocumentNode.SelectNodes("//p | //a");
            var innertext = "";
            var wordCount = 0;

            foreach (var item in bodycontent)
            {
                innertext += item.InnerText + " ";
            }
            Debug.WriteLine(innertext);
            string[] content = innertext.Split(null);

            foreach (var item in content)
                if (item.Length > 0)
                    wordCount++;

            Debug.WriteLine("wordCount: " + wordCount);

            return wordCount;
        }

        /// <summary>
        /// Adds result to table IntLinksTable
        /// </summary>
        /// <param name="page">Page of origin</param>
        /// <param name="wordcount">Word count of page</param>
        private void AddToTable(string icon, string page, string wordcount)
        {
            var tRow = new TableRow();

            var tCellIcon = new TableCell();
            tCellIcon.Text = icon;
            tRow.Cells.Add(tCellIcon);

            var tCellPage = new TableCell();
            tCellPage.Text = "<a href='" + page + "' target='_blank'>" + page + "</a>";
            tRow.Cells.Add(tCellPage);

            var tCellCount = new TableCell();
            tCellCount.Text = wordcount;
            tRow.Cells.Add(tCellCount);


            AmountOfContentTable.Rows.Add(tRow);
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 6m)
                AmountOfContentRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                AmountOfContentRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                AmountOfContentRating.Attributes.Add("class", "excellentScore ratingCircle");
        }
    }
}
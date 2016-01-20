using HtmlAgilityPack;
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
    public partial class PageTitles : System.Web.UI.Page
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

            var ths = new ThreadStart(GetPageTitles);
            var th = new Thread(ths);
            th.Start();

            th.Join();

            var sb = new System.Text.StringBuilder();
            PageTitlesSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["PageTitles"] = htmlstring;
        }

        /// <summary>
        /// Gather info on all available titles into lists and rate titles
        /// </summary>
        private void GetPageTitles()
        {
            // Voor de beoordeling geldt per pagina zonder titel de formule 1/{aantal geteste pagina's} * 15
            // Per pagina met een te lange titel geldt de formule 1/{aantal geteste pagina's} * 10
            // Het resultaat hiervan wordt afgetrokken van de huidige beoordeling

            Debug.WriteLine("PageTitles >>>>>");
            var rating = 10.0m;
            var sitemap = (List<string>)Session["selectedSites"];
            var noTitles = new List<string>();
            var longTitles = new List<string>();
            var message = "";
            var isDetailed = (bool)Session["IsDetailedTest"];

            foreach (var page in sitemap)
            {
                var title = GetTitle(page);
                if (title != "")
                {
                    // Has title in <head>
                    if (IsLongTitle(title))
                    {
                        // Has long title ( title > 59 chars)
                        longTitles.Add(page);
                        AddToTable(title, page);
                        rating = rating - ((1m / (decimal)sitemap.Count) * 10m);
                    }
                }
                else
                {
                    // Does not have title in <head>
                    // Add to List
                    noTitles.Add(page);
                    rating = rating - ((1m / (decimal)sitemap.Count) * 15m);
                }
            }

            if (noTitles.Count > 0)
            {
                var noTitleGrammar = "zijn " + noTitles.Count + " pagina's";
                if (noTitles.Count == 1)
                {
                    noTitleGrammar = "is " + noTitles.Count + " pagina";
                }
                var noTitleUl = "";

                foreach (var page in noTitles)
                {
                    noTitleUl += "<li><a href='" + page + "' target='_blank'>" + page + "</a></li>";
                }

                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er " + noTitleGrammar + " gevonden zonder titel:<ul>" + noTitleUl + "</ul>"
                    + "Dit is zeer slecht doordat een gebruiker snel moet kunnen weten waar deze zich bevindt en doordat zoekmachines de titel weergeven op de resultatenpagina.</span></div>";
            }

            if (longTitles.Count > 0)
            {
                var longTitlesGrammar = "heeft";
                if (longTitles.Count > 1)
                    longTitlesGrammar = "hebben";
                var siteMapGrammer = "pagina's";
                if (sitemap.Count == 1)
                    siteMapGrammer = "pagina";

                if (isDetailed)
                    PageTitlesTableHidden.Attributes.Remove("class");

                if (!isDetailed)
                    PageTitlesTable.Rows.Clear();

                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> " + longTitles.Count + " van de " + sitemap.Count + " geteste " + siteMapGrammer + " " + longTitlesGrammar + " een te lange titel.<br/>"
                    + "Dit is slecht doordat een gebruiker snel moet kunnen weten waar deze zich bevindt en zoekmachines zullen te lange titels afkappen waardoor er niet het maximale uit een titel kan worden gehaald op een resultatenpagina.</span></div>";
            }

            if (longTitles.Count == 0 && noTitles.Count == 0)
            {
                message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Op elke pagina is een goede titel aanwezig.<br/>"
                    + "Dit is uitstekend doordat een gebruiker snel moet kunnen weten waar deze zich bevindt en doordat zoekmachines de titel weergeven op de resultatenpagina.</span></div>";
            }

            var goodTitleCnt = sitemap.Count - longTitles.Count;
            var goodTitlePerc = ((decimal)goodTitleCnt / (decimal)sitemap.Count) * 100m;
            var titleUsedPerc = (((decimal)sitemap.Count - (decimal)noTitles.Count) / (decimal)sitemap.Count) * 100m;

            var pieGraph1 = GetPieGraphContent(titleUsedPerc);
            var pieGraph2 = GetPieGraphContent(goodTitlePerc);

            message = "<div class='well well-lg resultWell text-center'>"
                + "<div class='pieContainer'>" + pieGraph1 + "</div>"
                + "<br/><span>van de pagina's bevat een titel</span></div>"
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg resultWell text-center thirdPercentageChild'>"
                + "<div class='pieContainer'>" + pieGraph2 + "</div>"
                + "<br/><span>van de pagina's heeft een goede titel</span></div>"
                + message;

            PageTitleResults.InnerHtml = message;

            if (rating < 0m)
            {
                rating = 0.0m;
            }
            if (rating == 10.0m)
                rating = 10m;
            decimal rounded = decimal.Round(rating, 1);
            PageTitlesRating.InnerHtml = rounded.ToString();
            var ratingAccess = (decimal)Session["RatingAccess"];
            Session["RatingAccess"] = rounded + ratingAccess;
            var RatingMarketing = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = rounded + RatingMarketing;
            SetRatingDisplay(rating);
            Session["PageTitlesRating"] = rounded;
        }

        /// <summary>
        /// Get if title is present
        /// </summary>
        /// <param name="page">page of origin</param>
        /// <returns>string title, empty when no title was found</returns>
        private string GetTitle(string page)
        {
            var title = "";
            var webget = new HtmlWeb();
            var doc = webget.Load(page);
            if (doc.DocumentNode.SelectNodes("//title[parent::head]") != null)
            {
                foreach (var node in doc.DocumentNode.SelectNodes("//title[parent::head]"))
                {
                    if (node.InnerText != "")
                    {
                        title = node.InnerText;
                        Debug.WriteLine("Gevonden titel is: " + title);
                    }
                }
            }
            return title;
        }

        /// <summary>
        /// Get if title is long
        /// </summary>
        /// <param name="title">title</param>
        /// <returns>true if title > 59 chars</returns>
        private bool IsLongTitle(string title)
        {
            if (title.Length <= 59)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Add to PageTitlesTable
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="page">Page of origin</param>
        private void AddToTable(string title, string page)
        {
            var tRow = new TableRow();

            var tCellTitle = new TableCell();
            tCellTitle.Text = title;
            tRow.Cells.Add(tCellTitle);

            var tCellCssPage = new TableCell();
            tCellCssPage.Text = "<a href='" + page + "' target='_blank' >" + page + "</a>";
            tRow.Cells.Add(tCellCssPage);

            PageTitlesTable.Rows.Add(tRow);
        }

        /// <summary>
        /// Get the HTML snippet for a circle diagram that displays the percentage accordingly
        /// </summary>
        /// <param name="percentage">decimal percentage</param>
        /// <returns>HTML snippet</returns>
        private string GetPieGraphContent(decimal percentage)
        {
            var mainBckClr = "";
            var pieBckClr = "";
            var pieInsideLeft = "";
            var holdTransform = "";
            var pieTransform = "";

            if (percentage > 50)
            {
                mainBckClr = "#54b721";
                pieBckClr = "rgba(189, 195, 199,1)";
                if (percentage == 100)
                    pieInsideLeft = "7px";
                else
                    pieInsideLeft = "17px";
                holdTransform = "180";
            }
            else
            {
                mainBckClr = "rgba(189, 195, 199,.5)";
                pieBckClr = "#54b721";
                if (percentage > 9)
                    pieInsideLeft = "17px";
                else
                    pieInsideLeft = "24px";
                holdTransform = "0";
            }

            pieTransform = Math.Round((percentage / 100m * 360m), 0).ToString();
            var content = "<div class='pieBackground' style='background-color: " + mainBckClr + ";'></div>"
                + "<div class='pieInside'><span style='left: " + pieInsideLeft + ";'>" + percentage.ToString("#,##0") + "%</span></div>"
                + "<div id='pieSlice1' class='hold' style='-webkit-transform: rotate(" + holdTransform + "deg);-moz-transform: rotate(" + holdTransform + "deg);-o-transform: rotate(" + holdTransform + "deg);transform: rotate(" + holdTransform + "deg);'>"
                + "<div class='pie' style='background-color: " + pieBckClr + ";-webkit-transform: rotate(" + pieTransform + "deg);-moz-transform: rotate(" + pieTransform + "deg);-o-transform: rotate(" + pieTransform + "deg);transform: rotate(" + pieTransform + "deg);'></div>"
                + "</div>";

            return content;
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            var element = PageTitlesRating;

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
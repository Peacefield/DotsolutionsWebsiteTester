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
                        // Has long title ( title > 55 chars)
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
                    noTitleUl += "<li>" + page + "</li>";
                }

                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> Er " + noTitleGrammar + " gevonden zonder titel:</span><ul>" + noTitleUl + "</ul></div>";
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
                {
                    message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                        + "<span> " + longTitles.Count + " van de " + sitemap.Count + " geteste " + siteMapGrammer + " " + longTitlesGrammar + " een te lange titel:</span></div>";
                    PageTitlesTableHidden.Attributes.Remove("class");
                }
                else
                    message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                        + "<span> Kort verhaal over hoe lange titels gebruikt worden op sommige pagina's en waarom dit niet goed is</span></div>";
            }

            if (longTitles.Count == 0 && noTitles.Count == 0)
            {
                message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span> Op elke pagina is een goede titel aanwezig</span></div>";
            }

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
        /// <returns>true if title > 55 chars</returns>
        private bool IsLongTitle(string title)
        {
            if (title.Length <= 55)
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
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 6m)
                PageTitlesRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                PageTitlesRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                PageTitlesRating.Attributes.Add("class", "excellentScore ratingCircle");
        }
    }
}
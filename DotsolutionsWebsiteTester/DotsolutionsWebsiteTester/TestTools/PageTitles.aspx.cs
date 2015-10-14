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

        private void GetPageTitles()
        {
            var rating = 10m;
            var sitemap = (List<string>)Session["selectedSites"];
            var noTitles = new List<string>();
            var longTitles = new List<string>();
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
                        rating = rating - ((1m / (decimal)sitemap.Count) * 5m);
                    }
                }
                else
                {
                    // Does not have title in <head>
                    // Add to List
                    noTitles.Add(page);
                    rating = rating - ((1m / (decimal)sitemap.Count) * 10m);
                }
            }

            if (noTitles.Count > 0)
            {
                var noTitleGrammar = "zijn " + noTitles.Count + " pagina's";
                if (noTitles.Count == 1)
                {
                    noTitleGrammar = "is " + noTitles.Count + " pagina";
                }
                PageTitleResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> Er " + noTitleGrammar + " gevonden zonder titel</span></div>";
            }

            if (longTitles.Count > 0)
            {
                var longTitlesGrammar = "heeft";
                if (longTitles.Count > 1)
                    longTitlesGrammar = "hebben";
                var siteMapGrammer = "pagina's";
                if (sitemap.Count == 1)
                    siteMapGrammer = "pagina";

                PageTitlesTableHidden.Attributes.Remove("class");
                PageTitleResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> " + longTitles.Count + " van de " + sitemap.Count + " geteste " + siteMapGrammer + " " + longTitlesGrammar + " een te lange titel:</span></div>";
            }

            if (longTitles.Count == 0 && noTitles.Count == 0)
            {
                PageTitleResults.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span> Op elke pagina is een goede titel aanwezig</span></div>";
            }


            if (rating < 1m)
            {
                rating = 1.0m;
            }

            decimal rounded = decimal.Round(rating, 1);
            Rating.InnerHtml = rounded.ToString();
            var ratingAccess = (decimal)Session["RatingAccess"];
            Session["RatingAccess"] = rounded + ratingAccess;
            var RatingMarketing = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = rounded + RatingMarketing;
        }

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

        private bool IsLongTitle(string title)
        {
            if (title.Length < 55)
            {
                return false;
            }
            return true;
        }

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
    }
}
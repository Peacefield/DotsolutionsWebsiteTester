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
            var totalCount = 0;
            var message = "";
            var isDetailed = (bool)Session["IsDetailedTest"];

            foreach (var page in sitemap)
            {
                var threadList = new List<Thread>();
                var Webget = new HtmlWeb();
                var doc = Webget.Load(page);
                if (doc.DocumentNode.SelectNodes("//a[@href]") != null)
                {
                    foreach (var link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        var ths = new ThreadStart(() => TestFormat(link.Attributes["href"].Value, page));
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

                    if(isDetailed)
                        UrlFormatHiddenTable.Attributes.Remove("class");
                }

                longUrl.Clear();
                dirtyUrl.Clear();
            }

            if (totalCount == 0)
            {
                message = "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12 text-center' role='alert'>"
                    + "<i class='fa fa-link fa-3x'></i><br/>"
                    + "<span class='messageText'> Alle URLs zijn schoon en gebruiksvriendelijk.</span></div>";
            }
            else
            {
                message = "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12 text-center' role='alert'>"
                    + "<i class='fa fa-chain-broken fa-3x'></i><br/>"
                    + "<span class='messageText'> " + totalCount + " foutieve URLs gevonden.</span></div>";
            }

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
        private void TestFormat(string link, string page)
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
                    rating = rating - (5m / sitemap.Count);
                    longUrl.Add(path);
                }

                if (IsDirty(path))
                {
                    rating = rating - (5m / sitemap.Count);
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
            if (rating < 6m)
                UrlFormatRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                UrlFormatRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                UrlFormatRating.Attributes.Add("class", "excellentScore ratingCircle");
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
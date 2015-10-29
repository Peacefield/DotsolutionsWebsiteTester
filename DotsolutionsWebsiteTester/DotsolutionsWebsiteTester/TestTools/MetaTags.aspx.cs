using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    // Meta keywords zijn helemaal niet belangrijk!
    // Meta beschrijving is belangrijk, 115-145 karakters // http://blog.hubspot.com/marketing/seo-tactics-2015 // https://support.google.com/webmasters/answer/79812?hl=en
    // Title is geen meta tag maar wordt wel gebruikt in SERP
    // Robots is belangrijk; content all is alles laten indexeren


    public partial class MetaTags : System.Web.UI.Page
    {
        List<string> sitemap;
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
            GetMetaTags();

            var sb = new System.Text.StringBuilder();
            MetaTagsSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["MetaTags"] = htmlstring;
        }

        private void GetMetaTags()
        {
            var hasLongDescription = new List<string>();
            var hasNoDescription = new List<string>();
            var hasNoRobots = new List<string>();
            var rating = 10.0m;

            foreach (var url in sitemap)
            {
                Debug.WriteLine("Getting meta tags from: " + url);
                var Webget = new HtmlWeb();
                var doc = Webget.Load(url);

                if (doc.DocumentNode.SelectSingleNode("//head") != null)
                {
                    var normalMetas = GetNormalMetaTags(doc);
                    var openGraphMeta = GetOpenGraphMetaTags(doc);
                    var httpEquivMeta = GetHttpEquivMetaTags(doc);
                    var hasDescription = 0;
                    var hasRobots = 0;

                    var metaListContainer = new List<KeyValuePair<string, Dictionary<string, string>>>();
                    metaListContainer.Add(new KeyValuePair<string, Dictionary<string, string>>("name", normalMetas));
                    metaListContainer.Add(new KeyValuePair<string, Dictionary<string, string>>("property", openGraphMeta));
                    metaListContainer.Add(new KeyValuePair<string, Dictionary<string, string>>("http-equiv", httpEquivMeta));

                    foreach (var list in metaListContainer)
                    {
                        var count = 0;
                        foreach (var item in list.Value)
                        {
                            if (count < 5)
                                AddToTable(url, list.Key, item.Key, item.Value);
                            count++;

                            if (item.Key == "description" && item.Value.Length > 0)
                            {
                                hasDescription++;
                                if (item.Value.Length > 145)
                                    hasLongDescription.Add(url);
                            }

                            if (item.Key == "robots" && item.Value.Length > 0)
                                hasRobots++;
                        }
                        if (count >= 5)
                            AddToTable(url, "<strong>" + list.Key + "</strong>", "<strong>...</strong>", "<strong>" + (count - 4) + " niet getoond</strong>");
                    }

                    if (hasDescription == 0)
                        hasNoDescription.Add(url);
                    if (hasRobots == 0)
                        hasNoRobots.Add(url);
                }
            }
            rating = GetDescriptionRating(rating, hasNoDescription, hasLongDescription);
            rating = GetRobotRating(rating, hasNoRobots);
            SetRating(rating);
        }

        private Dictionary<string, string> GetNormalMetaTags(HtmlDocument doc)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            if (doc.DocumentNode.SelectNodes("//meta[@name and @content][parent::head]") != null)
                foreach (var node in doc.DocumentNode.SelectNodes("//meta[@name and @content][parent::head]"))
                    dictionary.Add(node.Attributes["name"].Value, node.Attributes["content"].Value);
            return dictionary;
        }

        private Dictionary<string, string> GetOpenGraphMetaTags(HtmlDocument doc)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            if (doc.DocumentNode.SelectNodes("//meta[@property and @content][parent::head]") != null)
                foreach (var node in doc.DocumentNode.SelectNodes("//meta[@property and @content][parent::head]"))
                    dictionary.Add(node.Attributes["property"].Value, node.Attributes["content"].Value);
            return dictionary;
        }

        private Dictionary<string, string> GetHttpEquivMetaTags(HtmlDocument doc)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            if (doc.DocumentNode.SelectNodes("//meta[@http-equiv and @content][parent::head]") != null)
                foreach (var node in doc.DocumentNode.SelectNodes("//meta[@http-equiv and @content][parent::head]"))
                    dictionary.Add(node.Attributes["http-equiv"].Value, node.Attributes["content"].Value);
            return dictionary;
        }

        private decimal GetDescriptionRating(decimal rating, List<string> hasNoDescription, List<string> hasLongDescription)
        {
            if (hasNoDescription.Count == 0 && hasLongDescription.Count == 0)
            {
                // Good job, using no long descriptions!
                MetaErrorsFound.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span> Er wordt op alle pagina's correct gebruik gemaakt van de description meta-tag</span></div>";
            }
            else
            {
                // Booo
                if (hasNoDescription.Count > 0)
                {
                    var ul = "";
                    foreach (var item in hasNoDescription)
                    {
                        ul += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";
                        rating = rating - (10m / (decimal)sitemap.Count);
                    }

                    MetaErrorsFound.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                        + "<span> De volgende pagina's gebruiken geen description meta-tag:</span>"
                        + "<ul>" + ul + "</ul></div>";
                }

                if (hasLongDescription.Count > 0)
                {
                    var ul = "";
                    foreach (var item in hasLongDescription)
                    {
                        rating = rating - (5m / (decimal)sitemap.Count);
                        ul += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";
                    }
                    MetaErrorsFound.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                        + "<span> De volgende pagina's hebben een te lange meta-description:</span>"
                        + "<ul>" + ul + "</ul></div>";
                }
            }
            return rating;
        }

        private decimal GetRobotRating(decimal rating, List<string> hasNoRobots)
        {
            if (hasNoRobots.Count == 0)
            {
                // Good job, using robots!
                MetaErrorsFound.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span> Er wordt op alle pagina's gebruik gemaakt van de robots meta-tag</span></div>";
            }
            else
            {
                // Booo
                var ul = "";
                foreach (var item in hasNoRobots)
                {
                    rating = rating - (10m / (decimal)sitemap.Count);
                    ul += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";
                }
                MetaErrorsFound.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> De volgende pagina's gebruiken geen robots meta-tag:</span>"
                    + "<ul>" + ul + "</ul></div>";
            }
            return rating;
        }

        /// <summary>
        /// Add a meta tag to the table
        /// </summary>
        /// <param name="url">Page of origin</param>
        /// <param name="type">Type of meta tag</param>
        /// <param name="name">Key</param>
        /// <param name="content">Value</param>
        private void AddToTable(string url, string type, string name, string content)
        {
            MetaResultsTableHidden.Attributes.Remove("class");
            var tRow = new TableRow();

            var tCellUrl = new TableCell();
            tCellUrl.Text = "<a href='" + url + "' target='_blank'>" + url + "</a>";
            tRow.Cells.Add(tCellUrl);

            var tCellType = new TableCell();
            tCellType.Text = type;
            tRow.Cells.Add(tCellType);

            var tCellName = new TableCell();
            tCellName.Text = name;
            tRow.Cells.Add(tCellName);

            var tCellContent = new TableCell();
            tCellContent.Text = content;
            tRow.Cells.Add(tCellContent);

            table.Rows.Add(tRow);
        }

        /// <summary>
        /// Set rating in HTML and Sessions
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRating(decimal rating)
        {
            if (rating < 0m)
                rating = 0.0m;
            if (rating == 10.0m)
                rating = 10m;

            // Add to HTMl
            decimal rounded = decimal.Round(rating, 1);
            MetaTagsRating.InnerHtml = rounded.ToString();
            SetRatingDisplay(rating);

            // Set sessions
            var temp = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = temp + rounded;

            temp = (decimal)Session["RatingTech"];
            Session["RatingTech"] = temp + rounded;

            Session["MetaTagsRating"] = rounded;
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 6m)
                MetaTagsRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                MetaTagsRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                MetaTagsRating.Attributes.Add("class", "excellentScore ratingCircle");
        }
    }
}
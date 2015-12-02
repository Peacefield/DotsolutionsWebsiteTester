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
    // Meta beschrijving is belangrijk, 115-150 karakters // http://blog.hubspot.com/marketing/seo-tactics-2015 // https://support.google.com/webmasters/answer/79812?hl=en
    // Robots is belangrijk; content all is alles laten indexeren
    
    public partial class MetaTags : System.Web.UI.Page
    {
        List<string> sitemap;
        private string message;
        private bool isDetailed;

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

        /// <summary>
        /// Main function.
        /// Get/show meta-tags and set rating
        /// </summary>
        private void GetMetaTags()
        {
            Debug.WriteLine("MetaTags >>>>>");
            var hasLongDescription = new List<string>();
            var hasNoDescription = new List<string>();
            var hasNoKeywords = new List<string>();
            var hasNoRobots = new List<string>();
            var rating = 10.0m;
            isDetailed = (bool)Session["IsDetailedTest"];
            foreach (var url in sitemap)
            {
                Debug.WriteLine("Getting meta tags from: " + url);
                var Webget = new HtmlWeb();
                var doc = Webget.Load(url);

                if (doc.DocumentNode.SelectSingleNode("//head") != null)
                {
                    var normalMetas = GetNormalMetaTags(doc);
                    //var openGraphMeta = GetOpenGraphMetaTags(doc);
                    //var httpEquivMeta = GetHttpEquivMetaTags(doc);
                    var hasDescription = false;
                    var hasKeywords = false;
                    var hasRobots = false;

                    var metaListContainer = new List<KeyValuePair<string, Dictionary<string, string>>>();
                    metaListContainer.Add(new KeyValuePair<string, Dictionary<string, string>>("name", normalMetas));
                    //metaListContainer.Add(new KeyValuePair<string, Dictionary<string, string>>("property", openGraphMeta));
                    //metaListContainer.Add(new KeyValuePair<string, Dictionary<string, string>>("http-equiv", httpEquivMeta));

                    foreach (var list in metaListContainer)
                    {
                        var count = 0;
                        foreach (var item in list.Value)
                        {
                            count++;

                            if (item.Key == "description")
                            {
                                AddToTable(url, list.Key, item.Key, item.Value);
                                if (item.Value.Length > 0)
                                {
                                    hasDescription = true;
                                    if (HttpUtility.HtmlDecode(item.Value).Length > 150)
                                        hasLongDescription.Add(url);
                                }
                            }

                            if (item.Key == "keywords")
                            {
                                AddToTable(url, list.Key, item.Key, item.Value);
                                if (item.Value.Length > 0)
                                    hasKeywords = true;
                            }

                            if (item.Key == "robots" && item.Value.Length > 0)
                            {
                                AddToTable(url, list.Key, item.Key, item.Value);
                                hasRobots = true;
                            }
                        }
                        //if (count > 0)
                        //{
                        //    if (count > 1)
                        //        AddToTable(url, "<strong>" + list.Key + "</strong>", "<strong>...</strong>", "<strong>" + count + " meta tags van dit type gevonden</strong>");
                        //    else
                        //        AddToTable(url, "<strong>" + list.Key + "</strong>", "<strong>...</strong>", "<strong>" + count + " meta tag van dit type gevonden</strong>");
                        //}

                        if (isDetailed)
                            MetaResultsTableHidden.Attributes.Remove("class");
                        if (!isDetailed)
                            MetaResultsTable.Rows.Clear();
                    }

                    if (!hasDescription)
                        hasNoDescription.Add(url);
                    if (!hasKeywords)
                        hasNoKeywords.Add(url);
                    if (!hasRobots)
                        hasNoRobots.Add(url);
                }
            }
            SetSERPDisplay();
            rating = GetDescriptionRating(rating, hasNoDescription, hasLongDescription);
            rating = GetRobotRating(rating, hasNoRobots);
            rating = GetKeywordsRating(rating, hasNoKeywords);
            rating = GetTitleRating(rating);
            SetRating(rating);
            MetaErrorsFound.InnerHtml = message;
        }

        /// <summary>
        /// Add example of result in Search Engine Result Page to string message
        /// </summary>
        private void SetSERPDisplay()
        {
            var url = Session["MainUrl"].ToString();
            var Webget = new HtmlWeb();
            var doc = Webget.Load(url);
            if (doc.DocumentNode.SelectSingleNode("//head") != null)
            {
                var title = "";
                var desc = "";
                var normalMetas = GetNormalMetaTags(doc);

                foreach (var item in normalMetas)
                {
                    if (item.Key == "description")
                    {
                        desc = HttpUtility.HtmlDecode(item.Value);
                        Debug.WriteLine("desc: " + desc);
                        Debug.WriteLine("desc length: " + desc.Length);

                        if (desc.Length > 150)
                        {
                            string[] split = desc.Split(' ');
                            var temp = "";
                            foreach (var word in split)
                            {
                                temp += word + " ";
                                if (temp.Length <= 150)
                                {
                                    desc = temp;
                                }
                                else
                                    break;
                            }
                            desc += "...";
                        }
                    }
                }

                if (doc.DocumentNode.SelectNodes("//title[parent::head]") != null)
                {
                    foreach (var node in doc.DocumentNode.SelectNodes("//title[parent::head]"))
                    {
                        if (node.InnerText != "")
                        {
                            title = HttpUtility.HtmlDecode(node.InnerText);
                            Debug.WriteLine("Gevonden titel is: " + title);
                            if (title.Length > 55)
                            {
                                string[] split = title.Split(' ');
                                var temp = "";
                                foreach (var word in split)
                                {
                                    temp += word;
                                    if (temp.Length <= 55)
                                    {
                                        temp += " ";
                                        title = temp;
                                    }
                                    else
                                        break;
                                }
                                title += "...";
                            }
                        }
                        else
                            title = "Untitled";
                    }
                }

                if (title == "")
                    title = "Untitled";
                if (desc == "")
                    desc = "Geen description meta-tag gevonden.";

                message += "<span class='help-block'>Zo ziet de website er uit op de resultatenpagina van Google:</span>";
                message += "<div class='well well-sm col-md-6 col-sm-7'>"
                            + "<h3 class='googleResult_title noselect'>" + title + "</h3>"
                            + "<p class='googleResult_url noselect'>" + url + "</p>"
                            + "<p class='googleResult_desc noselect'>" + desc + "</p>"
                            + "</div>";
            }
        }

        /// <summary>
        /// Get Dictionary containing meta tags with name and content attributes
        /// </summary>
        /// <param name="doc">HtmlDocument</param>
        /// <returns>Dictionary as name:content</returns>
        private Dictionary<string, string> GetNormalMetaTags(HtmlDocument doc)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            if (doc.DocumentNode.SelectNodes("//meta[@name and @content][parent::head]") != null)
                foreach (var node in doc.DocumentNode.SelectNodes("//meta[@name and @content][parent::head]"))
                    dictionary.Add(node.Attributes["name"].Value, node.Attributes["content"].Value);
            return dictionary;
        }

        #region REDACTED
        /// <summary>
        /// Get Dictionary containing meta tags with property and content attributes
        /// </summary>
        /// <param name="doc">HtmlDocument</param>
        /// <returns>Dictionary as property:content</returns>
        //private Dictionary<string, string> GetOpenGraphMetaTags(HtmlDocument doc)
        //{
        //    Dictionary<string, string> dictionary = new Dictionary<string, string>();

        //    if (doc.DocumentNode.SelectNodes("//meta[@property and @content][parent::head]") != null)
        //        foreach (var node in doc.DocumentNode.SelectNodes("//meta[@property and @content][parent::head]"))
        //            dictionary.Add(node.Attributes["property"].Value, node.Attributes["content"].Value);
        //    return dictionary;
        //}

        /// <summary>
        /// Get Dictionary containing meta tags with http-equiv and content attributes
        /// </summary>
        /// <param name="doc">HtmlDocument</param>
        /// <returns>Dictionary as http-equiv:content</returns>
        //private Dictionary<string, string> GetHttpEquivMetaTags(HtmlDocument doc)
        //{
        //    Dictionary<string, string> dictionary = new Dictionary<string, string>();

        //    if (doc.DocumentNode.SelectNodes("//meta[@http-equiv and @content][parent::head]") != null)
        //        foreach (var node in doc.DocumentNode.SelectNodes("//meta[@http-equiv and @content][parent::head]"))
        //            dictionary.Add(node.Attributes["http-equiv"].Value, node.Attributes["content"].Value);
        //    return dictionary;
        //}
        #endregion

        /// <summary>
        /// Get rating in reaction to description-testresults
        /// </summary>
        /// <param name="rating">current rating</param>
        /// <param name="hasNoDescription">list string hasNoDescription</param>
        /// <param name="hasLongDescription">list string hasLongDescription</param>
        /// <returns>rating after reduction</returns>
        private decimal GetDescriptionRating(decimal rating, List<string> hasNoDescription, List<string> hasLongDescription)
        {
            if (hasNoDescription.Count == 0 && hasLongDescription.Count == 0)
            {
                // Good job, using no long descriptions!
                message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er wordt op alle pagina's correct gebruik gemaakt van de description meta-tag. Dit is uitstekend doordat dit getoond wordt op de resultatenpagina van een zoekmachine en een zoekmachine weet wat er zich op de website kan bevinden.</span></div>";
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

                    message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                        + "<span class='messageText'> De volgende pagina's gebruiken geen description meta-tag:"
                        + "<ul>" + ul + "</ul>Dit is zeer slecht doordat dit getoond wordt op de resultatenpagina van een zoekmachine. Nu zal er een willekeurig stuk tekst worden getoond. Met de description meta-tag heeft de website hier zelf invloed op.</span></div>";
                }

                if (hasLongDescription.Count > 0)
                {
                    var ul = "";
                    foreach (var item in hasLongDescription)
                    {
                        rating = rating - (5m / (decimal)sitemap.Count);
                        ul += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";
                    }
                    message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                        + "<span class='messageText'> De volgende pagina's hebben een te lange meta-description:"
                        + "<ul>" + ul + "</ul>Dit is slecht doordat de zoekmachine deze zal afkappen bij het tonen van de website beschrijving.</span></div>";
                }
            }
            return rating;
        }

        /// <summary>
        /// Get rating in reaction to robot-testresults
        /// </summary>
        /// <param name="rating">current rating</param>
        /// <param name="hasNoRobots">list string hasNoRobots</param>
        /// <returns>rating after reduction</returns>
        private decimal GetRobotRating(decimal rating, List<string> hasNoRobots)
        {
            if (hasNoRobots.Count == 0)
            {
                // Good job, using robots!
                message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er wordt op alle pagina's gebruik gemaakt van de robots meta-tag. Dit is uitstekend aangezien zo wordt aangegeven welke delen van de website geïndexeerd mogen worden.</span></div>";
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
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> De volgende pagina's gebruiken geen robots meta-tag:"
                    + "<ul>" + ul + "</ul>Het gebruik van de robots meta-tag heeft veel invloed op de beoordeling en positionering vanuit een zoekmachine en is daarom zeer belangrijk.</span></div>";
            }
            return rating;
        }

        /// <summary>
        /// Get rating in reaction to keywords-testresults
        /// </summary>
        /// <param name="rating">current rating</param>
        /// <param name="hasNoKeywords">list string hasNoKeywords</param>
        /// <returns>rating after reduction</returns>
        private decimal GetKeywordsRating(decimal rating, List<string> hasNoKeywords)
        {
            if (hasNoKeywords.Count == 0)
            {
                // Good job, using robots!
                message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er wordt op alle pagina's gebruik gemaakt van de keywords meta-tag. Dit is uitstekend aangezien een zoekmachine de website kan indelen aan de hand hiervan.</span></div>";
            }
            else
            {
                // Booo
                var ul = "";
                foreach (var item in hasNoKeywords)
                {
                    rating = rating - (5m / (decimal)sitemap.Count);
                    ul += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";
                }
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> De volgende pagina's gebruiken geen keywords meta-tag:"
                    + "<ul>" + ul + "</ul>Het gebruik van de keywords meta-tag heeft niet veel invloed op de beoordeling en positionering vanuit een zoekmachine, maar het is beter wanneer het wel wordt toegevoegd.</span></div>";
            }
            return rating;
        }

        /// <summary>
        /// Get rating in reaction to title-testresults
        /// </summary>
        /// <param name="rating">current rating</param>
        /// <returns>rating after reduction</returns>
        private decimal GetTitleRating(decimal rating)
        {
            var url = Session["MainUrl"].ToString();
            var Webget = new HtmlWeb();
            var doc = Webget.Load(url);
            var isLong = false;
            var isEmpty = false;

            if (doc.DocumentNode.SelectNodes("//title[parent::head]") != null)
            {
                foreach (var node in doc.DocumentNode.SelectNodes("//title[parent::head]"))
                {
                    if (node.InnerText != "")
                    {
                        var title = HttpUtility.HtmlDecode(node.InnerText);
                        Debug.WriteLine("Gevonden titel is: " + title);
                        if (title.Length > 55)
                        {
                            isLong = true;
                        }
                    }
                    else
                        isEmpty = true;
                }
            }
            else
                isEmpty = true;

            if (isEmpty)
            {
                rating = rating - (10m / (decimal)sitemap.Count);
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er wordt geen titel gebruikt op de ingevoerde pagina. "
                    + "Dit is zeer slecht doordat dit de titel is die op een resultatenpagina te zien is van een zoekopdracht. Dit zal ook een negatieve invloed hebben op de positionering binnen deze pagina.</span></div>";
            }
            if (isLong)
            {
                rating = rating - (5m / (decimal)sitemap.Count);
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er wordt een te lange titel gebruikt op de ingevoerde pagina. Dit is slecht doordat de zoekmachine deze titel zal afkappen waardoor er niet het maximale uit een titel wordt gehaald.</span></div>";
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

            MetaResultsTable.Rows.Add(tRow);
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
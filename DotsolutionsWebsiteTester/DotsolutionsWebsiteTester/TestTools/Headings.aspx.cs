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
    public partial class Headings : System.Web.UI.Page
    {
        private List<string> noHeadings = new List<string>();
        private int errorCnt = 0;
        private int totalHeadingCnt = 0;
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

            var ths = new ThreadStart(GetHeadings);
            var th = new Thread(ths);
            th.Start();

            th.Join();

            var sb = new System.Text.StringBuilder();
            HeadingsSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Headings"] = htmlstring;
        }

        private void GetHeadings()
        {
            Debug.WriteLine("Headings >>>>>");
            var sitemap = (List<string>)Session["selectedSites"];
            var rating = 10.0m;
            var message = "";
            var isDetailed = (bool)Session["IsDetailedTest"];
            foreach (var url in sitemap)
            {
                GetHeadingsOnUrl(url);
            }

            if (noHeadings.Count > 0)
            {
                string unorderedlist = "<ul>";
                foreach (var item in noHeadings)
                {
                    // Point reduction equal to percentage of the total amount of tested sites
                    // E.g. when 1/5 has no heading it gets 1/5th of 10 reduction from remaining rating
                    rating = rating - ((1m / (decimal)sitemap.Count) * 10m);
                    unorderedlist += "<li><a href='"+item+"' target='_blank'>" + item + "</a></li>";
                }
                unorderedlist += "</ul>";

                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> De volgende pagina's gebruiken geen headers" + unorderedlist + "</span></div>";
            }

            if (totalHeadingCnt > 0)
            {
                if (errorCnt > 0)
                {
                    if (isDetailed)
                    {
                        headingTableHidden.Attributes.Remove("class");
                        message += "<div class='alert alert-info col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                            + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg messageIcon'></i>"
                            + "<span class='messageText'> De volgende headers zijn niet voorafgegaan door een groter, overkoepelend header-element:</span></div>";
                    }
                    else
                        message += "<div class='alert alert-info col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                            + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg messageIcon'></i>"
                            + "<span class='messageText'> Niet alle geteste pagina's gebruiken headers in een aflopende volgorde. Dit kan de gebruikerservaring negatief beïnvloeden doordat de indeling minder duidelijk kan zijn.</span></div>";

                    // Every misplaced heading causes a reduction in relation the the total amount of headers used
                    // E.g. 3 misplaced headings while there was a total of 20 headings used cause a reduction of 3/20th * 10
                    rating = rating - (((decimal)errorCnt / (decimal)totalHeadingCnt) * 10m);

                }
                else
                {
                    message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-ok glyphicons-lg messageIcon'></i>"
                        + "<span class='messageText'> Alle headers zijn correct ingedeeld. Dit is uitstekend aangezien dit de indeling van een pagina direct duidelijk kan maken voor bezoekers en zoekmachines kunnen de website zo beter indelen op inhoud.</span></div>";
                }
            }
            else
            {
                message = "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Geen enkele geteste pagina bevat headers. Dit is zeer slecht doordat de gebruiker niet snel een idee krijgt van de indeling van een pagina, maar dit is ook slecht voor de SEO. "
                    + "Zoekmachines gebruiken headers namelijk om o.a. in te schatten waar de pagina over gaat.</span></div>";
            }

            headingMessages.InnerHtml = message;

            if (rating == 10.0m)
                rating = 10m;

            if (rating > 0)
            {
                decimal rounded = decimal.Round(rating, 1);
                HeadingsRating.InnerHtml = rounded.ToString();

                var temp = (decimal)Session["RatingAccess"];
                Session["RatingAccess"] = temp + rounded;

                temp = (decimal)Session["RatingMarketing"];
                Session["RatingMarketing"] = temp + rounded;

                temp = (decimal)Session["RatingTech"];
                Session["RatingTech"] = temp + rounded;
                Session["HeadingsRating"] = rounded;
            }
            else
            {
                Session["HeadingsRating"] = 0.0m;
                HeadingsRating.InnerHtml = "0,0";
            }

            SetRatingDisplay(rating);
        }

        private void GetHeadingsOnUrl(string url)
        {
            Debug.WriteLine("Header check op -> " + url);

            var Webget = new HtmlWeb();
            var doc = Webget.Load(url);

            if (doc.DocumentNode.SelectNodes("//h1") == null
                && doc.DocumentNode.SelectNodes("//h2") == null
                && doc.DocumentNode.SelectNodes("//h3") == null
                && doc.DocumentNode.SelectNodes("//h4") == null
                && doc.DocumentNode.SelectNodes("//h5") == null
                && doc.DocumentNode.SelectNodes("//h6") == null)
            {
                noHeadings.Add(url);
                Debug.WriteLine(url + " bevat geen headers");
            }
            else
            {
                var h1list = new List<KeyValuePair<int, string>>();
                var h2list = new List<KeyValuePair<int, string>>();
                var h3list = new List<KeyValuePair<int, string>>();
                var h4list = new List<KeyValuePair<int, string>>();
                var h5list = new List<KeyValuePair<int, string>>();
                var h6list = new List<KeyValuePair<int, string>>();

                // Fill lists
                var templist = new List<List<KeyValuePair<int, string>>>() { h1list, h2list, h3list, h4list, h5list, h6list };
                var current = 1;

                foreach (var list in templist)
                {
                    if (doc.DocumentNode.SelectNodes("//h" + current) != null)
                    {
                        foreach (var item in doc.DocumentNode.SelectNodes("//h" + current))
                        {
                            list.Add(new KeyValuePair<int, string>(item.StreamPosition, item.InnerText));
                            totalHeadingCnt++;
                        }
                    }
                    current++;
                }

                List<List<KeyValuePair<int, string>>> listlist = new List<List<KeyValuePair<int, string>>>(templist);

                var listId = 2;
                for (int i = 0; i < listlist.Count; i++)
                {
                    var nextheader = "h" + listId;

                    if (listlist[i].Count == 0)
                    {
                        if (doc.DocumentNode.SelectNodes("//" + nextheader + "") != null)
                        {
                            foreach (var item in doc.DocumentNode.SelectNodes("//" + nextheader + ""))
                            {
                                AddToTable(item.InnerText, nextheader, url);
                            }
                        }
                    }
                    else
                    {
                        var next = i + 1;
                        if (next < listlist.Count)
                        {
                            if (listlist[next].Count > 0)
                            {
                                foreach (var nextlist in listlist[next])
                                {
                                    var found = false;
                                    for (int j = 0; j < listlist[i].Count; j++)
                                    {
                                        if (listlist[i][j].Key < nextlist.Key)
                                        {
                                            found = true;
                                            break;
                                        }
                                        else
                                            found = false;
                                    }
                                    if (!found)
                                    {
                                        AddToTable(nextlist.Value, nextheader, url);
                                    }
                                }
                            }
                        }
                    }

                    listId++;
                }
            }
        }

        /// <summary>
        /// Add the error/warning message to the table
        /// </summary>
        /// <param name="url">URL of origin of the message</param>
        /// <param name="type">Error or warning</param>
        /// <param name="line">Last Line</param>
        /// <param name="column">Last Column</param>
        /// <param name="msg">Additional error/warning message</param>
        private void AddToTable(string msg, string type, string url)
        {
            errorCnt++;

            var tRow = new TableRow();

            var tCellMsg = new TableCell();
            tCellMsg.Text = "<b>" + msg + "</b>";
            tRow.Cells.Add(tCellMsg);

            var tCellType = new TableCell();
            tCellType.Text = type;
            tRow.Cells.Add(tCellType);

            var tCellUrl = new TableCell();
            tCellUrl.Text = "<a href='" + url + "' target='_blank'>" + url + "</a>";
            tRow.Cells.Add(tCellUrl);

            table.Rows.Add(tRow);
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 6m)
                HeadingsRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                HeadingsRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                HeadingsRating.Attributes.Add("class", "excellentScore ratingCircle");
        }
    }
}
﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    // TODO: Commentaar toevoegen
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
            // Wanneer een pagina geen headers gebruikt wordt dit beoordeeld met 1/{aantal pagina's} * 10

            // Wanneer een pagina wel headers gebruikt, maar in de verkeerde volgorde wordt per pagina opgehaald hoeveel headers er gebruikt zijn
            // De beoordeling is afhankelijk van {het aantal verkeerd gebruikte headers}/{totaal aantal gebruikte headers} * 10


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
                    unorderedlist += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";
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
                            + "<span class='messageText'> De volgende headers zijn niet voorafgegaan door een groter, overkoepelend header-element. Dit kan de gebruikerservaring negatief beïnvloeden doordat de indeling minder duidelijk kan zijn.</span></div>";
                    }
                    else
                    {
                        headingTable.Rows.Clear();
                        message += "<div class='alert alert-info col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                            + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg messageIcon'></i>"
                            + "<span class='messageText'> Niet alle geteste pagina's gebruiken headers in een aflopende volgorde. Dit kan de gebruikerservaring negatief beïnvloeden doordat de indeling minder duidelijk kan zijn.</span></div>";
                    }
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

                var correctCnt = totalHeadingCnt - errorCnt;
                var correctPerccentage = decimal.Round((((decimal)correctCnt / (decimal)totalHeadingCnt) * 100m), 1);

                var pieGraph = GetPieGraphContent(correctPerccentage);
                message = "<div class='well well-lg resultWell text-center'>"
                    + "<div class='pieContainer'>" + pieGraph + "</div>"
                    + "<br/><span>van de headers is correct ingedeeld</span></div>"
                    + "<div class='resultDivider'></div>"
                    + "<div class='well well-lg resultWell text-center'>"
                    + "<i class='fa fa-header fa-3x'></i><i class='fa fa-header fa-2x'></i><i class='fa fa-header fa-1x'></i><br/><br/><br/>"
                    + "<span>" + totalHeadingCnt + " headers gevonden</span></div>"
                    + message;
            }
            else
            {
                message = "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12 text-center' role='alert'>"
                    + "<i class='fa fa-header fa-3x'></i><i class='fa fa-header fa-2x'></i><i class='fa fa-header fa-1x'></i><br/><br/><br/>"
                    + "<span class='messageText'>Geen enkele geteste pagina bevat headers. Dit is zeer slecht doordat de gebruiker niet snel een idee krijgt van de indeling van een pagina, maar dit is ook slecht voor de SEO. "
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

            headingTable.Rows.Add(tRow);
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
            var element = HeadingsRating;

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
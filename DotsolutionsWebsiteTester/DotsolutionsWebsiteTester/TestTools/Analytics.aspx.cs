﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class Analytics : System.Web.UI.Page
    {
        private List<Thread> threadList = new List<Thread>();
        // List for checking different types of analytics software
        // Currently only checking the most widely used one: google analytics
        private List<KeyValuePair<string, string>> analyticTypes = new List<KeyValuePair<string, string>>();

        private List<string> noAnalytics = new List<string>();
        private List<string> yesAnalytics = new List<string>();
        private int found;

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

            var ths = new ThreadStart(TestAnalytics);
            var th = new Thread(ths);
            th.Start();

            th.Join();

            var sb = new System.Text.StringBuilder();
            AnalyticsSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Analytics"] = htmlstring;
        }

        /// <summary>
        /// Check the page for a reference to some type of analytics
        /// Currently only checking for google-analytics
        /// </summary>
        private void TestAnalytics()
        {
            // Voor de beoordeling wordt de volgende formule gebruikt: 10 - ({aantal pagina's zonder analytics}/{aantal geteste pagina's} * 10)

            Debug.WriteLine(">>>> Analytics");
            var sitemap = (List<string>)Session["selectedSites"];
            var analyticslist = new List<KeyValuePair<string, string>>();
            var rating = 10.0m;
            var message = "";
            var isDetailed = (bool)Session["IsDetailedTest"];

            analyticTypes.Add(new KeyValuePair<string, string>("google-analytics.com", "Google Analytics"));
            analyticTypes.Add(new KeyValuePair<string, string>("yandex.ru/metrika", "Yandex Metrika"));
            //analyticTypes.Add(new KeyValuePair<string, string>("googleadservices.com", "Google Ad Services"));
            //analyticTypes.Add(new KeyValuePair<string, string>("placeholder-type", "placeholder-name"));

            // List for gathering
            var analytics = new List<string>();

            for (int i = 0; i < analyticTypes.Count; i++)
            {
                Debug.WriteLine("Op zoek naar " + analyticTypes[i].Key);
                found = 0;
                // Check every url in sitemap for analytics software for the current analytictype
                foreach (string url in sitemap)
                {
                    var ths = new ThreadStart(() => TestSite(i, url));
                    var th = new Thread(ths);
                    th.Start();

                    threadList.Add(th);
                }

                // Join Threads
                foreach (var thread in threadList)
                    thread.Join();

                string percentage = "0%";
                if (found > 0)
                {
                    double perc = (double)found / (double)sitemap.Count;
                    percentage = (perc * 100) + "%";

                    AddToTable(analyticTypes[i].Value, percentage);
                    analyticslist.Add(new KeyValuePair<string, string>(analyticTypes[i].Value, percentage));
                }
            }

            // Nothing found vs some things found
            if (analyticslist.Count == 0)
            {
                message = "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Geen analytics software gevonden. Dit is slecht doordat analytics software er voor zorgt dat het gedrag van een bezoeker is te analyseren.</span></div>";
            }
            else if (yesAnalytics.Count == sitemap.Count)
            {
                message = "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Op alle pagina's is een soort analytics software gevonden. Dit is uitstekend, analytics software zorgt er namelijk voor dat het gedrag van een bezoeker is te analyseren.</span></div>";
            }
            else
            {
                var analyticsGrammar = analyticslist.Count + " soort";
                if (analyticslist.Count > 1)
                    analyticsGrammar = analyticslist.Count + " soorten";
                var sitemapGrammar = sitemap.Count + " pagina's";
                if (sitemap.Count == 1)
                    sitemapGrammar = sitemap.Count + " pagina";
                if (isDetailed)
                {
                    message = "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-ok glyphicons-lg messageIcon'></i>"
                        + "<span class='messageText'> " + analyticsGrammar + " analytics software gevonden op " + yesAnalytics.Count + " van de " + sitemapGrammar + "</span></div>";
                }
                else
                {
                    message = "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-ok glyphicons-lg messageIcon'></i>"
                        + "<span class='messageText'> Op " + yesAnalytics.Count + " van de " + sitemapGrammar + " is een soort analytics software gevonden. Dit kan beter door op alle pagina's gebruik te maken van analytics software. "
                        + "Analytics software zorgt er namelijk voor dat het gedrag van een bezoeker is te analyseren.</span></div>";
                }
            }

            // Did not find analytics software on every page
            if (analyticslist.Count > 0 && noAnalytics.Count > 0)
            {
                string nothing = "";

                foreach (var item in noAnalytics)
                    nothing += "<li><a href='" + item + "' target='blank'>" + item + "</a></li>";

                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Geen analytics software gevonden op volgende pagina's"
                    + "<ul>" + nothing + "</ul></span></div>";

            }

            var percentageUsed = (yesAnalytics.Count / sitemap.Count) * 100;
            var amountFound = analyticslist.Count + " soorten";
            if (analyticslist.Count == 1)
                amountFound = analyticslist.Count + " soort";

            var pieGraph = GetPieGraphContent(percentageUsed);

            message = "<div class='well well-lg resultWell text-center'>"
                + "<div class='pieContainer'>" + pieGraph + "</div>"
                + "<br/><span>van de pagina's gebruikt analytics</span></div>"
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg resultWell text-center'>"
                + "<i class='fa fa-search fa-3x'></i><br/><br/><br/>"
                + "<span>" + amountFound + " analytics gevonden</span></div>"
                + message;

            AnalyticsResults.InnerHtml = message;

            if (isDetailed && analyticslist.Count > 0)
                AnalyticsTableHidden.Attributes.Remove("class");

            if (!isDetailed)
                AnalyticsTable.Rows.Clear();

            if (analyticslist.Count > 0)
            {
                if (noAnalytics.Count > 0)
                    rating = rating - (((decimal)noAnalytics.Count / (decimal)sitemap.Count) * 10m);
                else
                    rating = 10m;
            }
            else
                rating = 0.0m;

            if (rating == 10.0m)
                rating = 10m;
            decimal rounded = decimal.Round(rating, 1);
            AnalyticsRating.InnerHtml = rounded.ToString();

            var RatingMarketing = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = rounded + RatingMarketing;
            Session["AnalyticsRating"] = rounded;
            SetRatingDisplay(rating);
        }

        /// <summary>
        /// Test a single page on a specific type of analytics tool
        /// </summary>
        /// <param name="index">Index of analytics tool in analyticTypes</param>
        /// <param name="url">URL to be tested</param>
        private void TestSite(int index, string url)
        {
            try
            {
                var Webget = new HtmlWeb();
                var doc = Webget.Load(url);
                bool done = false;

                if (doc.DocumentNode.SelectNodes("//script") != null)
                {
                    Debug.WriteLine("Scipts check for Analytics " + url);
                    foreach (var node in doc.DocumentNode.SelectNodes("//script"))
                    {
                        // if type from list is detected do this
                        if (node.InnerHtml.Contains(analyticTypes[index].Key))
                        {
                            Debug.WriteLine("Scipts check for Analytics gevonden:" + analyticTypes[index].Value);
                            found++;
                            done = true;
                            // Add to list that tested positive to some kind of analytics software if it's not already in there
                            if (!yesAnalytics.Contains(url))
                            {
                                yesAnalytics.Add(url);
                            }

                            // remove url from noAnalytics list since current analytics search found something
                            if (noAnalytics.Contains(url))
                            {
                                noAnalytics.Remove(url);
                            }
                            // Break in case they added the same type in multiple forms, e.g. //www.google-analytics.com/analytics.js and (the older version: ) https://ssl.google-analytics.com/ga.js
                            break;
                        }
                    }
                }

                if (!done && doc.DocumentNode.SelectSingleNode("//html") != null)
                {
                    var node = doc.DocumentNode.SelectSingleNode("//html");
                    Debug.WriteLine("HTML check for Analytics " + url);

                    // if type from list is detected do this
                    if (node.InnerHtml.Contains(analyticTypes[index].Key))
                    {
                        Debug.WriteLine("HTML check for Analytics gevonden:" + analyticTypes[index].Value);
                        found++;
                        // Add to list that tested positive to some kind of analytics software if it's not already in there
                        if (!yesAnalytics.Contains(url))
                        {
                            yesAnalytics.Add(url);
                        }

                        // remove url from noAnalytics list since current analytics search found something
                        if (noAnalytics.Contains(url))
                        {
                            noAnalytics.Remove(url);
                        }
                    }
                }
                // Add to list of noAnalytics found if it's not already in there and has not tested postive to another kind yet
                if (!yesAnalytics.Contains(url) && !noAnalytics.Contains(url))
                {
                    noAnalytics.Add(url);
                }
            }
            catch (WebException)
            {

            }
        }

        /// <summary>
        /// Add the found analytics software to the table
        /// </summary>
        /// <param name="type">Name of the type of analytics</param>
        /// <param name="percentage">Percentage of the amount found across all pages checked</param>
        private void AddToTable(string type, string percentage)
        {
            var tRow = new TableRow();

            var tCellType = new TableCell();
            tCellType.Text = type;
            tRow.Cells.Add(tCellType);

            var tCellPerc = new TableCell();
            tCellPerc.Text = percentage;
            tRow.Cells.Add(tCellPerc);

            AnalyticsTable.Rows.Add(tRow);
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
            var element = AnalyticsRating;

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
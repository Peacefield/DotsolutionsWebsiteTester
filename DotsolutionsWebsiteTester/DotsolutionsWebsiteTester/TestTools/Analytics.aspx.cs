using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            var sb = new System.Text.StringBuilder();
            AnalyticsSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Analytics"] = htmlstring;
            return;


            var ths = new ThreadStart(TestAnalytics);
            var th = new Thread(ths);
            th.Start();

            th.Join();

            //var sb = new System.Text.StringBuilder();
            //AnalyticsSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            //string htmlstring = sb.ToString();

            //Session["Analytics"] = htmlstring;
        }

        /// <summary>
        /// Check the page for a reference to some type of analytics
        /// Currently only checking for google-analytics
        /// </summary>
        private void TestAnalytics()
        {            
            Debug.WriteLine(">>>> Analytics");
            var sitemap = (List<string>)Session["selectedSites"];
            var analyticslist = new List<KeyValuePair<string, string>>();

            analyticTypes.Add(new KeyValuePair<string, string>("google-analytics.com", "Google Analytics"));
            analyticTypes.Add(new KeyValuePair<string, string>("googleadservices.com", "Google Ad Services"));
            //analyticTypes.Add(new KeyValuePair<string, string>("placeholder-type", "placeholder-name"));

            // List for gathering
            var analytics = new List<string>();

            for (int i = 0; i < analyticTypes.Count; i++)
            {
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
                    AnalyticsTableHidden.Attributes.Remove("class");
                }
            }

            if (analyticslist.Count == 0)
                AnalyticsResults.InnerHtml = "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg'></i>"
                    + "<span> Geen analytics software gevonden.</span></div>";
            else if (analyticslist.Count == 1)
                AnalyticsResults.InnerHtml = "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span> " + analyticslist.Count + " soort analytics software gevonden op " + yesAnalytics.Count + " van de " + sitemap.Count + " pagina's</span></div>";
            else
                AnalyticsResults.InnerHtml = "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span> " + analyticslist.Count + " soorten analytics software gevonden op " + yesAnalytics.Count + " van de " + sitemap.Count + " pagina's</span></div>";

            if (analyticslist.Count > 0 && noAnalytics.Count > 0)
            {
                string nothing = "";

                foreach (var item in noAnalytics)
                    nothing += "<li><a href='" + item + "' target='blank'>" + item + "</a></li>";

                AnalyticsResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg'></i>"
                    + "<span> Geen analytics software gevonden op volgende pagina's</span>"
                    + "<ul>" + nothing + "</ul></div>";
            }
        }

        /// <summary>
        /// Test a single page on a specific type of analytics tool
        /// </summary>
        /// <param name="index">Index of analytics tool in analyticTypes</param>
        /// <param name="url">URL to be tested</param>
        private void TestSite(int index, string url)
        {
            var Webget = new HtmlWeb();
            var doc = Webget.Load(url);
            bool done = false;

            if (doc.DocumentNode.SelectNodes("//script") != null)
            {
                Debug.WriteLine(doc.DocumentNode.SelectSingleNode("//script").InnerHtml);
                foreach (var node in doc.DocumentNode.SelectNodes("//script"))
                {
                    // if type from list is detected do this
                    if (node.InnerHtml.Contains(analyticTypes[index].Key))
                    {
                        found++;
                        done = true;
                        Debug.WriteLine("Done naar true <<<<<<<<<<<<<<<<< !!!!!!!!!!!!!!!!!!!!! ");
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
                        // Break incase they added the same type in multiple forms, e.g. //www.google-analytics.com/analytics.js and https://ssl.google-analytics.com/ga.js
                        break;
                    }
                }
            }

            if (!done && doc.DocumentNode.SelectSingleNode("//html") != null)
            {
                var node = doc.DocumentNode.SelectSingleNode("//html");
                Debug.WriteLine("HTML <<<<<<<<<<<<<<<<< !!!!!!!!!!!!!!!!!!!!! ");
                Debug.WriteLine(node.InnerHtml);

                // if type from list is detected do this
                if (node.InnerHtml.Contains(analyticTypes[index].Key))
                {
                    Debug.WriteLine("Gevonden <<<<<<<<<<<<<<<<< !!!!!!!!!!!!!!!!!!!!! ");
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
    }
}
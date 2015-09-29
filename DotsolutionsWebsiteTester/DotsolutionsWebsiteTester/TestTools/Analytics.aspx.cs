using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class Analytics : System.Web.UI.Page
    {
        private List<System.Threading.Thread> ThreadList = new List<System.Threading.Thread>();
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
                Response.Redirect("~/Default.aspx");
                return;
            }

            System.Threading.ThreadStart ths = new System.Threading.ThreadStart(TestAnalytics);
            System.Threading.Thread th = new System.Threading.Thread(ths);
            th.Start();

            th.Join();
        }

        /// <summary>
        /// Check the page for a reference to some type of analytics
        /// Currently only checking for google-analytics
        /// </summary>
        private void TestAnalytics()
        {
            System.Diagnostics.Debug.WriteLine(">>>> Analytics");
            List<string> sitemap = (List<string>)Session["selectedSites"];
            // List for session
            List<KeyValuePair<string, string>> analyticslist = new List<KeyValuePair<string, string>>();


            analyticTypes.Add(new KeyValuePair<string, string>("google-analytics", "Google Analytics"));
            analyticTypes.Add(new KeyValuePair<string, string>("wp-statistics", "WordPress stats plugin")); // Not sure if this one works
            //analyticTypes.Add(new KeyValuePair<string, string>("placeholder-type", "placeholder-name"));

            // List for gathering
            List<string> analytics = new List<string>();

            for (int i = 0; i < analyticTypes.Count; i++)
            {
                found = 0;
                // Check every url in sitemap for analytics software for the current analytictype
                foreach (string url in sitemap)
                {
                    System.Threading.ThreadStart ths = new System.Threading.ThreadStart(() => TestSite(i, url));
                    System.Threading.Thread th = new System.Threading.Thread(ths);
                    th.Start();

                    ThreadList.Add(th);
                    System.Threading.Thread.Sleep(10);
                }

                // Join Threads
                foreach (System.Threading.Thread thread in ThreadList)
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
                    + "<span>Geen analytics software gevonden.</span></div>";
            else
                AnalyticsResults.InnerHtml = "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span>" + analyticslist.Count + " soort(en) analytics software gevonden op " + yesAnalytics.Count + " van de " + sitemap.Count + " pagina's</span></div>";

            if (analyticslist.Count > 0 && noAnalytics.Count > 0)
            {
                string nothing = "";

                foreach (var item in noAnalytics)
                    nothing += "<li><a href='" + item + "' target='blank'>" + item + "</li>";

                AnalyticsResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg'></i>"
                    + "<span>Geen analytics software gevonden op volgende pagina's</span>"
                    + "<ul>" + nothing + "</ul></div>";
            }

            Session["noAnalytics"] = noAnalytics;
            Session["analyticslist"] = analyticslist;
        }

        private void TestSite(int index, string url)
        {

            var Webget = new HtmlWeb();
            var doc = Webget.Load(url);

            if (doc.DocumentNode.SelectNodes("//script") != null)
            {
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//script"))
                {
                    // if type from list is detected do this
                    if (node.InnerHtml.Contains(analyticTypes[index].Key))
                    {
                        System.Diagnostics.Debug.WriteLine(">>>>>>>>>>>>> Gevonden >>>>>>>>>>>>> " + analyticTypes[index].Value);
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
                        // Break incase they added the same type in multiple forms, e.g. //www.google-analytics.com/analytics.js and https://ssl.google-analytics.com/ga.js
                        break;
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
            TableRow tRow = new TableRow();

            TableCell tCellUrl = new TableCell();
            tCellUrl.Text = type;
            tCellUrl.CssClass = "col-md-10";
            tRow.Cells.Add(tCellUrl);

            TableCell tCellType = new TableCell();
            tCellType.Text = percentage;
            tCellType.CssClass = "col-md-2";
            tRow.Cells.Add(tCellType);

            AnalyticsTable.Rows.Add(tRow);
        }
    }
}
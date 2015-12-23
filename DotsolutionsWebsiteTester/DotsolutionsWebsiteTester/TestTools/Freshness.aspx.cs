using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class Freshness : System.Web.UI.Page
    {
        //List<DateTime> dateList = new List<DateTime>();
        ConcurrentBag<DateTime> threadSafe = new ConcurrentBag<DateTime>();
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

            Debug.WriteLine("Page_Load");

            GetFreshness();

            //var ths = new ThreadStart(GetFreshness);
            //var th = new Thread(ths);
            //th.Start();
            //Debug.WriteLine("Na starten mainthread");
            //Thread.Sleep(10);
            //th.Join();
            //Debug.WriteLine("Na joinen mainthread");

            var sb = new System.Text.StringBuilder();
            FreshnessSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Freshness"] = htmlstring;
        }

        private void GetFreshness()
        {
            Debug.WriteLine("GetFreshness");

            var message = "";
            var sitemap = (List<string>)Session["selectedSites"];
            var latestDate = new DateTime();
            var rating = 10.0m;
            var isDetailed = (bool)Session["IsDetailedTest"];
            foreach (var page in sitemap)
            {
                Debug.WriteLine(" -------------------------------------------------------------- Werken aan ------------------------------- " + page + " -------------------------------------------------------------- ");
                var newDate = GetLatestDate(page);
                if (newDate > latestDate)
                    latestDate = newDate;
            }

            var culture = new System.Globalization.CultureInfo("nl-NL"); // Displays (D)D-(M)M-YYYY
            message += "De content van de website is voor het laatst bijgewerkt op " + latestDate.ToString("d", culture) + ".<br/>";

            var todayDate = DateTime.Today;
            var oneMonthAgo = todayDate.AddMonths(-1);
            var twoMonthsAgo = todayDate.AddMonths(-2);
            var threeMonthsAgo = todayDate.AddMonths(-3);
            var fourMonthsAgo = todayDate.AddMonths(-4);

            if (latestDate < oneMonthAgo)
            {
                if (latestDate > twoMonthsAgo)
                {
                    rating = 7.5m;
                    message += "Dit is redelijk goed. Het is beter om de website maandelijks bij te werken.";
                }
                else if (latestDate > threeMonthsAgo)
                {
                    rating = 5.5m;
                    message += "Dit is redelijk slecht. Het is beter om de website maandelijks bij te werken.";
                }
                else if (latestDate > fourMonthsAgo)
                {
                    rating = 3.0m;
                    message += "Dit is slecht. Het is beter om de website maandelijks bij te werken.";
                }
                else
                {
                    rating = 0.0m;
                    message += "Dit is zeer slecht. Het is beter om de website maandelijks bij te werken.";
                }
            }
            else
            {
                message += "Dit is uitstekend. Het is goed om de website maandelijks bij te werken.";
            }

            FreshnessResults.InnerHtml = message;

            //if (isDetailed && FreshnessTable.Rows.Count > 0)
            FreshnessTableHidden.Attributes.Remove("class");
            //else
            //    FreshnessTable.Rows.Clear();

            if (rating == 10.0m)
                rating = 10m;
            if (rating < 0m)
                rating = 0.0m;

            decimal rounded = decimal.Round(rating, 1);
            FreshnessRating.InnerHtml = rounded.ToString();
            SetRatingDisplay(rounded);

            var temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = rounded + temp;

            temp = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = rounded + temp;

            Session["FreshnessRating"] = rounded;

        }

        private DateTime GetLatestDate(string page)
        {
            Debug.WriteLine("GetLatestDate");
            var latestDate = new DateTime();
            var date = GetDateByLastModified(page);
            if (date != new DateTime())
            {
                latestDate = date;
            }
            else
            {
                date = GetDateFromAdditionalContent(page);
                if (date != new DateTime())
                {
                    latestDate = date;
                }
            }
            return latestDate;
        }

        /// <summary>
        /// Get DateTime from additional content: images, javascript and css.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private DateTime GetDateFromAdditionalContent(string site)
        {
            Debug.WriteLine("GetDateFromAdditionalContent");
            var dateList = new List<DateTime>();
            var contentList = GetContentList(site);
            //var threadpool = new List<Thread>();
            //dateList.Clear();
            foreach (var item in contentList)
            {
                // Start thread voor controleren datum per item

                //var ths = new ThreadStart(() => AddToDateListFromThread(item, site));
                //var th = new Thread(ths);
                //th.Start();
                ////Thread.Sleep(1);
                //threadpool.Add(th);

                //Thread.Sleep(5);

                var lastModifiedDate = GetDateByLastModified(item);
                if (lastModifiedDate != new DateTime())
                {
                    dateList.Add(lastModifiedDate);
                    // Tabel vullen voor weergeven gevonden data
                    //message += lastModifiedDate.ToShortDateString() + " -- <a href='" + item + "' target='_blank'>" + item + "</a><br/>";
                    //Debug.WriteLine("AddToDateListFromThread__pageOfOrigin: " + pageOfOrigin + " -- lastModifiedDate: " + lastModifiedDate.ToShortDateString() + " -- contentUrl: " + contentUrl);

                    AddToTable(lastModifiedDate.ToShortDateString(), item, site);
                }
            }

            //foreach (var thread in threadpool)
            //    thread.Join();

            DateTime latestDate = new DateTime();
            foreach (var item in dateList)
            {
                Debug.WriteLine("item in dateList: " + item);
                // Vergelijk om meest recente datum te vinden
                if (latestDate < item)
                    latestDate = item;
            }

            return latestDate;
        }

        //private void AddToDateListFromThread(string contentUrl, string pageOfOrigin)
        //{
        //    Debug.WriteLine("AddToDateListFromThread");
        //    var lastModifiedDate = GetDateByLastModified(contentUrl);
        //    if (lastModifiedDate != new DateTime())
        //    {
        //        dateList.Add(lastModifiedDate);
        //        // Tabel vullen voor weergeven gevonden data
        //        //message += lastModifiedDate.ToShortDateString() + " -- <a href='" + item + "' target='_blank'>" + item + "</a><br/>";
        //        //Debug.WriteLine("AddToDateListFromThread__pageOfOrigin: " + pageOfOrigin + " -- lastModifiedDate: " + lastModifiedDate.ToShortDateString() + " -- contentUrl: " + contentUrl);

        //        AddToTable(lastModifiedDate.ToShortDateString(), contentUrl, pageOfOrigin);
        //    }
        //}

        private List<string> GetContentList(string site)
        {
            Debug.WriteLine("GetContentList");
            var contentList = new List<string>();
            var webget = new HtmlWeb();
            var doc = webget.Load(site);

            if (doc.DocumentNode.SelectNodes("//img") != null)
            {
                foreach (var item in doc.DocumentNode.SelectNodes("//img"))
                {
                    if (item.Attributes["src"] != null)
                    {
                        var url = CreateUrl(item.Attributes["src"].Value);
                        if (url.Length > 0)
                            contentList.Add(url);
                    }
                }
            }

            if (doc.DocumentNode.SelectNodes("//script") != null)
            {
                foreach (var item in doc.DocumentNode.SelectNodes("//script"))
                {
                    if (item.Attributes["src"] != null)
                    {
                        var url = CreateUrl(item.Attributes["src"].Value);
                        if (url.Length > 0)
                            contentList.Add(url);
                    }
                }
            }

            if (doc.DocumentNode.SelectNodes("//link") != null)
            {
                foreach (var item in doc.DocumentNode.SelectNodes("//link"))
                {
                    if (item.Attributes["href"] != null)
                    {
                        var url = CreateUrl(item.Attributes["href"].Value);
                        if (url.Length > 0)
                            contentList.Add(url);
                    }
                }
            }
            return contentList;
        }

        private string CreateUrl(string foundUrl)
        {
            Debug.WriteLine("CreateUrl");
            var testLink = "";
            var mainUrl = Session["MainUrl"].ToString();

            if (foundUrl.StartsWith(" "))
                foundUrl = foundUrl.Remove(0, 1);

            if (foundUrl.Contains("http:/") || foundUrl.Contains("https:/"))
            {
                testLink = foundUrl;
            }
            else if (foundUrl.Contains("//www."))
            {
                testLink = "http:" + foundUrl;
            }
            else if (foundUrl.Contains("www."))
            {
                testLink = "http://" + foundUrl;
            }
            else if (foundUrl.StartsWith("//"))
            {
                testLink = "http:" + foundUrl;
            }
            else
            {
                if (mainUrl.EndsWith("/") && foundUrl.StartsWith("/"))
                    testLink = mainUrl.Remove(mainUrl.Length - 1) + foundUrl;
                else if ((mainUrl.EndsWith("/") && !foundUrl.StartsWith("/")) || (!mainUrl.EndsWith("/") && foundUrl.StartsWith("/")))
                    testLink = mainUrl + foundUrl;
                else if (!mainUrl.EndsWith("/") && !foundUrl.StartsWith("/"))
                    testLink = mainUrl + "/" + foundUrl;
            }

            return testLink;
        }

        /// <summary>
        /// Get the date specified in the Last-Modified response header.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Date of new DateTime when Last-Modified header is not specified. Otherwise it return the date specified.</returns>
        private DateTime GetDateByLastModified(string url)
        {
            Debug.WriteLine("GetDateByLastModified");
            DateTime date = new DateTime();
            var dateString = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.Headers["Last-Modified"] != null)
                {
                    dateString = response.Headers["Last-Modified"].ToString();
                    dateString = dateString.Remove(0, dateString.IndexOf(',') + 2);
                    string[] array = dateString.Split(' ');
                    var month = MonthToInt(array[1]);
                    DateTime thisDate = new DateTime(Int32.Parse(array[2]), month, Int32.Parse(array[0]));
                    return thisDate;
                }
            }
            catch (WebException we)
            {
                Debug.WriteLine(url + " veroorzaakt een WebException: " + we.Message);
            }
            catch (UriFormatException ufe)
            {
                Debug.WriteLine(url + " veroorzaakt een UriFormatException: " + ufe.Message);
            }

            return date;
        }

        private int MonthToInt(string month)
        {
            switch (month)
            {
                case "Jan":
                    return 1;
                case "Feb":
                    return 2;
                case "Mar":
                    return 3;
                case "Apr":
                    return 4;
                case "May":
                    return 5;
                case "Jun":
                    return 6;
                case "Jul":
                    return 7;
                case "Aug":
                    return 8;
                case "Sep":
                    return 9;
                case "Oct":
                    return 10;
                case "Nov":
                    return 11;
                case "Dec":
                    return 12;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 6m)
                FreshnessRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                FreshnessRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                FreshnessRating.Attributes.Add("class", "excellentScore ratingCircle");
        }

        /// <summary>
        /// Add the found dates to table
        /// </summary>
        /// <param name="date">date found</param>
        /// <param name="contentUrl">URL of tested content (img, js, css)</param>
        /// <param name="pageOfOrigin">URL of page where content was found</param>
        private void AddToTable(string date, string contentUrl, string pageOfOrigin)
        {
            var tRow = new TableRow();

            var tCellDate = new TableCell();
            tCellDate.Text = date;
            tRow.Cells.Add(tCellDate);

            var tCellContentUrl = new TableCell();
            tCellContentUrl.Text = "<a href='" + contentUrl + "' target='_blank'>" + contentUrl + "</a>";
            tRow.Cells.Add(tCellContentUrl);

            var tCellUrlOrigin = new TableCell();
            tCellUrlOrigin.Text = "<a href='" + pageOfOrigin + "' target='_blank'>" + pageOfOrigin + "</a>";
            tRow.Cells.Add(tCellUrlOrigin);

            FreshnessTable.Rows.Add(tRow);
        }
    }
}
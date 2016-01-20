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
        //ConcurrentBag<DateTime> threadSafe = new ConcurrentBag<DateTime>();
        List<DateTime> threadSafe = new List<DateTime>();
        List<KeyValuePair<string, List<KeyValuePair<string, DateTime>>>> DateListContainer = new List<KeyValuePair<string, List<KeyValuePair<string, DateTime>>>>();
        //ConcurrentBag<string> contentCheckedContainer = new ConcurrentBag<string>();
        List<string> contentCheckedContainer = new List<string>();

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

            GetFreshness();

            var sb = new System.Text.StringBuilder();
            FreshnessSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Freshness"] = htmlstring;
        }

        private void GetFreshness()
        {
            var message = "";
            var sitemap = (List<string>)Session["selectedSites"];
            var latestDate = new DateTime();
            var rating = 10.0m;
            var isDetailed = (bool)Session["IsDetailedTest"];

            var threadpool = new List<Thread>();

            foreach (var page in sitemap)
            {
                Debug.WriteLine(" --------------------------------------------------------- Start werken aan ------------------------------- " + page + " --------------------------------------------------------- ");

                //var ths = new ThreadStart(() => GetLatestDate(page));
                //var th = new Thread(ths);
                //th.Start();
                //threadpool.Add(th);

                GetLatestDate(page);
            }

            //foreach (var thread in threadpool)
            //    thread.Join();

            if (isDetailed)
            {
                var i = 0;
                foreach (var overheadlist in DateListContainer)
                {
                    foreach (var list in overheadlist.Value)
                        AddToTable(list.Value.ToShortDateString(), list.Key, sitemap[i]);
                    i++;
                }
            }
            
            foreach (var newDate in threadSafe)
            {
                if (newDate > latestDate)
                    latestDate = newDate;
            }

            var culture = new System.Globalization.CultureInfo("nl-NL"); // Displays (D)D-(M)M-YYYY

            //message += "<div class='text-center'><span><i class='fa fa-clock-o fa-3x'></i></span><span class='fa-2x'> " + latestDate.ToString("d", culture) + "</span></div>";


            if (latestDate == new DateTime())
                message += "<span>Er konden geen data worden gevonden op de geteste pagina's.</span><br/>";
            else if (FreshnessTable.Rows.Count > 1)
            {
                var total = FreshnessTable.Rows.Count - 1;
                message += "<span>Na het doorlopen van " + total
                    + " bestanden is er gedetecteerd dat de content van de website voor het laatst is bijgewerkt op <time class='emphasis'>" + latestDate.ToString("d", culture) + "</time>.</span><br/>";
            }
            else
                message += "<span>De content van de website is voor het laatst bijgewerkt op <time class='emphasis'>" + latestDate.ToString("d", culture) + "</time>.</span><br/>";

            var todayDate = DateTime.Today;
            var oneMonthAgo = todayDate.AddMonths(-1);
            var twoMonthsAgo = todayDate.AddMonths(-2);
            var threeMonthsAgo = todayDate.AddMonths(-3);
            var fourMonthsAgo = todayDate.AddMonths(-4);

            message += "<span>";
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
            message += "</span>";


            if (rating > 5.5m)
                message = "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>" + message + "</div>";
            else
                message = "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>" + message + "</div>";


            FreshnessResults.InnerHtml = message;
            if (isDetailed && FreshnessTable.Rows.Count > 1)
                FreshnessTableHidden.Attributes.Remove("class");
            else
                FreshnessTable.Rows.Clear();

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

        /// <summary>
        /// Initiate looking for latest date on a page
        /// </summary>
        /// <param name="page">string page to be tested</param>
        private void GetLatestDate(string page)
        {
            //Debug.WriteLine("GetLatestDate");
            var latestDate = new DateTime();
            var date = GetDateByLastModified(page);
            var isDetailed = (bool)Session["IsDetailedTest"];

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
            threadSafe.Add(latestDate);
        }

        /// <summary>
        /// Get DateTime from additional content: images, javascript and css.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private DateTime GetDateFromAdditionalContent(string site)
        {
            //Debug.WriteLine("GetDateFromAdditionalContent");
            var dateList = new List<DateTime>();
            var contentList = GetContentList(site);
            var isDetailed = (bool)Session["IsDetailedTest"];
            var list = new List<KeyValuePair<string, DateTime>>();
            foreach (var item in contentList)
            {
                var lastModifiedDate = GetDateByLastModified(item);
                if (lastModifiedDate != new DateTime())
                {
                    dateList.Add(lastModifiedDate);

                    // Add to key value pair; key: contentUrl - value: date

                    if (isDetailed)
                        list.Add(new KeyValuePair<string, DateTime>(item, lastModifiedDate));
                }
            }

            // add sorted key value pair as value to key value pair with key: page name
            // used to fill table
            if (isDetailed)
            {
                // sort key value pair on value(date)
                // http://www.dotnetperls.com/sort-keyvaluepair
                list.Sort(Compare);

                Debug.WriteLine(" --- list.Sort --- ");

                DateListContainer.Add(new KeyValuePair<string, List<KeyValuePair<string, DateTime>>>(site, list));
            }

            DateTime latestDate = new DateTime();
            foreach (var item in dateList)
            {
                //Debug.WriteLine("item in dateList: " + item);
                // Compare to find most recent date
                if (latestDate < item)
                    latestDate = item;
            }

            return latestDate;
        }

        /// <summary>
        /// Compare delegate to be used to sort a list descending
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static int Compare(KeyValuePair<string, DateTime> a, KeyValuePair<string, DateTime> b)
        {
            return a.Value.CompareTo(b.Value);
        }

        /// <summary>
        /// Get a List<string> of content on a page
        /// </summary>
        /// <param name="site">string site to be crawled</param>
        /// <returns>String List containing src/href to found content</returns>
        private List<string> GetContentList(string site)
        {
            var contentList = new List<string>();
            var webget = new HtmlWeb();
            var doc = webget.Load(site);

            if (doc.DocumentNode.SelectNodes("//img") != null)
            {
                foreach (var item in doc.DocumentNode.SelectNodes("//img"))
                {
                    if (item.Attributes["src"] != null)
                    {
                        if (!contentCheckedContainer.Contains(item.Attributes["src"].Value))
                        {
                            contentCheckedContainer.Add(item.Attributes["src"].Value);

                            if ((!item.Attributes["src"].Value.StartsWith("http") && !item.Attributes["src"].Value.StartsWith("//")) || IsOfDomain(site, item.Attributes["src"].Value))
                            {
                                var url = CreateUrl(item.Attributes["src"].Value);
                                if (url.Length > 0)
                                {
                                    contentList.Add(url);
                                }
                            }
                        }
                    }
                }
            }

            if (doc.DocumentNode.SelectNodes("//script") != null)
            {
                foreach (var item in doc.DocumentNode.SelectNodes("//script"))
                {
                    if (item.Attributes["src"] != null)
                    {
                        if (!contentCheckedContainer.Contains(item.Attributes["src"].Value))
                        {
                            contentCheckedContainer.Add(item.Attributes["src"].Value);

                            if ((!item.Attributes["src"].Value.StartsWith("http") && !item.Attributes["src"].Value.StartsWith("//")) || IsOfDomain(site, item.Attributes["src"].Value))
                            {
                                var url = CreateUrl(item.Attributes["src"].Value);
                                if (url.Length > 0)
                                    contentList.Add(url);
                            }
                        }
                    }
                }
            }

            if (doc.DocumentNode.SelectNodes("//link") != null)
            {
                foreach (var item in doc.DocumentNode.SelectNodes("//link"))
                {
                    if (item.Attributes["href"] != null)
                    {
                        if (!contentCheckedContainer.Contains(item.Attributes["href"].Value))
                        {
                            contentCheckedContainer.Add(item.Attributes["href"].Value);

                            if ((!item.Attributes["href"].Value.StartsWith("http") && !item.Attributes["href"].Value.StartsWith("//")) || IsOfDomain(site, item.Attributes["href"].Value))
                            {
                                var url = CreateUrl(item.Attributes["href"].Value);
                                if (url.Length > 0)
                                    contentList.Add(url);
                            }
                        }
                    }
                }
            }
            return contentList;
        }

        /// <summary>
        /// Check if a found URL is part of the user-entered url
        /// </summary>
        /// <param name="url">User-entered URL</param>
        /// <param name="addition">Found URL</param>
        /// <returns></returns>
        private bool IsOfDomain(string url, string addition)
        {
            if (addition.Contains(url))
            {
                return true;
            }
            try
            {
                var foundUrl = CreateUrl(addition);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(foundUrl);
                request.UserAgent = Session["userAgent"].ToString();
                request.Timeout = 1000;
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var uri = new Uri(url);
                IPAddress[] addresslistMain = Dns.GetHostAddresses(uri.Host);
                IPAddress[] addresslist = Dns.GetHostAddresses(response.ResponseUri.Host.ToString());

                //IP check
                foreach (IPAddress theaddress in addresslist)
                {
                    if (addresslistMain.Contains(theaddress))
                    {
                        return true;
                    }
                }
            }
            catch (WebException we)
            {
                Debug.WriteLine(addition + " heeft een fout veroorzaakt.");
                Debug.WriteLine("IsOfDomain Catch: " + we.Message);
            }

            return false;
        }

        /// <summary>
        /// Create a valid URL that can be used for a HttpWebRequest
        /// </summary>
        /// <param name="foundUrl">To be converted URL</param>
        /// <returns>Converted URL</returns>
        private string CreateUrl(string foundUrl)
        {
            //Debug.WriteLine("CreateUrl");
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
            //Debug.WriteLine("GetDateByLastModified");
            DateTime date = new DateTime();
            var dateString = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                request.Timeout = 1000;
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
            var element = FreshnessRating;

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
            tCellContentUrl.CssClass = "word-break";
            tCellContentUrl.Text = "<a href='" + contentUrl + "' target='_blank'>" + contentUrl + "</a>";
            tRow.Cells.Add(tCellContentUrl);

            //var tCellUrlOrigin = new TableCell();
            //tCellUrlOrigin.Text = "<a href='" + pageOfOrigin + "' target='_blank'>" + pageOfOrigin + "</a>";
            //tRow.Cells.Add(tCellUrlOrigin);

            FreshnessTable.Rows.Add(tRow);
        }
    }
}
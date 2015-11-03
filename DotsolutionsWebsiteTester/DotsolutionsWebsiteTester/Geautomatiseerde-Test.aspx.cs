using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace DotsolutionsWebsiteTester
{
    public partial class ProcessTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
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

                UrlTesting.InnerText = Session["MainUrl"].ToString();

                // Set user agent
                string userAgent = "Mozilla/5.0 (DotTestBot, http://www.example.net)";
                //string userAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";
                Session["userAgent"] = userAgent;
                Session["robotsTxt"] = GetRobotsTxt(UrlTesting.InnerText);
                Session["ApiKeys"] = GetApiKeys();

                var ths = new ThreadStart(GetTestList);
                var th = new Thread(ths);
                th.Start();

                var ths2 = new ThreadStart(GetSiteList);
                var th2 = new Thread(ths2);
                th2.Start();

                if ((bool)Session["ManualTest"])
                {
                    // Do manual test stuff
                    manualResultHidden.Attributes.Remove("class");
                    GetManualTestResults();
                }

                th.Join();
                th2.Join();

                // Set rating sessions
                Session["RatingAccess"] = 0m;
                Session["RatingUx"] = 0m;
                Session["RatingMarketing"] = 0m;
                Session["RatingTech"] = 0m;
            }
        }

        private List<KeyValuePair<string, string>> GetApiKeys()
        {
            string[] lines = System.IO.File.ReadAllLines(@"C:\Users\Michael\Documents\Visual Studio 2013\Projects\DotsolutionsWebsiteTester\DotsolutionsWebsiteTester\DotsolutionsWebsiteTester\App_Data\keys.txt");
            var list = new List<KeyValuePair<string, string>>();
            foreach (string line in lines)
            {
                // Use a tab to indent each line of the file.
                var item = line.Split(':');
                list.Add(new KeyValuePair<string, string>(item[0], item[1]));
            }
            return list;
        }

        /// <summary>
        /// Get the robots.txt file from a domain
        /// </summary>
        /// <param name="url">URL of origin</param>
        /// <returns>String list of files in Disallow for all bots</returns>
        private List<string> GetRobotsTxt(string url)
        {
            var robots = new List<string>();

            var uri = new Uri(url);
            url = uri.Scheme + "://" + uri.Host + "/";

            try
            {
                var txt = "";
                using (var wc = new System.Net.WebClient())
                    txt = wc.DownloadString(url + "robots.txt");

                using (StringReader reader = new StringReader(txt))
                {
                    string line;
                    var applies = false;
                    while ((line = reader.ReadLine()) != null)
                    {
                        //Debug.WriteLine(line);
                        if (line == "User-agent: *")
                        {
                            applies = true;
                        }
                        if (line.Contains("User-agent") && !line.Contains("*"))
                        {
                            applies = false;
                        }
                        if (line.Contains("Disallow:") && applies == true)
                        {
                            try
                            {
                                line = line.Remove(0, 10);
                                robots.Add(line);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                // Nothing in Disallow
                            }
                        }
                    }
                }
            }
            catch (WebException we)
            {
                // Robots.txt probably does not exist, or internet failure
                Debug.WriteLine("WebException " + we.Message);
            }

            return robots;
        }

        /// <summary>
        /// Get list of selected tests from Session["selectedTests"]
        /// Adds this list to the page for the user to see, but also to use in the jquery AJAX requests
        /// </summary>
        private void GetTestList()
        {
            var selectedTests = (List<string>)Session["selectedTests"];
            var selectedTestsName = (List<string>)Session["selectedTestsName"];

            for (int i = 0; i < selectedTests.Count; i++)
            {
                // Set list for ajax requests
                performedTests.InnerHtml += "<li>" + selectedTests[i] + "</li>";
                // Display name for user
                PerformedTestsName.InnerHtml += "<li><a onclick=animateTo('" + selectedTests[i] + "') href='#" + selectedTests[i] + "'>" + selectedTestsName[i] + "</a></li>";
            }
        }

        /// <summary>
        /// Get list of sites we can test from the google search api
        /// Adds this list to Session["selectedSites"] so we can access it throughout the application
        /// </summary>
        private void GetSiteList()
        {
            var sitemap = new List<string>();
            string url = Session["MainUrl"].ToString();
            string userAgent = Session["userAgent"].ToString();
            bool isPresent = false;

            var apiKey = GetFromApiKeys("GoogleAPI");
            Debug.WriteLine(">>>> GetSiteList >>> ");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&key=" + apiKey + "&q=%22" + url + "%22&rsz=5");
            // Additional parameters
            // &rsz=[1-8] resultSize can be 1 through 8. Currently using 5.
            // &start=[x] Indicate where to start searching
            request.UserAgent = userAgent;
            request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            var reader = new StreamReader(dataStream);
            // Read the content. 
            string responseFromServer = reader.ReadToEnd();

            JObject googleSearch = JObject.Parse(responseFromServer);
            IList<JToken> results = googleSearch["responseData"]["results"].Children().ToList();

            System.Uri uri = new Uri(url);
            string uriWithoutScheme = uri.Host;

            if (results.Count != 0)
            {
                foreach (JToken item in results)
                {
                    if (IsOfDomain(url, item["unescapedUrl"].ToString()))
                    {
                        sitemap.Add(item["unescapedUrl"].ToString());

                        System.Uri uriFound = new Uri(item["unescapedUrl"].ToString());
                        string uriFoundWithoutScheme = uriFound.Host;
                        // If entered URL is not in found URL's it will be added
                        // This if-statement detects if it IS among the found URL's by comparing it to several possible URL formats
                        if (uriFoundWithoutScheme == uriWithoutScheme || "www." + uriFoundWithoutScheme == uriWithoutScheme)
                            isPresent = true;

                    }
                }
                if (!isPresent)
                    sitemap.Add(url);
            }
            else
                sitemap.Add(url);

            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();

            foreach (string item in sitemap)
                TestedSitesList.InnerHtml += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";

            // Add tested sites to session
            Session["selectedSites"] = sitemap;
        }

        /// <summary>
        /// Get ApiKey from Session["ApiKeys"]
        /// </summary>
        /// <param name="key">ApiKey</param>
        /// <returns>ApiKey Value</returns>
        private string GetFromApiKeys(string key)
        {
            var list = (List<KeyValuePair<string, string>>)Session["ApiKeys"];
            foreach (var element in list)
                if (element.Key == key)
                    return element.Value;
            return "";
        }

        private bool IsOfDomain(string url, string addition)
        {
            if (addition.Contains(url))
            {
                return true;
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(addition);
                request.UserAgent = Session["userAgent"].ToString();
                request.Timeout = 1000;
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var uri = new Uri(url);

                //Debug.WriteLine("Session['MainUrl'].ToString() uri.host =======> " + uri.Host);

                IPAddress[] addresslistMain = Dns.GetHostAddresses(uri.Host);
                IPAddress[] addresslist = Dns.GetHostAddresses(response.ResponseUri.Host.ToString());

                foreach (IPAddress theaddress in addresslist)
                {
                    if (addresslistMain.Contains(theaddress))
                        return true;
                }
            }
            catch (WebException we)
            {
                Debug.WriteLine("IsOfDomain Catch" + we.Message);
            }

            //IP check
            return false;
        }

        /// <summary>
        /// Click event handler to initiate Pdf creation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void CreatePdfBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/PdfTemplate");

            return;
        }

        private void GetManualTestResults()
        {
            var vormProfOpma = Convert.ToInt32(Session["VormProfOpma"]);
            var vormProfHuis = Convert.ToInt32(Session["VormProfHuis"]);
            var vormProfKleur = Convert.ToInt32(Session["VormProfKleur"]);

            var vormUxMen = Convert.ToInt32(Session["VormUxMen"]);
            var vormUxStruc = Convert.ToInt32(Session["VormUxStruc"]);

            var vormgevingOpmerking = Session["VormOpm"].ToString();

            CalculateStars(vormProfOpma, VormProfOpma);
            CalculateStars(vormProfHuis, VormProfHuis);
            CalculateStars(vormProfKleur, VormProfKleur);
            CalculateStars(vormUxMen, VormUxMen);
            CalculateStars(vormUxStruc, VormUxStruc);

            VormComment.InnerText = vormgevingOpmerking;

            // Read contents from div and add as Session for PDF
            var sb = new System.Text.StringBuilder();
            manualResultHidden.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["ManualTestResults"] = htmlstring;
        }

        private void CalculateStars(int rating, System.Web.UI.HtmlControls.HtmlGenericControl control)
        {
            for (int i = 0; i < rating; i++)
            {
                control.InnerHtml += "<i class='fa fa-star'></i>";
            }
            if (rating < 10)
            {
                var remainder = 10 - rating;
                for (int i = 0; i < remainder; i++)
                {
                    control.InnerHtml += "<i class='fa fa-star-o'></i>";
                }
            }
        }

        /// <summary>
        /// Add Criteria List to Session so it is accessible from PdfTemplate
        /// </summary>
        /// <param name="session"></param>
        /// <param name="innerhtml"></param>
        [System.Web.Services.WebMethod]
        public static void AddCriteriaListSession(string session, string innerhtml)
        {
            HttpContext.Current.Session[session] = innerhtml;
        }

        #region WebMethods for setting total rating


        [System.Web.Services.WebMethod]
        public static void ResetRating()
        {
            // Set rating sessions
            HttpContext.Current.Session["RatingAccess"] = 0m;
            HttpContext.Current.Session["RatingUx"] = 0m;
            HttpContext.Current.Session["RatingMarketing"] = 0m;
            HttpContext.Current.Session["RatingTech"] = 0m;
        }

        [System.Web.Services.WebMethod]
        public static string GetAccessRating()
        {
            var AccessRatingList = new List<string>();
            var selectedTests = (List<string>)HttpContext.Current.Session["selectedTests"];
            string[] accessRatingList = { "CodeQuality", 
                                                "PageTitles", 
                                                "MobileCompatibility", 
                                                "Headings", 
                                                "InternalLinks", 
                                                "UrlFormat" };
            AccessRatingList.AddRange(accessRatingList);

            if ((decimal)HttpContext.Current.Session["RatingAccess"] != 0m)
            {
                var AccessRatingSession = (decimal)HttpContext.Current.Session["RatingAccess"];
                var count = 0m;
                Debug.WriteLine("AccessRatingSession -- " + AccessRatingSession);
                foreach (var item in selectedTests)
                {
                    if (AccessRatingList.Contains(item))
                    {
                        count++;
                    }
                }

                Debug.WriteLine("AccessRatingSession divide by " + count);
                var temp = decimal.Round(AccessRatingSession / count, 1);
                Debug.WriteLine("AccessRatingSession = " + temp);
                HttpContext.Current.Session["RatingAccess"] = temp;
                if (temp == 10.0m)
                    return temp.ToString();
                return temp.ToString("0.0");
            }
            return "0,0";
        }

        [System.Web.Services.WebMethod]
        public static string GetUserxRating()
        {
            var UserxRatingList = new List<string>();
            var selectedTests = (List<string>)HttpContext.Current.Session["selectedTests"];

            string[] userxRatingList = { "GooglePlus", 
                                               "Facebook", 
                                               "Twitter", 
                                               "SocialInterest", 
                                               "Popularity", 
                                               "AmountOfContent", 
                                               "Images", 
                                               "ServerBehaviour", 
                                               "MobileCompatibility", 
                                               "InternalLinks", 
                                               "Printability", 
                                               "UrlFormat", 
                                               "Freshness" };
            UserxRatingList.AddRange(userxRatingList);

            if ((decimal)HttpContext.Current.Session["RatingUx"] != 0m)
            {
                var UserxRatingSession = (decimal)HttpContext.Current.Session["RatingUx"];
                var count = 0m;
                Debug.WriteLine("UserxRatingSession -- " + UserxRatingSession);
                foreach (var item in selectedTests)
                {
                    if (UserxRatingList.Contains(item))
                    {
                        count++;
                    }
                }
                Debug.WriteLine("UserxRatingSession divide by " + count);
                var temp = decimal.Round(UserxRatingSession / count, 1);
                Debug.WriteLine("UserxRatingSession = " + temp);
                HttpContext.Current.Session["RatingUx"] = temp;
                if (temp == 10.0m)
                    return temp.ToString();
                return temp.ToString("0.0");
            }
            return "0,0";
        }

        [System.Web.Services.WebMethod]
        public static string GetMarketingRating()
        {
            var MarketingRatingList = new List<string>();
            var selectedTests = (List<string>)HttpContext.Current.Session["selectedTests"];

            string[] marketingRatingList = { "GooglePlus", 
                                               "Facebook", 
                                               "Twitter", 
                                               "SocialInterest", 
                                               "Popularity", 
                                               "AmountOfContent", 
                                               "PageTitles", 
                                               "Headings", 
                                               "IncomingLinks", 
                                               "InternalLinks", 
                                               "Freshness", 
                                               "Analytics", 
                                               "MetaTags" };
            MarketingRatingList.AddRange(marketingRatingList);

            if ((decimal)HttpContext.Current.Session["RatingMarketing"] != 0m)
            {
                var MarketingRatingSession = (decimal)HttpContext.Current.Session["RatingMarketing"];
                var count = 0m;
                Debug.WriteLine("MarketingRatingSession -- " + MarketingRatingSession);
                foreach (var item in selectedTests)
                {
                    if (MarketingRatingList.Contains(item))
                    {
                        count++;
                    }
                }
                Debug.WriteLine("MarketingRatingSession divide by " + count);
                var temp = decimal.Round(MarketingRatingSession / count, 1);
                Debug.WriteLine("MarketingRatingSession = " + temp);
                HttpContext.Current.Session["RatingMarketing"] = temp;
                if (temp == 10.0m)
                    return temp.ToString();
                return temp.ToString("0.0");
            }
            return "0,0";
        }

        [System.Web.Services.WebMethod]
        public static string GetTechRating()
        {
            var TechRatingList = new List<string>();
            var selectedTests = (List<string>)HttpContext.Current.Session["selectedTests"];

            string[] techRatingList = { "CodeQuality", 
                                                "Images", 
                                                "ServerBehaviour", 
                                                "MobileCompatibility", 
                                                "Headings", 
                                                "InternalLinks", 
                                                "MetaTags", 
                                                "Printability", 
                                                "UrlFormat" };

            TechRatingList.AddRange(techRatingList);

            if ((decimal)HttpContext.Current.Session["RatingTech"] != 0m)
            {
                var TechRatingSession = (decimal)HttpContext.Current.Session["RatingTech"];
                var count = 0m;
                Debug.WriteLine("TechRatingSession -- " + TechRatingSession);
                foreach (var item in selectedTests)
                {
                    if (TechRatingList.Contains(item))
                    {
                        count++;
                    }
                }
                Debug.WriteLine("TechRatingSession divide by " + count);
                var temp = decimal.Round(TechRatingSession / count, 1);
                Debug.WriteLine("TechRatingSession = " + temp);
                HttpContext.Current.Session["RatingTech"] = temp;
                if (temp == 10.0m)
                    return temp.ToString();
                return temp.ToString("0.0");
            }
            return "0,0";
        }
        #endregion

        #region WebMethods for setting individual rating

        [System.Web.Services.WebMethod]
        public static string GetRatingAccessList()
        {
            var selectedTests = (List<string>)HttpContext.Current.Session["selectedTests"];
            var applicableTests = new List<string>();

            foreach (var item in selectedTests)
            {
                if (item == "CodeQuality" ||
                    item == "PageTitles" ||
                    item == "MobileCompatibility" ||
                    item == "Headings" ||
                    item == "InternalLinks" ||
                    item == "UrlFormat")
                    applicableTests.Add(item);
            }

            var orderedList = OrderByRating(applicableTests);
            var list = GetList(orderedList);
            return list;
        }

        [System.Web.Services.WebMethod]
        public static string GetRatingUxList()
        {
            var selectedTests = (List<string>)HttpContext.Current.Session["selectedTests"];
            var applicableTests = new List<string>();

            foreach (var item in selectedTests)
            {
                if (item == "GooglePlus" ||
                    item == "Facebook" ||
                    item == "Twitter" ||
                    item == "SocialInterest" ||
                    item == "Popularity" ||
                    item == "AmountOfContent" ||
                    item == "Images" ||
                    item == "ServerBehaviour" ||
                    item == "MobileCompatibility" ||
                    item == "InternalLinks" ||
                    item == "Printability" ||
                    item == "UrlFormat" ||
                    item == "Freshness")
                    applicableTests.Add(item);
            }

            var orderedList = OrderByRating(applicableTests);
            var list = GetList(orderedList);
            return list;
        }

        [System.Web.Services.WebMethod]
        public static string GetRatingMarketingList()
        {
            var selectedTests = (List<string>)HttpContext.Current.Session["selectedTests"];
            var applicableTests = new List<string>();

            foreach (var item in selectedTests)
            {
                if (item == "GooglePlus" ||
                    item == "Facebook" ||
                    item == "Twitter" ||
                    item == "SocialInterest" ||
                    item == "Popularity" ||
                    item == "AmountOfContent" ||
                    item == "PageTitles" ||
                    item == "Headings" ||
                    item == "IncomingLinks" ||
                    item == "InternalLinks" ||
                    item == "Freshness" ||
                    item == "Analytics" ||
                    item == "MetaTags")
                    applicableTests.Add(item);
            }

            var orderedList = OrderByRating(applicableTests);
            var list = GetList(orderedList);
            return list;
        }


        [System.Web.Services.WebMethod]
        public static string GetRatingTechList()
        {
            var selectedTests = (List<string>)HttpContext.Current.Session["selectedTests"];
            var applicableTests = new List<string>();

            foreach (var item in selectedTests)
            {
                if (item == "CodeQuality" ||
                    item == "Images" ||
                    item == "ServerBehaviour" ||
                    item == "MobileCompatibility" ||
                    item == "Headings" ||
                    item == "InternalLinks" ||
                    item == "MetaTags" ||
                    item == "Printability" ||
                    item == "UrlFormat")
                    applicableTests.Add(item);
            }

            var orderedList = OrderByRating(applicableTests);
            var list = GetList(orderedList);
            return list;
        }

        public static string GetList(Dictionary<string, decimal> orderedList)
        {
            var list = "";
            foreach (var item in orderedList)
            {
                var ratingClass = item.Key + "Rating " + GetRatingClass(item.Value);

                list += "<li><span class='" + ratingClass + "' >" + item.Value + "</span>"
                        + "<a onclick=animateTo('" + item.Key + "') href='#" + item.Key + "'>" + GetTestName(item.Key) + "</a></li>";
            }
            return list;
        }

        public static string GetRatingClass(decimal rating)
        {
            if (rating < 6m)
                return "lowScore ratingSquare";
            else if (rating < 8.5m)
                return "mediocreScore ratingSquare";
            else
                return "excellentScore ratingSquare";
        }

        /// <summary>
        /// Sort the ratings of a list in ascending order
        /// </summary>
        /// <param name="ratingList">Unordered list of a criterium</param>
        /// <returns>Ordered dictionary<string, decimal></returns>
        public static Dictionary<string, decimal> OrderByRating(List<string> ratingList)
        {
            var ratings = new Dictionary<string, decimal>();
            foreach (var item in ratingList)
            {
                //ratings.Add(new KeyValuePair<string, int>(item, (int)Session[item + "Rating"]));
                ratings.Add(item, (decimal)HttpContext.Current.Session[item + "Rating"]);
            }

            var sortedDict = from entry in ratings orderby entry.Value ascending select entry;
            var orderedList = new Dictionary<string, decimal>();
            foreach (var pair in sortedDict)
                orderedList.Add(pair.Key, pair.Value);

            return orderedList;
        }

        /// <summary>
        /// Get the name of the test that is familiair to the user
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public static string GetTestName(string test)
        {
            switch (test)
            {
                case "CodeQuality":
                    return "Code Kwaliteit";
                case "PageTitles":
                    return "Pagina titels";
                case "MobileCompatibility":
                    return "Mobiele compatibiliteit";
                case "Headings":
                    return "Headers";
                case "InternalLinks":
                    return "Interne links";
                case "UrlFormat":
                    return "URL formaat";

                case "GooglePlus":
                    return "Google+ Pagina";
                case "Facebook":
                    return "Facebook Pagina";
                case "Twitter":
                    return "Twitter Pagina";
                case "SocialInterest":
                    return "Sociale interesse";
                case "Popularity":
                    return "Populariteit";
                case "AmountOfContent":
                    return "Hoeveelheid content";
                case "Images":
                    return "Afbeeldingen";

                case "ServerBehaviour":
                    return "Server gedrag";
                case "Printability":
                    return "Printbaarheid";
                case "Freshness":
                    return "Actueelheid";
                case "IncomingLinks":
                    return "Binnenkomende links";
                case "Analytics":
                    return "Analytics";
                case "MetaTags":
                    return "Meta tags";

                default:
                    return "";
            }
        }

        #endregion
    }
}
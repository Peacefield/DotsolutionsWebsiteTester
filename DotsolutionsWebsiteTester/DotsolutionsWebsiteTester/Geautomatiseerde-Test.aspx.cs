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
                string userAgent = "Mozilla/5.0 (Quality test, http://www.example.net)";
                Session["userAgent"] = userAgent;

                var ths = new ThreadStart(GetTestList);
                var th = new Thread(ths);
                th.Start();

                var ths2 = new ThreadStart(GetSiteList);
                var th2 = new Thread(ths2);
                th2.Start();

                th.Join();
                th2.Join();

                if ((bool)Session["ManualTest"])
                {
                    // Do manual test stuff
                    manualResultHidden.Attributes.Remove("class");
                    GetManualTestResults();
                }

                // Set rating sessions
                Session["RatingAccess"] = 0m;
                Session["RatingUx"] = 0m;
                Session["RatingMarketing"] = 0m;
                Session["RatingTech"] = 0m;
            }
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
                PerformedTestsName.InnerHtml += "<li><a onclick=animateTo('" + selectedTests[i] + "') >" + selectedTestsName[i] + "</a></li>";
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


            //sitemap.Add(url);
            //testedsiteslist.InnerHtml += "<li><a href='" + url + "' target='_blank'>" + url + "</a></li>";
            //// Add tested sites to session
            //Session["selectedSites"] = sitemap;
            //return;

            var queryString = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&key=AIzaSyCW4MrrpXcOPU6JYkz-aauIctDQEoFymow&q=%22" + url + "%22&rsz=5";
            Debug.WriteLine(">>>> GetSiteList >>> " + queryString);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(queryString);
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&key=AIzaSyCW4MrrpXcOPU6JYkz-aauIctDQEoFymow&rsz=5&q=%22" + url + "%22");
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

            //if(uriWithoutScheme.Contains("www."))
            //    uriWithoutScheme = uriWithoutScheme.Remove(uriWithoutScheme.IndexOf)

            if (results.Count != 0)
            {
                foreach (JToken item in results)
                {
                    sitemap.Add(item["url"].ToString());

                    // If entered URL is not in found URL's it will be added
                    // This if-statement detects if it IS among the found URL's by comparing it to several possible URL formats
                    if (item["url"].ToString() == url || item["url"].ToString() == (url + "/")
                        || item["url"].ToString() == url.Replace("http://", "https://") || item["url"].ToString() == url.Replace("https://", "http://")
                        || item["url"].ToString() == url.Replace("http://", "https://") + "/" || item["url"].ToString() == url.Replace("https://", "http://") + "/")
                        isPresent = true;

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
        /// Click event handler to initiate Pdf creation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void CreatePdfBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/PdfTemplate.aspx");

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
                control.InnerHtml += "<i class='glyphicon glyphicon-star'></i>";
            }
            if (rating < 10)
            {
                var remainder = 10 - rating;
                for (int i = 0; i < remainder; i++)
                {
                    control.InnerHtml += "<i class='glyphicon glyphicon-star-empty'></i>";
                }
            }
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
                var temp = decimal.Round(AccessRatingSession / count, 1);
                HttpContext.Current.Session["RatingAccess"] = temp;
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
                var temp = decimal.Round(UserxRatingSession / count, 1);
                HttpContext.Current.Session["RatingUx"] = temp;
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
                var temp = decimal.Round(MarketingRatingSession / count, 1);
                HttpContext.Current.Session["RatingMarketing"] = temp;
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
                var temp = decimal.Round(TechRatingSession / count, 1);
                HttpContext.Current.Session["RatingTech"] = temp;
                return temp.ToString("0.0");
            }
            return "0,0";
        }
        #endregion
    }
}
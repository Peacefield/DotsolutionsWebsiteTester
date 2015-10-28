using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class GooglePlus : System.Web.UI.Page
    {
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

            GetGooglePlus();

            var sb = new System.Text.StringBuilder();
            GooglePlusSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["GooglePlus"] = htmlstring;
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

        /// <summary>
        /// Find a Google Plus page via either Google Search or URL entered by user
        /// </summary>
        private void GetGooglePlus()
        {
            Debug.WriteLine("GetGooglePlus >>>> ");
            var googleList = GetPossibleGPlus();
            var isGoogleFound = false;
            var screenName = "";
            decimal rating;
            foreach (var possibility in googleList)
            {
                if (IsGooglePage(possibility))
                {
                    Debug.WriteLine(possibility + " gevonden via Google!");

                    isGoogleFound = true;
                    screenName = possibility;
                    break;
                }
            }

            if (!isGoogleFound)
            {
                Debug.WriteLine("Nog niks gevonden via Google!");
                googleList = GetPossibleGPlusFromPage();
                foreach (var possibility in googleList)
                {
                    if (IsGooglePage(possibility))
                    {
                        Debug.WriteLine(possibility + " gevonden via Pagina!");

                        isGoogleFound = true;
                        screenName = possibility;
                        break;
                    }
                }
            }

            if (!isGoogleFound)
            {
                rating = 0.0m;
                googlePlusResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> Er is geen Google+ account gevonden die geassocieerd is met deze website. Zorg ervoor dat de URL van uw pagina in uw Google+-profiel staat</span></div>";
            }
            else
            {
                rating = GetGooglePlusRating(screenName);
            }

            decimal rounded = decimal.Round(rating, 1);
            GooglePlusRating.InnerHtml = rounded.ToString();

            var temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = rounded + temp;

            temp = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = rounded + temp;

            Session["GooglePlusRating"] = rounded;
            SetRatingDisplay(rating);
        }

        /// <summary>
        /// Search for possible Google Plus pages associated with the entere URL using Google Search
        /// </summary>
        /// <returns>List of possible screenNames</returns>
        private List<string> GetPossibleGPlus()
        {
            Debug.WriteLine("GetPossibleGPlus <<<< ");
            var url = Session["MainUrl"].ToString();
            var googleList = new List<string>();
            var apiKey = GetFromApiKeys("GoogleAPI");
            var requestString = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&key=" + apiKey + "&rsz=8&q=Google%2B+profile+" + url;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestString);
            request.UserAgent = Session["userAgent"].ToString();
            request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            JObject googleSearch = JObject.Parse(responseFromServer);
            IList<JToken> results = googleSearch["responseData"]["results"].Children().ToList();

            if (results.Count != 0)
            {
                foreach (JToken item in results)
                {
                    var screenName = "";
                    if (item["unescapedUrl"].ToString().Contains("+"))
                        screenName = SliceScreenName(item["unescapedUrl"].ToString());
                    if (screenName != "")
                    {
                        googleList.Add(screenName);
                    }
                }
            }
            return googleList;
        }

        /// <summary>
        /// Get a list of screen names found on a page
        /// </summary>
        /// <param name="url">Page to check for Facebook link</param>
        /// <returns>List of possible screen names found on page</returns>
        private List<string> GetPossibleGPlusFromPage()
        {
            var url = Session["MainUrl"].ToString();
            var screenNames = new List<string>();

            var webget = new HtmlWeb();
            var doc = webget.Load(url);
            if (doc.DocumentNode.SelectNodes("//a[@href]") != null)
            {
                foreach (var node in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    if (node.Attributes["href"].Value.Contains("plus.google.com"))
                    {
                        var temp = SliceScreenName(node.Attributes["href"].Value);
                        if (temp != "")
                            screenNames.Add(temp);
                    }
                }
            }
            return screenNames;
        }

        /// <summary>
        /// Get the response from Google+ API as JObject for a profile name
        /// </summary>
        /// <param name="screenName">Profile name</param>
        /// <returns>JSON Object responseFromServer</returns>
        private JObject GetFromApi(string screenName)
        {
            var apiKey = GetFromApiKeys("GoogleAPI");
            var requestString = "https://www.googleapis.com/plus/v1/people/" + screenName + "?key=" + apiKey;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestString);
            request.UserAgent = Session["userAgent"].ToString();
            request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            JObject googleSearch = JObject.Parse(responseFromServer);
            return googleSearch;
        }

        /// <summary>
        /// Calculate rating according to Plus Ones and followers
        /// Show found Plus Ones and followers on page
        /// </summary>
        /// <param name="screenName">Username</param>
        /// <returns>decimal rating</returns>
        private decimal GetGooglePlusRating(string screenName)
        {
            var googleSearch = GetFromApi(screenName);

            var profileImage = googleSearch["image"]["url"].ToString();
            var googlePlusOnes = (int)googleSearch["plusOneCount"];
            var followersCount = (int)googleSearch["circledByCount"];

            var rating = 10m;

            var percentage = (decimal)followersCount / (decimal)googlePlusOnes * 100;

            Debug.WriteLine("Percentage: " + percentage);

            if (followersCount >= googlePlusOnes)
            {
                rating = 10m;
            }
            else if (percentage > 75m)
            {
                rating = 10m;
            }
            else if (percentage > 60m)
            {
                rating = 8.0m;
            }
            else if (percentage > 50m)
            {
                rating = 7.5m;
            }
            else if (percentage > 33m)
            {
                rating = 5.5m;
            }
            else if (percentage > 10m)
            {
                rating = 4.0m;
            }
            else
            {
                rating = 1.0m;
            }

            googlePlusResults.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                + "<a href='https://plus.google.com/" + screenName + "' target='_blank'><img src='" + profileImage + "' alt='profileimage'/></a> "
                + "<span> Google+ account <a href='https://plus.google.com/" + screenName + "' target='_blank' font-size='large'>" + screenName + "</a> gevonden</span>"
                + "</div>";

            googlePlusResults.InnerHtml += "<div class='well well-lg resultWell'>"
                + "<i class='fa fa-google-plus-square fa-3x'></i>"
                + "<span> Dit account heeft " + googlePlusOnes.ToString("#,##0") + " Google +1's </span></div>"
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg resultWell'>"
                + "<i class='fa fa-users fa-3x'></i>"
                + "<span> Dit account heeft " + followersCount.ToString("#,##0") + " volgers</span></div>";

            return rating;
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 6m)
                GooglePlusRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                GooglePlusRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                GooglePlusRating.Attributes.Add("class", "excellentScore ratingCircle");
        }

        /// <summary>
        /// Slice screenName returned from Google results or Page results to try to make it a screen name
        /// </summary>
        /// <param name="screenName">URL containing possible screen name</param>
        /// <returns>string screenName</returns>
        private string SliceScreenName(string screenName)
        {
            Debug.WriteLine("SliceScreenName <<<<< " + screenName);

            if (screenName.Contains("plus.google.com/"))
            {
                // Cut off first part of the URL
                screenName = screenName.Remove(0, screenName.IndexOf("+"));

                if (screenName.EndsWith("/"))
                    screenName = screenName.Remove(screenName.Length - 1);

                // Check if still contains a /
                // This most probably indicates a part after the username
                if (screenName.Contains("/"))
                    screenName = screenName.Remove(screenName.IndexOf("/"), (screenName.Length - screenName.IndexOf("/")));
                
                // Remove any possible parameters
                if (screenName.Contains("?"))
                    screenName = screenName.Remove(screenName.IndexOf("?"), (screenName.Length - screenName.IndexOf("?")));

                if (!screenName.Contains("/") && screenName != "")
                {
                    return screenName;
                }
            }

            return "";
        }


        /// <summary>
        /// Compares found screenName with content of the associated page
        /// </summary>
        /// <param name="screenName">Google+ Page Name</param>
        /// <returns>Returns true if profile has entered URL associated with page</returns>
        private bool IsGooglePage(string screenName)
        {
            var googleSearch = GetFromApi(screenName);
            try
            {
                IList<JToken> results = googleSearch["urls"].Children().ToList();

                if (results.Count != 0)
                {
                    foreach (JToken item in results)
                    {
                        if (HasWebsite(item["value"].ToString()))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("Page has no URLs");
            }
            return false;
        }

        /// <summary>
        /// Check if URL in the Google+ profile is the same as the one the user entered
        /// </summary>
        /// <param name="fbUrl">URL of website found in Facebook profile</param>
        /// <returns></returns>
        private bool HasWebsite(string url)
        {
            if (url == null || url == "")
            {
                return false;
            }

            if (!url.Contains("http://") && !url.Contains("https://"))
            {
                url = "http://" + url;
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = Session["userAgent"].ToString();
                request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");
                request.Timeout = 10000;
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var uriMain = new Uri(Session["MainUrl"].ToString());

                //Debug.WriteLine("Session['MainUrl'].ToString() uri.host =======> " + uri.Host);

                IPAddress[] addresslistMain = Dns.GetHostAddresses(uriMain.Host);
                IPAddress[] addresslist = Dns.GetHostAddresses(response.ResponseUri.Host.ToString());

                foreach (IPAddress theaddress in addresslist)
                    if (addresslistMain.Contains(theaddress))
                        return true;
            }
            catch (UriFormatException)
            {
                Debug.WriteLine("IsWebsite UriFormatException Catch --- " + url);
                return false;
            }
            catch (WebException)
            {
                Debug.WriteLine("IsWebsite WebException Catch --- " + url);
                return false;
            }

            return false;
        }
    }
}
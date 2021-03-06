﻿using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class GooglePlus : System.Web.UI.Page
    {
        private string message;
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
        /// Find a Google Plus page via either Google Search or URL entered by user
        /// </summary>
        private void GetGooglePlus()
        {
            // De beoordeling is afhankelijk van het aantal volgers en aantal +1's van een pagina
            // Geen Google+ pagina zorgt voor een 0.0
            // 75+% van het aantal +1's volgt de pagina: 10
            // 60-75%: 8.0
            // 50-60%: 7.5
            // 33-50%: 5.5
            // 10-33%: 4.0
            //  0-10%: 1.0

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
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er is geen Google+ account gevonden die geassocieerd is met deze website. Zorg ervoor dat de URL van uw pagina in uw Google+-profiel staat</span></div>";
            }
            else
            {
                rating = GetGooglePlusRating(screenName);
            }

            GooglePlusResults.InnerHtml = message;

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
            var apiKey = System.Web.Configuration.WebConfigurationManager.AppSettings["GoogleAPI"];
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
            var apiKey = System.Web.Configuration.WebConfigurationManager.AppSettings["GoogleAPI"];
            var requestString = "https://www.googleapis.com/plus/v1/people/" + screenName + "?key=" + apiKey;
            var googleSearch = new JObject();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestString);
                request.UserAgent = Session["userAgent"].ToString();
                request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                googleSearch = JObject.Parse(responseFromServer);
            }
            catch (WebException)
            {
                Debug.WriteLine("ScreenName does not exist");
            }
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
            var coverImage = googleSearch["cover"]["coverPhoto"]["url"].ToString();
            var googlePlusOnes = (int)googleSearch["plusOneCount"];
            var followersCount = (int)googleSearch["circledByCount"];
            var displayName = googleSearch["displayName"].ToString();

            var rating = 10m;

            var percentage = (decimal)followersCount / (decimal)googlePlusOnes * 100;

            Debug.WriteLine("Percentage: " + percentage);

            if (followersCount >= googlePlusOnes)
            {
                rating = 10m;
            }
            else if (percentage >= 75m)
            {
                rating = 10m;
            }
            else if (percentage >= 60m)
            {
                rating = 8.0m;
            }
            else if (percentage >= 50m)
            {
                rating = 7.5m;
            }
            else if (percentage >= 33m)
            {
                rating = 5.5m;
            }
            else if (percentage >= 10m)
            {
                rating = 4.0m;
            }
            else
            {
                rating = 1.0m;
            }

            message += "<a href='https://plus.google.com/" + screenName + "' target='_blank'><span class='well well-lg coverpicture' style='background-image: url(" + coverImage + ")'></span></a>";

            message += "<div class='socialResults'>"
                + "<div class='socialResultBox-2 row'>"
                + "<i class='fa fa-google-plus-square fa-3x'></i>"
                + "<span> Dit account heeft " + googlePlusOnes.ToString("#,##0") + " Google +1's </span></div>"
                + "<div class='socialResultBox-2 row'>"
                + "<i class='fa fa-users fa-3x'></i>"
                + "<span> Dit account heeft " + followersCount.ToString("#,##0") + " volgers</span></div>"
                + "</div>";

            message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                + "<a href='https://plus.google.com/" + screenName + "' target='_blank'><img src='" + profileImage + "' alt='profileimage'/></a> "
                + "<span> Google+ account <a href='https://plus.google.com/" + screenName + "' target='_blank' font-size='large'>" + displayName + "</a> gevonden</span>"
                + "</div>";

            return rating;
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            var element = GooglePlusRating;

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
        /// Slice screenName returned from Google results or Page results to try to make it a screen name
        /// </summary>
        /// <param name="screenName">URL containing possible screen name</param>
        /// <returns>string screenName</returns>
        private string SliceScreenName(string url)
        {
            Debug.WriteLine("SliceScreenName <<<<< " + url);

            if (url.Contains("plus.google.com/"))
            {
                if (url.EndsWith("/"))
                    url = url.Remove(url.Length - 1);

                // If URL is custom made it contains a +
                if (url.IndexOf("+") != -1)
                {
                    // Cut off first part of the URL
                    url = url.Remove(0, url.IndexOf("+"));
                }
                else
                {
                    url = url.Remove(0, url.LastIndexOf("/") + 1);
                }

                // Check if still contains a /
                // This most probably indicates a part after the username
                if (url.Contains("/"))
                    url = url.Remove(url.IndexOf("/"), (url.Length - url.IndexOf("/")));

                // Remove any possible parameters
                if (url.Contains("?"))
                    url = url.Remove(url.IndexOf("?"), (url.Length - url.IndexOf("?")));

                if (!url.Contains("/") && url != "")
                {
                    return url;
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
            if (googleSearch.Count < 0)
                return false;

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
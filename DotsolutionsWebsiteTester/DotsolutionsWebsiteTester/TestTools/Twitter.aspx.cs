﻿using HtmlAgilityPack;
using LinqToTwitter;
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
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class Twitter : System.Web.UI.Page
    {
        // Currently using my personal twitter keys
        private SingleUserAuthorizer authorizer;
        private TwitterContext twitterContext;
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

            var ConsumerSecret = GetFromApiKeys("TwitterConsumerSecret");
            var AccesTokenSecret = GetFromApiKeys("TwitterAccesTokenSecret");

            this.authorizer = new SingleUserAuthorizer
                {
                    CredentialStore =
                       new SingleUserInMemoryCredentialStore
                       {
                           ConsumerKey =
                               "lZiItDrOsCPBBIiKioA3QV6IS",
                           ConsumerSecret =
                              ConsumerSecret,
                           AccessToken =
                              "39354153-VVOkgQxTdA8v34eInxOqPi5oY3GBp1nyNxV7TrTLZ",
                           AccessTokenSecret =
                              AccesTokenSecret
                       }
                };

            this.twitterContext = new TwitterContext(authorizer);

            var ths = new ThreadStart(GetTwitter);
            var th = new Thread(ths);
            th.Start();

            th.Join();

            var sb = new System.Text.StringBuilder();
            TwitterSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Twitter"] = htmlstring;
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

        private void GetTwitter()
        {
            var url = Session["MainUrl"].ToString();
            Debug.WriteLine("GetTwitter <<< ");

            var screennameList = new List<string>();
            var rating = 1.0m;
            var apiKey = GetFromApiKeys("GoogleAPI");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&key=" + apiKey + "&rsz=8&q=twitter%20" + url);
            // Additional parameters
            // &rsz=[1-8] resultSize can be 1 through 8, currently using 8
            // &start=[x] Indicate where to start searching
            request.UserAgent = Session["userAgent"].ToString();
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

            var isTwitterfound = false;
            if (results.Count != 0)
            {
                foreach (JToken item in results)
                {
                    var screenName = SliceScreenName(item["unescapedUrl"].ToString());
                    if (screenName != "")
                    {
                        screennameList.Add(screenName);
                        Debug.WriteLine(screenName + " aan lijst toegevoegd!");
                    }
                }

                foreach (var screenName in screennameList)
                {
                    if (IsTwitter(screenName))
                    {
                        Debug.WriteLine(screenName + " gevonden via Google!");
                        isTwitterfound = true;

                        rating = GetTwitterRating(screenName);

                        break;
                    }
                }
            }

            // If !isTwitterfound { doorzoek pagina op aanwezigheid twitter.com mbv agility pack
            if (!isTwitterfound)
            {
                var screenNames = GetScreenNamesFromPage(url);
                if (screenNames.Count > 0)
                {
                    foreach (var screenName in screenNames)
                    {
                        if (IsTwitter(screenName))
                        {
                            Debug.WriteLine(screenName + " gevonden via pagina!");
                            rating = GetTwitterRating(screenName);

                            isTwitterfound = true;

                            break;
                        }
                    }
                }
            }

            if (!isTwitterfound)
            {
                rating = 0.0m;
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er is geen Twitter account gevonden die geassocieerd is met deze website. Zorg ervoor dat de URL van uw pagina in uw Twitter-profiel staat</span></div>";
            }
            twitterResults.InnerHtml = message;
            decimal rounded = decimal.Round(rating, 1);
            TwitterRating.InnerHtml = rounded.ToString();

            var temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = rounded + temp;
            temp = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = rounded + temp;
            Session["TwitterRating"] = rounded;
            SetRatingDisplay(rating);
        }

        /// <summary>
        /// Get a list of screen names found on a page
        /// </summary>
        /// <param name="url">Page to check for Facebook link</param>
        /// <returns>List of possible screen names found on page</returns>
        private List<string> GetScreenNamesFromPage(string url)
        {
            Debug.WriteLine("GetScreenNamesFromPage <<< ");
            var screenNames = new List<string>();

            var webget = new HtmlWeb();
            var doc = webget.Load(url);
            if (doc.DocumentNode.SelectNodes("//a[@href]") != null)
            {
                foreach (var node in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    if (node.Attributes["href"].Value.Contains("twitter.com"))
                    {
                        var temp = SliceScreenName(node.Attributes["href"].Value);

                        Debug.WriteLine(temp + " aan lijst toevoegen!");
                        if (temp != "")
                            screenNames.Add(temp);
                    }
                }
            }
            return screenNames;
        }

        /// <summary>
        /// Slice screenName returned from Google results or Page results to try to make it a screen name
        /// </summary>
        /// <param name="screenName">URL containing possible screen name</param>
        /// <returns>string screenName</returns>
        private string SliceScreenName(string screenName)
        {
            if (screenName.Contains("twitter.com/"))
            {
                if (screenName.EndsWith("/"))
                    screenName = screenName.Remove(screenName.Length - 1);

                screenName = screenName.Remove(0, screenName.LastIndexOf("/") + 1);

                if (screenName.Contains("?"))
                    screenName = screenName.Remove(screenName.IndexOf("?"), (screenName.Length - screenName.IndexOf("?")));

                if (!screenName.Contains("/") && screenName != "")
                {
                    return screenName;
                }
            }

            return "";
        }

        private decimal GetTwitterRating(string screenName)
        {
            var rating = 1m;
            var users = from user in twitterContext.User
                        where user.Type == UserType.Show &&
                        user.ScreenName == screenName
                        select user;
            var returnedUser = users.ToList();

            var TweetCount = GetTweetCount(returnedUser);
            var FollowersCount = GetFollowerCount(returnedUser);

            var TweetCountString = TweetCount.ToString("#,##0");
            var FollowersCountString = FollowersCount.ToString("#,##0");
            var ProfileImage = GetProfileImage(returnedUser);

            var percentage = ((decimal)FollowersCount / (decimal)TweetCount) * 100m;

            Debug.WriteLine("percentage = " + percentage);

            if (FollowersCount >= TweetCount)
            {
                rating = 10m;
            }
            else if (percentage > 75m)
            {
                rating = 10m;
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
                rating = 4m;
            }
            else
            {
                rating = 1m;
            }

            message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                + "<a href='https://www.twitter.com/" + screenName + "' target='_blank'><img src='" + ProfileImage + "' alt='profileimage'/></a> "
                + "<span> Twitter account <a href='https://www.twitter.com/" + screenName + "' target='_blank' font-size='large'>@" + screenName + "</a> gevonden</span>"
                + "</div>";

            message += "<div class='well well-lg resultWell text-center'>"
                + "<i class='fa fa-retweet fa-3x'></i><br/>"
                + "<span> Dit account heeft " + TweetCountString + " tweets gemaakt </span></div>"
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg resultWell text-center'>"
                + "<i class='fa fa-users fa-3x'></i><br/>"
                + "<span> Dit account heeft " + FollowersCountString + " volgers</span></div>";

            return rating;
        }

        /// <summary>
        /// Find if the found name is a Twitteraccount with the URL in the description
        /// </summary>
        /// <param name="screenName">Found screen name</param>
        /// <returns>Boolean true if the account has the URL in description</returns>
        private bool IsTwitter(string screenName)
        {
            Debug.WriteLine("IsTwitter <<<<< " + screenName);
            var allUsers = new List<User>();

            var users = from user in twitterContext.User
                        where user.Type == UserType.Show &&
                        user.ScreenName == screenName
                        select user;
            try
            {
                allUsers = users.ToList();

                foreach (var item in allUsers)
                {
                    Debug.WriteLine("Er zit een item in allUsers");
                    if (item.Url != null)
                    {
                        Debug.WriteLine("Er zit een item in allUsers die een URL heeft in de profielinformatie: " + item.Url);

                        try
                        {
                            Debug.WriteLine("trying IsTwitter HttpWebReq");

                            #region shortened URL in bio

                            WebRequest request = WebRequest.Create(item.Url);
                            request.Method = WebRequestMethods.Http.Head;
                            WebResponse response = request.GetResponse();
                            var destination = response.ResponseUri.ToString();

                            request = WebRequest.Create(Session["MainUrl"].ToString());
                            request.Method = WebRequestMethods.Http.Head;
                            response = request.GetResponse();
                            var destinationOriginal = response.ResponseUri.ToString();
                            
                            if (destination == destinationOriginal)
                                return true;

                            #endregion

                            #region non-shortened URL in bio

                            var HttpWebRequest = (HttpWebRequest)WebRequest.Create(item.Url);
                            HttpWebRequest.UserAgent = Session["userAgent"].ToString();
                            HttpWebRequest.Timeout = 10000;
                            // Get the response.
                            var HttpWebResponse = (HttpWebResponse)HttpWebRequest.GetResponse();

                            var uriMain = new Uri(Session["MainUrl"].ToString());
                            var uriOther = new Uri(item.Url);

                            IPAddress[] addresslistMain = Dns.GetHostAddresses(uriMain.Host);
                            IPAddress[] addresslist = Dns.GetHostAddresses(HttpWebResponse.ResponseUri.Host.ToString());

                            foreach (IPAddress theaddress in addresslist)
                                if (addresslistMain.Contains(theaddress))
                                    return true;

                            #endregion

                            Debug.WriteLine("tried IsTwitter HttpWebReq");


                        }
                        catch (UriFormatException)
                        {
                            Debug.WriteLine("IsTwitter UriFormatException Catch --- " + item.Url);
                            return false;
                        }
                        catch (WebException we)
                        {
                            Debug.WriteLine("IsTwitter Catch" + we.Message);
                            return false;
                        }
                    }
                }
            }
            catch (AggregateException)
            {
                Debug.WriteLine(screenName + " veroorzaakte een fout!");
                return false;
            }
            return false;
        }

        /// <summary>
        /// Get amount of followers
        /// </summary>
        /// <param name="screenName">string screen name</param>
        /// <returns>Returns int amount of followers</returns>
        private int GetFollowerCount(List<User> returnedUser)
        {
            var amount = 0;

            foreach (var item in returnedUser)
            {
                amount = item.FollowersCount;
            }
            return amount;
        }

        /// <summary>
        /// Get amount of tweets from user
        /// </summary>
        /// <param name="screenName">string screen name</param>
        /// <returns>Returns int amount of tweets</returns>
        private int GetTweetCount(List<User> returnedUser)
        {
            var amount = 0;

            foreach (var item in returnedUser)
            {
                amount = item.StatusesCount;
            }
            return amount;
        }

        /// <summary>
        /// Get profile image of user
        /// </summary>
        /// <param name="screenName">string screen name</param>
        /// <returns>string url of profile image</returns>
        private string GetProfileImage(List<User> returnedUser)
        {
            var profileimage = "";

            foreach (var item in returnedUser)
            {
                profileimage = item.ProfileImageUrl;
            }
            return profileimage;

            /// <summary>
            /// Set the colour that indicates the rating accordingly
            /// </summary>
            /// <param name="rating">decimal rating</param>
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 6m)
                TwitterRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                TwitterRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                TwitterRating.Attributes.Add("class", "excellentScore ratingCircle");
        }
    }
}
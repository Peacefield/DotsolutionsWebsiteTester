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

            var ths = new ThreadStart(() => GetTwitter(Session["MainUrl"].ToString()));
            var th = new Thread(ths);
            th.Start();

            th.Join();

            var sb = new System.Text.StringBuilder();
            TwitterSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Twitter"] = htmlstring;
        }

        private string GetFromApiKeys(string key)
        {
            var list = (List<KeyValuePair<string, string>>)Session["ApiKeys"];
            var value = "";

            foreach (var element in list)
            {
                if (element.Key == key)
                    value = element.Value;
            }
            return value;
        }

        private void GetTwitter(string url)
        {
            Debug.WriteLine("GetTwitter <<< ");

            var screennameList = new List<string>();
            var rating = 1.0m;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&key=AIzaSyCW4MrrpXcOPU6JYkz-aauIctDQEoFymow&rsz=8&q=twitter%20" + url);
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

            if (results.Count != 0)
            {
                foreach (JToken item in results)
                {
                    var screenName = "";

                    if (item["unescapedUrl"].ToString().Contains("https://www.twitter.com/"))
                        screenName = item["unescapedUrl"].ToString().Remove(0, 24);
                    else if (item["unescapedUrl"].ToString().Contains("https://twitter.com/"))
                    {
                        screenName = item["unescapedUrl"].ToString().Remove(0, 20);
                        if (screenName.Contains("/"))
                            screenName = screenName.Remove(screenName.Length - 1);
                    }

                    if (screenName.EndsWith("/"))
                        screenName = screenName.Remove(screenName.Length - 1);

                    if (screenName.Contains("?"))
                        screenName = screenName.Remove(screenName.IndexOf("?"), (screenName.Length - screenName.IndexOf("?")));

                    if (!screenName.Contains("/") && screenName != "")
                    {
                        screennameList.Add(screenName);
                    }
                }

                var twitterfound = false;
                foreach (var screenName in screennameList)
                {
                    if (IsTwitter(screenName))
                    {
                        Debug.WriteLine(screenName + " gevonden!");
                        twitterfound = true;

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


                        twitterResults.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                            + "<a href='https://www.twitter.com/" + screenName + "' target='_blank'><img src='" + ProfileImage + "' alt='profileimage'/></a> "
                            + "<span> Twitter account <a href='https://www.twitter.com/" + screenName + "' target='_blank' font-size='large'>@" + screenName + "</a> gevonden</span>"
                            + "</div>";

                        twitterResults.InnerHtml += "<div class='well well-lg resultWell'>"
                            + "<i class='fa fa-retweet fa-3x'></i>"
                            + "<span> Dit account heeft " + TweetCountString + " tweets gemaakt </span></div>"
                            + "<div class='resultDivider'></div>"
                            + "<div class='well well-lg resultWell'>"
                            + "<i class='fa fa-users fa-3x'></i>"
                            + "<span> Dit account heeft " + FollowersCountString + " volgers</span></div>";
                        break;
                    }
                }
                if (!twitterfound)
                {
                    rating = 0.0m;
                    twitterResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                        + "<span> Er is geen Twitter account gevonden die geassocieerd is met deze website. Zorg ervoor dat de URL van uw pagina in uw Twitter-profiel staat</span></div>";
                }
            }
            else
            {
                rating = 1.0m;
                twitterResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> Er is geen Twitter account gevonden die geassocieerd is met deze website. Zorg ervoor dat de URL van uw pagina in uw Twitter-profiel staat</span></div>";
            }

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
        /// Find if the found name is a Twitteraccount with the URL in the description
        /// </summary>
        /// <param name="screenName">Found screen name</param>
        /// <returns>Boolean true if the account has the URL in description</returns>
        private bool IsTwitter(string screenName)
        {
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
                    if (item.Url != null)
                    {
                        try
                        {
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(item.Url);
                            request.UserAgent = Session["userAgent"].ToString();
                            request.Timeout = 10000;
                            // Get the response.
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                            var uri = new Uri(Session["MainUrl"].ToString());

                            //Debug.WriteLine("Session['MainUrl'].ToString() uri.host =======> " + uri.Host);

                            IPAddress[] addresslistMain = Dns.GetHostAddresses(uri.Host);
                            IPAddress[] addresslist = Dns.GetHostAddresses(response.ResponseUri.Host.ToString());

                            foreach (IPAddress theaddress in addresslist)
                                if (addresslistMain.Contains(theaddress))
                                    return true;
                        }
                        catch (WebException we)
                        {
                            Debug.WriteLine("IsTwitter Catch" + we.Message);
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
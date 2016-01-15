using HtmlAgilityPack;
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

            var ConsumerSecret = System.Web.Configuration.WebConfigurationManager.AppSettings["TwitterConsumerSecret"];
            var AccesTokenSecret = System.Web.Configuration.WebConfigurationManager.AppSettings["TwitterAccesTokenSecret"];

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

        private void GetTwitter()
        {
            // De beoordeling van dit onderdeel is afhankelijk van
            // - Het aantal tweets in de afgelopen 7 dagen, 3+ keer is maximaal 10, 2 keer is maximaal 8, 1 keer is maximaal 6, 0 keer is maximaal 4.
            // - Het aantal volgers van het account tegenover het aantal tweets. 50-75% is 2 punten aftrek, 33-50% is 3 punten aftrek, 10-33% is 4 punten aftrek, 0-10% is 5 punten aftrek.
            // - Het aantal gebruikers dat het account volgt tegenover het aantal volgers. Wanneer het aantal volgend 75+% is van aantal volgers: 2 punten aftrek.

            var url = Session["MainUrl"].ToString();
            Debug.WriteLine("GetTwitter <<< ");

            var screennameList = new List<string>();
            var rating = 1.0m;
            var apiKey = System.Web.Configuration.WebConfigurationManager.AppSettings["GoogleAPI"];

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

            Debug.WriteLine("Rounded = " + rounded);
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

        /// <summary>
        /// Get variables to decide rating
        /// </summary>
        /// <param name="screenName">string screenName</param>
        /// <returns>decimal rating</returns>
        private decimal GetTwitterRating(string screenName)
        {
            // Beoordeling a.d.h.v aantal tweets tegenover aantal volgers
            // Gebaseerd op percentage

            // Beoordeling a.d.h.v. aantal volgers tegenover aantal volgend
            // Aantal volgers meer dan 75% van aantal volgend = 2 punten aftrek

            // Beoordeling a.d.h.v. aantal tweets over afgelopen 7 dagen
            // 3x getweet = 10, 2x = 8, 1x = 6, daarna onvoldoende

            var rating = 10.0m;
            var users = from user in twitterContext.User
                        where user.Type == UserType.Show &&
                        user.ScreenName == screenName
                        select user;
            var returnedUser = users.ToList();

            var TweetCount = GetTweetCount(returnedUser);
            var FollowingCount = GetFollowingCount(returnedUser);
            var FollowersCount = GetFollowerCount(returnedUser);

            var TweetCountString = TweetCount.ToString("#,##0");
            var FollowersCountString = FollowersCount.ToString("#,##0");
            var FollowingCountString = FollowingCount.ToString("#,##0");
            var ProfileImage = GetProfileImage(returnedUser);
            var CoverImage = GetCoverImage(returnedUser);

            var TweetAmountString = "";
            var TweetAmountLastDays = GetTweetAmountLastDays(screenName);
            
            if (TweetAmountLastDays == 20)
                TweetAmountString = "Dit account heeft in de afgelopen 7 dagen minimaal 20 tweets verzonden";
            else if (TweetAmountLastDays == 1)
                TweetAmountString = "Dit account heeft in de afgelopen 7 dagen 1 tweet verzonden";
            else
                TweetAmountString = "Dit account heeft in de afgelopen 7 dagen " + TweetAmountLastDays + " tweets verzonden";

            if (TweetAmountLastDays < 3)
            {
                if (TweetAmountLastDays == 2)
                    rating = 8.0m;
                else if (TweetAmountLastDays == 1)
                    rating = 6.0m;
                else
                    rating = 4.0m;
            }
            
            var percentage = ((decimal)FollowersCount / (decimal)TweetCount) * 100m;
            Debug.WriteLine("percentage = " + percentage);

            if (percentage < 75)
            {
                if (percentage >= 50m)
                    rating = rating - 2m;
                else if (percentage >= 33m)
                    rating = rating - 3m;
                else if (percentage >= 10m)
                    rating = rating - 4m;
                else
                    rating = rating - 5m;
            }

            if (FollowingCount > (0.75 * FollowersCount))
                rating = rating - 2m;

            if (rating == 10.0m)
                rating = 10m;
            if (rating < 0)
                rating = 0.0m;

            if (CoverImage != null)
                message += "<a href='https://www.twitter.com/" + screenName + "' target='_blank'><span class='well well-lg coverpicture' style='background-image: url(" + CoverImage + ")'></span></a>";

            message += "<div class='socialResults'>"
                + "<div class='socialResultBox-3 row'>"
                + "<i class='fa fa-twitter-square fa-3x'></i>"
                + "<span> " + TweetAmountString + "</span></div>"
                + "<div class='socialResultBox-3 row'>"
                + "<i class='fa fa-retweet fa-3x'></i>"
                + "<span> Dit account heeft " + TweetCountString + " tweets gemaakt </span></div>"
                + "<div class='socialResultBox-3 row'>"
                + "<i class='fa fa-users fa-3x'></i>"
                + "<span> Dit account heeft " + FollowersCountString + " volgers en volgt " + FollowingCountString + " gebruikers</span></div>"
                + "</div>";

            message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                + "<a href='https://www.twitter.com/" + screenName + "' target='_blank'><img src='" + ProfileImage + "' alt='profileimage'/></a> "
                + "<span> Twitter account <a href='https://www.twitter.com/" + screenName + "' target='_blank' font-size='large'>@" + screenName + "</a> gevonden</span>"
                + "</div>";

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
                            request.Method = WebRequestMethods.Http.Get;
                            WebResponse response = request.GetResponse();
                            var destination = response.ResponseUri.ToString();

                            request = WebRequest.Create(Session["MainUrl"].ToString());
                            request.Method = WebRequestMethods.Http.Get;
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
        /// Get amount of accounts following this user
        /// </summary>
        /// <param name="returnedUser">string screen name</param>
        /// <returns>Returns int amount of following</returns>
        private int GetFollowingCount(List<User> returnedUser)
        {
            var amount = 0;

            foreach (var item in returnedUser)
            {
                amount = item.FriendsCount;
            }
            return amount;
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
        }

        /// <summary>
        /// Get profile image of user
        /// </summary>
        /// <param name="screenName">string screen name</param>
        /// <returns>string url of profile image</returns>
        private string GetCoverImage(List<User> returnedUser)
        {
            var coverImage = "";

            foreach (var item in returnedUser)
            {
                coverImage = item.ProfileBannerUrl;
            }
            return coverImage;
        }

        /// <summary>
        /// Gets the amount of tweets sent over the past 7 days
        /// </summary>
        /// <param name="screenName">string screen name</param>
        /// <returns>int amount of tweets sent</returns>
        private int GetTweetAmountLastDays(string screenName)
        {
            var tweetAmount = 0;
            var tweets = from tweet in twitterContext.Status
                         where tweet.Type == StatusType.User &&
                        tweet.ScreenName == screenName
                         select tweet;
            var returnedUser = tweets.ToList();

            foreach (var item in tweets)
            {
                if (IsInLastSevenDays(item.CreatedAt))
                    tweetAmount++;
                else
                    break;
            }

            return tweetAmount;
        }

        /// <summary>
        /// Check if tweet was sent more or less than 7 days ago
        /// </summary>
        /// <param name="dateTime">DateTime dateTime</param>
        /// <returns>bool</returns>
        private bool IsInLastSevenDays(DateTime dateTime)
        {
            var sevenDaysAgo = DateTime.Today.AddDays(-7);
            if (dateTime > sevenDaysAgo)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating == 10m)
                TwitterRating.Attributes.Add("class", "score-10 ratingCircle");
            else if (rating > 9m)
                TwitterRating.Attributes.Add("class", "score-9 ratingCircle");
            else if (rating > 8m)
                TwitterRating.Attributes.Add("class", "score-8 ratingCircle");
            else if (rating > 7m)
                TwitterRating.Attributes.Add("class", "score-7 ratingCircle");
            else if (rating > 6m)
                TwitterRating.Attributes.Add("class", "score-6 ratingCircle");
            else if (rating > 5m)
                TwitterRating.Attributes.Add("class", "score-5 ratingCircle");
            else if (rating > 4m)
                TwitterRating.Attributes.Add("class", "score-4 ratingCircle");
            else if (rating > 3m)
                TwitterRating.Attributes.Add("class", "score-3 ratingCircle");
            else if (rating > 2m)
                TwitterRating.Attributes.Add("class", "score-2 ratingCircle");
            else if (rating > 1m)
                TwitterRating.Attributes.Add("class", "score-1 ratingCircle");
            else
                TwitterRating.Attributes.Add("class", "score-0 ratingCircle");
        }
    }
}
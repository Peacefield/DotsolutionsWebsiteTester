using LinqToTwitter;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class SocialInterest : System.Web.UI.Page
    {
        private string message = "";
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


            var ths = new ThreadStart(GetSocialInterest);
            var th = new Thread(ths);
            th.Start();

            th.Join();

            //GetSocialInterest();

            var sb = new System.Text.StringBuilder();
            SocialInterestSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["SocialInterest"] = htmlstring;
        }

        private void GetSocialInterest()
        {
            var sitemap = (List<string>)Session["selectedSites"];
            var rating = -1m;
            
            var twitterShare = 0;
            //var linkedinShare = 0;
            var facebookShare = 0;
            var googleShare = 0;

            foreach (var page in sitemap)
            {
                twitterShare += GetTwitterShare(page);
                //linkedinShare = GetLinkedinShare(site);
                facebookShare += GetFacebookShare(page);
                googleShare += GetGoogleShare(page);
            }

            //var site = Session["MainUrl"].ToString();
            //var twitterShare = GetTwitterShare(site);
            ////var linkedinShare = GetLinkedinShare(site);
            //var facebookShare = GetFacebookShare(site);
            //var googleShare = GetGoogleShare(site);

            var twitterString = twitterShare.ToString("#,##0") + " tweets";
            //var linkedinString = linkedinShare.ToString("#,##0") + " shares";
            var facebookString = facebookShare.ToString("#,##0") + " shares";
            var googleString = googleShare.ToString("#,##0") + " +1's";

            if (twitterShare == 1)
                twitterString = twitterShare + " tweet";
            //if (linkedinShare == 1)
            //    linkedinString = linkedinShare + " share";
            if (facebookShare == 1)
                facebookString = facebookShare + " share";
            if (googleShare == 1)
                googleString = googleShare + " +1";

            message = "<div class='well well-lg thirdResultWell text-center'>"
                + "<i class='fa fa-twitter-square fa-3x'></i><br/>"
                + "<span> " + twitterString + "</span></div>"
                //+ "<i class='fa fa-linkedin-square fa-3x'></i><br/>"
                //+ "<span> " + linkedinString + "</span></div>"
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg thirdResultWell text-center'>"
                + "<i class='fa fa-facebook-official fa-3x'></i><br/>"
                + "<span> " + facebookString + "</span></div>"
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg thirdResultWell text-center'>"
                + "<i class='fa fa-google-plus-square fa-3x'></i><br/>"
                + "<span> " + googleString + "</span></div>"
                + message;

            SocialInterestResults.InnerHtml = message;
            //decimal rounded = decimal.Round(rating, 1);
            //SocialInterestRating.InnerHtml = rounded.ToString();
            SocialInterestRating.InnerHtml = "i";

            //var temp = (decimal)Session["RatingUx"];
            //Session["RatingUx"] = rounded + temp;
            //temp = (decimal)Session["RatingMarketing"];
            //Session["RatingMarketing"] = rounded + temp;
            Session["SocialInterestRating"] = rating;

            //Debug.WriteLine("Rounded = " + rounded);
            //SetRatingDisplay(rating);
        }

        private int GetTwitterShare(string site)
        {
            var ConsumerSecret = GetFromApiKeys("TwitterConsumerSecret");
            var AccesTokenSecret = GetFromApiKeys("TwitterAccesTokenSecret");
            var authorizer = new SingleUserAuthorizer
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
            var twitterContext = new TwitterContext(authorizer);

            return GetTweetCount(site, twitterContext);
        }

        private int GetTweetCount(string site, TwitterContext twitterContext)
        {
            var count = 0;

            var searchResponse = from search in twitterContext.Search
                                 where search.Type == SearchType.Search &&
                                 search.Query == site
                                 select search;
            try
            {
                var allSearches = searchResponse.ToList();
                foreach (var search in allSearches)
                {
                    Debug.WriteLine("Er zit een item in allSearches!");
                    Debug.WriteLine("Statuses.Count: " + search.Statuses.Count);
                    count = search.Statuses.Count;

                    foreach (var item in search.Statuses)
                    {
                        Debug.WriteLine("item.CreatedAt: " + item.CreatedAt);
                        Debug.WriteLine("item.Text: " + item.Text);
                    }
                }
            }
            catch (AggregateException)
            {
                message += "<div class='alert alert-info col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er ging iets mis bij het ophalen van gegevens van Twitter.</span></div>";
                Debug.WriteLine("Er ging iets mis.");
            }

            return count;
        }

        private int GetLinkedinShare(string site)
        {
            var count = 0;
            var requestString = "http://www.linkedin.com/countserv/count/share?url=" + site + "&format=json";

            var responseObject = GetResponseFromUrl(requestString, "LinkedIn");
            if (responseObject != null)
                count = Int32.Parse(responseObject["count"].ToString());

            return count;
        }

        private int GetFacebookShare(string site)
        {
            var count = 0;
            var requestString = "http://graph.facebook.com/?id=" + site;

            var responseObject = GetResponseFromUrl(requestString, "Facebook");
            if (responseObject["shares"] != null)
                count = Int32.Parse(responseObject["shares"].ToString());

            return count;
        }

        private JObject GetResponseFromUrl(string requestString, string platform)
        {
            JObject responseObject = null;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(requestString);
                request.UserAgent = Session["userAgent"].ToString();
                request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");

                var response = (HttpWebResponse)request.GetResponse();
                var dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);
                var responseFromServer = reader.ReadToEnd();
                responseObject = JObject.Parse(responseFromServer);
            }
            catch (WebException wex)
            {
                message += "<div class='alert alert-info col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er ging iets mis bij het ophalen van gegevens van " + platform + ".</span></div>";
                Debug.WriteLine(wex.Message);
            }

            return responseObject;
        }

        private int GetGoogleShare(string site)
        {
            var count = 0;
            var responseObject = GetGoogleResponseFromUrl(site);
            if (responseObject != null)
                count = Int32.Parse(responseObject["result"]["metadata"]["globalCounts"]["count"].ToString());

            return count;
        }

        private JObject GetGoogleResponseFromUrl(string url)
        {
            JObject responseObject = null;
            try
            {
                var apikey = GetFromApiKeys("GoogleAPI");
                string googleApiUrl = "https://clients6.google.com/rpc?key=" + "AIzaSyCKSbrvQasunBoV16zDH9R33D88CeLr9gQ";

                string postData = @"[{""method"":""pos.plusones.get"",""id"":""p"",""params"":{""nolog"":true,""id"":""" + url + @""",""source"":""widget"",""userId"":""@viewer"",""groupId"":""@self""},""jsonrpc"":""2.0"",""key"":""p"",""apiVersion"":""v1""}]";

                var request = (HttpWebRequest)WebRequest.Create(googleApiUrl);
                request.Method = "POST";
                request.ContentType = "application/json-rpc";
                request.UserAgent = Session["userAgent"].ToString();
                request.ContentLength = postData.Length;

                var writeStream = request.GetRequestStream();
                var encoding = new UTF8Encoding();
                byte[] bytes = encoding.GetBytes(postData);
                writeStream.Write(bytes, 0, bytes.Length);
                writeStream.Close();

                var response = (HttpWebResponse)request.GetResponse();
                var dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream, Encoding.UTF8);
                var responseFromServer = reader.ReadToEnd();
                responseFromServer = responseFromServer.Replace("[", "");
                responseFromServer = responseFromServer.Replace("]", "");
                responseObject = JObject.Parse(responseFromServer);
            }
            catch (WebException wex)
            {
                message += "<div class='alert alert-info col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er ging iets mis bij het ophalen van gegevens van Twitter.</span></div>";
                Debug.WriteLine(wex.Message);
            }
            return responseObject;
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
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 6m)
                SocialInterestRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                SocialInterestRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                SocialInterestRating.Attributes.Add("class", "excellentScore ratingCircle");
        }
    }
}
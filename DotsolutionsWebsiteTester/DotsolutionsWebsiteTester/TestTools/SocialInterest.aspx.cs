using LinqToTwitter;
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

            GetSocialInterest();

            var sb = new System.Text.StringBuilder();
            SocialInterestSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["SocialInterest"] = htmlstring;
        }

        // Alles behalve twitter
        // https://api.facebook.com/method/fql.query?query=select%20%20like_count%20from%20link_stat%20where%20url=%22url%22&format=json
        // https://gist.github.com/jonathanmoore/2640302

        private void GetSocialInterest()
        {
            var sitemap = (List<string>)Session["selectedSites"];
            var rating = 0.0m;

            var site = Session["MainUrl"].ToString();
            var twitterShare = GetTwitterShare(site);
            var facebookShare = GetFacebookShare(site);
            var googleShare = GetGoogleShare(site);


            message += "<div class='well well-lg thirdResultWell text-center'>"
                + "<i class='fa fa-twitter-square fa-3x'></i><br/>"
                + "<span> " + twitterShare + " tweets</span></div>"
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg thirdResultWell text-center'>"
                + "<i class='fa fa-facebook-official fa-3x'></i><br/>"
                + "<span> " + facebookShare + " likes</span></div>"
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg thirdResultWell text-center'>"
                + "<i class='fa fa-google-plus-square fa-3x'></i><br/>"
                + "<span> " + googleShare + " +1's</span></div>";

            SocialInterestResults.InnerHtml = message;
            decimal rounded = decimal.Round(rating, 1);
            SocialInterestRating.InnerHtml = rounded.ToString();

            var temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = rounded + temp;
            temp = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = rounded + temp;
            Session["SocialInterestRating"] = rounded;

            Debug.WriteLine("Rounded = " + rounded);
            SetRatingDisplay(rating);
        }

        private int GetTwitterShare(string site)
        {
            var count = 0;
            var requestString = "http://cdn.api.twitter.com/1/urls/count.json?url=" + site;

            var responseObject = GetResponseFromUrl(requestString, "Twitter");
            if (responseObject != null)
            {
                count = Int32.Parse(responseObject["count"].ToString());

                Debug.WriteLine("count: " + responseObject["count"]);
                Debug.WriteLine("url: " + responseObject["url"]);
            }

            return count;
        }

        private int GetFacebookShare(string site)
        {
            var count = 0;
            var requestString = "http://graph.facebook.com/?id=" + site;

            var responseObject = GetResponseFromUrl(requestString, "Facebook");
            if (responseObject != null)
            {
                count = Int32.Parse(responseObject["shares"].ToString());

                Debug.WriteLine("id: " + responseObject["id"]);
                Debug.WriteLine("shares: " + responseObject["shares"]);
            }

            return count;
        }

        private int GetGoogleShare(string site)
        {
            var count = 0;
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
                Stream dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);
                var responseFromServer = reader.ReadToEnd();
                responseObject = JObject.Parse(responseFromServer);
            }
            catch (WebException wex)
            {
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er ging iets mis bij het ophalen van gegevens van " + platform + ".</span></div>";
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
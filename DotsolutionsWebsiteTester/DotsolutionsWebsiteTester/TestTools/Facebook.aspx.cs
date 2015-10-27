using Facebook;
using HtmlAgilityPack;
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
    public partial class Facebook : System.Web.UI.Page
    {
        private FacebookClient fbc;
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

            var AccesTokenSecret = GetFromApiKeys("FacebookAppSecret");

            fbc = new FacebookClient();
            try
            {

                // Create App and fill in credentials
                dynamic result = fbc.Get("oauth/access_token", new
                {
                    client_id = "1506552426310758", // App ID
                    client_secret = AccesTokenSecret, // App Secret
                    grant_type = "client_credentials"
                });

                var accessToken = result.access_token;

                if (result.error == null)
                {
                    fbc.AccessToken = accessToken;
                }

                var ths = new ThreadStart(GetFacebook);
                var th = new Thread(ths);
                th.Start();

                th.Join();
            }
            catch (FacebookOAuthException)
            {
                var rating = 5.5m;
                FacebookResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> Er kon geen beveiligde verbinding worden vastgesteld.</span></div>";

                decimal rounded = decimal.Round(rating, 1);
                FacebookRating.InnerHtml = rounded.ToString();

                var temp = (decimal)Session["RatingUx"];
                Session["RatingUx"] = rounded + temp;
                temp = (decimal)Session["RatingMarketing"];
                Session["RatingMarketing"] = rounded + temp;
                Session["FacebookRating"] = rounded;
                SetRatingDisplay(rating);
            }

            var sb = new System.Text.StringBuilder();
            FacebookSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Facebook"] = htmlstring;
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

        /// <summary>
        /// Find Facebook Page using Google and user-entered page-content
        /// </summary>
        private void GetFacebook()
        {
            Debug.WriteLine("GetFacebook <<< ");

            var url = Session["MainUrl"].ToString();
            var screennameList = new List<string>();
            var rating = 1.0m;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&key=AIzaSyCW4MrrpXcOPU6JYkz-aauIctDQEoFymow&rsz=8&q=facebook%20" + url);
            request.UserAgent = Session["userAgent"].ToString();
            request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            JObject googleSearch = JObject.Parse(responseFromServer);
            IList<JToken> results = googleSearch["responseData"]["results"].Children().ToList();

            var isFacebookFound = false;
            if (results.Count != 0)
            {
                foreach (JToken item in results)
                {
                    var screenName = "";

                    screenName = SliceScreenName(item["unescapedUrl"].ToString());
                    if (screenName != "")
                    {
                        screennameList.Add(screenName);
                    }
                }

                foreach (var screenName in screennameList)
                {
                    Debug.WriteLine(screenName + " testen via Facebook API!");
                    if (IsFacebook(screenName))
                    {
                        Debug.WriteLine(screenName + " facebook gevonden via Google!");
                        rating = GetFacebookRating(screenName);

                        isFacebookFound = true;

                        break;

                    }
                }
            }

            // If !isFacebookFound { doorzoek pagina op aanwezigheid facebook.com mbv agility pack
            if (!isFacebookFound)
            {
                var screenNames = GetScreenNamesFromPage(url);
                if (screenNames.Count > 0)
                {
                    foreach (var screenName in screenNames)
                    {
                        if (IsFacebook(screenName))
                        {
                            Debug.WriteLine(screenName + " facebook gevonden via pagina!");
                            rating = GetFacebookRating(screenName);

                            isFacebookFound = true;

                            break;
                        }
                    }
                }
            }

            if (!isFacebookFound)
            {
                rating = 0.0m;
                FacebookResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> Er is geen Facebook account gevonden die geassocieerd is met deze website. Zorg ervoor dat de URL van uw pagina in uw Facebook-profiel staat</span></div>";
            }

            decimal rounded = decimal.Round(rating, 1);
            FacebookRating.InnerHtml = rounded.ToString();

            var temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = rounded + temp;
            temp = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = rounded + temp;
            SetRatingDisplay(rating);
            Session["FacebookRating"] = rounded;
        }

        /// <summary>
        /// Slice screenName returned from Google results or Page results to try to make it a screen name
        /// </summary>
        /// <param name="screenName">URL containing possible screen name</param>
        /// <returns>string screenName</returns>
        private string SliceScreenName(string screenName)
        {
            if (screenName.Contains("facebook.com/"))
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
                    if (node.Attributes["href"].Value.Contains("facebook.com"))
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
        /// Check if screenName is a valid Facebook account with user-entered URL in website section
        /// </summary>
        /// <param name="screenName">Screen name you want to test</param>
        /// <returns>true if it is a valid Facebook account with user-entered URL in website section</returns>
        private bool IsFacebook(string screenName)
        {
            try
            {
                dynamic result = fbc.Get(screenName, new { fields = "id, name, username, website" });
                var fbId = result.id;
                var fbName = result.name;
                var fbUserName = result.username;
                var fbUrl = result.website;

                if (fbName != "")
                {
                    if (HasWebsite(fbUrl))
                        return true;
                    else
                        Debug.WriteLine("Geen URL in beschrijving");
                }
            }
            catch (FacebookApiException)
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Check if URL in facebook profile is the same that the user entered
        /// </summary>
        /// <param name="fbUrl">URL of website found in Facebook profile</param>
        /// <returns></returns>
        private bool HasWebsite(string fbUrl)
        {
            if (fbUrl == null || fbUrl == "")
            {
                return false;
            }

            if (!fbUrl.Contains("http://") && !fbUrl.Contains("https://"))
            {
                fbUrl = "http://" + fbUrl;
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fbUrl);
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
                Debug.WriteLine("IsWebsite UriFormatException Catch --- " + fbUrl);
                return false;
            }
            catch (WebException)
            {
                Debug.WriteLine("IsWebsite WebException Catch --- " + fbUrl);
                return false;
            }

            return false;
        }

        /// <summary>
        /// Determine the rating by comparing amount of likes to the amount of people talking about the page
        /// </summary>
        /// <param name="screenName">Screen name of page</param>
        /// <returns>decimal rating</returns>
        private decimal GetFacebookRating(string screenName)
        {
            var rating = 1m;
            dynamic result = fbc.Get(screenName, new { fields = "likes, picture, talking_about_count" });
            var fbLikes = result.likes.ToString("#,##0");
            var fbPicture = result.picture.data["url"];
            var fbTalking = result.talking_about_count.ToString("#,##0");

            var percentage = ((decimal)result.talking_about_count / (decimal)result.likes) * 100;
            if (percentage > 10m)
            {
                rating = 10m;
            }
            else if (percentage > 1m)
            {
                rating = 7.5m;
            }
            else if (percentage > 0.5m)
            {
                rating = 5.5m;
            }
            else if (percentage > 0.25m)
            {
                rating = 4m;
            }
            else
            {
                rating = 1m;
            }

            FacebookResults.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                + "<a href='https://www.facebook.com/" + screenName + "' target='_blank'><img src='" + fbPicture + "' alt='profileimage'/></a> "
                + "<span> Facebook account <a href='https://www.facebook.com/" + screenName + "' target='_blank' font-size='larger'>" + screenName + "</a> gevonden</span></div>";

            var likesGrammar = " likes";
            if (fbLikes == "1") likesGrammar = " like";
            var talkingGrammar = " mensen praten";
            if (fbTalking == "1") talkingGrammar = " persoon praat";

            FacebookResults.InnerHtml += "<div class='well well-lg resultWell'>"
                + "<i class='fa fa-thumbs-o-up fa-3x'></i>"
                + "<span> Dit account heeft " + fbLikes + likesGrammar + "</span></div>"
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg resultWell'>"
                + "<i class='fa fa-commenting-o fa-3x'></i>"
                + "<span> " + fbTalking + talkingGrammar + " hier over</span></div>";

            return rating;
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 6m)
                FacebookRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                FacebookRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                FacebookRating.Attributes.Add("class", "excellentScore ratingCircle");
        }
    }
}
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

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class Facebook : System.Web.UI.Page
    {
        private FacebookClient fbc;
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

            var ClientId = System.Web.Configuration.WebConfigurationManager.AppSettings["FacebookAppId"];
            var AccesTokenSecret = System.Web.Configuration.WebConfigurationManager.AppSettings["FacebookAppSecret"];
            
            fbc = new FacebookClient();
            try
            {

                // Create App and fill in credentials
                dynamic result = fbc.Get("oauth/access_token", new
                {
                    client_id = ClientId, // App ID
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
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er kon geen beveiligde verbinding worden vastgesteld.</span></div>";

                decimal rounded = decimal.Round(rating, 1);
                FacebookRating.InnerHtml = rounded.ToString();

                var temp = (decimal)Session["RatingUx"];
                Session["RatingUx"] = rounded + temp;
                temp = (decimal)Session["RatingMarketing"];
                Session["RatingMarketing"] = rounded + temp;
                Session["FacebookRating"] = rounded;
                SetRatingDisplay(rating);
            }

            FacebookResults.InnerHtml = message;

            var sb = new System.Text.StringBuilder();
            FacebookSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Facebook"] = htmlstring;
        }

        /// <summary>
        /// Find Facebook Page using Google and user-entered page-content
        /// </summary>
        private void GetFacebook()
        {
            // Geen Facebook account zorgt voor een 0.0 beoordeling

            // Beoordeling gebaseerd op aantal likes en aantal mensen die praten over de pagina
            // 10+% van aantal likes praat over pagina: 10
            // 1%-10%: 7,5
            // 0.5% - 1%: 5.5
            // 0.25%-0.5%: 4.0
            // 0%-0.25%: 1.0

            Debug.WriteLine("GetFacebook <<< ");

            var url = Session["MainUrl"].ToString();
            var screennameList = new List<string>();
            var rating = 0.0m;

            var apiKey = System.Web.Configuration.WebConfigurationManager.AppSettings["GoogleAPI"];

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&key=" + apiKey + "&rsz=8&q=facebook%20" + url);
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
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er is geen Facebook pagina gevonden die geassocieerd is met deze website. Zorg ervoor dat de URL van uw pagina in uw Facebook-profiel staat.</span></div>";
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
        /// Check if screenName is a valid Facebook page with user-entered URL in website section
        /// </summary>
        /// <param name="screenName">Screen name you want to test</param>
        /// <returns>true if it is a valid Facebook page with user-entered URL in website section</returns>
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
            dynamic result = fbc.Get(screenName, new { fields = "likes, picture, talking_about_count, cover, posts" });
            var fbLikes = result.likes.ToString("#,##0");
            var fbPicture = result.picture.data["url"];
            var fbTalking = result.talking_about_count.ToString("#,##0");
            var fbCover= "";
            try
            {
                fbCover = result.cover["source"];
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {

            }

            var percentage = ((decimal)result.talking_about_count / (decimal)result.likes) * 100;
            if (percentage >= 10m)
            {
                rating = 10m;
            }
            else if (percentage >= 1m)
            {
                rating = 7.5m;
            }
            else if (percentage >= 0.5m)
            {
                rating = 5.5m;
            }
            else if (percentage >= 0.25m)
            {
                rating = 4.0m;
            }
            else
            {
                rating = 1m;
            }

            message += "<a href='https://www.facebook.com/" + screenName + "' target='_blank'><span class='well well-lg coverpicture' style='background-image: url(" + fbCover + ")'></span></a>";


            var likesGrammar = " likes";
            if (fbLikes == "1") likesGrammar = " like";
            var talkingGrammar = " mensen praten";
            if (fbTalking == "1") talkingGrammar = " persoon praat";

            var sitemap = (List<string>)Session["selectedSites"];
            var shares = 0;

            foreach (var page in sitemap)
            {
                shares += GetFacebookShare(page);
            }
            message += "<div class='socialResults'>"
                + "<div class='socialResultBox-3 row'>"
                + "<i class='fa fa-thumbs-o-up fa-3x'></i>"
                + "<span> Deze pagina heeft " + fbLikes + likesGrammar + "</span></div>"
                + "<div class='socialResultBox-3 row'>"
                + "<i class='fa fa-share fa-3x'></i>"
                + "<span> De geteste pagina's zijn " + shares.ToString("#,##0") + " keer gedeeld</span></div>"
                + "<div class='socialResultBox-3 row'>"
                + "<i class='fa fa-commenting-o fa-3x'></i>"
                + "<span> " + fbTalking + talkingGrammar + " hier over</span></div>"
                + "</div>";

            message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                + "<a href='https://www.facebook.com/" + screenName + "' target='_blank'><img src='" + fbPicture + "' alt='profileimage'/></a> "
                + "<span> Facebook pagina <a href='https://www.facebook.com/" + screenName + "' target='_blank' font-size='larger'>" + screenName + "</a> gevonden</span></div>";

            return rating;
        }

        /// <summary>
        /// Get amount of Facebookshares from JObject
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private int GetFacebookShare(string site)
        {
            var count = 0;
            var requestString = "http://graph.facebook.com/?id=" + site;

            var responseObject = GetResponseFromUrl(requestString);
            if (responseObject["shares"] != null)
                count = Int32.Parse(responseObject["shares"].ToString());

            return count;
        }

        /// <summary>
        /// Get JObject containing response from requestString
        /// </summary>
        /// <param name="requestString">string requestString</param>
        /// <param name="platform">string platform</param>
        /// <returns>JObject</returns>
        private JObject GetResponseFromUrl(string requestString)
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
                    + "<span class='messageText'> Er ging iets mis bij het ophalen van gegevens.</span></div>";
                Debug.WriteLine(wex.Message);
            }

            return responseObject;
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            var element = FacebookRating;

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
    }
}
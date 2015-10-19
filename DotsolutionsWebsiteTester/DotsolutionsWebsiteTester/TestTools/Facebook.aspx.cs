using Facebook;
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

            fbc = new FacebookClient();
            try
            {

                // Create App and fill in credentials
                dynamic result = fbc.Get("oauth/access_token", new
                {
                    client_id = "1506552426310758", // App ID
                    client_secret = "a7d0d5c36c32dc43d82bb39df60ebe52", // App Secret
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
                Rating.InnerHtml = rounded.ToString();

                var temp = (decimal)Session["RatingUx"];
                Session["RatingUx"] = rounded + temp;
                temp = (decimal)Session["RatingMarketing"];
                Session["RatingMarketing"] = rounded + temp;
                SetRatingDisplay(rating);
            }

            var sb = new System.Text.StringBuilder();
            FacebookSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Facebook"] = htmlstring;
        }

        private void GetFacebook()
        {
            Debug.WriteLine("GetFacebook <<< ");

            var url = Session["MainUrl"].ToString();
            var screennameList = new List<string>();
            var rating = 1.0m;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&key=AIzaSyCW4MrrpXcOPU6JYkz-aauIctDQEoFymow&rsz=8&q=facebook%20" + url);
            request.UserAgent = Session["userAgent"].ToString();
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

                    if (item["unescapedUrl"].ToString().Contains("https://www.facebook.com/"))
                        screenName = item["unescapedUrl"].ToString().Remove(0, 25);
                    else if (item["unescapedUrl"].ToString().Contains("https://facebook.com/"))
                        screenName = item["unescapedUrl"].ToString().Remove(0, 21);

                    if (screenName.EndsWith("/"))
                        screenName = screenName.Remove(screenName.Length - 1);

                    if (screenName.Contains("?"))
                        screenName = screenName.Remove(screenName.IndexOf("?"), (screenName.Length - screenName.IndexOf("?")));

                    if (!screenName.Contains("/") && screenName != "")
                    {
                        screennameList.Add(screenName);
                    }
                }

                var isFacebookFound = false;
                foreach (var screenName in screennameList)
                {
                    if (IsFacebook(screenName))
                    {
                        rating = 10m;
                        Debug.WriteLine(screenName + " facebook gevonden!");
                        isFacebookFound = true;

                        dynamic result = fbc.Get(screenName, new { fields = "likes, picture, talking_about_count" });
                        var fbLikes = result.likes.ToString("#,##0");
                        var fbPicture = result.picture.data["url"];
                        var fbTalking = result.talking_about_count.ToString("#,##0");

                        FacebookResults.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                            + "<a href='https://www.facebook.com/" + screenName + "' target='_blank'><img src='" + fbPicture + "' alt='profileimage'/></a> "
                            + "<span> Facebook account <a href='https://www.facebook.com/" + screenName + "' target='_blank' font-size='larger'>" + screenName + "</a> gevonden</span></div>";

                        FacebookResults.InnerHtml += "<div class='well well-lg ResultWell'>"
                            + "<i class='fa fa-thumbs-o-up fa-3x'></i>"
                            + "<span> Dit account heeft " + fbLikes + " likes </span></div>"
                            + "<div class='ResultDivider'></div>"
                            + "<div class='well well-lg ResultWell'>"
                            + "<i class='fa fa-commenting-o fa-3x'></i>"
                            + "<span> " + fbTalking + " mensen praten hier over</span></div>";
                    }
                }
                if (!isFacebookFound)
                {
                    rating = 1.0m;
                    FacebookResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                        + "<span> Er is geen Facebook account gevonden die geassocieerd is met deze website. Zorg ervoor dat de URL van uw pagina in uw Facebook-profiel staat</span></div>";
                }
            }
            else
            {
                rating = 1.0m;
                FacebookResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> Er is geen Facebook account gevonden die geassocieerd is met deze website. Zorg ervoor dat de URL van uw pagina in uw Facebook-profiel staat</span></div>";
            }
            decimal rounded = decimal.Round(rating, 1);
            Rating.InnerHtml = rounded.ToString();

            var temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = rounded + temp;
            temp = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = rounded + temp;
            SetRatingDisplay(rating);
        }

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
                }
            }
            catch (FacebookApiException)
            {
                return false;
            }

            return false;
        }

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
        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 4)
            {
                Rating.Style.Add("background-color", "red");
                Rating.Style.Add("color", "white");
            }
            else if (rating < 8)
            {
                Rating.Style.Add("background-color", "orangered");
                Rating.Style.Add("color", "white");
            }
            else
            {
                Rating.Style.Add("background-color", "green");
                Rating.Style.Add("color", "white");
            }
        }
    }
}
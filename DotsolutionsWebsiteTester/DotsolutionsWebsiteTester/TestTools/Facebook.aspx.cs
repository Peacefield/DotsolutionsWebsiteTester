using Facebook;
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
    public partial class Facebook : System.Web.UI.Page
    {
        FacebookClient fbc;
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
                GetFacebook();
            }
            catch (FacebookOAuthException)
            {
                var rating = 5.5m;
                facebookResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> Er kon geen beveiligde verbinding worden vastgesteld.</span></div>";

                decimal rounded = decimal.Round(rating, 1);
                Rating.InnerHtml = rounded.ToString();

                var temp = (decimal)Session["RatingUx"];
                Session["RatingUx"] = rounded + temp;
                temp = (decimal)Session["RatingMarketing"];
                Session["RatingMarketing"] = rounded + temp;
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

                        //dynamic result = fbc.Get(screenName, new { fields = "fan_count" });
                        dynamic result = fbc.Get(screenName, new { fields = "likes" });
                        var fbLikes = result.likes.ToString("#,##0"); ;

                        facebookResults.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                            + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                            + "<span> Facebook account <a href='https://www.facebook.com/" + screenName + "' target='_blank' font-size='larger'>" + screenName + "</a> gevonden</span></div>";
                        facebookResults.InnerHtml += "<div class='alert alert-info col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                            + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg'></i>"
                            + "<span> Dit account heeft " + fbLikes + " likes</span></div>";
                    }
                }
                if (!isFacebookFound)
                {
                    rating = 1.0m;
                    facebookResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                        + "<span> Er is geen Facebook account gevonden die geassocieerd is met deze website. Zorg ervoor dat de URL van uw pagina in uw Facebook-profiel staat</span></div>";
                }
            }
            else
            {
                rating = 1.0m;
                facebookResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> Er is geen Facebook account gevonden die geassocieerd is met deze website. Zorg ervoor dat de URL van uw pagina in uw Facebook-profiel staat</span></div>";
            }
            decimal rounded = decimal.Round(rating, 1);
            Rating.InnerHtml = rounded.ToString();

            var temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = rounded + temp;
            temp = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = rounded + temp;
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
                    if (IsWebsite(fbUrl))
                        return true;
                }
            }
            catch (FacebookApiException)
            {
                return false;
            }

            return false;
        }

        private bool IsWebsite(string fbUrl)
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
            catch (UriFormatException ufe)
            {
                Debug.WriteLine("IsWebsite UriFormatException Catch --- " + fbUrl);
                return false;
            }
            catch (WebException we)
            {
                Debug.WriteLine("IsWebsite WebException Catch --- " + fbUrl);
                return false;
            }

            return false;
        }

        //Acces Token: CAACEdEose0cBAN6Mglo4kgzCv5RaOAovPlZBQmYLc1mSjXTYxZBaHd14tvtofH3EEqzpWtSORuFKnfuYrC3zNDpVwALPm9JoMUUOrzLBoDazyxF6mRVH0wTX2H1FXVMhgZCQXioGZCrKEuOb6ZAZBKZAo6YaXKS13aC4GtOKYAvAaPqeXhVs7yLSLy8geYkuZBeQJfuKHBuduAZDZD
        //SELECT fan_count, website, pic FROM page WHERE username="dotsolutions"
        //{
        //  "data": [
        //    {
        //      "fan_count": 253,
        //      "website": "http://www.dotsolutions.nl",
        //      "pic": "https://scontent.xx.fbcdn.net/hprofile-xat1/v/t1.0-1/p100x100/12144761_1146334558728006_6208729277974377873_n.png?oh=48e85ea1a9cc32ea8902d6bd5f5fcf2f&oe=56C87E22"
        //    }
        //  ],
        //}
    }
}
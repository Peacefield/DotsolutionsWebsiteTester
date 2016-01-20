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
    public partial class ServerBehaviour : System.Web.UI.Page
    {
        decimal rating = 10.0m;
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

            GetServerBehaviour();

            var sb = new System.Text.StringBuilder();
            ServerBehaviourSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["ServerBehaviour"] = htmlstring;
        }

        private void GetServerBehaviour()
        {
            var site = Session["MainUrl"].ToString();
            var message = "";

            message += Get404Message(site);
            message += GetGzipMessage(site);
            message += GetRedirectMessage(site);
            message += GetPageSpeedInisghts(site);
            message += GetHttpsMessage(site);
            message += GetServerTypeMessage(site);

            ServerBehaviourResults.InnerHtml = message;

            if (rating == 10.0m)
                rating = 10m;
            if (rating < 0m)
                rating = 0.0m;

            decimal rounded = decimal.Round(rating, 1);
            ServerBehaviourRating.InnerHtml = rounded.ToString();

            var temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = rounded + temp;

            temp = (decimal)Session["RatingTech"];
            Session["RatingTech"] = rounded + temp;

            Session["ServerBehaviourRating"] = rounded;
            SetRatingDisplay(rating);
        }

        /// <summary>
        /// Get a message displaying the handling of missing pages
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private string Get404Message(string site)
        {
            // http://netvantagemarketing.com/?page_id=7/whateverwhatever geeft geen 404
            var result = "";
            var icon = "";

            if (Handles404(site))
            {
                icon = "fa-check";
                result = "De server geeft een 404 HTTP code wanneer een pagina niet gevonden kan worden.<br/>Dit is goed om meerdere redenen.<br/>"
                    + "Één van deze redenen is dat een website op deze manier duidelijk kan maken aan de gebruiker dat de website niet bestaat.<br/>"
                    + "Verder wordt een pagina die 404 als antwoord geeft niet geïndexeerd door zoekmachine's waardoor er geen ongewenste resultaten verschijnen bij een zoekopdracht.";
            }
            else
            {
                rating = rating - 2m;
                icon = "fa-times";
                result = "De server geeft geen 404 terug wanneer een pagina niet kan worden gevonden.<br/>Dit is niet goed om meerdere redenen.<br/>"
                    + "Één van deze redenen is dat een website op deze manier niet duidelijk kan maken dat een website niet bestaat.<br/>"
                    + "Verder wordt een pagina die 404 als antwoord geeft niet geïndexeerd door zoekmachine's waardoor er geen ongewenste resultaten verschijnen bij een zoekopdracht.";
            }

            return "<div class='resultBox-12 row'><div class='col-xs-2 col-sm-2 col-md-2 col-lg-2 text-center'>"
                    + "<span class='fa-stack fa-3x'>"
                    + "<i class='fa " + icon + " fa-stack-2x'></i>"
                    + "<span class='fa-stack-1x noselect'>404</span>"
                    + "</span></div>"
                    + "<span class='col-xs-10 col-sm-10 col-md-10 col-lg-10'>"
                    + result
                    + "</span></div>";
        }

        /// <summary>
        /// Check if a non-existing page returns a 404 httpcode
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private bool Handles404(string site)
        {
            if (!site.EndsWith("/"))
                site = site + "/";

            var rnd = new Random();
            var randomnumber = rnd.Next(10000, 100000);
            site = site + randomnumber;

            System.Diagnostics.Debug.WriteLine("404 testen van: " + site);

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(site);
                request.UserAgent = Session["userAgent"].ToString();
                request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex)
            {
                System.Diagnostics.Debug.WriteLine("wex.Message: " + wex.Message);
                System.Diagnostics.Debug.WriteLine("wex.InnerException: " + wex.InnerException);
                if (wex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = wex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        if ((int)response.StatusCode == 404)
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Get a message displaying the handling of GZIP compression
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private string GetGzipMessage(string site)
        {
            // http://www.platformhoogeveen.nl gebruikt geen GZIP

            var result = "";
            var icon = "";

            if (IsGzip(site))
            {
                icon = "fa-check";
                result = "De server ondersteunt GZIP compressie. Dit is goed doordat dit de snelheid van de website verbeterd.";
            }
            else
            {
                rating = rating -2m;
                icon = "fa-times";
                result = "De server ondersteunt geen GZIP compressie. Dit is niet goed doordat GZIP compressie de snelheid van een website kan verbeteren.";
            }

            return "<div class='resultBox-12 row'><div class='col-xs-2 col-sm-2 col-md-2 col-lg-2 text-center'>"
                    + "<span class='fa-stack fa-3x'>"
                    + "<i class='fa " + icon + " fa-stack-2x'></i>"
                    + "<i class='fa fa-file-archive-o fa-stack-1x'></i>"
                    + "</span></div>"
                    + "<span class='col-xs-10 col-sm-10 col-md-10 col-lg-10'>"
                    + result
                    + "</span></div>";
        }

        /// <summary>
        /// Check if server accepts GZIP header
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private bool IsGzip(string site)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(site);
            request.Headers.Add("Accept-Encoding", "gzip");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.Headers["Content-Encoding"] != null)
            {
                if (response.Headers["Content-Encoding"].Contains("gzip"))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get a message displaying the handling of redirection when not using www.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private string GetRedirectMessage(string site)
        {
            var after = site;
            var before = site;

            if (site.Contains("/www."))
                before = site.Replace("/www.", "/");
            else if (site.Contains("http://"))
                after = site.Replace("http://", "http://www.");
            else if (site.Contains("https://"))
                after = site.Replace("https://", "https://www.");

            var result = "";
            var icon = "";

            if (RedirectsWww(site))
            {
                icon = "fa-check";
                result = "Er is een permanente (HTTP 301) doorverwijzing ingesteld van " + before + " naar " + after + "."
                    + "<br/>Dit is goed doordat dit beter wordt gewaardeerd door zoekmachines. Beide versies worden namelijk beschouwd als verschillende websites wanneer dit niet wordt gedaan.";
            }
            else
            {
                rating = rating - 2m;
                icon = "fa-times";
                result = "Er is geen permanente (HTTP 301) doorverwijzing ingesteld van " + before + " naar " + after + "."
                    + "<br/>Dit is slecht doordat beide versies beschouwd worden als verschillende websites en zoekmachines dit minder goed waarderen.";
            }

            return "<div class='resultBox-12 row'><div class='col-xs-2 col-sm-2 col-md-2 col-lg-2 text-center'>"
                    + "<span class='fa-stack fa-3x'>"
                    + "<i class='fa " + icon + " fa-stack-2x'></i>"
                    + "<i class='fa fa-reply fa-stack-1x'></i>"
                    + "</span></div>"
                    + "<span class='col-xs-10 col-sm-10 col-md-10 col-lg-10'>"
                    + result
                    + "</span></div>";
        }

        /// <summary>
        /// Check if there is a redirect when not using www. in address.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private bool RedirectsWww(string site)
        {
            if (site.Contains("/www."))
                site = site.Replace("/www.", "/");

            System.Diagnostics.Debug.WriteLine("301 testen van: " + site);

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(site);
                request.AllowAutoRedirect = false;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if ((int)response.StatusCode == 301)
                    return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Get a message displaying the type of the hosting server
        /// </summary>
        /// <param name="site"></param>
        /// <returns>string message</returns>
        private string GetServerTypeMessage(string site)
        {
            var result = "";
            var serverType = GetServerType(site);

            if (serverType.Length > 0)
            {
                result = "Server is van type: " + serverType + ".";
            }
            else
            {
                result = "Servertype niet gevonden.";
            }

            return "<div class='resultBox-12 row'><div class='col-xs-2 col-sm-2 col-md-2 col-lg-2 text-center'>"
                    + "<span class='fa-stack fa-3x'>"
                    + "<i class='fa fa-info-circle fa-stack-2x'></i>"
                    + "<i class='fa fa-server fa-stack-1x'></i>"
                    + "</span></div>"
                    + "<span class='col-xs-10 col-sm-10 col-md-10 col-lg-10'>"
                    + result
                    + "</span></div>";
        }

        /// <summary>
        /// Get the type of hosting server
        /// </summary>
        /// <param name="site"></param>
        /// <returns>string type</returns>
        private string GetServerType(string site)
        {
            var type = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(site);
            request.Headers.Add("Accept-Encoding", "gzip");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.Headers["Server"] != null)
            {
                type = response.Headers["Server"].ToString();
            }

            return type;
        }

        /// <summary>
        /// Get a message displaying the speed-score via Google's pageSpeed Insights
        /// </summary>
        /// <param name="site"></param>
        /// <returns>string message</returns>
        private string GetPageSpeedInisghts(string site)
        {
            var result = "";
            var apikey = System.Web.Configuration.WebConfigurationManager.AppSettings["GoogleAPI"];

            var requestString = "https://www.googleapis.com/pagespeedonline/v2/runPagespeed?url=" + site + "&key=" + apikey;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestString);
            request.UserAgent = Session["userAgent"].ToString();
            request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            JObject PageSpeedInsightJson = JObject.Parse(responseFromServer);

            var score = PageSpeedInsightJson["ruleGroups"]["SPEED"]["score"].ToString();

            result += "Google's PageSpeed Insights geeft deze website een score van <span class='emphasis'>" + score + "/100</span>.<br/><a href='https://developers.google.com/speed/pagespeed/insights/?url=" + site + "&tab=desktop' target='_blank'>Klik hier voor een volledig verslag</a>";

            var icon = "fa-check";

            if (Int32.Parse(score) < 55)
                icon = "fa-times";

            rating = rating - (2m - decimal.Parse(score) / 100m * 2m);

            return "<div class='resultBox-12 row'><div class='col-xs-2 col-sm-2 col-md-2 col-lg-2 text-center'>"
                    + "<span class='fa-stack fa-3x'>"
                    + "<i class='fa " + icon + " fa-stack-2x'></i>"
                    + "<i class='fa fa-google fa-stack-1x'></i>"
                    + "</span></div>"
                    + "<span class='col-xs-10 col-sm-10 col-md-10 col-lg-10'>"
                    + result
                    + "</span></div>";
        }

        /// <summary>
        /// Get a message displaying the speed-score via Google's pageSpeed Insights
        /// </summary>
        /// <param name="site"></param>
        /// <returns>string message</returns>
        private string GetHttpsMessage(string site)
        {
            var isHttps = false;
            var result = "";
            if (site.Contains("https:"))
                isHttps = true;
            else
            {
                var requestString = site.Replace("http:", "https:");

                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestString);
                    request.UserAgent = Session["userAgent"].ToString();
                    request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    Debug.WriteLine("GetHttpsMessage-Response.StatusCode" + response.StatusCode);
                    Debug.WriteLine("GetHttpsMessage-(int)Response.StatusCode" + (int)response.StatusCode);

                    if ((int)response.StatusCode == 200)
                        isHttps = true;
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.TrustFailure)
                        isHttps = false;
                }

            }

            var icon = "fa-check";
            // HTTPS faster loading: https://www.httpvshttps.com/
            if (!isHttps)
            {
                icon = "fa-times";
                rating = rating - 2m;
                result = "Deze website ondersteunt geen HTTPS. Dit is slecht.<br/>Veiligheid wordt steeds belangrijker en Google geeft zelfs voorrang aan websites die HTTPS ondersteunen.";
            }
            else
                result = "Deze website ondersteunt HTTPS. Dit is uitstekend.<br/>Veiligheid wordt steeds belangrijker en Google geeft zelfs voorrang aan websites die HTTPS ondersteunen.";

            return "<div class='resultBox-12 row'><div class='col-xs-2 col-sm-2 col-md-2 col-lg-2 text-center'>"
                    + "<span class='fa-stack fa-3x'>"
                    + "<i class='fa " + icon + " fa-stack-2x'></i>"
                    + "<i class='fa fa-lock fa-stack-1x'></i>"
                    + "</span></div>"
                    + "<span class='col-xs-10 col-sm-10 col-md-10 col-lg-10'>"
                    + result
                    + "</span></div>";
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            var element = ServerBehaviourRating;

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
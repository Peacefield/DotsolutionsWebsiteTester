using System;
using System.Collections.Generic;
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
            // TODO: Invullen Uitleg waarom dit niet goed is
            // http://netvantagemarketing.com/?page_id=7/whateverwhatever geeft geen 404

            var message = "<div class='resultBox-12 row'><div class='col-xs-2 col-sm-2 col-md-2 col-lg-2 text-center'>";
            if (Handles404(site))
                message += "<span class='fa-stack fa-2x'>"
                    + "<i class='fa fa-check fa-stack-2x'></i>"
                    + "<span class='fa-stack-1x noselect'>404</span>"
                    + "</span></div>"
                    + "<span class='col-xs-10 col-sm-10 col-md-10 col-lg-10'>De server geeft een 404 HTTP code wanneer een pagina niet gevonden kan worden. Dit is goed om meerdere redenen.<br/>"
                    + "Één van deze redenen is dat een website op deze manier duidelijk kan maken aan de gebruiker dat de website niet bestaat.<br/>"
                    + "Verder wordt een pagina die 404 als antwoord geeft niet geïndexeerd door zoekmachine's waardoor er geen ongewenste resultaten verschijnen bij een zoekopdracht.</span>";
            else
            {
                rating = rating - 3.3m;
                message += "<span class='fa-stack fa-2x'>"
                    + "<i class='fa fa-times fa-stack-2x'></i>"
                    + "<span class='fa-stack-1x'>404</span>"
                    + "</span></div>"
                    + "<span class='col-xs-10 col-sm-10 col-md-10 col-lg-10'>De server geeft geen 404 terug wanneer een pagina niet kan worden gevonden. Dit is niet goed om meerdere redenen.<br/>"
                    + "Één van deze redenen is dat een website op deze manier niet duidelijk kan maken dat een website niet bestaat.<br/>"
                    + "Verder wordt een pagina die 404 als antwoord geeft niet geïndexeerd door zoekmachine's waardoor er geen ongewenste resultaten verschijnen bij een zoekopdracht.</span>";
            }
            message += "</div>";

            return message;
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
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex)
            {
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
            var message = "<div class='resultBox-12 row'><i class='fa fa-file-archive-o fa-3x col-xs-2 col-sm-2 col-md-2 col-lg-2 text-center'></i>";
            message += "<span class='col-xs-10 col-sm-10 col-md-10 col-lg-10'>";
            if (IsGzip(site))
                message += "De server ondersteund GZIP compressie. Dit is goed doordat dit de snelheid van de website verbeterd.";
            else
            {
                rating = rating - 3.3m;
                message += "De server ondersteund geen GZIP compressie. Dit is niet goed doordat GZIP compressie de snelheid van een website kan verbeteren.";
            }

            message += "</span></div>";

            return message;
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
            var message = "<div class='resultBox-12 row'><i class='fa fa-reply fa-3x col-xs-2 col-sm-2 col-md-2 col-lg-2 text-center'></i>";
            message += "<span class='col-xs-10 col-sm-10 col-md-10 col-lg-10'>";
            if (RedirectsWww(site))
                message += "Er is een 301 verwijzing ingesteld voor het invoeren van het adres zonder www. "
                    +"Dit is goed doordat dit beter wordt gewaardeerd door zoekmachines. De versies met- en zonder www. worden namelijk beschouwd als verschillende websites.";
            else
            {
                rating = rating - 3.3m;
                message += "Er is geen 301 verwijzing ingesteld voor het invoeren van het adres zonder www. "
                    + "Dit is slecht doordat de versies met- en zonder www. beschouwd worden als verschillende websites en zoekmachines dit minder goed waarderen..";
            }

            message += "</span></div>";

            return message;
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
            var message = "<div class='resultBox-12 row'><i class='fa fa-server fa-3x col-xs-2 col-sm-2 col-md-2 col-lg-2 text-center'></i>";
            var serverType = GetServerType(site);

            message += "<span class='col-xs-10 col-sm-10 col-md-10 col-lg-10'>";
            if (serverType.Length > 0)
                message += "Server is van type: " + serverType;
            else
            {
                message += "Servertype niet gevonden";
            }

            message += "</span></div>";

            return message;
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
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating < 6m)
                ServerBehaviourRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                ServerBehaviourRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                ServerBehaviourRating.Attributes.Add("class", "excellentScore ratingCircle");
        }
    }
}
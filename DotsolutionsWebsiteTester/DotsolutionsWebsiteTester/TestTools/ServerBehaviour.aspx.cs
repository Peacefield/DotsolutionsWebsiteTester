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
            var rating = 10.0m;
            var message = "";

            message += site + " aan het testen.<hr/>";

            message += GetGzipMessage(site);
            message += Get404Message(site);
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
            var message = "";
            if (Handles404(site))
                message += "Vangt missende pagina's correct af door een 404 http code te retourneren.<br/>";
            else
                message += "Vangt missende pagina's niet correct af. Missende pagina's moeten een 404 http code retourneren.<br/>";

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
            var message = "";
            if (IsGzip(site))
                message += "GZIP gedetecteerd...<br/>";
            else
                message += "Geen GZIP gedetecteerd...<br/>";

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
            var message = "";
            if (RedirectsWww(site))
                message += "adres zonder www. verwijst met 301 naar adres met www.<br/>";
            else
                message += "adres zonder www. verwijst niet met 301 naar adres met www.<br/>";

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
            var message = "";
            var serverType = GetServerType(site);
            if (serverType.Length > 0)
                message += "Server is van type: " + serverType + "<br/>";
            else
                message += "Servertype niet gevonden<br/>";

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
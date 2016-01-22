using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class MobileCompatibility : System.Web.UI.Page
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

            var ths = new ThreadStart(() => GetMC());
            var th = new Thread(ths);
            th.Start();

            th.Join();
            var sb = new System.Text.StringBuilder();
            MobileCompatibilitySession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["MobileCompatibility"] = htmlstring;
        }

        /// <summary>
        /// Get Mobile Compatibilit rating from pages in Session["selectedSites"] by performing multiple tests
        /// </summary>
        private void GetMC()
        {
            // Per pagina die niet compatibel is met mobiel wordt 1/{aantal geteste pagina's} * 10 afgetrokken van het huidige totaal

            Debug.WriteLine("MobileCompatibility >>>>>");
            var compatible = new List<string>();
            var notCompatiblePage = new List<string>();
            var sitemap = (List<string>)Session["selectedSites"];
            var rating = 10.0m;
            var message = "";
            var isDetailed = (bool)Session["IsDetailedTest"];

            SetPreviewImages();

            foreach (var url in sitemap)
            {
                var webget = new HtmlWeb();
                var doc = webget.Load(url);

                if (HasNoCss(doc))
                {
                    if (!HasMobileRedirect(url))
                        notCompatiblePage.Add(url);
                }
                else if (HasMobileCssSheet(doc) || HasMobileRedirect(url) || HasMobileCssQuery(doc))
                {
                    compatible.Add(url);
                }
                else
                {
                    notCompatiblePage.Add(url);
                }
            }

            if (notCompatiblePage.Count > 0)
            {
                string notcompatiblelist = "";
                foreach (var item in notCompatiblePage)
                {
                    rating = rating - (10.0m / (decimal)sitemap.Count);
                    notcompatiblelist += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";
                }
                string amount = "";
                if (notCompatiblePage.Count > 1)
                    amount = "bevatten " + notCompatiblePage.Count + " pagina's";
                else
                    amount = "bevat " + notCompatiblePage.Count + " pagina";

                // Geen rekening gehouden op sommige pagina's
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Van de " + sitemap.Count + " geteste pagina's " + amount + " geen CSS die rekening houdt met de mobiele compatibiliteit:"
                    + "<ul>" + notcompatiblelist + "</ul>Dit is slecht doordat een pagina ten alle tijden geschikt moet zijn om zonder moeite te bezoeken.</span></div>";
            }
            else
            {
                    message += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                         + "<i class='glyphicon glyphicon-ok glyphicons-lg messageIcon'></i>"
                         + "<span class='messageText'> Er is rekening gehouden met de mobiele compatibiliteit op alle geteste pagina's. Dit is uitstekend door de website ten alle tijden te bezoeken is, vanuit een breed scala aan apparaten.</span></div>";
            }

            MobileCompatibilityResults.InnerHtml = message;

            if (rating == 10.0m)
                rating = 10m;
            var rounded = decimal.Round(rating, 1);
            Session["MobileCompatibilityRating"] = rounded;
            MobileCompatibilityRating.InnerHtml = rounded.ToString();

            var temp = (decimal)Session["RatingAccess"];
            Session["RatingAccess"] = temp + rounded;

            temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = temp + rounded;

            temp = (decimal)Session["RatingTech"];
            Session["RatingTech"] = temp + rounded;
            SetRatingDisplay(rating);
        }

        /// <summary>
        /// Add preview images to the page of tablet and smartphone viewings using Screenshot Machine
        /// </summary>
        private void SetPreviewImages()
        {
            // Payed services with 100 free unique requests per month, but WITH mobile resolution ability
            // https://www.screenshotmachine.com/
            var ApiKey = System.Web.Configuration.WebConfigurationManager.AppSettings["ScreenshotMachine"];
            var format = "PNG";
            var url = HttpUtility.UrlEncode(Session["mainUrl"].ToString());
            var size = "Nmob";
            var imgUrlMobile = "http://api.screenshotmachine.com/?key=" + ApiKey + "&size=" + size + "&format=" + format + "&url=" + url;
            size = "N";
            var imgUrlTablet = "http://api.screenshotmachine.com/?key=" + ApiKey + "&size=" + size + "&format=" + format + "&url=" + url;

            //var imgUrlTablet = "http://i.imgur.com/PtcoFun.png";
            //var imgUrlMobile = "http://i.imgur.com/8UIGhLL.png";

            tabletImg.InnerHtml = "<img width='400' height='300' class='tabletcontainer center-block' src='" + imgUrlTablet + "' title='Tablet preview " + Session["MainUrl"].ToString() + "' alt='Tablet preview " + Session["MainUrl"].ToString() + "'/>";
            mobileImg.InnerHtml = "<img width='480' height='800' class='mobilecontainer center-block' src='" + imgUrlMobile + "' title='Smartphone preview " + Session["MainUrl"].ToString() + "' alt='Smartphone preview " + Session["MainUrl"].ToString() + "'/>";
        }

        /// <summary>
        /// Check if page has no CSS
        /// </summary>
        /// <param name="doc">HtmlDocument page</param>
        /// <returns>true if page has no CSS</returns>
        private bool HasNoCss(HtmlDocument doc)
        {
            var boolean = false;
            if (doc.DocumentNode.SelectNodes("//link[@rel]") == null)
            {
                boolean = true;

                Debug.WriteLine("HasNoCss true");
            }
            return boolean;
        }

        /// <summary>
        /// Check if page has stylesheet dedicated to mobile viewing
        /// </summary>
        /// <param name="doc">HtmlDocument page</param>
        /// <returns>true if page has stylesheet dedicated to mobile viewing</returns>
        private bool HasMobileCssSheet(HtmlDocument doc)
        {
            var boolean = false;
            if (doc.DocumentNode.SelectNodes("//link[@media]") != null)
            {
                foreach (var node in doc.DocumentNode.SelectNodes("//link[@media]"))
                {
                    Debug.WriteLine("stylesheet in html: " + node.Attributes["media"].Value);
                    if (node.Attributes["media"].Value.Contains("max-width") || node.Attributes["media"].Value.Contains("min-width"))
                    {
                        boolean = true;

                        Debug.WriteLine("HasMobileCssSheet true");
                    }
                }
            }
            return boolean;
        }

        /// <summary>
        /// Check if page redirects
        /// </summary>
        /// <param name="url">String url page</param>
        /// <returns>true if page redirects</returns>
        private bool HasMobileRedirect(string url)
        {
            var boolean = false;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = Session["userAgent"].ToString();
                request.Method = "HEAD";
                request.AllowAutoRedirect = false;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Debug.WriteLine("response.StatusCode: " + response.StatusCode);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    boolean = true;
                    Debug.WriteLine("HasMobileRedirect true");
                }
            }
            catch (WebException we)
            {
                Debug.WriteLine("we.Message: " + we.Message);
                if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        request.UserAgent = Session["userAgent"].ToString();
                        request.Method = "GET";
                        request.AllowAutoRedirect = false;

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        Debug.WriteLine("response.StatusCode: " + response.StatusCode);
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            boolean = true;
                            Debug.WriteLine("HasMobileRedirect true");
                        }
                    }
                    catch (WebException we2)
                    {
                        Debug.WriteLine("we2.Message: " + we2.Message);
                    }
                    catch (Exception e2)
                    {
                        Debug.WriteLine("e2.Message: " + e2.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("e.Message: " + e.Message);
            }

            return boolean;
        }

        /// <summary>
        /// Check if stylesheet of a page contains @media queries
        /// </summary>
        /// <param name="doc">HtmlDocument page</param>
        /// <returns>True if page has stylesheet that uses @media queries</returns>
        private bool HasMobileCssQuery(HtmlDocument doc)
        {
            var boolean = false;
            var cssList = GetCssList(doc);
            foreach (var stylesheet in cssList)
            {
                Debug.WriteLine("stylesheet: " + stylesheet);
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(stylesheet);
                    request.UserAgent = Session["userAgent"].ToString();
                    request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");
                    request.Credentials = CredentialCache.DefaultCredentials;

                    // Get the response.
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    // Get the stream containing content returned by the server.
                    Stream dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    var reader = new StreamReader(dataStream);
                    // Read the content. 
                    string responseFromServer = reader.ReadToEnd();

                    if (responseFromServer.Contains("@media screen and") || responseFromServer.Contains("@media only screen and")
                        || responseFromServer.Contains("@media (min-") || responseFromServer.Contains("@media (max-"))
                    {
                        boolean = true;
                        Debug.WriteLine("HasMobileCssQuery true");
                        break;
                    }
                }
                catch (WebException)
                {
                    Debug.WriteLine("Could not fetch CSS");
                }
            }
            return boolean;
        }

        /// <summary>
        /// Get a list of used stylesheet on a page
        /// </summary>
        /// <param name="doc">HtmlDocument page</param>
        /// <returns>String list of stylesheets used on the page</returns>
        private List<string> GetCssList(HtmlDocument doc)
        {
            var cssList = new List<string>();
            if (doc.DocumentNode.SelectNodes("//link[@rel]") != null)
            {
                foreach (var node in doc.DocumentNode.SelectNodes("//link[@rel]"))
                {
                    var stylesheet = "";
                    if (node.Attributes["rel"].Value == "stylesheet")
                    {
                        string hrefstring = node.Attributes["href"].Value;

                        Uri uri = new Uri(Session["MainUrl"].ToString());
                        string scheme = uri.Scheme;
                        string host = uri.Host;
                        string baseUrl = scheme + "://" + host + "/";

                        if (hrefstring.Contains("//"))
                        {
                            if (hrefstring.Contains("https://") || hrefstring.Contains("http://"))
                            {
                                stylesheet = node.Attributes["href"].Value;
                            }
                            else if (!hrefstring.Contains("https://") && !hrefstring.Contains("http://"))
                            {
                                try
                                {
                                    Debug.WriteLine("Proberen in GetCssList ---> " + hrefstring);
                                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(hrefstring);
                                    stylesheet = hrefstring;
                                }
                                catch (InvalidCastException icex)
                                {
                                    Debug.WriteLine("InvalidCastException in GetCssList ---> " + icex.Message);
                                    //add baseurl since it apparently is not external
                                    stylesheet = hrefstring.Replace("//", scheme + "://");
                                    Debug.WriteLine("stylesheet in GetCssList ---> " + stylesheet);
                                }
                            }
                        }
                        else
                        {
                            stylesheet = baseUrl + hrefstring;
                        }

                        if (!cssList.Contains(stylesheet) && stylesheet.Length > 0)
                        {
                            cssList.Add(stylesheet);
                        }
                    }
                }
            }
            return cssList;
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            var element = MobileCompatibilityRating;

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
using HtmlAgilityPack;
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

        private void GetMC()
        {
            Debug.WriteLine("MobileCompatibility >>>>>");
            var compatible = new List<string>();
            var notCompatiblePage = new List<string>();
            var sitemap = (List<string>)Session["selectedSites"];
            var rating = 10m;

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
                MobileCompatibilityResults.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg'></i>"
                    + "<span> Van de " + sitemap.Count + " geteste pagina's " + amount + " geen CSS die rekening houdt met de mobiele compatibiliteit:</span>"
                    + "<ul>" + notcompatiblelist + "</ul></div>";
            }
            else
            {
                MobileCompatibilityResults.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span> Er is rekening gehouden met de mobiele compatibiliteit op alle geteste pagina's</span></div>";
            }

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

            SetPreviewImages();
        }

        private void SetPreviewImages()
        {
            // Free service without mobile resolutionn ability
            // http://www.shrinktheweb.com/
            // http://www.shrinktheweb.com/auth/stw-lobby
            //var accesskeyid = GetFromApiKeys("ShrinkTheWeb");
            //var imgUrl = "http://images.shrinktheweb.com/xino.php?"
            //    + "stwembed=1"
            //    + "&stwaccesskeyid= " + accesskeyid
            //    + "&stwinside=1"
            //    + "&stwsize=xlg"
            //    + "&stwurl=" + Session["MainUrl"].ToString();
            
            // Payed services with 100 free unique requests per month, but WITH mobile resolution ability
            // https://www.screenshotmachine.com/
            //var ApiKey = GetFromApiKeys("ScreenshotMachine");
            //var format = "PNG";
            //var url = HttpUtility.UrlEncode(Session["mainUrl"].ToString());
            //var size = "Nmob";
            //var imgUrlMobile = "http://api.screenshotmachine.com/?key=" + ApiKey + "&size=" + size + "&format=" + format + "&url=" + url;
            //size = "N";
            //var imgUrlComputer = "http://api.screenshotmachine.com/?key=" + ApiKey + "&size=" + size + "&format=" + format + "&url=" + url;

            var imgUrlComputer = "http://i.imgur.com/PtcoFun.png";
            var imgUrlMobile = "http://i.imgur.com/8UIGhLL.png";

            computerImg.InnerHtml = "<img width='400' height='300' class='computercontainer center-block' src='" + imgUrlComputer + "' title='Laptop preview " + Session["MainUrl"].ToString() + "' alt='Laptop preview " + Session["MainUrl"].ToString() + "'/>";
            mobileImg.InnerHtml = "<img width='480' height='800' class='mobilecontainer center-block' src='" + imgUrlMobile + "' title='Smartphone preview " + Session["MainUrl"].ToString() + "' alt='Smartphone preview " + Session["MainUrl"].ToString() + "'/>";
        }
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
            if (rating < 6m)
                MobileCompatibilityRating.Attributes.Add("class", "lowScore ratingCircle");
            else if (rating < 8.5m)
                MobileCompatibilityRating.Attributes.Add("class", "mediocreScore ratingCircle");
            else
                MobileCompatibilityRating.Attributes.Add("class", "excellentScore ratingCircle");
        }

        private string GetFromApiKeys(string key)
        {
            var list = (List<KeyValuePair<string, string>>)Session["ApiKeys"];
            foreach (var element in list)
                if (element.Key == key)
                    return element.Value;
            return "";
        }
    }
}
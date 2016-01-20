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
    public partial class Images : System.Web.UI.Page
    {
        private decimal rating = 10.0m;

        private int missingSize = 0;
        private int missingDesc = 0;
        private int imgDeclare = 0;
        private int imgResized = 0;
        private int img404Count = 0;

        private List<string> imgNotFoundList = new List<string>();

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

            GetImages();

            try
            {
                var sb = new System.Text.StringBuilder();
                ImagesSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
                var htmlstring = sb.ToString();

                Session["Images"] = htmlstring;
            }
            catch (NullReferenceException nex)
            {
                Debug.WriteLine("NullReferenceException --> " + nex.Message);
            }
        }

        /// <summary>
        /// Initiate tests to get image results and rating
        /// </summary>
        private void GetImages()
        {
            // Beoordeling is afhankelijk van correcte declaratie en werking
            // De formule hiervoor per pagina is {huidige beoordeling} - ({aantal foutieve afbeeldingen}/{totaal aantal afbeeldingen op pagina} * 10)

            var sitemap = (List<string>)Session["selectedSites"];
            var isDetailed = (bool)Session["IsDetailedTest"];
            var message = "";

            var totalimages = 0;
            foreach (var page in sitemap)
            {
                missingDesc = 0;
                missingSize = 0;
                imgResized = 0;
                imgNotFoundList.Clear();

                Debug.WriteLine(" ---------- Testen op: " + page + " ---------- ");
                var imagelist = GetAllImages(page);

                var threadpool = new List<Thread>();
                foreach (var imageNode in imagelist)
                {
                    var ths = new ThreadStart(() => TestImage(imageNode, page, imagelist.Count));
                    var th = new Thread(ths);
                    threadpool.Add(th);
                    th.Start();
                }

                foreach (var thread in threadpool)
                    thread.Join();

                Debug.WriteLine("imagelist.Count " + imagelist.Count);
                totalimages += imagelist.Count;

                //if (missingDesc > 4)
                //{
                //    AddToTable(page, "---", "<strong>" + (missingDesc - 4) + " overige afbeeldingen gevonden zonder alt en/of title attributen.</strong>");
                //}
                //if (missingSize > 4)
                //{
                //    AddToTable(page, "---", "<strong>" + (missingSize - 4) + " overige afbeeldingen gevonden zonder height en/of width  attributen.</strong>");
                //}
                //if (imgResized > 4)
                //{
                //    AddToTable(page, "---", "<strong>" + (imgResized - 4) + " overige afbeeldingen gevonden waarvan de in HTML gedeclareerde grootte niet overeen komt met de originele grootte van de afbeelding.</strong>");
                //}
                //if (img404Count > 4)
                //{
                //    AddToTable(page, "---", "<strong>" + (img404Count - 4) + " overige afbeeldingen niet gevonden.</strong>");
                //}
            }

            if (rating == 10.0m)
                rating = 10m;
            if (rating < 0m)
                rating = 0.0m;

            if (isDetailed && (missingDesc > 0 || missingSize > 0 || imgResized > 0 || img404Count > 0))
                ImagesTableHidden.Attributes.Remove("class");

            if (!isDetailed)
                ImagesTable.Rows.Clear();

            var percentageDeclared = 0m;
            var percentageStretched = 0m;

            if (totalimages > 0)
            {
                Debug.WriteLine("totalimages: " + totalimages);
                percentageDeclared = (decimal)imgDeclare / (decimal)totalimages * 100m;
                percentageStretched = (decimal)imgResized / (decimal)totalimages * 100m;
            }

            message = "<div class='well well-lg resultWell text-center'>"
                + "<span class='largetext'>" + percentageDeclared.ToString("#,0") + "%</span><br/>"
                + "<span>van de afbeelding is incorrect gedefinieerd</span></div>"
                + "<div class='resultDivider'></div>"
                + "<div class='well well-lg resultWell text-center'>"
                + "<i class='fa fa-picture-o fa-3x'></i><br/>"
                + "<span>" + percentageStretched.ToString("#,0") + "% van de afbeelding worden vervormd door de browser</span></div>";

            message += GetMessage();

            ImagesMessages.InnerHtml = message;

            var rounded = decimal.Round(rating, 1);
            Session["ImagesRating"] = rounded;
            ImagesRating.InnerHtml = rounded.ToString();
            SetRatingDisplay(rounded);
            // Set sessions
            var temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = temp + rounded;

            temp = (decimal)Session["RatingTech"];
            Session["RatingTech"] = temp + rounded;
        }

        /// <summary>
        /// Set messages that will be shown to explain found errors
        /// </summary>
        /// <returns>string temp</returns>
        private string GetMessage()
        {
            var temp = "";

            if (imgDeclare > 0)
            {
                string[] grammar = { "zijn " + imgDeclare + " afbeeldingen", "zijn" };
                if (imgDeclare == 1)
                {
                    grammar[0] = "is " + imgDeclare + " afbeelding";
                    grammar[1] = "is";
                }

                temp += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                   + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                   + "<span class='messageText'> Er " + grammar[0] + " gevonden die niet goed gedeclareerd " + grammar[1] + ". "
                   + "Dit betekent dat een afbeelding geen height, width en/of alt attribuut bevat.</span></div>";
            }

            if (imgResized > 0)
            {
                string[] grammar = { "zijn " + imgResized + " afbeeldingen", "bestaan" };
                if (imgResized == 1)
                {
                    grammar[0] = "is " + imgResized + " afbeelding";
                    grammar[1] = "wordt";
                }
                temp += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                   + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                   + "<span class='messageText'> Er " + grammar[0] + " gevonden die " + grammar[1] + " vervormd door de browser. "
                   + "Dit wordt veroorzaakt doordat de height en width attributen niet overeen komen met de originele grootte van de afbeelding.</span></div>";
            }

            if (img404Count > 0)
            {
                string[] grammar = { "zijn " + img404Count + " afbeeldingen", "bestaan" };
                if (img404Count == 1)
                {
                    grammar[0] = "is " + img404Count + " afbeelding";
                    grammar[1] = "bestaat";
                }
                temp += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                   + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                   + "<span class='messageText'> Er " + grammar[0] + " gevonden die niet (meer) " + grammar[1] + ". "
                   + "Dit kan worden veroorzaakt door een typefout of doordat de afbeelding verwijderd is van de server.</span></div>";
            }

            return temp;
        }

        /// <summary>
        /// Test an image found as HtmlNode on responsecode, size-declaration and alt-declaration
        /// </summary>
        /// <param name="imageNode">HtmlNode image</param>
        /// <param name="page">string page of origin</param>
        /// <param name="imagelistCount">Total amount of images on the page, used to calculate rating</param>
        private void TestImage(HtmlNode imageNode, string page, int imagelistCount)
        {
            Debug.WriteLine(" --------------- " + imageNode.Attributes["src"].Value + " testen op " + page + " --------------- ");

            var sitemap = (List<string>)Session["selectedSites"];
            var baseUrl = Session["MainUrl"].ToString();
            var imageUrl = imageNode.Attributes["src"].Value;

            if (imageUrl.StartsWith(" "))
                imageUrl = imageUrl.Remove(0, 1);
            // Untestable image format, will always work correctly
            if (imageUrl.StartsWith("data:image"))
                return;
            if (imageUrl.StartsWith("//"))
                imageUrl = "http:" + imageUrl;
            else
            {
                if (baseUrl.EndsWith("/") && imageUrl.StartsWith("/"))
                    baseUrl = baseUrl.Remove(baseUrl.Length - 1);
                if (!baseUrl.EndsWith("/") && !imageUrl.StartsWith("/"))
                    baseUrl = baseUrl + "/";
                if (!imageUrl.Contains("http"))
                    imageUrl = baseUrl + imageUrl;
            }
            if (IsImageFound(imageUrl))
            {
                var imgFaultyDeclare = false;
                if (!HasImgSizeAttributes(imageNode))
                {
                    imgFaultyDeclare = true;
                    //rating = rating - ((1m / (decimal)imagelistCount) * 10m);
                    rating = rating - ((1m / (decimal)imagelistCount) * (3.3m / (decimal)sitemap.Count));
                    //if (missingSize < 5)
                        AddToTable(page, "<a href='" + imageUrl + "' target='_blank'><img src='" + imageUrl + "' title='" + imageUrl + " zonder height en/of width attribuut' alt='" + imageUrl + "' class='tableImg center-block' /></a>",
                            "Geen height en/of width attributen aanwezig.");
                    missingSize++;
                }
                else
                {
                    if (!IsImageSize(imageUrl, imageNode))
                    {
                        imgFaultyDeclare = true;
                        rating = rating - ((1m / (decimal)imagelistCount) * (3.3m / (decimal)sitemap.Count));
                        //if (imgResized < 5)
                            AddToTable(page, "<a href='" + imageUrl + "' target='_blank'><img src='" + imageUrl + "' title='" + imageUrl + " gedeclareerde grootte is incorrect' alt='" + imageUrl + "' class='tableImg center-block' /></a>",
                                "In HTML gedeclareerde grootte komt niet overeen met originele grootte van de afbeelding.");
                        imgResized++;
                    }
                }

                if (!HasImgDescAttributes(imageNode))
                {
                    imgFaultyDeclare = true;
                    rating = rating - ((1m / (decimal)imagelistCount) * (3.3m / (decimal)sitemap.Count));
                    //if (missingDesc < 5)
                        //AddToTable(page, "<a href='" + imageUrl + "' target='_blank'><img src='" + imageUrl + "' title='" + imageUrl + "' alt='" + imageUrl + "' class='tableImg center-block' /></a>",
                        //    "Geen alt en/of title attributen aanwezig.");
                        AddToTable(page, "<a href='" + imageUrl + "' target='_blank'><img src='" + imageUrl + "' title='" + imageUrl + " heeft geen alt attribuut' alt='" + imageUrl + "' class='tableImg center-block' /></a>",
                            "Geen alt attribuut aanwezig.");
                    missingDesc++;
                }

                if (imgFaultyDeclare)
                    imgDeclare++;
            }
            else
            {
                if (!imgNotFoundList.Contains(imageUrl))
                {
                    imgNotFoundList.Add(imageUrl);

                    img404Count++;
                    //if (img404Count < 5)
                        AddToTable(page, imageUrl, "Afbeelding niet gevonden.");
                    rating = rating - ((1m / (decimal)imagelistCount) * 10m);

                    Debug.WriteLine("Added " + imageUrl + " to imgNotFound list which has " + imgNotFoundList.Count + " entries.");
                }
            }
        }

        /// <summary>
        /// Test if image works correctly
        /// </summary>
        /// <param name="imageUrl">URL of image</param>
        /// <returns></returns>
        private bool IsImageFound(string imageUrl)
        {
            if (imgNotFoundList.Contains(imageUrl))
                return false;

            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(imageUrl) as HttpWebRequest;
                request.UserAgent = Session["userAgent"].ToString();
                request.Timeout = 5000; // Set timout of 5 seconds so to not waste time
                request.Method = "GET";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                return true;
            }
            catch (WebException)
            {
                return false;
            }
        }

        /// <summary>
        /// Get a HtmlNode list of all images on a page
        /// </summary>
        /// <param name="page">string page</param>
        /// <returns>HtmlNode list</returns>
        private List<HtmlNode> GetAllImages(string page)
        {
            var list = new List<HtmlNode>();
            // Get images using html agility pack

            var webget = new HtmlWeb();
            var doc = webget.Load(page);

            if (doc.DocumentNode.SelectNodes("//img") != null)
            {
                foreach (var item in doc.DocumentNode.SelectNodes("//img"))
                {
                    if (item.Attributes["src"] != null)
                        list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// Check if item has width and height attributes
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool HasImgSizeAttributes(HtmlNode item)
        {
            if (item.Attributes["width"] != null && item.Attributes["height"] != null)
                if (item.Attributes["width"].Value != "" && item.Attributes["height"].Value != "")
                    return true;

            return false;
        }

        /// <summary>
        /// Check if item has an alt attribute
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool HasImgDescAttributes(HtmlNode item)
        {
            //if (item.Attributes["alt"] != null && item.Attributes["title"] != null)
            //    if (item.Attributes["alt"].Value != "" && item.Attributes["title"].Value != "")
            //        return true;

            if (item.Attributes["alt"] != null && item.Attributes["alt"].Value != "")
                    return true;

            return false;
        }

        /// <summary>
        /// Check if ImageFileName has the same size declared as original dimensions
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <param name="imageNode"></param>
        /// <returns></returns>
        private bool IsImageSize(string imageUrl, HtmlNode imageNode)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)(System.Net.HttpWebRequest.Create(imageUrl));
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                System.Drawing.Image img = System.Drawing.Image.FromStream(resp.GetResponseStream());
                resp.Close();

                var width = img.PhysicalDimension.Width;
                var height = img.PhysicalDimension.Height;
                if (width == float.Parse(imageNode.Attributes["width"].Value) && height == float.Parse(imageNode.Attributes["height"].Value))
                    return true;
            }
            catch (ArgumentException)
            {
                Debug.WriteLine("ArgumentException -> " + imageUrl);
            }
            return false;
        }

        /// <summary>
        /// Add found faulty image to table
        /// </summary>
        /// <param name="page">page of origin</param>
        /// <param name="imgUrl">image URL</param>
        /// <param name="msg">Explanation of error</param>
        private void AddToTable(string page, string imgUrl, string msg)
        {
            Debug.WriteLine("Added to table");

            var tRow = new TableRow();

            var tCellPage = new TableCell();
            tCellPage.Text = "<a href='" + page + "' target='_blank'>" + page + "</a>";
            tRow.Cells.Add(tCellPage);

            var tCellImg = new TableCell();
            //tCellImg.Text = imgUrl;
            tCellImg.Style.Add("text-align", "center");
            tCellImg.Text = imgUrl;
            tRow.Cells.Add(tCellImg);

            var tCellmsg = new TableCell();
            tCellmsg.Text = msg;
            tRow.Cells.Add(tCellmsg);

            ImagesTable.Rows.Add(tRow);
        }

        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            var element = ImagesRating;

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
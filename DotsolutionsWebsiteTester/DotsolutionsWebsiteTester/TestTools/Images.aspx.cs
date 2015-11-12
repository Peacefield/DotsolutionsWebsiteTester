using HtmlAgilityPack;
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
    public partial class Images : System.Web.UI.Page
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

            GetImages();

            var sb = new System.Text.StringBuilder();
            ImagesSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Images"] = htmlstring;
        }

        private void GetImages()
        {
            var rating = 10.0m;

            var sitemap = (List<string>)Session["selectedSites"];

            foreach (var page in sitemap)
            {
                Debug.WriteLine(" ---------- Testen op: " + page + " ---------- ");
                var imagelist = GetAllImages(page);
                foreach (var item in imagelist)
                {
                    Debug.WriteLine(" --------------- " + item.Attributes["src"].Value + " testen --------------- ");
                    if (HasImgAttributes(item))
                    {
                        Debug.WriteLine("Heeft height EN width attributen");

                        if (IsImageSize(item))
                        {
                            Debug.WriteLine("Alles is goed, niks aan de hand");
                        }
                        else
                        {
                            Debug.WriteLine("In HTML gedeclareerde grootte komt niet overeen met originele grootte van afbeelding");
                            rating = rating - ((1m / (decimal)imagelist.Count) * 5m);
                            AddToTable(page, item.Attributes["src"].Value, "In HTML gedeclareerde grootte komt niet overeen met originele grootte van afbeelding.");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Heel slecht, geen height en/of width attributen aanwezig");
                        rating = rating - ((1m / (decimal)imagelist.Count) * 10m);
                        AddToTable(page, item.Attributes["src"].Value, "Geen height en/of width attributen aanwezig.");
                    }
                }
            }

            if (rating == 10.0m)
                rating = 10m;
            if (rating < 0m)
                rating = 0.0m;

            ImagesTableHidden.Attributes.Remove("class");

            var rounded = decimal.Round(rating, 1);
            Session["ImagesRating"] = rounded;
            ImagesRating.InnerHtml = rounded.ToString();

            var temp = (decimal)Session["RatingUx"];
            Session["RatingUx"] = temp + rounded;

            temp = (decimal)Session["RatingTech"];
            Session["RatingTech"] = temp + rounded;

        }

        private List<HtmlNode> GetAllImages(string page)
        {
            var list = new List<HtmlNode>();
            // Get images using html agility pack

            var webget = new HtmlWeb();
            var doc = webget.Load(page);

            if (doc.DocumentNode.SelectNodes("//img") != null)
            {
                foreach (var item in doc.DocumentNode.SelectNodes("//img"))
                    list.Add(item);
            }

            return list;
        }
        private bool HasImgAttributes(HtmlNode item)
        {
            // Check if item has width and height attributes

            if (item.Attributes["width"] != null && item.Attributes["height"] != null)
            {
                return true;
            }

            return false;
        }

        private bool IsImageSize(HtmlNode ImageFileName)
        {
            var imageUrl = ImageFileName.Attributes["src"].Value;
            // Check if ImageFileName has the same size declared as original dimensions
            var baseUrl = Session["MainUrl"].ToString();
            if (baseUrl.EndsWith("/"))
            {
                baseUrl = baseUrl.Remove(baseUrl.Length - 1);
            }

            if (!imageUrl.Contains("http"))
            {
                imageUrl = baseUrl + imageUrl;
            }

            Debug.WriteLine("imageUrl " + imageUrl);

            try
            {
                HttpWebRequest req = (HttpWebRequest)(System.Net.HttpWebRequest.Create(imageUrl));
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                System.Drawing.Image img = System.Drawing.Image.FromStream(resp.GetResponseStream());
                resp.Close();

                var width = img.PhysicalDimension.Width;
                var height = img.PhysicalDimension.Height; 
                if (width == float.Parse(ImageFileName.Attributes["width"].Value) && height == float.Parse(ImageFileName.Attributes["height"].Value))
                {
                    return true;
                }

                Debug.WriteLine("width: " + width);
                Debug.WriteLine("node width: " + ImageFileName.Attributes["width"].Value);
                Debug.WriteLine("height: " + height);
                Debug.WriteLine("node height: " + ImageFileName.Attributes["height"].Value);
            }
            catch (ArgumentException)
            {
                Debug.WriteLine("ArgumentException -> " + imageUrl);
            }
            return false;
        }

        private void AddToTable(string page, string imgUrl, string msg)
        {
            var tRow = new TableRow();

            var tCellPage = new TableCell();
            tCellPage.Text = "<a href='" + page + "' target='_blank'>" + page + "</a>";
            tRow.Cells.Add(tCellPage);

            var tCellImg = new TableCell();
            tCellImg.Text = imgUrl;
            tRow.Cells.Add(tCellImg);

            var tCellmsg = new TableCell();
            tCellmsg.Text = msg;
            tRow.Cells.Add(tCellmsg);

            table.Rows.Add(tRow);
        }
    }
}
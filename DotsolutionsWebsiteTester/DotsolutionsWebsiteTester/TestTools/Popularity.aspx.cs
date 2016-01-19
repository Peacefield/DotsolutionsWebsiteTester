using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class Popularity : System.Web.UI.Page
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

            StartPopularityTest();

            var sb = new System.Text.StringBuilder();
            PopularitySession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Popularity"] = htmlstring;
        }

        private void StartPopularityTest()
        {
            var message = "";
            var rating = 10m;
            var mainUrl = Session["mainUrl"].ToString();

            var AlexaApiResponse = GetAlexaResponse(mainUrl);

            var alexaRank = ReadRankFromXml(AlexaApiResponse);
            var alexaDelta = ReadDeltaFromXml(AlexaApiResponse);

            if (alexaRank <= 0)
                message += "Geen rank kunnen vinden";
            else
                message += "Rank is: " + alexaRank.ToString("#,##0");

            message += GetDeltaMessage(alexaDelta);

            PopularityResults.InnerHtml = message;

            if (rating < 0m)
            {
                rating = 0.0m;
            }
            if (rating == 10.0m)
                rating = 10m;
            decimal rounded = decimal.Round(rating, 1);
            PopularityRating.InnerHtml = rounded.ToString();
            var ratingAccess = (decimal)Session["RatingUx"];
            Session["RatingUx"] = rounded + ratingAccess;
            var RatingMarketing = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = rounded + RatingMarketing;
            SetRatingDisplay(rating);
            Session["PopularityRating"] = rounded;
        }

        private string GetDeltaMessage(string alexaDelta)
        {
            var message = "";
            if (alexaDelta.Contains("+"))
            {

                var delta = Int32.Parse(alexaDelta.Replace("+",""));
                Debug.WriteLine(delta);
                if (delta == 0)
                    message = "<br/>Deze website heeft over de afgelopen 3 maanden geen stijging in populariteit gehad.";
                else
                    message = "<br/>Deze website heeft over de afgelopen 3 maanden een stijging in populariteit gehad van " + delta.ToString("#,##0") + ".";

            }
            else if (alexaDelta.Contains("-"))
            {
                var delta = Int32.Parse(alexaDelta.Remove(alexaDelta.IndexOf("-")));
                message = "<br/>Deze website heeft over de afgelopen 3 maanden een daling in populariteit gehad van " + delta.ToString("#,##0");
                Debug.WriteLine("Delta is -" + delta);
            }

            return message;
        }

        private string GetAlexaResponse(string mainUrl)
        {
            var requestString = "http://data.alexa.com/data?cli=10&url=" + mainUrl;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestString);
            request.UserAgent = Session["userAgent"].ToString();
            request.Headers.Add("Accept-Language", "nl-NL,nl;q=0.8,en-US;q=0.6,en;q=0.4");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            return responseFromServer;
        }

        private int ReadRankFromXml(string responseFromServer)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(responseFromServer)))
            {
                reader.ReadToFollowing("REACH");
                reader.MoveToFirstAttribute();
                return Int32.Parse(reader.Value);
            }
        }

        private string ReadDeltaFromXml(string responseFromServer)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(responseFromServer)))
            {
                reader.ReadToFollowing("RANK");
                reader.MoveToFirstAttribute();
                return reader.Value;
            }
        }


        /// <summary>
        /// Set the colour that indicates the rating accordingly
        /// </summary>
        /// <param name="rating">decimal rating</param>
        private void SetRatingDisplay(decimal rating)
        {
            if (rating == 10m)
                PopularityRating.Attributes.Add("class", "score-10 ratingCircle");
            else if (rating >= 9m)
                PopularityRating.Attributes.Add("class", "score-9 ratingCircle");
            else if (rating >= 8m)
                PopularityRating.Attributes.Add("class", "score-8 ratingCircle");
            else if (rating >= 7m)
                PopularityRating.Attributes.Add("class", "score-7 ratingCircle");
            else if (rating >= 6m)
                PopularityRating.Attributes.Add("class", "score-6 ratingCircle");
            else if (rating >= 5m)
                PopularityRating.Attributes.Add("class", "score-5 ratingCircle");
            else if (rating >= 4m)
                PopularityRating.Attributes.Add("class", "score-4 ratingCircle");
            else if (rating >= 3m)
                PopularityRating.Attributes.Add("class", "score-3 ratingCircle");
            else if (rating >= 2m)
                PopularityRating.Attributes.Add("class", "score-2 ratingCircle");
            else if (rating >= 1m)
                PopularityRating.Attributes.Add("class", "score-1 ratingCircle");
            else
                PopularityRating.Attributes.Add("class", "score-0 ratingCircle");
        }
    }
}
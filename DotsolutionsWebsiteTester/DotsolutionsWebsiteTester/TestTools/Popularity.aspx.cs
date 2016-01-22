using System;
using System.IO;
using System.Net;
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
            var rating = 5.5m;
            var mainUrl = Session["mainUrl"].ToString();
            var AlexaApiResponse = GetAlexaResponse(mainUrl);

            try
            {
                var alexaRank = ReadRankFromXml(AlexaApiResponse);
                var alexaDelta = ReadDeltaFromXml(AlexaApiResponse);

                if (alexaRank <= 0)
                    message += "<div class='well well-lg resultWell text-center'>"
                        + "<span>Geen rank kunnen vinden.</span></div>"
                        + "<div class='resultDivider'></div>";
                else
                    message += "<div class='well well-lg resultWell text-center'>"
                        + "<span class='largetext'>" + alexaRank.ToString("#,##0") + "</span><br/>"
                        + "<span>Alexa ranking</span></div>"
                        + "<div class='resultDivider'></div>";

                message += GetDeltaMessage(alexaDelta);

                rating = CalculateRating(alexaRank, alexaDelta);
            }
            catch (FormatException)
            {
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er is geen populariteit-ranking bekend bij <a href='http://www.alexa.com/' target='_blank'>Alexa</a>.</span></div>";
            }
            catch (ArgumentNullException)
            {
                message += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg messageIcon'></i>"
                    + "<span class='messageText'> Er is geen populariteit-ranking bekend bij <a href='http://www.alexa.com/' target='_blank'>Alexa</a>.</span></div>";
            }
            PopularityResults.InnerHtml = message;

            if (rating < 0m)
                rating = 0.0m;
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

        private decimal CalculateRating(int rank, string deltaStr)
        {
            var rating = 0.0m;
            var delta = 0m;

            // negative delta means rise in rank
            if (deltaStr.Contains("-"))
            {
                delta = decimal.Parse(deltaStr.Replace("-", ""));
                if (delta > 0)
                {
                    var percentage = delta / (decimal)rank * 100;
                    // Debug.WriteLine("percentage: " + percentage.ToString("#.##"));

                    if (percentage > 75)
                        rating = 10m;
                    else if (percentage > 60)
                        rating = 8.0m;
                    else if (percentage > 50)
                        rating = 7.0m;
                    else if (percentage > 30)
                        rating = 6.0m;
                    else if (percentage > 25)
                        rating = 5.5m;
                    else if (percentage > 10)
                        rating = 3.0m;
                    else
                        rating = 0.0m;
                }
                else
                {
                    if (rank < 1000)
                        rating = 10m;
                    else if (rank < 50000)
                        rating = 7.5m;
                    else if (rank < 100000)
                        rating = 5.5m;
                    else
                        rating = 0.0m;
                }
            }
            // positive delta means decline in rank
            else if (deltaStr.Contains("+"))
            {
                delta = decimal.Parse(deltaStr.Replace("+", ""));

                var percentage = delta / (decimal)rank * 100;
                //Debug.WriteLine("percentage: " + percentage.ToString("#.##"));

                if (percentage > 75)
                    rating = 0.0m;
                else if (percentage > 60)
                    rating = 3.0m;
                else if (percentage > 50)
                    rating = 5.5m;
                else if (percentage > 30)
                    rating = 6.0m;
                else if (percentage > 25)
                    rating = 7.0m;
                else if (percentage > 10)
                    rating = 8.0m;
                else
                    rating = 10m;
            }

            return rating;
        }

        private string GetDeltaMessage(string alexaDelta)
        {
            var message = "";
            if (alexaDelta.Contains("-"))
            {
                var delta = Int32.Parse(alexaDelta.Replace("-", ""));

                if (delta == 0)
                    message = "<div class='well well-lg resultWell text-center'>"
                        + "<span>Deze website heeft over de afgelopen 3 maanden geen stijging in populariteit gehad</span></div>";
                else
                    message = "<div class='well well-lg resultWell text-center'>"
                    + "<span class='largetext'>+ " + delta.ToString("#,##0") + "</span><br/>"
                    + "<span>Posities gestegen over de afgelopen 3 maanden.</span></div>";

            }
            else if (alexaDelta.Contains("+"))
            {
                var delta = Int32.Parse(alexaDelta.Replace("+", ""));

                message = "<div class='well well-lg resultWell text-center'>"
                + "<span class='largetext'>- " + delta.ToString("#,##0") + "</span><br/>"
                + "<span>Posities gedaald over de afgelopen 3 maanden.</span></div>";
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
                reader.ReadToFollowing("POPULARITY");
                var popularity = reader.GetAttribute("TEXT");
                return Int32.Parse(popularity);
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
            var element = PopularityRating;

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
using Scoop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class IncomingLinks : System.Web.UI.Page
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
            GetIncomingLinks();

            var sb = new System.Text.StringBuilder();
            IncomingLinksSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["IncomingLinks"] = htmlstring;
        }

        private void GetIncomingLinks()
        {
            var rating = 10m;

            var strAccessID = GetFromApiKeys("MozscapeAccessId");
            var strPrivateKey = GetFromApiKeys("MozscapeSecretKey");

            MozscapeAPI mozAPI = new MozscapeAPI();

            string strAPIURL = mozAPI.CreateAPIURL(strAccessID, strPrivateKey, 1, "url metrics", "www.dotsolutions.nl", "");
            string strResults = mozAPI.FetchResults(strAPIURL);
            MozscapeURLMetric msURLMetrics = mozAPI.ParseURLMetrics(strResults);
            string strExternalLinks = msURLMetrics.ueid;
            string strMozRankUrl = msURLMetrics.umrp;

            Debug.WriteLine("strAPIURL: " + strAPIURL);
            Debug.WriteLine("strResults: " + strResults);
            Debug.WriteLine("strExternalLinks: " + strExternalLinks);
            Debug.WriteLine("strMozRankUrl: " + strMozRankUrl);
                        
            decimal rounded = decimal.Round(rating, 1);
            var temp = (decimal)Session["RatingMarketing"];
            Session["RatingMarketing"] = temp + rounded;

            Session["IncomingLinksRating"] = rounded;
        }

        // https://moz.com/help/guides/moz-api/mozscape/api-reference/url-metrics
        // https://moz.com/help/guides/moz-api/mozscape/getting-started-with-mozscape
        // https://moz.com/help/guides/moz-api/mozscape/getting-started-with-mozscape/anatomy-of-a-mozscape-api-call

        // http://uk.queryclick.com/seo-news/using-mozscape-api-c-net/
        // https://github.com/QueryClick/MozscapeAPI/blob/master/MozscapeAPI.cs#L93

        /// <summary>
        /// Get ApiKey from Session["ApiKeys"]
        /// </summary>
        /// <param name="key">ApiKey</param>
        /// <returns>ApiKey Value</returns>
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
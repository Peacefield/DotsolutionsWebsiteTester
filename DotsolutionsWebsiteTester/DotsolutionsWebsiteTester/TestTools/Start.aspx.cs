using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class Start : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Session["MainUrl"].ToString();
            }
            catch (NullReferenceException)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            ThreadStart ths = new ThreadStart(GetSiteList);
            Thread th = new Thread(ths);
            th.Start();
            Thread.Sleep(1000);

            while (th.IsAlive) ;
        }

        /// <summary>
        /// Get list of sites we can test from the google search api
        /// Adds this list to Session["selectedSites"] so we can access it throughout the application
        /// </summary>
        private void GetSiteList()
        {
            List<string> sitemap = new List<string>();
            string url = Session["MainUrl"].ToString();
            string userAgent = Session["userAgent"].ToString();

            System.Diagnostics.Debug.WriteLine(">>>> Start >>> " + url);

            //sitemap.Add(url);
            //testedsiteslist.InnerHtml += "<li><a href='" + url + "' target='_blank'>" + url + "</a></li>";
            //// Add tested sites to session
            //Session["selectedSites"] = sitemap;

            //return;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&key=AIzaSyCW4MrrpXcOPU6JYkz-aauIctDQEoFymow&rsz=5&q=site:" + url + "%20" + url);
            // Additional parameters
            // &rsz=[1-8] resultSize can be 1 through 8
            // &start=[x] Indicate where to start searching
            request.UserAgent = userAgent;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content. 
            string responseFromServer = reader.ReadToEnd();


            JObject googleSearch = JObject.Parse(responseFromServer);
            IList<JToken> results = googleSearch["responseData"]["results"].Children().ToList();

            if (results.Count != 0)
            {
                foreach (var item in results)
                    sitemap.Add(item["url"].ToString());
            }
            else
                sitemap.Add(url);

            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();

            foreach (string item in sitemap)
                testedsiteslist.InnerHtml += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";

            // Add tested sites to session
            Session["selectedSites"] = sitemap;
        }
    }
}
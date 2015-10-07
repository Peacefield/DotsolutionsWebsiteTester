using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace DotsolutionsWebsiteTester
{
    public partial class ProcessTest : System.Web.UI.Page
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

            UrlTesting.InnerText = Session["MainUrl"].ToString();

            string userAgent = "Mozilla/5.0 (Quality test, http://www.example.net)";
            Session["userAgent"] = userAgent;

            var ths = new ThreadStart(GetTestList);
            var th = new Thread(ths);
            th.Start();

            var ths2 = new ThreadStart(GetSiteList);
            var th2 = new Thread(ths2);
            th2.Start();

            th.Join();
            th2.Join();
        }

        /// <summary>
        /// Get list of selected tests from Session["selectedTests"]
        /// Adds this list to the page for the user to see, but also to use in the jquery AJAX requests
        /// </summary>
        private void GetTestList()
        {
            var selectedTests = (List<string>)Session["selectedTests"];
            var selectedTestsName = (List<string>)Session["selectedTestsName"];

            for (int i = 0; i < selectedTests.Count; i++)
            {
                // Set list for ajax requests
                performedTests.InnerHtml += "<li>" + selectedTests[i] + "</li>";
                // Display name for user
                performedTestsName.InnerHtml += "<li><a onclick=animateTo('" + selectedTests[i] + "') >" + selectedTestsName[i] + "</a></li>";
            }
        }
        
        /// <summary>
        /// Get list of sites we can test from the google search api
        /// Adds this list to Session["selectedSites"] so we can access it throughout the application
        /// </summary>
        private void GetSiteList()
        {
            var sitemap = new List<string>();
            string url = Session["MainUrl"].ToString();
            string userAgent = Session["userAgent"].ToString();
            bool isPresent = false;

            Debug.WriteLine(">>>> GetSiteList >>> " + url);

            //sitemap.Add(url);
            //testedsiteslist.InnerHtml += "<li><a href='" + url + "' target='_blank'>" + url + "</a></li>";
            //// Add tested sites to session
            //Session["selectedSites"] = sitemap;
            //return;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&key=AIzaSyCW4MrrpXcOPU6JYkz-aauIctDQEoFymow&rsz=5&q=site:" + url + "%20" + url);
            // Additional parameters
            // &rsz=[1-8] resultSize can be 1 through 8. Currently using 5.
            // &start=[x] Indicate where to start searching
            request.UserAgent = userAgent;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            var reader = new StreamReader(dataStream);
            // Read the content. 
            string responseFromServer = reader.ReadToEnd();


            JObject googleSearch = JObject.Parse(responseFromServer);
            IList<JToken> results = googleSearch["responseData"]["results"].Children().ToList();

            if (results.Count != 0)
            {
                foreach (JToken item in results)
                {
                    sitemap.Add(item["url"].ToString());
                    // If entered URL is not in found URL's it will be added
                    // This if-statement detects if it IS among the found URL's by comparing it to several possible URL formats
                    if (item["url"].ToString() == url || item["url"].ToString() == (url + "/")
                        || item["url"].ToString() == url.Replace("http://", "https://") || item["url"].ToString() == url.Replace("https://", "http://")
                        || item["url"].ToString() == url.Replace("http://", "https://") + "/" || item["url"].ToString() == url.Replace("https://", "http://") + "/" )
                        isPresent = true;
                }
                if (!isPresent)
                    sitemap.Add(url);
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

        /// <summary>
        /// Click event handler to initiate Pdf creation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void CreatePdfBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/PdfTemplate.aspx");

            return;
        }
    }
}
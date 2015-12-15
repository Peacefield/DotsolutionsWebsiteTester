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
            //try
            //{
            //    Session["MainUrl"].ToString();
            //}
            //catch (NullReferenceException)
            //{
            //    Response.Redirect("~/");
            //    return;
            //}
            //var sb = new System.Text.StringBuilder();
            //ServerBehaviourSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            //string htmlstring = sb.ToString();

            //Session["ServerBehaviour"] = htmlstring;


            //var site = "http://www.michaelvredeveld.nl";
            var site = "http://www.dotsolutions.nl";

            //var site = "http://www.dotsolutions.nl/css/compressed.css";


            if (isGzip(site))
                ServerBehaviourRating.InnerHtml = "JA";
            else
                ServerBehaviourRating.InnerHtml = "NEE";
        }

        private bool isGzip(string site)
        {
            ServerBehaviourResults.InnerHtml += "<h3>Testen van " + site + "</h3>";
            //WebClient c = new WebClient();
            //byte[] responseData = c.DownloadData(site);

            //return responseData.Length >= 2 &&
            //    responseData[0] == 31 &&
            //    responseData[1] == 139;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(site);
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var stream = response.GetResponseStream();


            //using (WebResponse response = request.GetResponse())
            //{
            //    byte[] bytearray = new byte[3];
            //    response.GetResponseStream().Read(bytearray, 0, 3);

            //    foreach (var item in bytearray)
            //    {
            //        ServerBehaviourResults.InnerHtml += item + "<br/>";
            //    }

            //    var keys = response.Headers.AllKeys;
            //    foreach (var key in keys)
            //    {
            //        ServerBehaviourResults.InnerHtml += key + " -- " + response.Headers[key.ToString()] + "<br/>";
            //    }
            //}

            //MemoryStream ms = new MemoryStream();
            //Context.Request.InputStream.CopyTo(ms);

            //ServerBehaviourResults.InnerHtml += ms + "<br/>";
            //byte[] data = ms.ToArray();

            //ServerBehaviourResults.InnerHtml += data.Length + "<br/>";

            //string encoding = response.Headers["Accept-Encoding"]; 
            //if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
            //{
            //    ServerBehaviourResults.InnerHtml += "Is gzip'ed";
            //    return true;
            //}
            //else
            //{
            //    ServerBehaviourResults.InnerHtml += "Is niet gzip'ed";
            //    ServerBehaviourResults.InnerHtml += "<br/>" + encoding;
            //    return false;
            //}

            //if (response.ContentEncoding.Length > 0)
            //{
            //    ServerBehaviourResults.InnerHtml += "content-encoding: " + response.ContentEncoding + "<br/>";
            //}

            //var iets = response.Headers[HttpResponseHeader.ContentEncoding];

            //if (iets != null)
            //{
            //    ServerBehaviourResults.InnerHtml += "Content-Encoding not null";

            //    ServerBehaviourResults.InnerHtml += "<br/>header: " + iets;
            //}
            //else
            //{
            //    ServerBehaviourResults.InnerHtml += "Content-encoding toch wel null";
            //}

            //string header = null;

            //header = response.GetResponseHeader("Content-Encoding");

            //if (header != null)
            //{
            //    ServerBehaviourResults.InnerHtml += "Content-Encoding not null";

            //    ServerBehaviourResults.InnerHtml += "<br/>header: " + header.Length;

            //    if (header.Count() > 0)
            //    {
            //        ServerBehaviourResults.InnerHtml += "<br/>count > 0: " + header;
            //    }


            //    foreach (var item in header)
            //    {
            //        ServerBehaviourResults.InnerHtml += "<br/>header: " + item;
            //    }

            //    return true;
            //}
            //else
            //{
            //    ServerBehaviourResults.InnerHtml += "Content-encoding toch wel null";
            //}

            //if (response.Headers["Content-encoding"] != null)
            //{
            //    ServerBehaviourResults.InnerHtml += "Content-encoding not null";
            //}
            //else
            //{
            //    ServerBehaviourResults.InnerHtml += "Content-encoding null";
            //}


            //foreach (var item in response.Headers.AllKeys)
            //{
            //    ServerBehaviourResults.InnerHtml += item + "<br/>";
            //}


            //// Get the stream containing content returned by the server.
            //Stream dataStream = response.GetResponseStream();
            //// Open the stream using a StreamReader for easy access.
            //var reader = new StreamReader(dataStream);
            //// Read the content. 
            //string responseFromServer = reader.ReadToEnd();

            return false;

        }
    }
}
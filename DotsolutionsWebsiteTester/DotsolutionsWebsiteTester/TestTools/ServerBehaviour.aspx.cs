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


            var site = "http://www.dotsolutions.nl";
            
            if (isGzip(site))
                ServerBehaviourRating.InnerHtml = "JA";
            else
                ServerBehaviourRating.InnerHtml = "NEE";
        }

        private bool isGzip(string site)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(site);
            request.Headers.Add("Accept-Encoding", "gzip");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            
            if (response.Headers["Content-Encoding"] != null)
            {
                if (response.Headers["Content-Encoding"].Contains("gzip"))
                    return true;
            }
            
            return false;

        }
    }
}
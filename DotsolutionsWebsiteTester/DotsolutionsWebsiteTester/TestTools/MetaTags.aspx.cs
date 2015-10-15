using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class MetaTags : System.Web.UI.Page
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
            var sb = new System.Text.StringBuilder();
            MetaTagsSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["MetaTags"] = htmlstring;
        }

        private void GetMetaTags()
        {
            var sitemap = (List<string>)Session["testedSites"];
            foreach (var url in sitemap)
            {                
                var Webget = new HtmlWeb();
                var doc = Webget.Load(url);

                if (doc.DocumentNode.SelectSingleNode("//head") != null)
                {
                    var node = doc.DocumentNode.SelectSingleNode("//head");
                    Debug.WriteLine("Getting <head> contents of " + url);
                    Debug.WriteLine(node.InnerHtml);
                }
            }
        }
    }
}
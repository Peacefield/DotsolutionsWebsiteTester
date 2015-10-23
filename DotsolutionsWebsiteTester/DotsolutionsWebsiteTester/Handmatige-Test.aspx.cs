using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester
{
    public partial class HandmatigeTest : System.Web.UI.Page
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
            SiteUrl.InnerHtml = "<a href='" + Session["MainUrl"].ToString() + "' target='_blank'>" + Session["MainUrl"].ToString() + "</a>";

            // Payed services with 100 free requests per month, but WITH mobile resolution ability
            // https://www.screenshotmachine.com/index.php
            // https://screenshotlayer.com/
            // Free service without mobile resolutionn ability
            // http://www.shrinktheweb.com/
            // http://www.shrinktheweb.com/auth/stw-lobby
            //var accesskeyid = "1dd9ec6a56c1fac";
            //var imgUrl = "http://images.shrinktheweb.com/xino.php?"
            //    + "stwembed=1"
            //    + "&stwaccesskeyid= " + accesskeyid
            //    + "&stwinside=1"
            //    + "&stwsize=xlg"
            //    + "&stwurl=" + Session["MainUrl"].ToString();
            var imgUrl = "/Content/images/placeholder.jpg";
            websiteImg.InnerHtml = "<a href='" + Session["MainUrl"].ToString() + "' target='_blank'>"
                +"<img src='" + imgUrl + "' title='Preview " + Session["MainUrl"].ToString() + "' alt='Preview " + Session["MainUrl"].ToString() + "' /></a>";
        }

        /// <summary>
        /// Skip test by moving on to next page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SkipTest_Click(Object sender, EventArgs e)
        {
            Session["ManualTest"] = false;
            Response.Redirect("Geautomatiseerde-Test");
            return;
        }

        /// <summary>
        /// Continue test after filling in manual test form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void StartTest_Click(Object sender, EventArgs e)
        {
            var vormProfOpma = VormProfOpma.Text;
            var vormProfHuis = VormProfHuis.Text;
            var vormProfKleur = VormProfKleur.Text;
            var vormUxMen = VormUxMen.Text;
            var vormUxStruc = VormUxStruc.Text;
            var vormgevingOpmerking = VormgevingOpmerking.Text;

            if (vormProfOpma == "" || vormProfHuis == "" || vormProfKleur == "" || vormUxMen == "" || vormUxStruc == "")
            {
                required.Attributes.Remove("class");
                return;
            }

            VormgevingOpmerking.Text = "";
            Session["ManualTest"] = true;
            Session["VormProfOpma"] = vormProfOpma;
            Session["VormProfHuis"] = vormProfHuis;
            Session["VormProfKleur"] = vormProfKleur;

            Session["VormUxMen"] = vormUxMen;
            Session["VormUxStruc"] = vormUxStruc;

            Session["VormOpm"] = vormgevingOpmerking;

            Response.Redirect("Geautomatiseerde-Test");
            return;
        }
    }
}
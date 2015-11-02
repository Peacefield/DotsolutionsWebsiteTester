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
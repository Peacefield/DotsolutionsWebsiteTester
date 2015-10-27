using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class GooglePlus : System.Web.UI.Page
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
            GooglePlusSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["GooglePlus"] = htmlstring;
        }
        // URL van profiel is: https://www.googleapis.com/plus/v1/people/+dotsolutionsNl?key=AIzaSyCzstyvlPc3tRtHKjmjnYkARP5-SaoOp2I
        // Returned urls[] met daarin de ingevoerde URLs waaronder website als type "website"
        // Kan ook facebook/twitter bevatten als, bijvoorbeeld, other.
    }
}
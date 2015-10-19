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
            //try
            //{
            //    Session["MainUrl"].ToString();
            //}
            //catch (NullReferenceException)
            //{
            //    Response.Redirect("~/");
            //    return;
            //}
        }

        protected void SkipTest_Click(Object sender, EventArgs e)
        {
            Response.Redirect("Geautomatiseerde-Test.aspx");
            return;
        }

    }
}
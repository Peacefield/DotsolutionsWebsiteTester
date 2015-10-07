using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.ErrorPages
{
    public partial class _404 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("404 pagina bereikt");
        }

        protected void HomeBackBtn_Click(Object sender, EventArgs e)
        {
            Response.Redirect("~/");
        }

    }
}
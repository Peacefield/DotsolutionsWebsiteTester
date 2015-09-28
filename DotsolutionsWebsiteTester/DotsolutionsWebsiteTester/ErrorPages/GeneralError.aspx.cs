using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.ErrorPages
{
    public partial class GeneralError : System.Web.UI.Page
    {
        protected Exception ex = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            // Get the last error from the server
            Exception ex = Server.GetLastError();

            // Fill the page fields
            exMessage.Text = ex.Message;
        }
    }
}
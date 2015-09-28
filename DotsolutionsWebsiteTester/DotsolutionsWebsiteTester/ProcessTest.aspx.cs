using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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
                Response.Redirect("~/Default.aspx");
                return;
            }
            UrlTesting.InnerText = Session["MainUrl"].ToString();

            string userAgent = "Mozilla/5.0 (Quality test, http://www.example.net)";
            Session["userAgent"] = userAgent;

            System.Threading.ThreadStart ths = new System.Threading.ThreadStart(GetTestList);
            System.Threading.Thread th = new System.Threading.Thread(ths);
            th.Start();
            System.Threading.Thread.Sleep(1000);

            th.Join();
        }

        /// <summary>
        /// Get list of selected tests from Session["selectedTests"]
        /// Adds this list to the page for the user to see, but also to use in the jquery AJAX requests
        /// </summary>
        private void GetTestList()
        {
            List<string> selectedTests = (List<string>)Session["selectedTests"];
            List<string> selectedTestsName = (List<string>)Session["selectedTestsName"];

            for (int i = 0; i < selectedTests.Count; i++)
            {
                // Set list for ajax requests
                performedTests.InnerHtml += "<li>" + selectedTests[i] + "</li>";
                // Display name for user
                performedTestsName.InnerHtml += "<li><a href='#" + selectedTests[i] + "' >" + selectedTestsName[i] + "</a></li>";
            }
        }
        protected void CreatePdfBtn_Click(object sender, EventArgs e)
        {
            //Response.Redirect("PdfTemplate.aspx");
            return;
        }
    }
}
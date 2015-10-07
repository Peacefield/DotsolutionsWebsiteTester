using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester
{
    public partial class PdfTemplate : System.Web.UI.Page
    {
        protected List<string> selectedSites = new List<string>();
        protected List<string> selectedTests = new List<string>();
        protected List<string> selectedTestsName = new List<string>();
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                UrlTesting.InnerText = Session["MainUrl"].ToString();
            }
            catch (NullReferenceException)
            {
                Response.Redirect("~/");
                return;
            }

            this.selectedSites = (List<string>)Session["selectedSites"];
            this.selectedTests = (List<string>)Session["selectedTests"];
            this.selectedTestsName = (List<string>)Session["selectedTestsName"];

            // Add a list of links to the tested pages
            foreach (var item in selectedSites)
                testedsiteslist.InnerHtml += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";

            // Add a list of performed tests with navigation option
            for (int i = 0; i < selectedTests.Count; i++)
                performedTests.InnerHtml += "<li><a href='#" + selectedTests[i] + "' >" + selectedTestsName[i] + "</a></li>";

            // Append HTML to the results div
            foreach (var test in selectedTests)
            {
                try
                {
                    results.InnerHtml += "<div id='" + test + "'>" + Session[test].ToString() + "</div>";
                }
                catch (NullReferenceException)
                {
                    results.InnerHtml += "<div class = 'panel panel-danger' id='" + test + "'>"
                        + "<div class = 'panel-heading'>" + test + "</div>"
                        + "<div class = 'panel-body'>Test niet uitgevoerd, mogelijk in verband met adblocker</div></div>";
                }
            }
        }

        /// <summary>
        /// Add a 0 to the start of an integer if it's less than 10 to improve readability
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private string AddZero(int date)
        {
            string temp = date.ToString();
            if (date < 10)
                temp = "0" + date;
            return temp;
        }

        /// <summary>
        /// Overriding Render to intercept html when page_load is ready so we can use it for HTML to PDF conversion
        /// </summary>
        /// <param name="writer">HtmlTextWriter</param>
        protected override void Render(HtmlTextWriter writer)
        {
            var sbOut = new StringBuilder();
            var swOut = new StringWriter(sbOut);
            var htwOut = new HtmlTextWriter(swOut);
            base.Render(htwOut);
            string sOut = sbOut.ToString();

            string year = System.DateTime.Today.Year.ToString();
            int month = System.DateTime.Today.Month;
            int day = System.DateTime.Today.Day;
            int hour = System.DateTime.Now.Hour;
            int minute = System.DateTime.Now.Minute;

            string date = year + "-" + AddZero(month) + "-" + AddZero(day) + "_" + AddZero(hour) + "." + AddZero(minute);

            // URL format will be: http.example.com
            string url = Session["MainUrl"].ToString().Replace("://", ".").Replace("/", "-");

            // Filename with format -> YY-MM-DD_hh.mm Rapportage_http.example.com
            string filename = @"" + date + " Rapportage_" + url;

            //try
            //{
            //    // create an API client instance
            //    //Currently using my personal free license account
            //    pdfcrowd.Client client = new pdfcrowd.Client("Peacefield", "0db50169ce387c622116d715ea5350ce");

            //    // convert a web page and write the generated PDF to a memory stream
            //    MemoryStream Stream = new MemoryStream();
            //    client.convertHtml(sOut, Stream);

            //    // set HTTP response headers
            //    Response.Clear();
            //    Response.AddHeader("Content-Type", "application/pdf");
            //    Response.AddHeader("Cache-Control", "max-age=0");
            //    Response.AddHeader("Accept-Ranges", "none");
            //    Response.AddHeader("Content-Disposition", "attachment; filename=" + filename + ".pdf");

            //    // send the generated PDF
            //    Stream.WriteTo(Response.OutputStream);
            //    Stream.Close();
            //    Response.Flush();
            //    Response.End();
            //}
            //catch (pdfcrowd.Error why)
            //{
            //    Response.Write(why.ToString());
            //}


            // Saving pdfcrowd tokens by using streamwriter for now

            // Write the string to a file with format -> Rapportage_http.example.com_YY-MM-DD--hh.mm.ss
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"c:\users\michael\dropbox\hw\Stageopdracht_Dotsolutions\_TestUitrollingen\"
                + filename + ".html");
            file.WriteLine(sOut);

            file.Close();

            writer.Write(sOut);
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                Session["MainUrl"].ToString();
            }
            catch (NullReferenceException)
            {
                Response.Redirect("~/");
                return;
            }

            sizeref.InnerHtml = Session["sizeref"].ToString();

            this.selectedSites = (List<string>)Session["selectedSites"];
            this.selectedTests = (List<string>)Session["selectedTests"];
            this.selectedTestsName = (List<string>)Session["selectedTestsName"];

            // Add a list of links to the tested pages
            foreach (var item in selectedSites)
                testedsiteslist.InnerHtml += "<li><a href='" + item + "' target='_blank'>" + item + "</a></li>";

            SetTotalRating();

            if ((bool)Session["ManualTest"])
                manualresults.InnerHtml += Session["ManualTestResults"].ToString();

            //if ((bool)Session["ThreePageReport"])
                CriteriaSummaryContainer.InnerHtml = Session["CriteriaSummaryContents"].ToString();


            if (selectedTests.Count < 8)
                critlistpagebreak.Attributes.Remove("class");


            var isThreePageReport = (bool)Session["ThreePageReport"];

            if (!isThreePageReport)
            {
                var orderedList = OrderByRating(selectedTests);
                // Append HTML to the results div
                foreach (var test in orderedList)
                {
                    try
                    {
                        results.InnerHtml += "<div id='" + test.Key + "'>" + Session[test.Key].ToString() + "</div>";
                    }
                    catch (NullReferenceException)
                    {
                        results.InnerHtml += "<div class = 'panel panel-danger' id='" + test.Key + "'>"
                            + "<div class = 'panel-heading'>" + test.Key + "</div>"
                            + "<div class = 'panel-body'>Test niet uitgevoerd, mogelijk in verband met adblocker</div></div>";
                    }
                }
            }
            else
            {
                // add summary from session
            }
        }

        /// <summary>
        /// Sort the ratings of a list in ascending order
        /// </summary>
        /// <param name="ratingList">Unordered list of a criterium</param>
        /// <returns>Ordered dictionary<string, decimal></returns>
        private Dictionary<string, decimal> OrderByRating(List<string> ratingList)
        {
            var ratings = new Dictionary<string, decimal>();
            foreach (var item in ratingList)
            {
                //ratings.Add(new KeyValuePair<string, int>(item, (int)Session[item + "Rating"]));
                ratings.Add(item, (decimal)HttpContext.Current.Session[item + "Rating"]);
            }

            var sortedDict = from entry in ratings orderby entry.Value ascending select entry;
            var orderedList = new Dictionary<string, decimal>();
            foreach (var pair in sortedDict)
                orderedList.Add(pair.Key, pair.Value);

            return orderedList;
        }

        private void SetTotalRating()
        {
            RatingAccessList.InnerHtml = Session["RatingAccessList"].ToString();
            RatingUxList.InnerHtml = Session["RatingUxList"].ToString();
            RatingMarketingList.InnerHtml = Session["RatingMarketingList"].ToString();
            RatingTechList.InnerHtml = Session["RatingTechList"].ToString();

            RatingAccess.Attributes.Add("class", Session["RatingAccessClasses"].ToString());
            RatingUx.Attributes.Add("class", Session["RatingUxClasses"].ToString());
            RatingMarketing.Attributes.Add("class", Session["RatingMarketingClasses"].ToString());
            RatingTech.Attributes.Add("class", Session["RatingTechClasses"].ToString());

            if (!(bool)Session["ThreePageReport"])
            {
                RatingOverallHidden.Attributes.Remove("class");
                RatingOverall.Attributes.Add("class", Session["RatingOverallClasses"].ToString());
                if (Session["RatingOverall"].ToString() == "-1")
                    RatingOverall.InnerHtml = "-";
                else
                    RatingOverall.InnerHtml = Session["RatingOverall"].ToString();
            }

            if (RatingAccess.Attributes["class"].Contains("emptyScore"))
                RatingAccess.InnerHtml = "-";
            else
                RatingAccess.InnerHtml = GetRoundedRating((decimal)Session["RatingAccess"]);

            if (RatingUx.Attributes["class"].Contains("emptyScore"))
                RatingUx.InnerHtml = "-";
            else
                RatingUx.InnerHtml = GetRoundedRating((decimal)Session["RatingUx"]);

            if (RatingMarketing.Attributes["class"].Contains("emptyScore"))
                RatingMarketing.InnerHtml = "-";
            else
                RatingMarketing.InnerHtml = GetRoundedRating((decimal)Session["RatingMarketing"]);

            if (RatingTech.Attributes["class"].Contains("emptyScore"))
                RatingTech.InnerHtml = "-";
            else
                RatingTech.InnerHtml = GetRoundedRating((decimal)Session["RatingTech"]);
        }

        private string GetRoundedRating(decimal rating)
        {
            var roundedRating = rating.ToString("0.0");
            if (rating == 10m)
                roundedRating = "10";

            return roundedRating;
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
            var isDetailed = (bool)Session["IsDetailedTest"];
            var isPdfPrint = (bool)Session["IsPdfPrint"];

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

            if (!isDetailed)
                filename = filename + "_basis";

            if (isPdfPrint)
            {

                try
                {
                    // create an API client instance
                    //Currently using my personal free license account
                    var ApiKey = System.Web.Configuration.WebConfigurationManager.AppSettings["PdfCrowd"];
                    pdfcrowd.Client client = new pdfcrowd.Client("Peacefield", ApiKey);
                    client.setHtmlZoom(100);

                    // convert a web page and write the generated PDF to a memory stream
                    MemoryStream Stream = new MemoryStream();
                    client.convertHtml(sOut, Stream);

                    // set HTTP response headers
                    Response.Clear();
                    Response.AddHeader("Content-Type", "application/pdf");
                    Response.AddHeader("Cache-Control", "max-age=0");
                    Response.AddHeader("Accept-Ranges", "none");
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + filename + ".pdf");

                    // send the generated PDF
                    Stream.WriteTo(Response.OutputStream);
                    Stream.Close();
                    Response.Flush();
                    Response.End();
                }
                catch (pdfcrowd.Error why)
                {
                    Response.Write(why.ToString());
                }

            }
            else
            {
                // Write the string to a file with format -> Rapportage_http.example.com_YY-MM-DD--hh.mm.ss

                //System.IO.StreamWriter file = new System.IO.StreamWriter(@"c:\users\michael\dropbox\hw\Stageopdracht_Dotsolutions\_TestUitrollingen\"
                //    + filename + ".html");
                //file.WriteLine(sOut);
                //file.Close(); 
                System.IO.StreamWriter file = new System.IO.StreamWriter(Server.MapPath("~/Temp/"
                     + filename + ".html"));
                file.WriteLine(sOut);
                file.Close();

                try
                {
                    Response.ContentType = "application/octet-stream";
                    Response.AppendHeader("content-disposition", "attachment;filename=" + filename + ".html");
                    Response.TransmitFile(Server.MapPath("~/Temp/" + filename + ".html"));
                    Response.Flush();
                }
                finally
                {
                    string fullpath = Server.MapPath("~/Temp/" + filename + ".html");

                    if (System.IO.File.Exists(fullpath))
                    {
                        Debug.WriteLine("File exists");
                        Debug.WriteLine("fullpath" + fullpath);
                        System.IO.File.Delete(fullpath);
                        Debug.WriteLine("File deleted");
                    }
                    else
                    {
                        Debug.WriteLine("File does not exist");
                    }
                }

                Response.End();


            }

            writer.Write(sOut);
        }

        protected override void SavePageStateToPersistenceMedium(object state)
        {
            //base.SavePageStateToPersistenceMedium(state);
        }

        protected override object LoadPageStateFromPersistenceMedium()
        {
            return null; //return base.LoadPageStateFromPersistenceMedium();
        }

        protected override object SaveViewState()
        {
            return null;// base.SaveViewState();
        }
    }
}
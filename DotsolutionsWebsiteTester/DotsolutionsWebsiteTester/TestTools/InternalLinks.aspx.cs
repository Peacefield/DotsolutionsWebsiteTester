using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class InternalLinks : System.Web.UI.Page
    {
        private int errorCnt = 0;
        private List<Thread> ThreadList = new List<Thread>();
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

            ThreadStart ths = new ThreadStart(TestInternalLinks);
            Thread th = new Thread(ths);
            th.Start();

            th.Join();

            var sb = new System.Text.StringBuilder();
            InternalLinksSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["InternalLinks"] = htmlstring;
        }


        /// <summary>
        /// Check per page the links that are declared
        /// Checks for length and initiates a check to see if the link is working
        /// </summary>
        private void TestInternalLinks()
        {
            List<string> sitemap = (List<string>)Session["selectedSites"];
            foreach (string url in sitemap)
            {
                Debug.WriteLine("Link check op -> " + url);

                var Webget = new HtmlWeb();
                var doc = Webget.Load(url);

                // Test every internal link on current page from sitmap
                if (doc.DocumentNode.SelectNodes("//a[@href]") != null)
                {
                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        //TestLink(link, url);

                        ThreadStart ths = new ThreadStart(() => TestLink(link, url));
                        Thread th = new Thread(ths);
                        ThreadList.Add(th);
                        th.Start();
                    }
                }

            }
            // Join Threads that were executing TestLink
            foreach (Thread thread in ThreadList)
                thread.Join();

            // Show message with findings
            if (errorCnt > 0)
            {
                // Show table when errors are found 
                IntLinksHiddenTable.Attributes.Remove("class");
                internalLinksErrorsFound.InnerHtml = "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg'></i>"
                    + "<span>" + errorCnt + " meldingen gevonden.</span></div>";
            }
            else
            {
                internalLinksErrorsFound.InnerHtml = "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span> " + errorCnt + " meldingen gevonden.</span></div>";
            }
        }

        private void TestLink(HtmlNode link, string url)
        {
            string MainUrl = Session["MainUrl"].ToString();

            // Making sure we only test urls, instead of also including mailto: tel: javascript: etc.
            if (link.Attributes["href"].Value.Contains("/") && !link.Attributes["href"].Value.Contains("intent://"))
            {
                // Check that there is a description
                if (link.InnerText != "")
                {
                    // Check if the description is not too long
                    string[] words = link.InnerText.Split(new char[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (words.Length > 40)
                    {
                        errorCnt++;
                        AddToTable(link.Attributes["href"].Value, "Beschrijvende tekst is te lang (" + words.Length + " woorden)", url);
                    }
                }
                // If the link is an image there is no need for a description
                else if (!link.InnerHtml.Contains("img") && !link.InnerHtml.Contains("figure") && !link.InnerHtml.Contains("i"))
                {
                    errorCnt++;
                    AddToTable(link.Attributes["href"].Value, "Beschrijvende tekst van de URL is leeg", url);
                }

                // Test if the link does not return an errorcode
                if (!LinkWorks(MainUrl, link.Attributes["href"].Value))
                {
                    string tablelink;
                    if (link.Attributes["href"].Value.Contains("http://") || link.Attributes["href"].Value.Contains("https://"))
                    {
                        tablelink = "<a href='" + link.Attributes["href"].Value + "' target='_blank'>" + link.Attributes["href"].Value + "</a>";
                    }
                    else
                    {
                        if (MainUrl.EndsWith("/"))
                            tablelink = "<a href='" + MainUrl.Remove(MainUrl.Length - 1) + link.Attributes["href"].Value + "' target='_blank'>" + link.Attributes["href"].Value + "</a>";
                        else
                            tablelink = "<a href='" + MainUrl + link.Attributes["href"].Value + "' target='_blank'>" + link.Attributes["href"].Value + "</a>";
                    }
                    // add message to table
                    AddToTable(tablelink, "Link werkt niet", url);

                    errorCnt++;
                }
            }
        }

        /// <summary>
        /// Test if a link is broken, a.k.a. returns anything other than statuscode 200 OK or Redirect
        /// </summary>
        /// <param name="url">URL to be tested</param>
        /// <returns></returns>
        private bool LinkWorks(string mainurl, string link)
        {
            try
            {
                string requestString = "";
                if (link.Contains("http://") || link.Contains("https://"))
                {
                    //Debug.WriteLine("Testing http--->>> " + link);
                    requestString = link;
                }
                else
                {
                    if (mainurl.EndsWith("/"))
                    {
                        string temp = mainurl.Remove(mainurl.Length - 1);
                        //Debug.WriteLine("Testing endswith --->>> " + temp + link);
                        requestString = temp + link;
                    }
                    else
                    {
                        //Debug.WriteLine("Testing --->>> " + mainurl + link);
                        requestString = mainurl + link;
                    }
                }

                if (requestString == "")
                    return false;

                //Creating the HttpWebRequest
                //HttpWebRequest request = null;
                HttpWebRequest request = WebRequest.Create(requestString) as HttpWebRequest;
                request.Timeout = 10000; // Set timout of 10 seconds so to not waste time
                request.Method = "HEAD";
                request.UnsafeAuthenticatedConnectionSharing = true;
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                //Returns TRUE if the Status code == 200
                //return (response.StatusCode == HttpStatusCode.OK
                //    || response.StatusCode == HttpStatusCode.Redirect
                //    || response.StatusCode == HttpStatusCode.Moved
                //    || response.StatusCode == HttpStatusCode.MovedPermanently);
                //Debug.WriteLine("StatusDescription" + response.StatusDescription);
                response.Dispose();
                return true;
            }
            catch (WebException we)
            {
                //Any webexception will return true unless it's a 404.
                Debug.WriteLine("WebException " + we.Message + " <><><><><> met link: " + link + " en Status: " + we.Status);

                if (we.Message.Contains("404"))
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                //Any exception will return false
                //Debug.WriteLine("Algemene fout " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Adds result to table IntLinksTable
        /// </summary>
        /// <param name="link">Tested link</param>
        /// <param name="text">Description of the error</param>
        /// <param name="page">Page of origin</param>
        private void AddToTable(string link, string text, string page)
        {
            TableRow tRow = new TableRow();

            TableCell tCellLink = new TableCell();
            tCellLink.Text = link;
            tRow.Cells.Add(tCellLink);

            TableCell tCellMsg = new TableCell();
            tCellMsg.Text = text;
            tRow.Cells.Add(tCellMsg);

            TableCell tCellPage = new TableCell();
            tCellPage.Text = "<a href='" + page + "' target='_blank'>" + page + "</a>";
            tRow.Cells.Add(tCellPage);

            IntLinksTable.Rows.Add(tRow);
        }
    }
}
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class InternalLinks : System.Web.UI.Page
    {
        private int errorCnt = 0;
        private int i = 0;
        private List<System.Threading.Thread> ThreadList = new List<System.Threading.Thread>();
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

            System.Threading.ThreadStart ths = new System.Threading.ThreadStart(TestInternalLinks);
            System.Threading.Thread th = new System.Threading.Thread(ths);
            th.Start();

            th.Join();

            var sb = new System.Text.StringBuilder();
            InternalLinksSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["InternalLinks"] = htmlstring;
        }


        /// <summary>
        /// Check per page the links that are present
        /// Checks for length and initiates a check to see if the link is working
        /// </summary>
        private void TestInternalLinks()
        {
            List<string> sitemap = (List<string>)Session["selectedSites"];
            foreach (string url in sitemap)
            {
                System.Diagnostics.Debug.WriteLine("Link check op -> " + url);

                var Webget = new HtmlWeb();
                var doc = Webget.Load(url);

                // Test every internal link on current page from sitmap
                if (doc.DocumentNode.SelectNodes("//a[@href]") != null)
                {
                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        //TestLink(link, url);

                        System.Threading.ThreadStart ths = new System.Threading.ThreadStart(() => TestLink(link, url));
                        System.Threading.Thread th = new System.Threading.Thread(ths);
                        ThreadList.Add(th);
                        th.Start();

                        System.Threading.Thread.Sleep(10);
                    }
                }

            }
            // Join Threads
            foreach (System.Threading.Thread thread in ThreadList)
            {
                thread.Join();
                System.Threading.Thread.Sleep(10);
            }

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

            bool found = false;

            // Making sure we only test urls, instead of also including mailto: tel: javascript: etc.
            if (link.Attributes["href"].Value.Contains("/") && !link.Attributes["href"].Value.Contains("intent://"))
            {
                List<KeyValuePair<string, string>> templist = null;

                // Check that there is a description
                if (link.InnerText != "")
                {
                    // Check if the description is not too long
                    string[] words = link.InnerText.Split(new char[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (words.Length > 40)
                    {
                        errorCnt++;
                        found = true;
                        AddToTable(link.Attributes["href"].Value, "Beschrijvende tekst is te lang", url);
                        // add message to keyvaluepair
                        templist = new List<KeyValuePair<string, string>>()
                                {
                                    new KeyValuePair<string, string>("link", link.Attributes["href"].Value),
                                    new KeyValuePair<string, string>("text", "Beschrijvende tekst is te lang"),
                                    new KeyValuePair<string, string>("page", url)
                                };
                    }
                }
                // If the link is an image there is no need for a description
                else if (!link.InnerHtml.Contains("img") && !link.InnerHtml.Contains("figure") && !link.InnerHtml.Contains("i"))
                {
                    errorCnt++;
                    found = true;
                    AddToTable(link.Attributes["href"].Value, "Beschrijvende tekst van de URL is leeg", url);
                    // add message to keyvaluepair
                    templist = new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("link", link.Attributes["href"].Value),
                                new KeyValuePair<string, string>("text", "Beschrijvende tekst van de URL is leeg"),
                                new KeyValuePair<string, string>("page", url)
                            };
                }

                if (found)
                {
                    i++;
                    found = false;
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
                    // add message to keyvaluepair
                    templist = new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("link", tablelink),
                                new KeyValuePair<string, string>("text", "Link werkt niet"),
                                new KeyValuePair<string, string>("page", url)
                            };
                    // add message to table
                    AddToTable(tablelink, "Link werkt niet", url);

                    errorCnt++;
                    found = true;
                }

                if (found)
                    i++;
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
                    //System.Diagnostics.Debug.WriteLine("Testing http--->>> " + link);
                    requestString = link;
                }
                else
                {
                    if (mainurl.EndsWith("/"))
                    {
                        string temp = mainurl.Remove(mainurl.Length - 1);
                        //System.Diagnostics.Debug.WriteLine("Testing endswith --->>> " + temp + link);
                        requestString = temp + link;
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine("Testing --->>> " + mainurl + link);
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
                System.Diagnostics.Debug.WriteLine("StatusDescription" + response.StatusDescription);
                response.Dispose();
                return true;
            }
            catch (WebException we)
            {
                //Any exception will returns false.
                //System.Diagnostics.Debug.WriteLine("WebException " + we.Message + " <><><><><> met link: " + link + " en Status: " + we.Status);

                if (we.Message.Contains("404"))
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine("Algemene fout " + e.Message);
                return false;
            }
        }

        private void AddToTable(string link, string text, string page)
        {
            TableRow tRow = new TableRow();

            TableCell tCellUrl = new TableCell();
            tCellUrl.Text = link;
            tCellUrl.CssClass = "col-md-6";
            tRow.Cells.Add(tCellUrl);

            TableCell tCellType = new TableCell();
            tCellType.Text = text;
            tCellType.CssClass = "col-md-4";
            tRow.Cells.Add(tCellType);

            TableCell tCellLine = new TableCell();
            tCellLine.Text = "<a href='" + page + "' target='_blank'>" + page + "</a>";
            tCellLine.CssClass = "col-md-2";
            tRow.Cells.Add(tCellLine);

            IntLinksTable.Rows.Add(tRow);
        }
    }
}
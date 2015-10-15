using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class Headings : System.Web.UI.Page
    {
        private List<string> noHeadings = new List<string>();
        private int errorCnt = 0;
        private int totalHeadingCnt = 0;
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

            var ths = new ThreadStart(GetHeadings);
            var th = new Thread(ths);
            th.Start();

            th.Join();

            var sb = new System.Text.StringBuilder();
            HeadingsSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Headings"] = htmlstring;
        }

        private void GetHeadings()
        {
            var sitemap = (List<string>)Session["selectedSites"];
            var rating = 10m;
            foreach (var url in sitemap)
            {
                GetHeadingsOnUrl(url);
            }

            if (noHeadings.Count > 0)
            {
                string unorderedlist = "<ul>";
                foreach (var item in noHeadings)
                {
                    // Point reduction equal to percentage equal to the total amount of tested sites
                    rating = rating - ((1m / (decimal)sitemap.Count) * 10m);
                    unorderedlist += "<li>" + item + "</li>";
                }
                unorderedlist += "</ul>";

                headingMessages.InnerHtml += "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> De volgende pagina's gebruiken geen headers</span>" + unorderedlist + "</div>";
            }
            else
            {
                headingMessages.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                    + "<span> Er wordt op alle pagina's gebruik gemaakt van headers</span></div>";
            }

            if (totalHeadingCnt > 0)
            {

                if (errorCnt > 0)
                {
                    rating = rating - (((decimal)errorCnt / (decimal)totalHeadingCnt) * 4.5m);
                    headingTableHidden.Attributes.Remove("class");
                    headingMessages.InnerHtml += "<div class='alert alert-info col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-exclamation-sign glyphicons-lg'></i>"
                        + "<span> De volgende headers zijn niet voorafgegaan door een groter, overkoepelend header-element:</span></div>";
                }
                else
                {
                    headingMessages.InnerHtml += "<div class='alert alert-success col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                        + "<i class='glyphicon glyphicon-ok glyphicons-lg'></i>"
                        + "<span> Alle headers zijn correct ingedeeld</span></div>";
                }
            }
            else
            {
                headingMessages.InnerHtml = "<div class='alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12' role='alert'>"
                    + "<i class='glyphicon glyphicon-alert glyphicons-lg'></i>"
                    + "<span> Er zijn geen headers gebruikt op alle geteste pagina's</span></div>";
            }

            if (rating > 0)
            {
                decimal rounded = decimal.Round(rating, 1);
                Rating.InnerHtml = rounded.ToString();

                var temp = (decimal)Session["RatingAccess"];
                Session["RatingAccess"] = temp + rounded;

                temp = (decimal)Session["RatingMarketing"];
                Session["RatingMarketing"] = temp + rounded;

                temp = (decimal)Session["RatingTech"];
                Session["RatingTech"] = temp + rounded;
            }
            else
            {
                Rating.InnerHtml = "1,0";

                var temp = (decimal)Session["RatingAccess"];
                Session["RatingAccess"] = temp + 1m;

                temp = (decimal)Session["RatingMarketing"];
                Session["RatingMarketing"] = temp + 1m;

                temp = (decimal)Session["RatingTech"];
                Session["RatingTech"] = temp + 1m;
            }
        }

        private void GetHeadingsOnUrl(string url)
        {
            Debug.WriteLine("Header check op -> " + url);

            var Webget = new HtmlWeb();
            var doc = Webget.Load(url);

            if (doc.DocumentNode.SelectNodes("//h1") == null
                && doc.DocumentNode.SelectNodes("//h2") == null
                && doc.DocumentNode.SelectNodes("//h3") == null
                && doc.DocumentNode.SelectNodes("//h4") == null
                && doc.DocumentNode.SelectNodes("//h5") == null
                && doc.DocumentNode.SelectNodes("//h6") == null)
            {
                noHeadings.Add(url);
                Debug.WriteLine(url + " bevat geen headers");
            }
            else
            {
                var h1list = new List<KeyValuePair<int, string>>();
                var h2list = new List<KeyValuePair<int, string>>();
                var h3list = new List<KeyValuePair<int, string>>();
                var h4list = new List<KeyValuePair<int, string>>();
                var h5list = new List<KeyValuePair<int, string>>();
                var h6list = new List<KeyValuePair<int, string>>();

                // Fill lists
                var templist = new List<List<KeyValuePair<int, string>>>() { h1list, h2list, h3list, h4list, h5list, h6list };
                var current = 1;

                foreach (var list in templist)
                {
                    if (doc.DocumentNode.SelectNodes("//h" + current) != null)
                    {
                        foreach (var item in doc.DocumentNode.SelectNodes("//h" + current))
                        {
                            list.Add(new KeyValuePair<int, string>(item.StreamPosition, item.InnerText));
                            totalHeadingCnt++;
                        }
                    }
                    current++;
                }

                List<List<KeyValuePair<int, string>>> listlist = new List<List<KeyValuePair<int, string>>>(templist);

                var listId = 2;
                for (int i = 0; i < listlist.Count; i++)
                {
                    var nextheader = "h" + listId;

                    if (listlist[i].Count == 0)
                    {
                        if (doc.DocumentNode.SelectNodes("//" + nextheader + "") != null)
                        {
                            foreach (var item in doc.DocumentNode.SelectNodes("//" + nextheader + ""))
                            {
                                AddToTable(item.InnerText, nextheader, url);
                            }
                        }
                    }
                    else
                    {
                        var next = i + 1;
                        if (next < listlist.Count)
                        {
                            if (listlist[next].Count > 0)
                            {
                                foreach (var nextlist in listlist[next])
                                {
                                    var found = false;
                                    for (int j = 0; j < listlist[i].Count; j++)
                                    {
                                        if (listlist[i][j].Key < nextlist.Key)
                                        {
                                            found = true;
                                            break;
                                        }
                                        else
                                            found = false;
                                    }
                                    if (!found)
                                    {
                                        AddToTable(nextlist.Value, nextheader, url);
                                    }
                                }
                            }
                        }
                    }

                    listId++;
                }
            }
        }

        /// <summary>
        /// Add the error/warning message to the table
        /// </summary>
        /// <param name="url">URL of origin of the message</param>
        /// <param name="type">Error or warning</param>
        /// <param name="line">Last Line</param>
        /// <param name="column">Last Column</param>
        /// <param name="msg">Additional error/warning message</param>
        private void AddToTable(string msg, string type, string url)
        {
            errorCnt++;

            var tRow = new TableRow();

            var tCellMsg = new TableCell();
            tCellMsg.Text = "<b>" + msg + "</b>";
            tRow.Cells.Add(tCellMsg);

            var tCellType = new TableCell();
            tCellType.Text = type;
            tRow.Cells.Add(tCellType);

            var tCellUrl = new TableCell();
            tCellUrl.Text = "<a href='" + url + "' target='_blank'>" + url + "</a>";
            tRow.Cells.Add(tCellUrl);

            table.Rows.Add(tRow);
        }
    }
}
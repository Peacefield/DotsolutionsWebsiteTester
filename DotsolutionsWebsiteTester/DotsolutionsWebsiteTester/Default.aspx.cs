using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester
{
    public partial class _Default : Page
    {
        protected List<string> selectedTests = new List<string>();
        protected List<string> selectedTestsName = new List<string>();

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Click event handler that initiates the test if everything is ready
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void StartScanBtn_Click(Object sender, EventArgs e)
        {
            // Check if the entered Url is valid
            string url = UrlTextBox.Text;
            if (!IsValidUrl(url)) return;

            // Check if any tests were selected
            if (!IsTestAvailable()) return;

            // Set session that contains the url the user wants to test
            // This session is also checked throughout the application to check if all the needed sessions are set.
            Session["MainUrl"] = url;

            if (TestMethodList.SelectedValue == "DetailedTest")
            {
                Session["IsDetailedTest"] = true;
                Response.Redirect("Handmatige-Test");
            }
            else
            {
                Session["ManualTest"] = false;
                Session["IsDetailedTest"] = false;
                Response.Redirect("Geautomatiseerde-Test");
            }
            return;
        }

        /// <summary>
        /// Check to see if the entered Url is in a valid format
        /// </summary>
        /// <param name="url">To be tested Url</param>
        /// <returns>True when url is a valid Url</returns>
        private bool IsValidUrl(string url)
        {
            if (url == "" || url == "http://" || url == "https://")
            {
                invalidUrlHidden.Attributes.Remove("class");
                invalidUrl.InnerText = "Geen url ingevoerd";
                return false;
            }
            else
            {
                Uri uriResult;

                // First check on validity of Url.
                if (Uri.TryCreate(url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    Uri urlCheck = new Uri(url);

                    System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(urlCheck);
                    request.Timeout = 15000;
                    System.Net.HttpWebResponse response;

                    // Second check to see if we get a response
                    try
                    {
                        response = (System.Net.HttpWebResponse)request.GetResponse();

                        return true;
                    }
                    catch (Exception)
                    {
                        invalidUrlHidden.Attributes.Remove("class");
                        invalidUrl.InnerText = " Geen bestaande url ingevoerd";
                        return false; //could not connect to the website (most probably) 
                    }
                }
                invalidUrlHidden.Attributes.Remove("class");
                invalidUrl.InnerText = " Geen geldige url ingevoerd";
                return false;
            }
        }

        /// <summary>
        /// Get the tests the user selected to be tested
        /// </summary>
        private bool IsTestAvailable()
        {
            var checkBoxLists = new List<CheckBoxList>();
            var listItems = new List<string>();

            checkBoxLists.Add(TestsCheckBoxList1);
            checkBoxLists.Add(TestsCheckBoxList2);
            checkBoxLists.Add(TestsCheckBoxList3);

            for (int i = 0; i < checkBoxLists.Count; i++)
            {
                for (int j = 0; j < checkBoxLists[i].Items.Count; j++)
                {
                    if (checkBoxLists[i].Items[j].Selected)
                    {
                        this.selectedTests.Add(checkBoxLists[i].Items[j].Value);
                        this.selectedTestsName.Add(checkBoxLists[i].Items[j].Text);
                    }
                }
            }

            if (this.selectedTests.Count > 0)
            {
                // Add selected tests to session so we can execute them
                Session["selectedTests"] = this.selectedTests;
                Session["selectedTestsName"] = this.selectedTestsName;
                return true;
            }
            else
            {
                invalidUrlHidden.Attributes.Remove("class");
                invalidUrl.InnerText = " Geen tests geselecteerd";
                return false;
            }
        }
    }
}
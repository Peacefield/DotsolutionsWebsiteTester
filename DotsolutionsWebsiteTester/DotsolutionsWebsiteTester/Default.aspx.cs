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
        protected void StartScanBtn_Click(Object sender, EventArgs e)
        {
            // Check if the entered Url is valid
            string url = UrlTextBox.Text;
            if (!IsValidUrl(url)) return;

            // Check if any tests were selected
            if (!IsTestAvailable()) return;

            Session["MainUrl"] = url;
            //Response.Redirect("Testing.aspx");
            return;
        }

        /// <summary>
        /// Check to see if the entered Url is in a valid format
        /// </summary>
        /// <param name="url">To be tested Url</param>
        /// <returns>boolean</returns>
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
                        invalidUrl.InnerText = "Geen bestaande url ingevoerd";
                        return false; //could not connect to the website (most probably) 
                    }
                }
                invalidUrlHidden.Attributes.Remove("class");
                invalidUrl.InnerText = "Geen geldige url ingevoerd";
                return false;
            }
        }

        /// <summary>
        /// Get the tests the user selected to be tested
        /// </summary>
        private bool IsTestAvailable()
        {
            List<CheckBoxList> CheckBoxLists = new List<CheckBoxList>();
            List<string> ListItems = new List<string>();

            CheckBoxLists.Add(TestsCheckBoxList1);
            CheckBoxLists.Add(TestsCheckBoxList2);
            CheckBoxLists.Add(TestsCheckBoxList3);

            for (int i = 0; i < CheckBoxLists.Count; i++)
            {
                for (int j = 0; j < CheckBoxLists[i].Items.Count; j++)
                {
                    if (CheckBoxLists[i].Items[j].Selected)
                    {
                        this.selectedTests.Add(CheckBoxLists[i].Items[j].Value);
                        this.selectedTestsName.Add(CheckBoxLists[i].Items[j].Text);
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
                invalidUrl.InnerText = "Geen tests geselecteerd";
                return false;
            }
        }
    }
}
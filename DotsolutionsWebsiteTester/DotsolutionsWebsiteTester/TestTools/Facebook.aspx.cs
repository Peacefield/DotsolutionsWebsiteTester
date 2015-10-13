using Facebook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotsolutionsWebsiteTester.TestTools
{
    public partial class Facebook : System.Web.UI.Page
    {
        FacebookClient fbc;
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

            var accessToken = "CAAVaM5I0rGYBAJfwSTHk981NUSxMqjBAZCDGGDKCuN1KHHHuDJf83ZBh7zD5zKhczoNVjn6Rfib9i4V79em1ianMpSZCdJkkX81qjuG1Q5LbxkJ7OmiPOZB90WgepIEZC07BSzGKL9ZBliisRZAoZC9L5HxOmKRGEbxjrLcOU3cUUtEa2cztmN6ulUeUiqvvjTzIJ7y3SDD5SQZDZD";
            fbc = new FacebookClient(accessToken);

            GetFacebook();

            var sb = new System.Text.StringBuilder();
            FacebookSession.RenderControl(new System.Web.UI.HtmlTextWriter(new System.IO.StringWriter(sb)));
            string htmlstring = sb.ToString();

            Session["Facebook"] = htmlstring;
        }

        private void GetFacebook()
        {
            // Google Search API
            // Facebook http://www.example.net
            // facebook.com/example
            // example
            // IsFacebook(example)
            var temp = "dotsolutions";
            if (IsFacebook(temp))
                Debug.WriteLine("Is dotsolutions!");
        }

        private bool IsFacebook(string name)
        {
            dynamic result = fbc.Get(name, new { fields = "name, id" });

            string fbName = result.name;
            string fbId = result.id;
            if (fbName != "")
            {
                return true;
            }

            return false;
        }

        //Acces Token: CAACEdEose0cBAN6Mglo4kgzCv5RaOAovPlZBQmYLc1mSjXTYxZBaHd14tvtofH3EEqzpWtSORuFKnfuYrC3zNDpVwALPm9JoMUUOrzLBoDazyxF6mRVH0wTX2H1FXVMhgZCQXioGZCrKEuOb6ZAZBKZAo6YaXKS13aC4GtOKYAvAaPqeXhVs7yLSLy8geYkuZBeQJfuKHBuduAZDZD
        //SELECT fan_count, website, pic FROM page WHERE username="dotsolutions"
        //{
        //  "data": [
        //    {
        //      "fan_count": 253,
        //      "website": "http://www.dotsolutions.nl",
        //      "pic": "https://scontent.xx.fbcdn.net/hprofile-xat1/v/t1.0-1/p100x100/12144761_1146334558728006_6208729277974377873_n.png?oh=48e85ea1a9cc32ea8902d6bd5f5fcf2f&oe=56C87E22"
        //    }
        //  ],
        //}
    }
}
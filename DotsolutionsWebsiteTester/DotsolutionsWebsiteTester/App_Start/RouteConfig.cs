using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;

namespace DotsolutionsWebsiteTester
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            //var settings = new FriendlyUrlSettings();
            //settings.AutoRedirectMode = RedirectMode.Permanent;
            //routes.EnableFriendlyUrls(settings);
            //routes.EnableFriendlyUrls();

            routes.MapPageRoute("Handmatige-Test", "Handmatige-Test", "~/Handmatige-Test.aspx");
            routes.MapPageRoute("Geautomatiseerde-Test", "Geautomatiseerde-Test", "~/Geautomatiseerde-Test.aspx");
            routes.MapPageRoute("PdfTemplate", "PdfTemplate", "~/PdfTemplate.aspx");
        }
    }
}

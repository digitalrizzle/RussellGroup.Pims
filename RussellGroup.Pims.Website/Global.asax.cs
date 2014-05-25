using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace RussellGroup.Pims.Website
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public const string DATE_TIME_FORMAT = "yyyyMMddhhmmss";
        public static readonly CultureInfo CULTURE_EN_NZ = CultureInfo.CreateSpecificCulture("en-NZ");
        public static readonly DateTimeStyles DATE_TIME_STYLES_NONE = DateTimeStyles.None;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}

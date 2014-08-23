using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Diagnostics;
using System.Reflection;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website.Helpers
{
    public static class VersionHelper
    {
        public static MvcHtmlString GetCopyright(this HtmlHelper html)
        {
            return new MvcHtmlString(
                FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).LegalCopyright + " " + DateTime.Now.Year);
        }

        public static MvcHtmlString GetVersion(this HtmlHelper html)
        {
            return new MvcHtmlString(
                Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website
{
    public static class Extensions
    {
        public static string ToYesNo(this bool value)
        {
            return value ? "Yes" : "No";
        }

        public static string ActionLink(this Controller controller, string linkText, string actionName, object routeValues)
        {
            return string.Format("<a href=\"{0}\">{1}</a>", controller.Url.Action(actionName, routeValues), linkText);
        }

        public static string CrudLinks(this Controller controller, object routeValues)
        {
            var edit = ActionLink(controller, "Edit", "Edit", routeValues);
            var details = ActionLink(controller, "Details", "Details", routeValues);
            var delete = ActionLink(controller, "Delete", "Delete", routeValues);

            var links = string.Format("{0} | {1} | {2}", edit, details, delete);

            return links;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website
{
    public static class Extensions
    {
        public static string ActionLink(this Controller controller, string linkText, string actionName, object routeValues)
        {
            return string.Format("<a href=\"{0}\">{1}</a>", controller.Url.Action(actionName, routeValues), linkText);
        }

        public static string CrudLinks(this Controller controller, object routeValues)
        {
            var edit = string.Format("<a href=\"{0}\">{1}</a>", controller.Url.Action("Edit", routeValues), "Edit");
            var details = string.Format("<a href=\"{0}\">{1}</a>", controller.Url.Action("Details", routeValues), "Details");
            var delete = string.Format("<a href=\"{0}\">{1}</a>", controller.Url.Action("Delete", routeValues), "Delete");

            var link = string.Format("{0} | {1} | {2}", edit, details, delete);

            return link;
        }
    }
}
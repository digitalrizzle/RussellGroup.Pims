using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.DataAccess.Repositories;
using RussellGroup.Pims.Website.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website
{
    public static class Extensions
    {
        public static readonly Expression<Func<DateTime?, string>> LittleEndianDateString = (date) =>
            (SqlFunctions.DateName("day", date) + "/" +
            ("0" + SqlFunctions.StringConvert((double)date.Value.Month).Trim()).Substring(date.Value.Month / 10, 2) + "/" +
            SqlFunctions.DateName("year", date));

        public static bool IsAuthorized(this IPrincipal user, params string[] roles)
        {
            using (var _repository = new UserDbRepository())
            {
                var helper = new ActiveDirectoryHelper(_repository);

                return helper.IsAuthorized(roles);
            }
        }

        public static string ToYesNo(this bool value)
        {
            return value ? "Yes" : "No";
        }

        public static string ToYesNo(this bool? value)
        {
            if (value.HasValue)
            {
                return ToYesNo(value.Value);
            }

            return null;
        }

        public static string ConvertCrlfToBr(this string value)
        {
            return value.Replace(Environment.NewLine, "<br />");
        }

        public static string GetAction(this HtmlHelper html)
        {
            return html.ViewContext.RouteData.GetRequiredString("action");
        }

        public static bool IsAction(this HtmlHelper html, string name)
        {
            return GetAction(html).Equals(name);
        }

        public static string ActionLink(this Controller controller, string linkText, string actionName, object routeValues)
        {
            return string.Format("<a href=\"{0}\">{1}</a>", controller.Url.Action(actionName, routeValues), linkText);
        }

        public static string ActionLink(this Controller controller, string linkText, string actionName, string controllerName, object routeValues)
        {
            return string.Format("<a href=\"{0}\">{1}</a>", controller.Url.Action(actionName, controllerName, routeValues), linkText);
        }

        public static string CrudLinks(this Controller controller, object routeValues, bool canEdit)
        {
            var edit = ActionLink(controller, "Edit", "Edit", routeValues);
            var details = ActionLink(controller, "Details", "Details", routeValues);
            var delete = ActionLink(controller, "Delete", "Delete", routeValues);

            if (canEdit)
            {
                return string.Format("{0}&nbsp;| {1}&nbsp;| {2}", edit, details, delete);
            }
            else
            {
                return details;
            }
        }

        public static IEnumerable<Guid> GetGuids(this FormCollection collection, string prefix)
        {
            var ids = new List<Guid>();

            foreach (var key in collection.AllKeys)
            {
                if (key.StartsWith(prefix) && !string.IsNullOrWhiteSpace(collection[key]))
                {
                    var value = collection[key].Split(',')[0];
                    ids.Add(new Guid(value));
                }
            }

            return ids.Distinct();
        }

        public static IEnumerable<int> GetIds(this FormCollection collection, string prefix)
        {
            var ids = new List<int>();

            foreach (var key in collection.AllKeys)
            {
                if (key.StartsWith(prefix) && !string.IsNullOrWhiteSpace(collection[key]))
                {
                    var value = collection[key].Split(',')[0];
                    ids.Add(Convert.ToInt32(value));
                }
            }

            return ids.Distinct();
        }

        public static IEnumerable<KeyValuePair<int, int?>> GetIdsAndQuantities(this FormCollection collection, string idPrefix, string quantityPrefix)
        {
            var pairs = new List<KeyValuePair<int, int?>>();

            foreach (var idKey in collection.AllKeys)
            {
                if (idKey.StartsWith(idPrefix) && !string.IsNullOrWhiteSpace(collection[idKey]))
                {
                    var idFieldValue = Regex.Replace(idKey, @"[^\d]", "");
                    var idValue = collection[idKey].Split(',')[0];
                    var id = Convert.ToInt32(idValue);

                    int? quantity = null;
                    var quantityKey = collection.AllKeys.SingleOrDefault(f => f == quantityPrefix + idFieldValue);

                    if (quantityKey != null)
                    {
                        int result;
                        var quantityValue = collection[quantityKey].Split(',')[0];

                        if (int.TryParse(quantityValue, out result))
                        {
                            quantity = result;
                        }
                    }

                    pairs.Add(new KeyValuePair<int, int?>(id, quantity));
                }
            }

            return pairs.Distinct();
        }

        public static IComparable GetValue(this object entity, IEnumerable<string> propertyNames)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (propertyNames == null)
            {
                throw new ArgumentNullException("propertyNames");
            }

            var property = entity.GetType().GetProperty(propertyNames.First());

            if (property == null)
            {
                return null;
            }

            var value = property.GetValue(entity);

            if (propertyNames.Count() > 1)
            {
                value = GetValue(value, propertyNames.Skip(1));
            }

            return value as IComparable;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public static string ActionLink(this Controller controller, string linkText, string actionName, string controllerName, object routeValues)
        {
            return string.Format("<a href=\"{0}\">{1}</a>", controller.Url.Action(actionName, controllerName, routeValues), linkText);
        }

        public static string CrudLinks(this Controller controller, object routeValues)
        {
            var edit = ActionLink(controller, "Edit", "Edit", routeValues);
            var details = ActionLink(controller, "Details", "Details", routeValues);
            var delete = ActionLink(controller, "Delete", "Delete", routeValues);

            var links = string.Format("{0} | {1} | {2}", edit, details, delete);

            return links;
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

        public static IDictionary<int, int> GetQuantities(this FormCollection collection, string prefix, IEnumerable<int> ids)
        {
            var quantities = new Dictionary<int, int>();

            foreach (var key in collection.AllKeys)
            {
                foreach (var id in ids)
                {
                    if (key.Equals(prefix + id.ToString()) && !string.IsNullOrWhiteSpace(collection[key]))
                    {
                        var value = collection[key].Split(',')[0];
                        quantities.Add(id, Convert.ToInt32(value));
                    }
                }
            }

            return quantities;
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
    }
}
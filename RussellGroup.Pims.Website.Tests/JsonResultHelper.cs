using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace RussellGroup.Pims.Website.Tests
{
    public static class JsonResultHelper
    {
        public static object GetValue(this JsonResult result, string propertyName)
        {
            IDictionary<string, object> wrapper = new RouteValueDictionary(result.Data);

            return wrapper[propertyName];
        }

        public static T GetValue<T>(this JsonResult result, string propertyName)
        {
            return (T)GetValue(result, propertyName);
        }

        public static string[][] GetJqueryDataTableData(this JsonResult result)
        {
            var data = ((IEnumerable<object>)result.GetValue("Data"));

            var transformed = data.Select(datum =>
                    ConvertAnonymousType(datum).Select(element => element != null ? element.ToString() : null).ToArray()
                );

            return transformed.ToArray();
        }

        private static IEnumerable<object> ConvertAnonymousType(object value)
        {
            var type = value.GetType();

            if (!IsAnonymousType(type))
            {
                throw new ArgumentNullException("value", "The value must be an anonymous type.");
            }

            var genericType = type.GetGenericTypeDefinition();

            var parameterTypes = genericType
                .GetConstructors()[0]
                .GetParameters()
                .Select(p => p.ParameterType)
                .ToList();

            var propertyNames = genericType
                .GetProperties()
                .OrderBy(p => parameterTypes.IndexOf(p.PropertyType))
                .Select(p => p.Name);

            return propertyNames
                .Select(name => type
                    .GetProperty(name)
                    .GetValue(value, null))
                    .ToArray();
        }

        private static bool IsAnonymousType(Type type)
        {
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                       && type.IsGenericType && type.Name.Contains("AnonymousType")
                       && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) ||
                           type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
                       && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
    }
}

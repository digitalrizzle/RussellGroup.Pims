using System;
using System.Collections.Generic;
using System.Linq;
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

        public static string[][] GetJqueryDataTableAaData(this JsonResult result)
        {
            var data = ((IEnumerable<string[]>)result.GetValue("aaData")).ToArray();

            return data;
        }
    }
}

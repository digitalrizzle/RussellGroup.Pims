using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.Website.Helpers
{
    public static class DateTimeHelper
    {
        public static string ToIso8601DateString(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        public static DateTime UnixTimeToDateTime(this double unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime).ToLocalTime();
        }
    }
}
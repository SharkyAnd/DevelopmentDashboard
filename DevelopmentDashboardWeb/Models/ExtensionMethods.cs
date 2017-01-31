using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevelopmentDashboardWeb.Models
{
    public static partial class DateTimeExtensions
    {
        public static double ToJson(this DateTime dt)
        {
            return dt.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
    }

    public static class ExtensionMethods
    {
        public static int RoundOff(double i)
        {
            if (i >= 1)
                return Convert.ToInt32(i);
            if (i - Math.Truncate(i) > 0)
                return 1;
            else
                return 0;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Helpers
{
    public class DateTimeHelper
    {
        public static bool IsTodayLastDayOfMonth()
        {
            var now = DateTime.UtcNow;
            return now.Day == DateTime.DaysInMonth(now.Year, now.Month);
        }
    }
}
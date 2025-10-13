using System;
using System.Runtime.InteropServices;

namespace Keswa.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo EgyptZone;

        static DateTimeHelper()
        {
            try
            {
                // معرف المنطقة الزمنية يختلف بين ويندوز ولينكس
                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Egypt Standard Time"
                    : "Africa/Cairo";
                EgyptZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                // في حالة عدم العثور على المنطقة، استخدم التوقيت العالمي كبديل افتراضي
                EgyptZone = TimeZoneInfo.Utc;
            }
        }

        /// <summary>
        /// يرجع التاريخ والوقت الحالي حسب توقيت جمهورية مصر العربية
        /// </summary>
        public static DateTime EgyptNow
        {
            get
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EgyptZone);
            }
        }
    }
}
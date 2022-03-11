using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A_Common_Library.Data
{
    class TimeZoneUtility
    {
        private static string Current_TimeZone = "New Zealand Standard Time";

        /// <summary>SetTimeZone
        /// This function takes a standard Time Zone string.
        /// Please refer to C# documentation on Time zone standard names.
        /// The default value is "New Zealand Standard Time".
        /// </summary>
        public static void SetTimeZone(string timezone)
        {
            Current_TimeZone = timezone;
        }
        //public static string GetTimeZone()
        //{
        //    return Current_TimeZone;
        //}

        /// <summary>NZ_Now
        /// This function will return DateTime object on New Zealand standard time Zone.
        /// </summary>
        /// <returns></returns>
        public static DateTime NZ_Now()
        {
            var nzTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
            var utcNow = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, nzTimeZoneInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static DateTime Now()
        {
            var nzTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(Current_TimeZone);
            var utcNow = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, nzTimeZoneInfo);
        }

        public static DateTime Now(string tz)
        {
            try
            {
                var nzTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(tz);
                var utcNow = DateTime.UtcNow;
                return TimeZoneInfo.ConvertTimeFromUtc(utcNow, nzTimeZoneInfo);
            }
            catch
            {
                throw new TimeZoneNotFoundException("Given Time Zone is not valid");
            }
        }
    }
}
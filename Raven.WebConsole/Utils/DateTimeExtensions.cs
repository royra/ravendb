using System;
using System.Text.RegularExpressions;

namespace Raven.WebConsole.Utils
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1);
        
        public static long ToJavascriptDate(this DateTime d)
        {
            return (long) ((d.ToUniversalTime() - epoch).TotalMilliseconds);
        }

        public static long? ToJavascriptDate(this DateTime? d)
        {
            if (!d.HasValue)
                return null;

            return d.Value.ToJavascriptDate();
        }

        private static readonly Regex parseJsonDate = new Regex(@"^/Date\(([0-9]+)\)/$");
        public static DateTime? FromJsonDate(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;

            var match = parseJsonDate.Match(s);
            if (!match.Success)
                throw new Exception(string.Format("Invalid date format: {0}", s));

            var millisecondsFromEpoch = long.Parse(match.Groups[1].Value);
            return epoch.AddMilliseconds(millisecondsFromEpoch);
        }
    }
}
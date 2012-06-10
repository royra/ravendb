using System;

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
    }
}
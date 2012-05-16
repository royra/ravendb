using System;

namespace Raven.WebConsole.Utils
{
    public static class DecimalExtensions
    {
        public static decimal TruncateDigits(this decimal d, int numDigits)
        {
            if (numDigits < 0)
                throw new ArgumentOutOfRangeException("numDigits", "must be greater or equal to zero");

            var exp = (decimal)Math.Pow(10, numDigits);
            return Math.Truncate(d*exp)/exp;
        }

        public static decimal GetTruncatedMbytes(this long l, int numDigits = 2)
        {
            return ((decimal) l/(1024*1024)).TruncateDigits(numDigits);
        }
    }
}
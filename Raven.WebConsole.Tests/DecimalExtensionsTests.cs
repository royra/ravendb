using System;
using Xunit;

namespace Raven.WebConsole.Tests
{
    public class DecimalExtensionsTests
    {
        [Fact]
        public void TruncateSanity()
        {
            Assert.Equal(Utils.DecimalExtensions.TruncateDigits(new decimal(123.456), 2), new decimal(123.45));
            Assert.Equal(Utils.DecimalExtensions.TruncateDigits(new decimal(123.45), 2), new decimal(123.45));
            Assert.Equal(Utils.DecimalExtensions.TruncateDigits(new decimal(123.4), 2), new decimal(123.4));
            Assert.Equal(Utils.DecimalExtensions.TruncateDigits(new decimal(123), 2), new decimal(123));
            Assert.Equal(Utils.DecimalExtensions.TruncateDigits(new decimal(0), 2), new decimal(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => Utils.DecimalExtensions.TruncateDigits(new decimal(123.456), -1));
        }

        [Fact]
        public void GetTruncatedMbytesSanity()
        {
            Assert.Equal(Utils.DecimalExtensions.GetTruncatedMbytes(0), new decimal(0));
            Assert.Equal(Utils.DecimalExtensions.GetTruncatedMbytes(1103421), new decimal(1.05));
            Assert.Equal(Utils.DecimalExtensions.GetTruncatedMbytes(1103421, 1), new decimal(1.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => Utils.DecimalExtensions.GetTruncatedMbytes(1103421, -1));
        }
    }
}

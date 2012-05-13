using Raven.WebConsole.Entities;
using Xunit;

namespace Raven.WebConsole.Tests.Entities
{
    public class PasswordTests
    {
        [Fact]
        public void PasswordCanBeChecked()
        {
            var p = new Password("hunter");
            Assert.True(p.Check("hunter"));
            Assert.False(p.Check("hunter1"));
        }
    }
}

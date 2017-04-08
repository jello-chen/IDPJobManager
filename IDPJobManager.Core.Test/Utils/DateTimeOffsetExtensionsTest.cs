using System;
using Xunit;
using IDPJobManager.Core.Utils;

namespace IDPJobManager.Core.Test.Utils
{
    public class DateTimeOffsetExtensionsTest
    {
        [Fact]
        public void UnixTicks_MinDateTime_ReturnsLessThanZero()
        {
            var min = DateTime.MinValue;
            Assert.True(min.UnixTicks() < 0);
        }

        [Fact]
        public void UnixTicks_UnixDateTime_ReturnsZero()
        {
            var dt = new DateTime(1970, 1, 1);
            Assert.True(dt.UnixTicks() == 0);
        }

        [Fact]
        public void UnixTicks_MaxDateTime_ReturnsGreaterThanZero()
        {
            var max = DateTime.MaxValue;
            Assert.True(max.UnixTicks() > 0);
        }
    }
}

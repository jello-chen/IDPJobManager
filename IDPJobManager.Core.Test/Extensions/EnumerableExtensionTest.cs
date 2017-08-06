using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using IDPJobManager.Core.Extensions;

namespace IDPJobManager.Core.Test.Extensions
{
    public class EnumerableExtensionTest
    {
        [Fact]
        public void IsNullOrEmpty_InputsNullDictionaryReturnsTrue()
        {
            IDictionary<string, object> nullDict = null;
            Assert.True(nullDict.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_InputsEmptyDictionaryReturnsTrue()
        {
            IDictionary<string, object> emptyDict = new Dictionary<string, object>();
            Assert.True(emptyDict.IsNullOrEmpty());
        }
    }
}

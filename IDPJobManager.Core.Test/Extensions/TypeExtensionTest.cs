using Xunit;
using IDPJobManager.Core.Extensions;
using System;

namespace IDPJobManager.Core.Test.Extensions
{
    public class TypeExtensionTest
    {
        [Fact]
        public void Is_Null_ThrowsArgumentNullException()
        {
            Type type = null;
            Assert.Throws<ArgumentNullException>(() => type.Is<int>());
        }

        [Fact]
        public void Is_Generic_Int_Object_ReturnsBool()
        {
            Assert.True(typeof(int).Is<object>());
        }

        [Fact]
        public void Is_Int_Object_ReturnsBool()
        {
            Assert.True(typeof(int).Is(typeof(object)));
        }
    }
}

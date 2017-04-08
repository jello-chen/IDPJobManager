using Nancy;
using Nancy.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IDPJobManager.Web.Test.Modules
{
    public class HomeModuleTest
    {
        [Fact]
        public void Should_returns_status_ok_when_route_exists()
        {
            //Given
            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper);

            //When
            var result = browser.Get("/", with =>
            {
                with.HttpRequest();
            });

            //Then
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}

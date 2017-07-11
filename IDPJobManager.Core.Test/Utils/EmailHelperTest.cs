using IDPJobManager.Core.Utils;
using Xunit;

namespace IDPJobManager.Core.Test.Utils
{
    public class EmailHelperTest
    {
        [Fact]
        public void Send_ReturnsTrue()
        {
            EmailHelper emailHelper = new EmailHelper();
            emailHelper.From = "jello@qq.com";
            emailHelper.To = new string[] { "jello@qq.com" };
            emailHelper.Subject = "Jello";
            emailHelper.Body = "Jello";
            emailHelper.FromEmailPassword = "*************";//Here input authorization code after openning POP3/SMTP service when QQ mail
            emailHelper.Host = "smtp.qq.com";
            emailHelper.Port = 25;
            Assert.True(emailHelper.Send());
        }
    }
}

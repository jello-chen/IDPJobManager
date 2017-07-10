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
            emailHelper.From = "949908998@qq.com";
            emailHelper.To = new string[] { "chenjinliang3@huawei.com" };
            emailHelper.Subject = "Jello";
            emailHelper.Body = "Jello";
            emailHelper.FromEmailPassword = "*************";//Here input authorization code after openning POP3/SMTP service when QQ mail
            emailHelper.Host = "smtp.qq.com";
            emailHelper.Port = 25;
            Assert.True(emailHelper.Send());
        }
    }
}

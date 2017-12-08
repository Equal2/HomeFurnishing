using Data.Models;
using Model.Models;
using System.Configuration;
using Mailer;
using Mailer.Model;
using System.Threading.Tasks;
using System.Linq;
//using Models.Login.Infrastructure;

namespace EmailContents
{
    public class RegistrationEmailNotification
    {
        public async Task SendNewUserRegistrationNotification(string UserId)
        {
            EmailMessage message = new EmailMessage();
            message.Subject = "New user registered";
            string temp = (ConfigurationManager.AppSettings["SalesManager"]);
            string domain = ConfigurationManager.AppSettings["CurrentDomain"];
            message.To = "madhankumar191@gmail.com";
            string link = domain + "/SaleOrderHeader/Detail/" + UserId + "?transactionType=approve";

            SaleOrderHeader doc = new SaleOrderHeader();

            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                var RegUser = (from p in context.Users
                               where p.Id == UserId
                               select p).FirstOrDefault();

                message.Body += "New user " + RegUser.UserName + " registered";

            }
            SendEmail se = new SendEmail();
            await se.configSendGridasync(message);
        }

    }
}

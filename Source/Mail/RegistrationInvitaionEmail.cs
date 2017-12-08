using Model.Models;
using System.Configuration;
using Mailer;
using Mailer.Model;
using System.Threading.Tasks;
using System;

namespace EmailContents
{
    public class RegistrationInvitaionEmail
    {
        public async Task SendUserRegistrationInvitation(string ToEmailId, int AppId, string InvitationBy, string InvitationByEmail, DateTime InvitationDate, string UserRole, int SiteId, int DivisionId)
        {
            EmailMessage message = new EmailMessage();
            message.Subject = "Invitation for registration";
            //string temp = (ConfigurationManager.AppSettings["SalesManager"]);
            string domain = ConfigurationManager.AppSettings["LoginDomain"];
            message.To = ToEmailId;
            string link = domain + "/Account/Register?ToEmailId=" + ToEmailId + "&AppId=" + AppId + "&InvitationBy=" + InvitationBy +
                    "&InvitationByEmail=" + InvitationByEmail + "&InvitationDate=" + InvitationDate + "&UserRole=" + UserRole + "&SiteId=" + SiteId + "&DivisionId=" + DivisionId;

            SaleOrderHeader doc = new SaleOrderHeader();

            message.Body += "Please use the link to register to the company. <a href='" + link + "' target='_blank'> Click Here </a>";

            if (!string.IsNullOrEmpty(InvitationByEmail))
                message.CC = InvitationByEmail;

            SendEmail se = new SendEmail();
            await se.configSendGridasync(message);
        }

    }
}

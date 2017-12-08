using Mailer.Model;
using SendGrid;
using Model.ViewModels;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Mailer
{
    public class SendEmail
    {
        public static void SendEmailMsg(EmailMessage message)
        {

            // Create the email object first, then add the properties.
            SendGridMessage myMessage = new SendGridMessage();
            myMessage.AddTo(message.To.Split(',').ToList());

            myMessage.Subject = message.Subject;
            myMessage.Html = message.Body;

            string username = ConfigurationManager.AppSettings["MailAccount"];
            string password = ConfigurationManager.AppSettings["Mailpassword"];
            string FromEmail = ConfigurationManager.AppSettings["EmailUser"];
            string FromEmailName = ConfigurationManager.AppSettings["EmailUserName"];

            myMessage.From = new MailAddress(FromEmail,FromEmailName);

            // Create credentials, specifying your user name and password.
            var credentials = new NetworkCredential(username, password);

            // Create an Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.
            // You can also use the **DeliverAsync** method, which returns an awaitable task.
            try
            {
                // Send the email.
                if (transportWeb != null)
                {
                    transportWeb.DeliverAsync(myMessage);
                }
                else
                {
                    Trace.TraceError("Failed to create Web transport.");
                    Task.FromResult(0);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message + " SendGrid probably not configured correctly.");
            }

        }


        public static void SendEmailMsgWithAttachment(EmailMessage message,string FilePath)
        {

            // Create the email object first, then add the properties.
            SendGridMessage myMessage = new SendGridMessage();
            myMessage.AddTo(message.To.Split(',').ToList());

            foreach(var item in message.CC.Split(','))
            myMessage.AddCc(item);

            foreach (var item in message.BCC.Split(','))
                myMessage.AddBcc(item);

            myMessage.Subject = message.Subject;
            myMessage.Html = message.Body;

            string username = ConfigurationManager.AppSettings["MailAccount"];
            string password = ConfigurationManager.AppSettings["Mailpassword"];
            string FromEmail = ConfigurationManager.AppSettings["EmailUser"];
            string FromEmailName = ConfigurationManager.AppSettings["EmailUserName"];

            myMessage.From = new MailAddress(FromEmail, FromEmailName);

            if(!string.IsNullOrEmpty(FilePath))
            {
                foreach(string item in FilePath.Split(',').ToList())
                myMessage.AddAttachment(item);
            }
            

            // Create credentials, specifying your user name and password.
            var credentials = new NetworkCredential(username, password);

            // Create an Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.
            // You can also use the **DeliverAsync** method, which returns an awaitable task.
            try
            {
                // Send the email.
                if (transportWeb != null)
                {
                    transportWeb.DeliverAsync(myMessage);
                }
                else
                {
                    Trace.TraceError("Failed to create Web transport.");
                    Task.FromResult(0);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message + " SendGrid probably not configured correctly.");
            }

        }


        public async Task configSendGridasync(EmailMessage message)
        {
            // Create the email object first, then add the properties.
            SendGridMessage myMessage = new SendGridMessage();
            myMessage.AddTo(message.To.Split(',').ToList());

            myMessage.Subject = message.Subject;
            myMessage.Html = message.Body;

            string username = ConfigurationManager.AppSettings["MailAccount"];
            string password = ConfigurationManager.AppSettings["Mailpassword"];
            string FromEmail = ConfigurationManager.AppSettings["EmailUser"];
            string FromEmailName = ConfigurationManager.AppSettings["EmailUserName"];

            myMessage.From = new MailAddress(FromEmail, FromEmailName);

            // Create credentials, specifying your user name and password.
            var credentials = new NetworkCredential(username, password);

            // Create an Web transport for sending email.
            var transportWeb = new Web(credentials);

            try
            {
                // Send the email.
                if (transportWeb != null)
                {
                    await transportWeb.DeliverAsync(myMessage);
                }
                else
                {
                    Trace.TraceError("Failed to create Web transport.");
                    await Task.FromResult(0);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message + " SendGrid probably not configured correctly.");
            }
        }

        public static void SendEmailMsg(EmailContentViewModel MailContents)
        {

            // Create the email object first, then add the properties.
            SendGridMessage myMessage = new SendGridMessage();
            myMessage.AddTo(MailContents.EmailToStr.Split(',').ToList());

            if (MailContents.EmailCcStr != null && MailContents.EmailCcStr != "")
            { 
                foreach (var item in MailContents.EmailCcStr.Split(','))
                {
                    myMessage.AddCc(item);
                }
            }

            if (MailContents.EmailBCcStr != null && MailContents.EmailBCcStr != "")
            {
                foreach (var item in MailContents.EmailBCcStr.Split(','))
                {
                    myMessage.AddBcc(item);
                }
            }
                

            myMessage.Subject = MailContents.EmailSubject;
            myMessage.Html = MailContents.EmailBody;

            string username = ConfigurationManager.AppSettings["MailAccount"];
            string password = ConfigurationManager.AppSettings["Mailpassword"];
            string FromEmail = MailContents.EmailUser;
            string FromEmailName = MailContents.EmailUserName;

            myMessage.From = new MailAddress(FromEmail, FromEmailName);

            if (!string.IsNullOrEmpty(MailContents.FileNameList))
            {
                foreach (string item in MailContents.FileNameList.Split(',').ToList())
                    myMessage.AddAttachment(item);
            }


            // Create credentials, specifying your user name and password.
            var credentials = new NetworkCredential(username, password);

            // Create an Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.
            // You can also use the **DeliverAsync** method, which returns an awaitable task.
            try
            {
                // Send the email.
                if (transportWeb != null)
                {
                    transportWeb.DeliverAsync(myMessage);
                }
                else
                {
                    Trace.TraceError("Failed to create Web transport.");
                    Task.FromResult(0);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message + " SendGrid probably not configured correctly.");
            }

        }

    }
}

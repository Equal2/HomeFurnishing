using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Surya.Notification
{
  public  class NotificationServicesSendEmail
    {
        public static string GmailUsername { get; set; }
       public static string GmailPassword { get; set; }
       public static string GmailHost { get; set; }
       public static int GmailPort { get; set; }
       public static bool GmailSSL { get; set; }

       public string ToEmail { get; set; }
       public string Subject { get; set; }
       public string Body { get; set; }
       public bool IsHtml { get; set; }

       public string CC { get; set; }

       static NotificationServicesSendEmail()
       {
           GmailHost = "smtp.gmail.com";
           GmailPort = 25; // Gmail can use ports 25, 465 & 587; but must be 25 for medium trust environment.
           GmailSSL = true;
       }

       //        Outgoing Mail (SMTP) Server - requires TLS or SSL:	 
       //smtp.gmail.com
       //Use Authentication: Yes
       //Port for TLS/STARTTLS: 587
       //Port for SSL: 465
       //Server timeouts	 Greater than 1 minute, we recommend 5

       //Full Name or Display Name:	 [your name]
       //Account Name or User Name:	 your full email address (including @gmail.com or @your_domain.com)
       //Email Address:	 your email address (user...@gmail.com or username@your_domain.com)
       //Password:	 your Gmail password


       public void Send()
       {
           SmtpClient smtp = new SmtpClient();
           smtp.Host = GmailHost;
           smtp.Port = GmailPort;
           smtp.EnableSsl = GmailSSL;
           smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
           smtp.UseDefaultCredentials = false;
           smtp.Credentials = new NetworkCredential(GmailUsername, GmailPassword);

           var message = new MailMessage();
           message.From = new MailAddress(GmailUsername);

           string[] ToMuliId = ToEmail.Split(',');
           foreach (string ToEMailId in ToMuliId)
           {
               message.To.Add(new MailAddress(ToEMailId)); //adding multiple TO Email Id
           }

           string[] CCId = CC.Split(',');

           foreach (string CCEmail in CCId)
           {
               message.CC.Add(new MailAddress(CCEmail)); //Adding Multiple CC email Id
           }


           //using (var message = new MailMessage(GmailUsername, ToEmail))
           //{

           message.Subject = Subject;
           message.Body = Body;
           message.IsBodyHtml = IsHtml;
           smtp.Send(message);
           //}
       }



       public void Send(string toAddress, string ccAddress, string fromAddress, string body)
       {
           SmtpClient smtp = new SmtpClient();
           smtp.Host = GmailHost;
           smtp.Port = GmailPort;
           smtp.EnableSsl = GmailSSL;
           smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
           smtp.UseDefaultCredentials = false;
           GmailUsername = ConfigurationManager.AppSettings["GmailUser"].ToString();
           GmailPassword = ConfigurationManager.AppSettings["Gmailpassword"].ToString();
           smtp.Credentials = new NetworkCredential(GmailUsername, GmailPassword);

           var message = new MailMessage();
           message.From = new MailAddress(GmailUsername);

           string[] ToMuliId = toAddress.Split(',');
           foreach (string ToEMailId in ToMuliId)
           {
               message.To.Add(new MailAddress(ToEMailId)); //adding multiple TO Email Id
           }

           string[] CCId = ccAddress.Split(',');

           foreach (string CCEmail in CCId)
           {
               message.CC.Add(new MailAddress(CCEmail)); //Adding Multiple CC email Id
           }


           //using (var message = new MailMessage(GmailUsername, ToEmail))
           //{

           message.Subject = Subject;
           message.Body = Body;
           message.IsBodyHtml = true;
           smtp.Send(message);
           //}
       }


    }
}

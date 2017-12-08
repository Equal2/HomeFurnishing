using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Reports.Presentation.Helper
{
    public class GMailer
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

        static GMailer()
        {
            GmailHost = ConfigurationManager.AppSettings["EmailHost"]; // "smtp.gmail.com";

            int port=25;
            int.TryParse( ConfigurationManager.AppSettings["EmailPort"], out port ) ; // Gmail can use ports 25, 465 & 587; but must be 25 for medium trust environment.
            GmailPort = port;

            bool SSL = true;
            bool.TryParse(ConfigurationManager.AppSettings["EmailSSL"], out SSL);
            GmailSSL = SSL;
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
            
            bool isEmailEnabled = true;
            bool.TryParse(ConfigurationManager.AppSettings["EmailEnable"], out isEmailEnabled);
            if (!isEmailEnabled) return; 


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
            if (!string.IsNullOrEmpty(CC))
            { 
                string[] CCId = CC.Split(',');

                foreach (string CCEmail in CCId)
                {
                    message.CC.Add(new MailAddress(CCEmail)); //Adding Multiple CC email Id
                }
            }
            
            //using (var message = new MailMessage(GmailUsername, ToEmail))
            //{
                    
            message.Subject = Subject;
            message.Body = Body;
            message.IsBodyHtml = IsHtml;
            smtp.Send(message);
            //}
        }

        
    }
}

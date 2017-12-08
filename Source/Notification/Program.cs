using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;

namespace Surya.Notification
{
   public class Program
    {
      

        static void Main(string[] args)
        {
           SendNotification();
           SendEscalation();
        }

       public static void SendNotification()
        {
            NotificationServiceEmailContent notificationEmailContent= new NotificationServiceEmailContent();
            NotificationServicesSendEmail notificationServiceSendEmail = new NotificationServicesSendEmail();
            notificationServiceSendEmail.ToEmail = ConfigurationManager.AppSettings["notificationToEmail"].ToString();
            notificationServiceSendEmail.CC = ConfigurationManager.AppSettings["notificationCCEmail"].ToString();
            notificationServiceSendEmail.Subject = ConfigurationManager.AppSettings["Subject"].ToString()  + DateTime.Now + "   testing";

            notificationServiceSendEmail.Body = notificationEmailContent.GetNotificationContent();
            notificationServiceSendEmail.Send(notificationServiceSendEmail.ToEmail, notificationServiceSendEmail.CC, notificationServiceSendEmail.ToEmail, notificationServiceSendEmail.Body);

        }

       public static void SendEscalation()
       {
           NotificationServiceEmailContent notificationEmailContent = new NotificationServiceEmailContent();
           NotificationServicesSendEmail notificationServiceSendEmail = new NotificationServicesSendEmail();
           NotificationServicesDataAccess notificationDataAcce = new NotificationServicesDataAccess();

           var purchaseOrder = notificationDataAcce.GetData();
          
           notificationServiceSendEmail.CC = ConfigurationManager.AppSettings["notificationCCEmail"].ToString();
           notificationServiceSendEmail.Subject = ConfigurationManager.AppSettings["SubjectEscalation"].ToString() + DateTime.Now + "   testing";
           foreach (PurchaseOrderHeader poh in purchaseOrder)
           {
               notificationServiceSendEmail.ToEmail = poh.Email;
               notificationServiceSendEmail.Body = notificationEmailContent.GetEscalationContent(poh);
               if (!string.IsNullOrEmpty(notificationServiceSendEmail.Body))
               notificationServiceSendEmail.Send(notificationServiceSendEmail.ToEmail, notificationServiceSendEmail.CC, notificationServiceSendEmail.ToEmail, notificationServiceSendEmail.Body);

           }
       }
    }

  


   
}

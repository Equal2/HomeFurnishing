using System;
using System.Collections.Generic;
using System.Diagnostics;
using Service;
using Data.Models;
using System.Configuration;
using Mailer;
using Core.Common;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Mailer.Model;

namespace EmailContents
{
    public class DailyEmailContent
    {
        public static void DataCheckupEMail()
        {

            try
            {
                using (StreamWriter writer =
                 new StreamWriter(@"c:\temp\Cachecallback.txt", true))
                {
                    writer.WriteLine("Daily Data Checkup Email Event", DateTime.Now);
                    writer.Close();
                }
            }
            catch (Exception x)
            {
                Debug.WriteLine(x);
            }




            EmailMessage message = new EmailMessage();
            message.Subject = "Daily Data Checkup Reports";


            string ToAddress = "meet2arpitg@gmail.com";
            string CCAddress = "satyam.tripathi07@gmail.com,singh.akash409@gmail.com ";


            string domain = ConfigurationManager.AppSettings["domain"];

            message.To = ToAddress;
            message.CC = CCAddress;



            try
            {

                string FilenameList;

                string GenFileName = ReportFiles.CreateFiles("SpCheck__JobOrderHeaderStatus", "", "");

                using (StreamWriter writer =
                new StreamWriter(@"c:\temp\Cachecallback.txt", true))
                {
                    writer.WriteLine(GenFileName);
                    writer.Close();
                }



                FilenameList = GenFileName;
                SendEmail.SendEmailMsgWithAttachment(message, FilenameList);

            }

            catch (Exception ex)
            {
                return;
            }

        }
    }
}

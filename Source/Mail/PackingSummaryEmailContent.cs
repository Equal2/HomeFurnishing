using System;
using System.Collections.Generic;
using System.Diagnostics;
using Surya.India.Service;
using Surya.India.Data.Models;
using System.Configuration;
using Mailer;
using Surya.India.Core.Common;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Mailer.Model;

namespace EmailContents
{
    public class PackingSummaryEmailContent
    {
        public static void DailyPackingReviewEmail()
        {

              try
            {
                using (StreamWriter writer =
                 new StreamWriter(@"c:\temp\Cachecallback.txt", true))
                {
                    writer.WriteLine("Calling Email Event", DateTime.Now);
                    writer.Close();
                }
            }
              catch (Exception x)
              {
                  Debug.WriteLine(x);
              }

            string FilePath = "";
            EmailMessage message = new EmailMessage();
            message.Subject = "Packing Summary Details";
            string ToAddress = (ConfigurationManager.AppSettings["MD"]);
            string CCAddress = (ConfigurationManager.AppSettings["Surya"]) + "," + (ConfigurationManager.AppSettings["SalesManager"]) + "," + (ConfigurationManager.AppSettings["CA"]) + "," + (ConfigurationManager.AppSettings["AdminEmail"]);
            string domain = ConfigurationManager.AppSettings["domain"];
            //message.To = "madhankumar191@gmail.com";
            message.To = ToAddress;
            message.CC = CCAddress;
            string ToDate=DateTime.Now.ToString("dd/MMM/yyyy");
            string FromDate = DateTime.Now.Subtract(TimeSpan.FromDays(7)).ToString("dd/MMM/yyyy");

           // ApplicationDbContext Db = new ApplicationDbContext();
            String queryString =  "Web.ProcDailyPackingReview '" + ToDate + "' ";
            String SubqueryString =  "Web.ProcDailyPackingReview1 '" + ToDate + "' ";

            //String queryString = Db.strSchemaName + ".[ProcSaleOrderReport] NULL,NULL,   '31/Mar/2015'  ";


            DataTable Dt = new DataTable();
            DataTable Dt2=new DataTable();
            var SubReportDataList = new List<DataTable>();
            var SubReportNameList = new List<string>();

            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString()))
                {
                    SqlDataAdapter sqlDataAapter = new SqlDataAdapter(queryString.ToString(), sqlConnection);
                    sqlDataAapter.Fill(Dt);

                    SqlDataAdapter sqlDataAapter2 = new SqlDataAdapter(SubqueryString.ToString(), sqlConnection);
                    sqlDataAapter2.Fill(Dt2);
                }
                SubReportDataList.Add(Dt2);
                SubReportNameList.Add("DailyPackingReview_SubRep1");

                ReportGenerateService c = new ReportGenerateService();
                string mimetype = "";
                string Filename = GenerateDocument(c.ReportGenerate(Dt, out mimetype, ReportFileTypeConstants.PDF,null, SubReportDataList,null,SubReportNameList), "PackingSummary_" + DateTime.Now.ToString("dd-MMM-yyyy-HH_mm_ss"), mimetype);


           

                message.Body += "Packing Summary Details. PFA with detailed information";

            

            SendEmail.SendEmailMsgWithAttachment(message, Filename);

            }

             catch(Exception ex)
            {
                return;
            }

        }

        public static string GenerateDocument(byte[] Bytes,string FileName,string MimeType)
        {
            string output;
            if (MimeType == "application/pdf")
            {
                output = ".pdf";
            }
            else
                output = ".pdf";

            string path = System.AppDomain.CurrentDomain.BaseDirectory + "App_Data\\" + FileName+output;

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                stream.Write(Bytes, 0, Bytes.Length);
            }

            return path;
        }


    }
}

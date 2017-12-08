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
using Microsoft.Reporting.WebForms;
using Mailer.Model;

namespace EmailContents
{
    public class EMailContent
    {
        public string Subject { get; set; }
        public string ToAddress { get; set; }
        public string CCAddress { get; set; }
        public string BCCAddress { get; set; }
        public string Body { get; set; }
        public string FileName { get; set; }
    }

    public class ReportFiles
    {
        public static string CreateFiles(String QueryProcedure, string FileName, string FilterParameter)
        {


            string queryString = "Web." + QueryProcedure;


            DataTable Dt = new DataTable();
            var SubReportDataList = new List<DataTable>();
            var SubReportNameList = new List<string>();

            List<string> SubReportProcList = new List<string>();

            String StrSubReportProcList;
            String ReportTitle;

            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString()))
                {
                    SqlDataAdapter sqlDataAapter = new SqlDataAdapter(queryString.ToString(), sqlConnection);
                    sqlDataAapter.Fill(Dt);

                }



                if (Dt.Columns.Contains("SubReportProcList"))
                {
                    StrSubReportProcList = Dt.Rows[0]["SubReportProcList"].ToString();
                }
                else
                {

                    return "SubReportProcList Not Define";
                }


                if (Dt.Columns.Contains("ReportTitle"))
                {
                    ReportTitle = Dt.Rows[0]["ReportTitle"].ToString();
                }
                else
                {

                    return "ReportTitle Not Define";
                }



                if (Dt.Rows.Count > 0)
                {
                    if (Dt.Columns.Contains("SubReportProcList"))
                    {
                        SubReportProcList.Add(Dt.Rows[0]["SubReportProcList"].ToString());
                    }


                    DataTable SubRepData = new DataTable();
                    String SubReportProc;

                    if (SubReportProcList != null)
                    {
                        if (SubReportProcList.Count > 0)
                        {


                            SubRepData = Dt.Copy();

                            SqlConnection Con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString());

                            while (SubRepData.Rows.Count > 0 && SubRepData.Columns.Contains("SubReportProcList"))
                            {
                                SubReportProc = SubRepData.Rows[0]["SubReportProcList"].ToString();

                                if (SubReportProc != "")
                                {
                                    String query = "Web." + SubReportProc;
                                    SqlDataAdapter sqlDataAapter1 = new SqlDataAdapter(query.ToString(), Con);
                                    SubRepData.Reset();
                                    sqlDataAapter1.Fill(SubRepData);
                                    DataTable SubDataTable = new DataTable();
                                    SubDataTable = SubRepData.Copy();



                                    string SubRepName = "";
                                    if (SubDataTable.Rows.Count > 0)
                                    {
                                        SubReportDataList.Add(SubDataTable);
                                        SubRepName = (string)SubDataTable.Rows[0]["ReportName"];
                                        SubReportNameList.Add(SubRepName);
                                    }
                                    SubDataTable.Dispose();

                                }
                                else
                                {
                                    break;
                                }

                            }



                        }
                    }


                }



                ReportGenerateService c = new ReportGenerateService();
                string mimetype = "";
                string ReportFileName = "";

                ReportFileName = FileName == "" ? ReportTitle : FileName;


                var Paralist = new List<string>();
                string[] ParaA = null;

                if (FilterParameter != "")
                {
                    ParaA = FilterParameter.Split(new Char[] { ',' });
                }


                if (ParaA.Length > 0)
                {
                    foreach (var Para in ParaA)
                    {
                        Paralist.Add(Para);
                    }
                }




                string Filename = GenerateDocument(c.ReportGenerate(Dt, out mimetype, ReportFileTypeConstants.PDF, Paralist, SubReportDataList, null, SubReportNameList), ReportFileName + "_" + DateTime.Now.ToString("dd-MMM-yyyy-HH_mm_ss"), mimetype);




                // message.Body += ReportTitle + "'<br />'";




                return Filename;
            }



            catch (Exception ex)
            {
                using (StreamWriter writer =
               new StreamWriter(@"c:\temp\Cachecallback.txt", true))
                {
                    writer.WriteLine("Excp:" + ex.Message + "EX2:" + ex.InnerException.Message + "EX3:" + ex.InnerException.InnerException.Message + "EX4:" + ex.InnerException.InnerException.InnerException.Message);
                    writer.Close();
                }

                return ex.Message;
            }

        }

        public static string GenerateDocument(byte[] Bytes, string FileName, string MimeType)
        {
            FileName = FileName.Replace(":", "_");
            string output;
            if (MimeType == "application/pdf")
            {
                output = ".pdf";
            }
            else if (MimeType == "application/vnd.ms-excel")
            {
                output = ".xls";
            }
            else
                output = ".pdf";


            string path = System.AppDomain.CurrentDomain.BaseDirectory + "App_Data\\" + FileName + output;



            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                stream.Write(Bytes, 0, Bytes.Length);
            }


            return path;
        }

        public static string CreateFiles(String QueryProcedure, String QueryParameter, string ReportFormatType, string BaseDirectoryPath = null)
        {
            string queryString = QueryProcedure + " " + QueryParameter;

            DataTable Dt = new DataTable();
            var SubReportDataList = new List<DataTable>();

            String StrSubReportProcList;
            String ReportTitle;

            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString()))
                {
                    SqlDataAdapter sqlDataAapter = new SqlDataAdapter(queryString.ToString(), sqlConnection);
                    sqlDataAapter.Fill(Dt);

                }



                if (Dt.Columns.Contains("SubReportProcList"))
                {
                    StrSubReportProcList = Dt.Rows[0]["SubReportProcList"].ToString();
                }
                else
                {

                    return "SubReportProcList Not Define";
                }


                if (Dt.Columns.Contains("ReportTitle"))
                {
                    ReportTitle = Dt.Rows[0]["ReportTitle"].ToString();
                }
                else
                {

                    return "ReportTitle Not Define";
                }


                if (StrSubReportProcList != "")
                {

                    string[] SubReportProcList = StrSubReportProcList.Split(new Char[] { ',' });


                    if (SubReportProcList.Length > 0)
                    {
                        SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString());

                        foreach (var SubReportProc in SubReportProcList)
                        {

                            String query = "Web." + SubReportProc;
                            DataTable SubReport1Data = new DataTable();


                            SqlDataAdapter sqlDataAapter1 = new SqlDataAdapter(query.ToString(), sqlConnection);
                            sqlDataAapter1.Fill(SubReport1Data);


                            SubReportDataList.Add(SubReport1Data);

                            SubReport1Data = null;

                        }
                    }
                }


                ReportGenerateService c = new ReportGenerateService();
                string mimetype = "";
                string Filename = GenerateDocument(c.ReportGenerate(Dt, out mimetype, ReportFormatType, null, SubReportDataList, BaseDirectoryPath), ReportTitle + "_" + DateTime.Now.ToString("dd-MMM-yyyy-HH_mm_ss"), mimetype);





                return Filename;
            }



            catch (Exception ex)
            {
                return null;
            }

        }

        public static string CreateFilesFromSQLReporting(String ReportName, string FileName, string FilterParameter, string UserNameValue, string Parameters, string ParameterValue )
        {


            List<ReportParameter> Params = new List<ReportParameter>();


            string[] ParameterList = Parameters.Split(',');
            string[] ParameterValueList = ParameterValue.Split(',');


            int Length = 0;

            foreach ( string item in ParameterList)
            {
                
                ReportParameter Parameter = new ReportParameter();
                Parameter.Name = ParameterList[Length];
                Parameter.Values.Add(ParameterValueList[Length]);
                Params.Add(Parameter);
                Length = Length + 1;

            }


            ReportParameter UserName = new ReportParameter();
            UserName.Name = "PrintedBy";
            UserName.Values.Add(UserNameValue);
            Params.Add(UserName);

            ReportParameter ConString = new ReportParameter();
            ConString.Name = "DatabaseConnectionString";
            ConString.Values.Add(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString());
            Params.Add(ConString);



            ReportGenerateService c = new ReportGenerateService();
                string mimetype = "";
                string ReportFileName = "";

                ReportFileName = FileName;



            try
            {

                string Filename = GenerateDocument(c.ReportGenerateCustom(out mimetype, ReportFileTypeConstants.PDF,"Satyam.Tripathi", Params, ReportName), ReportFileName + "_" + DateTime.Now.ToString("dd-MMM-yyyy-HH_mm_ss"), mimetype);



                return Filename;
            }



            catch (Exception ex)
            {
                using (StreamWriter writer =
               new StreamWriter(@"c:\temp\Cachecallback.txt", true))
                {
                    writer.WriteLine("Excp:" + ex.Message + "EX2:" + ex.InnerException.Message + "EX3:" + ex.InnerException.InnerException.Message + "EX4:" + ex.InnerException.InnerException.InnerException.Message);
                    writer.Close();
                }

                return ex.Message;
            }

        }


    }
}

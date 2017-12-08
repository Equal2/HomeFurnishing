using Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using Model;
using Models.Reports.Models;
using Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;


namespace DocumentPrint
{
    public class DirectReportPrint
    {
        public byte[] DirectDocumentPrint(String queryString, string UserName, int DocumentId = 0, string ReportFileType = ReportFileTypeConstants.PDF)
        {
            var SubReportDataList = new List<DataTable>();
            var SubReportNameList = new List<string>();
            DataTable Dt = new DataTable();
            DataTable SubRepData = new DataTable();
            String SubReportProc;

            String MainQuery = queryString + " " + DocumentId.ToString();
            String StrSubReportProcList;
            using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
            {
                SqlDataAdapter sqlDataAapter = new SqlDataAdapter(MainQuery, sqlConnection);
                sqlDataAapter.Fill(Dt);
            }

            if (Dt.Columns.Contains("SubReportProcList"))
            {
                StrSubReportProcList = Dt.Rows[0]["SubReportProcList"].ToString();
            }
            else
            {
                //ViewBag.Message = "SubReportProcList is not define.";
                //return View("Close");
            }

            SubRepData = Dt.Copy();

            SqlConnection Con = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]);

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
                    //SubRepData = null;
                    break;
                }
            }



            string mimtype;
            ReportGenerateService c = new ReportGenerateService();


            string mimetype = "";
            return c.ReportGenerate(Dt, out mimtype, ReportFileTypeConstants.PDF, null, SubReportDataList, null, SubReportNameList, UserName);

        }

        public byte[] rsDirectDocumentPrint(String ReportSQL, string UserName, int DocumentId = 0, string ReportFileType = ReportFileTypeConstants.PDF)
        {
            string ReportName = "";
            List<ReportParameter> Params = new List<ReportParameter>();

            ReportParameter Param = new ReportParameter();
            Param.Name = "Id";
            Param.Values.Add(DocumentId.ToString());

            Params.Add(Param);

            ReportParameter rpUserName = new ReportParameter();
            rpUserName.Name = "PrintedBy";
            rpUserName.Values.Add(UserName);
            Params.Add(rpUserName);

            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            ReportParameter ConString = new ReportParameter();
            ConString.Name = "DatabaseConnectionString";
            ConString.Values.Add(ConnectionString);
            //Data Source=192.168.2.17;Initial Catalog=RUG;Integrated Security=false; User Id=sa; pwd=
            Params.Add(ConString);

            var uid = Guid.NewGuid();
            ApplicationDbContext context = new ApplicationDbContext();
            ReportUIDValues rid = new ReportUIDValues();
            rid.UID = uid;
            rid.Type = "Id";
            rid.Value = Params.Where(m => m.Name == "Id").FirstOrDefault().Values[0];

            rid.ObjectState = ObjectState.Added;
            context.ReportUIDValues.Add(rid);

            context.SaveChanges();

            ReportName = context.Database.SqlQuery<string>(ReportSQL.Replace("REPORTUID", uid.ToString())).FirstOrDefault();

            var Items = context.ReportUIDValues.Where(m => m.UID == uid).ToList();

            foreach (var item in Items)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                context.ReportUIDValues.Remove(item);
            }
            context.SaveChanges();

            context.Dispose();

            string mimtype;
            ReportGenerateService c = new ReportGenerateService();
            byte[] BAR;

            BAR = c.ReportGenerateCustom(out mimtype, ReportFileType, UserName, Params, ReportName);

            return BAR;
        }
    }

    public class PdfMerger
    {
        /// <summary>
        /// Merge pdf files.
        /// </summary>
        /// <param name="sourceFiles">PDF files being merged.</param>
        /// <returns></returns>
        public byte[] MergeFiles(List<byte[]> sourceFiles)
        {
            Document document = new Document();
            using (MemoryStream ms = new MemoryStream())
            {
                PdfCopy copy = new PdfCopy(document, ms);
                document.Open();
                int documentPageCounter = 0;

                // Iterate through all pdf documents
                for (int fileCounter = 0; fileCounter < sourceFiles.Count; fileCounter++)
                {
                    // Create pdf reader
                    PdfReader reader = new PdfReader(sourceFiles[fileCounter]);
                    int numberOfPages = reader.NumberOfPages;

                    // Iterate through all pages
                    for (int currentPageIndex = 1; currentPageIndex <= numberOfPages; currentPageIndex++)
                    {
                        documentPageCounter++;
                        PdfImportedPage importedPage = copy.GetImportedPage(reader, currentPageIndex);
                        PdfCopy.PageStamp pageStamp = copy.CreatePageStamp(importedPage);

                        pageStamp.AlterContents();

                        copy.AddPage(importedPage);
                    }

                    copy.FreeReader(reader);
                    reader.Close();
                }

                document.Close();
                return ms.GetBuffer();
            }
        }
    }
}

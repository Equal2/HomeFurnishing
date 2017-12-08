using Microsoft.Owin.Security;
using Data.Infrastructure;
using Data.Models;
using Reports.Presentation.Helper;
using Model.ViewModels;
using Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Model.Models;
using Reports.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Drawing;
using Reports.Common;
using Core.Common;
using Reports.Controllers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class Report_DocumentPrintController : ReportController
    {
 
        public Report_DocumentPrintController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;           
        }
        
        [HttpGet]
        public ActionResult DocumentPrint(String queryString, int DocumentId = 0, string ReportFileType = ReportFileTypeConstants.PDF  )
        {
            var SubReportDataList = new List<DataTable>();
            var SubReportNameList = new List<string>();
            DataTable Dt = new DataTable();
            DataTable SubRepData = new DataTable();
            String SubReportProc;

            String MainQuery = queryString + " " + DocumentId.ToString();
            String StrSubReportProcList;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {


                SqlDataAdapter sqlDataAapter = new SqlDataAdapter(MainQuery, sqlConnection);
                dsRep.EnforceConstraints = false;
                sqlDataAapter.Fill(Dt);
            }

            if (Dt.Columns.Contains("SubReportProcList"))
            {
                StrSubReportProcList = Dt.Rows[0]["SubReportProcList"].ToString();
            }
            else
            {

                ViewBag.Message = "SubReportProcList is not define.";
                return View("Close");


            }

            SubRepData = Dt.Copy ();

            SqlConnection Con = new SqlConnection(connectionString);

            while (SubRepData.Rows.Count > 0 && SubRepData.Columns.Contains("SubReportProcList"))
            {
                SubReportProc = SubRepData.Rows[0]["SubReportProcList"].ToString();

                if (SubReportProc != "")
                {
                    String query = "Web." + SubReportProc;                   
                    SqlDataAdapter sqlDataAapter1 = new SqlDataAdapter(query.ToString(), Con);
                    dsRep.EnforceConstraints = false;
                    SubRepData.Reset () ;

                    sqlDataAapter1.Fill(SubRepData);

                    DataTable SubDataTable = new DataTable();
                    SubDataTable = SubRepData.Copy();


                    

                    string SubRepName ="";
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
            byte[] BAR;
            //BAR = c.ReportGenerate(Dt, out mimtype, ReportFileTypeConstants.PDF);
            BAR = c.ReportGenerate(Dt, out mimtype, ReportFileType, null, SubReportDataList, null, SubReportNameList,  User.Identity.Name);
            return File(BAR, mimtype); 
        }
    }
}
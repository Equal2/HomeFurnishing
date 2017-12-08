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

namespace Jobs.Controllers
{
    [Authorize]
    public class Report_DocumentPrintController : ReportController
    {
 
        public Report_DocumentPrintController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;           
        }
        
        [HttpGet]
        public ActionResult DocumentPrint(String queryString, int DocumentId = 0, string ReportFileType = ReportFileTypeConstants.PDF)
        {
            var SubReportDataList = new List<DataTable>();
            DataTable Dt = new DataTable();
            String StrSubReportProcList;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlDataAdapter sqlDataAapter = new SqlDataAdapter(queryString.ToString(), sqlConnection);
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


            if (StrSubReportProcList !="")
            {
                 //Dim mPartyItem_UidArr As String() = Split(DtTemp.Rows(I)("PartyItem_Uid"), "|")
                string[] SubReportProcList = StrSubReportProcList.Split(new Char[] { ',' });
               

                if (SubReportProcList.Length  > 0)
                {
                    SqlConnection sqlConnection = new SqlConnection(connectionString);

                    foreach (var SubReportProc in SubReportProcList)
                    {

                        String query = "Web." + SubReportProc + " " + DocumentId.ToString();
                        DataTable SubReport1Data = new DataTable();


                        SqlDataAdapter sqlDataAapter1 = new SqlDataAdapter(query.ToString(), sqlConnection);
                        dsRep.EnforceConstraints = false;
                        sqlDataAapter1.Fill(SubReport1Data);


                        SubReportDataList.Add(SubReport1Data);

                        SubReport1Data = null;

                    }
                }
            }








            string mimtype;
            ReportGenerateService c = new ReportGenerateService();
            byte[] BAR;
            //BAR = c.ReportGenerate(Dt, out mimtype, ReportFileTypeConstants.PDF);
            BAR = c.ReportGenerate(Dt, out mimtype, ReportFileType, null, SubReportDataList);
            return File(BAR, mimtype); 
        }
    }
}
using Data.Infrastructure;
using Data.Models;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using Core.Common;
using Reports.Controllers;
using Jobs.Helpers;
using Model.ViewModel;
using System.Xml.Linq;

/* All Details Reports Will Be Created From Table 
 * Summary, Status, Balance  will be from View
 * Balance Report will be created directly from Views without Date Filter
 */

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class Posting_ProcedureExecuteController : ReportController
    {
        IReportLineService _ReportLineService;

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        public Posting_ProcedureExecuteController(IUnitOfWork unitOfWork, IReportLineService line)
        {
            _unitOfWork = unitOfWork;
            _ReportLineService = line;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        [HttpGet]
        public ActionResult ProcedureExcute(int MenuId)
        {
            //TempData["DefaultValues"] = TempData["DefaultValues"] as Dictionary<int, string>;
            Menu menu = new MenuService(_unitOfWork).Find(MenuId);
            return RedirectToAction("PostingLayout", "PostingLayout", new { name = menu.MenuName });
        }

        public ActionResult ProcedureExcute(FormCollection form, string ReportFileType)
        {

            DataTable ReportData = new DataTable();
            Dictionary<string, string> ReportFilters = new Dictionary<string, string>();
            StringBuilder queryString = new StringBuilder();

            string ReportHeaderId = (form["ReportHeaderId"].ToString());

            ReportHeader header = new ReportHeaderService(_unitOfWork).GetReportHeader(Convert.ToInt32(ReportHeaderId));
            List<ReportLine> lines = _ReportLineService.GetReportLineList(header.ReportHeaderId).ToList();



            ApplicationDbContext Db = new ApplicationDbContext();
            queryString.Append(db.strSchemaName + "." + header.SqlProc);


            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(queryString.ToString(), sqlConnection);

                foreach (var item in lines)
                {


                    if (item.SqlParameter != "" && item.SqlParameter != null)
                    {
                        if (item.SqlParameter == "@LoginSite" || item.SqlParameter == "@LoginDivision")
                        {
                            if (item.SqlParameter == "@LoginSite")
                                cmd.Parameters.AddWithValue(item.SqlParameter, (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginSiteId]);
                            else if (item.SqlParameter == "@LoginDivision")
                                cmd.Parameters.AddWithValue(item.SqlParameter, (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginDivisionId]);

                        }
                        else if (item.FieldName == "Site" && form[item.FieldName].ToString() == "")
                        {
                            cmd.Parameters.AddWithValue(item.SqlParameter, (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginSiteId]);

                        }

                        else if (item.FieldName == "Division" && form[item.FieldName].ToString() == "")
                        {

                            cmd.Parameters.AddWithValue(item.SqlParameter, (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginDivisionId]);
                        }

                        else
                        {
                            if (form[item.FieldName].ToString() != "")
                            {
                                if (item.DataType == "Date")
                                {
                                    cmd.Parameters.AddWithValue(item.SqlParameter, (form[item.FieldName].ToString() != "" ? String.Format("{0:MMMM dd yyyy}", form[item.FieldName].ToString()) : "Null"));

                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue(item.SqlParameter, (form[item.FieldName].ToString() != "" ? form[item.FieldName].ToString() : "Null"));

                                }
                            }
                        }

                    }
                }


                cmd.CommandTimeout = 200;
                SqlDataAdapter sqlDataAapter = new SqlDataAdapter(cmd);
                sqlDataAapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                dsRep.EnforceConstraints = false;
                sqlDataAapter.Fill(ReportData);


            }

            var Paralist = new List<string>();

            foreach (var item in lines)
            {
                if (item.SqlParameter == "@LoginSite" || item.SqlParameter == "@LoginDivision")
                {
                }
                else
                {
                    if (item.SqlParameter != "" && item.SqlParameter != null && form[item.FieldName].ToString() != "")
                    {
                        if (item.DataType == "Date")
                        {
                            if (!string.IsNullOrEmpty(form[item.FieldName].ToString())) { Paralist.Add(item.DisplayName + " : " + form[item.FieldName].ToString()); ReportFilters.Add(item.DisplayName, form[item.FieldName].ToString()); }
                        }
                        else if (item.DataType == "Single Select")
                        {
                            if (!string.IsNullOrEmpty(item.ListItem))
                            { Paralist.Add(item.DisplayName + " : " + form[item.FieldName].ToString()); }
                            else if (!string.IsNullOrEmpty(form[item.FieldName].ToString())) { Paralist.Add(item.DisplayName + " : " + form[item.FieldName + "Names"].ToString()); ReportFilters.Add(item.DisplayName, form[item.FieldName + "Names"].ToString()); }

                        }
                        else if (item.DataType == "Multi Select")
                        {
                            if (form[item.FieldName].ToString() != "") { Paralist.Add(item.DisplayName + " : " + form[item.FieldName + "Names"].ToString()); ReportFilters.Add(item.DisplayName, form[item.FieldName + "Names"].ToString()); }
                        }
                        else
                        {
                            if (form[item.FieldName].ToString() != "") { Paralist.Add(item.DisplayName + " : " + form[item.FieldName].ToString()); ReportFilters.Add(item.DisplayName, form[item.FieldName].ToString()); }
                        }
                    }
                }
            }

            XElement s = new XElement(CustomStringOp.CleanCode(header.ReportName));
            XElement Name = new XElement("Filters");
            foreach (var Rec in ReportFilters)
            {
                Name.Add(new XElement(CustomStringOp.CleanCode(Rec.Key), Rec.Value));
            }
            s.Add(Name);


            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDoctypeConstants.Report).DocumentTypeId,
                DocId = header.ReportHeaderId,
                Narration = header.ReportName,
                ActivityType = (int)ActivityTypeContants.Report,
                xEModifications = s,
            }));

            if (ReportData.Rows.Count > 0)
            {

                if (ReportData.Rows[0][0].ToString() == "Success")
                {
                    return View("Success");
                }
                else
                {
                    ViewBag.Error = ReportData.Rows[0][0].ToString();
                    return View("Error");
                }

            }

            else
            {
                ViewBag.Message = "No Record to Print.";
                return View("Close");
            }

        }



        [HttpPost]
        [MultipleButton(Name = "Print", Argument = "PDF")]
        public ActionResult PrintToPDF(FormCollection form)
        {
            return ProcedureExcute(form, ReportFileTypeConstants.PDF);

        }


    }
}
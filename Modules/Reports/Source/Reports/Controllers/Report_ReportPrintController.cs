using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Xml.Linq;
using Service;
using Models.Reports.Models;
using ProjLib.Constants;
using Components.Logging;
using Microsoft.Reporting.WebForms;
using Services.BasicSetup;
using ProjLib.DocumentConstants;
using Infrastructure.IO;
using Helper;
using DocumentPrint;
using Models.Reports.ViewModels;
using Newtonsoft.Json;

namespace Reports
{
    [Authorize]
    public class Report_ReportPrintController : Controller
    {
        private string connectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
        private ReportViewer reportViewer = new ReportViewer();

        IReportLineService _ReportLineService;
        IReportHeaderService _ReportHeaderService;
        ILogger _logger;
        IDocumentTypeService _documentTypeService;
        IReportUIDValuesService _reportUidValuesService;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();
        public Report_ReportPrintController(IReportLineService line, IReportHeaderService ReportHeader, ILogger logger, IDocumentTypeService DoctypeServ
            , IReportUIDValuesService reportUidValuesServ)
        {
            _ReportLineService = line;
            _ReportHeaderService = ReportHeader;
            _logger = logger;
            _documentTypeService = DoctypeServ;
            _reportUidValuesService = reportUidValuesServ;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        [HttpGet]
        public ActionResult ReportPrint(int MenuId)
        {
            return RedirectToAction("ReportLayout", "ReportLayout", new { name = _ReportHeaderService.GetMenuName(MenuId) });
        }

        public ActionResult ReportPrint(FormCollection form, string ReportFileType)
        {
            var SubReportDataList = new List<DataTable>();
            var SubReportNameList = new List<string>();
            DataTable ReportData = new DataTable();
            Dictionary<string, string> ReportFilters = new Dictionary<string, string>();
            StringBuilder queryString = new StringBuilder();

            string ReportHeaderId = (form["ReportHeaderId"].ToString());

            ReportHeader header = _ReportHeaderService.GetReportHeader(Convert.ToInt32(ReportHeaderId));
            List<ReportLineViewModel> lines = _ReportLineService.GetReportLineList(header.ReportHeaderId).ToList();


            if (string.IsNullOrEmpty(header.ReportSQL))
            {
                List<string> SubReportProcList = new List<string>();

                queryString.Append(header.SqlProc);

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
                                //cmd.Parameters.AddWithValue(item.SqlParameter, 17);
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
                    sqlDataAapter.Fill(ReportData);


                    if (ReportData.Rows.Count > 0)
                    {
                        if (ReportData.Columns.Contains("SubReportProcList"))
                        {
                            SubReportProcList.Add(ReportData.Rows[0]["SubReportProcList"].ToString());
                        }


                        DataTable SubRepData = new DataTable();
                        String SubReportProc;

                        if (SubReportProcList != null)
                        {
                            if (SubReportProcList.Count > 0)
                            {


                                SubRepData = ReportData.Copy();

                                SqlConnection Con = new SqlConnection(connectionString);

                                while (SubRepData.Rows.Count > 0 && SubRepData.Columns.Contains("SubReportProcList"))
                                {
                                    SubReportProc = SubRepData.Rows[0]["SubReportProcList"].ToString();

                                    if (SubReportProc != "")
                                    {

                                        String query = "Web." + SubReportProc;
                                        SqlDataAdapter sqlDataAapter1 = new SqlDataAdapter(query.ToString(), Con);
                                        sqlDataAapter1.SelectCommand.CommandTimeout = 200;
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
                            }
                        }
                    }
                }

                if (ReportData.Rows.Count > 0)
                {

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
                                //else if (item.DataType == "Constant Value")
                                //{

                                //    //if (!string.IsNullOrEmpty(form[item.FieldName].ToString())) { Paralist.Add(item.DisplayName + " : " + form[item.FieldName + "Names"].ToString()); }
                                //}

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

                    string mimtype;
                    ReportGenerateService c = new ReportGenerateService();
                    byte[] BAR;
                    //BAR = c.ReportGenerate(ReportData, out mimtype, ReportFileType, Paralist, SubReportDataList);
                    BAR = c.ReportGenerate(ReportData, out mimtype, ReportFileType, Paralist, SubReportDataList, null, SubReportNameList, User.Identity.Name);

                    XElement s = new XElement(CustomStringOp.CleanCode(header.ReportName));
                    XElement Name = new XElement("Filters");
                    foreach (var Rec in ReportFilters)
                    {
                        Name.Add(new XElement(CustomStringOp.CleanCode(Rec.Key), Rec.Value));
                    }
                    s.Add(Name);

                    _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = _documentTypeService.Find(TransactionDoctypeConstants.Report).DocumentTypeId,
                        DocId = header.ReportHeaderId,
                        ActivityType = (int)ActivityTypeContants.Report,
                        xEModifications = s,
                    }));

                    if (BAR.Length == 1)
                    {
                        ViewBag.Message = "Report Name is not define.";
                        return View("Close");
                    }
                    else if (BAR.Length == 2)
                    {
                        ViewBag.Message = "Report Title is not define.";
                        return View("Close");
                    }
                    else
                    {

                        //if (mimtype != "application/pdf")
                        if (mimtype == "application/vnd.ms-excel")
                            return File(BAR, mimtype, header.ReportName + ".xls");
                        else
                            return File(BAR, mimtype);
                    }

                }

                else
                {
                    ViewBag.Message = "No Record to Print.";
                    return View("Close");
                }

            }

            else
            {
                List<ReportParameter> Params = new List<ReportParameter>();
                string ReportName = "";

                foreach (var item in lines)
                {
                    if (item.SqlParameter != "" && item.SqlParameter != null)
                    {
                        if (item.SqlParameter == "@LoginSite" || item.SqlParameter == "@LoginDivision")
                        {
                            if (item.SqlParameter == "@LoginSite")
                            {
                                ReportParameter Param = new ReportParameter();
                                Param.Name = CustomStringOp.CleanCode(item.SqlParameter);
                                Param.Values.Add(Convert.ToString(System.Web.HttpContext.Current.Session[SessionNameConstants.LoginSiteId]));

                                Params.Add(Param);
                            }
                            else if (item.SqlParameter == "@LoginDivision")
                            {
                                ReportParameter Param = new ReportParameter();
                                Param.Name = CustomStringOp.CleanCode(item.SqlParameter);
                                Param.Values.Add(Convert.ToString(System.Web.HttpContext.Current.Session[SessionNameConstants.LoginDivisionId]));

                                Params.Add(Param);
                            }

                        }
                        else if (item.FieldName == "Site" && form[item.FieldName].ToString() == "")
                        {
                            ReportParameter Param = new ReportParameter();
                            Param.Name = CustomStringOp.CleanCode(item.SqlParameter);
                            Param.Values.Add(Convert.ToString(System.Web.HttpContext.Current.Session[SessionNameConstants.LoginSiteId]));

                            Params.Add(Param);
                        }

                        else if (item.FieldName == "Division" && form[item.FieldName].ToString() == "")
                        {
                            ReportParameter Param = new ReportParameter();
                            Param.Name = CustomStringOp.CleanCode(item.SqlParameter);
                            Param.Values.Add(Convert.ToString(System.Web.HttpContext.Current.Session[SessionNameConstants.LoginDivisionId]));

                            Params.Add(Param);
                        }

                        else
                        {
                            if (form[item.FieldName].ToString() != "")
                            {
                                if (item.DataType == "Date")
                                {
                                    ReportParameter Param = new ReportParameter();
                                    Param.Name = CustomStringOp.CleanCode(item.SqlParameter);
                                    Param.Values.Add((form[item.FieldName].ToString() != "" ? String.Format("{0:MMMM dd yyyy}", form[item.FieldName].ToString()) : "Null"));

                                    Params.Add(Param);
                                }
                                else
                                {
                                    ReportParameter Param = new ReportParameter();
                                    Param.Name = CustomStringOp.CleanCode(item.SqlParameter);
                                    Param.Values.Add((form[item.FieldName].ToString() != "" ? form[item.FieldName].ToString() : "Null"));

                                    Params.Add(Param);
                                }
                            }
                        }

                    }

                }



                int i = 0;
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
                                if (!string.IsNullOrEmpty(form[item.FieldName].ToString()))
                                {

                                    ReportParameter Param = new ReportParameter();
                                    Param.Name = "FilterStr" + ++i;
                                    Param.Values.Add(item.DisplayName + " : " + form[item.FieldName].ToString());
                                    Params.Add(Param);

                                    ReportFilters.Add(item.DisplayName, form[item.FieldName].ToString());
                                }
                            }
                            else if (item.DataType == "Single Select")
                            {
                                if (!string.IsNullOrEmpty(item.ListItem) && !string.IsNullOrEmpty(form[item.FieldName]))
                                {
                                    ReportParameter Param = new ReportParameter();
                                    Param.Name = "FilterStr" + ++i;
                                    Param.Values.Add(item.DisplayName + " : " + form[item.FieldName].ToString());
                                    Params.Add(Param);
                                }
                                else if (!string.IsNullOrEmpty(form[item.FieldName].ToString()) && !string.IsNullOrEmpty(form[item.FieldName + "Names"]))
                                {
                                    ReportParameter Param = new ReportParameter();
                                    Param.Name = "FilterStr" + ++i;
                                    Param.Values.Add(item.DisplayName + " : " + form[item.FieldName + "Names"].ToString());
                                    Params.Add(Param);

                                    ReportFilters.Add(item.DisplayName, form[item.FieldName + "Names"].ToString());
                                }

                            }

                            else if (item.DataType == "Multi Select")
                            {
                                if (form[item.FieldName].ToString() != "" && !string.IsNullOrEmpty(form[item.FieldName + "Names"]))
                                {
                                    ReportParameter Param = new ReportParameter();
                                    Param.Name = "FilterStr" + ++i;
                                    Param.Values.Add(item.DisplayName + " : " + form[item.FieldName + "Names"].ToString());
                                    Params.Add(Param);

                                    ReportFilters.Add(item.DisplayName, form[item.FieldName + "Names"].ToString());
                                }
                            }
                            else
                            {
                                if (form[item.FieldName].ToString() != "" && !string.IsNullOrEmpty(form[item.FieldName]))
                                {
                                    ReportParameter Param = new ReportParameter();
                                    Param.Name = "FilterStr" + ++i;
                                    Param.Values.Add(item.DisplayName + " : " + form[item.FieldName].ToString());
                                    Params.Add(Param);

                                    ReportFilters.Add(item.DisplayName, form[item.FieldName].ToString());
                                }
                            }
                        }
                    }
                }

                var uid = Guid.NewGuid();
              
                _reportUidValuesService.InsertRange(Params, uid);

                ReportName = _ReportHeaderService.GetReportNameFromProcedure(header.ReportSQL.Replace("REPORTUID", uid.ToString()));

                _reportUidValuesService.DeleteRange(uid);

                ReportParameter UserName = new ReportParameter();
                UserName.Name = "PrintedBy";
                UserName.Values.Add(User.Identity.Name);
                Params.Add(UserName);

                ReportParameter ConString = new ReportParameter();
                ConString.Name = "DatabaseConnectionString";
                ConString.Values.Add(connectionString);
                //Data Source=192.168.2.17;Initial Catalog=RUG;Integrated Security=false; User Id=sa; pwd=
                Params.Add(ConString);

                string mimtype;
                ReportGenerateService c = new ReportGenerateService();
                byte[] BAR;

                BAR = c.ReportGenerateCustom(out mimtype, ReportFileType, User.Identity.Name, Params, ReportName);

                XElement s = new XElement(CustomStringOp.CleanCode(header.ReportName));
                XElement Name = new XElement("Filters");
                foreach (var Rec in ReportFilters)
                {
                    Name.Add(new XElement(CustomStringOp.CleanCode(Rec.Key), Rec.Value));
                }
                s.Add(Name);


                _logger.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = _documentTypeService.Find(TransactionDoctypeConstants.Report).DocumentTypeId,
                    DocId = header.ReportHeaderId,
                    ActivityType = (int)ActivityTypeContants.Report,
                }));

                if (BAR.Length == 1)
                {
                    ViewBag.Message = "Report Name is not define.";
                    return View("Close");
                }
                else if (BAR.Length == 2)
                {
                    ViewBag.Message = "Report Title is not define.";
                    return View("Close");
                }
                else
                {
                    if (mimtype == "application/vnd.ms-excel")
                        return File(BAR, mimtype, header.ReportName + ".xls");
                    else
                        return File(BAR, mimtype);
                }
            }

        }


        //public ActionResult ReportGrid(FormCollection form)
        //{
        //    var SubReportDataList = new List<DataTable>();
        //    var SubReportNameList = new List<string>();
        //    DataTable ReportData = new DataTable();
        //    Dictionary<string, string> ReportFilters = new Dictionary<string, string>();
        //    StringBuilder queryString = new StringBuilder();

        //    string ReportHeaderId = (form["ReportHeaderId"].ToString());

        //    ReportHeader header = new ReportHeaderService(_unitOfWork).GetReportHeader(Convert.ToInt32(ReportHeaderId));
        //    List<ReportLine> lines = _ReportLineService.GetReportLineList(header.ReportHeaderId).ToList();

        //    Dictionary<string, string> ReportFilters2 = new Dictionary<string, string>();

        //    List<string> SubReportProcList = new List<string>();

        //    ApplicationDbContext Db = new ApplicationDbContext();
        //    queryString.Append(db.strSchemaName + "." + header.SqlProc);


        //    using (SqlConnection sqlConnection = new SqlConnection(connectionString))
        //    {
        //        SqlCommand cmd = new SqlCommand(queryString.ToString(), sqlConnection);

        //        foreach (var item in lines)
        //        {


        //            if (item.SqlParameter != "" && item.SqlParameter != null)
        //            {
        //                if (item.SqlParameter == "@LoginSite" || item.SqlParameter == "@LoginDivision")
        //                {
        //                    if (item.SqlParameter == "@LoginSite")
        //                        cmd.Parameters.AddWithValue(item.SqlParameter, (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginSiteId]);
        //                    //cmd.Parameters.AddWithValue(item.SqlParameter, 17);
        //                    else if (item.SqlParameter == "@LoginDivision")
        //                        cmd.Parameters.AddWithValue(item.SqlParameter, (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginDivisionId]);
        //                }
        //                else if (item.FieldName == "Site" && form[item.FieldName].ToString() == "")
        //                {
        //                    cmd.Parameters.AddWithValue(item.SqlParameter, (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginSiteId]);
        //                }

        //                else if (item.FieldName == "Division" && form[item.FieldName].ToString() == "")
        //                {
        //                    cmd.Parameters.AddWithValue(item.SqlParameter, (int)System.Web.HttpContext.Current.Session[SessionNameConstants.LoginDivisionId]);
        //                }

        //                else
        //                {
        //                    if (form[item.FieldName].ToString() != "")
        //                    {
        //                        if (item.DataType == "Date")
        //                        {
        //                            cmd.Parameters.AddWithValue(item.SqlParameter, (form[item.FieldName].ToString() != "" ? String.Format("{0:MMMM dd yyyy}", form[item.FieldName].ToString()) : "Null"));

        //                        }
        //                        else
        //                        {
        //                            cmd.Parameters.AddWithValue(item.SqlParameter, (form[item.FieldName].ToString() != "" ? form[item.FieldName].ToString() : "Null"));

        //                        }
        //                    }
        //                }

        //                if (cmd.Parameters.Contains(item.SqlParameter))
        //                    ReportFilters2.Add(item.SqlParameter, cmd.Parameters[item.SqlParameter].Value.ToString());

        //            }
        //        }


        //        cmd.CommandTimeout = 200;
        //        SqlDataAdapter sqlDataAapter = new SqlDataAdapter(cmd);
        //        sqlDataAapter.SelectCommand.CommandType = CommandType.StoredProcedure;
        //        dsRep.EnforceConstraints = false;
        //        DataSet ds = new DataSet();
        //        sqlDataAapter.Fill(ds);
        //        AddIncrement(ds);

        //        TempData["TestTemp"] = ReportFilters2;

        //        var tem = JsonConvert.SerializeObject(ds.Tables[0]);
        //        var ColNames = GetColumnNames(header.ReportHeaderId, cmd.Parameters["@ReportType"].Value.ToString());
        //        var ReportTypes = new SubReportService(_unitOfWork).GetSubReportList(header.ReportHeaderId);
        //        ViewBag.ReportTypes = ReportTypes.Select(m => new SubReportViewModel { SubReportName = m.SubReportName }).ToList();
        //        ViewBag.SelectedReportType = cmd.Parameters["@ReportType"].Value.ToString();
        //        ViewBag.ReportDate = tem;
        //        ViewBag.Id = ReportTypes.Where(m => m.ReportHeaderId == header.ReportHeaderId && m.SubReportName == cmd.Parameters["@ReportType"].Value.ToString()).FirstOrDefault().SubReportId;
        //        ViewBag.ColumnNames = JsonConvert.SerializeObject(ColNames);

        //        var Paralist = new List<Tuple<string, string, Dictionary<string, string>, bool>>();

        //        foreach (var item in lines)
        //        {
        //            if (item.SqlParameter == "@LoginSite" || item.SqlParameter == "@LoginDivision")
        //            {
        //            }
        //            else if (item.Type == "2.Filter")
        //            {
        //                if (item.SqlParameter != "" && item.SqlParameter != null && form[item.FieldName].ToString() != "")
        //                {
        //                    if (item.DataType == "Date")
        //                    {
        //                        if (!string.IsNullOrEmpty(form[item.FieldName].ToString())) { Paralist.Add(new Tuple<string, string, Dictionary<string, string>, bool>(item.DisplayName, item.SqlParameter, new Dictionary<string, string>() { { form[item.FieldName].ToString(), form[item.FieldName].ToString() }}, item.IsMandatory)); ReportFilters.Add(item.DisplayName, form[item.FieldName].ToString()); }
        //                    }
        //                    else if (item.DataType == "Single Select")
        //                    {
        //                        if (!string.IsNullOrEmpty(item.ListItem))
        //                        { Paralist.Add(new Tuple<string, string, Dictionary<string, string>, bool>(item.DisplayName, item.SqlParameter, new Dictionary<string, string>() { { form[item.FieldName].ToString(), form[item.FieldName].ToString() }}, item.IsMandatory)); }
        //                        else if (!string.IsNullOrEmpty(form[item.FieldName].ToString())) { Paralist.Add(new Tuple<string, string, Dictionary<string, string>, bool>(item.DisplayName, item.SqlParameter, new Dictionary<string, string>() { { form[item.FieldName].ToString(), form[item.FieldName + "Names"].ToString() }}, item.IsMandatory)); ReportFilters.Add(item.DisplayName, form[item.FieldName + "Names"].ToString()); }

        //                    }
        //                    else if (item.DataType == "Multi Select")
        //                    {
        //                        Dictionary<string, string> kevval = new Dictionary<string, string>();

        //                        var Count = form[item.FieldName].Split(',').Count();
        //                        for (int i = 0; i < Count; i++)
        //                        {
        //                            var temp = form[item.FieldName + "dic[" + i + "].Key"];
        //                            var tempVal = form[item.FieldName + "dic[" + i + "].Value"];

        //                            if (!string.IsNullOrEmpty(temp) && !string.IsNullOrEmpty(tempVal))
        //                                kevval.Add(temp, tempVal);
        //                        }

        //                        if (form[item.FieldName].ToString() != "") { Paralist.Add(new Tuple<string, string, Dictionary<string, string>, bool>(item.DisplayName, item.SqlParameter, kevval, item.IsMandatory)); ReportFilters.Add(item.DisplayName, form[item.FieldName + "Names"].ToString()); }
        //                    }
        //                    else
        //                    {
        //                        if (form[item.FieldName].ToString() != "") { Paralist.Add(new Tuple<string, string, Dictionary<string, string>, bool>(item.DisplayName, item.SqlParameter, new Dictionary<string, string>() { { form[item.FieldName].ToString(), form[item.FieldName].ToString() }}, item.IsMandatory)); ReportFilters.Add(item.DisplayName, form[item.FieldName].ToString()); }
        //                    }
        //                }
        //            }
        //        }

        //        ViewBag.ReportFilters = Paralist;
        //        ViewBag.ReportName = header.ReportName;

        //        return View("GridReport");
        //    }
        //}

        private static void AddIncrement(DataSet ds)
        {
            DataColumn dc = new DataColumn();
            dc.AutoIncrement = true;
            dc.AutoIncrementStep = 1;
            dc.ColumnName = "id";
            dc.DataType = typeof(int);
            ds.Tables[0].Columns.Add(dc);

            int i = 0;
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                dr.SetField(dc, i++);
            }

        }


        //public ActionResult FetchReport(int ID, string ReportType, Dictionary<string, string> nameValuePairs, int? ReportColumnId)
        //{

        //    int SubReportId = ID;

        //    SubReport sr = new SubReportService(_unitOfWork).Find(ID);

        //    ReportHeader header = new ReportHeaderService(_unitOfWork).GetReportHeader(sr.ReportHeaderId);
        //    List<ReportLine> lines = _ReportLineService.GetReportLineList(header.ReportHeaderId).ToList();

        //    using (SqlConnection sqlConnection = new SqlConnection(connectionString))
        //    {
        //        SqlCommand cmd = new SqlCommand();

        //        var Filters = (Dictionary<string, string>)TempData["TestTemp"];

        //        foreach (var item in lines.Where(m => m.Type == "2.Filter"))
        //        {
        //            string value = "";

        //            bool Success = Filters.TryGetValue(item.SqlParameter, out value);

        //            if (Success)
        //                cmd.Parameters.AddWithValue(item.SqlParameter, value);
        //        }
        //        if (!string.IsNullOrEmpty(ReportType))
        //            cmd.Parameters.AddWithValue("ReportType", ReportType);
        //        else
        //            cmd.Parameters.AddWithValue("ReportType", sr.SubReportName);

        //        DataTable dt = new DataTable();
        //        dt.Clear();
        //        dt.Columns.Add("Name");
        //        dt.Columns.Add("Value");

        //        if (ReportColumnId.HasValue && ReportColumnId.Value > 0)
        //        {
        //            var rc = new ReportColumnService(_unitOfWork).Find(ReportColumnId.Value);
        //            var rcList = new ReportColumnService(_unitOfWork).GetREportColumnListFromReportColumn(ReportColumnId.Value);

        //            var Records = (from p in nameValuePairs
        //                           join t in rcList on p.Key equals t.FieldName
        //                           select new
        //                           {
        //                               key = t.ReportColumnId,
        //                               val = p.Value,
        //                           }).ToDictionary(m => m.key, m => m.val);

        //            if (Records.Count > 0)
        //            {
        //                foreach (var item in Records)
        //                {
        //                    DataRow dr = dt.NewRow();
        //                    dr["Name"] = item.Key;
        //                    dr["Value"] = item.Value;
        //                    dt.Rows.Add(dr);
        //                }
        //            }

        //            DataRow SubReportTypeRow = dt.NewRow();
        //            SubReportTypeRow["Name"] = "SubReportId";
        //            SubReportTypeRow["Value"] = rc.SubReportId;
        //            dt.Rows.Add(SubReportTypeRow);

        //        }

        //        DataRow ReportHeaderIdRow = dt.NewRow();
        //        ReportHeaderIdRow["Name"] = "ReportColumnId";
        //        ReportHeaderIdRow["Value"] = ReportColumnId;
        //        dt.Rows.Add(ReportHeaderIdRow);

        //        DataRow SubReportHeaderId = dt.NewRow();
        //        SubReportHeaderId["Name"] = "NextSubReportId";
        //        SubReportHeaderId["Value"] = ID;
        //        dt.Rows.Add(SubReportHeaderId);


        //        cmd.Parameters.AddWithValue("ReportColumnDictionary", dt);

        //        cmd.CommandText = db.strSchemaName + "." + header.SqlProc;
        //        cmd.Connection = sqlConnection;
        //        cmd.CommandTimeout = 180;
        //        SqlDataAdapter sqlDataAapter = new SqlDataAdapter(cmd);
        //        sqlDataAapter.SelectCommand.CommandType = CommandType.StoredProcedure;
        //        dsRep.EnforceConstraints = false;
        //        DataSet ds = new DataSet();
        //        sqlDataAapter.Fill(ds);

        //        TempData.Keep("TestTemp");

        //        AddIncrement(ds);

        //        var tem = JsonConvert.SerializeObject(ds.Tables[0]);
        //        var ColNames = GetColumnNames(header.ReportHeaderId, string.IsNullOrEmpty(ReportType) ? sr.SubReportName : ReportType);

        //        var ReportTypes = new SubReportService(_unitOfWork).GetSubReportList(header.ReportHeaderId);

        //        return Json(new { success = true, data = tem, columns = ColNames, reportType = ReportTypes.Select(m => m.SubReportName).ToList(), selectedReportType = string.IsNullOrEmpty(ReportType) ? sr.SubReportName : ReportType, ReportName = header.ReportName });

        //        //return View("GridReport");

        //    }
        //}




        [HttpPost]
        [MultipleButton(Name = "Print", Argument = "PDF")]
        public ActionResult PrintToPDF(FormCollection form)
        {
            return ReportPrint(form, ReportFileTypeConstants.PDF);

        }

        [HttpPost]
        [MultipleButton(Name = "Print", Argument = "Excel")]
        public ActionResult PrintToExcel(FormCollection form)
        {
            return ReportPrint(form, ReportFileTypeConstants.Excel);
        }

        //[HttpPost]
        //[MultipleButton(Name = "Print", Argument = "Grid")]
        //public ActionResult PrintToGrid(FormCollection form)
        //{
        //    return ReportGrid(form);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ReportHeaderService.Dispose();
            }
            base.Dispose(disposing);
        }

        //IEnumerable<ReportColumnViewModel> GetColumnNames(int ReportHeaderId, string ReportType)
        //{

        //    return new ReportColumnService(_unitOfWork).GetReportColumnList(ReportHeaderId, ReportType);

        //}

        public JsonResult DeleteFilter(string Filter, string iFilter)
        {

            var Filters = (Dictionary<string, string>)TempData["TestTemp"];
            string FilterParam = Filter;
            if (string.IsNullOrEmpty(iFilter))
            {
                string val = "";
                if (Filters.TryGetValue(FilterParam, out val))
                {
                    Filters.Remove(FilterParam);
                }
            }
            else
            {
                string val = "";
                if (Filters.TryGetValue(FilterParam, out val))
                {
                    if (val.Contains(iFilter))
                    {
                        val = val.Replace(iFilter, "");

                        if (val.Split(',').Where(m => !string.IsNullOrEmpty(m) && !string.IsNullOrWhiteSpace(m)).Select(m => m).Count() > 0)
                            Filters[FilterParam] = val;
                        else
                            Filters.Remove(FilterParam);
                    }
                }
            }

            TempData["TestTemp"] = Filters;

            return Json(new { success = true });

        }
    }
}
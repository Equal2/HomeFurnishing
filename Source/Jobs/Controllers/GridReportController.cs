using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using System.Configuration;
using Model.ViewModel;
using System.Data.SqlClient;
using System.Data;
using System.Xml.Linq;
using Model.DatabaseViews;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using Jobs.Helpers;
using System.Text;
using Model.ViewModels;
using System.Web.Script.Serialization;

namespace Jobs.Controllers
{
    [Authorize]
    public class GridReportController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        protected string connectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
        //IGridReportService _GridReportService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        public GridReportController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _exception = exec;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult GridReportLayout(int? MenuId, string MenuName, int? DocTypeId)
        {
            Menu menu = new Menu();
            ReportHeader header = null;
            List<ReportLine> lines = new List<ReportLine>();
            if (MenuId != null)
            {
                menu = new MenuService(_unitOfWork).Find((int)MenuId);
                header = new ReportHeaderService(_unitOfWork).GetReportHeaderByName(menu.MenuName);
                if (header != null)
                    lines = new ReportLineService(_unitOfWork).GetReportLineList(header.ReportHeaderId).ToList();
                else
                {
                    new GridReportService().GetGridReportByName(menu.MenuName, ref header, ref lines, null);
                }
            }
            else{
                new GridReportService().GetGridReportByName(MenuName, ref header, ref lines, null);
            }


            if (DocTypeId != null)
            {
                ReportLine ReportLineDocType = (from L in lines where L.FieldName == "DocumentType" select L).FirstOrDefault();
                ReportLineDocType.DefaultValue = DocTypeId.ToString();
            }
            

            Dictionary<int, string> DefaultValues = TempData["ReportLayoutDefaultValues"] as Dictionary<int, string>;
            TempData["ReportLayoutDefaultValues"] = DefaultValues;
            foreach (var item in lines)
            {
                if (DefaultValues != null && DefaultValues.ContainsKey(item.ReportLineId))
                {
                    item.DefaultValue = DefaultValues[item.ReportLineId];
                }
            }

            ReportMasterViewModel vm = new ReportMasterViewModel();

            if (TempData["closeOnSelectOption"] != null)
                vm.closeOnSelect = (bool)TempData["closeOnSelectOption"];

            vm.ReportHeader = header;
            vm.ReportLine = lines;
            vm.ReportHeaderId = header.ReportHeaderId;


            if (DocTypeId != null)
            {
                DocumentType DocType = db.DocumentType.Find(DocTypeId);
                vm.ReportTitle = DocType.DocumentTypeName + " Report";
            }

            vm.ReportHeaderCompanyDetail = new GridReportService().GetReportHeaderCompanyDetail();

            return View(vm);
        }

        public JsonResult GridReportFill(FormCollection form)
        {
            bool IsDatabaseReport = false;
            var SubReportDataList = new List<DataTable>();
            var SubReportNameList = new List<string>();
            DataTable ReportData = new DataTable();
            var list = new List<dynamic>();
            Dictionary<string, string> ReportFilters = new Dictionary<string, string>();
            StringBuilder queryString = new StringBuilder();

            string ReportName = (form["ReportHeader.ReportName"].ToString());

            ReportHeader header = null;
            List<ReportLine> lines = new List<ReportLine>();

            if (Convert.ToInt32(form["ReportHeaderId"]) != 0)
                IsDatabaseReport = true;

            if (IsDatabaseReport)
            {
                header = new ReportHeaderService(_unitOfWork).GetReportHeaderByName(ReportName);
                lines = new ReportLineService(_unitOfWork).GetReportLineList(header.ReportHeaderId).ToList();
            }
            else
            {
                new GridReportService().GetGridReportByName(ReportName, ref header, ref lines, form);
            }


            List<string> SubReportProcList = new List<string>();
            
            ApplicationDbContext Db = new ApplicationDbContext();

            if (IsDatabaseReport)
                if (header.SqlProc.Contains("Web.") == false)
                    header.SqlProc = "Web." + header.SqlProc;

            queryString.Append(header.SqlProc);


            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(queryString.ToString(), sqlConnection);
                foreach (var item in lines)
                {
                    if (form[item.FieldName] != null)
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
                }
                cmd.CommandTimeout = 2000;
                SqlDataAdapter sqlDataAapter = new SqlDataAdapter(cmd);
                ReportHeader DatabaseReportHeader = new ReportHeaderService(_unitOfWork).GetReportHeaderByName(ReportName);
                if (IsDatabaseReport)
                    sqlDataAapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                else
                    sqlDataAapter.SelectCommand.CommandType = CommandType.Text;
                sqlDataAapter.Fill(ReportData);


            }


            ReportData.Columns.Add("SysParamType");


            DataRow DRowAggregate = ReportData.NewRow();
            DRowAggregate["SysParamType"] = "Aggregate";


            List<string> RemoveColumnList = new List<string>();

            foreach (DataColumn column in ReportData.Columns)
            {
                DataRow[] NullValueColumn = ReportData.Select("[" + column.ColumnName + "] is not null ");

                if (NullValueColumn.Length == 0)
                    RemoveColumnList.Add(column.ColumnName);


                if (column.DataType.ToString() == "System.Int32" || column.DataType.ToString() == "System.Decimal"
                        || column.DataType.ToString() == "System.Float" || column.DataType.ToString() == "System.Double")
                {
                    DataRow[] ZeroValueColumn = ReportData.Select("[" + column.ColumnName + "] <> 0 ");
                    if (!RemoveColumnList.Contains(column.ColumnName))
                    {
                        if (ZeroValueColumn.Length == 0)
                            RemoveColumnList.Add(column.ColumnName);
                    }
                }

                if (column.ColumnName == form["DealUnitColumnCaption"].ToString())
                {
                    DataRow[] DealQtyValueColumn = ReportData.Select("[" + form["DealUnitColumnCaption"].ToString() + "] <> " + "[" + form["UnitColumnCaption"].ToString() + "]");
                    if (DealQtyValueColumn.Length == 0)
                    {
                        if (!RemoveColumnList.Contains(form["DealUnitColumnCaption"].ToString()))
                            RemoveColumnList.Add(form["DealUnitColumnCaption"].ToString());
                        if (!RemoveColumnList.Contains(form["DealQtyColumnCaption"].ToString()))
                            RemoveColumnList.Add(form["DealQtyColumnCaption"].ToString());
                    }
                        
                }

                switch (column.DataType.ToString())
                {
                    case "System.Decimal":
                        {
                            DRowAggregate[column.ColumnName] = 2;
                            break;
                        }
                }
            }

            foreach (string ColumnName in RemoveColumnList)
            {
                if (ColumnName != "SysParamType")
                    ReportData.Columns.Remove(ColumnName);
            }
                    
            ReportData.Rows.Add(DRowAggregate);


            if (ReportData.Rows.Count > 0)
            {
                //List<DataRow> Row = ReportData.Rows.Cast<DataRow>().ToList();

                //var temp = ReportData.Select().ToList();

                //var list = new List<dynamic>();




                foreach (DataRow row in ReportData.Rows)
                {
                    dynamic dyn = new ExpandoObject();
                    list.Add(dyn);
                    foreach (DataColumn column in ReportData.Columns)
                    {
                        var dic = (IDictionary<string, object>)dyn;
                        dic[column.ColumnName] = row[column];
                    }
                }


                JsonResult json = Json(new { Success = true, Data = list.ToList() }, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }

            else
            {
                ViewBag.Message = "No Record to Print.";
                return Json(new { Success = true, Data = list.ToList() }, JsonRequestBehavior.AllowGet);
            }
    }




        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

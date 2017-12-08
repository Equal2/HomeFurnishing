using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModel;
using Jobs.Helpers;
using AutoMapper;
using System.Xml.Linq;
using System.Data.SqlClient;
using Model.ViewModels;
using System.Data;
using System.Configuration;


namespace Jobs.Controllers
{
    [Authorize]
    public class MaterialPlanningWizardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();


        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        public MaterialPlanningWizardController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(id).ToList();           
            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("Index", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        public ActionResult Index(int Id)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(Id, DivisionId, SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "MaterialPlanSettings", new { id = Id });
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            ViewBag.id = Id;
            return View();
        }

        public JsonResult PendingSaleOrderPlan(int Id)
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(Id, DivisionId, SiteId);
            string ProcName = settings.PendingProdOrderList;

            if (string.IsNullOrEmpty(ProcName))
                throw new Exception("Pending ProdOrders Not Configured");

            SqlParameter SqlParameterDocType = new SqlParameter("@PlanningDocumentType", Id);
            SqlParameter SqlParameterSite = new SqlParameter("@Site", SiteId);
            SqlParameter SqlParameterDivision = new SqlParameter("@Division", DivisionId);
            SqlParameter SqlParameterBuyer = new SqlParameter("@BuyerId", (object)DBNull.Value);

            IEnumerable<PendingSaleOrderFromProc> PendingSaleOrders = db.Database.SqlQuery<PendingSaleOrderFromProc>(" " + ProcName + " @PlanningDocumentType, @Site, @Division, @BuyerId", SqlParameterDocType, SqlParameterSite, SqlParameterDivision, SqlParameterBuyer).ToList();



            var resu = PendingSaleOrders.Select((m, i) => new
            {
                BalanceQtyForPlan = m.BalanceQty,
                Qty = m.BalanceQty,
                SaleOrderDocNo = m.SaleOrderNo,
                SaleOrderDocDate = m.OrderDate.ToString("dd/MMM/yyyy"),
                Specification = m.Specification,
                ProductId = m.ProductId,
                ProductName = m.ProductName,
                BuyerId = m.BuyerId,
                BuyerName = m.BuyerName,
                Dimension1Id = m.Dimension1Id,
                Dimension1Name = m.Dimension1Name,
                Dimension2Id = m.Dimension2Id,
                Dimension2Name = m.Dimension2Name,
                SaleOrderLineId = m.SaleOrderLineId,
                UnitName = m.UnitName,
                Id = i,
            });

            return Json(new { data = resu }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSummary(IEnumerable<MaterialPlanForSaleOrderViewModel> selectedRec, int DocTypeId)
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            //MaterialPlanSettings Setting = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(DocTypeId, DivisionId, SiteId);
            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(DocTypeId, DivisionId, SiteId);

            var ProductIds = selectedRec.Select(m => m.ProductId).ToArray();
            List<MaterialPlanLineViewModel> Line = new List<MaterialPlanLineViewModel>();

            if (selectedRec != null)
            {
                System.Web.HttpContext.Current.Session["BuyerId"] = selectedRec.FirstOrDefault().BuyerId;
            }

            System.Web.HttpContext.Current.Session["SODyeingPlan"] = selectedRec;

            if (settings.SqlProcConsumptionSummary != null)
            {
                var prodorderlinelist = selectedRec.Where(m => m.Qty > 0).Select(m => new { m.SaleOrderLineId, m.Qty });

                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("SaleOrderLineId");
                dataTable.Columns.Add("ProductId");
                dataTable.Columns.Add("Qty");


                foreach (var item in prodorderlinelist)
                {

                    var dr = dataTable.NewRow();
                    dr["SaleOrderLineId"] = item.SaleOrderLineId;
                    dr["ProductId"] = null;
                    dr["Qty"] = item.Qty;
                    dataTable.Rows.Add(dr);

                }
                DataSet ds = new DataSet();
                using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(settings.SqlProcConsumptionSummary))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;
                        cmd.Parameters.AddWithValue("@T", dataTable);
                        cmd.Parameters.AddWithValue("@MaterialPlanHeaderId", 0);
                        using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                        {
                            adp.Fill(ds);
                        }
                    }
                }

                DataTable dt2 = ds.Tables[0];

                foreach (DataRow dr in dt2.Rows)
                {

                    MaterialPlanLineViewModel line = new MaterialPlanLineViewModel();

                    line.ProductId = dr["ProductId"] == System.DBNull.Value ? 0 : Convert.ToInt32(dr["ProductId"].ToString());
                    line.ProcessId = dr["ProcessId"] == System.DBNull.Value ? null : (int?)Convert.ToInt32(dr["ProcessId"].ToString());
                    line.Dimension1Id = dr["Dimension1Id"] == System.DBNull.Value ? null : (int?)Convert.ToInt32(dr["Dimension1Id"]);
                    line.Dimension2Id = dr["Dimension2Id"] == System.DBNull.Value ? null : (int?)Convert.ToInt32(dr["Dimension2Id"]);


                    line.ProductName = dr["ProductName"] == System.DBNull.Value ? null : dr["ProductName"].ToString();
                    line.Specification = dr["Specification"] == System.DBNull.Value ? null : dr["Specification"].ToString();
                    line.UnitName = dr["UnitName"] == System.DBNull.Value ? null : dr["UnitName"].ToString();
                    line.unitDecimalPlaces = dr["DecimalPlaces"] == System.DBNull.Value ? 0 : Convert.ToInt32(dr["DecimalPlaces"].ToString());
                    line.RequiredQty = dr["Qty"] == System.DBNull.Value ? 0 : Convert.ToDecimal(dr["Qty"].ToString());
                    line.Dimension1Name = dr["Dimension1Name"] == System.DBNull.Value ? null : dr["Dimension1Name"].ToString();
                    line.Dimension2Name = dr["Dimension2Name"] == System.DBNull.Value ? null : dr["Dimension2Name"].ToString();
                    line.ProcessName = dr["ProcessName"] == System.DBNull.Value ? null : dr["ProcessName"].ToString();


                    line.ExcessStockQty = 0;
                    line.MaterialPlanHeaderId = 0;


                    line.ProdPlanQty = (dr["PurchProd"].ToString() == "Purchase") ? 0 : Convert.ToDecimal(dr["Qty"].ToString());
                    line.PurchPlanQty = (dr["PurchProd"].ToString() == "Purchase") ? Convert.ToDecimal(dr["Qty"].ToString()) : 0;

                    line.GeneratedFor = MaterialPlanConstants.SaleOrder;

                    Line.Add(line);
                }
            }
            else
            {
                //var summary = (from p in db.Product.Where(p => ProductIds.Contains(p.ProductId)).AsEnumerable()
                //               join t in selectedRec on p.ProductId equals t.ProductId
                //               where t.Qty > 0
                //               group t by new { t.ProductId, p.ProductName, t.Specification } into g
                //               join p1 in db.Product.Where(p => ProductIds.Contains(p.ProductId)).AsEnumerable() on g.Key.ProductId equals p1.ProductId
                //               join u1 in db.Units on p1.UnitId equals u1.UnitId
                //               select new
                //               {
                //                   id = g.Key.ProductId,
                //                   QtySum = g.Sum(m => m.Qty),
                //                   //GroupedItems = g,
                //                   name = g.Key.ProductName,
                //                   unitname = u1.UnitName,
                //                   Specification = g.Key.Specification,
                //                   Fractionunits = u1.DecimalPlaces
                //               }).ToList();


                var summary = (from t in selectedRec 
                               join p in db.Product on t.ProductId equals p.ProductId into ProductTable from ProductTab in ProductTable.DefaultIfEmpty()
                               join d1 in db.Dimension1 on t.Dimension1Id equals d1.Dimension1Id into Dimension1Table from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                               join d2 in db.Dimension2 on t.Dimension2Id equals d2.Dimension2Id into Dimension2Table
                               from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                               where t.Qty > 0
                               group t by new { t.ProductId, t.Dimension1Id, t.Dimension2Id, t.Specification } into g
                               join p1 in db.Product.Where(p => ProductIds.Contains(p.ProductId)).AsEnumerable() on g.Key.ProductId equals p1.ProductId
                               join u1 in db.Units on p1.UnitId equals u1.UnitId
                               select new
                               {
                                   id = g.Key.ProductId,
                                   Dimension1Id = g.Key.Dimension1Id,
                                   Dimension2Id = g.Key.Dimension2Id,
                                   QtySum = g.Sum(m => m.Qty),
                                   //GroupedItems = g,
                                   name = g.Max(m => m.ProductName),
                                   unitname = u1.UnitName,
                                   Dimension1Name = g.Max(m => m.Dimension1Name),
                                   Dimension2Name = g.Max(m => m.Dimension2Name),
                                   Specification = g.Key.Specification,
                                   Fractionunits = u1.DecimalPlaces
                               }).ToList();

                int j = 0;
                foreach (var item in summary)
                {
                    MaterialPlanLineViewModel planline = new MaterialPlanLineViewModel();
                    planline.ProductName = item.name;
                    planline.Dimension1Name = item.Dimension1Name;
                    planline.Dimension2Name = item.Dimension2Name;
                    planline.Dimension1Id = item.Dimension1Id;
                    planline.Dimension2Id = item.Dimension2Id;
                    planline.RequiredQty = item.QtySum;
                    planline.ExcessStockQty = 0;
                    planline.Specification = item.Specification;
                    planline.MaterialPlanHeaderId = 0;
                    planline.ProductId = item.id;
                    planline.ProdPlanQty = item.QtySum;
                    planline.UnitName = item.unitname;
                    planline.unitDecimalPlaces = item.Fractionunits;
                    planline.GeneratedFor = MaterialPlanConstants.SaleOrder;
                    Line.Add(planline);

                    j++;
                }
            }



            MaterialPlanSummaryViewModel Summary = new MaterialPlanSummaryViewModel();
            //Summary.MaterialPlanSettings = Setting;
            Summary.MaterialPlanSettings = Mapper.Map<MaterialPlanSettings, MaterialPlanSettingsViewModel>(settings);

            var data = Line.OrderBy(m => m.ProductName).ThenBy(m => m.Dimension1Name).ThenBy(m => m.Dimension2Name)
                .Select((m, i) => new
                {
                    Dimension1Id = m.Dimension1Id,
                    Dimension1Name = m.Dimension1Name,
                    Dimension2Id = m.Dimension2Id,
                    Dimension2Name = m.Dimension2Name,
                    DueDate = m.DueDate,
                    MaterialPlanHeaderDocNo = m.MaterialPlanHeaderDocNo,
                    MaterialPlanHeaderId = m.MaterialPlanHeaderId,
                    MaterialPlanLineId = m.MaterialPlanLineId,
                    ProcessId = m.ProcessId,
                    ProcessName = m.ProcessName,
                    ProdPlanQty = m.ProdPlanQty,
                    ProductId = m.ProductId,
                    ProductName = m.ProductName,
                    PurchPlanQty = m.PurchPlanQty,
                    Remark = m.Remark,
                    RequiredQty = m.RequiredQty,
                    Specification = m.Specification,
                    StockPlanQty = m.StockPlanQty,
                    unitDecimalPlaces = m.unitDecimalPlaces,
                    UnitId = m.UnitId,
                    UnitName = m.UnitName,
                    Id = i
                })
                .ToList();

            return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult SelectedOrderLines(IEnumerable<MaterialPlanLineViewModel> selectedRec)
        {
            System.Web.HttpContext.Current.Session["SODyeingPlanSummary"] = selectedRec;

            return Json(new { success = true });
        }

        public ActionResult Create(int id)//DocumentTypeID
        {
            MaterialPlanHeaderViewModel vm = new MaterialPlanHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            
            vm.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            vm.MaterialPlanSettings = Mapper.Map<MaterialPlanSettings, MaterialPlanSettingsViewModel>(settings);
            vm.BuyerId = (int)System.Web.HttpContext.Current.Session["BuyerId"];


            List<MaterialPlanForSaleOrderViewModel> svm = new List<MaterialPlanForSaleOrderViewModel>();
            svm = (List<MaterialPlanForSaleOrderViewModel>)System.Web.HttpContext.Current.Session["SODyeingPlan"];

            vm.DocDate = DateTime.Now;
            vm.DueDate = DateTime.Now;
            vm.DocTypeId = id;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".MaterialPlanHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }


        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(MaterialPlanHeaderViewModel vm)
        {
            MaterialPlanHeader pt = AutoMapper.Mapper.Map<MaterialPlanHeaderViewModel, MaterialPlanHeader>(vm);

            //#region DocTypeTimeLineValidation

            //try
            //{

            //    if (vm.MaterialPlanHeaderId <= 0)
            //        TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(vm), DocumentTimePlanTypeConstants.Create, User.Identity.Name, out ExceptionMsg, out Continue);
            //    else
            //        TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(vm), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXC"] += message;
            //    TimePlanValidation = false;
            //}

            //if (!TimePlanValidation)
            //    TempData["CSEXC"] += ExceptionMsg;

            //#endregion

            if (ModelState.IsValid && (TimePlanValidation || Continue))
            {

                #region CreateRecord
                if (vm.MaterialPlanHeaderId <= 0)
                {

                    using (var context = new ApplicationDbContext())
                    {

                        pt.CreatedDate = DateTime.Now;
                        pt.ModifiedDate = DateTime.Now;
                        pt.CreatedBy = User.Identity.Name;
                        pt.ModifiedBy = User.Identity.Name;
                        pt.ObjectState = Model.ObjectState.Added;
                        context.MaterialPlanHeader.Add(pt);

                        int Serial = 0;

                        MaterialPlanLineListViewModel svm = new MaterialPlanLineListViewModel();
                        svm.MaterialPlanLineViewModel = (List<MaterialPlanForSaleOrderViewModel>)System.Web.HttpContext.Current.Session["SODyeingPlan"];

                        var Summary = (IEnumerable<MaterialPlanLineViewModel>)System.Web.HttpContext.Current.Session["SODyeingPlanSummary"];

                        MaterialPlanSettings Settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);


                        bool isPr = false;
                        bool isPP = false;
                        int j = 0;
                        foreach (var item in Summary)
                        {
                            {
                                MaterialPlanLine planLine = new MaterialPlanLine();
                                planLine.RequiredQty = item.RequiredQty;
                                planLine.ExcessStockQty = item.ExcessStockQty;
                                planLine.MaterialPlanHeaderId = item.MaterialPlanHeaderId;
                                planLine.ProductId = item.ProductId;
                                planLine.Dimension1Id = item.Dimension1Id;
                                planLine.Dimension2Id = item.Dimension2Id;
                                planLine.ProdPlanQty = item.ProdPlanQty;
                                planLine.CreatedBy = User.Identity.Name;
                                planLine.CreatedDate = DateTime.Now;
                                planLine.Sr = Serial++;
                                planLine.Specification = item.Specification;
                                planLine.ModifiedBy = User.Identity.Name;
                                planLine.MaterialPlanLineId = j;
                                planLine.ModifiedDate = DateTime.Now;
                                planLine.ProcessId = item.ProcessId;
                                planLine.Remark = item.Remark;
                                planLine.PurchPlanQty = item.PurchPlanQty;
                                planLine.StockPlanQty = item.StockPlanQty;
                                planLine.GeneratedFor = MaterialPlanConstants.SaleOrder;
                                planLine.ObjectState = Model.ObjectState.Added;
                                context.MaterialPlanLine.Add(planLine);
                                if (!isPr)
                                {
                                    if (item.ProdPlanQty > 0)
                                        isPr = true;
                                }
                                if (!isPP)
                                {
                                    if (item.PurchPlanQty > 0)
                                        isPP = true;
                                }
                            }
                            j++;
                        }

                        if (isPr)
                        {




                            ProdOrderHeader ExistingProdOrder = new ProdOrderHeaderService(_unitOfWork).GetProdOrderForMaterialPlan(pt.MaterialPlanHeaderId);
                            int ProdORderSerial = 1;
                            if (ExistingProdOrder == null)
                            {
                                ProdOrderHeader prodOrderHeader = new ProdOrderHeader();

                                prodOrderHeader.CreatedBy = User.Identity.Name;
                                prodOrderHeader.CreatedDate = DateTime.Now;
                                prodOrderHeader.DivisionId = pt.DivisionId;
                                prodOrderHeader.DocDate = pt.DocDate;
                                prodOrderHeader.DocNo = pt.DocNo;
                                prodOrderHeader.DocTypeId = Settings.DocTypeProductionOrderId.Value;
                                prodOrderHeader.DueDate = pt.DueDate;
                                prodOrderHeader.MaterialPlanHeaderId = pt.MaterialPlanHeaderId;
                                prodOrderHeader.ModifiedBy = User.Identity.Name;
                                prodOrderHeader.ModifiedDate = DateTime.Now;
                                prodOrderHeader.Remark = pt.Remark;
                                prodOrderHeader.BuyerId = pt.BuyerId;
                                prodOrderHeader.SiteId = pt.SiteId;
                                //prodOrderHeader.Status = header.Status;
                                prodOrderHeader.Status = (int)StatusConstants.System;
                                prodOrderHeader.ObjectState = Model.ObjectState.Added;
                                context.ProdOrderHeader.Add(prodOrderHeader);

                                //ForCreating ProdOrderStatus
                                ProdOrderHeaderStatus pts = new ProdOrderHeaderStatus();
                                pts.ProdOrderHeaderId = prodOrderHeader.ProdOrderHeaderId;
                                pts.ObjectState = Model.ObjectState.Added;
                                context.ProdOrderHeaderStatus.Add(pts);

                                int ProdOrderLineKey = 0;
                                foreach (var item in context.MaterialPlanLine.Local.Where(m => m.ProdPlanQty > 0))
                                {
                                    ProdOrderLine prodOrderLine = new ProdOrderLine();
                                    prodOrderLine.CreatedBy = User.Identity.Name;
                                    prodOrderLine.CreatedDate = DateTime.Now;
                                    prodOrderLine.MaterialPlanLineId = item.MaterialPlanLineId;
                                    prodOrderLine.ModifiedBy = User.Identity.Name;
                                    prodOrderLine.ModifiedDate = DateTime.Now;
                                    prodOrderLine.ProdOrderHeaderId = prodOrderHeader.ProdOrderHeaderId;
                                    prodOrderLine.Specification = item.Specification;
                                    prodOrderLine.ProductId = item.ProductId;
                                    prodOrderLine.Dimension1Id = item.Dimension1Id;
                                    prodOrderLine.Dimension2Id = item.Dimension2Id;
                                    prodOrderLine.Sr = ProdORderSerial++;
                                    prodOrderLine.Qty = item.ProdPlanQty;
                                    prodOrderLine.Remark = item.Remark;
                                    prodOrderLine.ProdOrderLineId = ProdOrderLineKey++;
                                    prodOrderLine.ObjectState = Model.ObjectState.Added;
                                    context.ProdOrderLine.Add(prodOrderLine);

                                    //ForAdding ProdrderLinestatus
                                    ProdOrderLineStatus ptl = new ProdOrderLineStatus();
                                    ptl.ProdOrderLineId = prodOrderLine.ProdOrderLineId;
                                    ptl.ObjectState = Model.ObjectState.Added;
                                    context.ProdOrderLineStatus.Add(ptl);

                                }

                            }
                            else
                            {
                                ProdORderSerial = new ProdOrderLineService(_unitOfWork).GetMaxSr(ExistingProdOrder.ProdOrderHeaderId);


                                int ProdOrderLineKey = 0;
                                foreach (var item in context.MaterialPlanLine.Local.Where(m => m.ProdPlanQty > 0))
                                {
                                    ProdOrderLine prodOrderLine = new ProdOrderLine();
                                    prodOrderLine.CreatedBy = User.Identity.Name;
                                    prodOrderLine.CreatedDate = DateTime.Now;
                                    prodOrderLine.MaterialPlanLineId = item.MaterialPlanLineId;
                                    prodOrderLine.ModifiedBy = User.Identity.Name;
                                    prodOrderLine.ModifiedDate = DateTime.Now;
                                    prodOrderLine.ProdOrderHeaderId = ExistingProdOrder.ProdOrderHeaderId;
                                    prodOrderLine.ProductId = item.ProductId;
                                    prodOrderLine.Dimension1Id = item.Dimension1Id;
                                    prodOrderLine.Dimension2Id = item.Dimension2Id;
                                    prodOrderLine.Specification = item.Specification;
                                    prodOrderLine.Qty = item.ProdPlanQty;
                                    prodOrderLine.Sr = ProdORderSerial++;
                                    prodOrderLine.Remark = item.Remark;
                                    prodOrderLine.ProdOrderLineId = ProdOrderLineKey++;
                                    prodOrderLine.ObjectState = Model.ObjectState.Added;
                                    context.ProdOrderLine.Add(prodOrderLine);

                                    //ForAdding ProdrderLinestatus
                                    ProdOrderLineStatus ptl = new ProdOrderLineStatus();
                                    ptl.ProdOrderLineId = prodOrderLine.ProdOrderLineId;
                                    ptl.ObjectState = Model.ObjectState.Added;
                                    context.ProdOrderLineStatus.Add(ptl);

                                }
                            }



                        }
                        if (isPP)
                        {


                            PurchaseIndentHeader ExistingIndent = new PurchaseIndentHeaderService(_unitOfWork).GetPurchaseIndentForMaterialPlan(pt.MaterialPlanHeaderId);
                            int PurchaseIndentSr = 1;
                            if (ExistingIndent == null)
                            {

                                PurchaseIndentHeader indentHeader = new PurchaseIndentHeader();
                                indentHeader.CreatedBy = User.Identity.Name;
                                indentHeader.CreatedDate = DateTime.Now;
                                indentHeader.DivisionId = pt.DivisionId;
                                indentHeader.DocDate = pt.DocDate;
                                indentHeader.DocNo = pt.DocNo;
                                indentHeader.DocTypeId = Settings.DocTypePurchaseIndentId.Value;
                                indentHeader.ModifiedBy = User.Identity.Name;
                                indentHeader.MaterialPlanHeaderId = pt.MaterialPlanHeaderId;
                                indentHeader.ModifiedDate = DateTime.Now;
                                indentHeader.Remark = pt.Remark;
                                indentHeader.SiteId = pt.SiteId;
                                //indentHeader.Status = header.Status;
                                indentHeader.Status = (int)StatusConstants.System;
                                indentHeader.DepartmentId = (int)DepartmentConstants.Production;
                                indentHeader.ObjectState = Model.ObjectState.Added;
                                context.PurchaseIndentHeader.Add(indentHeader);
                                foreach (var item in context.MaterialPlanLine.Local.Where(m => m.PurchPlanQty > 0))
                                {
                                    PurchaseIndentLine indentLine = new PurchaseIndentLine();
                                    indentLine.CreatedBy = User.Identity.Name;
                                    indentLine.CreatedDate = DateTime.Now;
                                    indentLine.MaterialPlanLineId = item.MaterialPlanLineId;
                                    indentLine.ModifiedBy = User.Identity.Name;
                                    indentLine.ModifiedDate = DateTime.Now;
                                    indentLine.ProductId = item.ProductId;
                                    indentLine.Dimension1Id = item.Dimension1Id;
                                    indentLine.Dimension2Id = item.Dimension2Id;
                                    indentLine.Specification = item.Specification;
                                    indentLine.PurchaseIndentHeaderId = indentHeader.PurchaseIndentHeaderId;
                                    indentLine.Qty = item.PurchPlanQty;
                                    indentLine.Sr = PurchaseIndentSr++;
                                    indentLine.Remark = item.Remark;
                                    indentLine.ObjectState = Model.ObjectState.Added;
                                    context.PurchaseIndentLine.Add(indentLine);
                                }

                            }
                            else
                            {
                                PurchaseIndentSr = new PurchaseIndentLineService(_unitOfWork).GetMaxSr(ExistingIndent.PurchaseIndentHeaderId);
                                foreach (var item in context.MaterialPlanLine.Local.Where(m => m.PurchPlanQty > 0))
                                {
                                    PurchaseIndentLine indentLine = new PurchaseIndentLine();
                                    indentLine.CreatedBy = User.Identity.Name;
                                    indentLine.CreatedDate = DateTime.Now;
                                    indentLine.MaterialPlanLineId = item.MaterialPlanLineId;
                                    indentLine.ModifiedBy = User.Identity.Name;
                                    indentLine.Specification = item.Specification;
                                    indentLine.ModifiedDate = DateTime.Now;
                                    indentLine.ProductId = item.ProductId;
                                    indentLine.Dimension1Id = item.Dimension1Id;
                                    indentLine.Dimension2Id = item.Dimension2Id;
                                    indentLine.Sr = PurchaseIndentSr++;
                                    indentLine.PurchaseIndentHeaderId = ExistingIndent.PurchaseIndentHeaderId;
                                    indentLine.Qty = item.PurchPlanQty;
                                    indentLine.Remark = item.Remark;
                                    indentLine.ObjectState = Model.ObjectState.Added;
                                    context.PurchaseIndentLine.Add(indentLine);
                                }
                            }




                        }


                        int i = 0;
                        int MaterialPlanForSaleOrderSr = new MaterialPlanForSaleOrderService(_unitOfWork).GetMaxSr(svm.MaterialPlanLineViewModel.FirstOrDefault().MaterialPlanHeaderId);
                        foreach (var item in svm.MaterialPlanLineViewModel)
                        {

                            if (item.Qty > 0)
                            {
                                MaterialPlanForSaleOrder order = new MaterialPlanForSaleOrder();
                                order.MaterialPlanHeaderId = item.MaterialPlanHeaderId;
                                order.Qty = item.Qty;
                                order.SaleOrderLineId = item.SaleOrderLineId;
                                order.MaterialPlanForSaleOrderId = i;
                                order.Sr = MaterialPlanForSaleOrderSr++;
                                order.CreatedBy = User.Identity.Name;
                                order.CreatedDate = DateTime.Now;
                                order.ModifiedBy = User.Identity.Name;
                                order.ModifiedDate = DateTime.Now;

                                //if (item.Dimension1Id != null)
                                //{
                                //    var MaterialPlan = context.MaterialPlanLine.Local.Where(m => m.ProductId == item.ProductId && m.Dimension1Id == item.Dimension1Id).FirstOrDefault();
                                //    if (order != null)
                                //    {
                                //        order.MaterialPlanLineId = MaterialPlan.MaterialPlanLineId;
                                //    }
                                //}
                                //else
                                //{
                                //    var MaterialPlan = context.MaterialPlanLine.Local.Where(m => m.ProductId == item.ProductId).FirstOrDefault();
                                //    if (order != null)
                                //    {
                                //        order.MaterialPlanLineId = MaterialPlan.MaterialPlanLineId;
                                //    }
                                //}

                                order.ObjectState = Model.ObjectState.Added;
                                context.MaterialPlanForSaleOrder.Add(order);

                                i++;

                            }
                        }

                        try
                        {
                            context.SaveChanges();
                        }

                        catch (Exception ex)
                        {
                            string message = _exception.HandleException(ex);
                            TempData["CSEXC"] += message;
                            ViewBag.Mode = "Add";
                            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(vm.DocTypeId).DocumentTypeName;
                            ViewBag.id = vm.DocTypeId;
                            return View("Create", vm);
                        }

                        //LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        //{
                        //    DocTypeId = pt.DocTypeId,
                        //    DocId = pt.MaterialPlanHeaderId,
                        //    ActivityType = (int)ActivityTypeContants.Added,
                        //    DocNo = pt.DocNo,
                        //    DocDate = pt.DocDate,
                        //    DocStatus = pt.Status,
                        //}));

                        return RedirectToAction("Index", "MaterialPlanHeader", new { id = pt.DocTypeId });
                    }
                }
                #endregion
            }
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(vm.DocTypeId).DocumentTypeName;
            ViewBag.id = vm.DocTypeId;
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public ActionResult DueDateUpdate(int Id, DateTime NewDueDate)
        //{

        //    if (ModelState.IsValid)
        //    {
        //        List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

        //        PurchaseOrderHeader Head = db.PurchaseOrderHeader.Find(Id);

        //        PurchaseOrderHeader ExRec = Mapper.Map<PurchaseOrderHeader>(Head);

        //        Head.DueDate = NewDueDate;
        //        Head.ModifiedBy = User.Identity.Name;
        //        Head.ModifiedDate = DateTime.Now;

        //        LogList.Add(new LogTypeViewModel
        //        {
        //            ExObj = ExRec,
        //            Obj = Head,
        //        });

        //        Head.ObjectState = Model.ObjectState.Modified;
        //        db.PurchaseOrderHeader.Add(Head);

        //        XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

        //        try
        //        {
        //            db.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(new { Success = false });
        //        }

        //        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
        //        {
        //            DocTypeId = Head.DocTypeId,
        //            DocId = Head.PurchaseOrderHeaderId,
        //            ActivityType = (int)ActivityTypeContants.Modified,
        //            DocNo = Head.DocNo,
        //            DocDate = Head.DocDate,
        //            DocStatus = Head.Status,
        //            xEModifications = Modifications,
        //        }));

        //        return Json(new { Success = true });
        //    }

        //    return Json(new { Success = false });

        //}

        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public ActionResult PriorityUpdate(int Id, string Priority)
        //{

        //    if (ModelState.IsValid)
        //    {
        //        List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

        //        PurchaseOrderHeader Head = db.PurchaseOrderHeader.Find(Id);

        //        PurchaseOrderHeader ExRec = Mapper.Map<PurchaseOrderHeader>(Head);

        //        int t = (int)Enum.Parse(typeof(SaleOrderPriority), Priority);
        //        Head.Priority = t;
        //        Head.ModifiedBy = User.Identity.Name;
        //        Head.ModifiedDate = DateTime.Now;

        //        LogList.Add(new LogTypeViewModel
        //        {
        //            ExObj = ExRec,
        //            Obj = Head,
        //        });

        //        Head.ObjectState = Model.ObjectState.Modified;
        //        db.PurchaseOrderHeader.Add(Head);

        //        XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

        //        try
        //        {
        //            db.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(new { Success = false });
        //        }

        //        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
        //        {
        //            DocTypeId = Head.DocTypeId,
        //            DocId = Head.PurchaseOrderHeaderId,
        //            ActivityType = (int)ActivityTypeContants.Modified,
        //            DocNo = Head.DocNo,
        //            DocDate = Head.DocDate,
        //            DocStatus = Head.Status,
        //            xEModifications = Modifications,
        //        }));

        //        return Json(new { Success = true });
        //    }

        //    return Json(new { Success = false });

        //}


        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
            {
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);
                TempData.Remove("CSEXC");
            }

            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

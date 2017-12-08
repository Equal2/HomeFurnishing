using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using AutoMapper;
using Model.ViewModel;
using Model.ViewModels;
using System.Net;
using Reports.Controllers;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Configuration;
using Jobs.Helpers;

namespace Jobs.Controllers
{

    [Authorize]
    public class MaterialPlanCancelLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IMaterialPlanCancelForSaleOrderService _MaterialPlanCancelForSaleOrderService;
        IMaterialPlanCancelForProdOrderService _MaterialPlanCancelForProdOrderLineService;
        IMaterialPlanCancelLineService _MaterialPlanCanceliLine;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public MaterialPlanCancelLineController(IMaterialPlanCancelForSaleOrderService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec, IMaterialPlanCancelForProdOrderService prodorder, IMaterialPlanCancelLineService line)
        {
            _MaterialPlanCancelForSaleOrderService = SaleOrder;
            _MaterialPlanCancelForProdOrderLineService = prodorder;
            _MaterialPlanCanceliLine = line;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = new MaterialPlanCancelLineService(_unitOfWork).GetMaterialPlanCancelLineList(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
        }
        //For SaleOrder::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public ActionResult _ForSaleOrder(int id)
        {
            MaterialPlanCancelFilterViewModel vm = new MaterialPlanCancelFilterViewModel();
            vm.DocTypeId = new MaterialPlanCancelHeaderService(_unitOfWork).Find(id).DocTypeId;
            vm.MaterialPlanCancelHeaderId = id;
            MaterialPlanCancelHeader Header = new MaterialPlanCancelHeaderService(_unitOfWork).Find(id);
            MaterialPlanSettings Settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            vm.MaterialPlanSettings = Mapper.Map<MaterialPlanSettings, MaterialPlanSettingsViewModel>(Settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_Filters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(MaterialPlanCancelFilterViewModel vm)
        {

            string ValidationMsg = "";

            List<MaterialPlanCancelLineViewModel> temp = _MaterialPlanCanceliLine.GetOrderPlanForFilters(vm).ToList();
            MaterialPlanCancelLineListViewModel svm = new MaterialPlanCancelLineListViewModel();

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            svm.MaterialPlanSettings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(vm.DocTypeId, DivisionId, SiteId);

            svm.MaterialPlanCancelLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(MaterialPlanCancelLineListViewModel vm)
        {

            MaterialPlanCancelHeader header = new MaterialPlanCancelHeaderService(_unitOfWork).Find(vm.MaterialPlanCancelLineViewModel.FirstOrDefault().MaterialPlanCancelHeaderId);

            MaterialPlanSettings Setting = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            if (ModelState.IsValid)
            {

                var ProductIds = vm.MaterialPlanCancelLineViewModel.Select(m => m.ProductId).ToArray();

                List<MaterialPlanCancelLineViewModel> Line = new List<MaterialPlanCancelLineViewModel>();

                int i = 0;
                int si = 0;
                int linePk = 0;
                int Cnt = 0;

                var mPlanLineIds = vm.MaterialPlanCancelLineViewModel.Where(m => m.Qty > 0 && m.Qty <= m.BalanceQty).Select(m => m.MaterialPlanLineId).ToList();


                //var mPlanRecords = db.ViewMaterialPlanForSaleOrderBalance.AsNoTracking().Where(m => mPlanLineIds.Contains(m.MaterialPlanLineId.Value)).ToList();
                //Changed because ViewMaterialPlanForSaleOrderBalance is not required.
                var mPlanRecords = db.MaterialPlanForSaleOrder.AsNoTracking().Where(m => mPlanLineIds.Contains(m.MaterialPlanLineId.Value)).ToList();

                foreach (var item in vm.MaterialPlanCancelLineViewModel.Where(m => m.Qty > 0 && m.Qty <= m.BalanceQty))
                {

                    MaterialPlanCancelLine cline = new MaterialPlanCancelLine();
                    cline.MaterialPlanCancelHeaderId = header.MaterialPlanCancelHeaderId;
                    cline.CreatedBy = User.Identity.Name;
                    cline.CreatedDate = DateTime.Now;
                    cline.MaterialPlanLineId = item.MaterialPlanLineId;
                    cline.CreatedBy = User.Identity.Name;
                    cline.CreatedDate = DateTime.Now;
                    cline.ModifiedBy = User.Identity.Name;
                    cline.ModifiedDate = DateTime.Now;
                    cline.ObjectState = Model.ObjectState.Added;
                    cline.Qty = item.Qty;
                    cline.Remark = item.Remark;
                    cline.Sr = i++;
                    cline.MaterialPlanCancelLineId = linePk++;
                    cline.ObjectState = Model.ObjectState.Added;
                    db.MaterialPlanCancelLine.Add(cline);

                    SqlParameter SqlParameterMaterialPlanLineId = new SqlParameter("@MaterialPlanLineId", cline.MaterialPlanLineId);
                    SqlParameter SqlParameterQty = new SqlParameter("@Qty", cline.Qty);

                    IEnumerable<MaterialPlanForSaleOrderFifo> MaterialPlanForSaleOrderFifo = db.Database.SqlQuery<MaterialPlanForSaleOrderFifo>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetMaterialPlanForSaleOrderFifo @MaterialPlanLineId, @Qty", SqlParameterMaterialPlanLineId, SqlParameterQty).ToList();


                    //foreach (var detailSo in mPlanRecords.Where(m => m.MaterialPlanLineId == item.MaterialPlanLineId).ToList())
                    foreach (var detailSo in MaterialPlanForSaleOrderFifo)
                    {
                        MaterialPlanCancelForSaleOrder cso = new MaterialPlanCancelForSaleOrder();
                        cso.CreatedBy = User.Identity.Name;
                        cso.CreatedDate = DateTime.Now;
                        cso.ModifiedBy = User.Identity.Name;
                        cso.ModifiedDate = DateTime.Now;
                        cso.MaterialPlanCancelHeaderId = header.MaterialPlanCancelHeaderId;
                        cso.MaterialPlanCancelLineId = cline.MaterialPlanCancelLineId;
                        cso.MaterialPlanForSaleOrderId = detailSo.MaterialPlanForSaleOrderId;
                        cso.Qty = detailSo.Qty;
                        cso.Sr = si++;
                        cso.ObjectState = Model.ObjectState.Added;
                        db.MaterialPlanCancelForSaleOrder.Add(cso);
                    }


                    var MaterialPlanLine = new MaterialPlanLineService(_unitOfWork).Find(item.MaterialPlanLineId);
                    int ProdOrderCancelHeaderId = 0;
                    if (MaterialPlanLine.ProdPlanQty > 0)
                    {
                        ProdOrderCancelHeader ExistingProdOrderCancel = new ProdOrderCancelHeaderService(_unitOfWork).GetProdOrderCancelForMaterialPlan(header.MaterialPlanCancelHeaderId);
                        ProdOrderCancelHeader ProdOrderCancelHeader = new ProdOrderCancelHeader();

                        if (ExistingProdOrderCancel == null && Cnt == 0)
                        {
                            ProdOrderCancelHeader.CreatedBy = User.Identity.Name;
                            ProdOrderCancelHeader.CreatedDate = DateTime.Now;
                            ProdOrderCancelHeader.DivisionId = header.DivisionId;
                            ProdOrderCancelHeader.DocDate = header.DocDate;
                            ProdOrderCancelHeader.DocNo = header.DocNo;
                            ProdOrderCancelHeader.DocTypeId = Setting.DocTypeProductionOrderId.Value;
                            ProdOrderCancelHeader.MaterialPlanCancelHeaderId = header.MaterialPlanCancelHeaderId;
                            ProdOrderCancelHeader.ModifiedBy = User.Identity.Name;
                            ProdOrderCancelHeader.ModifiedDate = DateTime.Now;
                            ProdOrderCancelHeader.Remark = header.Remark;
                            ProdOrderCancelHeader.SiteId = header.SiteId;
                            ProdOrderCancelHeader.Status = (int)StatusConstants.System;
                            ProdOrderCancelHeader.ObjectState = Model.ObjectState.Added;
                            db.ProdOrderCancelHeader.Add(ProdOrderCancelHeader);
                            ProdOrderCancelHeaderId = ProdOrderCancelHeader.ProdOrderCancelHeaderId;

                            Cnt = Cnt + 1;
                        }
                        else
                        {
                            if (ExistingProdOrderCancel == null)
                            {
                                ProdOrderCancelHeaderId = ProdOrderCancelHeader.ProdOrderCancelHeaderId;
                            }
                            else
                            {
                                ProdOrderCancelHeaderId = ExistingProdOrderCancel.ProdOrderCancelHeaderId;
                            }
                        }


                        var ProdOrderLine = new ProdOrderLineService(_unitOfWork).GetProdOrderLineForMaterialPlan(item.MaterialPlanLineId);
                        int ProdOrderCancelLineKey = 0;
                        ProdOrderCancelLine ProdOrderCancelLine = new ProdOrderCancelLine();
                        ProdOrderCancelLine.ProdOrderCancelLineId = linePk++;
                        ProdOrderCancelLine.CreatedBy = User.Identity.Name;
                        ProdOrderCancelLine.CreatedDate = DateTime.Now;
                        ProdOrderCancelLine.ProdOrderLineId = ProdOrderLine.FirstOrDefault().ProdOrderLineId;
                        ProdOrderCancelLine.ModifiedBy = User.Identity.Name;
                        ProdOrderCancelLine.ModifiedDate = DateTime.Now;
                        ProdOrderCancelLine.ProdOrderCancelHeaderId = ProdOrderCancelHeaderId;
                        ProdOrderCancelLine.MaterialPlanCancelLineId = cline.MaterialPlanCancelLineId;
                        ProdOrderCancelLine.Qty = item.Qty;
                        ProdOrderCancelLine.ProdOrderCancelLineId = ProdOrderCancelLineKey--;
                        ProdOrderCancelLine.ObjectState = Model.ObjectState.Added;
                        db.ProdOrderCancelLine.Add(ProdOrderCancelLine);
                    }


                    if (MaterialPlanLine.PurchPlanQty > 0)
                    {
                        PurchaseIndentCancelHeader ExistingPurchaseIndentCancel = new PurchaseIndentCancelHeaderService(_unitOfWork).GetPurchaseIndentCancelForMaterialPlan(header.MaterialPlanCancelHeaderId);

                        if (ExistingPurchaseIndentCancel == null)
                        {
                            PurchaseIndentCancelHeader PurchaseIndentCancelHeader = new PurchaseIndentCancelHeader();

                            PurchaseIndentCancelHeader.CreatedBy = User.Identity.Name;
                            PurchaseIndentCancelHeader.CreatedDate = DateTime.Now;
                            PurchaseIndentCancelHeader.DivisionId = header.DivisionId;
                            PurchaseIndentCancelHeader.DocDate = header.DocDate;
                            PurchaseIndentCancelHeader.DocNo = header.DocNo;
                            PurchaseIndentCancelHeader.DocTypeId = Setting.DocTypeProductionOrderId.Value;
                            PurchaseIndentCancelHeader.MaterialPlanCancelHeaderId = header.MaterialPlanCancelHeaderId;
                            PurchaseIndentCancelHeader.ModifiedBy = User.Identity.Name;
                            PurchaseIndentCancelHeader.ModifiedDate = DateTime.Now;
                            PurchaseIndentCancelHeader.Remark = header.Remark;
                            PurchaseIndentCancelHeader.SiteId = header.SiteId;
                            PurchaseIndentCancelHeader.Status = (int)StatusConstants.System;
                            PurchaseIndentCancelHeader.ObjectState = Model.ObjectState.Added;
                            db.PurchaseIndentCancelHeader.Add(PurchaseIndentCancelHeader);
                        }


                        var PurchaseIndentLine = new PurchaseIndentLineService(_unitOfWork).GetPurchaseIndentLineForMaterialPlan(item.MaterialPlanLineId);
                        int PurchaseIndentCancelLineKey = 0;
                        PurchaseIndentCancelLine PurchaseIndentCancelLine = new PurchaseIndentCancelLine();
                        PurchaseIndentCancelLine.CreatedBy = User.Identity.Name;
                        PurchaseIndentCancelLine.CreatedDate = DateTime.Now;
                        PurchaseIndentCancelLine.PurchaseIndentLineId = PurchaseIndentLine.FirstOrDefault().PurchaseIndentLineId;
                        PurchaseIndentCancelLine.ModifiedBy = User.Identity.Name;
                        PurchaseIndentCancelLine.ModifiedDate = DateTime.Now;
                        PurchaseIndentCancelLine.PurchaseIndentCancelHeaderId = ExistingPurchaseIndentCancel.PurchaseIndentCancelHeaderId;
                        PurchaseIndentCancelLine.MaterialPlanCancelLineId = cline.MaterialPlanCancelLineId;
                        PurchaseIndentCancelLine.Qty = item.Qty;
                        PurchaseIndentCancelLine.PurchaseIndentCancelLineId = PurchaseIndentCancelLineKey--;
                        PurchaseIndentCancelLine.ObjectState = Model.ObjectState.Added;
                        db.PurchaseIndentCancelLine.Add(PurchaseIndentCancelLine);
                    }
                }

                try
                {
                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Results", vm);
                }

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }



        //For ProdOrder::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public ActionResult _ForProdOrder(int id)
        {
            MaterialPlanCancelFilterViewModel vm = new MaterialPlanCancelFilterViewModel();
            vm.DocTypeId = new MaterialPlanCancelHeaderService(_unitOfWork).Find(id).DocTypeId;
            vm.MaterialPlanCancelHeaderId = id;
            MaterialPlanCancelHeader Header = new MaterialPlanCancelHeaderService(_unitOfWork).Find(id);
            MaterialPlanSettings Settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            vm.MaterialPlanSettings = Mapper.Map<MaterialPlanSettings, MaterialPlanSettingsViewModel>(Settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);

            return PartialView("_FiltersProduction", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostProduction(MaterialPlanCancelFilterViewModel vm)
        {

            string ValidationMsg = "";

            List<MaterialPlanCancelLineViewModel> temp = _MaterialPlanCanceliLine.GetOrderPlanForFilters(vm).ToList();
            MaterialPlanCancelLineListViewModel svm = new MaterialPlanCancelLineListViewModel();

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            svm.MaterialPlanSettings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(vm.DocTypeId, DivisionId, SiteId);

            svm.MaterialPlanCancelLineViewModel = temp;
            return PartialView("_ResultsProduction", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPostProduction(MaterialPlanCancelLineListViewModel vm)
        {

            MaterialPlanCancelHeader header = new MaterialPlanCancelHeaderService(_unitOfWork).Find(vm.MaterialPlanCancelLineViewModel.FirstOrDefault().MaterialPlanCancelHeaderId);

            MaterialPlanSettings Setting = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            if (ModelState.IsValid)
            {

                var ProductIds = vm.MaterialPlanCancelLineViewModel.Select(m => m.ProductId).ToArray();

                List<MaterialPlanCancelLineViewModel> Line = new List<MaterialPlanCancelLineViewModel>();

                int i = 0;
                int si = 0;
                int linePk = 0;
                int poLinePK = 0;

                var mPlanLineIds = vm.MaterialPlanCancelLineViewModel.Where(m => m.Qty > 0 && m.Qty == m.BalanceQty).Select(m => m.MaterialPlanLineId).ToList();

                var mPlanProdOrderLineRecords = db.ViewMaterialPlanForProdOrderLineBalance.AsNoTracking().Where(m => mPlanLineIds.Contains(m.MaterialPlanLineId.Value)).ToList();

                var mPlanProdOrderIds = mPlanProdOrderLineRecords.Select(m => m.MaterialPlanForProdOrderId).Distinct().ToList();

                var mPlanProdOrderRecords = db.ViewMaterialPlanForProdOrderBalance.AsNoTracking().Where(m => mPlanProdOrderIds.Contains(m.MaterialPlanForProdOrderId)).ToList();


                foreach (var item in vm.MaterialPlanCancelLineViewModel.Where(m => m.Qty > 0 && m.Qty == m.BalanceQty))
                {
                    MaterialPlanCancelLine cline = new MaterialPlanCancelLine();
                    cline.MaterialPlanCancelHeaderId = header.MaterialPlanCancelHeaderId;
                    cline.CreatedBy = User.Identity.Name;
                    cline.CreatedDate = DateTime.Now;
                    cline.MaterialPlanLineId = item.MaterialPlanLineId;
                    cline.CreatedBy = User.Identity.Name;
                    cline.CreatedDate = DateTime.Now;
                    cline.ModifiedBy = User.Identity.Name;
                    cline.ModifiedDate = DateTime.Now;
                    cline.ObjectState = Model.ObjectState.Added;
                    cline.Qty = item.Qty;
                    cline.Remark = item.Remark;
                    cline.Sr = i++;
                    cline.MaterialPlanCancelLineId = linePk++;
                    cline.ObjectState = Model.ObjectState.Added;
                    db.MaterialPlanCancelLine.Add(cline);

                    var poLineRecords = mPlanProdOrderLineRecords.Where(m => m.MaterialPlanLineId == item.MaterialPlanLineId).ToList();

                    var ProdOrder = mPlanProdOrderRecords.Where(m => m.MaterialPlanForProdOrderId == poLineRecords.Select(t => t.MaterialPlanForProdOrderId).FirstOrDefault()).FirstOrDefault();

                    MaterialPlanCancelForProdOrder poLine = new MaterialPlanCancelForProdOrder();
                    poLine.CreatedBy = User.Identity.Name;
                    poLine.CreatedDate = DateTime.Now;
                    poLine.MaterialPlanCancelHeaderId = header.MaterialPlanCancelHeaderId;
                    poLine.MaterialPlanLineId = cline.MaterialPlanLineId;
                    poLine.MaterialPlanCancelForProdOrderId = poLinePK++;
                    poLine.ModifiedDate = DateTime.Now;
                    poLine.ModifiedBy = User.Identity.Name;
                    poLine.ObjectState = Model.ObjectState.Added;
                    poLine.Qty = ProdOrder.BalanceQty;
                    poLine.Sr = cline.Sr;
                    poLine.ObjectState = Model.ObjectState.Added;
                    db.MaterialPlanCancelForProdOrder.Add(poLine);


                    foreach (var detailSo in poLineRecords)
                    {
                        MaterialPlanCancelForProdOrderLine cso = new MaterialPlanCancelForProdOrderLine();
                        cso.MaterialPlanCancelLineId = cline.MaterialPlanCancelLineId;
                        cso.MaterialPlanCancelForProdOrderId = poLine.MaterialPlanCancelForProdOrderId;
                        cso.Qty = detailSo.BalanceQty;
                        cso.Sr = si++;
                        cso.ObjectState = Model.ObjectState.Added;
                        db.MaterialPlanCancelForProdOrderLine.Add(cso);
                    }


                    var MaterialPlanLine = new MaterialPlanLineService(_unitOfWork).Find(item.MaterialPlanLineId);

                    if (MaterialPlanLine.ProdPlanQty > 0)
                    {
                        ProdOrderCancelHeader ExistingProdOrderCancel = new ProdOrderCancelHeaderService(_unitOfWork).GetProdOrderCancelForMaterialPlan(header.MaterialPlanCancelHeaderId);

                        if (ExistingProdOrderCancel == null)
                        {
                            ProdOrderCancelHeader ProdOrderCancelHeader = new ProdOrderCancelHeader();

                            ProdOrderCancelHeader.CreatedBy = User.Identity.Name;
                            ProdOrderCancelHeader.CreatedDate = DateTime.Now;
                            ProdOrderCancelHeader.DivisionId = header.DivisionId;
                            ProdOrderCancelHeader.DocDate = header.DocDate;
                            ProdOrderCancelHeader.DocNo = header.DocNo;
                            ProdOrderCancelHeader.DocTypeId = Setting.DocTypeProductionOrderId.Value;
                            ProdOrderCancelHeader.MaterialPlanCancelHeaderId = header.MaterialPlanCancelHeaderId;
                            ProdOrderCancelHeader.ModifiedBy = User.Identity.Name;
                            ProdOrderCancelHeader.ModifiedDate = DateTime.Now;
                            ProdOrderCancelHeader.Remark = header.Remark;
                            ProdOrderCancelHeader.SiteId = header.SiteId;
                            ProdOrderCancelHeader.Status = (int)StatusConstants.System;
                            ProdOrderCancelHeader.ObjectState = Model.ObjectState.Added;
                            db.ProdOrderCancelHeader.Add(ProdOrderCancelHeader);
                        }


                        var ProdOrderLine = new ProdOrderLineService(_unitOfWork).GetProdOrderLineForMaterialPlan(item.MaterialPlanLineId);
                        int ProdOrderCancelLineKey = 0;
                        ProdOrderCancelLine ProdOrderCancelLine = new ProdOrderCancelLine();
                        ProdOrderCancelLine.CreatedBy = User.Identity.Name;
                        ProdOrderCancelLine.CreatedDate = DateTime.Now;
                        ProdOrderCancelLine.ProdOrderLineId = ProdOrderLine.FirstOrDefault().ProdOrderLineId;
                        ProdOrderCancelLine.ModifiedBy = User.Identity.Name;
                        ProdOrderCancelLine.ModifiedDate = DateTime.Now;
                        ProdOrderCancelLine.ProdOrderCancelHeaderId = ExistingProdOrderCancel.ProdOrderCancelHeaderId;
                        ProdOrderCancelLine.Qty = item.Qty;
                        ProdOrderCancelLine.ProdOrderCancelLineId = ProdOrderCancelLineKey--;
                        ProdOrderCancelLine.ObjectState = Model.ObjectState.Added;
                        db.ProdOrderCancelLine.Add(ProdOrderCancelLine);
                    }


                    if (MaterialPlanLine.PurchPlanQty > 0)
                    {
                        PurchaseIndentCancelHeader ExistingPurchaseIndentCancel = new PurchaseIndentCancelHeaderService(_unitOfWork).GetPurchaseIndentCancelForMaterialPlan(header.MaterialPlanCancelHeaderId);

                        if (ExistingPurchaseIndentCancel == null)
                        {
                            PurchaseIndentCancelHeader PurchaseIndentCancelHeader = new PurchaseIndentCancelHeader();

                            PurchaseIndentCancelHeader.CreatedBy = User.Identity.Name;
                            PurchaseIndentCancelHeader.CreatedDate = DateTime.Now;
                            PurchaseIndentCancelHeader.DivisionId = header.DivisionId;
                            PurchaseIndentCancelHeader.DocDate = header.DocDate;
                            PurchaseIndentCancelHeader.DocNo = header.DocNo;
                            PurchaseIndentCancelHeader.DocTypeId = Setting.DocTypeProductionOrderId.Value;
                            PurchaseIndentCancelHeader.MaterialPlanCancelHeaderId = header.MaterialPlanCancelHeaderId;
                            PurchaseIndentCancelHeader.ModifiedBy = User.Identity.Name;
                            PurchaseIndentCancelHeader.ModifiedDate = DateTime.Now;
                            PurchaseIndentCancelHeader.Remark = header.Remark;
                            PurchaseIndentCancelHeader.SiteId = header.SiteId;
                            PurchaseIndentCancelHeader.Status = (int)StatusConstants.System;
                            PurchaseIndentCancelHeader.ObjectState = Model.ObjectState.Added;
                            db.PurchaseIndentCancelHeader.Add(PurchaseIndentCancelHeader);
                        }


                        var PurchaseIndentLine = new PurchaseIndentLineService(_unitOfWork).GetPurchaseIndentLineForMaterialPlan(item.MaterialPlanLineId);
                        int PurchaseIndentCancelLineKey = 0;
                        PurchaseIndentCancelLine PurchaseIndentCancelLine = new PurchaseIndentCancelLine();
                        PurchaseIndentCancelLine.CreatedBy = User.Identity.Name;
                        PurchaseIndentCancelLine.CreatedDate = DateTime.Now;
                        PurchaseIndentCancelLine.PurchaseIndentLineId = PurchaseIndentLine.FirstOrDefault().PurchaseIndentLineId;
                        PurchaseIndentCancelLine.ModifiedBy = User.Identity.Name;
                        PurchaseIndentCancelLine.ModifiedDate = DateTime.Now;
                        PurchaseIndentCancelLine.PurchaseIndentCancelHeaderId = ExistingPurchaseIndentCancel.PurchaseIndentCancelHeaderId;
                        PurchaseIndentCancelLine.Qty = item.Qty;
                        PurchaseIndentCancelLine.PurchaseIndentCancelLineId = PurchaseIndentCancelLineKey--;
                        PurchaseIndentCancelLine.ObjectState = Model.ObjectState.Added;
                        db.PurchaseIndentCancelLine.Add(PurchaseIndentCancelLine);
                    }




                }

                try
                {
                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Results", vm);
                }

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }


        [HttpGet]
        public ActionResult _DeleteLine(int id)
        {
            return _Delete(id);
        }
        [HttpGet]
        public ActionResult _DeleteLine_AfterSubmit(int id)
        {
            return _Delete(id);
        }

        private ActionResult _Delete(int id)
        {
            var line = _MaterialPlanCanceliLine.GetMaterialPlanCancelLine(id);

            if (line == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason = line.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXCL"] += ExceptionMsg;
            #endregion

            if ((TimePlanValidation || Continue))
                ViewBag.LineMode = "Delete";

            MaterialPlanCancelHeader H = new MaterialPlanCancelHeaderService(_unitOfWork).Find(line.MaterialPlanCancelHeaderId);

            //Getting Settings
            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            line.MaterialPlanSettings = Mapper.Map<MaterialPlanSettings, MaterialPlanSettingsViewModel>(settings);
            line.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            return PartialView("_Create", line);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(MaterialPlanCancelLineViewModel vm)
        {
            bool BeforeSave = true;
            //try
            //{
            //    BeforeSave = MaterialPlanCancelDocEvents.beforeLineDeleteEvent(this, new PurchaseEventArgs(vm.MaterialPlanCancelHeaderId, vm.MaterialPlanCancelLineId), ref db);
            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXC"] += message;
            //    EventException = true;
            //}

            //if (!BeforeSave)
            //    TempData["CSEXC"] += "Validation failed before delete.";

            if (BeforeSave)
            {

                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                MaterialPlanCancelLine MaterialPlanCancelLine = db.MaterialPlanCancelLine.Find(vm.MaterialPlanCancelLineId);

                //try
                //{
                //    MaterialPlanCancelDocEvents.onLineDeleteEvent(this, new PurchaseEventArgs(MaterialPlanCancelLine.MaterialPlanCancelHeaderId, MaterialPlanCancelLine.MaterialPlanCancelLineId), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXCL"] += message;
                //    EventException = true;
                //}

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<MaterialPlanCancelLine>(MaterialPlanCancelLine),
                });


                IEnumerable<MaterialPlanCancelForSaleOrder> sOrderList = db.MaterialPlanCancelForSaleOrder.Where(m => m.MaterialPlanCancelLineId == MaterialPlanCancelLine.MaterialPlanCancelLineId).ToList();

                IEnumerable<MaterialPlanCancelForProdOrderLine> pOrderList = db.MaterialPlanCancelForProdOrderLine.Where(m => m.MaterialPlanCancelLineId == MaterialPlanCancelLine.MaterialPlanCancelLineId).ToList();

                MaterialPlanCancelForProdOrder prodOrder = db.MaterialPlanCancelForProdOrder.Find(pOrderList.Select(m => m.MaterialPlanCancelForProdOrderId).FirstOrDefault());

                foreach (var item2 in sOrderList)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item2,
                    });

                    item2.ObjectState = Model.ObjectState.Deleted;
                    db.MaterialPlanCancelForSaleOrder.Remove(item2);
                }

                foreach (var item2 in pOrderList)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item2,
                    });

                    item2.ObjectState = Model.ObjectState.Deleted;
                    db.MaterialPlanCancelForProdOrderLine.Remove(item2);
                }

                if (prodOrder != null)
                {
                    prodOrder.ObjectState = Model.ObjectState.Deleted;
                    db.MaterialPlanCancelForProdOrder.Remove(prodOrder);
                }


                IEnumerable<ProdOrderCancelLine> ProdOrderCancelLineListAutoGenerated = new ProdOrderCancelLineService(_unitOfWork).GetProdOrderCancelLineForMaterialPlanCancel(vm.MaterialPlanCancelLineId);

                foreach (ProdOrderCancelLine item in ProdOrderCancelLineListAutoGenerated)
                {
                    //new ProdOrderCancelLineService(_unitOfWork).Delete(item.ProdOrderCancelLineId);
                    ProdOrderCancelLine ProdOrderCancelLine = db.ProdOrderCancelLine.Find(item.ProdOrderCancelLineId);
                    ProdOrderCancelLine.ObjectState = Model.ObjectState.Deleted;
                    db.ProdOrderCancelLine.Remove(ProdOrderCancelLine);
                }



                MaterialPlanCancelHeader header = new MaterialPlanCancelHeaderService(_unitOfWork).Find(MaterialPlanCancelLine.MaterialPlanCancelHeaderId);
                //_MaterialPlanCancelLineService.Delete(vm.MaterialPlanCancelLineId);

                MaterialPlanCancelLine.ObjectState = Model.ObjectState.Deleted;
                db.MaterialPlanCancelLine.Remove(MaterialPlanCancelLine);


                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.MaterialPlanCancelHeader.Add(header);
                    //new MaterialPlanCancelHeaderService(_unitOfWork).Update(header);
                }
                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                try
                {
                    db.SaveChanges();
                    //_unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    return PartialView("_Create", vm);
                }

                //try
                //{
                //    MaterialPlanCancelDocEvents.afterLineDeleteEvent(this, new PurchaseEventArgs(MaterialPlanCancelLine.MaterialPlanCancelHeaderId, MaterialPlanCancelLine.MaterialPlanCancelLineId), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXC"] += message;
                //}

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.MaterialPlanCancelHeaderId,
                    DocLineId = MaterialPlanCancelLine.MaterialPlanCancelLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));

            }
            return Json(new { success = true });
        }


        public JsonResult GetProdOrders(int id, string term)//DocTypeId-->Changed To DocHeaderId
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            MaterialPlanCancelHeader Header = new MaterialPlanCancelHeaderService(_unitOfWork).Find(id);

            var temp = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(Header.DocTypeId, DivisionId, SiteId);

            string ProcName = temp.PendingProdOrderList;

            return Json(new ProdOrderHeaderService(_unitOfWork).GetProdOrdersForDocumentType(term, id, ProcName), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingMaterialPlans(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _MaterialPlanCanceliLine.GetPendingPlanningHelpList(filter, searchTerm);

            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            //return Json(new SaleOrderHeaderService(_unitOfWork).GetSaleOrdersForDocumentType(id, term), JsonRequestBehavior.AllowGet);
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

    public class MaterialPlanForSaleOrderFifo
    {
        public int MaterialPlanForSaleOrderId { get; set; }
        public Decimal Qty { get; set; }
    }
}

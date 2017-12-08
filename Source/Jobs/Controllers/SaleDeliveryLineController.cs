using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Model.ViewModel;
using System.Xml.Linq;
using Reports.Controllers;
using Jobs.Helpers;

namespace Jobs.Controllers
{

    [Authorize]
    public class SaleDeliveryLineController : System.Web.Mvc.Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleDeliveryLineService _SaleDeliveryLineService;
        IPackingLineService _PackingLineService;
        IStockService _StockService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public SaleDeliveryLineController(IStockService StockService, ISaleDeliveryLineService SaleDelivery, IUnitOfWork unitOfWork, IExceptionHandlingService exec, IPackingLineService packLineServ)
        {
            _SaleDeliveryLineService = SaleDelivery;
            _StockService = StockService;
            _PackingLineService = packLineServ;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            //var p = _SaleDeliveryLineService.GetSaleDeliveryLineListForIndex(id).ToList();
            var p = _SaleDeliveryLineService.GetSaleDeliveryLineListForIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        public ActionResult _Index(int id, int Status)
        {
            ViewBag.Status = Status;
            ViewBag.SaleDeliveryHeaderId = id;
            var p = _SaleDeliveryLineService.GetSaleDeliveryLineListForIndex(id).ToList();
            return PartialView(p);
        }
        public ActionResult _ForOrder(int id)
        {
            SaleDeliveryFilterViewModel vm = new SaleDeliveryFilterViewModel();
            vm.SaleDeliveryHeaderId = id;
            SaleDeliveryHeader H = new SaleDeliveryHeaderService(_unitOfWork).Find(id);
            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            vm.SaleDeliverySettings = Mapper.Map<SaleDeliverySetting, SaleDeliverySettingsViewModel>(settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            return PartialView("_OrderFilters", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostOrders(SaleDeliveryFilterViewModel vm)
        {
            List<SaleDeliveryLineViewModel> temp = _SaleDeliveryLineService.GetSaleInvoicesForFilters(vm).ToList();
            SaleDeliveryListViewModel svm = new SaleDeliveryListViewModel();
            svm.SaleDeliveryLineViewModel = temp;
            return PartialView("_Results", svm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(SaleDeliveryListViewModel vm)
        {
            int Cnt = 0;


            SaleDeliveryHeader Dh = new SaleDeliveryHeaderService(_unitOfWork).Find(vm.SaleDeliveryLineViewModel.FirstOrDefault().SaleDeliveryHeaderId);

            SaleDeliverySetting Settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(Dh.DocTypeId, Dh.DivisionId, Dh.SiteId);




            int pk = 0;
            int PackingPrimaryKey = 0;
            int DeliveryPrimaryKey = 0;


            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            if (ModelState.IsValid)
            {
                foreach (var item in vm.SaleDeliveryLineViewModel)
                {
                    decimal balqty = (from p in db.ViewSaleInvoiceBalanceForDelivery
                                      where p.SaleInvoiceLineId == item.SaleInvoiceLineId
                                      select p.BalanceQty).FirstOrDefault();
                    if (item.Qty > 0 && item.Qty <= balqty)
                    {

                        SaleDeliveryLine Dl = new SaleDeliveryLine();

                        Dl.UnitConversionMultiplier = item.UnitConversionMultiplier ?? 0;
                        Dl.DealQty = item.Qty * item.UnitConversionMultiplier ?? 0;
                        Dl.DealUnitId = item.DealUnitId;
                        Dl.CreatedBy = User.Identity.Name;
                        Dl.CreatedDate = DateTime.Now;
                        Dl.ModifiedBy = User.Identity.Name;
                        Dl.ModifiedDate = DateTime.Now;
                        Dl.SaleDeliveryHeaderId = Dh.SaleDeliveryHeaderId;
                        Dl.Qty = item.Qty;
                        Dl.Remark = item.Remark;
                        Dl.SaleInvoiceLineId = item.SaleInvoiceLineId;
                        Dl.SaleDeliveryLineId = DeliveryPrimaryKey++;
                        Dl.ObjectState = Model.ObjectState.Added;
                        _SaleDeliveryLineService.Create(Dl);



                        LineList.Add(new LineDetailListViewModel { Amount = 0, Rate = 0, LineTableId = Dl.SaleDeliveryLineId, HeaderTableId = item.SaleDeliveryHeaderId, PersonID = Dh.SaleToBuyerId });

                        pk++;
                        Cnt = Cnt + 1;
                    }
                }

                new SaleDeliveryHeaderService(_unitOfWork).Update(Dh);


                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    return PartialView("_Results", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Dh.DocTypeId,
                    DocId = Dh.SaleDeliveryHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Dh.DocNo,
                    DocDate = Dh.DocDate,
                    DocStatus = Dh.Status,
                }));


                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }

        private void PrepareViewBag()
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        [HttpGet]
        public ActionResult CreateLine(int id, bool? IsSaleBased)
        {
            return _Create(id, IsSaleBased);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, bool? IsSaleBased)
        {
            return _Create(id, IsSaleBased);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int id, bool? IsSaleBased)
        {
            return _Create(id, IsSaleBased);
        }

        public ActionResult _Create(int Id, bool? IsSaleBased) //Id ==>Sale Order Header Id
        {
            SaleDeliveryHeader H = new SaleDeliveryHeaderService(_unitOfWork).Find(Id);
            SaleDeliveryLineViewModel s = new SaleDeliveryLineViewModel();



            //Getting Settings
            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.SaleDeliverySettings = Mapper.Map<SaleDeliverySetting, SaleDeliverySettingsViewModel>(settings);
            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.SaleDeliveryHeaderId = H.SaleDeliveryHeaderId;
            s.SaleDeliveryHeaderDocNo = H.DocNo;
            s.DocTypeId = H.DocTypeId;
            s.SiteId = H.SiteId;
            s.DivisionId = H.DivisionId;




            ViewBag.LineMode = "Create";
            PrepareViewBag();
            if (IsSaleBased == true)
            {
                return PartialView("_CreateForSaleOrder", s);

            }
            else
            {
                return PartialView("_Create", s);
            }

        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult _CreatePost(SaleDeliveryLineViewModel svm)
        {
            SaleDeliverySetting Settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(svm.DocTypeId,svm.DivisionId, svm.SiteId);
            SaleDeliveryHeader Dh = new SaleDeliveryHeaderService(_unitOfWork).Find(svm.SaleDeliveryHeaderId);

            if (svm.SaleDeliveryLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.SaleInvoiceLineId <= 0)
            {
                ModelState.AddModelError("SaleInvoiceLineId", "Sale Invoice field is required");
            }

            if (svm.Qty <= 0)
                ModelState.AddModelError("Qty", "The Qty field is required");




            if (ModelState.IsValid)
            {
                if (svm.SaleDeliveryLineId <= 0)
                {
                    SaleDeliveryLine Dl = Mapper.Map<SaleDeliveryLineViewModel, SaleDeliveryLine>(svm);
                                        
                    
                    Dl.SaleDeliveryHeaderId = Dh.SaleDeliveryHeaderId;
                    Dl.CreatedBy = User.Identity.Name;
                    Dl.CreatedDate = DateTime.Now;
                    Dl.ModifiedBy = User.Identity.Name;
                    Dl.ModifiedDate = DateTime.Now;
                    Dl.ObjectState = Model.ObjectState.Added;
                    _SaleDeliveryLineService.Create(Dl);





                    if (Dh.Status != (int)StatusConstants.Drafted)
                    {
                        Dh.Status = (int)StatusConstants.Modified;
                    }

                    new SaleDeliveryHeaderService(_unitOfWork).Update(Dh);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag();
                        return PartialView("_Create", svm);
                            
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Dh.DocTypeId,
                        DocId = Dl.SaleDeliveryHeaderId,
                        DocLineId = Dl.SaleDeliveryLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = Dh.DocNo,
                        DocDate = Dh.DocDate,
                        DocStatus = Dh.Status,
                    }));


                    return RedirectToAction("_Create", new { id = Dh.SaleDeliveryHeaderId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    int status = Dh.Status;


                    SaleDeliveryLine Dl = _SaleDeliveryLineService.Find(svm.SaleDeliveryLineId);


                    SaleDeliveryLine ExRecD = new SaleDeliveryLine();
                    ExRecD = Mapper.Map<SaleDeliveryLine>(Dl);



                    Dl.SaleInvoiceLineId = svm.SaleInvoiceLineId;
                    Dl.Qty = svm.Qty;
                    Dl.DealQty = svm.DealQty;
                    Dl.DealUnitId = svm.DealUnitId;
                    Dl.Remark = svm.Remark;
                    _SaleDeliveryLineService.Update(Dl);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecD,
                        Obj = Dl,
                    });





                    if (Dh.Status != (int)StatusConstants.Drafted)
                    {
                        Dh.Status = (int)StatusConstants.Modified;
                        new SaleDeliveryHeaderService(_unitOfWork).Update(Dh);
                    }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag();
                        return PartialView("_Create", svm);
                            
                    }

                    //Saving the Activity Log      

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Dh.DocTypeId,
                        DocId = Dl.SaleDeliveryHeaderId,
                        DocLineId = Dl.SaleDeliveryLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = Dh.DocNo,
                        xEModifications = Modifications,
                        DocDate = Dh.DocDate,
                        DocStatus = Dh.Status,
                    }));

                    //End of Saving the Activity Log

                    return Json(new { success = true });

                }
            }
            PrepareViewBag();
            return PartialView("_Create", svm);
        }


        [HttpGet]
        public ActionResult _ModifyLine(int id)
        {
            return _Modify(id);
        }

        [HttpGet]
        public ActionResult _ModifyLineAfterSubmit(int id)
        {
            return _Modify(id);
        }

        [HttpGet]
        public ActionResult _ModifyLineAfterApprove(int id)
        {
            return _Modify(id);
        }


        private ActionResult _Modify(int id)
        {
            SaleDeliveryLine temp = _SaleDeliveryLineService.Find(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason = temp.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

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
                ViewBag.LineMode = "Edit";

            SaleDeliveryHeader H = new SaleDeliveryHeaderService(_unitOfWork).Find(temp.SaleDeliveryHeaderId);
            PrepareViewBag();

            SaleDeliveryLineViewModel vm = _SaleDeliveryLineService.GetSaleDeliveryLineForEdit(id);
            //Getting Settings
            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            vm.SaleDeliverySettings = Mapper.Map<SaleDeliverySetting, SaleDeliverySettingsViewModel>(settings);

            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            vm.SiteId = H.SiteId;
            vm.DivisionId = H.DivisionId;
            vm.DocTypeId = H.DocTypeId;

            return PartialView("_Create", vm);
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

        [HttpGet]
        public ActionResult _DeleteLine_AfterApprove(int id)
        {
            return _Delete(id);
        }

        private ActionResult _Delete(int id)
        {
            SaleDeliveryLine temp = _SaleDeliveryLineService.Find(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason = temp.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

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


            SaleDeliveryHeader H = new SaleDeliveryHeaderService(_unitOfWork).Find(temp.SaleDeliveryHeaderId);
            PrepareViewBag();

            SaleDeliveryLineViewModel vm = _SaleDeliveryLineService.GetSaleDeliveryLineForEdit(id);
            //Getting Settings
            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            vm.SaleDeliverySettings = Mapper.Map<SaleDeliverySetting, SaleDeliverySettingsViewModel>(settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            return PartialView("_Create", vm);
        }

        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DeletePost(SaleDeliveryLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            int? StockId = 0;

            SaleDeliveryHeader Dh = new SaleDeliveryHeaderService(_unitOfWork).Find(vm.SaleDeliveryHeaderId);


            int status = Dh.Status;


            SaleDeliveryLine Dl = _SaleDeliveryLineService.Find(vm.SaleDeliveryLineId);




            LogList.Add(new LogTypeViewModel
            {
                ExObj = Dl,
            });




            _SaleDeliveryLineService.Delete(Dl);




            if (Dh.Status != (int)StatusConstants.Drafted)
            {
                Dh.Status = (int)StatusConstants.Modified;
                new SaleDeliveryHeaderService(_unitOfWork).Update(Dh);
            }


            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                PrepareViewBag();
                return PartialView("_Create", vm);

            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = Dh.DocTypeId,
                DocId = Dh.SaleDeliveryHeaderId,
                DocLineId = Dl.SaleDeliveryLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                DocNo = Dh.DocNo,
                xEModifications = Modifications,
                DocDate = Dh.DocDate,
                DocStatus = Dh.Status,
            }));

            return Json(new { success = true });
        }

        public ActionResult _Detail(int id)
        {
            SaleDeliveryLine temp = _SaleDeliveryLineService.Find(id);

            SaleDeliveryHeader H = new SaleDeliveryHeaderService(_unitOfWork).Find(temp.SaleDeliveryHeaderId);
            PrepareViewBag();

            SaleDeliveryLineViewModel vm = _SaleDeliveryLineService.GetSaleDeliveryLineForEdit(id);
            //Getting Settings
            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            vm.SaleDeliverySettings = Mapper.Map<SaleDeliverySetting, SaleDeliverySettingsViewModel>(settings);

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", vm);

        }

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



        public JsonResult GetProductDetailJson(int ProductId, int SaleDeliveryHeaderId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);

            SaleDeliveryHeader Header = new SaleDeliveryHeaderService(_unitOfWork).Find(SaleDeliveryHeaderId);

            //ProductViewModel ProductJson = new ProductViewModel();

            //var DealUnitId = _SaleDeliveryLineService.GetSaleDeliveryLineList(SaleDeliveryHeaderId).OrderByDescending(m => m.SaleDeliveryLineId).FirstOrDefault();

            var DealUnitId = _SaleDeliveryLineService.GetSaleDeliveryLineList(Header.SaleDeliveryHeaderId).OrderByDescending(m => m.SaleDeliveryLineId).FirstOrDefault();

            var DlUnit = new UnitService(_unitOfWork).Find((DealUnitId == null) ? product.UnitId : DealUnitId.DealUnitId);


            return Json(new { ProductId = product.ProductId, StandardCost = product.StandardCost, UnitId = product.UnitId, UnitName = product.UnitName, DealUnitId = (DealUnitId == null) ? product.UnitId : DealUnitId.DealUnitId, DealUnitDecimalPlaces = DlUnit.DecimalPlaces, Specification = product.ProductSpecification, ProductCode = product.ProductCode, ProductName = product.ProductName });
        }


        public JsonResult GetOrderDetail(int OrderId)
        {
            return Json(new SaleOrderLineService(_unitOfWork).GetSaleOrderDetailForInvoice(OrderId));
        }

        public JsonResult getunitconversiondetailjson(int productid, string unitid, string DealUnitId, int SaleDeliveryHeaderId)
        {

            SaleDeliveryHeader Delivery = new SaleDeliveryHeaderService(_unitOfWork).Find(SaleDeliveryHeaderId);

            var Settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(Delivery.DocTypeId, Delivery.DivisionId, Delivery.SiteId);

            if (Settings.UnitConversionForId.HasValue && Settings.UnitConversionForId > 0)
            {
                UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversionForUCF(productid, unitid, DealUnitId, Settings.UnitConversionForId ?? 0);
                List<SelectListItem> unitconversionjson = new List<SelectListItem>();
                if (uc != null)
                {
                    unitconversionjson.Add(new SelectListItem
                    {
                        Text = uc.Multiplier.ToString(),
                        Value = uc.Multiplier.ToString()
                    });
                }
                else
                {
                    unitconversionjson.Add(new SelectListItem
                    {
                        Text = "0",
                        Value = "0"
                    });
                }


                return Json(unitconversionjson);
            }
            else
            {
                UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(productid, unitid, DealUnitId);
                List<SelectListItem> unitconversionjson = new List<SelectListItem>();
                if (uc != null)
                {
                    unitconversionjson.Add(new SelectListItem
                    {
                        Text = uc.Multiplier.ToString(),
                        Value = uc.Multiplier.ToString()
                    });
                }
                else
                {
                    unitconversionjson.Add(new SelectListItem
                    {
                        Text = "0",
                        Value = "0"
                    });
                }


                return Json(unitconversionjson);
            }

        }

        public JsonResult GetProductUIDDetailJson(string ProductUIDNo)
        {
            ProductUidDetail productuiddetail = new ProductUidService(_unitOfWork).FGetProductUidDetail(ProductUIDNo);

            List<ProductUidDetail> ProductUidDetailJson = new List<ProductUidDetail>();

            if (productuiddetail != null)
            {
                ProductUidDetailJson.Add(new ProductUidDetail()
                {
                    ProductId = productuiddetail.ProductId,
                    ProductName = productuiddetail.ProductName,
                    ProductUidId = productuiddetail.ProductUidId,
                });
            }

            return Json(ProductUidDetailJson);
        }

        public JsonResult GetSaleInvoiceDetailJson(int SaleInvoiceLineId)
        {
            var temp = (from L in db.ViewSaleInvoiceBalanceForDelivery
                        join Dl in db.SaleInvoiceLine on L.SaleInvoiceLineId equals Dl.SaleInvoiceLineId into SaleInvoiceLineTable
                        from SaleInvoiceLineTab in SaleInvoiceLineTable.DefaultIfEmpty()
                        join P in db.Product on L.ProductId equals P.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join U in db.Units on ProductTab.UnitId equals U.UnitId into UnitTable
                        from UnitTab in UnitTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on L.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on L.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        where L.SaleInvoiceLineId == SaleInvoiceLineId
                        select new
                        {
                            SaleInvoiceHeaderDocNo = L.SaleInvoiceNo,
                            UnitId = UnitTab.UnitId,
                            UnitName = UnitTab.UnitName,
                            DealUnitId = SaleInvoiceLineTab.DealUnitId,
                            UnitConversionMultiplier = SaleInvoiceLineTab.UnitConversionMultiplier,
                            ProductId = L.ProductId,
                            Dimension1Id = L.Dimension1Id,
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Id = L.Dimension2Id,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            BalanceQty = L.BalanceQty
                        }).FirstOrDefault();

            if (temp != null)
            {
                return Json(temp);
            }
            else
            {
                return null;
            }
        }

        public JsonResult GetPendingSaleOrderCount(int ProductId, int SaleDeliveryHeaderId)
        {
            int BuyerId = new SaleDeliveryHeaderService(_unitOfWork).Find(SaleDeliveryHeaderId).SaleToBuyerId;
            var temp = (from L in db.ViewSaleOrderBalance
                        where L.ProductId == ProductId && L.BuyerId == BuyerId
                        group new { L } by new { L.SaleOrderLineId } into Result
                        select new
                        {
                            Cnt = Result.Count()
                        }).FirstOrDefault();

            if (temp != null)
            {
                return Json(temp.Cnt);
            }
            else
            {
                return null;
            }
        }
        
        public JsonResult SetSingleSaleInvoiceLine(int Ids)
        {
            ComboBoxResult SaleInvoiceJson = new ComboBoxResult();

            var SaleInvoiceLine = from L in db.SaleInvoiceLine
                                join H in db.SaleInvoiceHeader on L.SaleInvoiceHeaderId equals H.SaleInvoiceHeaderId into SaleInvoiceHeaderTable
                                from SaleInvoiceHeaderTab in SaleInvoiceHeaderTable.DefaultIfEmpty()
                                where L.SaleInvoiceLineId == Ids
                                select new
                                {
                                    SaleInvoiceLineId = L.SaleInvoiceLineId,
                                    SaleInvoiceNo = L.Product.ProductName
                                };

            SaleInvoiceJson.id = SaleInvoiceLine.FirstOrDefault().ToString();
            SaleInvoiceJson.text = SaleInvoiceLine.FirstOrDefault().SaleInvoiceNo;

            return Json(SaleInvoiceJson);
        }

        public ActionResult GetCustomProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleDeliveryLineService.GetCustomProducts(filter, searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1))
                .Take(pageSize)
                .ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetSaleInvoiceForProduct(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleDeliveryLineService.GetSaleInvoiceHelpListForProduct(filter, searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1))
                .Take(pageSize)
                .ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult GetCustomProductsForSaleDelivery(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var Query = _SaleDeliveryLineService.GetPendingProductsForSaleDelivery(filter, searchTerm);

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

        }

        public JsonResult GetSaleInvoice(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleDeliveryLineService.GetPendingSaleInvoiceForDelivery(filter, searchTerm);

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
        }


    }
}

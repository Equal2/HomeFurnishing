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
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using Reports.Controllers;
using Jobs.Helpers;

namespace Jobs.Controllers
{

    [Authorize]
    public class SaleOrderLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleOrderLineService _SaleOrderLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public SaleOrderLineController(ISaleOrderLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleOrderLineService = SaleOrder;
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
            var p = _SaleOrderLineService.GetSaleOrderLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        private void PrepareViewBag(SaleOrderHeader H)
        {
            ViewBag.Docno = H.DocNo;
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        [HttpGet]
        public ActionResult CreateLine(int id)
        {
            return _Create(id);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id)
        {
            return _Create(id);
        }

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            SaleOrderHeader H = new SaleOrderHeaderService(_unitOfWork).GetSaleOrderHeader(Id);
            SaleOrderLineViewModel s = new SaleOrderLineViewModel();
            s.SaleOrderHeaderId = H.SaleOrderHeaderId;
            ViewBag.DocNo = H.DocNo;
            ViewBag.Status = H.Status;
            ViewBag.LineMode = "Create";


            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(H.DocTypeId, H.DivisionId, H.SiteId);
            s.SaleOrderSettings = Mapper.Map<SaleOrderSettings, SaleOrderSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            PrepareViewBag(H);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(SaleOrderLineViewModel svm)
        {
            SaleOrderLine s = Mapper.Map<SaleOrderLineViewModel, SaleOrderLine>(svm);
            SaleOrderHeader temp = new SaleOrderHeaderService(_unitOfWork).Find(s.SaleOrderHeaderId);
            //if (Command == "Submit" && (s.ProductId == 0))
            //    return RedirectToAction("Submit", "SaleOrderHeader", new { id = s.SaleOrderHeaderId }).Success("Data saved successfully");

            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettingsForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);

            if (svm.Qty <= 0)
            {
                ModelState.AddModelError("Qty", "Please Check Qty");
            }
            else if (svm.Rate <= 0 && settings.isMandatoryRate == true)
            {
                ModelState.AddModelError("Rate", "Please Check Rate");
            }
            else if (svm.Amount <= 0 && settings.isMandatoryRate == true)
            {
                ModelState.AddModelError("Amount", "Please Check Amount");
            }
            if (svm.DueDate < temp.DocDate)
            {
                ModelState.AddModelError("DueDate", "DueDate greater than DocDate");
            }

            if (svm.SaleOrderLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (ModelState.IsValid)
            {
                if (svm.SaleOrderLineId <= 0)
                {
                    StockViewModel StockViewModel = new StockViewModel();
                    //Posting in Stock
                    if (settings.isPostedInStock.HasValue && settings.isPostedInStock == true)
                    {
                        StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                        StockViewModel.DocHeaderId = temp.SaleOrderHeaderId;
                        StockViewModel.DocLineId = s.SaleOrderLineId;
                        StockViewModel.DocTypeId = temp.DocTypeId;
                        StockViewModel.StockHeaderDocDate = temp.DocDate;
                        StockViewModel.StockDocDate = temp.DocDate;
                        StockViewModel.DocNo = temp.DocNo;
                        StockViewModel.DivisionId = temp.DivisionId;
                        StockViewModel.SiteId = temp.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.HeaderProcessId = null;
                        StockViewModel.PersonId = temp.SaleToBuyerId;
                        StockViewModel.ProductId = s.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = null;
                        StockViewModel.GodownId = temp.GodownId ?? 0;
                        StockViewModel.ProcessId = null;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = null;
                        StockViewModel.Qty_Iss = 0;
                        StockViewModel.Qty_Rec = s.Qty;
                        StockViewModel.Rate = s.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = s.Specification;
                        StockViewModel.Dimension1Id = s.Dimension1Id;
                        StockViewModel.Dimension2Id = s.Dimension2Id;
                        StockViewModel.Dimension3Id = s.Dimension3Id;
                        StockViewModel.Dimension4Id = s.Dimension4Id;
                        StockViewModel.Remark = s.Remark;
                        StockViewModel.ProductUidId = null;
                        StockViewModel.Status = temp.Status;
                        StockViewModel.CreatedBy = temp.CreatedBy;
                        StockViewModel.CreatedDate = DateTime.Now;
                        StockViewModel.ModifiedBy = temp.ModifiedBy;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPost(ref StockViewModel);

                        if (StockPostingError != "")
                        {
                            ModelState.AddModelError("", StockPostingError);
                            return PartialView("_Create", svm);
                        }

                        s.StockId = StockViewModel.StockId;

                        if (temp.StockHeaderId == null)
                        {
                            temp.StockHeaderId = StockViewModel.StockHeaderId;
                        }
                    }
                 
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    _SaleOrderLineService.Create(s);

                    new SaleOrderLineStatusService(_unitOfWork).CreateLineStatus(s.SaleOrderLineId);

                    SaleOrderHeader header = new SaleOrderHeaderService(_unitOfWork).Find(s.SaleOrderHeaderId);

                    if (temp.StockHeaderId != null && temp.StockHeaderId != 0)
                    {
                        header.StockHeaderId = temp.StockHeaderId;
                    }

                    if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                    {
                        header.Status = (int)StatusConstants.Modified;
                        header.ModifiedDate = DateTime.Now;
                        header.ModifiedBy = User.Identity.Name;
                    }

                    new SaleOrderHeaderService(_unitOfWork).Update(header);


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(temp);
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = header.DocTypeId,
                        DocId = header.SaleOrderHeaderId,
                        DocLineId = s.SaleOrderLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = header.DocNo,
                        DocDate = header.DocDate,
                        DocStatus = header.Status,
                    }));


                    return RedirectToAction("_Create", new { id = s.SaleOrderHeaderId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    SaleOrderHeader header = new SaleOrderHeaderService(_unitOfWork).Find(svm.SaleOrderHeaderId);
                    StringBuilder logstring = new StringBuilder();
                    int status = header.Status;
                    SaleOrderLine temp1 = _SaleOrderLineService.Find(svm.SaleOrderLineId);

                    SaleOrderLine ExRec = new SaleOrderLine();
                    ExRec = Mapper.Map<SaleOrderLine>(temp1);

                    //End of Tracking the Modifications::


                    if (temp1.StockId != null)
                    {
                        StockViewModel StockViewModel = new StockViewModel();
                        StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                        StockViewModel.StockId = temp1.StockId ?? 0;
                        StockViewModel.DocHeaderId = temp1.SaleOrderHeaderId;
                        StockViewModel.DocLineId = temp1.SaleOrderLineId;
                        StockViewModel.DocTypeId = temp.DocTypeId;
                        StockViewModel.StockHeaderDocDate = temp.DocDate;
                        StockViewModel.StockDocDate = temp.DocDate;
                        StockViewModel.DocNo = temp.DocNo;
                        StockViewModel.DivisionId = temp.DivisionId;
                        StockViewModel.SiteId = temp.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.HeaderProcessId = null;
                        StockViewModel.PersonId = temp.SaleToBuyerId;
                        StockViewModel.ProductId = s.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = temp.GodownId;
                        StockViewModel.GodownId = temp.GodownId ?? 0;
                        StockViewModel.ProcessId = null;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = null;
                        StockViewModel.Qty_Iss = 0;
                        StockViewModel.Qty_Rec = s.Qty;
                        StockViewModel.Rate = temp1.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = temp1.Specification;
                        StockViewModel.Dimension1Id = temp1.Dimension1Id;
                        StockViewModel.Dimension2Id = temp1.Dimension2Id;
                        StockViewModel.Dimension3Id = temp1.Dimension3Id;
                        StockViewModel.Dimension4Id = temp1.Dimension4Id;
                        StockViewModel.Remark = s.Remark;
                        StockViewModel.ProductUidId = null;
                        StockViewModel.Status = temp.Status;
                        StockViewModel.CreatedBy = temp1.CreatedBy;
                        StockViewModel.CreatedDate = temp1.CreatedDate;
                        StockViewModel.ModifiedBy = User.Identity.Name;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPost(ref StockViewModel);

                        if (StockPostingError != "")
                        {
                            ModelState.AddModelError("", StockPostingError);
                            return PartialView("_Create", svm);
                        }
                    }

                    temp1.DueDate = svm.DueDate;
                    temp1.ProductId = svm.ProductId ?? 0;
                    temp1.Specification = svm.Specification;
                    temp1.Dimension1Id = svm.Dimension1Id;
                    temp1.Dimension2Id = svm.Dimension2Id;
                    temp1.Dimension3Id = svm.Dimension3Id;
                    temp1.Dimension4Id = svm.Dimension4Id;
                    temp1.Qty = svm.Qty ?? 0;
                    temp1.DealQty = svm.DealQty ?? 0;
                    temp1.DealUnitId = svm.DealUnitId;
                    temp1.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    temp1.Rate = svm.Rate ?? 0;
                    temp1.Amount = svm.Amount ?? 0;
                    temp1.Remark = svm.Remark;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    _SaleOrderLineService.Update(temp1);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp1,
                    });

                    if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                    {

                        header.Status = (int)StatusConstants.Modified;
                        header.ModifiedBy = User.Identity.Name;
                        header.ModifiedDate = DateTime.Now;
                        new SaleOrderHeaderService(_unitOfWork).Update(header);

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
                        PrepareViewBag(temp);
                        return PartialView("_Create", svm);
                    }

                    //Saving the Activity Log

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp1.SaleOrderHeaderId,
                        DocLineId = temp1.SaleOrderLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    //End of Saving the Activity Log

                    return Json(new { success = true });

                }
            }

            ViewBag.Status = temp.Status;
            PrepareViewBag(temp);
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
        private ActionResult _Modify(int id)
        {
            SaleOrderLine temp = _SaleOrderLineService.GetSaleOrderLine(id);

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

            SaleOrderHeader H = new SaleOrderHeaderService(_unitOfWork).GetSaleOrderHeader(temp.SaleOrderHeaderId);
            ViewBag.DocNo = H.DocNo;
            SaleOrderLineViewModel s = Mapper.Map<SaleOrderLine, SaleOrderLineViewModel>(temp);

            string BuyerSku = (_SaleOrderLineService.GetBuyerSKU(temp.ProductId, temp.SaleOrderHeaderId));

            s.BuyerSku = BuyerSku;

            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(H.DocTypeId, H.DivisionId, H.SiteId);
            s.SaleOrderSettings = Mapper.Map<SaleOrderSettings, SaleOrderSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.UnitId = new ProductService(_unitOfWork).Find(temp.ProductId).UnitId;
            PrepareViewBag(H);

            return PartialView("_Create", s);
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
        private ActionResult _Delete(int id)
        {
            SaleOrderLine temp = _SaleOrderLineService.GetSaleOrderLine(id);

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

            SaleOrderHeader H = new SaleOrderHeaderService(_unitOfWork).GetSaleOrderHeader(temp.SaleOrderHeaderId);
            ViewBag.DocNo = H.DocNo;


            SaleOrderLineViewModel s = Mapper.Map<SaleOrderLine, SaleOrderLineViewModel>(temp);
            s.UnitId = new ProductService(_unitOfWork).Find(temp.ProductId).UnitId;

            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(H.DocTypeId, H.DivisionId, H.SiteId);
            s.SaleOrderSettings = Mapper.Map<SaleOrderSettings, SaleOrderSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);


            PrepareViewBag(H);

            return PartialView("_Create", s);
        }

        [HttpGet]
        private ActionResult _Detail(int id)
        {
            SaleOrderLine temp = _SaleOrderLineService.GetSaleOrderLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            SaleOrderHeader H = new SaleOrderHeaderService(_unitOfWork).GetSaleOrderHeader(temp.SaleOrderHeaderId);
            ViewBag.DocNo = H.DocNo;
            SaleOrderLineViewModel s = Mapper.Map<SaleOrderLine, SaleOrderLineViewModel>(temp);
            s.UnitId = new ProductService(_unitOfWork).Find(temp.ProductId).UnitId;
            PrepareViewBag(H);


            return PartialView("_Create", s);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(SaleOrderLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            SaleOrderLine SaleOrderLine = _SaleOrderLineService.Find(vm.SaleOrderLineId);

            int? StockId = 0;
            StockId = SaleOrderLine.StockId;

            if (StockId != null)
            {
                new StockService(_unitOfWork).DeleteStock((int)StockId);
            }

            if (SaleOrderLine.ReferenceDocLineId != null && SaleOrderLine.ReferenceDocLineId != 0 && SaleOrderLine.ReferenceDocTypeId != null && SaleOrderLine.ReferenceDocTypeId != 0)
            {
                var temp = (from L in db.SaleEnquiryLine
                                       join H in db.SaleEnquiryHeader on L.SaleEnquiryHeaderId equals H.SaleEnquiryHeaderId into SaleEnquiryHeaderTable
                                       from SaleEnquiryHeaderTab in SaleEnquiryHeaderTable.DefaultIfEmpty()
                                       where SaleEnquiryHeaderTab.DocTypeId == SaleOrderLine.ReferenceDocTypeId && L.SaleEnquiryLineId == SaleOrderLine.ReferenceDocLineId
                                       select L).FirstOrDefault();
                if (temp != null)
                {
                    SaleEnquiryLine SaleEnquiryLine = new SaleEnquiryLineService(_unitOfWork).Find(temp.SaleEnquiryLineId);
                    SaleEnquiryLine.LockReason = null;
                    new SaleEnquiryLineService(_unitOfWork).Update(SaleEnquiryLine);
                }
            }

            new SaleOrderLineStatusService(_unitOfWork).Delete(vm.SaleOrderLineId);



            _SaleOrderLineService.Delete(vm.SaleOrderLineId);
            SaleOrderHeader header = new SaleOrderHeaderService(_unitOfWork).Find(SaleOrderLine.SaleOrderHeaderId);

            if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
            {
                header.Status = (int)StatusConstants.Modified;
                header.ModifiedBy = User.Identity.Name;
                header.ModifiedDate = DateTime.Now;
                new SaleOrderHeaderService(_unitOfWork).Update(header);
            }



            LogList.Add(new LogTypeViewModel
            {
                Obj = SaleOrderLine,
            });

            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

            try
            {
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                ViewBag.Docno = header.DocNo;
                ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
                return PartialView("_Create", vm);
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = header.DocTypeId,
                DocId = header.SaleOrderHeaderId,
                DocLineId = SaleOrderLine.SaleOrderLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                DocNo = header.DocNo,
                xEModifications = Modifications,
                DocDate = header.DocDate,
                DocStatus = header.Status,
            }));

            return Json(new { success = true });
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

        public JsonResult GetUnitConversionDetailJson(int ProductId, string UnitId, string DeliveryUnitId, int HeaderId)
        {

            int UnitConversionForId = new SaleOrderHeaderService(_unitOfWork).Find(HeaderId).UnitConversionForId;

            UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversionForUCF(ProductId, UnitId, DeliveryUnitId, UnitConversionForId);
            List<SelectListItem> UnitConversionJson = new List<SelectListItem>();
            if (uc != null)
            {
                UnitConversionJson.Add(new SelectListItem
                {
                    Text = uc.Multiplier.ToString(),
                    Value = uc.Multiplier.ToString()
                });
            }
            else
            {
                UnitConversionJson.Add(new SelectListItem
                {
                    Text = "0",
                    Value = "0"
                });
            }

            return Json(UnitConversionJson);
        }

        public JsonResult GetProductDetailJson(int ProductId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);
            List<Product> ProductJson = new List<Product>();

            ProductJson.Add(new Product()
            {
                ProductId = product.ProductId,
                StandardCost = product.StandardCost,
                UnitId = product.UnitId,
                ProductSpecification = product.ProductSpecification
            });

            return Json(ProductJson);
        }

        public JsonResult CheckForValidationinEdit(int ProductId, int SaleOrderHeaderId, int SaleOrderLineId)
        {
            var temp = (_SaleOrderLineService.CheckForProductExists(ProductId, SaleOrderHeaderId, SaleOrderLineId));
            return Json(new { returnvalue = temp });
        }

        public JsonResult CheckForValidation(int ProductId, int SaleOrderHeaderId)
        {
            var temp = (_SaleOrderLineService.CheckForProductExists(ProductId, SaleOrderHeaderId));
            return Json(new { returnvalue = temp });
        }


        public JsonResult GetBuyerSKU(int ProductId, int SaleOrderHeaderId)
        {
            string temp = (_SaleOrderLineService.GetBuyerSKU(ProductId, SaleOrderHeaderId));
            return Json(temp, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCustomProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleOrderLineService.GetCustomProducts(filter, searchTerm);
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



    }
}

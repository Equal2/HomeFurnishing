using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using AutoMapper;
using Model.ViewModels;
using Jobs.Helpers;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class SaleOrderCancelLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleOrderCancelLineService _SaleOrderCancelLineService;
        IUnitOfWork _unitOfWork;
        IActivityLogService _ActivityLogService;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public SaleOrderCancelLineController(ISaleOrderCancelLineService SaleOrderCancelLineService, IUnitOfWork unitOfWork, IActivityLogService aclog, IExceptionHandlingService exec)
        {
            _SaleOrderCancelLineService = SaleOrderCancelLineService;
            _unitOfWork = unitOfWork;
            _ActivityLogService = aclog;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        //public ActionResult Index(int id)
        //{
        //    var header = new SaleOrderCancelHeaderService(_unitOfWork).GetSaleOrderCancelHeader(id);
        //    if(header!=null)
        //    {
        //        ViewBag.SaleOrderCancelHeaderId = header.SaleOrderCancelHeaderId;
        //        ViewBag.CancelNo = header.DocNo;
        //    }

        //    var temp = _SaleOrderCancelLineService.GetSaleOrderCancelLineListForHeader(id);
        //    return View(temp);
        //}

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _SaleOrderCancelLineService.GetSaleOrderCancelLineListForHeader(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _CreateMultiple(int id, int bid)
        {
            SaleOrderCancelFilterViewModel vm = new SaleOrderCancelFilterViewModel();
            vm.SaleOrderCancelHeaderId = id;
            vm.BuyerId = bid;
            SaleOrderCancelHeader H = new SaleOrderCancelHeaderService(_unitOfWork).Find(id);
            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(H.DocTypeId, H.DivisionId, H.SiteId);
            vm.SaleOrderSettings = Mapper.Map<SaleOrderSettings, SaleOrderSettingsViewModel>(settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(SaleOrderCancelFilterViewModel vm)
        {
            List<SaleOrderCancelLineViewModel> temp = _SaleOrderCancelLineService.GetSaleOrderLineForMultiSelect(vm).ToList();
            SaleOrderCancelMasterDetailModel svm = new SaleOrderCancelMasterDetailModel();
            svm.SaleOrderCancelViewModels = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(SaleOrderCancelMasterDetailModel vm)
        {
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            var Header = new SaleOrderCancelHeaderService(_unitOfWork).Find(vm.SaleOrderCancelViewModels.FirstOrDefault().SaleOrderCancelHeaderId);

            if (ModelState.IsValid)
            {
                foreach (var item in vm.SaleOrderCancelViewModels)
                {
                    decimal balqty = (from p in db.ViewSaleOrderBalanceForCancellation
                                      where p.SaleOrderLineId == item.SaleOrderLineId
                                      select p.BalanceQty).FirstOrDefault();
                    if (balqty < item.Qty)
                    {
                        return View("_Results", vm).Danger("Qty Exceeding Bal Qty");
                    }
                    if (item.Qty > 0)
                    {
                        SaleOrderCancelLine line = new SaleOrderCancelLine();

                        line.SaleOrderCancelHeaderId = item.SaleOrderCancelHeaderId;
                        line.SaleOrderLineId = item.SaleOrderLineId ?? 0;
                        line.Qty = item.Qty;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.Remark = item.LineRemark;

                        LineStatus.Add(line.SaleOrderLineId, line.Qty);

                        _SaleOrderCancelLineService.Create(line);

                    }
                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ObjectState = Model.ObjectState.Modified;
                    new SaleOrderCancelHeaderService(_unitOfWork).Update(Header);
                }

                new SaleOrderLineStatusService(_unitOfWork).UpdateSaleQtyCancelMultiple(LineStatus, Header.DocDate);

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
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.SaleOrderCancelHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }

        [HttpGet]
        public ActionResult CreateLine(int Id, int bid)
        {
            return _Create(Id, bid);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int Id, int bid)
        {
            return _Create(Id, bid);
        }

        public ActionResult _Create(int Id, int bid) //Id ==>Sale Order Header Id
        {
            SaleOrderCancelLineViewModel svm = new SaleOrderCancelLineViewModel();
            var header = new SaleOrderCancelHeaderService(_unitOfWork).Find(Id);
            svm.SaleOrderCancelHeaderId = Id;
            svm.BuyerId = bid;
            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(header.DocTypeId, header.DivisionId, header.SiteId);
            svm.SaleOrderSettings = Mapper.Map<SaleOrderSettings, SaleOrderSettingsViewModel>(settings);
            svm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(header.DocTypeId);

            ViewBag.LineMode = "Create";
            return PartialView("_Create", svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(SaleOrderCancelLineViewModel svm)
        {

            SaleOrderCancelHeader temp2 = new SaleOrderCancelHeaderService(_unitOfWork).Find(svm.SaleOrderCancelHeaderId);

            if (svm.SaleOrderCancelLineId <= 0)
            {
                SaleOrderCancelLine s = new SaleOrderCancelLine();
                decimal balqty = (from p in db.ViewSaleOrderBalanceForCancellation
                                  where p.SaleOrderLineId == svm.SaleOrderLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Balance Qty");
                }
                if (svm.Qty == 0)
                {
                    ModelState.AddModelError("Qty", "Please Check Qty");
                }
                if (ModelState.IsValid)
                {
                    s.Remark = svm.LineRemark;
                    s.SaleOrderCancelHeaderId = svm.SaleOrderCancelHeaderId;
                    s.SaleOrderLineId = svm.SaleOrderLineId ?? 0;
                    s.Qty = svm.Qty;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    _SaleOrderCancelLineService.Create(s);

                    new SaleOrderLineStatusService(_unitOfWork).UpdateSaleQtyOnCancel(s.SaleOrderLineId, s.SaleOrderCancelLineId, temp2.DocDate, s.Qty);


                    if (temp2.Status != (int)StatusConstants.Drafted && temp2.Status != (int)StatusConstants.Import)
                    { temp2.Status = (int)StatusConstants.Modified; temp2.ModifiedBy = User.Identity.Name; temp2.ModifiedDate = DateTime.Now; }

                    new SaleOrderCancelHeaderService(_unitOfWork).Update(temp2);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp2.DocTypeId,
                        DocId = temp2.SaleOrderCancelHeaderId,
                        DocLineId = s.SaleOrderCancelLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp2.DocNo,
                        DocDate = temp2.DocDate,
                        DocStatus = temp2.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.SaleOrderCancelHeaderId, bid = svm.BuyerId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                SaleOrderCancelHeader temp = new SaleOrderCancelHeaderService(_unitOfWork).Find(svm.SaleOrderCancelHeaderId);

                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                SaleOrderCancelLine s = _SaleOrderCancelLineService.Find(svm.SaleOrderCancelLineId);

                SaleOrderCancelLine ExRec = new SaleOrderCancelLine();
                ExRec = Mapper.Map<SaleOrderCancelLine>(s);

                decimal balqty = (from p in db.ViewSaleOrderBalanceForCancellation
                                  where p.SaleOrderLineId == svm.SaleOrderLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty + s.Qty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Balance Qty");
                }


                if (ModelState.IsValid)
                {
                    if (svm.Qty > 0)
                    {
                        //Tracking the Modification::

                        if (status == (int)StatusConstants.Approved)
                        {
                            logstring.Append("Editing SaleOrderCancelLineId No:" + svm.SaleOrderCancelLineId + "::\n");
                            if ((s.Remark != svm.Remark))
                                logstring.Append(s.Remark + " Remark Modified to " + svm.Remark + "::\n");
                            if ((s.Qty != svm.Qty))
                                logstring.Append(s.Qty + " Qty Modified to " + svm.Qty + "::\n");
                        }
                        //Ennd of Checking Modifications

                        s.Remark = svm.LineRemark;
                        s.Qty = svm.Qty;
                        s.ModifiedBy = User.Identity.Name;
                        s.ModifiedDate = DateTime.Now;

                        new SaleOrderLineStatusService(_unitOfWork).UpdateSaleQtyOnCancel(s.SaleOrderLineId, s.SaleOrderCancelLineId, temp.DocDate, s.Qty);
                    }

                    _SaleOrderCancelLineService.Update(s);

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                    }
                    new SaleOrderCancelHeaderService(_unitOfWork).Update(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = s,
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
                        return PartialView("_Create", svm);
                    }

                    //SAving the Activity Log::

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.SaleOrderCancelHeaderId,
                        DocLineId = s.SaleOrderCancelLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    //End Of Saving Activity Log


                    return Json(new { success = true });
                }
                return PartialView("_Create", svm);
            }

        }

        [HttpPost]
        public ActionResult CompleteSaleOrderCancelLine(SaleOrderCancelMasterDetailModel vm)
        {
            int SaleOrderCancelHeaderId = 0;

            if (ModelState.IsValid)
            {
                foreach (var item in vm.SaleOrderCancelViewModels)
                {
                    decimal balqty = (from p in db.ViewSaleOrderBalanceForCancellation
                                      where p.SaleOrderLineId == item.SaleOrderLineId
                                      select p.BalanceQty).FirstOrDefault();
                    if (balqty < item.Qty)
                    {
                        return View(vm).Danger("Qty Exceeding Bal Qty");
                    }
                    if (item.Qty > 0)
                    {
                        SaleOrderCancelLine line = new SaleOrderCancelLine();

                        line.SaleOrderCancelHeaderId = item.SaleOrderCancelHeaderId;
                        line.SaleOrderLineId = item.SaleOrderLineId ?? 0;
                        line.Qty = item.Qty;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.Remark = item.LineRemark;

                        _SaleOrderCancelLineService.Create(line);


                    }
                    SaleOrderCancelHeaderId = item.SaleOrderCancelHeaderId;
                }
                //_unitOfWork.Save();

                SaleOrderCancelHeader header = new SaleOrderCancelHeaderService(_unitOfWork).Find(SaleOrderCancelHeaderId);

                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                { header.Status = (int)StatusConstants.Submitted; header.ModifiedBy = User.Identity.Name; header.ModifiedDate = DateTime.Now; }

                //Activity Log
                ActivityLog al = new ActivityLog()
                {
                    ActivityType = (int)StatusConstants.Submitted,
                    DocId = header.SaleOrderCancelHeaderId,
                    UserRemark = header.Remark,
                    Narration = "Sale Order Cancel Submitted with Cancel no:" + header.DocNo,
                    CreatedDate = DateTime.Now,
                    CreatedBy = User.Identity.Name,
                    DocTypeId = header.DocTypeId,

                };

                new SaleOrderCancelHeaderService(_unitOfWork).Update(header);
                _ActivityLogService.Create(al);
                _unitOfWork.Save();

                return RedirectToActionPermanent("Index", "SaleOrderCancelHeader");
            }
            return View(vm);
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
            SaleOrderCancelLineViewModel temp = _SaleOrderCancelLineService.GetSaleOrderCancelLineForEdit(id);


            SaleOrderCancelHeader Header = new SaleOrderCancelHeaderService(_unitOfWork).Find(temp.SaleOrderCancelHeaderId);

            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            temp.SaleOrderSettings = Mapper.Map<SaleOrderSettings, SaleOrderSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);

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

            return PartialView("_Create", temp);
        }

        [HttpGet]
        public ActionResult _Detail(int id)
        {
            SaleOrderCancelLineViewModel temp = _SaleOrderCancelLineService.GetSaleOrderCancelLine(id);
            if (temp == null)
            {
                return HttpNotFound();
            }

            return PartialView("_Create", temp);
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
        public ActionResult _Delete(int id)
        {
            SaleOrderCancelLineViewModel temp = _SaleOrderCancelLineService.GetSaleOrderCancelLineForEdit(id);


            SaleOrderCancelHeader Header = new SaleOrderCancelHeaderService(_unitOfWork).Find(temp.SaleOrderCancelHeaderId);

            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            temp.SaleOrderSettings = Mapper.Map<SaleOrderSettings, SaleOrderSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);

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

            return PartialView("_Create", temp);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(SaleOrderCancelLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            SaleOrderCancelLine SaleOrderLine = _SaleOrderCancelLineService.Find(vm.SaleOrderCancelLineId);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = SaleOrderLine,
            });

            SaleOrderCancelHeader header = new SaleOrderCancelHeaderService(_unitOfWork).Find(SaleOrderLine.SaleOrderCancelHeaderId);

            new SaleOrderLineStatusService(_unitOfWork).UpdateSaleQtyOnCancel(SaleOrderLine.SaleOrderLineId, SaleOrderLine.SaleOrderCancelLineId, header.DocDate, 0);

            _SaleOrderCancelLineService.Delete(vm.SaleOrderCancelLineId);

            if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
            {
                header.Status = (int)StatusConstants.Modified;
                header.ModifiedDate = DateTime.Now;
                header.ModifiedBy = User.Identity.Name;
                new SaleOrderCancelHeaderService(_unitOfWork).Update(header);
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
                return PartialView("_Create", vm);
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = header.DocTypeId,
                DocId = header.SaleOrderCancelHeaderId,
                DocLineId = SaleOrderLine.SaleOrderCancelLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                DocNo = header.DocNo,
                xEModifications = Modifications,
                DocDate = header.DocDate,
                DocStatus = header.Status,
            }));

            return Json(new { success = true });
        }




        public JsonResult GetSaleOrderList(int ProductId, int BuyerId)
        {
            return Json(new SaleOrderLineService(_unitOfWork).GetSaleOrderForProduct(ProductId, BuyerId));
        }

        public JsonResult GetSaleOrder(int LineId)
        {
            return Json(new SaleOrderLineService(_unitOfWork).GetSaleOrder(LineId));
        }
        class TempSaleOrderQty
        {
            public decimal BalanceQty { get; set; }
        }
        public JsonResult GetSaleOrderLineJson(int SaleOrderLineId)
        {
            //decimal s = (from p in db.ViewSaleOrderBalance
            //        where p.SaleOrderLineId == SaleOrderLineId
            //        select p.BalanceQty).FirstOrDefault();
            decimal s = _SaleOrderCancelLineService.GetBalanceQuantity(SaleOrderLineId);
            TempSaleOrderQty temp = new TempSaleOrderQty();
            temp.BalanceQty = s;
            return Json(temp);
        }

        public JsonResult GetSaleOrderDocNoOnLoad(int SaleOrderLineId)
        {
            var temp = new SaleOrderLineService(_unitOfWork).GetSaleOrderLineVM(SaleOrderLineId);

            return Json(temp);
        }

        public JsonResult GetPendingSaleOrders(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {

            var Query=_SaleOrderCancelLineService.GetPendingSaleOrderHelpList(filter, searchTerm);

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

        public ActionResult GetSaleOrderForProduct(string searchTerm, int pageSize, int pageNum, int PersonId)//DocTypeId
        {
            var Query = new SaleDispatchLineService(_unitOfWork).GetSaleOrderHelpListForProduct(PersonId, searchTerm);
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

        public JsonResult GetSaleOrderDetailJson(int SaleOrderLineId)
        {
            var temp = (from L in db.ViewSaleOrderBalance
                        join Dl in db.SaleOrderLine on L.SaleOrderLineId equals Dl.SaleOrderLineId into SaleOrderLineTable
                        from SaleOrderLineTab in SaleOrderLineTable.DefaultIfEmpty()
                        join P in db.Product on L.ProductId equals P.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join U in db.Units on ProductTab.UnitId equals U.UnitId into UnitTable
                        from UnitTab in UnitTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on L.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on L.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        where L.SaleOrderLineId == SaleOrderLineId
                        select new
                        {
                            SaleOrderHeaderDocNo = L.SaleOrderNo,
                            UnitId = UnitTab.UnitId,
                            UnitName = UnitTab.UnitName,
                            DealUnitId = SaleOrderLineTab.DealUnitId,
                            Specification = SaleOrderLineTab.Specification,
                            UnitConversionMultiplier = SaleOrderLineTab.UnitConversionMultiplier,
                            ProductId = L.ProductId,
                            Dimension1Id = L.Dimension1Id,
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Id = L.Dimension2Id,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Rate = L.Rate,
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

        public JsonResult SetSingleSaleOrderLine(int Ids)
        {
            ComboBoxResult SaleOrderJson = new ComboBoxResult();

            var SaleOrderLine = from L in db.SaleOrderLine
                                join H in db.SaleOrderHeader on L.SaleOrderHeaderId equals H.SaleOrderHeaderId into SaleOrderHeaderTable
                                from SaleOrderHeaderTab in SaleOrderHeaderTable.DefaultIfEmpty()
                                where L.SaleOrderLineId == Ids
                                select new
                                {
                                    SaleOrderLineId = L.SaleOrderLineId,
                                    SaleOrderNo = L.Product.ProductName
                                };

            SaleOrderJson.id = SaleOrderLine.FirstOrDefault().ToString();
            SaleOrderJson.text = SaleOrderLine.FirstOrDefault().SaleOrderNo;

            return Json(SaleOrderJson);
        }
    }

}

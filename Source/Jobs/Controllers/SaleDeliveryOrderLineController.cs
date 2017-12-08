using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using AutoMapper;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using Model.ViewModels;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class SaleDeliveryOrderLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleDeliveryOrderLineService _SaleDeliveryOrderLineService;
        IUnitOfWork _unitOfWork;
        IActivityLogService _ActivityLogService;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public SaleDeliveryOrderLineController(ISaleDeliveryOrderLineService SaleDeliveryOrderLineService, IUnitOfWork unitOfWork, IActivityLogService aclog, IExceptionHandlingService exec)
        {
            _SaleDeliveryOrderLineService = SaleDeliveryOrderLineService;
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

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _SaleDeliveryOrderLineService.GetSaleDeliveryOrderLineList(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _CreateMultiple(int id, int bid)
        {
            SaleDeliveryOrderFilterViewModel vm = new SaleDeliveryOrderFilterViewModel();
            vm.SaleDeliveryOrderHeaderId = id;
            vm.BuyerId = bid;
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(SaleDeliveryOrderFilterViewModel vm)
        {
            List<SaleDeliveryOrderLineViewModel> temp = _SaleDeliveryOrderLineService.GetSaleOrderLineForMultiSelect(vm).ToList();
            SaleDeliveryMasterDetailModel svm = new SaleDeliveryMasterDetailModel();
            svm.SaleDeliveryOrderLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(SaleDeliveryMasterDetailModel vm)
        {
            if (ModelState.IsValid)
            {

                SaleDeliveryOrderHeader Header = new SaleDeliveryOrderHeaderService(_unitOfWork).Find(vm.SaleDeliveryOrderLineViewModel.FirstOrDefault().SaleDeliveryOrderHeaderId);

                foreach (var item in vm.SaleDeliveryOrderLineViewModel)
                {
                    decimal balqty = (from p in db.ViewSaleOrderBalanceForCancellation
                                      where p.SaleOrderLineId == item.SaleOrderLineId
                                      select p.BalanceQty).FirstOrDefault();

                    if (item.Qty > 0 && item.Qty <= balqty)
                    {
                        SaleDeliveryOrderLine line = new SaleDeliveryOrderLine();

                        line.SaleDeliveryOrderHeaderId = item.SaleDeliveryOrderHeaderId;
                        line.SaleOrderLineId = item.SaleOrderLineId;
                        line.Qty = item.Qty;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.Remark = item.Remark;
                        _SaleDeliveryOrderLineService.Create(line);

                    }
                }

                if(Header.Status!=(int)StatusConstants.Drafted && Header.Status!=(int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ObjectState = Model.ObjectState.Modified;
                    new SaleDeliveryOrderHeaderService(_unitOfWork).Update(Header);
                }

                try
                {
                    _unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Results", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.SaleDeliveryOrderHeaderId,
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

            SaleDeliveryOrderLineViewModel svm = new SaleDeliveryOrderLineViewModel();

            SaleDeliveryOrderHeader H = new SaleDeliveryOrderHeaderService(_unitOfWork).Find(Id);

            //Getting Settings
            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            svm.SaleDeliveryOrderSettings = Mapper.Map<SaleDeliveryOrderSettings, SaleDeliveryOrderSettingsViewModel>(settings);
            ViewBag.LineMode = "Create";
            svm.SaleDeliveryOrderHeaderId = Id;
            return PartialView("_Create", svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(SaleDeliveryOrderLineViewModel svm)
        {

            if (svm.SaleDeliveryOrderLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.SaleDeliveryOrderLineId <= 0)
            {
                SaleDeliveryOrderLine s = new SaleDeliveryOrderLine();
                decimal balqty = (from p in db.ViewSaleOrderBalanceForCancellation
                                  where p.SaleOrderLineId == svm.SaleOrderLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Balance Qty");
                }
                if (svm.Qty <= 0)
                {
                    ModelState.AddModelError("Qty", "Please Check Qty");
                }
                if (ModelState.IsValid)
                {
                    s.Remark = svm.Remark;
                    s.SaleDeliveryOrderHeaderId = svm.SaleDeliveryOrderHeaderId;
                    s.SaleOrderLineId = svm.SaleOrderLineId;
                    s.Qty = svm.Qty;
                    s.DueDate = svm.DueDate;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    _SaleDeliveryOrderLineService.Create(s);

                    SaleDeliveryOrderHeader temp2 = new SaleDeliveryOrderHeaderService(_unitOfWork).Find(s.SaleDeliveryOrderHeaderId);
                    if (temp2.Status != (int)StatusConstants.Drafted && temp2.Status != (int)StatusConstants.Import)
                    {
                        temp2.Status = (int)StatusConstants.Modified;
                        temp2.ModifiedBy = User.Identity.Name;
                        temp2.ModifiedDate = DateTime.Now;
                    }

                    new SaleDeliveryOrderHeaderService(_unitOfWork).Update(temp2);

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
                        DocId = temp2.SaleDeliveryOrderHeaderId,
                        DocLineId = s.SaleDeliveryOrderLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp2.DocNo,
                        DocDate = temp2.DocDate,
                        DocStatus = temp2.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.SaleDeliveryOrderHeaderId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                SaleDeliveryOrderHeader temp = new SaleDeliveryOrderHeaderService(_unitOfWork).Find(svm.SaleDeliveryOrderHeaderId);
                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                SaleDeliveryOrderLine s = _SaleDeliveryOrderLineService.Find(svm.SaleDeliveryOrderLineId);

                SaleDeliveryOrderLine ExRec = new SaleDeliveryOrderLine();
                ExRec = Mapper.Map<SaleDeliveryOrderLine>(s);


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
                        s.Remark = svm.Remark;
                        s.Qty = svm.Qty;
                        s.ModifiedBy = User.Identity.Name;
                        s.ModifiedDate = DateTime.Now;
                    }

                    _SaleDeliveryOrderLineService.Update(s);


                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                        new SaleDeliveryOrderHeaderService(_unitOfWork).Update(temp);
                    }

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

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = s.SaleDeliveryOrderHeaderId,
                        DocLineId = s.SaleDeliveryOrderLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return Json(new { success = true });
                }
                return PartialView("_Create", svm);
            }

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
            SaleDeliveryOrderLineViewModel temp = _SaleDeliveryOrderLineService.GetSaleDeliveryOrderLine(id);

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

            SaleDeliveryOrderHeader H = new SaleDeliveryOrderHeaderService(_unitOfWork).Find(temp.SaleDeliveryOrderHeaderId);
            //Getting Settings
            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.SaleDeliveryOrderSettings = Mapper.Map<SaleDeliveryOrderSettings, SaleDeliveryOrderSettingsViewModel>(settings);

            return PartialView("_Create", temp);
        }

        [HttpGet]
        private ActionResult _Detail(int id)
        {
            SaleDeliveryOrderLineViewModel temp = _SaleDeliveryOrderLineService.GetSaleDeliveryOrderLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }         

            SaleDeliveryOrderHeader H = new SaleDeliveryOrderHeaderService(_unitOfWork).Find(temp.SaleDeliveryOrderHeaderId);
            //Getting Settings
            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.SaleDeliveryOrderSettings = Mapper.Map<SaleDeliveryOrderSettings, SaleDeliveryOrderSettingsViewModel>(settings);

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
        public ActionResult _DeleteLine_AfterApprove(int id)
        {
            return _Delete(id);
        }

        private ActionResult _Delete(int id)
        {
            SaleDeliveryOrderLineViewModel temp = _SaleDeliveryOrderLineService.GetSaleDeliveryOrderLine(id);

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

            SaleDeliveryOrderHeader H = new SaleDeliveryOrderHeaderService(_unitOfWork).Find(temp.SaleDeliveryOrderHeaderId);
            //Getting Settings
            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.SaleDeliveryOrderSettings = Mapper.Map<SaleDeliveryOrderSettings, SaleDeliveryOrderSettingsViewModel>(settings);

            return PartialView("_Create", temp);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(SaleDeliveryOrderLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            SaleDeliveryOrderLine SaleOrderLine = _SaleDeliveryOrderLineService.Find(vm.SaleDeliveryOrderLineId);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = SaleOrderLine,
            });

            _SaleDeliveryOrderLineService.Delete(vm.SaleDeliveryOrderLineId);
            SaleDeliveryOrderHeader header = new SaleDeliveryOrderHeaderService(_unitOfWork).Find(SaleOrderLine.SaleDeliveryOrderHeaderId);

            if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
            {
                header.Status = (int)StatusConstants.Modified;
                header.ModifiedBy = User.Identity.Name;
                header.ModifiedDate = DateTime.Now;
                new SaleDeliveryOrderHeaderService(_unitOfWork).Update(header);
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
                DocId = header.SaleDeliveryOrderHeaderId,
                DocLineId = SaleOrderLine.SaleDeliveryOrderLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                DocNo = header.DocNo,
                xEModifications = Modifications,
                DocDate = header.DocDate,
                DocStatus = header.Status,
            }));
         
            return Json(new { success = true });
        }



        public JsonResult GetPendingSaleDeliveryProducts(int id, string term)//DocTypeId
        {
            return Json(_SaleDeliveryOrderLineService.GetPendingProductsForSaleDelivery(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingSaleOrders(int id, string term)//DocTypeId
        {
            return Json(_SaleDeliveryOrderLineService.GetPendingSaleOrders(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomProducts(int id, string term, int Limit)//Indent Header ID
        {
            return Json(_SaleDeliveryOrderLineService.GetProductHelpList(id, term, Limit), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSaleOrderDetail(int LineId)
        {
            return Json(_SaleDeliveryOrderLineService.GetSaleOrderDetailBalanceForDelivery(LineId));
        }

    }

}

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
using AutoMapper;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class SaleOrderQtyAmendmentLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();


        ISaleOrderQtyAmendmentLineService _SaleOrderQtyAmendmentLineService;
        IUnitOfWork _unitOfWork;
        IActivityLogService _ActivityLogService;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;


        public SaleOrderQtyAmendmentLineController(ISaleOrderQtyAmendmentLineService SaleOrderQtyAmendmentLineService, IUnitOfWork unitOfWork, IActivityLogService aclog, IExceptionHandlingService exec)
        {
            _SaleOrderQtyAmendmentLineService = SaleOrderQtyAmendmentLineService;
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
            var p = _SaleOrderQtyAmendmentLineService.GetSaleOrderQtyAmendmentLineForHeader(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ForOrder(int id, int sid)
        {
            SaleOrderAmendmentFilterViewModel vm = new SaleOrderAmendmentFilterViewModel();
            vm.SaleOrderAmendmentHeaderId = id;
            vm.BuyerId = sid;
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(SaleOrderAmendmentFilterViewModel vm)
        {
            List<SaleOrderQtyAmendmentLineViewModel> temp = _SaleOrderQtyAmendmentLineService.GetPurchaseOrderLineForMultiSelect(vm).ToList();
            SaleOrderAmendmentMasterDetailModel svm = new SaleOrderAmendmentMasterDetailModel();
            svm.SaleOrderQtyAmendmentViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(SaleOrderAmendmentMasterDetailModel vm)
        {

            SaleOrderAmendmentHeader Header = new SaleOrderAmendmentHeaderService(_unitOfWork).Find(vm.SaleOrderQtyAmendmentViewModel.FirstOrDefault().SaleOrderAmendmentHeaderId);

            if (ModelState.IsValid)
            {
                foreach (var item in vm.SaleOrderQtyAmendmentViewModel)
                {
                    if (item.Qty > 0)
                    {
                        SaleOrderQtyAmendmentLine line = new SaleOrderQtyAmendmentLine();

                        line.SaleOrderAmendmentHeaderId = item.SaleOrderAmendmentHeaderId;
                        line.SaleOrderLineId = item.SaleOrderLineId;
                        line.Qty = item.Qty;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.Remark = item.Remark;

                        _SaleOrderQtyAmendmentLineService.Create(line);

                    }
                }
                
                if(Header.Status!=(int)StatusConstants.Drafted && Header.Status!=(int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ObjectState = Model.ObjectState.Modified;
                    new SaleOrderAmendmentHeaderService(_unitOfWork).Update(Header);
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
                    DocId = Header.SaleOrderAmendmentHeaderId,
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
        public ActionResult CreateLine(int Id, int sid)
        {
            return _Create(Id, sid);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int Id, int sid)
        {
            return _Create(Id, sid);
        }

        public ActionResult _Create(int Id, int sid) //Id ==>Sale Order Header Id
        {

            SaleOrderQtyAmendmentLineViewModel svm = new SaleOrderQtyAmendmentLineViewModel();
            svm.SaleOrderAmendmentHeaderId = Id;
            svm.BuyerId = sid;
            return PartialView("_Create", svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(SaleOrderQtyAmendmentLineViewModel svm)
        {

            if (svm.SaleOrderQtyAmendmentLineId <= 0)
            {
                SaleOrderQtyAmendmentLine s = new SaleOrderQtyAmendmentLine();

                if (svm.Qty == 0)
                {
                    ModelState.AddModelError("Qty", "Please Check Qty");
                }
                if (ModelState.IsValid)
                {
                    s.Remark = svm.Remark;
                    s.SaleOrderAmendmentHeaderId = svm.SaleOrderAmendmentHeaderId;
                    s.SaleOrderLineId = svm.SaleOrderLineId;
                    s.Qty = svm.Qty;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    _SaleOrderQtyAmendmentLineService.Create(s);

                    SaleOrderAmendmentHeader temp2 = new SaleOrderAmendmentHeaderService(_unitOfWork).Find(s.SaleOrderAmendmentHeaderId);
                    if (temp2.Status != (int)StatusConstants.Drafted && temp2.Status != (int)StatusConstants.Import)
                    {
                        temp2.Status = (int)StatusConstants.Modified; temp2.ModifiedBy = User.Identity.Name;
                        temp2.ModifiedDate = DateTime.Now;
                        new SaleOrderAmendmentHeaderService(_unitOfWork).Update(temp2);
                    }

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
                        DocId = s.SaleOrderAmendmentHeaderId,
                        DocLineId = s.SaleOrderLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp2.DocNo,
                        DocDate = temp2.DocDate,
                        DocStatus = temp2.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.SaleOrderAmendmentHeaderId, sid = svm.BuyerId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                SaleOrderAmendmentHeader temp = new SaleOrderAmendmentHeaderService(_unitOfWork).Find(svm.SaleOrderAmendmentHeaderId);
                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                SaleOrderQtyAmendmentLine s = _SaleOrderQtyAmendmentLineService.Find(svm.SaleOrderQtyAmendmentLineId);

                SaleOrderQtyAmendmentLine ExRec = new SaleOrderQtyAmendmentLine();
                ExRec = Mapper.Map<SaleOrderQtyAmendmentLine>(s);


                if (ModelState.IsValid)
                {
                    if (svm.Qty > 0)
                    {
                        //Tracking the Modification::

                        if (status == (int)StatusConstants.Approved)
                        {
                            logstring.Append("Editing SaleOrderQtyAmendmentLineId No:" + svm.SaleOrderQtyAmendmentLineId + "::\n");
                            if ((s.Remark != svm.Remark))
                                logstring.Append(s.Remark + " Remark Modified to " + svm.Remark + "::\n");
                            if ((s.Qty != svm.Qty))
                                logstring.Append(s.Qty + " Qty Modified to " + svm.Qty + "::\n");
                        }
                        //Ennd of Checking Modifications

                        s.Remark = svm.Remark;
                        s.Qty = svm.Qty;
                        s.ModifiedBy = User.Identity.Name;
                        s.ModifiedDate = DateTime.Now;
                    }

                    _SaleOrderQtyAmendmentLineService.Update(s);

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    { temp.Status = (int)StatusConstants.Modified; temp.ModifiedDate = DateTime.Now; temp.ModifiedBy = User.Identity.Name; }

                    new SaleOrderAmendmentHeaderService(_unitOfWork).Update(temp);

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
                        DocId = s.SaleOrderAmendmentHeaderId,
                        DocLineId = s.SaleOrderQtyAmendmentLineId,
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
            SaleOrderQtyAmendmentLineViewModel temp = _SaleOrderQtyAmendmentLineService.GetSaleOrderQtyAmendmentLine(id);

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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var line = _SaleOrderQtyAmendmentLineService.GetSaleOrderQtyAmendmentLine(id);

            if (line == null)
            {
                return HttpNotFound();
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


            return PartialView("_Create",line);
        }

        [HttpGet]
        public ActionResult _Detail(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var line = _SaleOrderQtyAmendmentLineService.GetSaleOrderQtyAmendmentLine(id);

            if (line == null)
            {
                return HttpNotFound();
            }

            return PartialView("_Create", line);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(SaleOrderQtyAmendmentLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            SaleOrderQtyAmendmentLine PurchaseOrderLine = _SaleOrderQtyAmendmentLineService.Find(vm.SaleOrderQtyAmendmentLineId);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = PurchaseOrderLine,
            });

            _SaleOrderQtyAmendmentLineService.Delete(vm.SaleOrderQtyAmendmentLineId);
            SaleOrderAmendmentHeader header = new SaleOrderAmendmentHeaderService(_unitOfWork).Find(PurchaseOrderLine.SaleOrderAmendmentHeaderId);
            if (header.Status != (int)StatusConstants.Drafted && header.Status!=(int)StatusConstants.Import)
            {
                header.Status = (int)StatusConstants.Modified;
                header.ModifiedBy = User.Identity.Name;
                header.ModifiedDate = DateTime.Now;
                new SaleOrderAmendmentHeaderService(_unitOfWork).Update(header);
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
                DocId = header.SaleOrderAmendmentHeaderId,
                DocLineId = PurchaseOrderLine.SaleOrderQtyAmendmentLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                DocNo = header.DocNo,
                xEModifications = Modifications,
                DocDate = header.DocDate,
                DocStatus = header.Status,
            }));

            return Json(new { success = true });
        }


        public JsonResult GetSaleOrders(int ProductId, int BuyerId)
        {
            return Json(new SaleOrderHeaderService(_unitOfWork).GetSaleOrders(ProductId, BuyerId).ToList());
        }

        public JsonResult GetLineDetail(int LineId)
        {
            return Json(new SaleOrderLineService(_unitOfWork).GetSaleOrderLineModel(LineId));
        }     

    }

}

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
using Model;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using Reports.Controllers;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProdOrderCancelLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        IProdOrderCancelLineService _ProdOrderCancelLineService;
        IUnitOfWork _unitOfWork;
        IActivityLogService _ActivityLogService;
        IExceptionHandlingService _exception;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;


        public ProdOrderCancelLineController(IProdOrderCancelLineService ProdOrderCancelLineService, IUnitOfWork unitOfWork, IActivityLogService aclog, IExceptionHandlingService exec)
        {
            _ProdOrderCancelLineService = ProdOrderCancelLineService;
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
            var p = _ProdOrderCancelLineService.GetProdOrderCancelLineListForHeader(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _CreateMultiple(int id)
        {
            ProdOrderCancelFilterViewModel vm = new ProdOrderCancelFilterViewModel();
            vm.ProdOrderCancelHeaderId = id;
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(ProdOrderCancelFilterViewModel vm)
        {
            List<ProdOrderCancelLineViewModel> temp = _ProdOrderCancelLineService.GetProdOrderLineForMultiSelect(vm).ToList();
            ProdOrderCancelMasterDetailModel svm = new ProdOrderCancelMasterDetailModel();
            svm.ProdOrderCancelViewModels = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(ProdOrderCancelMasterDetailModel vm)
        {
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            var Header = new ProdOrderCancelHeaderService(_unitOfWork).Find(vm.ProdOrderCancelViewModels.FirstOrDefault().ProdOrderCancelHeaderId);
            if (ModelState.IsValid)
            {
                foreach (var item in vm.ProdOrderCancelViewModels)
                {
                    decimal balqty = (from p in db.ViewProdOrderBalance
                                      where p.ProdOrderLineId == item.ProdOrderLineId
                                      select p.BalanceQty).FirstOrDefault();

                    if (item.Qty > 0 && item.Qty <= balqty)
                    {
                        ProdOrderCancelLine line = new ProdOrderCancelLine();

                        line.ProdOrderCancelHeaderId = item.ProdOrderCancelHeaderId;
                        line.ProdOrderLineId = item.ProdOrderLineId;
                        line.Qty = item.Qty;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        LineStatus.Add(line.ProdOrderLineId, line.Qty);
                        _ProdOrderCancelLineService.Create(line);

                    }
                }

                new ProdOrderLineStatusService(_unitOfWork).UpdateProdQtyCancelMultiple(LineStatus, Header.DocDate);

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                    Header.ObjectState = Model.ObjectState.Modified;
                }

                new ProdOrderCancelHeaderService(_unitOfWork).Update(Header);

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
                    DocId = Header.ProdOrderCancelHeaderId,
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


        public ActionResult _Create(int Id) //Id ==>Prod Order Cancel Header Id
        {
            ProdOrderCancelHeader Header = new ProdOrderCancelHeaderService(_unitOfWork).Find(Id);
            ProdOrderCancelLineViewModel svm = new ProdOrderCancelLineViewModel();

            //Getting Settings
            var settings = new ProdOrderSettingsService(_unitOfWork).GetProdOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            svm.ProdOrderSettings = Mapper.Map<ProdOrderSettings, ProdOrderSettingsViewModel>(settings);

            svm.ProdOrderCancelHeaderId = Id;
            ViewBag.LineMode = "Create";
            svm.DivisionId = Header.DivisionId;
            return PartialView("_Create", svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(ProdOrderCancelLineViewModel svm)
        {

            if (svm.ProdOrderCancelLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.ProdOrderCancelLineId <= 0)
            {

                ProdOrderCancelLine s = new ProdOrderCancelLine();
                decimal balqty = (from p in db.ViewProdOrderBalance
                                  where p.ProdOrderLineId == svm.ProdOrderLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Balance Qty");
                }
                if (svm.Qty == 0)
                {
                    ModelState.AddModelError("Qty", "Please Check Qty");
                }
                if (svm.ProdOrderLineId == 0)
                {
                    ModelState.AddModelError("ProdOrderLineId", "The Prod Order field is required");
                }
                if (ModelState.IsValid)
                {
                    s.ProdOrderCancelHeaderId = svm.ProdOrderCancelHeaderId;
                    s.ProdOrderLineId = svm.ProdOrderLineId;
                    s.Qty = svm.Qty;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    _ProdOrderCancelLineService.Create(s);

                    ProdOrderCancelHeader temp2 = new ProdOrderCancelHeaderService(_unitOfWork).Find(s.ProdOrderCancelHeaderId);

                    new ProdOrderLineStatusService(_unitOfWork).UpdateProdQtyOnCancel(s.ProdOrderLineId, s.ProdOrderCancelLineId, temp2.DocDate, s.Qty, ref db, false);


                    if (temp2.Status != (int)StatusConstants.Drafted && temp2.Status != (int)StatusConstants.Import)
                    { 
                        temp2.Status = (int)StatusConstants.Modified;
                        temp2.ModifiedBy = User.Identity.Name;
                        temp2.ModifiedDate = DateTime.Now;
                    }
                    new ProdOrderCancelHeaderService(_unitOfWork).Update(temp2);

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
                        DocId = temp2.ProdOrderCancelHeaderId,
                        DocLineId = s.ProdOrderCancelLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp2.DocNo,
                        DocDate = temp2.DocDate,
                        DocStatus = temp2.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.ProdOrderCancelHeaderId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                ProdOrderCancelHeader temp = new ProdOrderCancelHeaderService(_unitOfWork).Find(svm.ProdOrderCancelHeaderId);
                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                ProdOrderCancelLine s = _ProdOrderCancelLineService.Find(svm.ProdOrderCancelLineId);

                ProdOrderCancelLine ExRec = new ProdOrderCancelLine();
                ExRec = Mapper.Map<ProdOrderCancelLine>(s);

                decimal balqty = (from p in db.ViewProdOrderBalance
                                  where p.ProdOrderLineId == svm.ProdOrderLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty + s.Qty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Balance Qty");
                }


                if (ModelState.IsValid)
                {
                    if (svm.Qty > 0)
                    {
                        s.Qty = svm.Qty;
                        s.ModifiedBy = User.Identity.Name;
                        s.ModifiedDate = DateTime.Now;
                    }

                    _ProdOrderCancelLineService.Update(s);

                    new ProdOrderLineStatusService(_unitOfWork).UpdateProdQtyOnCancel(s.ProdOrderLineId, s.ProdOrderCancelLineId, temp.DocDate, s.Qty, ref db, false);

                    temp.Status = (int)StatusConstants.Modified;
                    new ProdOrderCancelHeaderService(_unitOfWork).Update(temp);

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
                        DocId = s.ProdOrderCancelHeaderId,
                        DocLineId = s.ProdOrderCancelLineId,
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
            ProdOrderCancelLineViewModel temp = _ProdOrderCancelLineService.GetProdOrderCancelLine(id);

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

            ProdOrderCancelHeader H = new ProdOrderCancelHeaderService(_unitOfWork).Find(temp.ProdOrderCancelHeaderId);
            //Getting Settings
            var settings = new ProdOrderSettingsService(_unitOfWork).GetProdOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.ProdOrderSettings = Mapper.Map<ProdOrderSettings, ProdOrderSettingsViewModel>(settings);


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
            ProdOrderCancelLineViewModel temp = _ProdOrderCancelLineService.GetProdOrderCancelLine(id);

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

            ProdOrderCancelHeader H = new ProdOrderCancelHeaderService(_unitOfWork).Find(temp.ProdOrderCancelHeaderId);
            //Getting Settings
            var settings = new ProdOrderSettingsService(_unitOfWork).GetProdOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.ProdOrderSettings = Mapper.Map<ProdOrderSettings, ProdOrderSettingsViewModel>(settings);


            return PartialView("_Create", temp);
        }



        [HttpGet]
        public ActionResult _Detail(int id)
        {
            ProdOrderCancelLineViewModel temp = _ProdOrderCancelLineService.GetProdOrderCancelLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            ProdOrderCancelHeader H = new ProdOrderCancelHeaderService(_unitOfWork).Find(temp.ProdOrderCancelHeaderId);
            //Getting Settings
            var settings = new ProdOrderSettingsService(_unitOfWork).GetProdOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.ProdOrderSettings = Mapper.Map<ProdOrderSettings, ProdOrderSettingsViewModel>(settings);


            return PartialView("_Create", temp);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(ProdOrderCancelLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            ProdOrderCancelLine SaleOrderLine = _ProdOrderCancelLineService.Find(vm.ProdOrderCancelLineId);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = SaleOrderLine,
            });

            ProdOrderCancelHeader header = new ProdOrderCancelHeaderService(_unitOfWork).Find(SaleOrderLine.ProdOrderCancelHeaderId);
            int status = header.Status;

            new ProdOrderLineStatusService(_unitOfWork).UpdateProdQtyOnCancel(SaleOrderLine.ProdOrderLineId, SaleOrderLine.ProdOrderCancelLineId, header.DocDate, 0, ref db, false);

            _ProdOrderCancelLineService.Delete(vm.ProdOrderCancelLineId);

            if (header.Status != (int)StatusConstants.Drafted)
            {
                header.Status = (int)StatusConstants.Modified;
                new ProdOrderCancelHeaderService(_unitOfWork).Update(header);
            }
            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                return PartialView("_Create", vm);
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = header.DocTypeId,
                DocId = header.ProdOrderCancelHeaderId,
                DocLineId = SaleOrderLine.ProdOrderCancelLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                DocNo = header.DocNo,
                xEModifications = Modifications,
                DocDate = header.DocDate,
                DocStatus = header.Status,
            }));

            return Json(new { success = true });
        }




        public JsonResult GetPendingOrders(int ProductId)
        {
            return Json(_ProdOrderCancelLineService.GetProdOrderForProduct(ProductId));
        }

        public JsonResult GetSaleOrder(int LineId)
        {
            return Json(new SaleOrderLineService(_unitOfWork).GetSaleOrder(LineId));
        }

        public JsonResult GetBalQtyForProdOrderLineJson(int ProdOrderLineId)
        {
            decimal s = _ProdOrderCancelLineService.GetBalanceQuantity(ProdOrderLineId);
            decimal BalanceQty = s;
            return Json(BalanceQty);
        }

        public JsonResult GetSaleOrderDocNoOnLoad(int SaleOrderLineId)
        {
            var temp = new SaleOrderLineService(_unitOfWork).GetSaleOrderLineVM(SaleOrderLineId);

            return Json(temp);
        }



        public JsonResult GetPendingProdOrders(int HeaderId, string term, int Limit)
        {
            return Json(_ProdOrderCancelLineService.GetPendingProdOrdersWithPatternMatch(HeaderId, term, Limit));
        }

        public JsonResult GetOrderDetail(int ProdOrderLineId)
        {
            return Json(new ProdOrderLineService(_unitOfWork).GetLineDetail(ProdOrderLineId));
        }


        public JsonResult GetPendingProdOrdersForCancel(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _ProdOrderCancelLineService.GetPendingProdOrders(filter, searchTerm);

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

        public JsonResult GetPendingProdOrderProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var Query = _ProdOrderCancelLineService.GetPendingProductsForProdOrderCancel(filter, searchTerm);

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




    }

}

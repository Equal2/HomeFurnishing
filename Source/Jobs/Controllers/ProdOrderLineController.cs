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
    public class ProdOrderLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IProdOrderLineService _ProdOrderLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public ProdOrderLineController(IProdOrderLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProdOrderLineService = SaleOrder;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }


        public ActionResult _ForMaterialPlan(int id)
        {
            ProdOrderLineFilterViewModel vm = new ProdOrderLineFilterViewModel();
            vm.ProdOrderHeaderId = id;
            ProdOrderHeader Header = new ProdOrderHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(ProdOrderLineFilterViewModel vm)
        {
            List<ProdOrderLineViewModel> temp = _ProdOrderLineService.GetProdOrderForFilters(vm).ToList();

            ProdOrderMasterDetailModel svm = new ProdOrderMasterDetailModel();
            svm.ProdOrderLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(ProdOrderMasterDetailModel vm)
        {
            int Serial = _ProdOrderLineService.GetMaxSr(vm.ProdOrderLineViewModel.FirstOrDefault().ProdOrderHeaderId);
            int Pk = 0;
            if (ModelState.IsValid)
            {
                foreach (var item in vm.ProdOrderLineViewModel)
                {
                    if (item.Qty > 0)
                    {
                        ProdOrderLine line = new ProdOrderLine();

                        line.ProdOrderHeaderId = item.ProdOrderHeaderId;
                        line.MaterialPlanLineId = item.MaterialPlanLineId;
                        line.ProductId = item.ProductId;
                        line.Dimension1Id = item.Dimension1Id;
                        line.Dimension2Id = item.Dimension2Id;
                        line.Specification = item.Specification;
                        line.Remark = item.Remark;
                        line.Qty = item.Qty;
                        line.Sr = Serial++;
                        line.ProdOrderLineId = Pk;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;

                        line.ObjectState = Model.ObjectState.Added;
                        _ProdOrderLineService.Create(line);

                        new ProdOrderLineStatusService(_unitOfWork).CreateLineStatus(line.ProdOrderLineId);

                        Pk++;
                    }
                }

                ProdOrderHeader Header = db.ProdOrderHeader.Find(vm.ProdOrderLineViewModel.FirstOrDefault().ProdOrderHeaderId);

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                    Header.ObjectState = Model.ObjectState.Modified;

                    new ProdOrderHeaderService(_unitOfWork).Update(Header);
                }

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
                    DocId = Header.ProdOrderHeaderId,
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
        public JsonResult Index(int id)
        {
            var p = _ProdOrderLineService.GetProdOrderLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
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

        public ActionResult _Create(int Id) //Id ==>Prod Order Header Id
        {
            ProdOrderHeader H = new ProdOrderHeaderService(_unitOfWork).Find(Id);
            ProdOrderLineViewModel s = new ProdOrderLineViewModel();

            //Getting Settings
            var settings = new ProdOrderSettingsService(_unitOfWork).GetProdOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.ProdOrderSettings = Mapper.Map<ProdOrderSettings, ProdOrderSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.ProdOrderHeaderId = H.ProdOrderHeaderId;
            ViewBag.DocNo = H.DocNo;
            ViewBag.Status = H.Status;
            ViewBag.LineMode = "Create";

            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(ProdOrderLineViewModel svm)
        {
            ProdOrderLine s = Mapper.Map<ProdOrderLineViewModel, ProdOrderLine>(svm);
            ProdOrderHeader temp = new ProdOrderHeaderService(_unitOfWork).Find(s.ProdOrderHeaderId);

            if (svm.Qty <= 0)
            {
                ModelState.AddModelError("Qty", "Please Check Qty");
            }

            if (svm.ProdOrderLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (ModelState.IsValid)
            {
                if (svm.ProdOrderLineId == 0)
                {
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.Sr = _ProdOrderLineService.GetMaxSr(s.ProdOrderHeaderId);
                    s.ObjectState = Model.ObjectState.Added;
                    _ProdOrderLineService.Create(s);

                    new ProdOrderLineStatusService(_unitOfWork).CreateLineStatus(s.ProdOrderLineId);

                    ProdOrderHeader header = new ProdOrderHeaderService(_unitOfWork).Find(s.ProdOrderHeaderId);
                    if (header.Status != (int)StatusConstants.Drafted)
                    {
                        header.Status = (int)StatusConstants.Modified;
                        header.ModifiedDate = DateTime.Now;
                        header.ModifiedBy = User.Identity.Name;
                        new ProdOrderHeaderService(_unitOfWork).Update(header);
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
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.ProdOrderHeaderId,
                        DocLineId = s.ProdOrderLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.ProdOrderHeaderId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProdOrderHeader header = new ProdOrderHeaderService(_unitOfWork).Find(svm.ProdOrderHeaderId);
                    StringBuilder logstring = new StringBuilder();
                    int status = header.Status;
                    ProdOrderLine temp1 = _ProdOrderLineService.Find(svm.ProdOrderLineId);

                    ProdOrderLine ExRec = new ProdOrderLine();
                    ExRec = Mapper.Map<ProdOrderLine>(temp1);

                    temp1.DueDate = svm.DueDate;
                    temp1.ProductId = svm.ProductId;
                    temp1.Qty = svm.Qty;
                    temp1.Remark = svm.Remark;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    _ProdOrderLineService.Update(temp1);


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
                        new ProdOrderHeaderService(_unitOfWork).Update(header);

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
                        return PartialView("_Create", svm);
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = s.ProdOrderHeaderId,
                        DocLineId = s.ProdOrderLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return Json(new { success = true });

                }
            }

            ViewBag.Status = temp.Status;
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
            ProdOrderLineViewModel temp = _ProdOrderLineService.GetProdOrderLine(id);

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

            ProdOrderHeader H = new ProdOrderHeaderService(_unitOfWork).Find(temp.ProdOrderHeaderId);
            //Getting Settings
            var settings = new ProdOrderSettingsService(_unitOfWork).GetProdOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.ProdOrderSettings = Mapper.Map<ProdOrderSettings, ProdOrderSettingsViewModel>(settings);

            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            ViewBag.DocNo = H.DocNo;

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
            ProdOrderLineViewModel temp = _ProdOrderLineService.GetProdOrderLine(id);

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

            ProdOrderHeader H = new ProdOrderHeaderService(_unitOfWork).Find(temp.ProdOrderHeaderId);
            //Getting Settings
            var settings = new ProdOrderSettingsService(_unitOfWork).GetProdOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.ProdOrderSettings = Mapper.Map<ProdOrderSettings, ProdOrderSettingsViewModel>(settings);

            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            ViewBag.DocNo = H.DocNo;

            return PartialView("_Create", temp);
        }

        [HttpGet]
        public ActionResult _Detail(int id)
        {
            ProdOrderLineViewModel temp = _ProdOrderLineService.GetProdOrderLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            ProdOrderHeader H = new ProdOrderHeaderService(_unitOfWork).Find(temp.ProdOrderHeaderId);
            //Getting Settings
            var settings = new ProdOrderSettingsService(_unitOfWork).GetProdOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.ProdOrderSettings = Mapper.Map<ProdOrderSettings, ProdOrderSettingsViewModel>(settings);

            ViewBag.DocNo = H.DocNo;

            return PartialView("_Create", temp);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(ProdOrderLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            ProdOrderLine ProdOrderLine = _ProdOrderLineService.Find(vm.ProdOrderLineId);
            ProdOrderHeader header = new ProdOrderHeaderService(_unitOfWork).Find(ProdOrderLine.ProdOrderHeaderId);
            int status = header.Status;

            LogList.Add(new LogTypeViewModel
            {
                ExObj = ProdOrderLine,
            });

            new ProdOrderLineStatusService(_unitOfWork).Delete(ProdOrderLine.ProdOrderLineId);

            _ProdOrderLineService.Delete(vm.ProdOrderLineId);

            if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
            {
                header.Status = (int)StatusConstants.Modified;
                header.ModifiedDate = DateTime.Now;
                header.ModifiedBy = User.Identity.Name;
                new ProdOrderHeaderService(_unitOfWork).Update(header);
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
                DocId = header.ProdOrderHeaderId,
                DocLineId = ProdOrderLine.ProdOrderLineId,
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

        public JsonResult GetUnitConversionDetailJson(int ProductId, string UnitId, string DeliveryUnitId)
        {
            UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(ProductId, UnitId, DeliveryUnitId);
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
                UnitId = product.UnitId
            });

            return Json(ProductJson);
        }

        //public JsonResult GetMaterialPlans(int id, string term)//Indent Header ID
        //{
        //    return Json(_ProdOrderLineService.GetPendingMaterialPlanHelpList(id, term), JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetMaterialPlans(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {

            var temp = _ProdOrderLineService.GetPendingMaterialPlanHelpList(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _ProdOrderLineService.GetPendingMaterialPlanHelpList(filter, searchTerm).Count();

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

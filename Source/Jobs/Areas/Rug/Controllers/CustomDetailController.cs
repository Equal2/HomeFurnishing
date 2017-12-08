using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Model.ViewModels;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using AutoMapper;
using Presentation;
using Model.ViewModel;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class CustomDetailController : System.Web.Mvc.Controller
    {
        
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ICustomDetailService _CustomDetailService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public CustomDetailController(ICustomDetailService CustomDetailService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _CustomDetailService = CustomDetailService;
            _ActivityLogService = ActivityLogService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }



        
        public ActionResult Index() 
        {
            IQueryable<CustomDetailViewModel> p = _CustomDetailService.GetCustomDetailViewModelForIndex();
            return View(p);
        }

        
        public ActionResult PendingToSubmit()
        {
            IQueryable<CustomDetailViewModel> p = _CustomDetailService.GetCustomDetailListPendingToSubmit();
            return View(p);
        }

        
        public ActionResult PendingToApprove()
        {
            IQueryable<CustomDetailViewModel> p = _CustomDetailService.GetCustomDetailListPendingToApprove();
            return View(p);
        }

        [HttpGet]
        public ActionResult NextPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _CustomDetailService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }

        [HttpGet]
        public ActionResult PrevPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _CustomDetailService.PrevId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        [HttpGet]
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        [HttpGet]
        public ActionResult Report()
        {

            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.CustomDetail);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        [HttpGet]
        public ActionResult Remove()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        private void PrepareViewBag(CustomDetailViewModel s)
        {
            ViewBag.DocTypeList = new DocumentTypeService(_unitOfWork).GetDocumentTypeList(TransactionDocCategoryConstants.CustomDetail);
        }

        // GET: /CustomDetail/Create
        
        public ActionResult Create()
        {
            CustomDetailViewModel p = new CustomDetailViewModel();
            p.DocDate = DateTime.Now.Date;
            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            p.DocNo = _CustomDetailService.GetMaxDocNo();
            PrepareViewBag(p);
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CustomDetailViewModel svm)
        {
            string DataValidationMsg = DataValidation(svm);
            
            if (DataValidationMsg != "")
            {
                PrepareViewBag(svm);
                return View(svm).Danger(DataValidationMsg);
            }

            if (ModelState.IsValid)
            {
                if (svm.CustomDetailId == 0)
                {
                    CustomDetail CustomDetail = Mapper.Map<CustomDetailViewModel, CustomDetail>(svm);
                    CustomDetail.CreatedDate = DateTime.Now;
                    CustomDetail.ModifiedDate = DateTime.Now;
                    CustomDetail.CreatedBy = User.Identity.Name;
                    CustomDetail.ModifiedBy = User.Identity.Name;
                    CustomDetail.Status = (int)StatusConstants.Drafted;
                    _CustomDetailService.Create(CustomDetail);

                    try
                    {
                        _unitOfWork.Save();
                    }
                  
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(svm);
                        return View("Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.CustomDetail).DocumentTypeId,
                        DocId = CustomDetail.CustomDetailId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("Edit", new { id = CustomDetail.CustomDetailId }).Success("Data saved Successfully");
                }

                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    CustomDetail CustomDetail = Mapper.Map<CustomDetailViewModel, CustomDetail>(svm);

                    CustomDetail ExRec = Mapper.Map<CustomDetail>(CustomDetail);

                    int status = CustomDetail.Status;

                    if (CustomDetail.Status == (int)StatusConstants.Approved)
                        CustomDetail.Status = (int)StatusConstants.Modified;
                    
                    CustomDetail.Status = (int)StatusConstants.Modified;
                    CustomDetail.ModifiedDate = DateTime.Now;
                    CustomDetail.ModifiedBy = User.Identity.Name;
                    _CustomDetailService.Update(CustomDetail);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = CustomDetail,
                    });
                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        _unitOfWork.Save();
                    }
                   
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.CustomDetail).DocumentTypeId,
                        DocId = CustomDetail.CustomDetailId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));
                 
                    return RedirectToAction("Index").Success("Data saved successfully");
                }
            }
            PrepareViewBag(svm);
            return View("Create", svm);
        }

        
        public ActionResult Edit(int id, string PrevAction, string PrevController)
        {
            if (PrevAction != null)
            {
                ViewBag.Redirect = PrevAction;
            }

            CustomDetail s = _CustomDetailService.Find(id);
            CustomDetailViewModel svm = new CustomDetailViewModel();
            svm = Mapper.Map<CustomDetail,CustomDetailViewModel>(s);

            PrepareViewBag(svm);
            if (svm == null)
            {
                return HttpNotFound();
            }
            return View("Create", svm);
        }


        public string DataValidation(CustomDetailViewModel svm)
        {
            string ValidationMsg = "";


            return ValidationMsg;
        }


        // GET: /PurchaseOrderHeader/Delete/5
        
        public ActionResult Delete(int id, string PrevAction, string PrevController)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CustomDetail CustomDetail = _CustomDetailService.Find(id);
            if (CustomDetail == null)
            {
                return HttpNotFound();
            }
            ReasonViewModel rvm = new ReasonViewModel()
            {
                id = id,
            };
            return PartialView("_Reason", rvm);

            if (PrevAction != null)
            {
                return RedirectToAction("Detail", new { id = id, returl = PrevAction, transactionType = "delete" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            {
                var CustomDetail = _CustomDetailService.GetCustomDetail(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = CustomDetail,
                });               

                new CustomDetailService(_unitOfWork).Delete(vm.id);
                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                try
                {
                    _unitOfWork.Save();
                }
             
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Reason", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.CustomDetail).DocumentTypeId,
                    DocId = CustomDetail.CustomDetailId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    xEModifications = Modifications,
                }));

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }


      
        
        public ActionResult Submit(int id, string Redirect)
        {
            return RedirectToAction("Detail", new { id = id, returl = Redirect, transactionType = "submit" });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Submit")]
        public ActionResult Submitted(int Id, string returl)
        {
            CustomDetail pd = new CustomDetailService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {
                ActivityLog al = new ActivityLog()
                {
                    ActivityType = (int)ActivityTypeContants.Submitted,
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    DocId = pd.CustomDetailId,                    
                    Narration = "Packing no" + pd.DocNo + " submitted.",
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.Packing).DocumentTypeId,

                };
                _ActivityLogService.Create(al);

                if (pd.Status == (int)StatusConstants.Modified)
                    pd.Status = (int)StatusConstants.ModificationSubmitted;
                else
                    pd.Status = (int)StatusConstants.Submitted;
                _CustomDetailService.Update(pd);
                _unitOfWork.Save();

                //SendEmail_PODrafted(Id);

                if (returl == "PTS")
                    return RedirectToAction("PendingToSubmit");
                else
                    return RedirectToAction("Index");
            }

            return View();
        }


        
        public ActionResult Approve(int id,string Redirect)
        {
            return RedirectToAction("Detail", new { id = id, returl = Redirect, transactionType = "approve" });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Approve")]
        public ActionResult Approved(int Id, string returl)
        {
            CustomDetail pd = new CustomDetailService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {
                ActivityLog al = new ActivityLog()
                {
                    ActivityType = (int)ActivityTypeContants.Approved,
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    DocId = pd.CustomDetailId,                    
                    Narration = "Packing No" + pd.DocNo + " submitted.",
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.Packing).DocumentTypeId,

                };
                _ActivityLogService.Create(al);
                pd.Status = (int)StatusConstants.Approved;
                _CustomDetailService.Update(pd);
                _unitOfWork.Save();

                //SendEmail_POApproved(Id);
                if (returl == "PTA")
                    return RedirectToAction("PendingToApprove");
                else
                    return RedirectToAction("Index");                
            }

            return View();
        }

        public ActionResult Detail(int id, string returl, string transactionType)
        {
            CustomDetailViewModel H = _CustomDetailService.GetCustomDetailViewModel(id);
            CustomDetailViewModelWithLog Header = Mapper.Map<CustomDetailViewModel, CustomDetailViewModelWithLog>(H);

            ViewBag.transactionType = transactionType;
            return View(Header);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult GetPendingInvoice(string searchTerm, int pageSize, int pageNum, int CustomDetailId, DateTime DocDate)
        {
            AutoCompleteComboBoxRepositoryAndHelper ar = new AutoCompleteComboBoxRepositoryAndHelper(_CustomDetailService.GetInvoicePendingForCustomDetail(CustomDetailId, DocDate));


            List<ComboBoxList> prodLst = ar.GetListForComboBox(searchTerm, pageSize, pageNum);
            int prodCount = ar.GetCountForComboBox(searchTerm, pageSize, pageNum);

            //Translate the attendees into a format the select2 dropdown expects
            ComboBoxPagedResult pagedAttendees = ar.TranslateToComboBoxFormat(prodLst, prodCount);

            //Return the data as a jsonp result
            return new JsonResult
            {
                Data = pagedAttendees,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}

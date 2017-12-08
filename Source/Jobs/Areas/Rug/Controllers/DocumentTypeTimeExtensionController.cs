using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Model.ViewModels;
using Jobs.Helpers;
using System.Threading.Tasks;
using NotificationContents;
//using Models.Login.Models;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class DocumentTypeTimeExtensionController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IDocumentTypeTimeExtensionService _DocumentTypeTimeExtensionService;
        IExceptionHandlingService _exception;
        public DocumentTypeTimeExtensionController(IExceptionHandlingService exec)
        {
            _DocumentTypeTimeExtensionService = new DocumentTypeTimeExtensionService(db);
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        // GET: /ProductMaster/

        private void PrepareViewBag()
        {
            List<SelectListItem> DocPlanTypes = new List<SelectListItem>();

            DocPlanTypes.Add(new SelectListItem { Text = DocumentTimePlanTypeConstants.Create, Value = DocumentTimePlanTypeConstants.Create });
            DocPlanTypes.Add(new SelectListItem { Text = DocumentTimePlanTypeConstants.Modify, Value = DocumentTimePlanTypeConstants.Modify });
            DocPlanTypes.Add(new SelectListItem { Text = DocumentTimePlanTypeConstants.Submit, Value = DocumentTimePlanTypeConstants.Submit });
            DocPlanTypes.Add(new SelectListItem { Text = DocumentTimePlanTypeConstants.Delete, Value = DocumentTimePlanTypeConstants.Delete });
            DocPlanTypes.Add(new SelectListItem { Text = DocumentTimePlanTypeConstants.GatePassCreate, Value = DocumentTimePlanTypeConstants.GatePassCreate });
            DocPlanTypes.Add(new SelectListItem { Text = DocumentTimePlanTypeConstants.GatePassCancel, Value = DocumentTimePlanTypeConstants.GatePassCancel });

            ViewBag.DocPlanTypeList = new SelectList(DocPlanTypes, "Value", "Text", "");

            ViewBag.SiteList = db.Site.Select(m => new { SiteId = m.SiteId, SiteName = m.SiteCode }).ToList();
            ViewBag.DivisionList = db.Divisions.Select(m => new { DivisionId = m.DivisionId, DivisionName = m.DivisionName }).ToList();

        }
        public ActionResult Index()
        {
            //return RedirectToAction("Create");
            var vm = _DocumentTypeTimeExtensionService.GetDocumentTypeTimeExtensionList(User.Identity.Name);
            return View(vm);
        }

        // GET: /ProductMaster/Create

        public ActionResult Create()
        {
            DocumentTypeTimeExtensionViewModel vm = new DocumentTypeTimeExtensionViewModel();

            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.ExpiryDate = DateTime.Now;
            vm.DocDate = DateTime.Now;
            PrepareViewBag();
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(DocumentTypeTimeExtensionViewModel vm)
        {
            DocumentTypeTimeExtension pt = Mapper.Map<DocumentTypeTimeExtensionViewModel, DocumentTypeTimeExtension>(vm);

            if (ModelState.IsValid)
            {

                var DocType = db.DocumentType.Where(m => m.DocumentTypeName == MasterDocTypeConstants.DocumentTypeTimeExtension).FirstOrDefault();

                if (vm.DocumentTypeTimeExtensionId <= 0)
                {

                    _DocumentTypeTimeExtensionService.Create(pt, User.Identity.Name);

                    try
                    {
                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = DocType.DocumentTypeId,
                        DocId = pt.DocumentTypeTimeExtensionId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    NotifyUser(pt.DocumentTypeTimeExtensionId);

                    return RedirectToAction("Create").Success("Data saved successfully");

                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    DocumentTypeTimeExtension temp = _DocumentTypeTimeExtensionService.Find(pt.DocumentTypeTimeExtensionId);

                    DocumentTypeTimeExtension ExRec = Mapper.Map<DocumentTypeTimeExtension>(pt);

                    temp.DocTypeId = pt.DocTypeId;
                    temp.Type = pt.Type;
                    temp.ExpiryDate = pt.ExpiryDate;
                    temp.UserName = pt.UserName;
                    temp.Reason = pt.Reason;
                    temp.NoOfRecords = pt.NoOfRecords;
                    temp.DocDate = pt.DocDate;

                    _DocumentTypeTimeExtensionService.Update(temp, User.Identity.Name);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });
                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = DocType.DocumentTypeId,
                        DocId = temp.DocumentTypeTimeExtensionId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index").Success("Data saved successfully");
                }

            }
            PrepareViewBag();
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            DocumentTypeTimeExtension pt = _DocumentTypeTimeExtensionService.Find(id);

            DocumentTypeTimeExtensionViewModel Temp = Mapper.Map<DocumentTypeTimeExtension, DocumentTypeTimeExtensionViewModel>(pt);
            PrepareViewBag();
            if (pt == null)
            {
                return HttpNotFound();
            }
            return View("Create", Temp);
        }

        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentTypeTimeExtension DocumentTypeTimeExtension = _DocumentTypeTimeExtensionService.Find(id);
            if (DocumentTypeTimeExtension == null)
            {
                return HttpNotFound();
            }

            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vm);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            if (ModelState.IsValid)
            {

                var temp = _DocumentTypeTimeExtensionService.Find(vm.id);

                var DocType = db.DocumentType.Where(m => m.DocumentTypeName == MasterDocTypeConstants.DocumentTypeTimeExtension).FirstOrDefault();

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                _DocumentTypeTimeExtensionService.Delete(temp);


                try
                {
                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Reason", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = DocType.DocumentTypeId,
                    DocId = vm.id,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    xEModifications = Modifications,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }


        [HttpGet]
        public ActionResult NextPage(int id)//CurrentHeaderId
        {
            var nextId = _DocumentTypeTimeExtensionService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _DocumentTypeTimeExtensionService.PrevId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }
        [HttpGet]
        public ActionResult Print()
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



        public JsonResult GetDocTypeHelpList(string searchTerm, int pageSize, int pageNum, string filter)//Order Header ID
        {

            var Query = _DocumentTypeTimeExtensionService.GetDocTypesHelpList(filter, searchTerm).Skip(pageSize * (pageNum - 1));

            var temp = Query.Take(pageSize).ToList();

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

        public JsonResult GetHistory()
        {
            var Temp = _DocumentTypeTimeExtensionService.GetHistory(User.Identity.Name);
            return Json(new
            {
                DocTypeId = Temp.DocTypeId,
                DocTypeName = Temp.DocTypeName,
                NoOfRecords = Temp.NoOfRecords,
                ExpiryDate = Temp.ExpiryDate,
                DocDate = Temp.DocDate.AddDays(1),
                Type = Temp.Type,
                UserName = Temp.UserName,
                Reason = Temp.Reason
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task NotifyUser(int id)//TaskId
        {
            DocumentTypeTimeExtension Temp = db.DocumentTypeTimeExtension.Find(id);

            db.Entry<DocumentTypeTimeExtension>(Temp).Reference(m => m.DocType).Load();

            Notification NN = new Notification();

            NN.NotificationSubjectId = (int)NotificationSubjectConstants.PermissionAssigned;
            NN.CreatedBy = User.Identity.Name;
            NN.CreatedDate = DateTime.Now;
            NN.ExpiryDate = DateTime.Now.AddDays(1);
            NN.IsActive = true;
            NN.ModifiedBy = User.Identity.Name;
            NN.ModifiedDate = DateTime.Now;
            NN.NotificationText = "Permission assigned for " + Temp.DocType.DocumentTypeName + " dated " + Temp.DocDate.ToString("dd/MMM/yyyy");


            TaskNotification nc = new TaskNotification();
            await nc.CreateTaskNotificationAsync(NN, Temp.UserName);

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
}

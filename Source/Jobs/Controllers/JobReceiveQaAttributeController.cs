using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using Jobs.Helpers;
using System.Configuration;
using System.Xml.Linq;
using DocumentEvents;
using CustomEventArgs;
using JobReceiveQADocumentEvents;
using Reports.Reports;
using Model.ViewModels;
using Reports.Controllers;
using System.Data.SqlClient;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobReceiveQAAttributeController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IUnitOfWork _unitOfWork;
        IJobReceiveQAAttributeService _JobReceiveQAAttributeService;
        IExceptionHandlingService _exception;
        public JobReceiveQAAttributeController(IExceptionHandlingService exec, IUnitOfWork uow)
        {
            _unitOfWork = uow;
            _JobReceiveQAAttributeService = new JobReceiveQAAttributeService(db);
            _exception = exec;
            if (!JobReceiveQAEvents.Initialized)
            {
                JobReceiveQAEvents Obj = new JobReceiveQAEvents();
            }

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }


        public void PrepareViewBag(int id)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();

            ViewBag.Name = db.DocumentType.Find(id).DocumentTypeName;
        }

        // GET: /JobReceiveQAAttributeMaster/

        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = new JobReceiveQAHeaderService(db).FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("Index", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        public ActionResult Index(int id)//DocumentTypeId
        {
            var JobReceiveQAAttribute = _JobReceiveQAAttributeService.GetJobReceiveQAAttributeList(id, User.Identity.Name);
            ViewBag.Name = db.DocumentType.Find(id).DocumentTypeName;
            ViewBag.id = id;
            return View(JobReceiveQAAttribute);
        }


        public ActionResult Create(int id, int DocTypeId)//JobReceiveLineId
        {
            //JobReceiveQAAttributeViewModel vm = new JobReceiveQAAttributeViewModel();
            JobReceiveQAAttributeViewModel vm = _JobReceiveQAAttributeService.GetJobReceiveLineDetail(id);

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;

            var temp = _JobReceiveQAAttributeService.GetJobReceiveQAAttribute(id);
            vm.QAGroupLine = temp;


            LastValues LastValues = _JobReceiveQAAttributeService.GetLastValues(DocTypeId);

            if (LastValues != null)
            {
                if (LastValues.QAById != null)
                {
                    vm.QAById = (int)LastValues.QAById;
                }
            }


            //Getting Settings
            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(DocTypeId, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveQASettings", new { id = DocTypeId }).Warning("Please create job Inspection settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(settings);

            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(DocTypeId);

            vm.ProcessId = settings.ProcessId;
            vm.DocDate = DateTime.Now;
            vm.DocTypeId = DocTypeId;

            vm.DocNo = new  JobReceiveQAHeaderService(db).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReceiveQAHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            PrepareViewBag(DocTypeId);
            ViewBag.Mode = "Add"; 
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobReceiveQAAttributeViewModel vm)
        {
            if (ModelState.IsValid)
            {
                #region CreateRecord
                if (vm.JobReceiveQALineId <= 0)
                {
                    JobReceiveQALine Line = new JobReceiveQALine();
                    Line = _JobReceiveQAAttributeService.Create(vm, User.Identity.Name);



                    try
                    {
                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(vm.DocTypeId);
                        ViewBag.Mode = "Add";
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = vm.DocTypeId,
                        DocId = vm.JobReceiveQAAttributeId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocDate = vm.DocDate,
                        DocNo = vm.DocNo,
                        DocStatus = vm.Status,
                    }));


                    return RedirectToAction("Edit", new { id = Line.JobReceiveQALineId }).Success("Data saved successfully");
                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobReceiveQAHeader temp = new JobReceiveQAHeaderService(db).Find(vm.JobReceiveQAHeaderId);

                    JobReceiveQAHeader ExRec = new JobReceiveQAHeader();
                    ExRec = Mapper.Map<JobReceiveQAHeader>(temp);


                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted)
                        temp.Status = (int)StatusConstants.Modified;


                    _JobReceiveQAAttributeService.Update(vm, User.Identity.Name);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobReceiveQADocEvents.onHeaderSaveEvent(this, new JobEventArgs(temp.JobReceiveQAHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        EventException = true;
                    }

                    try
                    {
                        if (EventException)
                        { throw new Exception(); }
                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(temp.DocTypeId);
                        ViewBag.Mode = "Edit";
                        return View("Create", vm);
                    }

                    try
                    {
                        JobReceiveQADocEvents.afterHeaderSaveEvent(this, new JobEventArgs(temp.JobReceiveQAHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobReceiveQAHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        xEModifications = Modifications,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("Index", new { id = temp.DocTypeId }).Success("Data saved successfully");

                }
                #endregion

            }
            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        public ActionResult Edit(int id)
        {
            JobReceiveQAAttributeViewModel pt = _JobReceiveQAAttributeService.GetJobReceiveQAAttributeDetailForEdit(id);

            if (pt == null)
            {
                return HttpNotFound();
            }


            var temp = _JobReceiveQAAttributeService.GetJobReceiveQAAttributeForEdit(id);
            pt.QAGroupLine = temp;

            //Getting Settings
            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveQASettings", new { id = pt.DocTypeId }).Warning("Please create job Inspection settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(settings);

            pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

            PrepareViewBag(pt.DocTypeId);

            ViewBag.Mode = "Edit";
            if ((System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.JobReceiveQAHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", pt);
        }


        public JsonResult GetUnitConversionMultiplier(int ProductId, Decimal Length, Decimal Width, Decimal? Height, string ToUnitId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);

            Decimal UnitConversionMultiplier = 0;
            UnitConversionMultiplier = new ProductService(_unitOfWork).GetUnitConversionMultiplier(1, product.UnitId, Length, Width, Height, ToUnitId,db);

            return Json(UnitConversionMultiplier);
        }

        public JsonResult getunitconversiondetailjson(int productid, string unitid, string deliveryunitid, int JobReceiveLineId)
        {
            var temp = (from L in db.JobReceiveLine
                        where L.JobReceiveLineId == JobReceiveLineId
                        select new
                        {
                            UnitConversionForId = L.JobOrderLine.JobOrderHeader.UnitConversionForId
                        }).FirstOrDefault();

            //UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(productid, unitid, deliveryunitid);
            UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(productid, unitid, (int)temp.UnitConversionForId, deliveryunitid);
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

        //public JsonResult GetProductDetailJson(int ProductId, string DealUnitId)
        //{
        //    SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
        //    SqlParameter SqlParameterDealUnitId = new SqlParameter("@DealUnitId", DealUnitId);

        //    ProductDimensions ProductDimensions = db.Database.SqlQuery<ProductDimensions>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetProductDimensions @ProductId, @DealUnitId", SqlParameterProductId, SqlParameterDealUnitId).FirstOrDefault();

        //    return Json(ProductDimensions);
        //}

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

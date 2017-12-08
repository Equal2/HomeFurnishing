using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using Presentation.ViewModels;
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    // [Authorize]
    public class SizeController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();
        List<string> UserRoles = new List<string>();

        ISizeService _SizeService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public SizeController(ISizeService SizeService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SizeService = SizeService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        // GET: /ProductMaster/
        // 
        public ActionResult Index()
        {
            var Size = _SizeService.GetSizeList();
            return View(Size);
        }

        public void PrepareViewBag()
        {
            ViewBag.ProductShapeList = new ProductShapeService(_unitOfWork).GetProductShapeList().ToList();
            ViewBag.UnitList = new UnitService(_unitOfWork).GetUnitListWithFractions().ToList();
            ViewBag.DocTypeList = (from Dt in db.DocumentType
                                   where Dt.DocumentCategory.DocumentCategoryName == TransactionDocCategoryConstants.Size
                                   select Dt).ToList();
        }

        // GET: /ProductMaster/Create
        //
        [HttpGet]
        public ActionResult Create()
        {
            PrepareViewBag();
            Size pt = new Size();
            pt.IsActive = true;

            var temp = (from U in db.Size
                        group new { U } by new { U.UnitId } into Result
                        select new
                        {
                            UnitId = Result.Key.UnitId,
                            Count = Result.Count()
                        });

            var MaxSize = (from L in temp
                         orderby L.Count descending
                         select new
                         {
                             UnitId = L.UnitId
                         }).FirstOrDefault();

            if (MaxSize != null)
            {
                pt.UnitId = MaxSize.UnitId;
            }

            var ProductShape = new ProductShapeService(_unitOfWork).Find("Rectangle");

            if (ProductShape != null)
            {
                pt.ProductShapeId = ProductShape.ProductShapeId;
            }




            ViewBag.Mode = "Add";
            return View("Create", pt);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(Size ptt)
        {
            Size pt = ptt;

            if (ptt.Area == 0)
            {
                PrepareViewBag();
                string message = "Area field is required";
                ModelState.AddModelError("", message);
                ViewBag.Mode = "Add";
                return View("Create", ptt);
            }

            if (ptt.Perimeter == 0)
            {
                PrepareViewBag();
                string message = "Perimeter field is required";
                ModelState.AddModelError("", message);
                ViewBag.Mode = "Add";
                return View("Create", ptt);
            }

            if (ModelState.IsValid)
            {

                if (ptt.SizeId <= 0)
                {
                    ptt.CreatedDate = DateTime.Now;
                    ptt.ModifiedDate = DateTime.Now;
                    ptt.CreatedBy = User.Identity.Name;
                    ptt.ModifiedBy = User.Identity.Name;
                    ptt.ObjectState = Model.ObjectState.Added;
                    var createdSize = _SizeService.Add(ptt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        PrepareViewBag();
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        ViewBag.Mode = "Add";
                        return View("Create", ptt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Size).DocumentTypeId,
                        DocId = pt.SizeId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));


                    return RedirectToAction("Create").Success("Data saved successfully");
                }

                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    Size temp = _SizeService.Find(pt.SizeId);

                    Size ExRec = Mapper.Map<Size>(temp);

                    temp.SizeName = pt.SizeName;
                    temp.ProductShapeId = pt.ProductShapeId;
                    temp.UnitId = pt.UnitId;
                    temp.DocTypeId = pt.DocTypeId;
                    temp.Length = pt.Length;
                    temp.LengthFraction = pt.LengthFraction;
                    temp.Width = pt.Width;
                    temp.WidthFraction = pt.WidthFraction;
                    temp.Height = pt.Height;
                    temp.HeightFraction = pt.HeightFraction;
                    temp.Area = pt.Area;
                    temp.Perimeter = pt.Perimeter;
                    temp.IsActive = pt.IsActive;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _SizeService.Update(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
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
                        ViewBag.Mode = "Edit";
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Size).DocumentTypeId,
                        DocId = temp.SizeId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index").Success("Data saved successfully");
                }
            }
            PrepareViewBag();
            ViewBag.Mode = "Add";
            return View(ptt);
        }


        public ActionResult Edit(int id)
        {
            PrepareViewBag();
            Size pt = _SizeService.Find(id);

            bool SizeExists = _SizeService.CheckSizeExists(id);
            if (SizeExists)
            {
                TempData["CSEXC"] += "Product is already created.";
            }
            
            if(!SizeExists)
            {
                ViewBag.Mode = "Edit";
            }

            if (pt == null)
            {
                return HttpNotFound();
            }
            return View("Create", pt);
        }



        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Size Size = _SizeService.Find(id);
            if (Size == null)
            {
                return HttpNotFound();
            }

            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            {
                var temp = _SizeService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                // Now delete the Parent Size
                _SizeService.Delete(temp.SizeId);

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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Size).DocumentTypeId,
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
            var nextId = _SizeService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _SizeService.PrevId(id);
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

        [HttpGet]
        public ActionResult Report()
        {

            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Size);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        public JsonResult GetUnitFraction(string id)
        {
            if (!string.IsNullOrEmpty(id))
                return Json(new UnitService(_unitOfWork).Find(id).FractionUnits);
            else
                return Json(0);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Presentation.Helper;
using Services.Customize;
using Components.ExceptionHandlers;
using Models.Customize.ViewModels;
using Models.Customize.Models;
using TimePlanValidator;
using TimePlanValidator.ViewModels;
using Models.BasicSetup.ViewModels;
using CookieNotifier;

namespace Customize.Controllers
{

    [Authorize]
    public class RecipeLineController : System.Web.Mvc.Controller
    {
        List<string> UserRoles = new List<string>();

        IRecipeLineService _RecipeLineService;
        IRecipeHeaderService _RecipeHeaderService;
        IJobOrderSettingsService _jobOrderSettingsService;
        IExceptionHandler _exception;
        IDocumentValidation _validator;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public RecipeLineController(IRecipeLineService RecipeLineService, IExceptionHandler exec, IRecipeHeaderService RecipeHeaderService, IJobOrderSettingsService jobOrderSettingsServ
            , IDocumentValidation validator)
        {
            _RecipeLineService = RecipeLineService;
            _RecipeHeaderService = RecipeHeaderService;
            _jobOrderSettingsService = jobOrderSettingsServ;
            _exception = exec;
            _validator = validator;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
        }



        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _RecipeLineService.GetStockLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }


        private void PrepareViewBag(RecipeLineViewModel vm)
        {
            if (vm != null)
            {
                RecipeHeaderViewModel H = _RecipeHeaderService.GetRecipeHeader(vm.JobOrderHeaderId);
                ViewBag.DocNo = H.DocTypeName + "-" + H.DocNo;
            }
        }

        [HttpGet]
        public ActionResult CreateLine(int id, bool? IsProdBased)
        {
            return _Create(id, null, IsProdBased);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, bool? IsProdBased)
        {
            return _Create(id, null, IsProdBased);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int id, bool? IsProdBased)
        {
            return _Create(id, null, IsProdBased);
        }

        public ActionResult _Create(int Id, DateTime? date, bool? IsProdBased) //Id ==>Sale Order Header Id
        {
            RecipeHeaderViewModel H = _RecipeHeaderService.GetRecipeHeader(Id);
            RecipeLineViewModel s = new RecipeLineViewModel();

            s.JobOrderHeaderId = H.JobOrderHeaderId;
            s.StockHeaderId = (int)H.StockHeaderId;
            s.HeaderTestingQty = H.TestingQty;
            s.HeaderQty = H.Qty;
            ViewBag.Status = H.Status;

            LastValues LastValues = _RecipeLineService.GetLastValues(Id);

            if (LastValues != null)
            {
                if (LastValues.DyeingRatio != null)
                {
                    s.DyeingRatio = LastValues.DyeingRatio;
                }
                else
                {
                    s.DyeingRatio = 100;
                }
            }


            PrepareViewBag(s);
            ViewBag.LineMode = "Create";

            return PartialView("_Create", s);
        }






        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult _CreatePost(RecipeLineViewModel svm, string BulkMode)
        {

            bool BeforeSave = true;
            JobOrderHeader temp = _RecipeHeaderService.Find(svm.JobOrderHeaderId);


            if (svm.Qty <= 0)
                ModelState.AddModelError("Qty", "The Qty is required");

            if (_RecipeLineService.IsDuplicateLine(svm.StockHeaderId,svm.ProductId, svm.DyeingRatio, svm.StockLineId))
                ModelState.AddModelError("ProductId", "Product is already entered in recipe.");

            ViewBag.Status = temp.Status;


            if (svm.StockLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (ModelState.IsValid && BeforeSave)
            {

                if (svm.StockLineId <= 0)
                {
                    try
                    {
                        _RecipeLineService.Create(svm, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(svm);

                        return PartialView("_Create", svm);
                    }

                    return RedirectToAction("_Create", new { id = svm.JobOrderHeaderId });

                }


                else
                {

                    try
                    {
                        _RecipeLineService.Update(svm, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        PrepareViewBag(svm);
                        return PartialView("_Create", svm);
                    }

                    return Json(new { success = true });
                }

            }
            PrepareViewBag(svm);
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
        public ActionResult _ModifyLineAfterApprove(int id)
        {
            return _Modify(id);
        }

        private ActionResult _Modify(int id)
        {
            RecipeLineViewModel temp = _RecipeLineService.GetStockLine(id);

            RecipeHeaderViewModel H = _RecipeHeaderService.GetRecipeHeader(temp.JobOrderHeaderId);

            temp.HeaderTestingQty = H.TestingQty;
            temp.HeaderQty = H.Qty;

            //Getting Settings

            if (temp == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag(temp);

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = _validator.ValidateDocumentLine(new DocumentUniqueId { LockReason = temp.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

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
        public ActionResult _DeleteLine_AfterApprove(int id)
        {
            return _Delete(id);
        }

        private ActionResult _Delete(int id)
        {
            RecipeLineViewModel temp = _RecipeLineService.GetStockLine(id);

            JobOrderHeader H = _RecipeHeaderService.Find(temp.JobOrderHeaderId);

            //Getting Settings

            if (temp == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag(temp);
            //ViewBag.LineMode = "Delete";

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = _validator.ValidateDocumentLine(new DocumentUniqueId { LockReason = temp.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

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
        //[ValidateAntiForgeryToken]
        public ActionResult DeletePost(RecipeLineViewModel vm)
        {
            bool BeforeSave = true;

            if (BeforeSave)
            {
                try
                {
                    _RecipeLineService.Delete(vm, User.Identity.Name);
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    PrepareViewBag(vm);
                    ViewBag.LineMode = "Delete";
                    return PartialView("_Create", vm);

                }
            }

            return Json(new { success = true });

        }


        public JsonResult GetProductDetailJson(int ProductId)
        {
            ProductViewModel product = _RecipeLineService.GetProduct(ProductId);

            return Json(new
            {
                ProductId = product.ProductId,
                UnitId = product.UnitId,
                Specification = product.ProductSpecification,
                StandardCost = product.StandardCost
            });
        }


        public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        {
            return Json(_RecipeLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
            {
                GenerateCookie.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);
                TempData.Remove("CSEXC");
            }
            if (disposing)
            {
                _RecipeLineService.Dispose();
            }
            base.Dispose(disposing);
        }


        public ActionResult CopyRecipes(int JobOrderHeaderId, int Dimension1Id, Decimal HeaderTestingQty, Decimal HeaderQty)
        {
            ViewBag.JobOrderHeaderId = JobOrderHeaderId;
            ViewBag.Dimension1Id = Dimension1Id;
            ViewBag.HeaderTestingQty = HeaderTestingQty;
            ViewBag.HeaderQty = HeaderQty;
            return View("_CopyRecipe");
        }
        public ActionResult RecipeDetail(int JobOrderHeaderId, int CopyFromRecipeId, Decimal HeaderTestingQty, Decimal HeaderQty)
        {
            ViewBag.JobOrderHeaderId = JobOrderHeaderId;
            ViewBag.CopyFromRecipeId = CopyFromRecipeId;
            ViewBag.HeaderTestingQty = HeaderTestingQty;
            ViewBag.HeaderQty = HeaderQty;
            return View("_RecipeDetail");
        }

        public JsonResult GetRecipes(int Dimension1Id)
        {
            return Json(new { data = _RecipeLineService.GetRecipes(Dimension1Id) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetRecipeDetail(int CopyFromRecipeId)
        {
            return Json(new { data = _RecipeLineService.GetRecipeDetail(CopyFromRecipeId) }, JsonRequestBehavior.AllowGet);
        }

        public void _ResultsPost(int JobOrderHeaderId, int StockLineId, int ProductId, Decimal DyeingRatio, Decimal TestingQty, Decimal DocQty, Decimal? ExcessQty, Decimal Qty, Decimal Rate, Decimal Amount)
        {
            RecipeLineViewModel svm = new RecipeLineViewModel();
            JobOrderHeader JobOrderHeader = _RecipeHeaderService.Find(JobOrderHeaderId);
            RecipeLineViewModel RecipeLine = _RecipeLineService.GetStockLine(StockLineId);



            svm.JobOrderHeaderId = JobOrderHeaderId;
            svm.StockHeaderId = (int)JobOrderHeader.StockHeaderId;
            svm.ProductId = ProductId;
            svm.DyeingRatio = DyeingRatio;
            svm.TestingQty = TestingQty;
            svm.DocQty = DocQty;
            svm.ExcessQty = ExcessQty;
            svm.Qty = Qty;
            svm.Rate = Rate;
            svm.Amount = Amount;

            try
            {
                if (JobOrderHeader.StockHeaderId == RecipeLine.StockHeaderId)
                {
                    svm.StockLineId = RecipeLine.StockLineId;
                    _RecipeLineService.Update(svm, User.Identity.Name);
                }
                else
                {
                    _RecipeLineService.Create(svm, User.Identity.Name);
                }
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                PrepareViewBag(svm);
            }
        }

        public JsonResult GetExcessStock(int ProductId, int? Dim1, int? Dim2, int? ProcId, string Lot, int StockHeaderId, string ProcName)
        {
            return Json(_RecipeLineService.GetExcessStock(ProductId, Dim1, Dim2, ProcId, Lot, StockHeaderId, ProcName), JsonRequestBehavior.AllowGet);
        }
    }
}

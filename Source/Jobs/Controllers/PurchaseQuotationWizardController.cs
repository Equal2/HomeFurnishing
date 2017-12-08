using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModel;
using Jobs.Helpers;


namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseQuotationWizardController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();


        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        public PurchaseQuotationWizardController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult Wizard()
        {
            ViewBag.ShipMethodList = new ShipMethodService(_unitOfWork).GetShipMethodList().ToList();
            ViewBag.DeliveryTermsList = new DeliveryTermsService(_unitOfWork).GetDeliveryTermsList().ToList();
            ViewBag.CurrencyList = new CurrencyService(_unitOfWork).GetCurrencyList().ToList();
            ViewBag.SalesTaxGroupList = new ChargeGroupPersonService(_unitOfWork).GetChargeGroupPersonList((int)(TaxTypeConstants.SalesTax)).ToList();
            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();
            return View();

        }

        public JsonResult ProductIndex(int start, int count, string search)
        {
            var Query = (from p in db.Product
                         where p.ProductGroup.ProductTypeId == 1
                         orderby p.ProductName
                         select new item
                         {
                             id = p.ProductId,
                             title = p.ProductName,
                             description = p.ProductName,
                             url = "https://192.168.2.110:44309/Uploads/" + p.ImageFolderName + "/Thumbs/" + p.ImageFileName
                         });

            if (!string.IsNullOrEmpty(search))
                Query = Query.Where(m => m.title.ToLower().Contains(search.ToLower()));

            var ProductList = (Query).Skip(start).Take(count).ToList();

            return new JsonpResult
            {
                Data = new { hits = Query.Count(), request = new request { start = start }, results = ProductList },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }


        public JsonResult GetProductSummary(int[] ProductId)
        {
            var Query = (from p in db.Product
                         where p.ProductGroup.ProductTypeId == 1 && ProductId.Contains(p.ProductId)
                         orderby p.ProductName
                         select new item
                         {
                             id = p.ProductId,
                             title = p.ProductName,
                             description = p.ProductName,
                             url = "https://192.168.2.110:44309/Uploads/" + p.ImageFolderName + "/Thumbs/" + p.ImageFileName
                         }).ToList();

            var ProductList = (Query).Select((p, index) => new item
            {
                description = p.description,
                id = p.id,
                Sr = index + 1,
                title = p.title,
                url = p.url,
                Qty = 1,
            }).ToList();
         
            return Json(ProductList);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult WizardPost(PurchaseQuotationHeaderViewModel Header, List<item> Lines)
        {

            if (ModelState.IsValid)
            {
                PurchaseQuotationHeader Head = new PurchaseQuotationHeader();
                Head.CreatedBy = User.Identity.Name;
                Head.DocTypeId = 281;
                Head.CreatedDate = DateTime.Now;
                Head.CurrencyId = Header.CurrencyId;
                Head.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                Head.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                Head.DocDate = Header.DocDate;
                Head.DocNo = Header.DocNo;
                Head.ModifiedBy = User.Identity.Name;
                Head.ModifiedDate = DateTime.Now;
                Head.Remark = Header.Remark;
                Head.SupplierId = Header.SupplierId;
                Head.TermsAndConditions = Header.TermsAndConditions;
                Head.VendorQuotationDate = Header.VendorQuotationDate;
                Head.VendorQuotationNo = Header.VendorQuotationNo;

                Head.ObjectState = Model.ObjectState.Added;
                db.PurchaseQuotationHeader.Add(Head);

                foreach (var item in Lines)
                {
                    PurchaseQuotationLine Line = new PurchaseQuotationLine();
                    Line.CreatedBy = User.Identity.Name;
                    Line.CreatedDate = DateTime.Now;
                    Line.ModifiedBy = User.Identity.Name;
                    Line.ModifiedDate = DateTime.Now;
                    Line.ProductId = item.id;
                    Line.PurchaseQuotationHeaderId = Head.PurchaseQuotationHeaderId;
                    Line.Qty = item.Qty;
                    Line.ObjectState = Model.ObjectState.Added;

                    db.PurchaseQuotationLine.Add(Line);
                }

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Json(new { Success = false, Message = _exception.HandleException(ex) });
                }

                return Json(new { Success = true, url = "/PurchaseQuotationHeader/Index" });
            }

            return Json(new { Success = false, Message = "Error in saving record. Cannot continue." });

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

        [Serializable]
        public class request
        {
            public int start { get; set; }
        }
        [Serializable]
        public class result
        {
            public List<item> item { get; set; }
        }
        [Serializable]
        public class item
        {
            public int id { get; set; }
            public int Sr { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public decimal Qty { get; set; }
        }
    }


}

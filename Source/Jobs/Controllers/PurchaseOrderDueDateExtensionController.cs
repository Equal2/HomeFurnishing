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
using AutoMapper;
using System.Xml.Linq;


namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseOrderDueDateExtensionController : Controller
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


        public PurchaseOrderDueDateExtensionController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
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

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Index1()
        {
            return View();
        }

        public JsonResult PendingPurchaseOrderIndex(int start, int count, string search)
        {
            var Query = (from p in db.ViewPurchaseOrderBalance
                         join t in db.PurchaseOrderHeader on p.PurchaseOrderHeaderId equals t.PurchaseOrderHeaderId
                         join t2 in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t2.PurchaseOrderLineId
                         join t3 in db.Dimension1 on t2.Dimension1Id equals t3.Dimension1Id into t3table
                         from t3tab in t3table.DefaultIfEmpty()
                         where p.BalanceQty > 0
                         group new { t, t2, t3tab } by t.PurchaseOrderHeaderId into g
                         orderby g.Max(m => m.t.DocNo)
                         select new
                         {
                             DocNo = g.Max(m => m.t.DocNo),
                             Dimension1 = g.Max(m => m.t3tab.Dimension1Name),
                             DocDate = g.Max(m => m.t.DocDate),
                             DueDate = g.Max(m => m.t.DueDate),
                             Priority = g.Max(m => m.t.Priority),
                             PurchaseOrderId = g.Key,
                         });

            if (!string.IsNullOrEmpty(search))
                Query = Query.Where(m => m.DocNo.ToLower().Contains(search.ToLower()));

            var ProductList = (Query).Skip(start).Take(count).ToList().Select(m => new PurchaseOrderGridDetails { PurchaseOrderHeadId = m.PurchaseOrderId, DocNo = m.DocNo, Dimension1 = m.Dimension1, DocDate = m.DocDate.ToString("dd/MMM/yyyy"), DueDate = m.DueDate.ToString("dd/MMM/yyyy"), Priority = Enum.GetName(typeof(SaleOrderPriority), m.Priority) });

            return new JsonpResult
            {
                Data = new { hits = Query.Count(), request = new request { start = start }, results = ProductList },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult DueDateUpdate(int Id, DateTime NewDueDate)
        {

            if (ModelState.IsValid)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                PurchaseOrderHeader Head = db.PurchaseOrderHeader.Find(Id);

                PurchaseOrderHeader ExRec = Mapper.Map<PurchaseOrderHeader>(Head);

                Head.DueDate = NewDueDate;
                Head.ModifiedBy = User.Identity.Name;
                Head.ModifiedDate = DateTime.Now;

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                    Obj = Head,
                });

                Head.ObjectState = Model.ObjectState.Modified;
                db.PurchaseOrderHeader.Add(Head);

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Json(new { Success = false });
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Head.DocTypeId,
                    DocId = Head.PurchaseOrderHeaderId,
                    ActivityType = (int)ActivityTypeContants.Modified,
                    DocNo = Head.DocNo,
                    DocDate = Head.DocDate,
                    DocStatus = Head.Status,
                    xEModifications = Modifications,
                }));

                return Json(new { Success = true });
            }

            return Json(new { Success = false });

        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult PriorityUpdate(int Id, string Priority)
        {

            if (ModelState.IsValid)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                PurchaseOrderHeader Head = db.PurchaseOrderHeader.Find(Id);

                PurchaseOrderHeader ExRec = Mapper.Map<PurchaseOrderHeader>(Head);

                int t = (int)Enum.Parse(typeof(SaleOrderPriority), Priority);
                Head.Priority = t;
                Head.ModifiedBy = User.Identity.Name;
                Head.ModifiedDate = DateTime.Now;

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                    Obj = Head,
                });

                Head.ObjectState = Model.ObjectState.Modified;
                db.PurchaseOrderHeader.Add(Head);

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Json(new { Success = false });
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Head.DocTypeId,
                    DocId = Head.PurchaseOrderHeaderId,
                    ActivityType = (int)ActivityTypeContants.Modified,
                    DocNo = Head.DocNo,
                    DocDate = Head.DocDate,
                    DocStatus = Head.Status,
                    xEModifications = Modifications,
                }));

                return Json(new { Success = true });
            }

            return Json(new { Success = false });

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
        class request
        {
            public int start { get; set; }
        }
        [Serializable]
        class result
        {
            public List<PurchaseOrderGridDetails> item { get; set; }
        }
        [Serializable]
        class PurchaseOrderGridDetails
        {
            public int PurchaseOrderHeadId { get; set; }
            public string DocNo { get; set; }
            public string ProdOrderNo { get; set; }
            public string Dimension1 { get; set; }
            public string DocDate { get; set; }
            public string DueDate { get; set; }
            public DateTime? NewDueDate { get; set; }
            public string Priority { get; set; }
        }
    }
}

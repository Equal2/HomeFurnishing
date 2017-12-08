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
    public class JobOrderDueDateExtensionController : Controller
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


        public JobOrderDueDateExtensionController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
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

        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("Index", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        public void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
        }

        public ActionResult Index(int Id)
        {
            PrepareViewBag(Id);
            return View();
        }

        public JsonResult PendingJobOrderIndex(int id, int start, int count, string search)
        {
            var Query = (from p in db.ViewJobOrderBalance
                         join t in db.JobOrderHeader on p.JobOrderHeaderId equals t.JobOrderHeaderId
                         join t2 in db.JobOrderLine on p.JobOrderLineId equals t2.JobOrderLineId
                         join t3 in db.Dimension1 on t2.Dimension1Id equals t3.Dimension1Id into t3table
                         from t3tab in t3table.DefaultIfEmpty()
                         where p.BalanceQty > 0 && t.DocTypeId == id
                         group new { t, t2, t3tab } by t.JobOrderHeaderId into g
                         orderby g.Max(m => m.t.DocNo)
                         select new
                         {
                             DocNo = g.Max(m => m.t.DocNo),
                             Dimension1 = g.Max(m => m.t3tab.Dimension1Name),
                             DocDate = g.Max(m => m.t.DocDate),
                             DueDate = g.Max(m => m.t.DueDate),
                             Priority = g.Max(m => m.t.Priority),
                             JobOrderId = g.Key,
                         });

            if (!string.IsNullOrEmpty(search))
                Query = Query.Where(m => m.DocNo.ToLower().Contains(search.ToLower()));

            var ProductList = (Query).Skip(start).Take(count).ToList().Select(m => new JobOrderGridDetails { JobOrderHeadId = m.JobOrderId, DocNo = m.DocNo, Dimension1 = m.Dimension1, DocDate = m.DocDate.ToString("dd/MMM/yyyy"), DueDate = m.DueDate.ToString("dd/MMM/yyyy"), Priority = Enum.GetName(typeof(SaleOrderPriority), m.Priority) });

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

                JobOrderHeader Head = db.JobOrderHeader.Find(Id);

                JobOrderHeader ExRec = Mapper.Map<JobOrderHeader>(Head);

                Head.DueDate = NewDueDate;
                Head.ModifiedBy = User.Identity.Name;
                Head.ModifiedDate = DateTime.Now;

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                    Obj = Head,
                });

                Head.ObjectState = Model.ObjectState.Modified;
                db.JobOrderHeader.Add(Head);

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
                    DocId = Head.JobOrderHeaderId,
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

                JobOrderHeader Head = db.JobOrderHeader.Find(Id);

                JobOrderHeader ExRec = Mapper.Map<JobOrderHeader>(Head);
                //Head.DueDate = NewDueDate;
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
                db.JobOrderHeader.Add(Head);

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
                    DocId = Head.JobOrderHeaderId,
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
            public List<JobOrderGridDetails> item { get; set; }
        }
        [Serializable]
        class JobOrderGridDetails
        {
            public int JobOrderHeadId { get; set; }
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

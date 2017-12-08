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
using System.Configuration;
using Jobs.Helpers;
using System.Xml.Linq;
//using PurchaseOrderDocumentEvents;
using CustomEventArgs;
//using DocumentEvents;
using Reports.Reports;
using Model.ViewModels;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class GatePassController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();


        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IGatePassHeaderService _GatePassService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        public GatePassController(IGatePassHeaderService GatePassService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _GatePassService = GatePassService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            /*  if (!PurchaseOrderEvents.Initialized)
              {
                  PurchaseOrderEvents Obj = new PurchaseOrderEvents();
              }
              */
            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }






        public ActionResult Index()//DocumentTypeId

        {
            int GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];
            //   User.Identity.Name
            ViewBag.IndexStatus = "All";
            var GatePaassHeader = _GatePassService.GetPendingGatePassList(GodownId);

            // var g = GatePaassHeader.ToList();

            //var Query2 = (
            //             from G in db.GatePassHeader
            //             join L in db.GatePassLine on G.GatePassHeaderId equals L.GatePassHeaderId
            //             join P in db.Persons on G.PersonId equals P.PersonID
            //             where G.GodownId== GodownId
            //             select new
            //             {
            //                 GatePassHeaderId = G.GatePassHeaderId,
            //                 DocNo = G.DocNo,
            //                 DocDate = G.DocDate,
            //                 Status = G.Status,
            //                 Name = P.Name,
            //                 Product = L.Product,
            //                 Qty = L.Qty,
            //                 UnitId = L.UnitId
            //             });

            //var Query3 = (from a in Query2.ToList()
            //              group a by a.GatePassHeaderId into g
            //              select new GatePassHeader
            //              {
            //                  GatePassHeaderId = g.Key,
            //                  DocNo = g.Max(x => x.DocNo),
            //                  DocDate = g.Max(x => x.DocDate),
            //                  Status = g.Max(x => x.Status),
            //                  Remark = string.Join(",", g.Select(x => x.Product + " ( " + x.Qty + x.UnitId + " ) "))
            //              });

            return View(GatePaassHeader);
        }

        public ActionResult Index1()//DocumentTypeId

        {
            int GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];
            //   User.Identity.Name
            ViewBag.IndexStatus = "All";
            var GatePaassHeader = _GatePassService.GetPendingGatePassList(GodownId);
            return View(GatePaassHeader);
        }

        public ActionResult HeaderPost(string Ids)
        {
            string[] subStr = Ids.Split(',');
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                GatePassHeader ah = _GatePassService.Find(temp);
                ah.Status = 1;
                ah.ObjectState = Model.ObjectState.Modified;
                db.GatePassHeader.Add(ah);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);

                }

            }
            //  return Json(new { success = true });

            return RedirectToAction("Index1", "GatePass");


        }



        public JsonResult PendingGatePassHeaders()//DocTypeId
        {
            int GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];
            ViewBag.IndexStatus = "All";
            //var temp = _GatePassService.GetPendingGatePassList(GodownId).ToList();
            //return Json(new { data = temp }, JsonRequestBehavior.AllowGet);




            var Query2 = (
                         from G in db.GatePassHeader
                         join L in db.GatePassLine on G.GatePassHeaderId equals L.GatePassHeaderId
                         join P in db.Persons on G.PersonId equals P.PersonID
                         where G.GodownId == GodownId
                         select new
                         {
                             GatePassHeaderId = G.GatePassHeaderId,
                             DocNo = G.DocNo,
                             DocDate = G.DocDate,
                             Status = G.Status,
                             Name = P.Name,
                             Product = L.Product,
                             Qty = L.Qty,
                             UnitId = L.UnitId
                         });

            var Query3 = (from a in Query2.ToList()
                          group a by a.GatePassHeaderId into g
                          select new GatePassHeaderViewModel1
                          {
                              GatePassHeaderId = g.Key,
                              DocNo = g.Max(x => x.DocNo),
                              Name = g.Max(x => x.Name),
                              Remark = string.Join(",", g.Select(x => x.Product + " ( " + x.Qty + x.UnitId + " ) "))
                          });

            var temp = Query3.ToList();
            return Json(new { data = temp }, JsonRequestBehavior.AllowGet);



        }




        public ActionResult LineDetail(int Ids)
        {
            var Query2 = (
                       from G in db.GatePassHeader
                       join L in db.GatePassLine on G.GatePassHeaderId equals L.GatePassHeaderId
                       join P in db.Persons on G.PersonId equals P.PersonID
                       where G.GatePassHeaderId == Ids
                       select new GatePassHeaderViewModel1
                       {
                           GatePassHeaderId = G.GatePassHeaderId,
                           DocNo = G.DocNo,
                           DocDate = G.DocDate,
                           Status = G.Status,
                           Name = P.Name,
                           Product = L.Product,
                           Qty = L.Qty,
                           UnitId = L.UnitId
                       });



            return PartialView("_LineDetail", Query2.ToList());
        }



        // GET: /PurchaseOrderHeaderMaster/Create





        /* public ActionResult GeneratePrints(string Ids, int DocTypeId)
         {

             if (!string.IsNullOrEmpty(Ids))
             {
                 int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                 int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                 var Settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocTypeId, DivisionId, SiteId);

                 try
                 {

                     List<byte[]> PdfStream = new List<byte[]>();
                     foreach (var item in Ids.Split(',').Select(Int32.Parse))
                     {

                         DirectReportPrint drp = new DirectReportPrint();

                         var pd = db.PurchaseOrderHeader.Find(item);

                         LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                         {
                             DocTypeId = pd.DocTypeId,
                             DocId = pd.PurchaseOrderHeaderId,
                             ActivityType = (int)ActivityTypeContants.Print,
                             DocNo = pd.DocNo,
                             DocDate = pd.DocDate,
                             DocStatus = pd.Status,
                         }));

                         byte[] Pdf;

                         if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Import || pd.Status == (int)StatusConstants.Modified)
                         {
                             //LogAct(item.ToString());
                             Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);

                             PdfStream.Add(Pdf);
                         }
                         else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
                         {
                             Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterSubmit, User.Identity.Name, item);

                             PdfStream.Add(Pdf);
                         }
                         else
                         {
                             Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterApprove, User.Identity.Name, item);
                             PdfStream.Add(Pdf);
                         }

                     }

                     PdfMerger pm = new PdfMerger();

                     byte[] Merge = pm.MergeFiles(PdfStream);

                     if (Merge != null)
                         return File(Merge, "application/pdf");

                 }

                 catch (Exception ex)
                 {
                     string message = _exception.HandleException(ex);
                     return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
                 }


                 return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet);

             }
             return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);

         }
         [HttpGet]
         public ActionResult Email()
         {
             //To Be Implemented
             return View("~/Views/Shared/UnderImplementation.cshtml");
         }
         */

        /*public ActionResult Action_OnSubmit(int Id, int DocTypeId)//DocId
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocTypeId, DivisionId, SiteId);

            if (settings != null)
            {
                if (settings.OnSubmitMenuId != null)
                {
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.OnSubmitMenuId);

                    if (menuviewmodel != null)
                    {
                        if (!string.IsNullOrEmpty(menuviewmodel.URL))
                        {
                            return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + Id);
                        }
                        else
                        {
                            return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { id = Id });
                        }
                    }
                }
            }
            return RedirectToAction("Index", new { id = DocTypeId });
        }
        */
        /*
        public ActionResult Action_OnApprove(int Id, int DocTypeId)//DocId
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocTypeId, DivisionId, SiteId);

            if (settings != null)
            {
                if (settings.OnApproveMenuId != null)
                {
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.OnApproveMenuId);

                    if (menuviewmodel != null)
                    {
                        if (!string.IsNullOrEmpty(menuviewmodel.URL))
                        {
                            return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + Id);
                        }
                        else
                        {
                            return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { id = Id });
                        }
                    }
                }
            }
            return RedirectToAction("Index", new { id = DocTypeId });
        }
        */
        /* private void NotifyUser(int Id, ActivityTypeContants ActivityType)
         {
             AttendanceHeader Header = new AttendanceHeaderService(_unitOfWork).Find(Id);
             PurchaseOrderSetting PurchaseOrderSettings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

             DocEmailContent DocEmailContentSettings = new DocEmailContentService(_unitOfWork).GetDocEmailContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
             DocNotificationContent DocNotificationContentSettings = new DocNotificationContentService(_unitOfWork).GetDocNotificationContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
             DocSmsContent DocSmsContentSettings = new DocSmsContentService(_unitOfWork).GetDocSmsContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);

             new NotifyUserController(_unitOfWork).SendEmailMessage(Id, ActivityType, DocEmailContentSettings, PurchaseOrderSettings.SqlProcDocumentPrint);
             new NotifyUserController(_unitOfWork).SendNotificationMessage(Id, ActivityType, DocNotificationContentSettings, User.Identity.Name);
             new NotifyUserController(_unitOfWork).SendSmsMessage(Id, ActivityType, DocSmsContentSettings);

         }*/

        //private void SendEmailMessage(int Id, ActivityTypeContants ActivityType)
        //{
        //    PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);
        //    DocEmailContent DocEmailContentSettings = new DocEmailContentService(_unitOfWork).GetDocEmailContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
        //    PurchaseOrderSetting PurchaseOrderSettings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

        //    if (DocEmailContentSettings != null)
        //    {
        //        if (DocEmailContentSettings.ProcEmailContent != null && DocEmailContentSettings.ProcEmailContent != "")
        //        {
        //            SqlParameter SqlParameterId = new SqlParameter("@Id", Id);

        //            IEnumerable<EmailContentViewModel> MailContent = db.Database.SqlQuery<EmailContentViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + DocEmailContentSettings.ProcEmailContent + " @Id", SqlParameterId);

        //            foreach (EmailContentViewModel item in MailContent)
        //            {
        //                if (DocEmailContentSettings.AttachmentTypes != null && DocEmailContentSettings.AttachmentTypes != "")
        //                {
        //                    string[] AttachmentTypeArr = DocEmailContentSettings.AttachmentTypes.Split(',');

        //                    for (int i = 0; i <= AttachmentTypeArr.Length - 1; i++)
        //                    {
        //                        if (item.FileNameList != "" && item.FileNameList != null) { item.FileNameList = item.FileNameList + ","; }
        //                        if (AttachmentTypeArr[i].ToUpper() == "PDF")
        //                        {
        //                            item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(PurchaseOrderSettings.SqlProcDocumentPrint, Id.ToString(), ReportFileTypeConstants.PDF, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
        //                        }
        //                        else if (AttachmentTypeArr[i].ToUpper() == "EXCEL")
        //                        {
        //                            item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(PurchaseOrderSettings.SqlProcDocumentPrint, Id.ToString(), ReportFileTypeConstants.Excel, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
        //                        }
        //                        else if (AttachmentTypeArr[i].ToUpper() == "WORD")
        //                        {
        //                            item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(PurchaseOrderSettings.SqlProcDocumentPrint, Id.ToString(), ReportFileTypeConstants.Word, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
        //                        }
        //                        else
        //                        {
        //                            item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(PurchaseOrderSettings.SqlProcDocumentPrint, Id.ToString(), ReportFileTypeConstants.PDF, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
        //                        }
        //                    }
        //                    item.EmailBody = item.EmailBody.Replace("DomainName", (string)System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"]);
        //                }

        //                SendEmail.SendEmailMsg(item);
        //            }
        //        }
        //    }
        //}

        //private void SendNotificationMessage(int Id, ActivityTypeContants ActivityType)
        //{
        //    PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);
        //    DocNotificationContent DocNotificationContentSettings = new DocNotificationContentService(_unitOfWork).GetDocNotificationContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
        //    PurchaseOrderSetting PurchaseOrderSettings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

        //    if (DocNotificationContentSettings != null)
        //    {
        //        if (DocNotificationContentSettings.ProcNotificationContent != null && DocNotificationContentSettings.ProcNotificationContent != "")
        //        {
        //            SqlParameter SqlParameterId = new SqlParameter("@Id", Id);

        //            IEnumerable<NotificationContentViewModel> NotificationContent = db.Database.SqlQuery<NotificationContentViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + DocNotificationContentSettings.ProcNotificationContent + " @Id", SqlParameterId);

        //            foreach (NotificationContentViewModel item in NotificationContent)
        //            {
        //                Notification Note = new Notification();
        //                if (ActivityType == ActivityTypeContants.Submitted)
        //                {
        //                    Note.NotificationSubjectId = (int)NotificationSubjectConstants.PurchaseOrderSubmitted;
        //                }
        //                else
        //                {
        //                    Note.NotificationSubjectId = (int)NotificationSubjectConstants.PurchaseOrderApproved;
        //                }
        //                Note.NotificationText = item.NotificationText;
        //                Note.NotificationUrl = item.NotificationUrl;
        //                Note.UrlKey = item.UrlKey;
        //                Note.ExpiryDate = item.ExpiryDate;
        //                Note.IsActive = true;
        //                Note.CreatedBy = User.Identity.Name;
        //                Note.ModifiedBy = User.Identity.Name;
        //                Note.CreatedDate = DateTime.Now;
        //                Note.ModifiedDate = DateTime.Now;
        //                new NotificationService(_unitOfWork).Create(Note);

        //                string[] UserNameArr = item.UserNameList.Split(',');

        //                foreach (string UserName in UserNameArr)
        //                {
        //                    NotificationUser NoteUser = new NotificationUser();
        //                    NoteUser.NotificationId = Note.NotificationId;
        //                    NoteUser.UserName = UserName;
        //                    new NotificationUserService(_unitOfWork).Create(NoteUser);
        //                }
        //            }
        //        }
        //    }
        //}

        //private void SendSmsMessage(int Id, ActivityTypeContants ActivityType)
        //{
        //    PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);
        //    DocSmsContent DocSmsContentSettings = new DocSmsContentService(_unitOfWork).GetDocSmsContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
        //    PurchaseOrderSetting PurchaseOrderSettings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

        //    if (DocSmsContentSettings != null)
        //    {
        //        if (DocSmsContentSettings.ProcSmsContent != null && DocSmsContentSettings.ProcSmsContent != "")
        //        {
        //            SqlParameter SqlParameterId = new SqlParameter("@Id", Id);

        //            IEnumerable<SmsContentViewModel> SmsContent = db.Database.SqlQuery<SmsContentViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + DocSmsContentSettings.ProcSmsContent + " @Id", SqlParameterId);

        //            foreach (SmsContentViewModel item in SmsContent)
        //            {

        //            }
        //        }
        //    }
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

using System.Web.Mvc;
using Data.Models;
using Service;
using Model.ViewModel;
using System.Collections.Generic;
using Core.Common;
using Model.Models;
using System;
using System.Linq;
using System.IO;

namespace Jobs.Controllers
{
    public class DocumentAttachmentController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IExceptionHandlingService _exception;

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();
        public DocumentAttachmentController(IExceptionHandlingService exec)
        {
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        [HttpGet]
        public ActionResult AttachDocument(int DocId, int DocTypeId)
        {

            List<DocumentAttachmentViewModel> vm = new List<DocumentAttachmentViewModel>();

            var Attachments = (from p in db.DocumentAttachment.AsNoTracking()
                               where p.DocId == DocId && p.DocTypeId == p.DocTypeId
                               select p).ToList();

            if (Attachments.Count() > 0)
                foreach (var item in Attachments)
                {
                    DocumentAttachmentViewModel dm = new DocumentAttachmentViewModel();
                    dm.DocId = item.DocId;
                    dm.DocTypeId = item.DocTypeId;
                    dm.DocumentAttachmentId = item.DocumentAttachmentId;
                    dm.FileFolderName = item.FileFolderName;
                    dm.FileName = item.FileName;
                    dm.SetExtension();
                    vm.Add(dm);
                }
            else
                vm.Add(new DocumentAttachmentViewModel
                {
                    DocId = DocId,
                    DocTypeId = DocTypeId,
                });

            return PartialView("~/Views/Shared/_DocumentAttachment.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AttachDocument(DocumentAttachmentViewModel vm)
        {

            if (Request.Files[0] != null && Request.Files[0].ContentLength > 0)
            {
                UploadDocument ud = new UploadDocument();
                string FileNames = "";
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    string FileName = "";
                    string FolderName = "";

                    ud.UploadFile(i, new Dictionary<string, string>(), vm.DocTypeId + "_" + vm.DocId, FileTypeConstants.Other, out FolderName, out FileName);

                    DocumentAttachment da = new DocumentAttachment();

                    da.DocId = vm.DocId;
                    da.DocTypeId = vm.DocTypeId;
                    da.FileFolderName = FolderName;
                    da.FileName = FileName;
                    da.CreatedBy = User.Identity.Name;
                    da.CreatedDate = DateTime.Now;
                    da.ModifiedBy = User.Identity.Name;
                    da.ModifiedDate = DateTime.Now;
                    da.ObjectState = Model.ObjectState.Added;
                    db.DocumentAttachment.Add(da);

                    FileNames += da.FileName + ", ";

                }

                db.SaveChanges();

                //LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                //{
                //    DocTypeId = vm.DocTypeId,
                //    DocId = vm.DocId,
                //    ActivityType = (int)ActivityTypeContants.FileAdded,
                //    DocNo = FileNames,
                //}));

            }

            return RedirectToAction("AttachDocument", new { DocId = vm.DocId, DocTypeId = vm.DocTypeId });
        }


        public FileStreamResult Download(int id)
        {

            var DA = db.DocumentAttachment.Find(id);

            string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/" + DA.FileFolderName + "/";

            return File(new FileStream(path + DA.FileName, FileMode.Open), "text/plain", DA.FileName);
        }

        public ActionResult Delete(int id)
        {
            string Exception = "";
            var DA = db.DocumentAttachment.Find(id);

            if (DA != null)
            {
                try
                {
                    if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + DA.FileFolderName + "/" + DA.FileName)))
                    {
                        DA.ObjectState = Model.ObjectState.Deleted;
                        db.DocumentAttachment.Remove(DA);

                        db.SaveChanges();

                        //System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + DA.FileFolderName + "/" + DA.FileName));

                        //LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        //{
                        //    DocTypeId = DA.DocTypeId,
                        //    DocId = DA.DocId,
                        //    ActivityType = (int)ActivityTypeContants.FileRemoved,
                        //    Modifications = DA.FileFolderName + "/" + DA.FileName,
                        //    DocNo = DA.FileName,
                        //}));

                    }
                    else
                        Exception += "File not found";
                }
                catch (Exception ex)
                {
                    Exception += ex.Message;
                }
            }
            else
                Exception += "Record not found to delete.";


            if (string.IsNullOrEmpty(Exception))
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { success = false, message = Exception }, JsonRequestBehavior.AllowGet);
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

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using System.Data.SqlClient;
using Model.ViewModels;
using EmailContents;
using Mailer;
using System.Configuration;
//using Models.Login.ViewModels;
//using Models.Login.Models;

namespace Jobs.Controllers
{
    [Authorize]
    public class NotifyUserController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IUnitOfWork _unitOfWork;

        public NotifyUserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void SendEmailMessage(int Id, ActivityTypeContants ActivityType, DocEmailContent DocEmailContentSettings, string AttachmentProcedureName)
        {
            if (DocEmailContentSettings != null)
            {
                if (DocEmailContentSettings.ProcEmailContent != null && DocEmailContentSettings.ProcEmailContent != "")
                {
                    SqlParameter SqlParameterId = new SqlParameter("@Id", Id);

                    IEnumerable<EmailContentViewModel> MailContent = db.Database.SqlQuery<EmailContentViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + DocEmailContentSettings.ProcEmailContent + " @Id", SqlParameterId);

                    foreach (EmailContentViewModel item in MailContent)
                    {
                        if (DocEmailContentSettings.AttachmentTypes != null && DocEmailContentSettings.AttachmentTypes != "")
                        {
                            string[] AttachmentTypeArr = DocEmailContentSettings.AttachmentTypes.Split(',');

                            for (int i = 0; i <= AttachmentTypeArr.Length - 1; i++)
                            {
                                if (item.FileNameList != "" && item.FileNameList != null) { item.FileNameList = item.FileNameList + ","; }
                                if (AttachmentTypeArr[i].ToUpper() == "PDF")
                                {
                                    item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(AttachmentProcedureName, Id.ToString(), ReportFileTypeConstants.PDF, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
                                }
                                else if (AttachmentTypeArr[i].ToUpper() == "EXCEL")
                                {
                                    item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(AttachmentProcedureName, Id.ToString(), ReportFileTypeConstants.Excel, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
                                }
                                else if (AttachmentTypeArr[i].ToUpper() == "WORD")
                                {
                                    item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(AttachmentProcedureName, Id.ToString(), ReportFileTypeConstants.Word, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
                                }
                                else
                                {
                                    item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(AttachmentProcedureName, Id.ToString(), ReportFileTypeConstants.PDF, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
                                }
                            }
                            item.EmailBody = item.EmailBody.Replace("DomainName", (string)System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"]);
                        }

                        SendEmail.SendEmailMsg(item);
                    }
                }
            }
        }

        public void SendNotificationMessage(int Id, ActivityTypeContants ActivityType, DocNotificationContent DocNotificationContentSettings, string NotificationCreatedBy)
        {

            if (DocNotificationContentSettings != null)
            {
                if (DocNotificationContentSettings.ProcNotificationContent != null && DocNotificationContentSettings.ProcNotificationContent != "")
                {
                    SqlParameter SqlParameterId = new SqlParameter("@Id", Id);

                    IEnumerable<NotificationContentViewModel> NotificationContent = db.Database.SqlQuery<NotificationContentViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + DocNotificationContentSettings.ProcNotificationContent + " @Id", SqlParameterId);

                    foreach (NotificationContentViewModel item in NotificationContent)
                    {
                        Notification Note = new Notification();
                        Note.NotificationSubjectId = item.NotificationSubjectId;
                        Note.NotificationText = item.NotificationText;
                        Note.NotificationUrl = item.NotificationUrl;
                        Note.UrlKey = item.UrlKey;
                        Note.ExpiryDate = item.ExpiryDate;
                        Note.IsActive = true;
                        Note.CreatedBy = NotificationCreatedBy;
                        Note.ModifiedBy = NotificationCreatedBy;
                        Note.CreatedDate = DateTime.Now;
                        Note.ModifiedDate = DateTime.Now;
                        //new NotificationService(_unitOfWork).Create(Note);

                        string[] UserNameArr = item.UserNameList.Split(',');

                        foreach (string UserName in UserNameArr)
                        {
                            NotificationUser NoteUser = new NotificationUser();
                            NoteUser.NotificationId = Note.NotificationId;
                            NoteUser.UserName = UserName;
                            new NotificationUserService(_unitOfWork).Create(NoteUser);
                        }
                    }
                }
            }
        }

        public void SendSmsMessage(int Id, ActivityTypeContants ActivityType, DocSmsContent DocSmsContentSettings)
        {
            if (DocSmsContentSettings != null)
            {
                if (DocSmsContentSettings.ProcSmsContent != null && DocSmsContentSettings.ProcSmsContent != "")
                {
                    SqlParameter SqlParameterId = new SqlParameter("@Id", Id);

                    IEnumerable<SmsContentViewModel> SmsContent = db.Database.SqlQuery<SmsContentViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + DocSmsContentSettings.ProcSmsContent + " @Id", SqlParameterId);

                    foreach (SmsContentViewModel item in SmsContent)
                    {

                    }
                }
            }
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

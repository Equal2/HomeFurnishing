using System;
using System.Collections.Generic;
using System.Diagnostics;
using Service;
using Data.Models;
using System.Configuration;
using Mailer;
using Core.Common;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Mailer.Model;
using Model.Models;
using Data.Infrastructure;
using Service;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
//using Models.Login.Models;

namespace EmailContents
{
    public class NotificationContents
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;
        public NotificationContents(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void PurchaseOrder_OnSubmit(int Id)
        {
            PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);

            Notification Note = new Notification();
            Note.NotificationSubjectId = (int)NotificationSubjectConstants.PurchaseOrderSubmitted;
            Note.NotificationText = "Purchase Order " + Header.DocNo + " is submitted.";
            Note.NotificationUrl = "/PurchaseOrderHeader/Detail/" + Id.ToString() + "?transactionType=apptove";
            Note.UrlKey = (string)System.Configuration.ConfigurationManager.AppSettings["PurchaseDomain"];
            Note.ExpiryDate = DateTime.Now.AddHours(24);
            Note.IsActive = true;
            Note.CreatedBy = Header.CreatedBy;
            Note.ModifiedBy = Header.CreatedBy;
            Note.CreatedDate =  DateTime.Now;
            Note.ModifiedDate = DateTime.Now;
            new NotificationService(_unitOfWork).Create(Note);

            var RoleList = new string[] { "Purchase Manager (Finished Tufted)", "Purchase Manager (Finished Knotted)", "Purchase Manager (Finished Kelim),Purchase Manager (Raw)" };
            IEnumerable<IdentityUser> UserList = new UserService().GetUserList(RoleList);

            foreach (IdentityUser item in UserList)
            {
                NotificationUser NoteUser = new NotificationUser();
                NoteUser.NotificationId = Note.NotificationId;
                NoteUser.UserName = item.UserName;
                new NotificationUserService(_unitOfWork).Create(NoteUser);
            }
        }

        public void PurchaseOrder_OnApprove(int Id)
        {
            PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);

            Notification Note = new Notification();
            Note.NotificationSubjectId = (int)NotificationSubjectConstants.PurchaseOrderApproved;
            Note.NotificationText = "Purchase Order " + Header.DocNo + " is approved.";
            Note.NotificationUrl = "/PurchaseOrderHeader/Detail/" + Id.ToString() + "?transactionType=detail";
            Note.UrlKey = (string)System.Configuration.ConfigurationManager.AppSettings["PurchaseDomain"];
            Note.ExpiryDate = DateTime.Now.AddHours(24);
            Note.IsActive = true;
            Note.CreatedBy = Header.CreatedBy;
            Note.ModifiedBy = Header.CreatedBy;
            Note.CreatedDate = DateTime.Now;
            Note.ModifiedDate = DateTime.Now;

            new NotificationService(_unitOfWork).Create(Note);
        }
    }
}

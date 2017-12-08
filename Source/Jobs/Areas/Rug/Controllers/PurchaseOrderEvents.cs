using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data;
using Data.Models;
using Service;
using Core;
using Model.Models;
using System.Configuration;
using System.Text;
using Data.Infrastructure;
using Core.Common;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using EmailContents;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class PurchaseOrderEventsController : System.Web.Mvc.Controller
    {
        IUnitOfWork _unitOfWork;

        public PurchaseOrderEventsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public ActionResult PurchaseOrder_OnSubmit(int Id)
        {
            //NotificationContents _NotificationContents = new NotificationContents(_unitOfWork);
            //MailContents _MailContents = new MailContents(_unitOfWork);
            //_NotificationContents.PurchaseOrder_OnSubmit(Id);
            //_MailContents.PurchaseOrder_OnSubmit(Id);

            PurchaseOrderHeader PurchaseOrderHeader = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);




            return Redirect(System.Configuration.ConfigurationManager.AppSettings["PurchaseDomain"] + "/" + "PurchaseOrderHeader" + "/" + "Index" + "/" + PurchaseOrderHeader.DocTypeId);
        }

        public ActionResult PurchaseOrder_OnApprove(int Id)
        {
            //NotificationContents _NotificationContents = new NotificationContents(_unitOfWork);
            //MailContents _MailContents = new MailContents(_unitOfWork);
            //_NotificationContents.PurchaseOrder_OnApprove(Id);
            //_MailContents.PurchaseOrder_OnApprove(Id);

            PurchaseOrderHeader H = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["PurchaseDomain"] + "/" + "PurchaseOrderHeader" + "/" + "Index" + "/" + H.DocTypeId);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Model.ViewModel;
using AutoMapper;
using Model.ViewModels;
using System.Configuration;
using System.Text;
using System.Data.SqlClient;

namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseOrderHeaderChargeController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IPurchaseOrderHeaderService _PurchaseOrderHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        

        public PurchaseOrderHeaderChargeController(IPurchaseOrderHeaderService PurchaseOrderHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseOrderHeaderService = PurchaseOrderHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }


        public ActionResult GetPOHeaderChargeForEdit(int Id, string HeaderTable, string LineTable)//PurchaseOrderHeader Id
        {

            var temp = new PurchaseOrderHeaderChargeService(_unitOfWork).GetCalculationFooterListSProc(Id, HeaderTable, LineTable).ToList();
            ViewBag.ChargeType = TransactionDocCategoryConstants.PurchaseOrder;
            return PartialView("FooterChargeEdit", temp);
        }



        [HttpPost]
        public ActionResult PostPOCalculationFields(List<HeaderChargeViewModel> temp)
        {
            foreach(var item in temp)
            {
                var header = new PurchaseOrderHeaderChargeService(_unitOfWork).Find(item.Id);
                header.Rate = item.Rate;
                header.Amount = item.Amount;
                new PurchaseOrderHeaderChargeService(_unitOfWork).Update(header);
            }

            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                
                return PartialView("~/TaxCalculation/FooterChargeEdit", temp);

            }


            return Json(new { success = true });
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

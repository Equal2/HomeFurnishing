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
using System.IO;
using ImageResizer;
using System.Configuration;
using Model.ViewModel;

namespace Jobs.Controllers
{
    [Authorize]
    public class TaxCalculationController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public TaxCalculationController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _unitOfWork = unitOfWork;
            _exception = exec;
        }



        public JsonResult GetMaxLineId(int HeaderId,string LineTable,string LineKeyField,string HeaderKeyField)
        {
            return Json(new PurchaseOrderLineChargeService(_unitOfWork).GetMaxProductCharge(HeaderId, LineTable, HeaderKeyField, LineKeyField),JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetCalculationFieldsFooter(int HeaderId, int? CalculationId,string HeaderChargeTable,string LineChargeTable,int DocTypeId,int SiteId,int DivisionId)
        {

            //var HeaderChargesCount = new PurchaseOrderHeaderChargeService(_unitOfWork).GetCalculationFooterList(HeaderId).Count();
            //if (HeaderChargesCount > 0)
            //{
            //    return Json(new PurchaseOrderHeaderChargeService(_unitOfWork).GetCalculationFooterList(HeaderId).ToList());
            //}

            //return Json(new CalculationFooterService(_unitOfWork).GetCalculationFooterList(CalculationId ?? 0).ToList());


            var count = new PurchaseOrderHeaderChargeService(_unitOfWork).GetCalculationFooterListSProc(HeaderId, HeaderChargeTable, LineChargeTable).Count();

            if(count>0)
            {

                var temp=new PurchaseOrderHeaderChargeService(_unitOfWork).GetCalculationFooterListSProc(HeaderId, HeaderChargeTable, LineChargeTable).ToList();

                return Json(temp, JsonRequestBehavior.AllowGet);
            }

            return Json(new CalculationFooterService(_unitOfWork).GetCalculationFooterList(CalculationId ?? 0,DocTypeId, SiteId, DivisionId).ToList());

        }



        public JsonResult GetCalculationFieldsProduct(int HeaderId, int? CalculationId, string LineChargeTable, int MaxLineId, int DocTypeId, int SiteId, int DivisionId)
        {

           // var maxchargescount = new PurchaseOrderLineChargeService(_unitOfWork).GetMaxProductCharge(HeaderId,LineTable,HeaderTableFieldName,LineTableFieldName);


            if (MaxLineId <=0)
            {                
                return Json(new CalculationProductService(_unitOfWork).GetCalculationProductList(CalculationId ?? 0, DocTypeId, SiteId, DivisionId).ToList());
            }
            else
            {
                var temp = new PurchaseOrderLineChargeService(_unitOfWork).GetCalculationProductListSProc(MaxLineId, LineChargeTable).ToList();
                foreach (var item in temp)
                {
                    item.Amount = 0;
                }
                return Json(temp);
            }
        }



        public JsonResult GetChargeRates(int? CalculationId, int DocTypeId, int SiteId, int DivisionId, int ProcessId, int? ChargeGroupPersonId, int? ChargeGroupProductId, int? ProductId = null)
        {
            return Json(new CalculationProductService(_unitOfWork).GetChargeRates(CalculationId ?? 0, DocTypeId, SiteId, DivisionId, ProcessId, ChargeGroupPersonId, ChargeGroupProductId, ProductId).ToList());
        }












        //public JsonResult GetCalculationFieldsFooterEdit(int HeaderId)
        //{
        //    return Json(new PurchaseOrderHeaderChargeService(_unitOfWork).GetCalculationFooterList(HeaderId).ToList());
        //}

        //public JsonResult GetCalculationFieldsFooterEdit(int HeaderId, string HeaderTable, string LineTable)
        //{
        //    return Json(new PurchaseOrderHeaderChargeService(_unitOfWork).GetCalculationFooterListSProc(HeaderId, "Web.PurchaseOrderHeaderCharges", "Web.PurchaseOrderLineCharges").ToList());
        //}

        public JsonResult GetProductCharge(int LineId, string LineTable)
        {
            return Json(new PurchaseOrderLineChargeService(_unitOfWork).GetCalculationProductListSProc(LineId, LineTable).ToList());
        }



        public JsonResult GetHeaderCharge(int HeaderId, string HeaderTable, string LineTable)
        {
            return Json(new PurchaseOrderHeaderChargeService(_unitOfWork).GetCalculationFooterListSProc(HeaderId, HeaderTable, LineTable).ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _GetHeaderCharge(int HeaderId, string HeaderTable, string LineTable, string EditUrl)
        {
            ViewBag.HeaderTable = HeaderTable;
            ViewBag.LineTable = LineTable;
            ViewBag.EditUrl = EditUrl;
            return PartialView(new PurchaseOrderHeaderChargeService(_unitOfWork).GetCalculationFooterListSProc(HeaderId, HeaderTable, LineTable).ToList());
        }

        public ActionResult GetHeaderChargeForEdit(int Id, string HeaderTable, string LineTable)//PurchaseOrderHeader Id
        {

            var temp = new PurchaseOrderHeaderChargeService(_unitOfWork).GetCalculationFooterListSProc(Id, HeaderTable, LineTable).ToList();
            return PartialView("FooterChargeEdit", temp);
        }





        public ActionResult GetSIHeaderChargeForEdit(int Id, string HeaderTable, string LineTable)//PurchaseOrderHeader Id
        {

            var temp = new PurchaseOrderHeaderChargeService(_unitOfWork).GetCalculationFooterListSProc(Id, HeaderTable, LineTable).ToList();
            ViewBag.ChargeType = TransactionDocCategoryConstants.SaleInvoice;
            return PartialView("FooterChargeEdit", temp);
        }



        public ActionResult GetPIRHeaderChargeForEdit(int Id, string HeaderTable, string LineTable)//PurchaseOrderHeader Id
        {

            var temp = new PurchaseOrderHeaderChargeService(_unitOfWork).GetCalculationFooterListSProc(Id, HeaderTable, LineTable).ToList();
            ViewBag.ChargeType = TransactionDocCategoryConstants.SaleInvoiceReturn;
            return PartialView("FooterChargeEdit", temp);
        }















        //ForEditing the Header Record







        public ActionResult GetJOHeaderChargeForEdit(int Id, string HeaderTable, string LineTable)//PurchaseOrderHeader Id
        {

            var temp = new JobOrderHeaderChargeService(_unitOfWork).GetCalculationFooterListSProc(Id, HeaderTable, LineTable).ToList();
            ViewBag.ChargeType = TransactionDocCategoryConstants.JobOrder;
            return PartialView("FooterChargeEdit", temp);
        }



        [HttpPost]
        public ActionResult PostJOCalculationFields(List<HeaderChargeViewModel> temp)
        {
            foreach (var item in temp)
            {
                var header = new JobOrderHeaderChargeService(_unitOfWork).Find(item.Id);
                header.Rate = item.Rate;
                header.Amount = item.Amount;
                new JobOrderHeaderChargeService(_unitOfWork).Update(header);
            }

            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);

                return PartialView("FooterChargeEdit", temp);

            }


            return Json(new { success = true });
        }



        public ActionResult GetJIHeaderChargeForEdit(int Id, string HeaderTable, string LineTable)//JobOrderHeader Id
        {

            var temp = new JobOrderHeaderChargeService(_unitOfWork).GetCalculationFooterListSProc(Id, HeaderTable, LineTable).ToList();
            ViewBag.ChargeType = TransactionDocCategoryConstants.JobInvoice;
            return PartialView("FooterChargeEdit", temp);
        }



        [HttpPost]
        public ActionResult PostJICalculationFields(List<HeaderChargeViewModel> temp)
        {
            foreach (var item in temp)
            {
                var header = new JobInvoiceHeaderChargeService(_unitOfWork).Find(item.Id);
                header.Rate = item.Rate;
                header.Amount = item.Amount;
                new JobInvoiceHeaderChargeService(_unitOfWork).Update(header);
            }

            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);

                return PartialView("FooterChargeEdit", temp);

            }


            return Json(new { success = true });
        }

        public ActionResult GetJIRHeaderChargeForEdit(int Id, string HeaderTable, string LineTable)//JobOrderHeader Id
        {

            var temp = new JobInvoiceReturnHeaderChargeService(db).GetCalculationFooterListSProc(Id, HeaderTable, LineTable).ToList();
            ViewBag.ChargeType = TransactionDocCategoryConstants.JobInvoiceReturn;
            return PartialView("FooterChargeEdit", temp);
        }



        [HttpPost]
        public ActionResult PostJIRCalculationFields(List<HeaderChargeViewModel> temp)
        {
            foreach (var item in temp)
            {
                var header = new JobInvoiceReturnHeaderChargeService(db).Find(item.Id);
                header.Rate = item.Rate;
                header.Amount = item.Amount;
                new JobInvoiceReturnHeaderChargeService(db).Update(header);
            }

            try
            {
                db.SaveChanges();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);

                return PartialView("FooterChargeEdit", temp);

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
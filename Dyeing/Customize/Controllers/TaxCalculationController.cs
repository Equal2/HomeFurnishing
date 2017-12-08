using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Components.ExceptionHandlers;
using Services.BasicSetup;
using ProjLib.DocumentConstants;
using Models.Company.ViewModels;
using Services.Customize;

namespace Customize.Controllers
{
    [Authorize]
    public class TaxCalculationController : System.Web.Mvc.Controller
    {
        ICalculationProductService _calculationProductService;
        ICalculationFooterService _calculationFooterService;
        IJobOrderHeaderChargeService _jobOrderHeaderChargeService;
        IExceptionHandler _exception;
        public TaxCalculationController(IExceptionHandler excep, ICalculationProductService calculationProductServ, ICalculationFooterService calculationfooterServ,
            IJobOrderHeaderChargeService jobOrderHeaderChargeSer)
        {
            _exception = excep;
            _calculationProductService = calculationProductServ;
            _calculationFooterService = calculationfooterServ;
            _jobOrderHeaderChargeService = jobOrderHeaderChargeSer;
        }



        public JsonResult GetMaxLineId(int HeaderId,string LineTable,string LineKeyField,string HeaderKeyField)
        {
            return Json(_calculationProductService.GetMaxProductCharge(HeaderId, LineTable, HeaderKeyField, LineKeyField), JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetCalculationFieldsFooter(int HeaderId, int? CalculationId,string HeaderChargeTable,string LineChargeTable,int DocTypeId,int SiteId,int DivisionId)
        {
            var Records = _calculationFooterService.GetCalculationFooterListSProc(HeaderId, HeaderChargeTable, LineChargeTable).ToList();

            if(Records.Count()>0)
            {
                return Json(Records, JsonRequestBehavior.AllowGet);
            }

            return Json(_calculationFooterService.GetCalculationFooterList(CalculationId ?? 0, DocTypeId, SiteId, DivisionId).ToList());

        }



        public JsonResult GetCalculationFieldsProduct(int HeaderId, int? CalculationId, string LineChargeTable, int MaxLineId, int DocTypeId, int SiteId, int DivisionId)
        {
            if (MaxLineId <=0)
            {                
                return Json(_calculationProductService.GetCalculationProductList(CalculationId ?? 0, DocTypeId, SiteId, DivisionId).ToList());
            }
            else
            {
                var temp = _calculationProductService.GetCalculationProductListSProc(MaxLineId, LineChargeTable).ToList();
                foreach (var item in temp)
                {
                    item.Amount = 0;
                }
                return Json(temp);
            }
        }

        public JsonResult GetProductCharge(int LineId, string LineTable)
        {
            return Json(_calculationProductService.GetCalculationProductListSProc(LineId, LineTable).ToList());
        }



        public JsonResult GetHeaderCharge(int HeaderId, string HeaderTable, string LineTable)
        {
            return Json(_calculationFooterService.GetCalculationFooterListSProc(HeaderId, HeaderTable, LineTable).ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetHeaderChargeForEdit(int Id, string HeaderTable, string LineTable)//PurchaseOrderHeader Id
        {
            var temp = _calculationFooterService.GetCalculationFooterListSProc(Id, HeaderTable, LineTable).ToList();
            return PartialView("FooterChargeEdit", temp);
        }







        //ForEditing the Header Record

        public ActionResult GetJOHeaderChargeForEdit(int Id, string HeaderTable, string LineTable)//PurchaseOrderHeader Id
        {

            var temp = _calculationFooterService.GetCalculationFooterListSProc(Id, HeaderTable, LineTable).ToList();
            ViewBag.ChargeType = TransactionDocCategoryConstants.JobOrder;
            return PartialView("FooterChargeEdit", temp);
        }



        [HttpPost]
        public ActionResult PostJOCalculationFields(List<HeaderChargeViewModel> temp)
        {
            try
            {
                _jobOrderHeaderChargeService.UpdateHeaderCharges(temp);
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
                _calculationFooterService.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
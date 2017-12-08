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
using AutoMapper;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class WeavingInvoiceController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobInvoiceHeaderService _JobInvoiceHeaderService;
        IJobInvoiceLineService _JobInvoiceLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        string TanaPaymentDueDocType = "Tana Payment Due";
        string TdsCollectionDocType = "Tds Collection";
        string TanaPaymentDueAccount = "Tana Payment Due A/c";

        public WeavingInvoiceController(IJobInvoiceLineService SaleOrder, IJobInvoiceHeaderService JobInvoiceHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobInvoiceLineService = SaleOrder;
            _JobInvoiceHeaderService = JobInvoiceHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public void PrepareViewBag(int id)
        {
            var Header = _JobInvoiceHeaderService.Find(id);
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(Header.DocTypeId).DocumentTypeName;
            ViewBag.DocNo = Header.DocNo;

            if (Header.Status == (int)StatusConstants.Drafted || Header.Status == (int)StatusConstants.Import)
            {
                ViewBag.Url = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobInvoiceHeader/Modify/" + id;
            }
            else if (Header.Status == (int)StatusConstants.Submitted || Header.Status == (int)StatusConstants.ModificationSubmitted || Header.Status == (int)StatusConstants.Modified)
            {
                ViewBag.Url = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobInvoiceHeader/ModifyAfter_Submit/" + id;
            }
            else if (Header.Status == (int)StatusConstants.Approved || Header.Status == (int)StatusConstants.Closed)
            {
                ViewBag.Url = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobInvoiceHeader/ModifyAfter_Approve/" + id;
            }


        }


        [HttpGet]
        public ActionResult GetSummary(int id)
        {
            var Header = _JobInvoiceHeaderService.Find(id);

            SqlParameter SqlParameterJobInvoiceHeaderId = new SqlParameter("@JobInvoiceHeaderId", id);

            List<JobInvoiceSummaryViewModel> JobInvoices = db.Database.SqlQuery<JobInvoiceSummaryViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetPersonDueAndAdvance @JobInvoiceHeaderId", SqlParameterJobInvoiceHeaderId).ToList();


            JobInvoiceSummaryDetailViewModel vm = new JobInvoiceSummaryDetailViewModel();
            vm.JobInvoiceHeaderId = id;
            vm.DocDate = Header.DocDate;
            vm.DocNo = Header.DocNo;
            vm.ProcessId = Header.ProcessId;
            vm.JobInvoiceSummaryViewModel = JobInvoices;
            PrepareViewBag(id);

            if (JobInvoices.Count() == 0)
                return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobInvoiceHeader/Index/" + Header.DocTypeId);
            else
                return View("Summary", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostSummary(JobInvoiceSummaryDetailViewModel vm)
        {

            //TempData["CSEXC"] = "Customize Test Exception";


            String ErrorMsg = "";

            foreach (var item in vm.JobInvoiceSummaryViewModel)
            {
                if (item.AdvanceAdjusted > item.InvoiceAmount + item.TanaAmount )
                {
                    string ProductUidName = "";
                    if (item.ProductUidId!=null )
                     ProductUidName = new ProductUidService(_unitOfWork).Find((int)item.ProductUidId).ProductUidName;

                    ErrorMsg = "Total adjusted advance for barcode " + ProductUidName + " is exceeding invoice amount.";
                }
            }


            var AdvanceData = (from H in vm.JobInvoiceSummaryViewModel
                               group new { H } by new { H.CostCenterId } into Result
                               select new
                               {
                                   CostCenterId = Result.Key.CostCenterId,
                                   TotalAdvanceAmount = Result.Max(m => m.H.AdvanceAmount),
                                   TotalAdvanceAdjusted = Result.Sum(m => m.H.AdvanceAdjusted)
                               }).ToList();

            foreach(var item in AdvanceData)
            {
                if (item.TotalAdvanceAdjusted > item.TotalAdvanceAmount)
                {
                    string CostCenterName = new CostCenterService(_unitOfWork).Find(item.CostCenterId).CostCenterName;
                    ErrorMsg = "Total adjusted advance for purja " + CostCenterName + " is exceeding total advance amount.";
                }
            }





            var TdsData = (from H in vm.JobInvoiceSummaryViewModel
                               group new { H } by new { H.PersonId } into Result
                               select new
                               {
                                   PersonId = Result.Key.PersonId,
                                   TotalTdsAmount = Result.Max(m => m.H.TdsAmount),
                                   TotalTdsAdjusted = Result.Sum(m => m.H.TdsAdjusted)
                               }).ToList();

            foreach (var item in TdsData)
            {
                if (item.TotalTdsAdjusted > item.TotalTdsAmount)
                {
                    string PersonName = new PersonService(_unitOfWork).Find(item.PersonId).Name;
                    ErrorMsg = "Total adjusted tds for person " + PersonName + " is exceeding total tds amount.";
                }
            }

            if (ErrorMsg != "")
            {
                TempData["CSEXC"] = ErrorMsg;
                return View("Summary", vm);
            }



            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            bool Modified = false;
            int Id = vm.JobInvoiceHeaderId;

            var Header = _JobInvoiceHeaderService.Find(Id);


            int LedgerHeaderId = 0;
            int LedgerLineId = 0;
            int LedgerId = 0;
            int LedgerAdjId = 0;
            int TanaPaymentDueCnt = 0;
            int TdsCollectionCnt = 0;
            int SavedTanaLedgerHeaderId = 0;
            int SavedTdsCollectionLedgerHeaderId = 0;
            int TanaPaymentDueAcId = new LedgerAccountService(_unitOfWork).Find(TanaPaymentDueAccount).LedgerAccountId;


            DeleteLedgerTransaction(Header.JobInvoiceHeaderId);


            DataTable AdvanceAdjustedDataTable = new DataTable();
            AdvanceAdjustedDataTable.Columns.Add("Id");
            AdvanceAdjustedDataTable.Columns.Add("Qty");

            foreach (var item in vm.JobInvoiceSummaryViewModel)
            {
                if (item.TanaAmount > 0)
                {
                    #region "Tana Payment Due"
                    if (TanaPaymentDueCnt == 0)
                    {
                        LedgerHeader TanaLedgerHeader = new LedgerHeader();
                        TanaLedgerHeader.LedgerHeaderId = LedgerHeaderId;
                        TanaLedgerHeader.DocTypeId = new DocumentTypeService(_unitOfWork).Find(TanaPaymentDueDocType).DocumentTypeId;
                        TanaLedgerHeader.DocDate = DateTime.Now.Date;
                        TanaLedgerHeader.DivisionId = Header.DivisionId;
                        TanaLedgerHeader.SiteId = Header.SiteId;
                        TanaLedgerHeader.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".LedgerHeaders", TanaLedgerHeader.DocTypeId, TanaLedgerHeader.DocDate, TanaLedgerHeader.DivisionId, TanaLedgerHeader.SiteId);
                        TanaLedgerHeader.LedgerAccountId = TanaPaymentDueAcId;
                        TanaLedgerHeader.Narration = "Tana Payment Due";
                        TanaLedgerHeader.Status = (int)StatusConstants.Submitted;
                        TanaLedgerHeader.ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId;
                        TanaLedgerHeader.CostCenterId = null;
                        TanaLedgerHeader.AdjustmentType = "Due";
                        TanaLedgerHeader.PaymentFor = DateTime.Now.Date;

                        TanaLedgerHeader.ReferenceDocId = Header.JobInvoiceHeaderId;
                        TanaLedgerHeader.ReferenceDocTypeId = Header.DocTypeId;

                        TanaLedgerHeader.CreatedDate = DateTime.Now;
                        TanaLedgerHeader.ModifiedDate = DateTime.Now;
                        TanaLedgerHeader.CreatedBy = User.Identity.Name;
                        TanaLedgerHeader.ModifiedBy = User.Identity.Name;
                        TanaLedgerHeader.ObjectState = Model.ObjectState.Added;
                        new LedgerHeaderService(_unitOfWork).Create(TanaLedgerHeader);
                        LedgerHeaderId = LedgerHeaderId - 1;
                        TanaPaymentDueCnt = TanaPaymentDueCnt + 1;
                        SavedTanaLedgerHeaderId = TanaLedgerHeader.LedgerHeaderId;
                    }


                    LedgerLine TanaLedgerLine = new LedgerLine();
                    TanaLedgerLine.LedgerLineId = LedgerLineId;
                    TanaLedgerLine.LedgerHeaderId = SavedTanaLedgerHeaderId;
                    TanaLedgerLine.LedgerAccountId = new LedgerAccountService(_unitOfWork).GetLedgerAccountByPersondId(item.PersonId).LedgerAccountId;
                    TanaLedgerLine.CostCenterId = item.CostCenterId;
                    TanaLedgerLine.ProductUidId = item.ProductUidId;
                    TanaLedgerLine.Amount = item.TanaAmount;
                    TanaLedgerLine.ReferenceDocLineId = item.JobInvoiceLineId;
                    TanaLedgerLine.ReferenceDocTypeId = Header.DocTypeId;
                    TanaLedgerLine.CreatedDate = DateTime.Now;
                    TanaLedgerLine.ModifiedDate = DateTime.Now;
                    TanaLedgerLine.CreatedBy = User.Identity.Name;
                    TanaLedgerLine.ModifiedBy = User.Identity.Name;
                    TanaLedgerLine.ObjectState = Model.ObjectState.Added;
                    new LedgerLineService(_unitOfWork).Create(TanaLedgerLine);
                    LedgerLineId = LedgerLineId - 1;


                    Ledger TanaLedgerDr = new Ledger();
                    TanaLedgerDr.LedgerId = LedgerId;
                    TanaLedgerDr.LedgerHeaderId = SavedTanaLedgerHeaderId;
                    TanaLedgerDr.LedgerAccountId = TanaPaymentDueAcId;
                    TanaLedgerDr.ContraLedgerAccountId = TanaLedgerLine.LedgerAccountId;
                    TanaLedgerDr.CostCenterId = null;
                    TanaLedgerDr.ProductUidId = item.ProductUidId;
                    TanaLedgerDr.AmtDr = TanaLedgerLine.Amount;
                    TanaLedgerDr.AmtCr = 0;
                    TanaLedgerDr.Narration = "Tana Payment Due";
                    TanaLedgerDr.LedgerLineId = TanaLedgerLine.LedgerLineId;
                    TanaLedgerDr.ObjectState = Model.ObjectState.Added;
                    new LedgerService(_unitOfWork).Create(TanaLedgerDr);
                    LedgerId = LedgerId - 1;


                    Ledger TanaLedgerCr = new Ledger();
                    TanaLedgerDr.LedgerId = LedgerId;
                    TanaLedgerCr.LedgerHeaderId = SavedTanaLedgerHeaderId;
                    TanaLedgerCr.LedgerAccountId = TanaLedgerLine.LedgerAccountId;
                    TanaLedgerCr.ContraLedgerAccountId = TanaPaymentDueAcId;
                    TanaLedgerCr.CostCenterId = item.CostCenterId;
                    TanaLedgerDr.ProductUidId = item.ProductUidId;
                    TanaLedgerCr.AmtDr = 0;
                    TanaLedgerCr.AmtCr = TanaLedgerLine.Amount;
                    TanaLedgerCr.Narration = null;
                    TanaLedgerCr.LedgerLineId = TanaLedgerLine.LedgerLineId;
                    TanaLedgerCr.ObjectState = Model.ObjectState.Added;
                    new LedgerService(_unitOfWork).Create(TanaLedgerCr);
                    LedgerId = LedgerId - 1;

                #endregion
                }

                if (item.TdsAdjusted > 0)
                {
                    #region "Tds Collection"


                    var TdsRate = (from H in db.BusinessEntity
                                   join Tr in db.TdsRate on new { X1 = H.TdsCategoryId ?? 0, X2 = H.TdsGroupId ?? 0 } equals new { X1 = Tr.TdsCategoryId, X2 = Tr.TdsGroupId } into TdsRateTable
                                   from TdsRateTab in TdsRateTable.DefaultIfEmpty()
                                   where H.PersonID == item.PersonId
                                   select TdsRateTab).FirstOrDefault();


                    if (TdsCollectionCnt == 0)
                    {
                        LedgerHeader TdsLedgerHeader = new LedgerHeader();
                        TdsLedgerHeader.LedgerHeaderId = LedgerHeaderId;
                        TdsLedgerHeader.DocTypeId = new DocumentTypeService(_unitOfWork).Find(TdsCollectionDocType).DocumentTypeId;
                        TdsLedgerHeader.DocDate = DateTime.Now.Date;
                        TdsLedgerHeader.DivisionId = Header.DivisionId;
                        TdsLedgerHeader.SiteId = Header.SiteId;
                        TdsLedgerHeader.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".LedgerHeaders", TdsLedgerHeader.DocTypeId, TdsLedgerHeader.DocDate, TdsLedgerHeader.DivisionId, TdsLedgerHeader.SiteId);
                        TdsLedgerHeader.LedgerAccountId = TdsRate.LedgerAccountId;
                        TdsLedgerHeader.Narration = "Tds Adjustment";
                        TdsLedgerHeader.Status = (int)StatusConstants.Submitted;
                        TdsLedgerHeader.ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId;
                        TdsLedgerHeader.CostCenterId = null;
                        TdsLedgerHeader.AdjustmentType = "Due";
                        TdsLedgerHeader.PaymentFor = DateTime.Now.Date;

                        TdsLedgerHeader.ReferenceDocId = Header.JobInvoiceHeaderId;
                        TdsLedgerHeader.ReferenceDocTypeId = Header.DocTypeId;

                        TdsLedgerHeader.CreatedDate = DateTime.Now;
                        TdsLedgerHeader.ModifiedDate = DateTime.Now;
                        TdsLedgerHeader.CreatedBy = User.Identity.Name;
                        TdsLedgerHeader.ModifiedBy = User.Identity.Name;
                        TdsLedgerHeader.ObjectState = Model.ObjectState.Added;
                        new LedgerHeaderService(_unitOfWork).Create(TdsLedgerHeader);
                        LedgerHeaderId = LedgerHeaderId - 1;
                        TdsCollectionCnt = TdsCollectionCnt + 1;
                        SavedTdsCollectionLedgerHeaderId = TdsLedgerHeader.LedgerHeaderId;
                    }


                    LedgerLine TdsLedgerLine = new LedgerLine();
                    TdsLedgerLine.LedgerLineId = LedgerLineId;
                    TdsLedgerLine.LedgerHeaderId = SavedTdsCollectionLedgerHeaderId;
                    TdsLedgerLine.LedgerAccountId = new LedgerAccountService(_unitOfWork).GetLedgerAccountByPersondId(item.PersonId).LedgerAccountId;
                    if (item.CostCenterId != 0)
                    {
                        TdsLedgerLine.CostCenterId = item.CostCenterId;
                    }                    
                    TdsLedgerLine.ProductUidId = item.ProductUidId;
                    TdsLedgerLine.BaseValue = item.TdsAdjusted * 100 / TdsRate.Percentage;
                    TdsLedgerLine.Amount = item.TdsAdjusted;
                    TdsLedgerLine.CreatedDate = DateTime.Now;
                    TdsLedgerLine.ModifiedDate = DateTime.Now;
                    TdsLedgerLine.CreatedBy = User.Identity.Name;
                    TdsLedgerLine.ModifiedBy = User.Identity.Name;
                    TdsLedgerLine.ObjectState = Model.ObjectState.Added;
                    new LedgerLineService(_unitOfWork).Create(TdsLedgerLine);
                    LedgerLineId = LedgerLineId - 1;


                    Ledger TdsLedgerDr = new Ledger();
                    TdsLedgerDr.LedgerId = LedgerId;
                    TdsLedgerDr.LedgerHeaderId = SavedTdsCollectionLedgerHeaderId;
                    TdsLedgerDr.LedgerAccountId = TdsLedgerLine.LedgerAccountId;
                    TdsLedgerDr.ContraLedgerAccountId = TdsRate.LedgerAccountId;
                    if (item.CostCenterId != 0)
                    {
                        TdsLedgerDr.CostCenterId = item.CostCenterId;
                    }
                    TdsLedgerDr.ProductUidId = item.ProductUidId;
                    TdsLedgerDr.AmtDr = TdsLedgerLine.Amount;
                    TdsLedgerDr.AmtCr = 0;
                    TdsLedgerDr.Narration = "Tds Adjusted.";
                    TdsLedgerDr.LedgerLineId = TdsLedgerLine.LedgerLineId;
                    TdsLedgerDr.ObjectState = Model.ObjectState.Added;
                    new LedgerService(_unitOfWork).Create(TdsLedgerDr);
                    LedgerId = LedgerId - 1;


                    Ledger TdsLedgerCr = new Ledger();
                    TdsLedgerCr.LedgerId = LedgerId;
                    TdsLedgerCr.LedgerHeaderId = SavedTdsCollectionLedgerHeaderId;
                    TdsLedgerCr.LedgerAccountId = (int)TdsRate.LedgerAccountId;
                    TdsLedgerCr.ContraLedgerAccountId = TdsLedgerLine.LedgerAccountId;
                    TdsLedgerCr.CostCenterId = null;
                    TdsLedgerCr.ProductUidId = item.ProductUidId;
                    TdsLedgerCr.AmtDr = 0;
                    TdsLedgerCr.AmtCr = TdsLedgerLine.Amount;
                    TdsLedgerCr.Narration = null;
                    TdsLedgerCr.LedgerLineId = TdsLedgerLine.LedgerLineId;
                    TdsLedgerCr.ObjectState = Model.ObjectState.Added;
                    new LedgerService(_unitOfWork).Create(TdsLedgerCr);
                    LedgerId = LedgerId - 1;


                    var TempLedger = (from L in db.Ledger 
                                      where L.ReferenceDocLineId == item.JobInvoiceLineId && L.ReferenceDocTypeId == Header.DocTypeId
                                      select new
                                      {
                                          LedgerCrId = L.LedgerId
                                      }).FirstOrDefault();

                    LedgerAdj LedgerAdj = new LedgerAdj();
                    LedgerAdj.LedgerAdjId = LedgerAdjId;
                    LedgerAdj.DrLedgerId = TdsLedgerDr.LedgerId;
                    LedgerAdj.CrLedgerId = TempLedger ==null ? 0 : TempLedger.LedgerCrId;
                    LedgerAdj.SiteId = Header.SiteId;
                    LedgerAdj.Adj_Type = "Tds Adjustment";
                    LedgerAdj.Amount = TdsLedgerDr.AmtDr;
                    LedgerAdj.CreatedBy = Header.CreatedBy;
                    LedgerAdj.CreatedDate = DateTime.Now;
                    LedgerAdj.ModifiedBy = Header.ModifiedBy;
                    LedgerAdj.ModifiedDate = DateTime.Now;
                    LedgerAdj.ObjectState = Model.ObjectState.Added;
                    new LedgerAdjService(_unitOfWork).Create(LedgerAdj);
                    LedgerAdjId = LedgerAdjId - 1;

                    #endregion
                }

                if (item.AdvanceAdjusted > 0)
                {
                    var AdvanceAdjustedDataRow = AdvanceAdjustedDataTable.NewRow();
                    AdvanceAdjustedDataRow["Id"] = item.JobInvoiceLineId;
                    AdvanceAdjustedDataRow["Qty"] = item.AdvanceAdjusted ;
                    AdvanceAdjustedDataTable.Rows.Add(AdvanceAdjustedDataRow);
                }
            }


            if (AdvanceAdjustedDataTable.Rows.Count > 0)
            {
                string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
                DataSet ds = new DataSet();
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spGetLedgerAdj"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;
                        cmd.Parameters.AddWithValue("@T", AdvanceAdjustedDataTable);
                        cmd.CommandTimeout = 1000;
                        using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                        {
                            adp.Fill(ds);
                        }
                    }
                }

                for (int j = 0; j <= ds.Tables[0].Rows.Count - 1; j++)
                {
                    LedgerAdj LedgerAdj = new LedgerAdj();
                    LedgerAdj.LedgerAdjId = LedgerAdjId;
                    LedgerAdj.DrLedgerId = (int)ds.Tables[0].Rows[j]["LedgerDrId"];
                    LedgerAdj.CrLedgerId = (int)ds.Tables[0].Rows[j]["LedgerCrId"];
                    LedgerAdj.SiteId = Header.SiteId;
                    LedgerAdj.Adj_Type = "Advance Adjustment";
                    LedgerAdj.Amount = (decimal)ds.Tables[0].Rows[j]["BalanceDr"];
                    LedgerAdj.CreatedBy = Header.CreatedBy;
                    LedgerAdj.CreatedDate = DateTime.Now;
                    LedgerAdj.ModifiedBy = Header.ModifiedBy;
                    LedgerAdj.ModifiedDate = DateTime.Now;
                    LedgerAdj.ObjectState = Model.ObjectState.Added;
                    new LedgerAdjService(_unitOfWork).Create(LedgerAdj);
                    LedgerAdjId = LedgerAdjId - 1;
                }
            }

            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                PrepareViewBag(vm.JobInvoiceHeaderId);
                return Json(new { Success = false });
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = Header.DocTypeId,
                DocId = Header.JobInvoiceHeaderId,
                ActivityType = (int)ActivityTypeContants.Modified,
                DocNo = Header.DocNo,
                //xEModifications = Modifications,
                DocDate = Header.DocDate,
                DocStatus = Header.Status,
            }));

            string RetUrl = "";


            if (Header.Status == (int)StatusConstants.Drafted || Header.Status == (int)StatusConstants.Import)
            {
                RetUrl = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobInvoiceHeader/Modify/" + Header.JobInvoiceHeaderId;
            }
            else if (Header.Status == (int)StatusConstants.Submitted || Header.Status == (int)StatusConstants.ModificationSubmitted || Header.Status == (int)StatusConstants.Modified)
            {
                RetUrl = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobInvoiceHeader/ModifyAfter_Submit/" + Header.JobInvoiceHeaderId;
            }
            else if (Header.Status == (int)StatusConstants.Approved || Header.Status == (int)StatusConstants.Closed)
            {
                RetUrl = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobInvoiceHeader/ModifyAfter_Approve/" + Header.JobInvoiceHeaderId;
            }
            else
                RetUrl = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobInvoiceHeader/Index/" + Header.DocTypeId;

            return Json(new { Success = true, Url = RetUrl });
        }



        public void DeleteLedgerTransaction(int JobInvoiceHeaderId)
        {
            var Header = new JobInvoiceHeaderService(_unitOfWork).Find(JobInvoiceHeaderId);

            int TanaPaymentDueDocTypeId = new DocumentTypeService(_unitOfWork).Find(TanaPaymentDueDocType).DocumentTypeId;
            int TdsCollectionDocTypeId = new DocumentTypeService(_unitOfWork).Find(TdsCollectionDocType).DocumentTypeId;

            var LedgerHeaderList = (from H in db.LedgerHeader
                                    where H.ReferenceDocId == Header.JobInvoiceHeaderId && H.ReferenceDocTypeId == Header.DocTypeId
                                    && (H.DocTypeId == TanaPaymentDueDocTypeId || H.DocTypeId == TdsCollectionDocTypeId)
                                    select H).ToList();

            foreach (var LedgerHeader in LedgerHeaderList)
            {
                var ExisitingLedgers = (from p in db.Ledger
                                        where p.LedgerHeaderId == LedgerHeader.LedgerHeaderId
                                        select p).ToList();

                var LedgerLines = (from p in db.LedgerLine
                                   where p.LedgerHeaderId == LedgerHeader.LedgerHeaderId
                                    select p).ToList();


                foreach (var item in ExisitingLedgers)
                {

                    var LedgerAdjs = (from p in db.LedgerAdj
                                        where p.CrLedgerId == item.LedgerId || p.DrLedgerId == item.LedgerId
                                        select p).ToList();

                    foreach (var item2 in LedgerAdjs)
                    {
                        new LedgerAdjService(_unitOfWork).Delete(item2.LedgerAdjId);

                        //item2.ObjectState = Model.ObjectState.Deleted;
                        //db.LedgerAdj.Remove(item2);
                    }
                    //item.ObjectState = Model.ObjectState.Deleted;
                    //db.Ledger.Remove(item);
                    new LedgerService(_unitOfWork).Delete(item.LedgerId);
                }


                foreach (var item in LedgerLines)
                {
                    var RefLines = (from p in db.LedgerLineRefValue
                                    where p.LedgerLineId == item.LedgerLineId
                                    select p).ToList();

                    foreach (var RefLine in RefLines)
                    {
                        //RefLine.ObjectState = Model.ObjectState.Deleted;
                        //db.LedgerLineRefValue.Remove(RefLine);
                        new LedgerLineRefValueService(_unitOfWork).Delete(RefLine.LedgerLineRefValueId);
                    }

                    //item.ObjectState = Model.ObjectState.Deleted;
                    //db.LedgerLine.Remove(item);
                    new LedgerLineService(_unitOfWork).Delete(item.LedgerLineId);

                }

                //LedgerHeader.ObjectState = Model.ObjectState.Deleted;
                //db.LedgerHeader.Remove(LedgerHeader);
                new LedgerHeaderService(_unitOfWork).Delete(LedgerHeader.LedgerHeaderId);
            }

            var LedgerAdj = (from L in db.JobInvoiceLine
                             join H in db.JobInvoiceHeader on L.JobInvoiceHeaderId equals H.JobInvoiceHeaderId into JobInvoiceHeaderTable from JobInvoiceHeaderTab in JobInvoiceHeaderTable.DefaultIfEmpty()
                             join Ld in db.Ledger on new { A1 = L.JobInvoiceLineId, A2 = JobInvoiceHeaderTab.DocTypeId } equals new { A1 = Ld.ReferenceDocLineId ?? 0, A2 = Ld.ReferenceDocTypeId ?? 0 } into LedgerTable from LedgerTab in LedgerTable.DefaultIfEmpty()
                             join LAd in db.LedgerAdj on LedgerTab.LedgerId equals LAd.CrLedgerId into LedgerAdjTable from LedgerAdjTab in LedgerAdjTable.DefaultIfEmpty()
                             where L.JobInvoiceHeaderId == JobInvoiceHeaderId
                             && LedgerAdjTab.Adj_Type == "Advance Adjustment"
                             select LedgerAdjTab).ToList();

            foreach(var item in LedgerAdj)
            {
                //item.ObjectState = Model.ObjectState.Deleted;
                //db.LedgerAdj.Remove(item);
                new LedgerAdjService(_unitOfWork).Delete(item.LedgerAdjId);
            }
        }



        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);

            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    


}



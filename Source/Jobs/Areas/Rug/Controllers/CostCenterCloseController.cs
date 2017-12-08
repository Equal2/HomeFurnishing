using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using System.Configuration;
using Model.ViewModel;
using System.Data.SqlClient;
using System.Xml.Linq;
using Model.DatabaseViews;



namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class CostCenterCloseController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        IJobOrderHeaderService _JobOrderHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public CostCenterCloseController(IJobOrderHeaderService PurchaseOrderHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobOrderHeaderService = PurchaseOrderHeaderService;
            _exception = exec;
            _unitOfWork = unitOfWork;
        }

        public ActionResult CloseCostCenter()
        {
            return View("CloseCostCenters");
        }

        public JsonResult GetPendingCostCenters(DateTime FromDate, DateTime ToDate, int? JobWorker)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterJobWorker = new SqlParameter("@JobWorker", JobWorker == null ? DBNull.Value : (object)JobWorker);


            IEnumerable<PendingCostCenterForClosing> Tep = db.Database.SqlQuery<PendingCostCenterForClosing>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetPendingCostCenterListForClosing @SiteId, @DivisionId, @FromDate, @ToDate, @JobWorker ", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterJobWorker).ToList();
            //IEnumerable<PendingCostCenterForClosing> Tep = db.Database.SqlQuery<PendingCostCenterForClosing>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetPendingCostCenterListForClosing_OnlyForTrf @SiteId, @DivisionId, @FromDate, @ToDate, @JobWorker ", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterJobWorker).ToList();


            var FormattedDate = Tep.AsEnumerable().Select(m => new
            {
                JobWorker = m.JobWorker,
                CostCenter = m.CostCenter,
                StartDate = m.StartDate.ToString("dd/MMM/yyyy"),
                CompleteDate = m.CompleteDate.ToString("dd/MMM/yyyy"),
                CostCenterId = m.CostCenterId,
                Design = m.Design,
                BalAmount = m.BalAmount > 0 ? m.BalAmount.ToString() + " Dr" : Math.Abs((decimal)m.BalAmount).ToString() + " Cr",
            }).ToList();


            return Json(new { Success = true, Data = FormattedDate }, JsonRequestBehavior.AllowGet);
        }


        public class PendingCostCenterForClosing
        {
            public int CostCenterId { get; set; }
            public string JobWorker { get; set; }
            public string CostCenter { get; set; }
            public string Design { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime CompleteDate { get; set; }
            public decimal BalAmount { get; set; }
        }


        public ActionResult PostCostCenters(List<int> CostCenterId, DateTime CloseDate)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            int[] CostCenterIds = CostCenterId.Select(m => m).ToArray();




            if (ModelState.IsValid)
            {

                var CostCenters = (from p in db.CostCenter
                                   join t in db.CostCenterStatus on p.CostCenterId equals t.CostCenterId into CostCenterStatusTable
                                   from CostCenterStatusTab in CostCenterStatusTable.DefaultIfEmpty()
                                   join jos in db.JobOrderSettings on new { A = p.ReferenceDocTypeId ?? 0, B = p.SiteId ?? 0, C = p.DivisionId ?? 0 } equals new { A = jos.DocTypeId, B = jos.SiteId, C = jos.DivisionId } into josTable
                                   from josTab in josTable.DefaultIfEmpty()
                                   where CostCenterIds.Contains(p.CostCenterId)
                                   select new
                                   {
                                       p.CostCenterId,
                                       p.LedgerAccountId,
                                       p.DivisionId,
                                       p.SiteId,
                                       p.ReferenceDocId,
                                       p.ReferenceDocTypeId,
                                       AmountDr = CostCenterStatusTab.AmountDr ?? 0,
                                       AmountCr = CostCenterStatusTab.AmountCr ?? 0,
                                       p.DocTypeId,
                                       p.StartDate,
                                       p.Status,
                                       p.CloseDate,
                                       p.ModifiedBy,
                                       p.ModifiedDate,
                                       josTab.RetensionCostCenter,

                                   }).ToList();

                int? ProcessId = null;
                var Process = db.Process.Where(m => m.ProcessName == ProcessConstants.Weaving).FirstOrDefault();
                if (Process != null)
                    ProcessId = Process.ProcessId;

                int itemcount = 0;
                int ROitemcount = 0;
                string DocNo = "";
                string TempDocNo = "";


                foreach (var item in CostCenters)
                {

                    itemcount = itemcount + 1;
                    int PurjaAmountTransferDocTypeId = 717;
                    int LedgerAccountId = (int)item.LedgerAccountId;
                    LedgerHeader Header = new LedgerHeader();



                    Header.CreatedBy = User.Identity.Name;
                    Header.CreatedDate = DateTime.Now;
                    Header.DivisionId = (int)item.DivisionId;
                    Header.SiteId = (int)item.SiteId;
                    Header.DivisionId = (int)item.DivisionId;
                    Header.DocDate = CloseDate;
                    Header.PaymentFor = CloseDate;
                    Header.ProcessId = ProcessId;
                    //Header.DocDate = (DateTime)item.CloseDate;
                    //Header.PaymentFor = item.CloseDate;
                    Header.DocTypeId = PurjaAmountTransferDocTypeId;
                    Header.CostCenterId = item.CostCenterId;
                    // Header.DocHeaderId = StokHeader.StockHeaderId;
                    DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".LedgerHeaders", Header.DocTypeId, Header.DocDate, Header.DivisionId, Header.SiteId); 


                    TempDocNo = DocNo.Substring(0, 2) + "-" + (Convert.ToInt32(DocNo.Substring(DocNo.IndexOf("-") + 1)) + itemcount - 1).ToString().PadLeft(4, '0').ToString();
                    DocNo = TempDocNo;



                    Header.LedgerHeaderId = -itemcount;
                    Header.DocNo = DocNo;
                    Header.LedgerAccountId = LedgerAccountId;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                    //Header.Remark = StokHeader.Remark;
                    Header.Status = (int)StatusConstants.Locked;
                    Header.ObjectState = Model.ObjectState.Added;
                    db.LedgerHeader.Add(Header);


                    decimal Amount = Math.Round((decimal)item.AmountDr - (decimal)item.AmountCr, 0, MidpointRounding.AwayFromZero);
                    decimal RoundOffAmount = Amount - ((decimal)item.AmountDr - (decimal)item.AmountCr);

                    if (Amount != 0)
                    {

                        //Postingcontra LedgerRecord;;
                        LedgerLine Ledgerline = new LedgerLine();
                        Ledgerline.LedgerHeaderId = Header.LedgerHeaderId;
                        Ledgerline.LedgerAccountId = LedgerAccountId;
                        Ledgerline.Amount = Amount;
                        Ledgerline.CostCenterId = item.RetensionCostCenter;
                        Ledgerline.CreatedDate = DateTime.Now;
                        Ledgerline.ModifiedDate = DateTime.Now;
                        Ledgerline.CreatedBy = User.Identity.Name;
                        //Ledgerline.Remark = svm.Remark;
                        Ledgerline.ModifiedBy = User.Identity.Name;
                        Ledgerline.ObjectState = Model.ObjectState.Added;


                        db.LedgerLine.Add(Ledgerline);


                        //Postingcontra LedgerRecord;;
                        Ledger Ledger = new Ledger();

                        Ledger.AmtDr = Amount < 0 ? Math.Abs(Amount) : 0;
                        Ledger.AmtCr = Amount < 0 ? 0 : Math.Abs(Amount);
                        Ledger.LedgerHeaderId = Header.LedgerHeaderId;
                        Ledger.LedgerAccountId = LedgerAccountId;
                        Ledger.CostCenterId = item.CostCenterId;
                        Ledger.ContraLedgerAccountId = LedgerAccountId;
                        Ledger.ObjectState = Model.ObjectState.Added;
                        db.Ledger.Add(Ledger);
                        //new LedgerService(_unitOfWork).Create(Ledger);



                        //Postingcontra LedgerRecord;;
                        Ledger ContraLedger = new Ledger();
                        ContraLedger.AmtCr = Amount;
                        ContraLedger.AmtDr = Amount < 0 ? 0 : Math.Abs(Amount);
                        ContraLedger.AmtCr = Amount < 0 ? Math.Abs(Amount) : 0;
                        ContraLedger.LedgerHeaderId = Header.LedgerHeaderId;
                        ContraLedger.LedgerAccountId = LedgerAccountId;
                        ContraLedger.CostCenterId = item.RetensionCostCenter;
                        ContraLedger.ContraLedgerAccountId = LedgerAccountId;
                        ContraLedger.ObjectState = Model.ObjectState.Added;
                        //new LedgerService(_unitOfWork).Create(ContraLedger);
                        db.Ledger.Add(ContraLedger);

                    }

                    

                    if (RoundOffAmount != 0)
                    {
                        ROitemcount = ROitemcount + 1;

                        LedgerHeader ROLHeader = new LedgerHeader();

                        int RODocType = 0;
                        int RoundOffAccountId = 6660;

                        if (RoundOffAmount > 0)
                            RODocType = 785;//dr
                        else
                            RODocType = 786;//cr


                        ROLHeader.CreatedBy = User.Identity.Name;
                        ROLHeader.CreatedDate = DateTime.Now;
                        ROLHeader.DivisionId = (int)item.DivisionId;
                        ROLHeader.SiteId = (int)item.SiteId;
                        ROLHeader.DivisionId = (int)item.DivisionId;
                        ROLHeader.DocDate = CloseDate;
                        ROLHeader.PaymentFor = CloseDate;
                        ROLHeader.ProcessId = ProcessId;
                        ROLHeader.DocTypeId = RODocType;
                        ROLHeader.CostCenterId = item.CostCenterId;
                        DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".LedgerHeaders", ROLHeader.DocTypeId, ROLHeader.DocDate, ROLHeader.DivisionId, ROLHeader.SiteId); ;


                        TempDocNo = DocNo.Substring(0, 2) + "-" + (Convert.ToInt32(DocNo.Substring(DocNo.IndexOf("-") + 1)) + ROitemcount - 1).ToString().PadLeft(4, '0').ToString();
                        DocNo = TempDocNo;

                        ROLHeader.LedgerHeaderId = +ROitemcount;
                        ROLHeader.DocNo = DocNo;
                        ROLHeader.LedgerAccountId = RoundOffAccountId;
                        ROLHeader.ModifiedBy = User.Identity.Name;
                        ROLHeader.ModifiedDate = DateTime.Now;
                        ROLHeader.Remark = "On Cost Center Closing";
                        ROLHeader.Status = (int)StatusConstants.Locked;
                        ROLHeader.ObjectState = Model.ObjectState.Added;

                        db.LedgerHeader.Add(ROLHeader);



                        //Postingcontra LedgerRecord;;
                        LedgerLine Ledgerline1 = new LedgerLine();
                        Ledgerline1.LedgerHeaderId = ROLHeader.LedgerHeaderId;
                        Ledgerline1.LedgerAccountId = LedgerAccountId;
                        Ledgerline1.Amount = Math.Abs(RoundOffAmount);
                        Ledgerline1.CostCenterId = item.CostCenterId;
                        Ledgerline1.CreatedDate = DateTime.Now;
                        Ledgerline1.ModifiedDate = DateTime.Now;
                        Ledgerline1.CreatedBy = User.Identity.Name;
                        //Ledgerline.Remark = svm.Remark;
                        Ledgerline1.ModifiedBy = User.Identity.Name;
                        Ledgerline1.ObjectState = Model.ObjectState.Added;


                        db.LedgerLine.Add(Ledgerline1);


                        #region CSEUpdate
                        var CSE = db.CostCenterStatusExtended.Find(item.CostCenterId);

                        if (RODocType == 785)
                        {
                            CSE.DebitAmount = (CSE.DebitAmount ?? 0) + Math.Abs(RoundOffAmount);
                        }
                        else if (RODocType == 786)
                        {
                            CSE.CreditAmount = (CSE.CreditAmount ?? 0) + Math.Abs(RoundOffAmount);
                        }

                        CSE.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatusExtended.Add(CSE);
                        #endregion



                        //Postingcontra LedgerRecord;;
                        Ledger Ledger1 = new Ledger();



                        Ledger1.AmtDr = RoundOffAmount > 0 ? Math.Abs(RoundOffAmount) : 0;
                        Ledger1.AmtCr = RoundOffAmount < 0 ? Math.Abs(RoundOffAmount) : 0;
                        Ledger1.LedgerHeaderId = ROLHeader.LedgerHeaderId;
                        Ledger1.LedgerAccountId = LedgerAccountId;
                        Ledger1.CostCenterId = item.CostCenterId;
                        Ledger1.ContraLedgerAccountId = RoundOffAccountId;
                        Ledger1.ObjectState = Model.ObjectState.Added;
                        db.Ledger.Add(Ledger1);
                        //new LedgerService(_unitOfWork).Create(Ledger);



                        //Postingcontra LedgerRecord;;
                        Ledger ContraLedger1 = new Ledger();
                        ContraLedger1.AmtDr = RoundOffAmount < 0 ? Math.Abs(RoundOffAmount) : 0;
                        ContraLedger1.AmtCr = RoundOffAmount > 0 ? Math.Abs(RoundOffAmount) : 0;
                        ContraLedger1.LedgerHeaderId = ROLHeader.LedgerHeaderId;
                        ContraLedger1.LedgerAccountId = RoundOffAccountId;
                        ContraLedger1.ContraLedgerAccountId = LedgerAccountId;
                        ContraLedger1.ObjectState = Model.ObjectState.Added;
                        //new LedgerService(_unitOfWork).Create(ContraLedger);
                        db.Ledger.Add(ContraLedger1);



                    }


                }




                // Start For Old RetentionTrf 
                foreach (var item in CostCenters)
                {
                    IQueryable<JobOrderHeaderViewModel> p = _JobOrderHeaderService.GetJobOrderHeaderListByCostCenter(item.CostCenterId);

                    foreach (var V1 in p)
                    {
                        JobOrderHeader joborder = db.JobOrderHeader.Find(V1.JobOrderHeaderId);
                        joborder.Status = (int)StatusConstants.Closed;
                        joborder.ModifiedBy = User.Identity.Name;
                        joborder.ModifiedDate = DateTime.Now;
                        joborder.ObjectState = Model.ObjectState.Modified;
                        db.JobOrderHeader.Add(joborder);


                        #region CSEUpdate
                        var JOHS = db.JobOrderHeaderStatus.Find(V1.JobOrderHeaderId);                        
                        JOHS.Status = (int)StatusConstants.Closed;
                        JOHS.ObjectState = Model.ObjectState.Modified;
                        db.JobOrderHeaderStatus.Add(JOHS);
                        #endregion

                    }

                    CostCenter costcenter = db.CostCenter.Find(item.CostCenterId);

                    costcenter.Status = (int)StatusConstants.Closed;
                    costcenter.CloseDate = CloseDate;
                    costcenter.IsActive = false;
                    costcenter.ModifiedBy = User.Identity.Name;
                    costcenter.ModifiedDate = DateTime.Now;
                    costcenter.ObjectState = Model.ObjectState.Modified;
                    db.CostCenter.Add(costcenter);


                    LogList.Add(new LogTypeViewModel { ExObj = costcenter, Obj = costcenter });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                    ActivityLog Log = new ActivityLog()
                    {
                        ActivityType = (int)ActivityTypeContants.Closed,
                        CreatedBy = User.Identity.Name,
                        CreatedDate = DateTime.Now,
                        DocId = item.CostCenterId,
                        DocTypeId = item.DocTypeId,
                        Modifications = Modifications.ToString(),
                        Narration = "",
                    };

                    Log.ObjectState = Model.ObjectState.Added;
                    db.ActivityLog.Add(Log);
                }

                // End  For Old RetentionTrf 
                try
                {
                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return Json(new { Success = false });
                }


                return Json(new { Success = true });
            }

            //PrepareViewBag(svm.DocTypeId);
            //ViewBag.Mode = "Add";
            //return View("Create", svm);
            return View("CloseCostCenters");

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

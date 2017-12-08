using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Model.ViewModels;
using Service;
using Jobs.Helpers;
using Data.Infrastructure;
using System.Configuration;
using Presentation;
using Model.ViewModel;
using CustomEventArgs;
using DocumentEvents;
using LedgerDocumentEvents;



namespace Jobs.Controllers
{
    [Authorize]
    public class PaymentCancelWizardController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

        private bool EventException = false;

        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public PaymentCancelWizardController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _exception = exec;
            _unitOfWork = unitOfWork;
            //if (!JobInvoiceAmendmentEvents.Initialized)
            //{
            //    JobInvoiceAmendmentEvents Obj = new JobInvoiceAmendmentEvents();
            //}
        }

        public void PrepareViewBag(int id)
        {
            DocumentType DocType = new DocumentTypeService(_unitOfWork).Find(id);
            ViewBag.Name = DocType.DocumentTypeName;
            ViewBag.id = id;
            ViewBag.ReasonList = new ReasonService(_unitOfWork).GetReasonList(DocType.DocumentTypeName).ToList();

        }
        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("PaymentCancelWizard", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        public ActionResult PaymentCancelWizard(int id)//DocumentTypeId
        {
            PrepareViewBag(id);
            LedgerHeaderViewModel vm = new LedgerHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //Getting Settings
            var settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateForCancel", "LedgerSetting", new { id = id }).Warning("Please create payment cancel settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            ViewBag.ProcessId = settings.ProcessId;
            return View();
        }

        public JsonResult AjaxGetJsonData(int DocType, int? ProcessId, DateTime? FromDate, DateTime? ToDate, string LedgerHeaderId, string LedgerAcCr, string LedgerAcDr, string ChqNo, string CostCenter, decimal? Amount)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            string SortColName = "";

            // note: we only sort one column at a time
            if (Request.Form["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.Form["order[0][column]"]);
                SortColName = Request.Form["columns[" + sortColumn + "][data]"];
            }
            if (Request.Form["order[0][dir]"] != null)
            {
                sortDirection = Request.Form["order[0][dir]"];
            }

            //DataTableData dataTableData = new DataTableData();
            //dataTableData.draw = draw;
            //int recordsFiltered = 0;
            //dataTableData.data = FilterData(ref recordsFiltered, ref TOTAL_ROWS, start, length, search, sortColumn, sortDirection, DocType, ProcessId, FromDate, ToDate, LedgerHeaderId, LedgerAcCr, LedgerAcDr, ChqNo, CostCenter, Amount);
            //dataTableData.recordsTotal = TOTAL_ROWS;
            //dataTableData.recordsFiltered = recordsFiltered;

            //return Json(dataTableData, JsonRequestBehavior.AllowGet);

            bool Success = true;

            var data = FilterData(search, sortColumn, sortDirection, DocType, ProcessId, FromDate, ToDate, LedgerHeaderId, LedgerAcCr, LedgerAcDr, ChqNo, CostCenter, Amount);

            var RecCount = data.Count();

            if (RecCount > 1000 || RecCount == 0)
            {
                Success = false;
                return Json(new { Success = Success, Message = (RecCount > 1000 ? "No of records exceeding 1000." : "No Records found.") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CList = data.ToList().Select(m => new PaymentCancelWizardViewModel
                {
                    LedgerLineId = m.LedgerLineId,
                    LedgerHeaderId = m.LedgerHeaderId,
                    LedgerAcCr = m.LedgerAcCr,
                    LedgerAcDr = m.LedgerAcDr,
                    ChqNo = m.ChqNo,
                    Amount = m.Amount,
                    CostCenterName = m.CostCenterName,
                    DocNo = m.DocNo,
                    SDocDate = m.DocDate.HasValue ? m.DocDate.Value.ToString("dd/MMM/yyyy") : "",
                    Remark = m.Remark,
                    DocTypeName = m.DocTypeName,
                }).ToList();

                return Json(new { Data = CList, Success = Success }, JsonRequestBehavior.AllowGet);
            }
        }


        private static int TOTAL_ROWS = 0;
        //private static readonly List<DataItem> _data = CreateData();    
        public class DataTableData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<PaymentCancelWizardViewModel> data { get; set; }
        }

        private int SortString(string s1, string s2, string sortDirection)
        {
            return sortDirection == "asc" ? s1.CompareTo(s2) : s2.CompareTo(s1);
        }

        private int SortInteger(string s1, string s2, string sortDirection)
        {
            int i1 = int.Parse(s1);
            int i2 = int.Parse(s2);
            return sortDirection == "asc" ? i1.CompareTo(i2) : i2.CompareTo(i1);
        }

        private int SortDateTime(string s1, string s2, string sortDirection)
        {
            DateTime d1 = DateTime.Parse(s1);
            DateTime d2 = DateTime.Parse(s2);
            return sortDirection == "asc" ? d1.CompareTo(d2) : d2.CompareTo(d1);
        }

        // here we simulate SQL search, sorting and paging operations
        private IQueryable<PaymentCancelWizardViewModel> FilterData(string search, int sortColumn, string sortDirection, int DocType
            , int? ProcessId, DateTime? FromDate, DateTime? ToDate, string LedgerHeaderId, string LedgerAcCr, string LedgerAcDr, string ChqNo, string CostCenter, decimal? Amount)
        {

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var Settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(DocType, DivisionId, SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { contraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            List<int> LedgerHeaderIds = new List<int>();
            if (!string.IsNullOrEmpty(LedgerHeaderId))
                foreach (var item in LedgerHeaderId.Split(','))
                    LedgerHeaderIds.Add(Convert.ToInt32(item));

            List<int> LedgerAcCrIds = new List<int>();
            if (!string.IsNullOrEmpty(LedgerAcCr))
                foreach (var item in LedgerAcCr.Split(','))
                    LedgerAcCrIds.Add(Convert.ToInt32(item));

            List<int> LedgerAcDrIds = new List<int>();
            if (!string.IsNullOrEmpty(LedgerAcDr))
                foreach (var item in LedgerAcDr.Split(','))
                    LedgerAcDrIds.Add(Convert.ToInt32(item));


            IQueryable<PaymentCancelWizardViewModel> _data = from p in db.LedgerLine
                                                             join ledger in db.Ledger on new { x = p.LedgerLineId, y = p.LedgerAccountId } equals new { x = ledger.LedgerLineId ?? 0, y = ledger.LedgerAccountId } into lgr
                                                             from lgrtab in lgr.DefaultIfEmpty()
                                                             join la in db.LedgerAdj on lgrtab.LedgerId equals la.CrLedgerId into ladjcr
                                                             from ladjtabcr in ladjcr.DefaultIfEmpty()
                                                             join lad in db.LedgerAdj on lgrtab.LedgerId equals lad.DrLedgerId into ladjDr
                                                             from ladjtabdr in ladjDr.DefaultIfEmpty()
                                                             join tab2 in db.LedgerHeader on p.LedgerHeaderId equals tab2.LedgerHeaderId
                                                             where (string.IsNullOrEmpty(Settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(tab2.DocTypeId.ToString())) && tab2.SiteId == SiteId && tab2.DivisionId == DivisionId && ladjtabcr == null && ladjtabdr == null
                                                             && (Settings.ProcessId.HasValue ? tab2.ProcessId == Settings.ProcessId : 1 == 1)
                                                             select new PaymentCancelWizardViewModel
                                                             {
                                                                 LedgerLineId = p.LedgerLineId,
                                                                 LedgerHeaderId = p.LedgerHeaderId,
                                                                 LedgerAcCrId = p.LedgerAccountId,
                                                                 LedgerAcDrId = tab2.LedgerAccountId,
                                                                 LedgerAcCr = p.LedgerAccount.LedgerAccountName,
                                                                 LedgerAcDr = tab2.LedgerAccount.LedgerAccountName,
                                                                 ChqNo = p.ChqNo,
                                                                 Amount = p.Amount,
                                                                 CostCenterName = p.CostCenter.CostCenterName,
                                                                 DocNo = tab2.DocNo,
                                                                 DocDate = tab2.DocDate,
                                                                 Remark = p.Remark,
                                                                 DocTypeName = tab2.DocType.DocumentTypeName,
                                                             };




            DateTime Date;
            bool Success = DateTime.TryParse(search, out Date);

            List<PaymentCancelWizardViewModel> list = new List<PaymentCancelWizardViewModel>();
            if (string.IsNullOrEmpty(search))
            {

            }
            else
            {

                // simulate search

                //if (!Success)
                //{
                //    _data = from m in _data
                //            where (m.DocNo + m.Amount + m.ChqNo + m.CostCenterName + m.LedgerAcCr + m.LedgerAcDr).ToString().ToLower().Contains(search.ToLower())
                //            select m;
                //}
                //else
                //{
                //    _data = _data.Where(m => m.DocDate == Date);
                //}

            }



            if (FromDate.HasValue)
                _data = _data.Where(m => m.DocDate >= FromDate);

            if (ToDate.HasValue)
                _data = _data.Where(m => m.DocDate <= ToDate);

            if (Amount.HasValue && Amount.Value > 0)
                _data = _data.Where(m => m.Amount == Amount.Value);

            if (!string.IsNullOrEmpty(LedgerHeaderId))
                _data = _data.Where(m => LedgerHeaderIds.Contains(m.LedgerHeaderId));

            if (!string.IsNullOrEmpty(LedgerAcCr))
                _data = _data.Where(m => LedgerAcCrIds.Contains(m.LedgerAcCrId ?? 0));

            if (!string.IsNullOrEmpty(LedgerAcDr))
                _data = _data.Where(m => LedgerAcDrIds.Contains(m.LedgerAcDrId ?? 0));

            if (!string.IsNullOrEmpty(ChqNo))
                _data = _data.Where(m => m.ChqNo.Contains(ChqNo));


            // simulate sort
            //if (sortColumn == 0)
            //{// sort Name
            _data = _data.OrderByDescending(m => m.DocDate).ThenByDescending(m => m.DocNo);
            //}
            //else if (sortColumn == 1)
            //{// sort Age
            //    _data = sortDirection == "asc" ? _data.OrderBy(m => m.DocNo) : _data.OrderByDescending(m => m.DocNo);
            //}
            //else if (sortColumn == 2)
            //{   // sort DoB
            //    _data = sortDirection == "asc" ? _data.OrderBy(m => m.DocDate) : _data.OrderByDescending(m => m.DocDate);
            //}
            //else if (sortColumn == 3)
            //{   // sort DoB
            //    _data = sortDirection == "asc" ? _data.OrderBy(m => m.DueDate) : _data.OrderByDescending(m => m.DueDate);
            //}
            // get just one page of data



            return _data.Select(m => new PaymentCancelWizardViewModel
            {
                LedgerLineId = m.LedgerLineId,
                LedgerHeaderId = m.LedgerHeaderId,
                LedgerAcCrId = m.LedgerAcCrId,
                LedgerAcDrId = m.LedgerAcDrId,
                LedgerAcCr = m.LedgerAcCr,
                LedgerAcDr = m.LedgerAcDr,
                ChqNo = m.ChqNo,
                Amount = m.Amount,
                CostCenterName = m.CostCenterName,
                DocNo = m.DocNo,
                DocDate = m.DocDate,
                Remark = m.Remark,
                DocTypeName = m.DocTypeName,
            });

        }

        public ActionResult ConfirmedJobInvoices(List<PaymentCancelWizardViewModel> ConfirmedList, int DocTypeId, string UserRemark)
        {


            if (ConfirmedList.Count() > 0 && ConfirmedList.GroupBy(m => m.LedgerHeaderId).Count() > 1)
                return Json(new { Success = false, Data = " Multiple Headers are selected. " }, JsonRequestBehavior.AllowGet);
            else
            {

                int LedgerHeaderId = ConfirmedList.GroupBy(m => m.LedgerHeaderId).FirstOrDefault().Key;

                var OrderIds = ConfirmedList.Select(m => m.LedgerLineId).ToArray();

                var RecordDetails = (from p in db.LedgerLine
                                     where OrderIds.Contains(p.LedgerLineId)
                                     select p).ToList();

                var Ledgers = (from p in db.Ledger
                               where OrderIds.Contains(p.LedgerLineId ?? 0)
                               select p).ToList();
                DocumentType Type = db.DocumentType.Find(DocTypeId);

                string Nature = Type.Nature;

                var LedgerHead = db.LedgerHeader.Find(LedgerHeaderId);

                LedgerHeader Header = new LedgerHeader();
                Header.AdjustmentType = LedgerHead.AdjustmentType;
                Header.CostCenterId = LedgerHead.CostCenterId;
                Header.CreatedBy = User.Identity.Name;
                Header.CreatedDate = DateTime.Now;
                Header.CreditDays = LedgerHead.CreditDays;
                Header.DivisionId = LedgerHead.DivisionId;
                Header.SiteId = LedgerHead.SiteId;
                Header.DocDate = DateTime.Now;
                Header.DocTypeId = DocTypeId;
                Header.DocHeaderId = LedgerHead.DocHeaderId;
                Header.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".LedgerHeaders", Header.DocTypeId, Header.DocDate, Header.DivisionId, Header.SiteId);
                Header.LedgerAccountId = LedgerHead.LedgerAccountId;
                Header.LockReason = LedgerHead.LockReason;
                Header.ModifiedBy = User.Identity.Name;
                Header.ModifiedDate = DateTime.Now;
                Header.Narration = LedgerHead.Narration;
                Header.PaymentFor = LedgerHead.PaymentFor;
                Header.ProcessId = LedgerHead.ProcessId;
                Header.Remark = UserRemark;
                Header.ObjectState = Model.ObjectState.Added;
                db.LedgerHeader.Add(Header);

                int pk = 1;
                int pk1 = 1;

                foreach (var item in RecordDetails)
                {
                    LedgerLine Line = new LedgerLine();

                    Line.Amount = item.Amount;
                    Line.BaseRate = item.BaseRate;
                    Line.BaseValue = item.BaseValue;
                    Line.ChqDate = item.ChqDate;
                    Line.ChqNo = item.ChqNo;
                    Line.CostCenterId = item.CostCenterId;
                    Line.CreatedBy = User.Identity.Name;
                    Line.CreatedDate = DateTime.Now;
                    Line.LedgerAccountId = item.LedgerAccountId;
                    Line.LedgerHeaderId = Header.LedgerHeaderId;
                    Line.ModifiedBy = User.Identity.Name;
                    Line.ModifiedDate = DateTime.Now;
                    Line.ReferenceDocId = item.LedgerHeaderId;
                    Line.ReferenceDocLineId = item.LedgerLineId;
                    Line.ReferenceDocTypeId = LedgerHead.DocTypeId;
                    Line.ReferenceId = Ledgers.Where(m => m.LedgerLineId == item.LedgerLineId && m.LedgerAccountId == item.LedgerAccountId).FirstOrDefault().LedgerId;
                    Line.Remark = UserRemark;
                    Line.LedgerLineId = pk;
                    Line.ObjectState = Model.ObjectState.Added;

                    db.LedgerLine.Add(Line);


                    #region LedgerSave
                    Ledger Ledger = new Ledger();

                    if (Nature == NatureConstants.Credit)
                    {
                        Ledger.AmtDr = Line.Amount;

                    }
                    else if (Nature == NatureConstants.Debit)
                    {
                        Ledger.AmtCr = Line.Amount;
                    }
                    Ledger.ChqNo = Line.ChqNo;
                    Ledger.ContraLedgerAccountId = Header.LedgerAccountId;
                    Ledger.CostCenterId = Line.CostCenterId;
                    Ledger.DueDate = Line.ChqDate;
                    Ledger.LedgerAccountId = Line.LedgerAccountId;
                    Ledger.LedgerHeaderId = Line.LedgerHeaderId;
                    Ledger.LedgerLineId = Line.LedgerLineId;
                    Ledger.Narration = Header.Narration + Line.Remark;
                    Ledger.ObjectState = Model.ObjectState.Added;
                    Ledger.LedgerId = pk;
                    db.Ledger.Add(Ledger);

                    if (Line.ReferenceId != null)
                    {
                        LedgerAdj LedgerAdj = new LedgerAdj();
                        if (Nature == NatureConstants.Credit)
                        {
                            LedgerAdj.DrLedgerId = Ledger.LedgerId;
                            //LedgerAdj.CrLedgerId = (int)Line.ReferenceId;
                            LedgerAdj.LedgerId = (int)Line.ReferenceId;
                        }
                        else
                        {
                            //LedgerAdj.DrLedgerId = (int)Line.ReferenceId;
                            LedgerAdj.LedgerId = (int)Line.ReferenceId;
                            LedgerAdj.CrLedgerId = Ledger.LedgerId;
                        }

                        LedgerAdj.LedgerAdjId  = pk1;
                        LedgerAdj.Amount = Line.Amount;
                        LedgerAdj.SiteId = Header.SiteId;
                        LedgerAdj.CreatedDate = DateTime.Now;
                        LedgerAdj.ModifiedDate = DateTime.Now;
                        LedgerAdj.CreatedBy = User.Identity.Name;
                        LedgerAdj.ModifiedBy = User.Identity.Name;
                        LedgerAdj.ObjectState = Model.ObjectState.Added;
                        db.LedgerAdj.Add(LedgerAdj);
                    }
                    #endregion


                    #region ContraLedgerSave
                    Ledger ContraLedger = new Ledger();
                    if (Nature == NatureConstants.Credit)
                    {
                        ContraLedger.AmtCr = Line.Amount;

                    }
                    else if (Nature == NatureConstants.Debit)
                    {
                        ContraLedger.AmtDr = Line.Amount;
                    }
                    ContraLedger.LedgerHeaderId = Header.LedgerHeaderId;
                    ContraLedger.CostCenterId = Header.CostCenterId;
                    ContraLedger.LedgerLineId = Line.LedgerLineId;
                    ContraLedger.LedgerAccountId = Header.LedgerAccountId.Value;
                    ContraLedger.ContraLedgerAccountId = Line.LedgerAccountId;
                    ContraLedger.ObjectState = Model.ObjectState.Added;
                    db.Ledger.Add(ContraLedger);
                    #endregion

                    pk++;
                    pk1++;

                }

                try
                {
                    LedgerDocEvents.onWizardSaveEvent(this, new LedgerEventArgs(Header.LedgerHeaderId, EventModeConstants.Add), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }


                try
                {
                    if (EventException)
                    { throw new Exception((string)TempData["CSEXC"]); }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    return Json(new { Success = false, Data = message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Success = "URL", Data = "/LedgerHeader/Submit/" + Header.LedgerHeaderId }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Filters(int DocTypeId, int? ProcessId, DateTime? FromDate, DateTime? ToDate, string LedgerHeaderId, string LedgerAcCr, string LedgerAcDr, string ChqNo, string CostCenter, decimal? Amount)
        {
            PaymentCancelWizardFilterViewModel vm = new PaymentCancelWizardFilterViewModel();
            vm.DocTypeId = DocTypeId;
            vm.ProcessId = ProcessId;
            vm.FromDate = FromDate;
            vm.ToDate = ToDate;
            vm.LedgerHeaderId = LedgerHeaderId;
            vm.LedgerAcCr = LedgerAcCr;
            vm.LedgerAcDr = LedgerAcDr;
            vm.ChqNo = ChqNo;
            vm.CostCenter = CostCenter;
            vm.Amount = Amount;
            return PartialView("_Filters", vm);
        }


        public JsonResult GetPendingLedgersHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var Settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(filter, DivisionId, SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { contraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }



            var Records = from p in db.LedgerLine
                          join ledger in db.Ledger on new { x = p.LedgerLineId, y = p.LedgerAccountId } equals new { x = ledger.LedgerLineId ?? 0, y = ledger.LedgerAccountId } into lgr
                          from lgrtab in lgr.DefaultIfEmpty()
                          join la in db.LedgerAdj on lgrtab.LedgerId equals la.CrLedgerId into ladjcr
                          from ladjtabcr in ladjcr.DefaultIfEmpty()
                          join lad in db.LedgerAdj on lgrtab.LedgerId equals lad.DrLedgerId into ladjDr
                          from ladjtabdr in ladjDr.DefaultIfEmpty()
                          join tab2 in db.LedgerHeader on p.LedgerHeaderId equals tab2.LedgerHeaderId
                          where (string.IsNullOrEmpty(Settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(tab2.DocTypeId.ToString())) && tab2.SiteId == SiteId && tab2.DivisionId == DivisionId && ladjtabcr == null && ladjtabdr == null
                          && (Settings.ProcessId.HasValue ? tab2.ProcessId == Settings.ProcessId : 1 == 1)
                          select new ComboBoxResult
                          {
                              id = tab2.LedgerHeaderId.ToString(),
                              text = tab2.DocNo,
                          };

            var temp = Records.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Records.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetPendingCostCenterHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var Settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(filter, DivisionId, SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { contraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }



            var Records = from p in db.LedgerLine
                          join ledger in db.Ledger on new { x = p.LedgerLineId, y = p.LedgerAccountId } equals new { x = ledger.LedgerLineId ?? 0, y = ledger.LedgerAccountId } into lgr
                          from lgrtab in lgr.DefaultIfEmpty()
                          join la in db.LedgerAdj on lgrtab.LedgerId equals la.CrLedgerId into ladjcr
                          from ladjtabcr in ladjcr.DefaultIfEmpty()
                          join lad in db.LedgerAdj on lgrtab.LedgerId equals lad.DrLedgerId into ladjDr
                          from ladjtabdr in ladjDr.DefaultIfEmpty()
                          join tab2 in db.LedgerHeader on p.LedgerHeaderId equals tab2.LedgerHeaderId
                          where (string.IsNullOrEmpty(Settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(tab2.DocTypeId.ToString())) && tab2.SiteId == SiteId && tab2.DivisionId == DivisionId && ladjtabcr == null && ladjtabdr == null
                          && tab2.ProcessId == Settings.ProcessId
                          select new ComboBoxResult
                          {
                              id = p.CostCenterId.ToString(),
                              text = p.CostCenter.CostCenterName,
                          };

            var temp = Records.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Records.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetPendingProductHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            var Records = new JobInvoiceRateAmendmentLineService(_unitOfWork).GetPendingProductHelpList(filter, searchTerm);

            var temp = Records.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Records.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

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

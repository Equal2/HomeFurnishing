using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.SqlServer;
using Models.BasicSetup.Models;
using Models.BasicSetup.ViewModels;
using Infrastructure.IO;
using Models.Company.Models;
using ProjLib.Constants;

namespace Services.BasicSetup
{
    public interface ILedgerHeaderService : IDisposable
    {
        LedgerHeader Create(LedgerHeader s);
        void Delete(int id);
        void Delete(LedgerHeader s);
        LedgerHeader GetLedgerHeader(int id);
        IQueryable<LedgerHeaderViewModel> GetLedgerHeaderList(int id, string Uname);//Document CategoryId
        IQueryable<LedgerHeaderViewModel> GetLedgerHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<LedgerHeaderViewModel> GetLedgerHeaderListPendingToReview(int id, string Uname);
        LedgerHeaderViewModel GetLedgerHeaderVm(int id);//HeaderID
        LedgerHeader Find(int id);
        void Update(LedgerHeader s);
        string GetMaxDocNo();
        LedgerHeader FindByDocNo(string Docno);
        int NextId(int id);
        int PrevId(int id);
        LedgerHeader FindByDocHeader(int? DocHeaderId, int DocTypeId, int SiteId, int DivisionId);
    }
    public class LedgerHeaderService : ILedgerHeaderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<LedgerHeader> _ledgerHeaderRepository;

        public LedgerHeaderService(IUnitOfWork unit, IRepository<LedgerHeader> ledgerHeaderRepo)
        {
            _unitOfWork = unit;
            _ledgerHeaderRepository = ledgerHeaderRepo;
        }
        public LedgerHeaderService(IUnitOfWork unit)
        {
            _unitOfWork = unit;
            _ledgerHeaderRepository = unit.Repository<LedgerHeader>();
        }
        public LedgerHeader Create(LedgerHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _ledgerHeaderRepository.Insert(s);
            return s;
        }
        public LedgerHeaderViewModel GetLedgerHeaderVm(int id)
        {
            return (from p in _ledgerHeaderRepository.Instance
                    join t in _unitOfWork.Repository<DocumentType>().Instance on p.DocTypeId equals t.DocumentTypeId into table
                    from tab in table.DefaultIfEmpty()
                    where p.LedgerHeaderId == id
                    select new LedgerHeaderViewModel
                    {
                        CreditDays = p.CreditDays,
                        DivisionId = p.DivisionId,
                        DocDate = p.DocDate,
                        PaymentFor = p.PaymentFor,
                        DocHeaderId = p.DocHeaderId,
                        DocNo = p.DocNo,
                        DocTypeId = p.DocTypeId,
                        LedgerAccountId = p.LedgerAccountId,
                        LedgerHeaderId = p.LedgerHeaderId,
                        CostCenterId = p.CostCenterId,
                        Narration = p.Narration,
                        Remark = p.Remark,
                        SiteId = p.SiteId,
                        Status = p.Status,
                        AdjustmentType = p.AdjustmentType,
                        DocumentCategoryId = tab.DocumentCategoryId,
                        ProcessId = p.ProcessId,
                        GodownId = p.GodownId,
                        ModifiedBy = p.ModifiedBy,
                        CreatedDate = p.CreatedDate,
                        LockReason = p.LockReason,

                    }
                        ).FirstOrDefault();
        }
        public IQueryable<LedgerHeaderViewModel> GetLedgerHeaderList(int id, string Uname)//Document Category Id
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in _ledgerHeaderRepository.Instance
                    join t in _unitOfWork.Repository<LedgerLine>().Instance on p.LedgerHeaderId equals t.LedgerHeaderId into ProdTable
                    from ProTab in ProdTable.DefaultIfEmpty()
                    where p.DocTypeId == id && p.SiteId == SiteId && p.DivisionId == DivisionId
                    group new { p, ProTab } by p.LedgerHeaderId into g
                    orderby g.Key descending
                    select new LedgerHeaderViewModel
                    {
                        CreditDays = g.Max(m => m.p.CreditDays),
                        DivisionId = g.Max(m => m.p.DivisionId),
                        DocDate = g.Max(m => m.p.DocDate),
                        DocNo = g.Max(m => m.p.DocNo),
                        DocTypeName = g.Max(m => m.p.DocType.DocumentTypeName),
                        LedgerAccountName = g.Max(m => m.p.LedgerAccount.LedgerAccountName) + " {" + g.Max(m => m.p.LedgerAccount.LedgerAccountSuffix) + "}",
                        Narration = g.Max(m => m.p.Narration),
                        ModifiedBy = g.Max(m => m.p.ModifiedBy),
                        Remark = g.Max(m => m.p.Remark),
                        SiteName = g.Max(m => m.p.Site.SiteName),
                        Status = g.Max(m => m.p.Status),
                        LedgerHeaderId = g.Max(m => m.p.LedgerHeaderId),
                        AccountName = g.Max(m => m.ProTab.LedgerAccount.LedgerAccountName) + " {" + g.Max(m => m.ProTab.LedgerAccount.LedgerAccountSuffix) + "}",
                        ReviewCount = g.Max(m => m.p.ReviewCount),
                        ReviewBy = g.Max(m => m.p.ReviewBy),
                        Reviewed = (SqlFunctions.CharIndex(Uname, g.Max(m => m.p.ReviewBy)) > 0),
                    });
        }

        public void Delete(int id)
        {
            _ledgerHeaderRepository.Delete(id);
        }
        public void Delete(LedgerHeader s)
        {
            _ledgerHeaderRepository.Delete(s);
        }
        public void Update(LedgerHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _ledgerHeaderRepository.Update(s);
        }

        public LedgerHeader GetLedgerHeader(int id)
        {
            return _ledgerHeaderRepository.Query().Get().Where(m => m.LedgerHeaderId == id).FirstOrDefault();
        }

        public LedgerHeader Find(int id)
        {
            return _ledgerHeaderRepository.Find(id);
        }


        public LedgerHeader FindByDocNo(string Docno)
        {
            return _ledgerHeaderRepository.Query().Get().Where(m => m.DocNo == Docno).FirstOrDefault();

        }

        public LedgerHeader FindByDocHeader(int? DocHeaderId, int DocTypeId, int SiteId, int DivisionId)
        {
            return _ledgerHeaderRepository.Query().Get().Where(m => m.DocHeaderId == DocHeaderId && m.DocTypeId == DocTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId).FirstOrDefault();
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _ledgerHeaderRepository.Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }
        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _ledgerHeaderRepository.Instance
                        orderby p.DocDate descending, p.DocNo descending
                        select p.LedgerHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _ledgerHeaderRepository.Instance
                        orderby p.DocDate descending, p.DocNo descending
                        select p.LedgerHeaderId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in _ledgerHeaderRepository.Instance
                        orderby p.DocDate descending, p.DocNo descending
                        select p.LedgerHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _ledgerHeaderRepository.Instance
                        orderby p.DocDate descending, p.DocNo descending
                        select p.LedgerHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public IQueryable<LedgerHeaderViewModel> GetLedgerHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetLedgerHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<LedgerHeaderViewModel> GetLedgerHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetLedgerHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}

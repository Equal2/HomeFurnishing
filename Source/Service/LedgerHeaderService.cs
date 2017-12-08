using Data.Infrastructure;
using Data.Models;
using Model;
using Model.Models;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Common;
using Model.ViewModel;
using System.Data.Entity.SqlServer;

namespace Service
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
        string GetLedgerAccountType(int id);//LedgerAccount Id
        LedgerHeader FindByDocNo(string Docno);
        int NextId(int id);
        int PrevId(int id);
        //IQueryable<ComboBoxResult> GetLedgerAccountHelpList(int DocTypeId, string term);
    }
    public class LedgerHeaderService : ILedgerHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public LedgerHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

        public LedgerHeader Create(LedgerHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<LedgerHeader>().Insert(s);
            return s;
        }
        public LedgerHeaderViewModel GetLedgerHeaderVm(int id)
        {
            return (from p in db.LedgerHeader
                    join t in db.DocumentType on p.DocTypeId equals t.DocumentTypeId into table
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
                        DrCr = p.DrCr,
                        LedgerHeaderId = p.LedgerHeaderId,
                        CostCenterId = p.CostCenterId,
                        PartyDocNo = p.PartyDocNo,
                        PartyDocDate = p.PartyDocDate,
                        Narration = p.Narration,
                        Remark = p.Remark,
                        SiteId = p.SiteId,
                        Status = p.Status,
                        AdjustmentType = p.AdjustmentType,
                        DocumentCategoryId = tab.DocumentCategoryId,
                        ProcessId=p.ProcessId,
                        GodownId=p.GodownId,
                        ModifiedBy=p.ModifiedBy,
                        CreatedDate=p.CreatedDate,
                        LockReason=p.LockReason,

                    }
                        ).FirstOrDefault();
        }
        public string GetLedgerAccountType(int id)//LedgerAccountId
        {
            return (from p in db.LedgerAccount
                    join t in db.LedgerAccountGroup on p.LedgerAccountGroupId equals t.LedgerAccountGroupId into table
                    from tab in table.DefaultIfEmpty()
                    where p.LedgerAccountId == id
                    select tab.LedgerAccountType
                        ).FirstOrDefault();
        }
        public IQueryable<LedgerHeaderViewModel> GetLedgerHeaderList(int id, string Uname)//Document Category Id
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //return (from p in db.LedgerHeader
            //        where p.DocTypeId == id && p.SiteId == SiteId && p.DivisionId == DivisionId && (p.Status == 0 ? (p.ModifiedBy == Uname || UserRoles.Contains("Admin")) : 1 == 1)
            //        orderby p.DocDate descending, p.DocNo descending
            //        select new LedgerHeaderViewModel
            //        {
            //            CreditDays = p.CreditDays,
            //            DivisionId = p.DivisionId,
            //            DocDate = p.DocDate,
            //            DocNo=p.DocNo,
            //            DocTypeName = p.DocType.DocumentTypeName,
            //            LedgerAccountName = p.LedgerAccount.LedgerAccountName,
            //            Narration = p.Narration,
            //            ModifiedBy=p.ModifiedBy,
            //            Remark = p.Remark,
            //            SiteName = p.Site.SiteName,
            //            Status = p.Status,
            //            LedgerHeaderId=p.LedgerHeaderId,

            //        });



            return (from p in db.LedgerHeader
                    join t in db.LedgerLine on p.LedgerHeaderId equals t.LedgerHeaderId into ProdTable
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
                        //LedgerAccountName = g.Max(m => m.p.LedgerAccount.LedgerAccountName) + " {" + g.Max(m => m.p.LedgerAccount.LedgerAccountSuffix) + "}",
                        LedgerAccountName = g.Max(m => m.p.LedgerAccount.LedgerAccountName + ", " + (m.p.LedgerAccount.PersonId != null ? m.p.LedgerAccount.Person.Suffix + " [" + m.p.LedgerAccount.Person.Code + "]" : m.p.LedgerAccount.LedgerAccountSuffix)),
                        Narration = g.Max(m => m.p.Narration),
                        ModifiedBy = g.Max(m => m.p.ModifiedBy),
                        Remark = g.Max(m => m.p.Remark),
                        SiteName = g.Max(m => m.p.Site.SiteName),
                        Status = g.Max(m => m.p.Status),
                        LedgerHeaderId = g.Max(m => m.p.LedgerHeaderId),
                        //AccountName = g.Max(m => m.ProTab.LedgerAccount.LedgerAccountName) + " {" + g.Max(m => m.ProTab.LedgerAccount.LedgerAccountSuffix) + "}",
                        //AccountName = g.Max(m => m.ProTab.LedgerAccount.LedgerAccountName + "," + m.ProTab.LedgerAccount.LedgerAccountSuffix),
                        AccountName = g.Max(m => m.ProTab.LedgerAccount.LedgerAccountName + ", " + (m.ProTab.LedgerAccount.PersonId != null ? m.ProTab.LedgerAccount.Person.Suffix + " [" + m.ProTab.LedgerAccount.Person.Code + "]" : m.ProTab.LedgerAccount.LedgerAccountSuffix)),
                        ReviewCount = g.Max(m => m.p.ReviewCount),
                        ReviewBy = g.Max(m => m.p.ReviewBy),
                        Reviewed = (SqlFunctions.CharIndex(Uname, g.Max(m => m.p.ReviewBy)) > 0),
                        TotalAmount = g.Sum(m => m.ProTab.Amount),
                    });
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<LedgerHeader>().Delete(id);
        }
        public void Delete(LedgerHeader s)
        {
            _unitOfWork.Repository<LedgerHeader>().Delete(s);
        }
        public void Update(LedgerHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<LedgerHeader>().Update(s);
        }

        public LedgerHeader GetLedgerHeader(int id)
        {
            return _unitOfWork.Repository<LedgerHeader>().Query().Get().Where(m => m.LedgerHeaderId == id).FirstOrDefault();
        }

        public LedgerHeader Find(int id)
        {
            return _unitOfWork.Repository<LedgerHeader>().Find(id);
        }


        public LedgerHeader FindByDocNo(string Docno)
        {
            return _unitOfWork.Repository<LedgerHeader>().Query().Get().Where(m => m.DocNo == Docno).FirstOrDefault();

        }

        public LedgerHeader FindByDocHeader(int? DocHeaderId, int DocTypeId, int SiteId, int DivisionId)
        {
            return _unitOfWork.Repository<LedgerHeader>().Query().Get().Where(m => m.DocHeaderId == DocHeaderId && m.DocTypeId == DocTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId).FirstOrDefault();
        }

        public LedgerHeader FindByDocNoHeader(string DocNo, int DocTypeId, int SiteId, int DivisionId)
        {
            return _unitOfWork.Repository<LedgerHeader>().Query().Get().Where(m => m.DocNo == DocNo && m.DocTypeId == DocTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId).FirstOrDefault();
        }


        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<LedgerHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }
        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.LedgerHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.LedgerHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.LedgerHeader
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

                temp = (from p in db.LedgerHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.LedgerHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.LedgerHeader
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

        //public IQueryable<ComboBoxResult> GetEmployeeHelpListWithProcessFilter(int DocTypeId, string term)
        //{

        //    int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
        //    int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

        //    var Settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(DocTypeId, CurrentDivisionId, CurrentSiteId);

        //    int[] fitlterHeaderProcess = Settings.filterPersonProcessHeaders.Split(',').Select(Int32.Parse).ToArray();            

        //    string DivId = "|" + CurrentDivisionId.ToString() + "|";
        //    string SiteId = "|" + CurrentSiteId.ToString() + "|";

        //    var list = (from b in db.LedgerAccount
        //                join bus in db.BusinessEntity on b.PersonId equals bus.PersonID into BusinessEntityTable
        //                from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
        //                join p in db.Persons on b.PersonId equals p.PersonID into PersonTable
        //                from PersonTab in PersonTable.DefaultIfEmpty()
        //                join pp in db.PersonProcess on b.PersonId equals pp.PersonId into PersonProcessTable
        //                from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
        //                where fitlterHeaderProcess.Contains(PersonProcessTab.ProcessId)
        //                && (string.IsNullOrEmpty(term) ? 1 == 1 : PersonTab.Name.ToLower().Contains(term.ToLower()))
        //                && BusinessEntityTab.DivisionIds.IndexOf(DivId) != -1
        //                && BusinessEntityTab.SiteIds.IndexOf(SiteId) != -1
        //                orderby PersonTab.Name
        //                select new ComboBoxResult
        //                {
        //                    id = b.LedgerAccountId.ToString(),
        //                    text = b.LedgerAccountName
        //                }
        //      );

        //    return list;
        //}


        public void Dispose()
        {
        }
    }
}

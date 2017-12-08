using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;

using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Model.ViewModel;
using System.Data.Entity.SqlServer;
using Model.ViewModels;

namespace Service
{
    public interface ISaleOrderAmendmentHeaderService : IDisposable
    {
        SaleOrderAmendmentHeader Create(SaleOrderAmendmentHeader pt);
        void Delete(int id);
        void Delete(SaleOrderAmendmentHeader pt);
        SaleOrderAmendmentHeader Find(int id);
        IEnumerable<SaleOrderAmendmentHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleOrderAmendmentHeader pt);
        SaleOrderAmendmentHeader Add(SaleOrderAmendmentHeader pt);
        IQueryable<SaleOrderAmendmentHeaderIndexViewModel> GetSaleOrderAmendmentHeaderList(int id, string Uname);
        IQueryable<SaleOrderAmendmentHeaderIndexViewModel> GetSaleOrderAmendmentHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<SaleOrderAmendmentHeaderIndexViewModel> GetSaleOrderAmendmentHeaderListPendingToReview(int id, string Uname);
        Task<IEquatable<SaleOrderAmendmentHeader>> GetAsync();
        Task<SaleOrderAmendmentHeader> FindAsync(int id);
        SaleOrderAmendmentHeader GetSaleOrderAmendmentHeaderByName(string terms);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class SaleOrderAmendmentHeaderService : ISaleOrderAmendmentHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleOrderAmendmentHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        public SaleOrderAmendmentHeader GetSaleOrderAmendmentHeaderByName(string terms)
        {
            return (from p in db.SaleOrderAmendmentHeader
                    where p.DocNo == terms
                    select p).FirstOrDefault();
        }


        public SaleOrderAmendmentHeader Find(int id)
        {
            return _unitOfWork.Repository<SaleOrderAmendmentHeader>().Find(id);
        }

        public SaleOrderAmendmentHeader Create(SaleOrderAmendmentHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleOrderAmendmentHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleOrderAmendmentHeader>().Delete(id);
        }

        public void Delete(SaleOrderAmendmentHeader pt)
        {
            _unitOfWork.Repository<SaleOrderAmendmentHeader>().Delete(pt);
        }

        public void Update(SaleOrderAmendmentHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleOrderAmendmentHeader>().Update(pt);
        }

        public IEnumerable<SaleOrderAmendmentHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleOrderAmendmentHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<SaleOrderAmendmentHeaderIndexViewModel> GetSaleOrderAmendmentHeaderList(int id, string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from p in db.SaleOrderAmendmentHeader
                    orderby  p.DocDate descending,p.DocNo descending
                    where p.SiteId==SiteId && p.DivisionId==p.DivisionId && p.DocTypeId==id
                    select new SaleOrderAmendmentHeaderIndexViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        BuyerName=p.Buyer.Person.Name,
                        SaleOrderAmendmentHeaderId = p.SaleOrderAmendmentHeaderId,
                        ReasonName = p.Reason.ReasonName,
                        Remark = p.Remark,
                        Status = p.Status,  
                        ModifiedBy=p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                    }
                        );
        }

        public IQueryable<SaleOrderAmendmentHeaderIndexViewModel> GetSaleOrderAmendmentHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetSaleOrderAmendmentHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<SaleOrderAmendmentHeaderIndexViewModel> GetSaleOrderAmendmentHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetSaleOrderAmendmentHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public SaleOrderAmendmentHeader Add(SaleOrderAmendmentHeader pt)
        {
            _unitOfWork.Repository<SaleOrderAmendmentHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SaleOrderAmendmentHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.SaleOrderAmendmentHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleOrderAmendmentHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.SaleOrderAmendmentHeaderId).FirstOrDefault();
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

                temp = (from p in db.SaleOrderAmendmentHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.SaleOrderAmendmentHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleOrderAmendmentHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.SaleOrderAmendmentHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<SaleOrderAmendmentHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);

            string[] PersonRoles = null;
            if (!string.IsNullOrEmpty(settings.filterPersonRoles)) { PersonRoles = settings.filterPersonRoles.Split(",".ToCharArray()); }
            else { PersonRoles = new string[] { "NA" }; }

            string DivIdStr = "|" + DivisionId.ToString() + "|";
            string SiteIdStr = "|" + SiteId.ToString() + "|";

            var list = (from p in db.Persons
                        join bus in db.BusinessEntity on p.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join pp in db.PersonProcess on p.PersonID equals pp.PersonId into PersonProcessTable
                        from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                        join pr in db.PersonRole on p.PersonID equals pr.PersonId into PersonRoleTable
                        from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                        where PersonProcessTab.ProcessId == settings.ProcessId
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (p.Name.ToLower().Contains(term.ToLower()) || p.Code.ToLower().Contains(term.ToLower())))
                        && (string.IsNullOrEmpty(settings.filterPersonRoles) ? 1 == 1 : PersonRoles.Contains(PersonRoleTab.RoleDocTypeId.ToString()))
                        && BusinessEntityTab.DivisionIds.IndexOf(DivIdStr) != -1
                        && BusinessEntityTab.SiteIds.IndexOf(SiteIdStr) != -1
                        && (p.IsActive == null ? 1 == 1 : p.IsActive == true)
                        group new { p } by new { p.PersonID } into Result
                        orderby Result.Max(m => m.p.Name)
                        select new ComboBoxResult
                        {
                            id = Result.Key.PersonID.ToString(),
                            text = Result.Max(m => m.p.Name + ", " + m.p.Suffix + " [" + m.p.Code + "]"),
                        }
              );

            return list;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SaleOrderAmendmentHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleOrderAmendmentHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

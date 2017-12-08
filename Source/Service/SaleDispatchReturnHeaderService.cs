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
    public interface ISaleDispatchReturnHeaderService : IDisposable
    {
        SaleDispatchReturnHeader Create(SaleDispatchReturnHeader pt);
        void Delete(int id);
        void Delete(SaleDispatchReturnHeader pt);
        SaleDispatchReturnHeader Find(string Name);
        SaleDispatchReturnHeader Find(int id);
        SaleDispatchReturnHeader FindByInvioceReturn(int id);
        IEnumerable<SaleDispatchReturnHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleDispatchReturnHeader pt);
        SaleDispatchReturnHeader Add(SaleDispatchReturnHeader pt);
        SaleDispatchReturnHeaderViewModel GetSaleDispatchReturnHeader(int id);//HeadeRId
        IQueryable<SaleDispatchReturnHeaderViewModel> GetSaleDispatchReturnHeaderList(int id, string Uname);
        IQueryable<SaleDispatchReturnHeaderViewModel> GetSaleDispatchReturnPendingToSubmit(int id, string Uname);
        IQueryable<SaleDispatchReturnHeaderViewModel> GetSaleDispatchReturnPendingToReview(int id, string Uname);
        Task<IEquatable<SaleDispatchReturnHeader>> GetAsync();
        Task<SaleDispatchReturnHeader> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class SaleDispatchReturnHeaderService : ISaleDispatchReturnHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleDispatchReturnHeader> _SaleDispatchReturnHeaderRepository;
        RepositoryQuery<SaleDispatchReturnHeader> SaleDispatchReturnHeaderRepository;
        public SaleDispatchReturnHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleDispatchReturnHeaderRepository = new Repository<SaleDispatchReturnHeader>(db);
            SaleDispatchReturnHeaderRepository = new RepositoryQuery<SaleDispatchReturnHeader>(_SaleDispatchReturnHeaderRepository);
        }

        public SaleDispatchReturnHeader Find(string Name)
        {
            return SaleDispatchReturnHeaderRepository.Get().Where(i => i.DocNo == Name).FirstOrDefault();
        }


        public SaleDispatchReturnHeader Find(int id)
        {
            return _unitOfWork.Repository<SaleDispatchReturnHeader>().Find(id);
        }

        public SaleDispatchReturnHeader FindByInvioceReturn(int id)
        {
            return (from p in db.SaleDispatchReturnHeader
                    join pi in db.SaleInvoiceReturnHeader on p.SaleDispatchReturnHeaderId equals pi.SaleDispatchReturnHeaderId
                    where pi.SaleInvoiceReturnHeaderId == id
                    select p).FirstOrDefault();
        }

        public SaleDispatchReturnHeader Create(SaleDispatchReturnHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleDispatchReturnHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleDispatchReturnHeader>().Delete(id);
        }

        public void Delete(SaleDispatchReturnHeader pt)
        {
            _unitOfWork.Repository<SaleDispatchReturnHeader>().Delete(pt);
        }

        public void Update(SaleDispatchReturnHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleDispatchReturnHeader>().Update(pt);
        }

        public IEnumerable<SaleDispatchReturnHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleDispatchReturnHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public SaleDispatchReturnHeaderViewModel GetSaleDispatchReturnHeader(int id)
        {
            return (from p in db.SaleDispatchReturnHeader
                    where p.SaleDispatchReturnHeaderId == id
                    select new SaleDispatchReturnHeaderViewModel
                    {
                        SaleDispatchReturnHeaderId = p.SaleDispatchReturnHeaderId,
                        DivisionId = p.DivisionId,
                        DocNo = p.DocNo,
                        DocDate = p.DocDate,
                        DocTypeId = p.DocTypeId,
                        Remark = p.Remark,
                        SiteId = p.SiteId,
                        GodownId=p.GodownId,
                        Status = p.Status,
                        BuyerId = p.BuyerId,                                              
                        ReasonId=p.ReasonId,
                        ModifiedBy = p.ModifiedBy,
                        CreatedDate = p.CreatedDate,
                        CreatedBy = p.CreatedBy,
                        LockReason = p.LockReason,
                    }

                        ).FirstOrDefault();
        }
        public IQueryable<SaleDispatchReturnHeaderViewModel> GetSaleDispatchReturnHeaderList(int id, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"]; 

            var pt = (from p in db.SaleDispatchReturnHeader
                      join t in db._Users on p.ModifiedBy equals t.UserName into table3
                      from tab3 in table3.DefaultIfEmpty()
                      join t in db.DocumentType on p.DocTypeId equals t.DocumentTypeId into table
                      from tab in table.DefaultIfEmpty()
                      join t1 in db.Persons on p.BuyerId equals t1.PersonID into table2
                      from tab2 in table2.DefaultIfEmpty()
                      orderby p.DocDate descending, p.DocNo descending
                      where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id
                      select new SaleDispatchReturnHeaderViewModel
                      {
                          DocDate = p.DocDate,
                          DocNo = p.DocNo,
                          DocTypeName = tab.DocumentTypeName,
                          SaleDispatchReturnHeaderId = p.SaleDispatchReturnHeaderId,
                          Remark = p.Remark,
                          Status = p.Status,
                          BuyerName = tab2.Name,
                          ModifiedBy = p.ModifiedBy,
                          FirstName = tab3.FirstName,
                          ReviewCount = p.ReviewCount,
                          ReviewBy = p.ReviewBy,
                          Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                      }
                         );
            return pt;
        }

        public SaleDispatchReturnHeader Add(SaleDispatchReturnHeader pt)
        {
            _unitOfWork.Repository<SaleDispatchReturnHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SaleDispatchReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.SaleDispatchReturnHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDispatchReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.SaleDispatchReturnHeaderId).FirstOrDefault();
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

                temp = (from p in db.SaleDispatchReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.SaleDispatchReturnHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDispatchReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.SaleDispatchReturnHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<SaleDispatchReturnHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<SaleDispatchReturnHeaderViewModel> GetSaleDispatchReturnPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var SaleDispatchReturnHeader = GetSaleDispatchReturnHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in SaleDispatchReturnHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<SaleDispatchReturnHeaderViewModel> GetSaleDispatchReturnPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var SaleDispatchReturnHeader = GetSaleDispatchReturnHeaderList(id, Uname).AsQueryable();

            var PendingToApprove = from p in SaleDispatchReturnHeader
                                   where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToApprove;

        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(DocTypeId, DivisionId, SiteId);

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


        public Task<IEquatable<SaleDispatchReturnHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleDispatchReturnHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

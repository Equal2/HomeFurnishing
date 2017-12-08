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
    public interface IPurchaseOrderCancelHeaderService : IDisposable
    {
        PurchaseOrderCancelHeader Create(PurchaseOrderCancelHeader pt);
        void Delete(int id);
        void Delete(PurchaseOrderCancelHeader pt);
        PurchaseOrderCancelHeader Find(int id);
        IEnumerable<PurchaseOrderCancelHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseOrderCancelHeader pt);
        PurchaseOrderCancelHeader Add(PurchaseOrderCancelHeader pt);
        IQueryable<PurchaseOrderCancelHeaderIndexViewModel> GetPurchaseOrderCancelHeaderList(int id,string Uname);
        IQueryable<PurchaseOrderCancelHeaderIndexViewModel> GetPurchaseOrderCancelPendingToSubmit(int id, string Uname);
        IQueryable<PurchaseOrderCancelHeaderIndexViewModel> GetPurchaseOrderCancelPendingToReview(int id, string Uname);

        // IEnumerable<PurchaseOrderCancelHeader> GetPurchaseOrderCancelHeaderList(int buyerId);
        Task<IEquatable<PurchaseOrderCancelHeader>> GetAsync();
        Task<PurchaseOrderCancelHeader> FindAsync(int id);
        PurchaseOrderCancelHeader GetPurchaseOrderCancelHeaderByName(string terms);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class PurchaseOrderCancelHeaderService : IPurchaseOrderCancelHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public PurchaseOrderCancelHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        public PurchaseOrderCancelHeader GetPurchaseOrderCancelHeaderByName(string terms)
        {
            return (from p in db.PurchaseOrderCancelHeader
                    where p.DocNo == terms
                    select p).FirstOrDefault();
        }


        public PurchaseOrderCancelHeader Find(int id)
        {
            return _unitOfWork.Repository<PurchaseOrderCancelHeader>().Find(id);
        }

        public PurchaseOrderCancelHeader Create(PurchaseOrderCancelHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseOrderCancelHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseOrderCancelHeader>().Delete(id);
        }

        public void Delete(PurchaseOrderCancelHeader pt)
        {
            _unitOfWork.Repository<PurchaseOrderCancelHeader>().Delete(pt);
        }

        public void Update(PurchaseOrderCancelHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseOrderCancelHeader>().Update(pt);
        }

        public IEnumerable<PurchaseOrderCancelHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseOrderCancelHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<PurchaseOrderCancelHeaderIndexViewModel> GetPurchaseOrderCancelHeaderList(int id,string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"]; 

            return (from p in db.PurchaseOrderCancelHeader
                    join t in db._Users on p.ModifiedBy equals t.UserName into table
                    from tab in table.DefaultIfEmpty()
                    orderby p.DocDate descending,p.DocNo ascending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id
                    select new PurchaseOrderCancelHeaderIndexViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        PurchaseOrderCancelHeaderId = p.PurchaseOrderCancelHeaderId,
                        ReasonName = p.Reason.ReasonName,
                        Remark = p.Remark,
                        Status = p.Status,
                        SupplierName = p.Supplier.Person.Name,
                        ModifiedBy = p.ModifiedBy,
                        FirstName = tab.FirstName,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                    }
                        );
        }

        public PurchaseOrderCancelHeader Add(PurchaseOrderCancelHeader pt)
        {
            _unitOfWork.Repository<PurchaseOrderCancelHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseOrderCancelHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.PurchaseOrderCancelHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderCancelHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.PurchaseOrderCancelHeaderId).FirstOrDefault();
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

                temp = (from p in db.PurchaseOrderCancelHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.PurchaseOrderCancelHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderCancelHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.PurchaseOrderCancelHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<PurchaseOrderCancelHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<PurchaseOrderCancelHeaderIndexViewModel> GetPurchaseOrderCancelPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseOrderCancelHeader = GetPurchaseOrderCancelHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in PurchaseOrderCancelHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<PurchaseOrderCancelHeaderIndexViewModel> GetPurchaseOrderCancelPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseOrderCancelHeader = GetPurchaseOrderCancelHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in PurchaseOrderCancelHeader
                                   where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocTypeId, DivisionId, SiteId);

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


        public Task<IEquatable<PurchaseOrderCancelHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrderCancelHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

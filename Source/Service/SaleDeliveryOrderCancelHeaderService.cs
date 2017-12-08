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
    public interface ISaleDeliveryOrderCancelHeaderService : IDisposable
    {
        SaleDeliveryOrderCancelHeader Create(SaleDeliveryOrderCancelHeader pt);
        void Delete(int id);
        void Delete(SaleDeliveryOrderCancelHeader pt);
        SaleDeliveryOrderCancelHeader Find(int id);
        IEnumerable<SaleDeliveryOrderCancelHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleDeliveryOrderCancelHeader pt);
        SaleDeliveryOrderCancelHeader Add(SaleDeliveryOrderCancelHeader pt);
        IQueryable<SaleDeliveryOrderCancelHeaderViewModel> GetSaleDeliveryOrderCancelHeaderList(int id,string Uname);
        IQueryable<SaleDeliveryOrderCancelHeaderViewModel> GetSaleDeliveryOrderCancelPendingToSubmit(int id, string Uname);
        IQueryable<SaleDeliveryOrderCancelHeaderViewModel> GetSaleDeliveryOrderCancelPendingToReview(int id, string Uname);

        // IEnumerable<SaleDeliveryOrderCancelHeader> GetSaleDeliveryOrderCancelHeaderList(int buyerId);
        Task<IEquatable<SaleDeliveryOrderCancelHeader>> GetAsync();
        Task<SaleDeliveryOrderCancelHeader> FindAsync(int id);
        SaleDeliveryOrderCancelHeader GetSaleDeliveryOrderCancelHeaderByName(string terms);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class SaleDeliveryOrderCancelHeaderService : ISaleDeliveryOrderCancelHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleDeliveryOrderCancelHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        public SaleDeliveryOrderCancelHeader GetSaleDeliveryOrderCancelHeaderByName(string terms)
        {
            return (from p in db.SaleDeliveryOrderCancelHeader
                    where p.DocNo == terms
                    select p).FirstOrDefault();
        }


        public SaleDeliveryOrderCancelHeader Find(int id)
        {
            return _unitOfWork.Repository<SaleDeliveryOrderCancelHeader>().Find(id);
        }

        public SaleDeliveryOrderCancelHeader Create(SaleDeliveryOrderCancelHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleDeliveryOrderCancelHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleDeliveryOrderCancelHeader>().Delete(id);
        }

        public void Delete(SaleDeliveryOrderCancelHeader pt)
        {
            _unitOfWork.Repository<SaleDeliveryOrderCancelHeader>().Delete(pt);
        }

        public void Update(SaleDeliveryOrderCancelHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleDeliveryOrderCancelHeader>().Update(pt);
        }

        public IEnumerable<SaleDeliveryOrderCancelHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleDeliveryOrderCancelHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<SaleDeliveryOrderCancelHeaderViewModel> GetSaleDeliveryOrderCancelHeaderList(int id,string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"]; 

            return (from p in db.SaleDeliveryOrderCancelHeader
                    join t in db._Users on p.ModifiedBy equals t.UserName into table
                    from tab in table.DefaultIfEmpty()
                    orderby p.DocDate descending,p.DocNo ascending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id
                    select new SaleDeliveryOrderCancelHeaderViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        SaleDeliveryOrderCancelHeaderId = p.SaleDeliveryOrderCancelHeaderId,
                        ReasonName = p.Reason.ReasonName,
                        Remark = p.Remark,
                        Status = p.Status,
                        BuyerName = p.Buyer.Person.Name,
                        ModifiedBy = p.ModifiedBy,
                        //FirstName = tab.FirstName,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                    }
                        );
        }

        public SaleDeliveryOrderCancelHeader Add(SaleDeliveryOrderCancelHeader pt)
        {
            _unitOfWork.Repository<SaleDeliveryOrderCancelHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SaleDeliveryOrderCancelHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.SaleDeliveryOrderCancelHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDeliveryOrderCancelHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.SaleDeliveryOrderCancelHeaderId).FirstOrDefault();
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

                temp = (from p in db.SaleDeliveryOrderCancelHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.SaleDeliveryOrderCancelHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleDeliveryOrderCancelHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.SaleDeliveryOrderCancelHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<SaleDeliveryOrderCancelHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<SaleDeliveryOrderCancelHeaderViewModel> GetSaleDeliveryOrderCancelPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var SaleDeliveryOrderCancelHeader = GetSaleDeliveryOrderCancelHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in SaleDeliveryOrderCancelHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<SaleDeliveryOrderCancelHeaderViewModel> GetSaleDeliveryOrderCancelPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var SaleDeliveryOrderCancelHeader = GetSaleDeliveryOrderCancelHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in SaleDeliveryOrderCancelHeader
                                   where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);

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


        public Task<IEquatable<SaleDeliveryOrderCancelHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleDeliveryOrderCancelHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

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
using System.Data.Entity.SqlServer;
using Model.ViewModel;

namespace Service
{
    public interface ISaleDispatchHeaderService : IDisposable
    {
        SaleDispatchHeader Create(SaleDispatchHeader s);
        void Delete(int id);
        void Delete(SaleDispatchHeader s);
        SaleDispatchHeader GetSaleDispatchHeader(int id);

        SaleDispatchHeader Find(int id);
        void Update(SaleDispatchHeader s);
        string GetMaxDocNo();
        SaleDispatchHeader FindByDocNo(string Docno);

        IQueryable<SaleDispatchHeaderIndexViewModel> GetSaleDispatchHeaderList(int id, string Uname);
        IQueryable<SaleDispatchHeaderIndexViewModel> GetSaleDispatchHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<SaleDispatchHeaderIndexViewModel> GetSaleDispatchHeaderListPendingToReview(int id, string Uname);

        IEnumerable<SaleDispatchListViewModel> GetPendingReceipts(int id, int SaleGoodsReturnHeaderId);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }
    public class SaleDispatchHeaderService : ISaleDispatchHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleDispatchHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

     public SaleDispatchHeader Create(SaleDispatchHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleDispatchHeader>().Insert(s);
            return s;
        }

       public void Delete(int id)
     {
         _unitOfWork.Repository<SaleDispatchHeader>().Delete(id);
     }
       public void Delete(SaleDispatchHeader s)
        {
            _unitOfWork.Repository<SaleDispatchHeader>().Delete(s);
        }
       public void Update(SaleDispatchHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleDispatchHeader>().Update(s);            
        }

       public SaleDispatchHeader GetSaleDispatchHeader(int id)
        {
            return _unitOfWork.Repository<SaleDispatchHeader>().Query().Get().Where(m => m.SaleDispatchHeaderId == id).FirstOrDefault();
        }

        public SaleDispatchHeader Find(int id)
        {
            return _unitOfWork.Repository<SaleDispatchHeader>().Find(id);
        }


        public SaleDispatchHeader FindByDocNo(string Docno)
       {
         return  _unitOfWork.Repository<SaleDispatchHeader>().Query().Get().Where(m => m.DocNo == Docno).FirstOrDefault();

       }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<SaleDispatchHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<SaleDispatchHeaderIndexViewModel> GetSaleDispatchHeaderList(int id, string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];


            var temp = from p in db.SaleDispatchHeader
                       join t in db.Persons on p.SaleToBuyerId equals t.PersonID
                       orderby p.DocDate descending, p.DocNo descending
                       where p.DivisionId == DivisionId && p.SiteId == SiteId && p.DocTypeId == id
                       select new SaleDispatchHeaderIndexViewModel
                       {
                           Remark = p.Remark,
                           DocDate = p.DocDate,
                           SaleDispatchHeaderId = p.SaleDispatchHeaderId,
                           DocNo = p.DocNo,
                           SaleToBuyerName = t.Name,
                           Status = p.Status,
                           ModifiedBy = p.ModifiedBy,
                           ReviewCount = p.ReviewCount,
                           ReviewBy = p.ReviewBy,
                           Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                           GatePassDocNo = p.GatePassHeader.DocNo,
                           GatePassHeaderId = p.GatePassHeaderId,
                           GatePassDocDate = p.GatePassHeader.DocDate,
                           GatePassStatus = (p.GatePassHeaderId != null ? p.GatePassHeader.Status : 0),
                       };
            return temp;
        }

        public IQueryable<SaleDispatchHeaderIndexViewModel> GetSaleDispatchHeaderListPendingToSubmit(int id, string Uname)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetSaleDispatchHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;
        }

        public IQueryable<SaleDispatchHeaderIndexViewModel> GetSaleDispatchHeaderListPendingToReview(int id, string Uname)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetSaleDispatchHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;
        }

        public IEnumerable<SaleDispatchListViewModel> GetPendingReceipts(int id, int SaleGoodsReturnHeaderId)
        {

            var SaleGoodsReturn = new SaleDispatchReturnHeaderService(_unitOfWork).Find(SaleGoodsReturnHeaderId);

            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(SaleGoodsReturn.DocTypeId, SaleGoodsReturn.DivisionId, SaleGoodsReturn.SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            string[] contraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { contraSites = new string[] { "NA" }; }

            string[] contraDivisions = null;
            if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            return (from p in db.ViewSaleDispatchBalance
                    join t in db.SaleDispatchHeader on p.SaleDispatchHeaderId equals t.SaleDispatchHeaderId into table
                    from tab in table.DefaultIfEmpty()
                    join t1 in db.SaleOrderLine on p.SaleOrderLineId equals t1.SaleOrderLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    where p.ProductId == id && tab.SaleToBuyerId == SaleGoodsReturn.BuyerId && p.BalanceQty > 0
                    && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == CurrentSiteId : contraSites.Contains(p.SiteId.ToString()))
                    && (string.IsNullOrEmpty(settings.filterContraDivisions) ? p.DivisionId == CurrentDivisionId : contraDivisions.Contains(p.DivisionId.ToString()))
                    select new SaleDispatchListViewModel
                    {
                        SaleDispatchLineId = p.SaleDispatchLineId,
                        SaleDispatchHeaderId = p.SaleDispatchHeaderId,
                        DocNo = tab.DocNo,
                        Dimension1Name = tab1.Dimension1.Dimension1Name,
                        Dimension2Name = tab1.Dimension2.Dimension2Name,
                    }
                        );
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
    }
}

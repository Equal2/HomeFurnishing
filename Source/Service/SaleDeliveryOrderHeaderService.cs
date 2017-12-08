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
    public interface ISaleDeliveryOrderHeaderService : IDisposable
    {
        SaleDeliveryOrderHeader Create(SaleDeliveryOrderHeader s);
        void Delete(int id);
        void Delete(SaleDeliveryOrderHeader s);
        SaleDeliveryOrderHeader GetSaleDeliveryOrderHeader(int id);
        SaleDeliveryOrderHeader Find(int id);
        IQueryable<SaleDeliveryOrderHeaderIndexViewModel> GetSaleDeliveryOrderHeaderList(int id, string Uname);
        IQueryable<SaleDeliveryOrderHeaderIndexViewModel> GetSaleDeliveryOrderHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<SaleDeliveryOrderHeaderIndexViewModel> GetSaleDeliveryOrderHeaderListPendingToReview(int id, string Uname);
        void Update(SaleDeliveryOrderHeader s);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }
    public class SaleDeliveryOrderHeaderService : ISaleDeliveryOrderHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleDeliveryOrderHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

        public SaleDeliveryOrderHeader Create(SaleDeliveryOrderHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleDeliveryOrderHeader>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleDeliveryOrderHeader>().Delete(id);
        }
        public void Delete(SaleDeliveryOrderHeader s)
        {
            _unitOfWork.Repository<SaleDeliveryOrderHeader>().Delete(s);
        }
        public void Update(SaleDeliveryOrderHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleDeliveryOrderHeader>().Update(s);
        }

        public SaleDeliveryOrderHeader GetSaleDeliveryOrderHeader(int id)
        {
            return (from p in db.SaleDeliveryOrderHeader
                    where p.SaleDeliveryOrderHeaderId == id
                    select p).FirstOrDefault();
        }

        public SaleDeliveryOrderHeader Find(int id)
        {
            return _unitOfWork.Repository<SaleDeliveryOrderHeader>().Find(id);
        }



        public IQueryable<SaleDeliveryOrderHeaderIndexViewModel> GetSaleDeliveryOrderHeaderList(int id, string Uname)
        {
            int divisionid = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int siteid = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var temp = from p in db.SaleDeliveryOrderHeader
                       orderby p.DocDate descending, p.DocNo descending
                       where p.DivisionId == divisionid && p.SiteId == siteid && p.DocTypeId == id
                       select new SaleDeliveryOrderHeaderIndexViewModel
                       {
                           DocDate = p.DocDate,
                           SaleDeliveryOrderHeaderId = p.SaleDeliveryOrderHeaderId,
                           DocNo = p.DocNo,
                           DueDate = p.DueDate,
                           BuyerName = p.Buyer.Person.Name,
                           Status = p.Status,
                           ModifiedBy=p.ModifiedBy,
                           ReviewCount = p.ReviewCount,
                           ReviewBy = p.ReviewBy,
                           Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                       };
            return temp;
        }

        public IQueryable<SaleDeliveryOrderHeaderIndexViewModel> GetSaleDeliveryOrderHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetSaleDeliveryOrderHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Import || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<SaleDeliveryOrderHeaderIndexViewModel> GetSaleDeliveryOrderHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetSaleDeliveryOrderHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }


        public IEnumerable<SaleDeliveryOrderLineListViewModel> GetPendingSaleDeliveryOrdersForOrderCancel(int ProductId, int SaleDeliveryOrderCancelHeaderId)//Product Id
        {

            var SaleDeliveryOrderCancel = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(SaleDeliveryOrderCancelHeaderId);

            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(SaleDeliveryOrderCancel.DocTypeId, SaleDeliveryOrderCancel.DivisionId, SaleDeliveryOrderCancel.SiteId);

            //string[] contraDocTypes = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            //else { contraDocTypes = new string[] { "NA" }; }

            //string[] contraSites = null;
            //if (!string.IsNullOrEmpty(settings.filterContraSites)) { contraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            //else { contraSites = new string[] { "NA" }; }

            //string[] contraDivisions = null;
            //if (!string.IsNullOrEmpty(settings.filterContraDivisions)) { contraDivisions = settings.filterContraDivisions.Split(",".ToCharArray()); }
            //else { contraDivisions = new string[] { "NA" }; }

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var tem = from p in db.ViewSaleDeliveryOrderBalance
                      join t in db.SaleDeliveryOrderLine on p.SaleDeliveryOrderLineId equals t.SaleDeliveryOrderLineId into table
                      from tab in table.DefaultIfEmpty()
                      join sol in db.SaleOrderLine on tab.SaleOrderLineId equals sol.SaleOrderLineId
                      where sol.ProductId == ProductId && p.BalanceQty > 0 && p.BuyerId == SaleDeliveryOrderCancel.BuyerId
                      //&& (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(p.DocTypeId.ToString()))
                      && p.SiteId == CurrentSiteId 
                      && p.DivisionId == CurrentDivisionId 
                      orderby p.SaleDeliveryOrderNo
                      select new SaleDeliveryOrderLineListViewModel
                      {
                          DocNo = p.SaleDeliveryOrderNo,
                          SaleDeliveryOrderLineId = p.SaleDeliveryOrderLineId,
                          Dimension1Name = sol.Dimension1.Dimension1Name,
                          Dimension2Name = sol.Dimension2.Dimension2Name,
                      };

            return (tem);
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
    }
}

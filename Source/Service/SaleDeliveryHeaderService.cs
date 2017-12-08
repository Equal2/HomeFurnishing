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
    public interface ISaleDeliveryHeaderService : IDisposable
    {
        SaleDeliveryHeader Create(SaleDeliveryHeader s);
        void Delete(int id);
        void Delete(SaleDeliveryHeader s);
        SaleDeliveryHeader GetSaleDeliveryHeader(int id);

        SaleDeliveryHeader Find(int id);
        void Update(SaleDeliveryHeader s);
        string GetMaxDocNo();
        SaleDeliveryHeader FindByDocNo(string Docno);

        IQueryable<SaleDeliveryHeaderIndexViewModel> GetSaleDeliveryHeaderList(int id, string Uname);
        IQueryable<SaleDeliveryHeaderIndexViewModel> GetSaleDeliveryHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<SaleDeliveryHeaderIndexViewModel> GetSaleDeliveryHeaderListPendingToReview(int id, string Uname);

        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
        IEnumerable<SaleDeliveryWizardViewModel> GetSaleInvoiceForSaleDeliveryWizard(int DocTypeId);//DocTypeId
    }
    public class SaleDeliveryHeaderService : ISaleDeliveryHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleDeliveryHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

     public SaleDeliveryHeader Create(SaleDeliveryHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleDeliveryHeader>().Insert(s);
            return s;
        }

       public void Delete(int id)
     {
         _unitOfWork.Repository<SaleDeliveryHeader>().Delete(id);
     }
       public void Delete(SaleDeliveryHeader s)
        {
            _unitOfWork.Repository<SaleDeliveryHeader>().Delete(s);
        }
       public void Update(SaleDeliveryHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleDeliveryHeader>().Update(s);            
        }

       public SaleDeliveryHeader GetSaleDeliveryHeader(int id)
        {
            return _unitOfWork.Repository<SaleDeliveryHeader>().Query().Get().Where(m => m.SaleDeliveryHeaderId == id).FirstOrDefault();
        }

        public SaleDeliveryHeader Find(int id)
        {
            return _unitOfWork.Repository<SaleDeliveryHeader>().Find(id);
        }


        public SaleDeliveryHeader FindByDocNo(string Docno)
       {
         return  _unitOfWork.Repository<SaleDeliveryHeader>().Query().Get().Where(m => m.DocNo == Docno).FirstOrDefault();

       }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<SaleDeliveryHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<SaleDeliveryHeaderIndexViewModel> GetSaleDeliveryHeaderList(int id, string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];


            var temp = from p in db.SaleDeliveryHeader
                       join t in db.Persons on p.SaleToBuyerId equals t.PersonID
                       orderby p.DocDate descending, p.DocNo descending
                       where p.DivisionId == DivisionId && p.SiteId == SiteId && p.DocTypeId == id
                       select new SaleDeliveryHeaderIndexViewModel
                       {
                           Remark = p.Remark,
                           DocDate = p.DocDate,
                           SaleDeliveryHeaderId = p.SaleDeliveryHeaderId,
                           DocNo = p.DocNo,
                           SaleToBuyerName = t.Name,
                           Status = p.Status,
                           ModifiedBy = p.ModifiedBy,
                           GatePassDocNo = p.GatePassHeader.DocNo,
                           GatePassHeaderId = p.GatePassHeaderId,
                           GatePassDocDate = p.GatePassHeader.DocDate,
                           GatePassStatus = (p.GatePassHeaderId != null ? p.GatePassHeader.Status : 0),
                           ReviewCount = p.ReviewCount,
                           ReviewBy = p.ReviewBy,
                           Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),

                       };
            return temp;
        }

        public IQueryable<SaleDeliveryHeaderIndexViewModel> GetSaleDeliveryHeaderListPendingToSubmit(int id, string Uname)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetSaleDeliveryHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;
        }

        public IQueryable<SaleDeliveryHeaderIndexViewModel> GetSaleDeliveryHeaderListPendingToReview(int id, string Uname)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetSaleDeliveryHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;
        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(DocTypeId, DivisionId, SiteId);

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

        public IEnumerable<SaleDeliveryWizardViewModel> GetSaleInvoiceForSaleDeliveryWizard(int DocTypeId)//DocTypeId
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(DocTypeId, DivisionId, SiteId);

            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);
            SqlParameter SqlParameterDocTypeId = new SqlParameter("@DocumentTypeId", DocTypeId);

            IEnumerable<SaleDeliveryWizardViewModel> temp = db.Database.SqlQuery<SaleDeliveryWizardViewModel>("Web.sp_GetSaleInvoiceForSaleDeliveryWizard @SiteId, @DivisionId, @DocumentTypeId", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterDocTypeId).ToList();

            return temp;

        }

        public void Dispose()
        {
        }
    }
}

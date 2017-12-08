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
    public interface ISaleEnquiryHeaderService : IDisposable
    {
        SaleEnquiryHeader Create(SaleEnquiryHeader s);
        void Delete(int id);
        void Delete(SaleEnquiryHeader s);
        SaleEnquiryHeader GetSaleEnquiryHeader(int id);

        SaleEnquiryHeaderIndexViewModel GetSaleEnquiryHeaderVM(int id);
        SaleEnquiryHeader Find(int id);
        IQueryable<SaleEnquiryHeaderIndexViewModel> GetSaleEnquiryHeaderList(int id, string Uname);
        IQueryable<SaleEnquiryHeaderIndexViewModel> GetSaleEnquiryHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<SaleEnquiryHeaderIndexViewModel> GetSaleEnquiryHeaderListPendingToReview(int id, string Uname);
        void Update(SaleEnquiryHeader s);
        string GetMaxDocNo();
        SaleEnquiryHeader FindByDocNo(string Docno);
        IEnumerable<SaleEnquiryPrintViewModel> FGetPrintData(int Id);
        IEnumerable<SaleEnquiryHeader> GetSaleEnquiryListForReport(int BuyerId);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<SaleEnquiryLineListViewModel> GetSaleEnquirysForDocumentType(int HeaderId, string term);//DoctypeIds
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }
    public class SaleEnquiryHeaderService : ISaleEnquiryHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SaleEnquiryHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

        public SaleEnquiryHeader Create(SaleEnquiryHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleEnquiryHeader>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleEnquiryHeader>().Delete(id);
        }
        public void Delete(SaleEnquiryHeader s)
        {
            _unitOfWork.Repository<SaleEnquiryHeader>().Delete(s);
        }
        public void Update(SaleEnquiryHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleEnquiryHeader>().Update(s);
        }

        public SaleEnquiryHeader GetSaleEnquiryHeader(int id)
        {
            return _unitOfWork.Repository<SaleEnquiryHeader>().Query().Get().Where(m => m.SaleEnquiryHeaderId == id).FirstOrDefault();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.SaleEnquiryHeader
                        join t in db.Persons on p.SaleToBuyerId equals t.PersonID
                        orderby p.DocDate descending, p.DocNo descending
                        select p.SaleEnquiryHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();


            }
            else
            {
                temp = (from p in db.SaleEnquiryHeader
                        join t in db.Persons on p.SaleToBuyerId equals t.PersonID
                        orderby p.DocDate descending, p.DocNo descending
                        select p.SaleEnquiryHeaderId).FirstOrDefault();
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

                temp = (from p in db.SaleEnquiryHeader
                        join t in db.Persons on p.SaleToBuyerId equals t.PersonID
                        orderby p.DocDate descending, p.DocNo descending
                        select p.SaleEnquiryHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleEnquiryHeader
                        join t in db.Persons on p.SaleToBuyerId equals t.PersonID
                        orderby p.DocDate descending, p.DocNo descending
                        select p.SaleEnquiryHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public SaleEnquiryHeaderIndexViewModel GetSaleEnquiryHeaderVM(int id)
        {

            SaleEnquiryHeaderIndexViewModel temp = (from p in db.SaleEnquiryHeader
                                                  join t in db.Persons on p.SaleToBuyerId equals t.PersonID into table
                                                  from tab in table.DefaultIfEmpty()
                                                  join t2 in db.Persons on p.BillToBuyerId equals t2.PersonID into table2
                                                  from tab2 in table2.DefaultIfEmpty()

                                                  where p.SaleEnquiryHeaderId == id
                                                  select new SaleEnquiryHeaderIndexViewModel
                                                  {
                                                      DocTypeId = p.DocTypeId,
                                                      BillToBuyerName = tab2.Name,
                                                      BuyerEnquiryNo = p.BuyerEnquiryNo,
                                                      CreatedBy = p.CreatedBy,
                                                      CreatedDate = p.CreatedDate,
                                                      DivisionName = p.Division.DivisionName,
                                                      DocDate = p.DocDate,
                                                      DocNo = p.DocNo,
                                                      DueDate = p.DueDate,
                                                      ModifiedBy = p.ModifiedBy,
                                                      ModifiedDate = p.ModifiedDate,
                                                      Remark = p.Remark,
                                                      SaleEnquiryHeaderId = p.SaleEnquiryHeaderId,
                                                      SaleToBuyerName = tab.Name,
                                                      ShipAddress = p.ShipAddress,
                                                      SiteName = p.Site.SiteName,
                                                      SiteId = p.SiteId,
                                                      DivisionId = p.DivisionId,
                                                      Status = p.Status,
                                                      DocumentTypeName = p.DocType.DocumentTypeName,
                                                      CurrencyName = p.Currency.Name,
                                                      ShipMethodName = p.ShipMethod.ShipMethodName,
                                                      DeliveryTermsName = p.DeliveryTerms.DeliveryTermsName,
                                                      CreditDays = p.CreditDays,
                                                      TermsAndConditions = p.TermsAndConditions,
                                                      Priority = p.Priority,

                                                  }

                ).FirstOrDefault();

            temp.PriorityName = Enum.GetName(typeof(SaleEnquiryPriority), temp.Priority);
            return temp;
        }

        public SaleEnquiryHeader Find(int id)
        {
            return _unitOfWork.Repository<SaleEnquiryHeader>().Find(id);
        }



        public IQueryable<SaleEnquiryHeaderIndexViewModel> GetSaleEnquiryHeaderList(int id, string Uname)
        {
            int divisionid = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int siteid = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var temp = from p in db.SaleEnquiryHeader
                       join t in db.Persons on p.SaleToBuyerId equals t.PersonID
                       orderby p.DocDate descending, p.DocNo descending
                       where p.DivisionId == divisionid && p.SiteId == siteid && p.DocTypeId == id
                       select new SaleEnquiryHeaderIndexViewModel
                       {
                           Remark = p.Remark,
                           DocDate = p.DocDate,
                           SaleEnquiryHeaderId = p.SaleEnquiryHeaderId,
                           DocNo = p.DocNo,
                           DueDate = p.DueDate,
                           SaleToBuyerName = t.Name,
                           Status = p.Status,
                           ModifiedBy = p.ModifiedBy,
                           ReviewCount = p.ReviewCount,
                           ReviewBy = p.ReviewBy,
                           Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                           TotalQty = p.SaleEnquiryLines.Sum(m => m.Qty),
                           DecimalPlaces = 0,
                       };
            return temp;
        }

        public IQueryable<SaleEnquiryHeaderIndexViewModel> GetSaleEnquiryHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetSaleEnquiryHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Import || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<SaleEnquiryHeaderIndexViewModel> GetSaleEnquiryHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetSaleEnquiryHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }


        public IEnumerable<SaleEnquiryLineListViewModel> GetSaleEnquirys(int ProductId, int BuyerId)//Product Id
        {
            var tem = from p in db.SaleEnquiryHeader
                      join t in db.SaleEnquiryLine on p.SaleEnquiryHeaderId equals t.SaleEnquiryHeaderId into table
                      from tab in table.DefaultIfEmpty()

                      where tab.ProductId == ProductId && p.SaleToBuyerId == BuyerId
                      orderby p.DocNo
                      select new SaleEnquiryLineListViewModel
                      {
                          DocNo = p.DocNo,
                          SaleEnquiryLineId = tab.SaleEnquiryLineId,
                      };

            return (tem);
        }

        public SaleEnquiryHeader FindByDocNo(string Docno)
        {
            return _unitOfWork.Repository<SaleEnquiryHeader>().Query().Get().Where(m => m.DocNo == Docno).FirstOrDefault();

        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<SaleEnquiryHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }
        public void Dispose()
        {
        }

        public IEnumerable<SaleEnquiryPrintViewModel> FGetPrintData(int Id)
        {
            ApplicationDbContext Db = new ApplicationDbContext();
            IEnumerable<SaleEnquiryPrintViewModel> SaleEnquiryprintviewmodel = db.Database.SqlQuery<SaleEnquiryPrintViewModel>(Db.strSchemaName + ".ProcSaleEnquiryPrint @Id", new SqlParameter("@Id", Id)).ToList();
            return SaleEnquiryprintviewmodel;
        }

        public IEnumerable<SaleEnquiryHeader> GetSaleEnquiryListForReport(int BuyerId)
        {
            return _unitOfWork.Repository<SaleEnquiryHeader>().Query().Include(m => m.DocType).Get().Where(m => m.SaleToBuyerId == BuyerId);
        }

        public IEnumerable<SaleEnquiryHeader> GetSaleEnquiryListFromIds(String StrSaleEnquiryIdsList)
        {
            string[] strarr = StrSaleEnquiryIdsList.Split(',');
            int[] SaleEnquiryListArr = Array.ConvertAll(strarr, s => int.Parse(s));

            var p = (from H in db.SaleEnquiryHeader where SaleEnquiryListArr.Contains(H.SaleEnquiryHeaderId) select H).ToList();

            return p;


            //return _unitOfWork.Repository<SaleEnquiryHeader>().Query().Get().Where(SaleEnquiryListArr.Contains(m => m.SaleEnquiryHeaderId));
        }


        public IEnumerable<SaleEnquiryLineListViewModel> GetSaleEnquirysForDocumentType(int HeaderId, string term)
        {
            //return (from p in db.SaleEnquiryHeader
            //        where DocTypeIds.Contains(p.DocTypeId.ToString())
            //        orderby p.DocDate descending, p.DocNo descending
            //        select new SaleEnquiryLineListViewModel
            //        {
            //            DocNo = p.DocNo,
            //            SaleEnquiryHeaderId = p.SaleEnquiryHeaderId
            //        }
            //            );

            var Header = new MaterialPlanHeaderService(_unitOfWork).Find(HeaderId);

            var Settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            SqlParameter SqlParameterDocType = new SqlParameter("@PlanningDocumentType", Header.DocTypeId);
            SqlParameter SqlParameterSite = new SqlParameter("@Site", Header.SiteId);
            SqlParameter SqlParameterDivision = new SqlParameter("@Division", Header.DivisionId);
            SqlParameter SqlParameterBuyer = new SqlParameter("@BuyerId", Header.BuyerId.HasValue ? Header.BuyerId : (object)DBNull.Value);

            string ProcName = Settings.PendingProdOrderList;
            if (string.IsNullOrEmpty(ProcName))
                throw new Exception("Pending ProdOrders not configured");

            IEnumerable<PendingSaleEnquiryFromProc> CalculationLineList = db.Database.SqlQuery<PendingSaleEnquiryFromProc>("" + ProcName + " @PlanningDocumentType, @Site, @Division, @BuyerId", SqlParameterDocType, SqlParameterSite, SqlParameterDivision, SqlParameterBuyer).ToList();

            var list = (from p in CalculationLineList
                        where p.SaleEnquiryNo.ToLower().Contains(term.ToLower())
                        group new { p } by p.SaleEnquiryHeaderId into g
                        select new SaleEnquiryLineListViewModel
                        {
                            DocNo = g.Max(m => m.p.SaleEnquiryNo),
                            SaleEnquiryHeaderId = g.Key
                        }
                          );

            return list.ToList();
        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new SaleEnquirySettingsService(_unitOfWork).GetSaleEnquirySettingsForDucument(DocTypeId, DivisionId, SiteId);

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
    }
}

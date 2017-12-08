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
    public interface IStockHeaderService : IDisposable
    {
        StockHeader Create(StockHeader s);
        void Delete(int id);
        void Delete(StockHeader s);
        StockHeader Find(int id);
        void Update(StockHeader s);
        string GetMaxDocNo();
        StockHeader FindByDocNo(string Docno);
        StockHeader FindByDocHeader(int? DocHeaderId, int? StockHeaderId, int DocTypeId, int SiteId, int DivisionId);
        IQueryable<StockHeaderViewModel> GetStockHeaderList(int DocTypeId, string UName);
        IQueryable<StockHeaderViewModel> GetStockHeaderListPendingToSubmit(int DocTypeId, string UName);
        IQueryable<StockHeaderViewModel> GetStockHeaderListPendingToReview(int DocTypeId, string UName);
        StockHeaderViewModel GetStockHeader(int id);

        void UpdateStockHeader(StockHeaderViewModel S);
        string GetPersonName(int id);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term, int? ProcessId = null);
        IEnumerable<DocumentTypeHeaderAttributeViewModel> GetDocumentHeaderAttribute(int id);
    }
    public class StockHeaderService : IStockHeaderService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public StockHeaderService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

        public StockHeader Create(StockHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<StockHeader>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<StockHeader>().Delete(id);
        }


        public void Delete(int id, ApplicationDbContext Context)
        {
            //_unitOfWork.Repository<StockHeader>().Delete(id);
            StockHeader stockheader = (from H in Context.StockHeader where H.StockHeaderId == id select H).FirstOrDefault();
            stockheader.ObjectState = Model.ObjectState.Deleted;
            Context.StockHeader.Attach(stockheader);
            Context.StockHeader.Remove(stockheader);
        }

        public void Delete(StockHeader s)
        {
            _unitOfWork.Repository<StockHeader>().Delete(s);
        }
        public void Update(StockHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<StockHeader>().Update(s);
        }
        public StockHeader Find(int id)
        {
            return _unitOfWork.Repository<StockHeader>().Find(id);
        }


        public StockHeader FindByDocNo(string Docno)
        {
            return _unitOfWork.Repository<StockHeader>().Query().Get().Where(m => m.DocNo == Docno).FirstOrDefault();

        }

        public StockHeader FindByDocHeader(int? DocHeaderId, int? StockHeaderId, int DocTypeId, int SiteId, int DivisionId)
        {
            if (DocHeaderId != null)
                return _unitOfWork.Repository<StockHeader>().Query().Get().Where(m => m.DocHeaderId == DocHeaderId && m.DocTypeId == DocTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId).FirstOrDefault();
            else
                return _unitOfWork.Repository<StockHeader>().Query().Get().Where(m => m.StockHeaderId == StockHeaderId && m.DocTypeId == DocTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId).FirstOrDefault();

        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<StockHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<StockHeaderViewModel> GetStockHeaderList(int DocTypeId, string UName)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from p in db.StockHeader
                    join prod1 in db.Product on p.MachineId equals prod1.ProductId
                    into ProductTable from ProductTab in ProductTable.DefaultIfEmpty()
                    orderby p.DocDate descending, p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == DocTypeId
                    select new StockHeaderViewModel
                    {
                        CurrencyName = p.Currency.Name,
                        DivisionName = p.Division.DivisionName,
                        DocDate = p.DocDate,
                        DocHeaderId = p.DocHeaderId,
                        MachineName = ProductTab.ProductName,
                        CostCenterName = p.CostCenter.CostCenterName,
                        DocNo = p.DocNo,
                        DocTypeId = p.DocTypeId,
                        DocTypeName = p.DocType.DocumentTypeName,
                        FromGodownName = p.FromGodown.GodownName,
                        GodownName = p.Godown.GodownName,
                        PersonName = p.Person.Name,
                        ProcessName = p.Process.ProcessName,
                        Remark = p.Remark,
                        Status = p.Status,
                        StockHeaderId = p.StockHeaderId,
                        ModifiedBy = p.ModifiedBy,
                        GatePassDocNo = p.GatePassHeader.DocNo,
                        GatePassHeaderId = p.GatePassHeaderId,
                        GatePassDocDate = p.GatePassHeader.DocDate,
                        GatePassStatus = (p.GatePassHeaderId != null ? p.GatePassHeader.Status : 0),
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(UName, p.ReviewBy) > 0),
                        TotalQty = p.StockLines.Sum(m => m.Qty),
                        DecimalPlaces = (from o in p.StockLines
                                         join prod in db.Product on o.ProductId equals prod.ProductId
                                         join u in db.Units on prod.UnitId equals u.UnitId
                                         select u.DecimalPlaces).Max(),
                    }
                        );

        }

        public IQueryable<StockHeaderViewModel> GetStockHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var StockHeader = GetStockHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in StockHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Import || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<StockHeaderViewModel> GetStockHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var StockHeader = GetStockHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in StockHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public StockHeaderViewModel GetStockHeader(int id)
        {

            return (from p in db.StockHeader
                    where p.StockHeaderId == id
                    select new StockHeaderViewModel
                    {
                        CurrencyId = p.CurrencyId,
                        DocDate = p.DocDate,
                        DocHeaderId = p.DocHeaderId,
                        DocNo = p.DocNo,
                        DocTypeId = p.DocTypeId,
                        MachineId = p.MachineId,
                        CostCenterId = p.CostCenterId,
                        FromGodownId = p.FromGodownId,
                        SiteId = p.SiteId,
                        DivisionId = p.DivisionId,
                        GodownId = p.GodownId,
                        PersonId = p.PersonId,
                        ProcessId = p.ProcessId,
                        Remark = p.Remark,
                        Status = p.Status,
                        StockHeaderId = p.StockHeaderId,
                        DocTypeName = p.DocType.DocumentTypeName,
                        GatePassHeaderId = p.GatePassHeaderId,
                        GatePassDocNo = p.GatePassHeader.DocNo,
                        GatePassStatus = (p.GatePassHeader == null ? 0 : p.GatePassHeader.Status),
                        GatePassDocDate = p.GatePassHeader.DocDate,
                        ModifiedBy = p.ModifiedBy,
                        CreatedDate = p.CreatedDate,
                        LockReason = p.LockReason,
                    }
                        ).FirstOrDefault();

        }

        public void UpdateStockHeader(StockHeaderViewModel S)
        {
            StockHeader StockHeader = Find(S.StockHeaderId);

            StockHeader.DocTypeId = S.DocTypeId;
            StockHeader.DocDate = S.DocDate;
            StockHeader.DocNo = S.DocNo;
            StockHeader.DivisionId = S.DivisionId;
            StockHeader.SiteId = S.SiteId;
            StockHeader.CurrencyId = S.CurrencyId;
            StockHeader.PersonId = S.PersonId;
            StockHeader.ProcessId = S.ProcessId;
            StockHeader.FromGodownId = S.FromGodownId;
            StockHeader.GodownId = S.GodownId;
            StockHeader.Remark = S.Remark;
            StockHeader.Status = S.Status;
            StockHeader.ModifiedBy = S.ModifiedBy;
            StockHeader.ModifiedDate = S.ModifiedDate;
            StockHeader.CostCenterId = S.CostCenterId;
            StockHeader.MachineId = S.MachineId;


            Update(StockHeader);

        }

        public string GetPersonName(int id)
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var Settings = (from p in db.MaterialIssueSettings
                            where p.DocTypeId == id && p.SiteId == SiteId && p.DivisionId == DivisionId
                            select p).FirstOrDefault();

            return Settings != null ? (string.IsNullOrEmpty(Settings.PersonFieldHeading) ? "Person" : Settings.PersonFieldHeading) : "Person";


        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term, int? ProcessId = null)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(DocTypeId, DivisionId, SiteId);

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
                        where (ProcessId == null ? 1 == 1 : PersonProcessTab.ProcessId == ProcessId)
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

        public IEnumerable<DocumentTypeHeaderAttributeViewModel> GetDocumentHeaderAttribute(int id)
        {
            var Header = db.StockHeader.Find(id);

            var temp = from Dta in db.DocumentTypeHeaderAttribute
                       join Ha in db.StockHeaderAttributes on Dta.DocumentTypeHeaderAttributeId equals Ha.DocumentTypeHeaderAttributeId into HeaderAttributeTable
                       from HeaderAttributeTab in HeaderAttributeTable.Where(m => m.HeaderTableId == id).DefaultIfEmpty()
                       where (Dta.DocumentTypeId == Header.DocTypeId)
                       select new DocumentTypeHeaderAttributeViewModel
                       {
                           ListItem = Dta.ListItem,
                           DataType = Dta.DataType,
                           Value = HeaderAttributeTab.Value,
                           Name = Dta.Name,
                           DocumentTypeHeaderAttributeId = Dta.DocumentTypeHeaderAttributeId,
                       };

            return temp;
        }

        public void Dispose()
        {
        }
    }
}

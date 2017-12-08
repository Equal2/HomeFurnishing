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
    public interface IPurchaseGoodsReturnHeaderService : IDisposable
    {
        PurchaseGoodsReturnHeader Create(PurchaseGoodsReturnHeader pt);
        void Delete(int id);
        void Delete(PurchaseGoodsReturnHeader pt);
        PurchaseGoodsReturnHeader Find(string Name);
        PurchaseGoodsReturnHeader Find(int id);
        PurchaseGoodsReturnHeader FindByInvioceReturn(int id);
        IEnumerable<PurchaseGoodsReturnHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseGoodsReturnHeader pt);
        PurchaseGoodsReturnHeader Add(PurchaseGoodsReturnHeader pt);
        PurchaseGoodsReturnHeaderViewModel GetPurchaseGoodsReturnHeader(int id);//HeadeRId
        IQueryable<PurchaseGoodsReturnHeaderViewModel> GetPurchaseGoodsReturnHeaderList(int id, string Uname);
        IQueryable<PurchaseGoodsReturnHeaderViewModel> GetPurchaseGoodsReturnPendingToSubmit(int id, string Uname);
        IQueryable<PurchaseGoodsReturnHeaderViewModel> GetPurchaseGoodsReturnPendingToReview(int id, string Uname);
        Task<IEquatable<PurchaseGoodsReturnHeader>> GetAsync();
        Task<PurchaseGoodsReturnHeader> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();

        IEnumerable<PurchaseGoodsReturnLine> GetPurchaseGoodsReturnLineList(int PurchaseGoodsReturnHeaderId);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class PurchaseGoodsReturnHeaderService : IPurchaseGoodsReturnHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseGoodsReturnHeader> _PurchaseGoodsReturnHeaderRepository;
        RepositoryQuery<PurchaseGoodsReturnHeader> PurchaseGoodsReturnHeaderRepository;
        public PurchaseGoodsReturnHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseGoodsReturnHeaderRepository = new Repository<PurchaseGoodsReturnHeader>(db);
            PurchaseGoodsReturnHeaderRepository = new RepositoryQuery<PurchaseGoodsReturnHeader>(_PurchaseGoodsReturnHeaderRepository);
        }

        public PurchaseGoodsReturnHeader Find(string Name)
        {
            return PurchaseGoodsReturnHeaderRepository.Get().Where(i => i.DocNo == Name).FirstOrDefault();
        }


        public PurchaseGoodsReturnHeader Find(int id)
        {
            return _unitOfWork.Repository<PurchaseGoodsReturnHeader>().Find(id);
        }

        public PurchaseGoodsReturnHeader FindByInvioceReturn(int id)
        {
            return (from p in db.PurchaseGoodsReturnHeader
                    join pi in db.PurchaseInvoiceReturnHeader on p.PurchaseGoodsReturnHeaderId equals pi.PurchaseGoodsReturnHeaderId
                    where pi.PurchaseInvoiceReturnHeaderId == id
                    select p).FirstOrDefault();
        }

        public PurchaseGoodsReturnHeader Create(PurchaseGoodsReturnHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseGoodsReturnHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseGoodsReturnHeader>().Delete(id);
        }

        public void Delete(PurchaseGoodsReturnHeader pt)
        {
            _unitOfWork.Repository<PurchaseGoodsReturnHeader>().Delete(pt);
        }

        public void Update(PurchaseGoodsReturnHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseGoodsReturnHeader>().Update(pt);
        }

        public IEnumerable<PurchaseGoodsReturnHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseGoodsReturnHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public PurchaseGoodsReturnHeaderViewModel GetPurchaseGoodsReturnHeader(int id)
        {
            return (from p in db.PurchaseGoodsReturnHeader
                    where p.PurchaseGoodsReturnHeaderId == id
                    select new PurchaseGoodsReturnHeaderViewModel
                    {
                        PurchaseGoodsReturnHeaderId = p.PurchaseGoodsReturnHeaderId,
                        DivisionId = p.DivisionId,
                        DocNo = p.DocNo,
                        DocDate = p.DocDate,
                        DocTypeId = p.DocTypeId,
                        Remark = p.Remark,
                        SiteId = p.SiteId,
                        GodownId=p.GodownId,
                        Status = p.Status,
                        SupplierId = p.SupplierId,                                              
                        ReasonId=p.ReasonId,
                        ModifiedBy=p.ModifiedBy,
                        GatePassHeaderId = p.GatePassHeaderId,
                        GatePassDocNo = p.GatePassHeader.DocNo,
                        GatePassStatus = (p.GatePassHeader == null ? 0 : p.GatePassHeader.Status),
                        GatePassDocDate = p.GatePassHeader.DocDate,
                        CreatedBy=p.CreatedBy,
                        CreatedDate=p.CreatedDate,
                        LockReason=p.LockReason,
                    }

                        ).FirstOrDefault();
        }
        public IQueryable<PurchaseGoodsReturnHeaderViewModel> GetPurchaseGoodsReturnHeaderList(int id, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"]; 

            var pt = (from p in db.PurchaseGoodsReturnHeader
                      join t in db._Users on p.ModifiedBy equals t.UserName into table3
                      from tab3 in table3.DefaultIfEmpty()
                      join t in db.DocumentType on p.DocTypeId equals t.DocumentTypeId into table
                      from tab in table.DefaultIfEmpty()
                      join t1 in db.Persons on p.SupplierId equals t1.PersonID into table2
                      from tab2 in table2.DefaultIfEmpty()
                      orderby p.DocDate descending, p.DocNo descending
                      where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id 
                      select new PurchaseGoodsReturnHeaderViewModel
                      {
                          DocDate = p.DocDate,
                          DocNo = p.DocNo,
                          DocTypeName = tab.DocumentTypeName,
                          PurchaseGoodsReturnHeaderId = p.PurchaseGoodsReturnHeaderId,
                          Remark = p.Remark,
                          Status = p.Status,
                          SupplierName = tab2.Name,
                          ModifiedBy = p.ModifiedBy,
                          FirstName = tab3.FirstName,
                          ReviewCount = p.ReviewCount,
                          ReviewBy = p.ReviewBy,
                          Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                          GatePassDocNo = p.GatePassHeader.DocNo,
                          GatePassHeaderId = p.GatePassHeaderId,
                          GatePassDocDate = p.GatePassHeader.DocDate,
                          GatePassStatus = (p.GatePassHeaderId != null ? p.GatePassHeader.Status : 0),
                      }
                         );
            return pt;
        }

        public PurchaseGoodsReturnHeader Add(PurchaseGoodsReturnHeader pt)
        {
            _unitOfWork.Repository<PurchaseGoodsReturnHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseGoodsReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseGoodsReturnHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseGoodsReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseGoodsReturnHeaderId).FirstOrDefault();
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

                temp = (from p in db.PurchaseGoodsReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseGoodsReturnHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseGoodsReturnHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseGoodsReturnHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<PurchaseGoodsReturnHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<PurchaseGoodsReturnHeaderViewModel> GetPurchaseGoodsReturnPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseGoodsReturnHeader = GetPurchaseGoodsReturnHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in PurchaseGoodsReturnHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Import || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<PurchaseGoodsReturnHeaderViewModel> GetPurchaseGoodsReturnPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var PurchaseGoodsReturnHeader = GetPurchaseGoodsReturnHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in PurchaseGoodsReturnHeader
                                   where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        }

        public IEnumerable<PurchaseGoodsReturnLine> GetPurchaseGoodsReturnLineList(int PurchaseGoodsReturnHeaderId)
        {
            IEnumerable<PurchaseGoodsReturnLine> PurchaseGoodsReturnLineList = (from L in db.PurchaseGoodsReturnLine
                                                                                  where L.PurchaseGoodsReturnHeaderId == PurchaseGoodsReturnHeaderId
                                                                                  select L).ToList();
            return PurchaseGoodsReturnLineList;
        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(DocTypeId, DivisionId, SiteId);

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


        public Task<IEquatable<PurchaseGoodsReturnHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseGoodsReturnHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

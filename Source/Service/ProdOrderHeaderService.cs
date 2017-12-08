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
using System.Data.SqlClient;
using System.Configuration;
using Model.ViewModels;
using System.Data.Entity.SqlServer;

namespace Service
{
    public interface IProdOrderHeaderService : IDisposable
    {
        ProdOrderHeader Create(ProdOrderHeader pt);
        void Delete(int id);
        void Delete(ProdOrderHeader pt);
        ProdOrderHeader Find(string Name);
        ProdOrderHeader Find(int id);
        IEnumerable<ProdOrderHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProdOrderHeader pt);
        ProdOrderHeader Add(ProdOrderHeader pt);
        IQueryable<ProdOrderHeaderViewModel> GetProdOrderHeaderList(int id, string Uname);
        IQueryable<ProdOrderHeaderViewModel> GetProdOrderHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<ProdOrderHeaderViewModel> GetProdOrderHeaderListPendingToReview(int id, string Uname);
        Task<IEquatable<ProdOrderHeader>> GetAsync();
        Task<ProdOrderHeader> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        ProdOrderHeaderViewModel GetProdOrderHeader(int id);//HeaderId
        string GetMaxDocNo();

        /// <summary>
        ///Get the ProductionOrderHeader based on the materialplan headerid
        /// </summary>
        /// <param name="id">MaterialPlanHeaderId</param>        
        ProdOrderHeader GetProdOrderForMaterialPlan(int id);
        IEnumerable<ProdOrderHeaderViewModel> GetProdOrdersForDocumentType(string term,int DocTypeId,string ProcName);//DocTypeIds

        ProdOrderHeader GetPurchOrProdForMaterialPlan(int id, int PurchOrProdDocTypeId);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
        
    }

    public class ProdOrderHeaderService : IProdOrderHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProdOrderHeader> _ProdOrderHeaderRepository;
        RepositoryQuery<ProdOrderHeader> ProdOrderHeaderRepository;
        public ProdOrderHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProdOrderHeaderRepository = new Repository<ProdOrderHeader>(db);
            ProdOrderHeaderRepository = new RepositoryQuery<ProdOrderHeader>(_ProdOrderHeaderRepository);
        }
        public ProdOrderHeader Find(string Name)
        {
            return ProdOrderHeaderRepository.Get().Where(i => i.DocNo == Name).FirstOrDefault();
        }


        public ProdOrderHeader Find(int id)
        {
            return _unitOfWork.Repository<ProdOrderHeader>().Find(id);
        }

        public ProdOrderHeader Create(ProdOrderHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProdOrderHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProdOrderHeader>().Delete(id);
        }
        public IEnumerable<ProdOrderHeader> GetProdOrderListForMaterialPlan(int id)
        {
            return (from p in db.ProdOrderHeader
                    where p.MaterialPlanHeaderId == id
                    select p);
        }

        public void Delete(ProdOrderHeader pt)
        {
            _unitOfWork.Repository<ProdOrderHeader>().Delete(pt);
        }

        public void Update(ProdOrderHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProdOrderHeader>().Update(pt);
        }

        public IEnumerable<ProdOrderHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProdOrderHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<ProdOrderHeaderViewModel> GetProdOrderHeaderList(int id, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from p in db.ProdOrderHeader
                    join t in db.DocumentType on p.DocTypeId equals t.DocumentTypeId into table
                    from tab in table.DefaultIfEmpty()
                    where p.DivisionId==DivisionId && p.SiteId==SiteId && p.DocTypeId==id
                    orderby p.DocDate descending, p.DocNo descending
                    select new ProdOrderHeaderViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        DocTypeName = tab.DocumentTypeName,
                        DueDate = p.DueDate,
                        ProdOrderHeaderId = p.ProdOrderHeaderId,
                        Remark = p.Remark,
                        BuyerName = p.Buyer.Name,
                        Status = p.Status,
                        ModifiedBy = p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                    }
                        );
            
        }

        public IQueryable<ProdOrderHeaderViewModel> GetProdOrderHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var ProdOrderHeader = GetProdOrderHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in ProdOrderHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<ProdOrderHeaderViewModel> GetProdOrderHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var ProdOrderHeader = GetProdOrderHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in ProdOrderHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }


        public ProductionOrderSettings GetProductionOrderSettingsForDocument(int DocTypeId, int DivisionId, int SiteId)
        {
            return (from p in db.ProductionOrderSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }

        public ProdOrderHeaderViewModel GetProdOrderHeader(int id)
        {
            return (from p in db.ProdOrderHeader
                    join t in db.MaterialPlanHeader on p.MaterialPlanHeaderId equals t.MaterialPlanHeaderId into table from tab in table.DefaultIfEmpty()
                    where p.ProdOrderHeaderId == id
                    select new ProdOrderHeaderViewModel
                    {
                        MaterialPlanDocNo=tab.DocNo,
                        DivisionId = p.DivisionId,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        DocTypeId = p.DocTypeId,
                        DueDate = p.DueDate,
                        BuyerId = p.BuyerId,
                        ProdOrderHeaderId = p.ProdOrderHeaderId,
                        Remark = p.Remark,
                        SiteId = p.SiteId,
                        Status = p.Status,
                        ModifiedBy=p.ModifiedBy,
                        CreatedDate=p.CreatedDate,
                        LockReason=p.LockReason,
                    }
                        ).FirstOrDefault();
        }

        public ProdOrderHeader Add(ProdOrderHeader pt)
        {
            _unitOfWork.Repository<ProdOrderHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProdOrderHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.ProdOrderHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProdOrderHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.ProdOrderHeaderId).FirstOrDefault();
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

                temp = (from p in db.ProdOrderHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.ProdOrderHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProdOrderHeader
                        orderby p.DocDate descending,p.DocNo descending
                        select p.ProdOrderHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public IEnumerable<ProdOrderHeaderListViewModel> GetPendingProdOrders(int id)
        {
            return (from p in db.ViewProdOrderBalance
                    join t in db.ProdOrderHeader on p.ProdOrderHeaderId equals t.ProdOrderHeaderId into table
                    from tab in table.DefaultIfEmpty()
                    join t1 in db.ProdOrderLine on p.ProdOrderLineId equals t1.ProdOrderLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    where p.ProductId == id && p.BalanceQty > 0
                    select new ProdOrderHeaderListViewModel
                    {
                        ProdOrderLineId= p.ProdOrderLineId,
                        ProdOrderHeaderId= p.ProdOrderHeaderId,
                        DocNo = tab.DocNo,
                        Dimension1Name = tab1.Dimension1.Dimension1Name,
                        Dimension2Name = tab1.Dimension2.Dimension2Name,
                    }
                        );
        }
        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<ProdOrderHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public ProdOrderHeader GetProdOrderForMaterialPlan(int id)
        {
            return (from p in db.ProdOrderHeader
                    where p.MaterialPlanHeaderId == id
                    select p
                        ).FirstOrDefault();
        }


        public IEnumerable<ProdOrderHeaderViewModel> GetProdOrdersForDocumentType( string term,int DocHeaderId,string ProcName)//DocTypeId
        {
            SqlParameter SqlParameterProductId = new SqlParameter("@MaterialPlanHeaderId", DocHeaderId);

            IEnumerable<ProdOrderBalanceViewModel> StockAvailableForPacking = db.Database.SqlQuery<ProdOrderBalanceViewModel>(" " + ProcName + " @MaterialPlanHeaderId", SqlParameterProductId).ToList();

            var temp = from p in StockAvailableForPacking                       
                       where  p.BalanceQty > 0 && p.ProdOrderNo.ToLower().Contains(term.ToLower())
                       group new { p } by p.ProdOrderHeaderId into g
                       orderby g.Key descending
                       select new ProdOrderHeaderViewModel
                       {
                           ProdOrderHeaderId = g.Key,
                           DocNo = g.Max(m => m.p.ProdOrderNo) +" | "+ g.Max(m => m.p.DocTypeName),                           
                       };

            return temp;

        }

        public ProdOrderHeader GetPurchOrProdForMaterialPlan(int id, int PurchOrProdDocTypeId)
        {
            return (from p in db.ProdOrderHeader
                    where p.MaterialPlanHeaderId == id && p.DocTypeId == PurchOrProdDocTypeId
                    select p
                ).FirstOrDefault();
        }


        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new ProdOrderSettingsService(_unitOfWork).GetProdOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);

            string[] PersonRoles = null;
            if (!string.IsNullOrEmpty(settings.filterPersonRoles)) { PersonRoles = settings.filterPersonRoles.Split(",".ToCharArray()); }
            else { PersonRoles = new string[] { "NA" }; }

            string DivIdStr = "|" + DivisionId.ToString() + "|";
            string SiteIdStr = "|" + SiteId.ToString() + "|";

            int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Sales).ProcessId;

            var list = (from p in db.Persons
                        join bus in db.BusinessEntity on p.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join pp in db.PersonProcess on p.PersonID equals pp.PersonId into PersonProcessTable
                        from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                        join pr in db.PersonRole on p.PersonID equals pr.PersonId into PersonRoleTable
                        from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                        where PersonProcessTab.ProcessId == ProcessId
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


        public Task<IEquatable<ProdOrderHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProdOrderHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

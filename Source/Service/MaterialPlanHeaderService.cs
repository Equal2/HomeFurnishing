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
    public interface IMaterialPlanHeaderService : IDisposable
    {
        MaterialPlanHeader Create(MaterialPlanHeader pt);
        void Delete(int id);
        void Delete(MaterialPlanHeader pt);
        MaterialPlanHeader Find(int id);
        IEnumerable<MaterialPlanHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialPlanHeader pt);
        MaterialPlanHeader Add(MaterialPlanHeader pt);
        IQueryable<MaterialPlanHeaderViewModel> GetMaterialPlanHeaderList(int DocTypeId, string Uname);
        IQueryable<MaterialPlanHeaderViewModel> GetMaterialPlanHeaderListPendingToSubmit(int DocTypeId, string Uname);
        IQueryable<MaterialPlanHeaderViewModel> GetMaterialPlanHeaderListPendingToReview(int DocTypeId, string Uname);
        Task<IEquatable<MaterialPlanHeader>> GetAsync();
        Task<MaterialPlanHeader> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class MaterialPlanHeaderService : IMaterialPlanHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialPlanHeader> _MaterialPlanHeaderRepository;
        RepositoryQuery<MaterialPlanHeader> MaterialPlanHeaderRepository;
        public MaterialPlanHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialPlanHeaderRepository = new Repository<MaterialPlanHeader>(db);
            MaterialPlanHeaderRepository = new RepositoryQuery<MaterialPlanHeader>(_MaterialPlanHeaderRepository);
        }


        public MaterialPlanHeader Find(int id)
        {
            return _unitOfWork.Repository<MaterialPlanHeader>().Find(id);
        }

        public MaterialPlanHeader Create(MaterialPlanHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialPlanHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialPlanHeader>().Delete(id);
        }

        public void Delete(MaterialPlanHeader pt)
        {
            _unitOfWork.Repository<MaterialPlanHeader>().Delete(pt);
        }

        public void Update(MaterialPlanHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialPlanHeader>().Update(pt);
        }

        public IEnumerable<MaterialPlanHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialPlanHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<MaterialPlanHeaderViewModel> GetMaterialPlanHeaderList(int DocTypeId, string Uname)
        {

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from p in db.MaterialPlanHeader
                    join PR in db.Persons on p.BuyerId  equals PR.PersonID  into PRtable
                    from PRtab in PRtable.DefaultIfEmpty()
                    orderby p.DocDate descending, p.DocNo descending
                    where p.DocTypeId == DocTypeId && p.SiteId == SiteId && p.DivisionId == DivisionId
                    select new MaterialPlanHeaderViewModel
                    {
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        DocTypeName = p.DocType.DocumentTypeName,
                        DueDate = p.DueDate,
                        BuyerName =PRtab.Name, 
                        MaterialPlanHeaderId = p.MaterialPlanHeaderId,
                        Remark = p.Remark,
                        Status = p.Status,
                        ModifiedBy=p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                    }
                        );

        }

        public IQueryable<MaterialPlanHeaderViewModel> GetMaterialPlanHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetMaterialPlanHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<MaterialPlanHeaderViewModel> GetMaterialPlanHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var LedgerHeader = GetMaterialPlanHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in LedgerHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public MaterialPlanHeader Add(MaterialPlanHeader pt)
        {
            _unitOfWork.Repository<MaterialPlanHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialPlanHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.MaterialPlanHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.MaterialPlanHeaderId).FirstOrDefault();
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

                temp = (from p in db.MaterialPlanHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.MaterialPlanHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.MaterialPlanHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<MaterialPlanHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(DocTypeId, DivisionId, SiteId);

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


        public Task<IEquatable<MaterialPlanHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialPlanHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

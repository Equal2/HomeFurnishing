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
using Model.ViewModels;
using Model.ViewModel;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;

namespace Service
{
    public interface IJobReceiveHeaderService : IDisposable
    {
        JobReceiveHeader Create(JobReceiveHeader pt);
        void Delete(int id);
        void Delete(JobReceiveHeader pt);
        JobReceiveHeader Find(int id);
        IEnumerable<JobReceiveHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobReceiveHeader pt);
        JobReceiveHeader Add(JobReceiveHeader pt);
        IQueryable<JobReceiveIndexViewModel> GetJobReceiveHeaderList(int DocTypeId, string Uname);//DocumentTypeId
        IQueryable<JobReceiveIndexViewModel> GetJobReceiveHeaderListPendingToSubmit(int DocTypeId, string Uname);//DocumentTypeId
        IQueryable<JobReceiveIndexViewModel> GetJobReceiveHeaderListPendingToReview(int DocTypeId, string Uname);//DocumentTypeId
        JobReceiveHeaderViewModel GetJobReceiveHeader(int id);

        // IEnumerable<JobReceiveHeader> GetJobReceiveHeaderList(int buyerId);
        Task<IEquatable<JobReceiveHeader>> GetAsync();
        Task<JobReceiveHeader> FindAsync(int id);
        JobReceiveHeader GetJobReceiveHeaderByName(string terms);

        void UpdateProdUidJobWorkers(ref ApplicationDbContext context, JobReceiveHeader Header);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();
        IEnumerable<WeavingReceiveWizardViewModel> GetJobOrdersForWeavingReceiveWizard(int DocTypeId);
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term);
    }

    public class JobReceiveHeaderService : IJobReceiveHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobReceiveHeader> _JobReceiveHeaderRepository;
        RepositoryQuery<JobReceiveHeader> JobReceiveHeaderRepository;
        public JobReceiveHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobReceiveHeaderRepository = new Repository<JobReceiveHeader>(db);
            JobReceiveHeaderRepository = new RepositoryQuery<JobReceiveHeader>(_JobReceiveHeaderRepository);
        }
        public JobReceiveHeader GetJobReceiveHeaderByName(string terms)
        {
            return (from p in db.JobReceiveHeader
                    where p.DocNo == terms
                    select p).FirstOrDefault();
        }


        public JobReceiveHeader Find(int id)
        {
            return _unitOfWork.Repository<JobReceiveHeader>().Find(id);
        }

        public JobReceiveHeader Create(JobReceiveHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobReceiveHeader>().Insert(pt);
            return pt;
        }
        public JobReceiveHeaderViewModel GetJobReceiveHeader(int id)
        {
            return (from p in db.JobReceiveHeader
                    where p.JobReceiveHeaderId == id
                    select new JobReceiveHeaderViewModel
                    {
                        DivisionId=p.DivisionId,
                        DivisionName=p.Division.DivisionName,
                        DocTypeName=p.DocType.DocumentTypeName,
                        GodownName=p.Godown.GodownName,
                        JobReceiveByName=p.JobReceiveBy.Person.Name,
                        JobReceiveById=p.JobReceiveById,
                        JobWorkerId=p.JobWorkerId,
                        JobWorkerName=p.JobWorker.Name,
                        ProcessId=p.ProcessId,
                        ProcessName=p.Process.ProcessName,
                        Status=p.Status,
                        JobWorkerDocNo=p.JobWorkerDocNo,
                        JobWorkerDocDate = p.JobWorkerDocDate,
                        SiteId=p.SiteId,
                        SiteName=p.Site.SiteName,
                        JobReceiveHeaderId=p.JobReceiveHeaderId,
                        DocTypeId = p.DocTypeId,
                        DocDate = p.DocDate,
                        DocNo = p.DocNo,
                        GodownId=p.GodownId,
                        Remark=p.Remark,
                        ModifiedBy=p.ModifiedBy,
                        LockReason=p.LockReason,
                        CreatedDate=p.CreatedDate,
                    }).FirstOrDefault();
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobReceiveHeader>().Delete(id);
        }

        public void Delete(JobReceiveHeader pt)
        {
            _unitOfWork.Repository<JobReceiveHeader>().Delete(pt);
        }

        public void Update(JobReceiveHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobReceiveHeader>().Update(pt);
        }

        public IEnumerable<JobReceiveHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobReceiveHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<JobReceiveIndexViewModel> GetJobReceiveHeaderList(int DocTypeId, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in db.JobReceiveHeader
                    orderby p.DocDate descending,p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == DocTypeId
                    select new JobReceiveIndexViewModel
                    {
                        JobReceiveHeaderId=p.JobReceiveHeaderId,
                        DocDate = p.DocDate,
                        JobWorkerDocNo=p.JobWorkerDocNo,
                        JobWorkerDocDate = p.JobWorkerDocDate,
                        DocNo = p.DocNo,
                        JobWorkerName = p.JobWorker.Name + ", " + p.JobWorker.Suffix + " [" + p.JobWorker.Code + "]",
                        DocTypeName = p.DocType.DocumentTypeName,
                        Remark = p.Remark,
                        Status=p.Status,
                        ModifiedBy=p.ModifiedBy,
                        ReviewCount = p.ReviewCount,
                        ReviewBy=p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                        TotalQty = p.JobReceiveLines.Sum(m => m.Qty),
                        DecimalPlaces = (from o in p.JobReceiveLines
                                         join ol in db.JobOrderLine on o.JobOrderLineId equals ol.JobOrderLineId
                                         join prod in db.Product on ol.ProductId equals prod.ProductId
                                         join u in db.Units on prod.UnitId equals u.UnitId
                                         select u.DecimalPlaces).Max(),
                    }
                );
        }

        public JobReceiveHeader Add(JobReceiveHeader pt)
        {
            _unitOfWork.Repository<JobReceiveHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobReceiveHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobReceiveHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobReceiveHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobReceiveHeaderId).FirstOrDefault();
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

                temp = (from p in db.JobReceiveHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobReceiveHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobReceiveHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobReceiveHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<JobReceiveHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }


        public IEnumerable<JobReceiveListViewModel> GetPendingReceipts(int id, int Jid)
        {
            return (from p in db.ViewJobReceiveBalance
                    join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId into table
                    from tab in table.DefaultIfEmpty()
                    join t1 in db.JobOrderLine on p.JobOrderLineId equals t1.JobOrderLineId into table1
                    from tab1 in table1.DefaultIfEmpty()
                    where p.ProductId == id && p.BalanceQty > 0 && p.JobWorkerId==Jid
                    select new JobReceiveListViewModel
                    {
                        JobReceiveLineId = p.JobReceiveLineId,
                        JobReceiveHeaderId = p.JobReceiveHeaderId,
                        DocNo = tab.DocNo,
                        JobWorkerDocNo = tab.JobWorkerDocNo,
                        JobWorkerDocDate = tab.JobWorkerDocDate,
                        Dimension1Name = tab1.Dimension1.Dimension1Name,
                        Dimension2Name = tab1.Dimension2.Dimension2Name,
                        Dimension3Name = tab1.Dimension3.Dimension3Name,
                        Dimension4Name = tab1.Dimension4.Dimension4Name,
                    }
                        );
        }

        public IEnumerable<JobReceiveHeaderListViewModel> GetPendingJobReceivesWithPatternMatch(int JobWorkerId, string term, int Limiter)//Product Id
        {
            var tem = (from p in db.ViewJobReceiveBalance
                       where p.BalanceQty > 0 && p.JobWorkerId == JobWorkerId
                       && ((string.IsNullOrEmpty(term) ? 1 == 1 : p.JobReceiveNo.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension1.Dimension1Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension2.Dimension2Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension3.Dimension3Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : p.Dimension4.Dimension4Name.ToLower().Contains(term.ToLower()))
                       || (string.IsNullOrEmpty(term) ? 1 == 1 : p.Product.ProductName.ToLower().Contains(term.ToLower())))
                       orderby p.JobReceiveNo
                       select new JobReceiveHeaderListViewModel
                       {
                           DocNo = p.JobReceiveNo,
                           JobReceiveLineId = p.JobReceiveLineId,
                           Dimension1Name = p.Dimension1.Dimension1Name,
                           Dimension2Name = p.Dimension2.Dimension2Name,
                           Dimension3Name = p.Dimension3.Dimension3Name,
                           Dimension4Name = p.Dimension4.Dimension4Name,
                           ProductName = p.Product.ProductName,

                       }).Take(Limiter);

            return (tem);
        }

        public IQueryable<JobReceiveIndexViewModel> GetJobReceiveHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobReceiveHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in JobOrderHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Import || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }
        public IQueryable<JobReceiveIndexViewModel> GetJobReceiveHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobReceiveHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in JobOrderHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        }


        public IEnumerable<WeavingReceiveWizardViewModel> GetJobOrdersForWeavingReceiveWizard(int DocTypeId)//DocTypeId
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);
            SqlParameter SqlParameterDocTypeId = new SqlParameter("@DocumentTypeId", DocTypeId);

            IEnumerable<WeavingReceiveWizardViewModel> temp = db.Database.SqlQuery<WeavingReceiveWizardViewModel>("Web.ProcWeavingReceiveWizard @SiteId, @DivisionId, @DocumentTypeId", SqlParameterSiteId, SqlParameterDivisionId, SqlParameterDocTypeId).ToList();

            return temp;
        }

        public void UpdateProdUidJobWorkers(ref ApplicationDbContext context, JobReceiveHeader Header)
        {
            var Lines = (from p in context.JobReceiveLine
                         where p.JobReceiveHeaderId == Header.JobReceiveHeaderId
                         select p).ToList();

            if (Lines.Count() > 0)
            {
                if (Lines.Where(m => m.ProductUidId != null).Count() > 0)
                {
                    var ProductUids = Lines.Select(m => m.ProductUidId).ToArray();

                    var ProductUidRecords = (from p in context.ProductUid
                                             where ProductUids.Contains(p.ProductUIDId)
                                             && p.LastTransactionDocId == Header.JobReceiveHeaderId
                                             select p).ToList();

                    foreach (var item in ProductUidRecords)
                    {
                        item.LastTransactionPersonId = Header.JobWorkerId;
                        item.LastTransactionDocNo = Header.DocNo;
                        item.LastTransactionDocDate = Header.DocDate;
                        item.CurrenctGodownId = Header.GodownId;
                        item.ObjectState = Model.ObjectState.Modified;

                        context.ProductUid.Add(item);
                    }

                }
            }

        }

        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(DocTypeId, DivisionId, SiteId);

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


        public Task<IEquatable<JobReceiveHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobReceiveHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

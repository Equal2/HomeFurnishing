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
using System.Data.Entity.SqlServer;
using Model.ViewModel;

namespace Service
{
    public interface IQAGroupService : IDisposable
    {
        QAGroup Create(QAGroup pt);
        void Delete(int id);
        void Delete(QAGroup pt);
        QAGroup Find(string Name);
        QAGroup Find(int id);
        IEnumerable<QAGroup> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(QAGroup pt);
        QAGroup Add(QAGroup pt);                
        Task<IEquatable<QAGroup>> GetAsync();
        Task<QAGroup> FindAsync(int id);

        QAGroupViewModel GetQAGroup(int id);
        //IQueryable<QAGroupViewModel> GetPendingGatePassList(int GodownId);

        IQueryable<QAGroupViewModel> GetQAGroupList(int DocumentTypeId, string Uname);

        IQueryable<QAGroupViewModel> GetQAGroupListPendingToSubmit(int DocumentTypeId, string Uname);
        IQueryable<QAGroupViewModel> GetQAGroupListPendingToReview(int DocumentTypeId, string Uname);
        // bool CheckForDocNoExists(int GodownId, string docno, int doctypeId);
        int NextId(int id, int DocTypeId);
        int PrevId(int id, int DocTypeId);
    }

    public class QAGroupService : IQAGroupService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<QAGroup> _QAGroupRepository;
        RepositoryQuery<QAGroup> QAGroupRepository;
        public QAGroupService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _QAGroupRepository = new Repository<QAGroup>(db);
            QAGroupRepository = new RepositoryQuery<QAGroup>(_QAGroupRepository);
        }

        public QAGroup Find(string Name)
        {
            return QAGroupRepository.Get().Where(i => i.QaGroupName == Name).FirstOrDefault();
        }


        public QAGroup Find(int id)
        {
            return _unitOfWork.Repository<QAGroup>().Find(id);
        }

        public QAGroup Create(QAGroup pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<QAGroup>().Insert(pt);
            return pt;
        }     

        public void Delete(int id)
        {
            _unitOfWork.Repository<QAGroup>().Delete(id);
        }

        public void Delete(QAGroup pt)
        {
            _unitOfWork.Repository<QAGroup>().Delete(pt);
        }

        public void Update(QAGroup pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<QAGroup>().Update(pt);
        }

        public IEnumerable<QAGroup> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<QAGroup>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.QaGroupName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public QAGroup Add(QAGroup pt)
        {
            _unitOfWork.Repository<QAGroup>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<QAGroup>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<QAGroup> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        
        


        public IQueryable<QAGroupViewModel> GetQAGroupList(int DocumentTypeId, string Uname)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in db.QAGroup                    
                    join dt in db.DocumentType on p.DocTypeId equals dt.DocumentTypeId                    
                    orderby p.CreatedDate descending, p.QaGroupName descending
                    where p.DocTypeId == DocumentTypeId 
                    select new QAGroupViewModel
                    {
                        DocTypeName = dt.DocumentTypeName,  
                        QaGroupName=p.QaGroupName, 
                        Description=p.Description,
                        Status = p.Status,
                        QAGroupId = p.QAGroupId,
                        ModifiedBy = p.ModifiedBy,
                        ReviewCount = p.ReviewCount,                       
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),                        
                    });
        }

        public IQueryable<QAGroupViewModel> GetQAGroupListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var QAGroup = GetQAGroupList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in QAGroup
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Import || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<QAGroupViewModel> GetQAGroupListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var QAGroup = GetQAGroupList(id, Uname).AsQueryable();

            var PendingToReview = from p in QAGroup
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public QAGroupViewModel GetQAGroup(int id)
        {
            return (from p in db.QAGroup
                    where p.QAGroupId == id
                    select new QAGroupViewModel
                    {
                        DocTypeName = p.DocType.DocumentTypeName,
                        QaGroupName = p.QaGroupName,
                        Description = p.Description,
                        QAGroupId = p.QAGroupId,
                        Status = p.Status,
                        DocTypeId = p.DocTypeId,                        
                        ModifiedBy = p.ModifiedBy,
                        CreatedDate = p.CreatedDate,                        
                    }
                        ).FirstOrDefault();
        }

        public int NextId(int id,int DocTypeId)
        {
            

            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.QAGroup
                        orderby p.QaGroupName
                        where p.DocTypeId == DocTypeId
                        select p.QAGroupId
                     ).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();


            }
            else
            {
                temp = (from p in db.QAGroup
                        orderby p.QaGroupName
                        select p.QAGroupId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id, int DocTypeId)
        {
       

            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.QAGroup
                        orderby p.QAGroupId
                        where p.DocTypeId == DocTypeId
                        select p.QAGroupId
                     ).AsEnumerable().SkipWhile(p => p != id).Skip(1).LastOrDefault();               
            }
            else
            {
                temp = (from p in db.QAGroup
                        orderby p.QaGroupName
                        select p.QAGroupId).LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
    }
}

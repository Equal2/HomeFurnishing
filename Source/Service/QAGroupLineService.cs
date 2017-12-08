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

namespace Service
{
    public interface IQAGroupLineService : IDisposable
    {
        QAGroupLine Create(QAGroupLine pt);
        void Delete(int id);
        void Delete(QAGroupLine pt);
        QAGroupLine Find(int id);
        IEnumerable<QAGroupLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(QAGroupLine pt);
        QAGroupLine Add(QAGroupLine pt);                
        Task<IEquatable<QAGroupLine>> GetAsync();
        Task<QAGroupLine> FindAsync(int id);
        IEnumerable<QAGroupLine> GetQAGroupLineList(int GatePassHeaderId);

        IQueryable<QAGroupLineViewModel> GetQAGroupLineListForIndex(int JobOrderHeaderId);
        QAGroupLineViewModel GetQAGroupLine(int id);
    }

    public class QAGroupLineService : IQAGroupLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<QAGroupLine> _QAGroupLineRepository;
        RepositoryQuery<QAGroupLine> QAGroupLineRepository;
        public QAGroupLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _QAGroupLineRepository = new Repository<QAGroupLine>(db);
            QAGroupLineRepository = new RepositoryQuery<QAGroupLine>(_QAGroupLineRepository);
        }
     

        public QAGroupLine Find(int id)
        {
            return _unitOfWork.Repository<QAGroupLine>().Find(id);
        }

        public QAGroupLine Create(QAGroupLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<QAGroupLine>().Insert(pt);
            return pt;
        }     

        public void Delete(int id)
        {
            _unitOfWork.Repository<QAGroupLine>().Delete(id);
        }

        public void Delete(QAGroupLine pt)
        {
            _unitOfWork.Repository<QAGroupLine>().Delete(pt);
        }

        public void Update(QAGroupLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<QAGroupLine>().Update(pt);
        }

        public IEnumerable<QAGroupLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<QAGroupLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.QAGroupLineId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public QAGroupLine Add(QAGroupLine pt)
        {
            _unitOfWork.Repository<QAGroupLine>().Insert(pt);
            return pt;
        }

        public IEnumerable<QAGroupLine> GetQAGroupLineList(int QAGroupId)
        {
            return (from p in db.QAGroupLine
                    where p.QAGroupId == QAGroupId
                    select p);
        }

        public IQueryable<QAGroupLineViewModel> GetQAGroupLineListForIndex(int QAGroupId)
        {
            var temp = from p in db.QAGroupLine                
                       where p.QAGroupId == QAGroupId
                       orderby p.QAGroupLineId
                       select new QAGroupLineViewModel
                       {
                           QAGroupId = p.QAGroupId,
                           QAGroupLineId=p.QAGroupLineId,
                           Name=p.Name,
                           IsMandatory=p.IsMandatory,
                           DataType=p.DataType,
                           ListItem=p.ListItem,
                           DefaultValue=p.DefaultValue,
                           IsActive=p.IsActive,                           
                           CreatedBy =p.CreatedBy,
                           CreatedDate = p.CreatedDate,
                           ModifiedBy = p.ModifiedBy,
                           ModifiedDate = p.ModifiedDate,
                           
                       };
            return temp;
        }

        
        public QAGroupLineViewModel GetQAGroupLine(int id)
        {
            var temp = (from p in db.QAGroupLine
                        where p.QAGroupLineId == id
                        select new QAGroupLineViewModel
                        {

                            QAGroupId = p.QAGroupId,
                            QAGroupLineId = p.QAGroupLineId,
                            Name = p.Name,
                            IsMandatory = p.IsMandatory,
                            DataType = p.DataType,
                            ListItem = p.ListItem,
                            DefaultValue = p.DefaultValue,
                            IsActive = p.IsActive,
                            CreatedBy = p.CreatedBy,
                            CreatedDate = p.CreatedDate,
                            ModifiedBy = p.ModifiedBy,
                            ModifiedDate = p.ModifiedDate,

                        }).FirstOrDefault();
            return temp;
        }
     
        public void Dispose()
        {
        }


        public Task<IEquatable<QAGroupLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<QAGroupLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

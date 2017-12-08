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

namespace Service
{
    public interface IPurchaseOrderHeaderStatusService : IDisposable
    {
        PurchaseOrderHeaderStatus Create(PurchaseOrderHeaderStatus pt);
        void Delete(int id);
        void Delete(PurchaseOrderHeaderStatus pt);
        PurchaseOrderHeaderStatus Find(int id);
        IEnumerable<PurchaseOrderHeaderStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseOrderHeaderStatus pt);
        PurchaseOrderHeaderStatus Add(PurchaseOrderHeaderStatus pt);
        IEnumerable<PurchaseOrderHeaderStatus> GetPurchaseOrderHeaderStatusList();
        Task<IEquatable<PurchaseOrderHeaderStatus>> GetAsync();
        Task<PurchaseOrderHeaderStatus> FindAsync(int id);
    }

    public class PurchaseOrderHeaderStatusService : IPurchaseOrderHeaderStatusService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseOrderHeaderStatus> _PurchaseOrderHeaderStatusRepository;
        RepositoryQuery<PurchaseOrderHeaderStatus> PurchaseOrderHeaderStatusRepository;
        public PurchaseOrderHeaderStatusService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PurchaseOrderHeaderStatusRepository = new Repository<PurchaseOrderHeaderStatus>(db);
            PurchaseOrderHeaderStatusRepository = new RepositoryQuery<PurchaseOrderHeaderStatus>(_PurchaseOrderHeaderStatusRepository);
        }


        public PurchaseOrderHeaderStatus Find(int id)
        {
            return _unitOfWork.Repository<PurchaseOrderHeaderStatus>().Find(id);
        }

        public PurchaseOrderHeaderStatus Create(PurchaseOrderHeaderStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseOrderHeaderStatus>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseOrderHeaderStatus>().Delete(id);
        }

        public void Delete(PurchaseOrderHeaderStatus pt)
        {
            _unitOfWork.Repository<PurchaseOrderHeaderStatus>().Delete(pt);
        }

        public void Update(PurchaseOrderHeaderStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseOrderHeaderStatus>().Update(pt);
        }

        public IEnumerable<PurchaseOrderHeaderStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseOrderHeaderStatus>()
                .Query()                             
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PurchaseOrderHeaderStatus> GetPurchaseOrderHeaderStatusList()
        {
            var pt = _unitOfWork.Repository<PurchaseOrderHeaderStatus>().Query().Get();

            return pt;
        }    

        public PurchaseOrderHeaderStatus Add(PurchaseOrderHeaderStatus pt)
        {
            _unitOfWork.Repository<PurchaseOrderHeaderStatus>().Insert(pt);
            return pt;
        }

        public void CreateHeaderStatus(int id,ref ApplicationDbContext Context)
        {
            PurchaseOrderHeaderStatus Stat = new PurchaseOrderHeaderStatus();
            Stat.PurchaseOrderHeaderId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            Context.PurchaseOrderHeaderStatus.Add(Stat);            
        }
        public void DeleteHeaderStatus(int id)
        {
            PurchaseOrderHeaderStatus Stat = Find(id);
            Delete(Stat);
        }
     
        public void Dispose()
        {
        }


        public Task<IEquatable<PurchaseOrderHeaderStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrderHeaderStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

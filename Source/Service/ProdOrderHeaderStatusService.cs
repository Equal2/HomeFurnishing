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
    public interface IProdOrderHeaderStatusService : IDisposable
    {
        ProdOrderHeaderStatus Create(ProdOrderHeaderStatus pt);
        void Delete(int id);
        void Delete(ProdOrderHeaderStatus pt);
        ProdOrderHeaderStatus Find(int id);
        IEnumerable<ProdOrderHeaderStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProdOrderHeaderStatus pt);
        ProdOrderHeaderStatus Add(ProdOrderHeaderStatus pt);
        IEnumerable<ProdOrderHeaderStatus> GetProdOrderHeaderStatusList();
        Task<IEquatable<ProdOrderHeaderStatus>> GetAsync();
        Task<ProdOrderHeaderStatus> FindAsync(int id);
        void CreateHeaderStatus(int id);
    }

    public class ProdOrderHeaderStatusService : IProdOrderHeaderStatusService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProdOrderHeaderStatus> _ProdOrderHeaderStatusRepository;
        RepositoryQuery<ProdOrderHeaderStatus> ProdOrderHeaderStatusRepository;
        public ProdOrderHeaderStatusService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProdOrderHeaderStatusRepository = new Repository<ProdOrderHeaderStatus>(db);
            ProdOrderHeaderStatusRepository = new RepositoryQuery<ProdOrderHeaderStatus>(_ProdOrderHeaderStatusRepository);
        }


        public ProdOrderHeaderStatus Find(int id)
        {
            return _unitOfWork.Repository<ProdOrderHeaderStatus>().Find(id);
        }

        public ProdOrderHeaderStatus Create(ProdOrderHeaderStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProdOrderHeaderStatus>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProdOrderHeaderStatus>().Delete(id);
        }

        public void Delete(ProdOrderHeaderStatus pt)
        {
            _unitOfWork.Repository<ProdOrderHeaderStatus>().Delete(pt);
        }

        public void Update(ProdOrderHeaderStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProdOrderHeaderStatus>().Update(pt);
        }

        public IEnumerable<ProdOrderHeaderStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProdOrderHeaderStatus>()
                .Query()                             
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProdOrderHeaderStatus> GetProdOrderHeaderStatusList()
        {
            var pt = _unitOfWork.Repository<ProdOrderHeaderStatus>().Query().Get();

            return pt;
        }    

        public ProdOrderHeaderStatus Add(ProdOrderHeaderStatus pt)
        {
            _unitOfWork.Repository<ProdOrderHeaderStatus>().Insert(pt);
            return pt;
        }

        public void CreateHeaderStatus(int id)
        {
            ProdOrderHeaderStatus Stat = new ProdOrderHeaderStatus();
            Stat.ProdOrderHeaderId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            Add(Stat);            
        }
        public void DeleteHeaderStatus(int id)
        {
            ProdOrderHeaderStatus Stat = Find(id);
            Delete(Stat);
        }
     
        public void Dispose()
        {
        }


        public Task<IEquatable<ProdOrderHeaderStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProdOrderHeaderStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

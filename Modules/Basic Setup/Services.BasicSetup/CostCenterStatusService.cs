using System.Collections.Generic;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;

namespace Services.BasicSetup
{
    public interface ICostCenterStatusService : IDisposable
    {
        CostCenterStatus Create(CostCenterStatus pt);
        void Delete(int id);
        void Delete(CostCenterStatus pt);
        CostCenterStatus Find(int id);
        IEnumerable<CostCenterStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(CostCenterStatus pt);
        CostCenterStatus Add(CostCenterStatus pt);
        IEnumerable<CostCenterStatus> GetCostCenterStatusList();
        Task<IEquatable<CostCenterStatus>> GetAsync();
        Task<CostCenterStatus> FindAsync(int id);
        void CreateLineStatus(int id);
    }

    public class CostCenterStatusService : ICostCenterStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CostCenterStatus> _CostCenterStatusRepository;
        public CostCenterStatusService(IUnitOfWork unitOfWork, IRepository<CostCenterStatus> costCenterStateusRepo)
        {
            _unitOfWork = unitOfWork;
            _CostCenterStatusRepository = costCenterStateusRepo;
        }

        public CostCenterStatusService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CostCenterStatusRepository = unitOfWork.Repository<CostCenterStatus>();
        }


        public CostCenterStatus Find(int id)
        {
            return _CostCenterStatusRepository.Find(id);
        }

        public CostCenterStatus Create(CostCenterStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _CostCenterStatusRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _CostCenterStatusRepository.Delete(id);
        }

        public void Delete(CostCenterStatus pt)
        {
            _CostCenterStatusRepository.Delete(pt);
        }

        public void Update(CostCenterStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _CostCenterStatusRepository.Update(pt);
        }

        public IEnumerable<CostCenterStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _CostCenterStatusRepository
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<CostCenterStatus> GetCostCenterStatusList()
        {
            var pt = _CostCenterStatusRepository.Query().Get();

            return pt;
        }

        public CostCenterStatus Add(CostCenterStatus pt)
        {
            _CostCenterStatusRepository.Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id)
        {
            CostCenterStatus Stat = new CostCenterStatus();
            Stat.CostCenterId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            CostCenterStatus Stat = Find(id);
            Delete(Stat);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<CostCenterStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CostCenterStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

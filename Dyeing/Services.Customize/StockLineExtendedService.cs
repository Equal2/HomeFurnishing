using System.Collections.Generic;
using System;
using Model;
using System.Threading.Tasks;
using Models.Customize.Models;
using Infrastructure.IO;

namespace Services.Customize
{
    public interface IStockLineExtendedService : IDisposable
    {
        StockLineExtended Create(StockLineExtended pt);
        void Delete(int id);
        void Delete(StockLineExtended pt);
        StockLineExtended Find(int id);
        IEnumerable<StockLineExtended> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(StockLineExtended pt);
        StockLineExtended Add(StockLineExtended pt);
        IEnumerable<StockLineExtended> GetStockLineExtendedList();
        Task<IEquatable<StockLineExtended>> GetAsync();
        Task<StockLineExtended> FindAsync(int id);
        void CreateLineStatus(int id);
    }

    public class StockLineExtendedService : IStockLineExtendedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<StockLineExtended> _StockLineExtendedRepository;
        public StockLineExtendedService(IUnitOfWork unitOfWork,IRepository<StockLineExtended> LineStatRepo)
        {
            _unitOfWork = unitOfWork;
            _StockLineExtendedRepository = LineStatRepo;
        }

        public StockLineExtendedService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _StockLineExtendedRepository = unitOfWork.Repository<StockLineExtended>();
        }


        public StockLineExtended Find(int id)
        {
            return _StockLineExtendedRepository.Find(id);
        }

        public StockLineExtended Create(StockLineExtended pt)
        {
            pt.ObjectState = ObjectState.Added;
            _StockLineExtendedRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _StockLineExtendedRepository.Delete(id);
        }

        public void Delete(StockLineExtended pt)
        {
            _StockLineExtendedRepository.Delete(pt);
        }

        public void Update(StockLineExtended pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _StockLineExtendedRepository.Update(pt);
        }

        public IEnumerable<StockLineExtended> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _StockLineExtendedRepository
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<StockLineExtended> GetStockLineExtendedList()
        {
            var pt = _StockLineExtendedRepository.Query().Get();

            return pt;
        }

        public StockLineExtended Add(StockLineExtended pt)
        {
            _StockLineExtendedRepository.Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id)
        {
            StockLineExtended Stat = new StockLineExtended();
            Stat.StockLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
                Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            StockLineExtended Stat = Find(id);
            Delete(Stat);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<StockLineExtended>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StockLineExtended> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

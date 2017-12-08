using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;
using Models.BasicSetup.ViewModels;

namespace Services.BasicSetup
{
    public interface IStockAdjService : IDisposable
    {
        StockAdj Create(StockAdj pt);
        void Delete(int id);
        void Delete(StockAdj pt);
        void Update(StockAdj pt);
        StockAdj Add(StockAdj pt);
    }

    public class StockAdjService : IStockAdjService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<StockAdj> _StockAdjRepository;

        public StockAdjService(IUnitOfWork unitOfWork, IRepository<StockAdj> stockBalRepo)
        {
            _unitOfWork = unitOfWork;
            _StockAdjRepository = stockBalRepo;
        }
        public StockAdjService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _StockAdjRepository = unitOfWork.Repository<StockAdj>();
        }
        public StockAdj Create(StockAdj pt)
        {
            pt.ObjectState = ObjectState.Added;
            _StockAdjRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _StockAdjRepository.Delete(id);
        }

        public void Delete(StockAdj pt)
        {
            _StockAdjRepository.Delete(pt);
        }


        public void Update(StockAdj pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _StockAdjRepository.Update(pt);
        }

        public StockAdj Add(StockAdj pt)
        {
            _StockAdjRepository.Insert(pt);
            return pt;
        }

        public Task<IEquatable<StockAdj>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StockAdj> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}

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
using AutoMapper;

namespace Service
{
    public interface IStockAdjService : IDisposable
    {
        StockAdj Create(StockAdj pt);
        void Delete(int id);
        void Update(StockAdj pt);
    }

    public class StockAdjService : IStockAdjService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<StockAdj> _StockAdjRepository;
        RepositoryQuery<StockAdj> StockAdjRepository;

        

        public StockAdjService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _StockAdjRepository = new Repository<StockAdj>(db);
            StockAdjRepository = new RepositoryQuery<StockAdj>(_StockAdjRepository);
        }

        public StockAdj Create(StockAdj pt)
        {
            pt.ObjectState = ObjectState.Added;            
            _unitOfWork.Repository<StockAdj>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<StockAdj>().Delete(id);
        }

        public void Delete(StockAdj pt)
        {
            _unitOfWork.Repository<StockAdj>().Delete(pt);
        }


        public void Update(StockAdj pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<StockAdj>().Update(pt);
        }

        public StockAdj Add(StockAdj pt)
        {
            _unitOfWork.Repository<StockAdj>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<StockAdj>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StockAdj> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Model.ViewModels;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;

namespace Service
{
    public interface ICounterService : IDisposable
    {
        Counter Create(Counter pt);
        void Delete(int id);
        void Delete(Counter pt);
        Counter Find(int ptId);
        void Update(Counter pt);
        Counter Add(Counter pt);
        IEnumerable<Counter> GetCounterList();
    }

    public class CounterService : ICounterService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Counter> _CounterRepository;
        RepositoryQuery<Counter> CounterRepository;
        public CounterService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CounterRepository = new Repository<Counter>(db);
            CounterRepository = new RepositoryQuery<Counter>(_CounterRepository);
        }

        public Counter Find(int pt)
        {
            return _unitOfWork.Repository<Counter>().Find(pt);
        }

        public Counter Create(Counter pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Counter>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Counter>().Delete(id);
        }

        public void Delete(Counter pt)
        {
            _unitOfWork.Repository<Counter>().Delete(pt);
        }

        public void Update(Counter pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Counter>().Update(pt);
        }

        public IEnumerable<Counter> GetCounterList()
        {
            var pt = _unitOfWork.Repository<Counter>().Query().Get();

            return pt;
        }


        public Counter Add(Counter pt)
        {
            _unitOfWork.Repository<Counter>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }
    }
}

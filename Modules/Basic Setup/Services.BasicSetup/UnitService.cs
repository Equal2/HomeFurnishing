using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;

namespace Services.BasicSetup
{
    public interface IUnitService : IDisposable
    {
        Unit Create(Unit pt);
        void Delete(string id);
        void Delete(Unit pt);
        Unit Find(string Id);
        Unit Find(int id);
        IEnumerable<Unit> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Unit pt);
        Unit Add(Unit pt);
        IEnumerable<Unit> GetUnitList();
        IEnumerable<Unit> GetUnitListWithFractions();
        Task<IEquatable<Unit>> GetAsync();
        Task<Unit> FindAsync(String id);        
        string NextId(string id);
        string PrevId(string id);
    }

    public class UnitService : IUnitService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Unit> _UnitRepository;

        public UnitService(IUnitOfWork unitOfWork, IRepository<Unit> unitRepo)
        {
            _unitOfWork = unitOfWork;
            _UnitRepository = unitRepo;
        }
        public UnitService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _UnitRepository = unitOfWork.Repository<Unit>();
        }

        public Unit Find(string Id)
        {
            return _UnitRepository.Find(Id);
        }


        public Unit Find(int id)
        {
            return _UnitRepository.Find(id);
        }

        public Unit Create(Unit pt)
        {
            pt.ObjectState = ObjectState.Added;
            _UnitRepository.Insert(pt);
            return pt;
        }
        public void Delete(string id)
        {
            _UnitRepository.Delete(id);
        }

        public void Delete(Unit pt)
        {
            _UnitRepository.Delete(pt);
        }

        public void Update(Unit pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _UnitRepository.Update(pt);
        }

        public IEnumerable<Unit> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _UnitRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.UnitName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Unit> GetUnitList()
        {
            var pt = _UnitRepository.Query().Get().OrderBy(m => m.UnitName);

            return pt;
        }
        public IEnumerable<Unit> GetUnitListWithFractions()
        {
            var pt = from p in _UnitRepository.Instance
                     where p.FractionUnits != null
                     select p;
            return pt;
        }

        public Unit Add(Unit pt)
        {
            _UnitRepository.Insert(pt);
            return pt;
        }

        public string NextId(string id)
        {
            string temp;
            if (id != null)
            {
                temp = (from p in _UnitRepository.Instance
                        orderby p.UnitName
                        select p.UnitId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _UnitRepository.Instance
                        orderby p.UnitName
                        select p.UnitId).FirstOrDefault();
            }
            if (temp != null)
                return temp;
            else
                return id;
        }

        public string PrevId(string id)
        {

            string temp;
            if (id != null)
            {

                temp = (from p in _UnitRepository.Instance
                        orderby p.UnitName
                        select p.UnitId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _UnitRepository.Instance
                        orderby p.UnitName
                        select p.UnitId).AsEnumerable().LastOrDefault();
            }
            if (temp != null)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

        public Task<IEquatable<Unit>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Unit> FindAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}

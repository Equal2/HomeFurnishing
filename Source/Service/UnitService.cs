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

        // IEnumerable<Unit> GetUnitList(int buyerId);
        Task<IEquatable<Unit>> GetAsync();
        Task<Unit> FindAsync(String id);        
        string NextId(string id);
        string PrevId(string id);
    }

    public class UnitService : IUnitService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Unit> _UnitRepository;
        RepositoryQuery<Unit> UnitRepository;
        public UnitService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _UnitRepository = new Repository<Unit>(db);
            UnitRepository = new RepositoryQuery<Unit>(_UnitRepository);
        }

        public Unit Find(string Id)
        {
            return _unitOfWork.Repository<Unit>().Find(Id);
        }


        public Unit Find(int id)
        {
            return _unitOfWork.Repository<Unit>().Find(id);
        }

        public Unit Create(Unit pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Unit>().Insert(pt);
            return pt;
        }
        public void Delete(string id)
        {
            _unitOfWork.Repository<Unit>().Delete(id);
        }

        public void Delete(Unit pt)
        {
            _unitOfWork.Repository<Unit>().Delete(pt);
        }

        public void Update(Unit pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Unit>().Update(pt);
        }

        public IEnumerable<Unit> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Unit>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.UnitName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Unit> GetUnitList()
        {
            var pt = _unitOfWork.Repository<Unit>().Query().Get().OrderBy(m=>m.UnitName);

            return pt;
        }
        public IEnumerable<Unit> GetUnitListWithFractions()
        {
            var pt = from p in db.Units
                     where p.FractionUnits != null
                     select p;
            return pt;
        }

        public Unit Add(Unit pt)
        {
            _unitOfWork.Repository<Unit>().Insert(pt);
            return pt;
        }

        public string NextId(string id)
        {
            string temp;
            if (id != null)
            {
                temp = (from p in db.Units
                        orderby p.UnitName
                        select p.UnitId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Units
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

                temp = (from p in db.Units
                        orderby p.UnitName
                        select p.UnitId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Units
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

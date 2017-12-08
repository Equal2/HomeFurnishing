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
    public interface IShipMethodService : IDisposable
    {
        ShipMethod Create(ShipMethod pt);
        void Delete(int id);
        void Delete(ShipMethod pt);
        ShipMethod Find(string Name);
        ShipMethod Find(int id);
        IEnumerable<ShipMethod> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        ShipMethod GetShipMethodByName(string Name);
        void Update(ShipMethod pt);
        ShipMethod Add(ShipMethod pt);
        IEnumerable<ShipMethod> GetShipMethodList();

        // IEnumerable<ShipMethod> GetShipMethodList(int buyerId);
        Task<IEquatable<ShipMethod>> GetAsync();
        Task<ShipMethod> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ShipMethodService : IShipMethodService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ShipMethod> _ShipMethodRepository;
        RepositoryQuery<ShipMethod> ShipMethodRepository;
        public ShipMethodService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ShipMethodRepository = new Repository<ShipMethod>(db);
            ShipMethodRepository = new RepositoryQuery<ShipMethod>(_ShipMethodRepository);
        }


        public ShipMethod GetShipMethodByName(string name)
        {
            return (from p in db.ShipMethod
                    where p.ShipMethodName == name
                    select p
                        ).FirstOrDefault();
        }

        public ShipMethod Find(string Name)
        {
            return ShipMethodRepository.Get().Where(i => i.ShipMethodName == Name).FirstOrDefault();
        }


        public ShipMethod Find(int id)
        {
            return _unitOfWork.Repository<ShipMethod>().Find(id);
        }

        public ShipMethod Create(ShipMethod pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ShipMethod>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ShipMethod>().Delete(id);
        }

        public void Delete(ShipMethod pt)
        {
            _unitOfWork.Repository<ShipMethod>().Delete(pt);
        }

        public void Update(ShipMethod pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ShipMethod>().Update(pt);
        }

        public IEnumerable<ShipMethod> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ShipMethod>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ShipMethodName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ShipMethod> GetShipMethodList()
        {
            var pt = _unitOfWork.Repository<ShipMethod>().Query().Get().OrderBy(m=>m.ShipMethodName);

            return pt;
        }

        public ShipMethod Add(ShipMethod pt)
        {
            _unitOfWork.Repository<ShipMethod>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ShipMethod
                        orderby p.ShipMethodName
                        select p.ShipMethodId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ShipMethod
                        orderby p.ShipMethodName
                        select p.ShipMethodId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.ShipMethod
                        orderby p.ShipMethodName
                        select p.ShipMethodId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ShipMethod
                        orderby p.ShipMethodName
                        select p.ShipMethodId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ShipMethod>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ShipMethod> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

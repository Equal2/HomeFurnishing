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
    public interface IChargeTypeService : IDisposable
    {
        ChargeType Create(ChargeType pt);
        void Delete(int id);
        void Delete(ChargeType pt);
        ChargeType Find(string Name);
        ChargeType Find(int id);
        IEnumerable<ChargeType> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ChargeType pt);
        ChargeType Add(ChargeType pt);
        IEnumerable<ChargeType> GetChargeTypeList();

        // IEnumerable<ChargeType> GetChargeTypeList(int buyerId);
        Task<IEquatable<ChargeType>> GetAsync();
        Task<ChargeType> FindAsync(int id);
        IEnumerable<ChargeType> GetPersonChargeTypeList();
        IEnumerable<ChargeType> GetProductChargeTypeList();
        int NextId(int id);
        int PrevId(int id);
    }

    public class ChargeTypeService : IChargeTypeService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ChargeType> _ChargeTypeRepository;
        RepositoryQuery<ChargeType> ChargeTypeRepository;
        public ChargeTypeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ChargeTypeRepository = new Repository<ChargeType>(db);
            ChargeTypeRepository = new RepositoryQuery<ChargeType>(_ChargeTypeRepository);
        }

        public ChargeType Find(string Name)
        {
            return (from p in db.ChargeType
                    where p.ChargeTypeName == Name
                    select p
                        ).FirstOrDefault();
        }
        public IEnumerable<ChargeType> GetPersonChargeTypeList()
        {
            return (from p in db.ChargeType
                    where p.isPersonBased == true
                    select p
                        );
        }
        public IEnumerable<ChargeType> GetProductChargeTypeList()
        {
            return (from p in db.ChargeType
                    where p.isProductBased == true
                    select p
                       );
        }


        public ChargeType Find(int id)
        {
            return _unitOfWork.Repository<ChargeType>().Find(id);
        }

        public ChargeType Create(ChargeType pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ChargeType>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ChargeType>().Delete(id);
        }

        public void Delete(ChargeType pt)
        {
            _unitOfWork.Repository<ChargeType>().Delete(pt);
        }

        public void Update(ChargeType pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ChargeType>().Update(pt);
        }

        public IEnumerable<ChargeType> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ChargeType>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ChargeTypeName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ChargeType> GetChargeTypeList()
        {
            var pt = (from p in db.ChargeType
                      orderby p.ChargeTypeName
                      select p);

            return pt;
        }

        public ChargeType Add(ChargeType pt)
        {
            _unitOfWork.Repository<ChargeType>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ChargeType
                        orderby p.ChargeTypeName
                        select p.ChargeTypeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ChargeType
                        orderby p.ChargeTypeName
                        select p.ChargeTypeId).FirstOrDefault();
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

                temp = (from p in db.ChargeType
                        orderby p.ChargeTypeName
                        select p.ChargeTypeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ChargeType
                        orderby p.ChargeTypeName
                        select p.ChargeTypeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ChargeType>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ChargeType> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

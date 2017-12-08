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
    public interface IChargeGroupProductService : IDisposable
    {
        ChargeGroupProduct Create(ChargeGroupProduct pt);
        void Delete(int id);
        void Delete(ChargeGroupProduct pt);
        ChargeGroupProduct Find(string Name);
        ChargeGroupProduct Find(int id);
        IEnumerable<ChargeGroupProduct> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ChargeGroupProduct pt);
        ChargeGroupProduct Add(ChargeGroupProduct pt);
        IQueryable<ChargeGroupProduct> GetChargeGroupProductList();
        Task<IEquatable<ChargeGroupProduct>> GetAsync();
        Task<ChargeGroupProduct> FindAsync(int id);
        int NextId(int id,int ctypeid);
        int PrevId(int id,int ctypeid);
    }

    public class ChargeGroupProductService : IChargeGroupProductService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ChargeGroupProduct> _ChargeGroupProductRepository;
        RepositoryQuery<ChargeGroupProduct> ChargeGroupProductRepository;
        public ChargeGroupProductService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ChargeGroupProductRepository = new Repository<ChargeGroupProduct>(db);
            ChargeGroupProductRepository = new RepositoryQuery<ChargeGroupProduct>(_ChargeGroupProductRepository);
        }

        public ChargeGroupProduct Find(string Name)
        {
            return (from p in db.ChargeGroupProduct
                    where p.ChargeGroupProductName == Name
                    select p
                        ).FirstOrDefault();
        }


        public ChargeGroupProduct Find(int id)
        {
            return _unitOfWork.Repository<ChargeGroupProduct>().Find(id);
        }

        public ChargeGroupProduct Create(ChargeGroupProduct pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ChargeGroupProduct>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ChargeGroupProduct>().Delete(id);
        }

        public void Delete(ChargeGroupProduct pt)
        {
            _unitOfWork.Repository<ChargeGroupProduct>().Delete(pt);
        }

        public void Update(ChargeGroupProduct pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ChargeGroupProduct>().Update(pt);
        }

        public IEnumerable<ChargeGroupProduct> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ChargeGroupProduct>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ChargeGroupProductName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<ChargeGroupProduct> GetChargeGroupProductList()
        {
            var pt = (from p in db.ChargeGroupProduct
                      orderby p.ChargeGroupProductName
                      select p
                          );

            return pt;
        }


        public ChargeGroupProduct Add(ChargeGroupProduct pt)
        {
            _unitOfWork.Repository<ChargeGroupProduct>().Insert(pt);
            return pt;
        }

        public int NextId(int id,int ctypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ChargeGroupProduct
                        orderby p.ChargeGroupProductName
                        select p.ChargeGroupProductId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ChargeGroupProduct
                        orderby p.ChargeGroupProductName
                        select p.ChargeGroupProductId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id, int ctypeid)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.ChargeGroupProduct
                        orderby p.ChargeGroupProductName
                        select p.ChargeGroupProductId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ChargeGroupProduct
                        orderby p.ChargeGroupProductName
                        select p.ChargeGroupProductId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ChargeGroupProduct>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ChargeGroupProduct> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

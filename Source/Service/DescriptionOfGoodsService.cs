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
    public interface IDescriptionOfGoodsService : IDisposable
    {
        DescriptionOfGoods Create(DescriptionOfGoods pt);
        void Delete(int id);
        void Delete(DescriptionOfGoods pt);
        DescriptionOfGoods Find(string Name);
        DescriptionOfGoods Find(int id);
        IEnumerable<DescriptionOfGoods> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(DescriptionOfGoods pt);
        DescriptionOfGoods Add(DescriptionOfGoods pt);
        IEnumerable<DescriptionOfGoods> GetDescriptionOfGoodsList();

        // IEnumerable<DescriptionOfGoods> GetDescriptionOfGoodsList(int buyerId);
        Task<IEquatable<DescriptionOfGoods>> GetAsync();
        Task<DescriptionOfGoods> FindAsync(int id);
        DescriptionOfGoods GetDescriptionOfGoodsByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class DescriptionOfGoodsService : IDescriptionOfGoodsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<DescriptionOfGoods> _DescriptionOfGoodsRepository;
        RepositoryQuery<DescriptionOfGoods> DescriptionOfGoodsRepository;
        public DescriptionOfGoodsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _DescriptionOfGoodsRepository = new Repository<DescriptionOfGoods>(db);
            DescriptionOfGoodsRepository = new RepositoryQuery<DescriptionOfGoods>(_DescriptionOfGoodsRepository);
        }
        public DescriptionOfGoods GetDescriptionOfGoodsByName(string terms)
        {
            return (from p in db.DescriptionOfGoods
                    where p.DescriptionOfGoodsName == terms
                    select p).FirstOrDefault();
        }

        public DescriptionOfGoods Find(string Name)
        {
            return DescriptionOfGoodsRepository.Get().Where(i => i.DescriptionOfGoodsName == Name).FirstOrDefault();
        }


        public DescriptionOfGoods Find(int id)
        {
            return _unitOfWork.Repository<DescriptionOfGoods>().Find(id);
        }

        public DescriptionOfGoods Create(DescriptionOfGoods pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<DescriptionOfGoods>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<DescriptionOfGoods>().Delete(id);
        }

        public void Delete(DescriptionOfGoods pt)
        {
            _unitOfWork.Repository<DescriptionOfGoods>().Delete(pt);
        }

        public void Update(DescriptionOfGoods pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<DescriptionOfGoods>().Update(pt);
        }

        public IEnumerable<DescriptionOfGoods> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<DescriptionOfGoods>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DescriptionOfGoodsName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<DescriptionOfGoods> GetDescriptionOfGoodsList()
        {
            var pt = _unitOfWork.Repository<DescriptionOfGoods>().Query().Get().OrderBy(m=>m.DescriptionOfGoodsName);

            return pt;
        }

        public DescriptionOfGoods Add(DescriptionOfGoods pt)
        {
            _unitOfWork.Repository<DescriptionOfGoods>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.DescriptionOfGoods
                        orderby p.DescriptionOfGoodsName
                        select p.DescriptionOfGoodsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.DescriptionOfGoods
                        orderby p.DescriptionOfGoodsName
                        select p.DescriptionOfGoodsId).FirstOrDefault();
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

                temp = (from p in db.DescriptionOfGoods
                        orderby p.DescriptionOfGoodsName
                        select p.DescriptionOfGoodsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.DescriptionOfGoods
                        orderby p.DescriptionOfGoodsName
                        select p.DescriptionOfGoodsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<DescriptionOfGoods>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DescriptionOfGoods> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

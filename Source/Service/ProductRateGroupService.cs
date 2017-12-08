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
    public interface IProductRateGroupService : IDisposable
    {
        ProductRateGroup Create(ProductRateGroup pt);
        void Delete(int id);
        void Delete(ProductRateGroup pt);
        ProductRateGroup Find(string Name);
        ProductRateGroup Find(int id);      
        void Update(ProductRateGroup pt);
        ProductRateGroup Add(ProductRateGroup pt);
        IQueryable<ProductRateGroup> GetProductRateGroupList();
        ProductRateGroup GetProductRateGroupByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ProductRateGroupService : IProductRateGroupService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductRateGroup> _ProductRateGroupRepository;
        RepositoryQuery<ProductRateGroup> ProductRateGroupRepository;

        public ProductRateGroupService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductRateGroupRepository = new Repository<ProductRateGroup>(db);
            ProductRateGroupRepository = new RepositoryQuery<ProductRateGroup>(_ProductRateGroupRepository);
        }

        public ProductRateGroup GetProductRateGroupByName(string terms)
        {
            return (from p in db.ProductRateGroup
                    where p.ProductRateGroupName == terms
                    select p).FirstOrDefault();
        }

        public ProductRateGroup Find(string Name)
        {
            return ProductRateGroupRepository.Get().Where(i => i.ProductRateGroupName == Name).FirstOrDefault();
        }


        public ProductRateGroup Find(int id)
        {
            return _unitOfWork.Repository<ProductRateGroup>().Find(id);
        }

        public ProductRateGroup Create(ProductRateGroup pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductRateGroup>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductRateGroup>().Delete(id);
        }

        public void Delete(ProductRateGroup pt)
        {
            _unitOfWork.Repository<ProductRateGroup>().Delete(pt);
        }

        public void Update(ProductRateGroup pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductRateGroup>().Update(pt);
        }

        public IQueryable<ProductRateGroup> GetProductRateGroupList()
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var pt = _unitOfWork.Repository<ProductRateGroup>().Query().Get().Where(m=>m.SiteId==SiteId && m.DivisionId==DivisionId).OrderBy(m=>m.ProductRateGroupName);

            return pt;
        }

        public ProductRateGroup Add(ProductRateGroup pt)
        {
            _unitOfWork.Repository<ProductRateGroup>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductRateGroup
                        orderby p.ProductRateGroupName
                        select p.ProductRateGroupId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductRateGroup
                        orderby p.ProductRateGroupName
                        select p.ProductRateGroupId).FirstOrDefault();
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

                temp = (from p in db.ProductRateGroup
                        orderby p.ProductRateGroupName
                        select p.ProductRateGroupId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductRateGroup
                        orderby p.ProductRateGroupName
                        select p.ProductRateGroupId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }

    }
}

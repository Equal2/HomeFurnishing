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
    public interface IProductSiteDetailService : IDisposable
    {
        ProductSiteDetail Create(ProductSiteDetail pt);
        void Delete(int id);
        void Delete(ProductSiteDetail pt);
        ProductSiteDetail Find(int id);
        void Update(ProductSiteDetail pt);
        ProductSiteDetail Add(ProductSiteDetail pt);
        IEnumerable<ProductSiteDetail> GetProductSiteDetailList();

        // IEnumerable<ProductSiteDetail> GetProductSiteDetailList(int buyerId);
        Task<IEquatable<ProductSiteDetail>> GetAsync();
        Task<ProductSiteDetail> FindAsync(int id);
        ProductSiteDetail FindforSite(int SiteId, int DivId, int ProdId);
        IEnumerable<ProductSiteDetail> GetSiteDetailForProduct(int id);//ProductId
    }

    public class ProductSiteDetailService : IProductSiteDetailService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductSiteDetail> _ProductSiteDetailRepository;
        RepositoryQuery<ProductSiteDetail> ProductSiteDetailRepository;
        public ProductSiteDetailService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductSiteDetailRepository = new Repository<ProductSiteDetail>(db);
            ProductSiteDetailRepository = new RepositoryQuery<ProductSiteDetail>(_ProductSiteDetailRepository);
        }

        public ProductSiteDetail FindforSite(int SiteId, int DivId, int ProdId)
        {
            return (from p in db.ProductSiteDetail
                    where p.ProductId == ProdId && p.SiteId == SiteId && p.DivisionId == DivId
                    select p
                        ).FirstOrDefault();
        }

        public IEnumerable<ProductSiteDetail> GetSiteDetailForProduct(int id)
        {
            return (from p in db.ProductSiteDetail
                    where p.ProductId == id
                    select p
                        );
        }


        public ProductSiteDetail Find(int id)
        {
            return _unitOfWork.Repository<ProductSiteDetail>().Find(id);
        }

        public ProductSiteDetail Create(ProductSiteDetail pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductSiteDetail>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductSiteDetail>().Delete(id);
        }

        public void Delete(ProductSiteDetail pt)
        {
            _unitOfWork.Repository<ProductSiteDetail>().Delete(pt);
        }

        public void Update(ProductSiteDetail pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductSiteDetail>().Update(pt);
        }


        public IEnumerable<ProductSiteDetail> GetProductSiteDetailList()
        {
            var pt = _unitOfWork.Repository<ProductSiteDetail>().Query().Get();

            return pt;
        }

        public ProductSiteDetail Add(ProductSiteDetail pt)
        {
            _unitOfWork.Repository<ProductSiteDetail>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProductSiteDetail>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductSiteDetail> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

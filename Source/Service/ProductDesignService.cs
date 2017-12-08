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
    public interface IProductDesignService : IDisposable
    {
        ProductDesign Create(ProductDesign pt);
        void Delete(int id);
        void Delete(ProductDesign pt);
        ProductDesign Find(string Name);
        ProductDesign Find(int id);
        IEnumerable<ProductDesign> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductDesign pt);
        ProductDesign Add(ProductDesign pt);
        
        IEnumerable<ProductDesign> GetProductDesignList(int id);
        IQueryable<ProductDesign> GetProductDesignListForColourWays();

        // IEnumerable<ProductDesign> GetProductDesignList(int buyerId);
        Task<IEquatable<ProductDesign>> GetAsync();
        Task<ProductDesign> FindAsync(int id);
        int NextId(int id,int ptypeid);
        int PrevId(int id,int ptypeid);
        IEnumerable<ProductType> GetProductTypeList();
    }

    public class ProductDesignService : IProductDesignService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductDesign> _ProductDesignRepository;
        RepositoryQuery<ProductDesign> ProductDesignRepository;
        public ProductDesignService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductDesignRepository = new Repository<ProductDesign>(db);
            ProductDesignRepository = new RepositoryQuery<ProductDesign>(_ProductDesignRepository);
        }


        public ProductDesign Find(string Name)
        {            
            return ProductDesignRepository.Get().Where(i => i.ProductDesignName == Name).FirstOrDefault();
        }

        public IEnumerable<ProductType> GetProductTypeList()
        {
            int id = new ProductTypeService(_unitOfWork).GetProductTypeByName(ProductTypeConstants.Rug).ProductTypeId;
            int finid=new ProductNatureService(_unitOfWork).GetProductNatureByName(ProductTypeConstants.FinishedMaterial).ProductNatureId;
            return (from p in db.ProductTypes
                        where p.ProductTypeId!=id && p.ProductNatureId==finid
                        select p
                        );
        }
       
        public ProductDesign Find(int id)
        {
            return _unitOfWork.Repository<ProductDesign>().Find(id);            
        }

        public ProductDesign Create(ProductDesign pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductDesign>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductDesign>().Delete(id);
        }

        public void Delete(ProductDesign pt)
        {
            _unitOfWork.Repository<ProductDesign>().Delete(pt);
        }

        public void Update(ProductDesign pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductDesign>().Update(pt);
        }

        public IEnumerable<ProductDesign> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductDesign>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductDesignName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductDesign> GetProductDesignList(int id)
        {             
            var pt = _unitOfWork.Repository<ProductDesign>().Query().Get().Where(m=>m.ProductTypeId==id).OrderBy(m=>m.ProductDesignName);

            return pt;
        }

        public IQueryable<ProductDesign> GetProductDesignListForColourWays()
        {

            int pid = new ProductTypeService(_unitOfWork).GetProductTypeByName(ProductTypeConstants.Rug).ProductTypeId;
            var pt = _unitOfWork.Repository<ProductDesign>().Query().Get().Where(m=>m.ProductTypeId==pid).OrderBy(m => m.ProductDesignName);

            return pt;
        }

        public ProductDesign Add(ProductDesign pt)
        {
            _unitOfWork.Repository<ProductDesign>().Insert(pt);
            return pt;
        }

        public int NextId(int id,int ptypeid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductDesigns   
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductDesignName
                        select p.ProductDesignId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductDesigns  
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductDesignName
                        select p.ProductDesignId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id,int ptypeid)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.ProductDesigns
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductDesignName
                        select p.ProductDesignId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductDesigns
                        where p.ProductTypeId==ptypeid
                        orderby p.ProductDesignName
                        select p.ProductDesignId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<ProductDesign>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductDesign> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

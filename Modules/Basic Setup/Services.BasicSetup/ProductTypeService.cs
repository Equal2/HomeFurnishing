using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;
using ProjLib.Constants;

namespace Services.BasicSetup
{
    public interface IProductTypeService : IDisposable
    {
        ProductType Create(ProductType pt);
        void Delete(int id);
        void Delete(ProductType pt);
        ProductType GetProductTypeByName(string name);
        ProductType GetProductTypeWithAttributes(int id);        
        ProductType Find(string Name);
        ProductType Find(int id);
        void Update(ProductType pt);
        ProductType Add(ProductType pt);
        IEnumerable<ProductType> GetProductTypeList();
        ProductType GetTypeForProduct(int id);//ProductId
        Task<IEquatable<ProductType>> GetAsync();
        Task<ProductType> FindAsync(int id);

        int NextId(int id);
        int PrevId(int id);
    }

    public class ProductTypeService : IProductTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ProductType> _ProductTypeRepository;
        public ProductTypeService(IUnitOfWork unitOfWork,IRepository<ProductType> productTypeRepo)
        {
            _unitOfWork = unitOfWork;
            _ProductTypeRepository = productTypeRepo;
        }
        public ProductTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductTypeRepository = unitOfWork.Repository<ProductType>();
        }
        public ProductType GetProductTypeByName(string name)
        {
            return (from p in _ProductTypeRepository.Instance
                    where p.ProductTypeName == name 
                    select p
                        ).FirstOrDefault();
        }

        public ProductType Find(string Name)
        {
            return _ProductTypeRepository.Query().Get().Where(i => i.ProductTypeName == Name).FirstOrDefault();
        }

        public ProductType GetProductTypeWithAttributes(int id)
        {
            return _ProductTypeRepository.Query().Get().Where(i => i.ProductTypeId == id).FirstOrDefault();
        }


        public ProductType Find(int id)
        {
            return _ProductTypeRepository.Find(id);
        }

        public ProductType Create(ProductType pt)
        {
            pt.ObjectState = ObjectState.Added;
            _ProductTypeRepository.Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _ProductTypeRepository.Delete(id);
        }

        public void Delete(ProductType pt)
        {
            pt.ObjectState = ObjectState.Deleted;
            _ProductTypeRepository.Delete(pt);
        }
        public ProductType GetTypeForProduct(int id)//ProductId
        {
            return (from p in _unitOfWork.Repository<Product>().Instance
                    join t in _unitOfWork.Repository<ProductGroup>().Instance on p.ProductGroupId equals t.ProductGroupId into table
                    from tab in table.DefaultIfEmpty()
                    join t1 in _ProductTypeRepository.Instance on tab.ProductTypeId equals t1.ProductTypeId into table2
                    from tab2 in table2.DefaultIfEmpty()
                    where p.ProductId == id
                    select tab2
                ).FirstOrDefault();
        }

        public void Update(ProductType pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _ProductTypeRepository.Update(pt);
        }


        public IEnumerable<ProductType> GetProductTypeList()
        {
            var pt = _ProductTypeRepository.Query().Get();

            return pt;
        }
        public IEnumerable<ProductType> GetProductTypeListForNature(int id)
        {
            return (from p in _ProductTypeRepository.Instance
                    where p.ProductNatureId == id
                    select p
                        ).ToList();
        }

        public IEnumerable<ProductType> GetProductTypeListForGroup()
        {
            var pt = (from p in _ProductTypeRepository.Instance
                          where p.IsCustomUI==null
                          select p
                          );

            return pt;
        }
        public IEnumerable<ProductType> GetProductTypeListForCategory()
        {
            var pt = (from p in _ProductTypeRepository.Instance
                      where p.IsCustomUI == null && p.ProductNature.ProductNatureName==ProductNatureConstants.FinishedMaterial
                      select p
                          );

            return pt;
        }

        public IEnumerable<ProductType> GetProductTypeListForMaterial(int id)
        {
            var pt = (from p in _ProductTypeRepository.Instance
                      where p.IsCustomUI == null && p.ProductNatureId==id
                      select p
                          );

            return pt;
        }

        public IEnumerable<ProductType> GetRawAndOtherMaterialProductTypes()
        {

            string RawMaterial = ProductNatureConstants.Rawmaterial;
            string OtherMaterial = ProductNatureConstants.OtherMaterial;

            var pt = (from p in _ProductTypeRepository.Instance
                      where p.ProductNature.ProductNatureName == RawMaterial || p.ProductNature.ProductNatureName == OtherMaterial
                      select p
                        );

            return pt;
        }


        public ProductType Add(ProductType pt)
        {
            _ProductTypeRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _ProductTypeRepository.Instance
                        orderby p.ProductTypeName
                        select p.ProductTypeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _ProductTypeRepository.Instance
                        orderby p.ProductTypeName
                        select p.ProductTypeId).FirstOrDefault();
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

                temp = (from p in _ProductTypeRepository.Instance
                        orderby p.ProductTypeName
                        select p.ProductTypeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _ProductTypeRepository.Instance
                        orderby p.ProductTypeName
                        select p.ProductTypeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<ProductType>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductType> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

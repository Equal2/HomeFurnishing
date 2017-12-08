using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using Models.BasicSetup.Models;
using Infrastructure.IO;
using Models.Company.Models;
using ProjLib.DocumentConstants;
using Models.BasicSetup.ViewModels;

namespace Services.BasicSetup
{
    public interface IProductUidHeaderService : IDisposable
    {
        ProductUidHeader Create(ProductUidHeader p);
        void Delete(int id);
        void Delete(ProductUidHeader p);
        IEnumerable<ProductUidHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductUidHeader p);
        ProductUidHeader Add(ProductUidHeader p);
        IQueryable<ProductUidHeader> GetProductUidHeaderList();
        IEnumerable<ProductUidHeaderIndexViewModel> GetProductUidHeaderIndexList();
        IEnumerable<ProductUidHeader> GetProductUidHeaderList(int prodyctTypeId);
        Task<IEquatable<ProductUidHeader>> GetAsync();
        Task<ProductUidHeader> FindAsync(int id);
        ProductUidHeader Find(int id);
        ProductUidHeaderIndexViewModel GetProductUidHeaderIndexViewModel(int ProductUidHeaderId);
        string FGetNewDocNo(string FieldName, string TableName, int DocTypeId, DateTime DocDate);
        int NextId(int id);
        int PrevId(int id);

    }



    public class ProductUidHeaderService : IProductUidHeaderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ProductUidHeader> _productUidHeaderRepository;

        public ProductUidHeaderService(IUnitOfWork unitOfWork, IRepository<ProductUidHeader> ProductuidRepo)
        {
            _unitOfWork = unitOfWork;
            _productUidHeaderRepository = ProductuidRepo;
        }
        public ProductUidHeaderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _productUidHeaderRepository = unitOfWork.Repository<ProductUidHeader>();
        }


        public ProductUidHeader Create(ProductUidHeader p)
        {
            p.ObjectState = ObjectState.Added;
            _productUidHeaderRepository.Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _productUidHeaderRepository.Delete(id);
        }

        public void Delete(ProductUidHeader p)
        {
            _productUidHeaderRepository.Delete(p);
        }
        
        public void Update(ProductUidHeader p)
        {
            p.ObjectState = ObjectState.Modified;
            _productUidHeaderRepository.Update(p);
        }

        public IEnumerable<ProductUidHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _productUidHeaderRepository
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public IQueryable<ProductUidHeader> GetProductUidHeaderList()
        {
            var p = from H in _productUidHeaderRepository.Instance
                    join D in _unitOfWork.Repository<DocumentType>().Instance on H.GenDocTypeId equals D.DocumentTypeId into DocumentTypeTable
                    from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                    where DocumentTypeTab.DocumentTypeName == MasterDocTypeConstants.ProductUid
                    orderby H.GenDocDate descending, H.ProductUidHeaderId descending
                    select H;

            return p;

        }


        public IEnumerable<ProductUidHeaderIndexViewModel> GetProductUidHeaderIndexList()
        {
            IEnumerable<ProductUidHeaderIndexViewModel> p = _unitOfWork.SqlQuery<ProductUidHeaderIndexViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_ProductUidHeaderIndex");

            return p;
        }

       

        public IEnumerable<ProductUidHeader> GetAccessoryList()
        {
            var p = _productUidHeaderRepository.Query().Get();
            return p;
        }

        public ProductUidHeader Find(int id)
        {

            return _productUidHeaderRepository.Find(id);
        }


        public IEnumerable<ProductUidHeader> GetProductUidHeaderList(int productTypeId)
        {
            return _productUidHeaderRepository.Query().Get().OrderBy(m => m.ProductUidHeaderId);
        }

        public ProductUidHeader Add(ProductUidHeader p)
        {
            _productUidHeaderRepository.Add(p);
            return p;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _productUidHeaderRepository.Instance
                        orderby p.GenDocDate descending
                        select p.ProductUidHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _productUidHeaderRepository.Instance
                        orderby p.GenDocDate descending
                        select p.ProductUidHeaderId).FirstOrDefault();
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

                temp = (from p in _productUidHeaderRepository.Instance
                        orderby p.GenDocDate descending
                        select p.ProductUidHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _productUidHeaderRepository.Instance
                        orderby p.GenDocDate descending
                        select p.ProductUidHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ProductUidHeaderIndexViewModel GetProductUidHeaderIndexViewModel(int ProductUidHeaderId)
        {
            var Temp = (from H in _productUidHeaderRepository.Instance
                        where H.ProductUidHeaderId == ProductUidHeaderId
                        select new ProductUidHeaderIndexViewModel
                        {
                            ProductUidHeaderId = H.ProductUidHeaderId,
                            GenDocDate = (DateTime) H.GenDocDate,
                            GenDocNo = H.GenDocNo,
                            GenPersonId = H.GenPersonId,
                            GenPersonName = H.GenPerson.Name,
                            ProductId = H.ProductId,
                            ProductName = H.Product.ProductName,
                            GenRemark = H.GenRemark
                        }).FirstOrDefault();

            string ProductUidStr = "";

            var ProductUids = (from L in _unitOfWork.Repository<ProductUid>().Instance
                               where L.ProductUidHeaderId == ProductUidHeaderId
                               select L).ToList();

            foreach (var item in ProductUids)
            {
                if (ProductUidStr == "")
                {
                    ProductUidStr = item.ProductUidName;
                }
                else{
                    ProductUidStr = ProductUidStr + "," +  item.ProductUidName;
                }
            }

            Temp.ProductUids = ProductUidStr;

            return Temp;
        }

        public string FGetNewDocNo(string FieldName, string TableName, int DocTypeId, DateTime DocDate)
        {
            SqlParameter SqlParameterFieldName = new SqlParameter("@FieldName", FieldName);
            SqlParameter SqlParameterTableName = new SqlParameter("@TableName", TableName);
            SqlParameter SqlParameterDocTypeId = new SqlParameter("@DocTypeId", DocTypeId);
            SqlParameter SqlParameterDocDate = new SqlParameter("@DocDate", DocDate);

            NewDocNoViewModel NewDocNoViewModel = _unitOfWork.SqlQuery<NewDocNoViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".GetProductUidHeaderNewDocNo @FieldName , @TableName ,@DocTypeId , @DocDate ", SqlParameterFieldName, SqlParameterTableName, SqlParameterDocTypeId, SqlParameterDocDate).FirstOrDefault();

            if (NewDocNoViewModel != null)
            {
                return NewDocNoViewModel.NewDocNo;
            }
            else
            {
                return null;
            }
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<ProductUidHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductUidHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

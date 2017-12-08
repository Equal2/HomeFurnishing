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

using Model.ViewModels;
using Model.ViewModel;
using System.Data.SqlClient;
using System.Configuration;
using Model.ViewModels;

namespace Service
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
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductUidHeader> _productRepository;
        //private readonly Repository<ProductUidHeaderDimension> _productdimensionRepository;

        RepositoryQuery<ProductUidHeader> productRepository;
        //RepositoryQuery<ProductUidHeaderDimension> productdimensionRepository;

        public ProductUidHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _productRepository = new Repository<ProductUidHeader>(db);
            //_productdimensionRepository = new Repository<ProductUidHeaderDimension>(db);

            productRepository = new RepositoryQuery<ProductUidHeader>(_productRepository);
            // productdimensionRepository = new RepositoryQuery<ProductUidHeaderDimension>(_productdimensionRepository);
        }



        public ProductUidHeader Create(ProductUidHeader p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductUidHeader>().Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductUidHeader>().Delete(id);
        }

        public void Delete(ProductUidHeader p)
        {
            _unitOfWork.Repository<ProductUidHeader>().Delete(p);
        }
        
        public void Update(ProductUidHeader p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductUidHeader>().Update(p);
        }

        public IEnumerable<ProductUidHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductUidHeader>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public IQueryable<ProductUidHeader> GetProductUidHeaderList()
        {
            //var p = _unitOfWork.Repository<ProductUidHeader>().Query().Get().OrderBy(m => m.ProductUidHeaderId) ;
            var p = from H in db.ProductUidHeader
                    join D in db.DocumentType on H.GenDocTypeId  equals D.DocumentTypeId into DocumentTypeTable from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                    where DocumentTypeTab.DocumentTypeName == MasterDocTypeConstants.ProductUid
                    orderby H.GenDocDate descending, H.ProductUidHeaderId descending
                    select H;

            return p;

        }


        public IEnumerable<ProductUidHeaderIndexViewModel> GetProductUidHeaderIndexList()
        {
            IEnumerable<ProductUidHeaderIndexViewModel> p = db.Database.SqlQuery<ProductUidHeaderIndexViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_ProductUidHeaderIndex");

            return p;
        }

       

        public IEnumerable<ProductUidHeader> GetAccessoryList()
        {
            var p = _unitOfWork.Repository<ProductUidHeader>().Query().Get();
            return p;
        }

        public ProductUidHeader Find(int id)
        {

            return _unitOfWork.Repository<ProductUidHeader>().Find(id);
        }


        public IEnumerable<ProductUidHeader> GetProductUidHeaderList(int productTypeId)
        {
            return _unitOfWork.Repository<ProductUidHeader>().Query().Get().OrderBy(m => m.ProductUidHeaderId);
        }

        public ProductUidHeader Add(ProductUidHeader p)
        {
            _unitOfWork.Repository<ProductUidHeader>().Add(p);
            return p;
        }

        


        public void Dispose()
        {
        }


        public Task<IEquatable<ProductUidHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductUidHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductUidHeader
                        orderby p.GenDocDate descending
                        select p.ProductUidHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductUidHeader
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

                temp = (from p in db.ProductUidHeader
                        orderby p.GenDocDate descending
                        select p.ProductUidHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductUidHeader
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
            var Temp = (from H in db.ProductUidHeader
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

            var ProductUids = (from L in db.ProductUid
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

            NewDocNoViewModel NewDocNoViewModel = db.Database.SqlQuery<NewDocNoViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".GetProductUidHeaderNewDocNo @FieldName , @TableName ,@DocTypeId , @DocDate ", SqlParameterFieldName, SqlParameterTableName, SqlParameterDocTypeId, SqlParameterDocDate).FirstOrDefault();

            if (NewDocNoViewModel != null)
            {
                return NewDocNoViewModel.NewDocNo;
            }
            else
            {
                return null;
            }
        }

    }
}

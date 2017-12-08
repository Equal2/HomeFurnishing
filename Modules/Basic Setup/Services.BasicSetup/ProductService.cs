using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using Models.BasicSetup.Models;
using Models.BasicSetup.ViewModels;
using Infrastructure.IO;
using Models.Company.Models;
using ProjLib.Constants;

namespace Services.BasicSetup
{
    public interface IProductService : IDisposable
    {
        Product Create(Product p);
        void Delete(int id);
        void Delete(Product p);
        ProductViewModel GetProduct(int p);
        IEnumerable<Product> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Product p);
        Product Add(Product p);
        IQueryable<Product> GetProductList();
        IEnumerable<Product> GetProductList(int prodyctTypeId);
        IQueryable<Product> GetProductForIndex();
        Task<IEquatable<Product>> GetAsync();
        Task<Product> FindAsync(int id);
        Product Find(string ProductName);
        Product FindProduct(string ProductName);
        Product Find(int id);
        IEnumerable<Product> GetAccessoryList();
        IQueryable<ProductIndexViewModel> GetProductListForIndex(int ProductTypeId);
        IQueryable<ProductIndexViewModel> GetProductListForIndex();
        IQueryable<ProductViewModel> GetProductListForGroup(int id);
        IQueryable<ProductIndexViewModel> GetProductListForMaterial(int ProductTypeId);
        MaterialViewModel GetMaterialProduct(int id);
        int NextId(int id);
        int PrevId(int id);
        int NextMaterialId(int id, int nid);
        int PrevMaterialId(int id, int nid);
        bool CheckForNameExists(string Name);
        bool CheckForNameExists(string Name, int Id);
        ProductPrevProcess FGetProductPrevProcess(int ProductId, int ProcessId);
        ComboBoxPagedResult GetProductHelpList(string searchTerm, int pageSize, int pageNum);
        #region HelpList Getter
        /// <summary>
        /// *General Function*
        /// This function will create the help list for Projects
        /// </summary>
        /// <param name="searchTerm">user search term</param>
        /// <param name="pageSize">no of records to fetch for each page</param>
        /// <param name="pageNum">current page size </param>
        /// <returns>ComboBoxPagedResult</returns>
        ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum);
        #endregion

        #region HelpList Setters
        /// <summary>
        /// *General Function*
        /// This function will return the object in (Id,Text) format based on the Id
        /// </summary>
        /// <param name="Id">Primarykey of the record</param>
        /// <returns>ComboBoxResult</returns>
        ComboBoxResult GetValue(int Id);

        /// <summary>
        /// *General Function*
        /// This function will return list of object in (Id,Text) format based on the Ids
        /// </summary>
        /// <param name="Id">PrimaryKey of the record(',' seperated string)</param>
        /// <returns>List<ComboBoxResult></returns>
        List<ComboBoxResult> GetListCsv(string Id);
        #endregion
    }



    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Product> _productRepository;

        public ProductService(IUnitOfWork unitOfWork, IRepository<Product> prodRepo)
        {
            _unitOfWork = unitOfWork;
            _productRepository = prodRepo;
        }
        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _productRepository = unitOfWork.Repository<Product>();
        }

        public MaterialViewModel GetMaterialProduct(int id)//ProductCategoryId
        {
            return (from p in _productRepository.Instance
                    where p.ProductId == id
                    select new MaterialViewModel
                    {
                        ProductGroupName = p.ProductGroup.ProductGroupName,
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        ProductCode = p.ProductCode,
                        ProductGroupId = (int)p.ProductGroupId,
                        StandardCost = p.StandardCost,
                        Tags = p.Tags,
                        UnitId = p.UnitId,
                        IsActive = p.IsActive,
                        SalesTaxGroupProductId = p.SalesTaxGroupProductId,
                        ImageFileName = p.ImageFileName,
                        ImageFolderName = p.ImageFolderName,
                        ReferenceDocId = p.ReferenceDocId,
                    }
                        ).FirstOrDefault();
        }

        public IQueryable<ProductViewModel> GetProductListForGroup(int id)
        {
            return (from p in _productRepository.Instance
                    where p.ProductGroupId == id
                    select new ProductViewModel
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                    });
        }


        public ProductViewModel GetProduct(int pId)
        {
            var temp = from p in _productRepository.Instance
                       where p.ProductId == pId
                       select new ProductViewModel
                       {

                           ProductId = p.ProductId,
                           ProductDescription = p.ProductDescription,
                           StandardCost = p.StandardCost,
                           UnitId = p.UnitId,
                           UnitName = p.Unit.UnitName,
                           ProductName = p.ProductName,
                           ProductCode = p.ProductCode,
                           SalesTaxGroupProductId = p.SalesTaxGroupProductId,
                           DivisionId = p.DivisionId,
                           DivisionName = p.Division.DivisionName,
                           ProductGroupId = (int)p.ProductGroupId,
                           ProductGroupName = p.ProductGroup.ProductGroupName,
                           IsActive = p.IsActive,
                           IsSystemDefine = p.IsSystemDefine,
                           StandardWeight = p.StandardWeight,
                           ProductSpecification = p.ProductSpecification,
                           //ColourId = p.ColourId,
                           //DescriptionOfGoodsId = p.DescriptionOfGoodsId,
                           //OriginCountryId = p.OriginCountryId
                       };



            return (temp).FirstOrDefault();


        }

        public Product Create(Product p)
        {
            p.ObjectState = ObjectState.Added;
            _productRepository.Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _productRepository.Delete(id);
        }

        public void Delete(Product p)
        {
            _productRepository.Delete(p);
        }

        public void Update(Product p)
        {
            p.ObjectState = ObjectState.Modified;
            _productRepository.Update(p);
        }

        public IEnumerable<Product> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _productRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductCode))
                .Filter(q => !string.IsNullOrEmpty(q.ProductName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public IQueryable<Product> GetProductList()
        {
            var p = _productRepository.Query().Get().OrderBy(m => m.ProductName);

            return p;

        }

        public IQueryable<Product> GetProductForIndex()
        {
            var pt = _unitOfWork.Repository<Product>().Query().Get().OrderBy(m => m.ProductName);

            return pt;
        }

        public IEnumerable<Product> GetAccessoryList()
        {
            var p = _productRepository.Query().Get();
            return p;
        }



        public Product Find(string ProductName)
        {

            Product p = _productRepository.Query().Get().Where(i => i.ProductName == ProductName).FirstOrDefault();

            return p;
        }

        public Product FindProduct(string ProductName)
        {

            int ProductNatureRawMaterial = 1;
            int ProductNatureFinishedMaterial = 2;
            int ProductNatureOtherMaterial = 3;
            var temp = from P in _productRepository.Instance
                       join PG in _unitOfWork.Repository<ProductGroup>().Instance on P.ProductGroupId equals PG.ProductGroupId into tableProductGroup
                       from tabProductGroup in tableProductGroup.DefaultIfEmpty()
                       join PT in _unitOfWork.Repository<ProductType>().Instance on tabProductGroup.ProductTypeId equals PT.ProductTypeId into tableTypeGroup
                       from tabProductType in tableTypeGroup.DefaultIfEmpty()
                       where P.ProductName == ProductName && ((int)tabProductType.ProductNatureId == ProductNatureRawMaterial || (int)tabProductType.ProductNatureId == ProductNatureFinishedMaterial || (int)tabProductType.ProductNatureId == ProductNatureOtherMaterial)
                       select P;

            return (temp).FirstOrDefault();

        }

        public Product Find(int id)
        {
            return _productRepository.Find(id);
        }


        public IEnumerable<Product> GetProductList(int productTypeId)
        {
            return _productRepository.Query().Include(i => i.ProductGroup).Get().OrderBy(m => m.ProductCode);
        }

        public IQueryable<ProductIndexViewModel> GetProductListForIndex(int ProductTypeId)
        {
            //Using LINQ
            var temp = (from p in _productRepository.Instance
                        join p3 in _unitOfWork.Repository<ProductGroup>().Instance on p.ProductGroupId equals p3.ProductGroupId
                        join p4 in _unitOfWork.Repository<Division>().Instance on p.DivisionId equals p4.DivisionId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join p5 in _unitOfWork.Repository<Unit>().Instance on p.UnitId equals p5.UnitId into table3
                        from tab3 in table3.DefaultIfEmpty()
                        orderby p.ProductName
                        select new ProductIndexViewModel
                        {
                            ProductId = p.ProductId,
                            ProductCode = p.ProductCode,
                            ProductName = p.ProductName,
                            ProductGroupName = p3.ProductGroupName,
                            UnitName = tab3.UnitName,
                            DivisionName = tab2.DivisionName,
                        });
            return temp;
        }

        public IQueryable<ProductIndexViewModel> GetProductListForIndex()
        {
            //Using LINQ
            var temp = (from p in _productRepository.Instance
                        join p3 in _unitOfWork.Repository<ProductGroup>().Instance on p.ProductGroupId equals p3.ProductGroupId
                        join p4 in _unitOfWork.Repository<Division>().Instance on p.DivisionId equals p4.DivisionId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join p5 in _unitOfWork.Repository<Unit>().Instance on p.UnitId equals p5.UnitId into table3
                        from tab3 in table3.DefaultIfEmpty()
                        orderby p.ProductName
                        select new ProductIndexViewModel
                        {
                            ProductId = p.ProductId,
                            ProductCode = p.ProductCode,
                            ProductName = p.ProductName,
                            ProductGroupName = p3.ProductGroupName,
                            UnitName = tab3.UnitName,
                            DivisionName = tab2.DivisionName,
                            IsActive = p.IsActive,
                            IsSystemDefine = p.IsSystemDefine

                        });
            return temp;
        }

        public IQueryable<ProductIndexViewModel> GetProductListForMaterial(int ProductTypeId)
        {

            var temp = (from p in _productRepository.Instance
                        join t in _unitOfWork.Repository<ProductGroup>().Instance on p.ProductGroupId equals t.ProductGroupId into table
                        from tab in table.DefaultIfEmpty()
                        where tab.ProductTypeId == ProductTypeId
                        orderby p.ProductName
                        select new ProductIndexViewModel
                        {
                            ProductId = p.ProductId,
                            ProductCode = p.ProductCode,
                            ProductName = p.ProductName,
                            ProductGroupName = tab.ProductGroupName,
                            IsActive = p.IsActive,
                            IsSystemDefine = p.IsSystemDefine

                        });
            return temp;

        }

        public Product Add(Product p)
        {
            _productRepository.Add(p);
            return p;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<Product>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Product> FindAsync(int id)
        {
            throw new NotImplementedException();
        }




        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _productRepository.Instance
                        orderby p.ProductName
                        select p.ProductId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _productRepository.Instance
                        orderby p.ProductName
                        select p.ProductId).FirstOrDefault();
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

                temp = (from p in _productRepository.Instance
                        orderby p.ProductName
                        select p.ProductId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _productRepository.Instance
                        orderby p.ProductName
                        select p.ProductId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public int NextMaterialId(int id, int ProductGroupId)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _productRepository.Instance
                        where p.ProductGroupId == ProductGroupId
                        orderby p.ProductName
                        select p.ProductId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _productRepository.Instance
                        where p.ProductGroupId == ProductGroupId
                        orderby p.ProductName
                        select p.ProductId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevMaterialId(int id, int ProductGroupId)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in _productRepository.Instance
                        where p.ProductGroupId == ProductGroupId
                        orderby p.ProductName
                        select p.ProductId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _productRepository.Instance
                        where p.ProductGroupId == ProductGroupId
                        orderby p.ProductName
                        select p.ProductId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ProductPrevProcess FGetProductPrevProcess(int ProductId, int ProcessId)
        {
            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
            SqlParameter SqlParameterProcessId = new SqlParameter("@ProcessId", ProcessId);

            ProductPrevProcess ProductPrevProcess = _unitOfWork.SqlQuery<ProductPrevProcess>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetPrevProcessForProduct @ProductId, @ProcessId", SqlParameterProductId, SqlParameterProcessId).FirstOrDefault();

            return ProductPrevProcess;
        }

        public ComboBoxPagedResult GetProductHelpList(string searchTerm, int pageSize, int pageNum)
        {

            var ProductTypeId = _unitOfWork.Repository<ProductType>().Query().Get().Where(m => m.ProductTypeName == ProductTypeConstants.Rug).FirstOrDefault().ProductTypeId;

            var Query = (from p in _productRepository.Instance
                         join t in _unitOfWork.Repository<ProductGroup>().Instance on p.ProductGroupId equals t.ProductGroupId
                         where t.ProductTypeId == ProductTypeId
                         orderby p.ProductName
                         select new ComboBoxResult
                         {
                             id = p.ProductId.ToString(),
                             text = p.ProductName,
                         });

            if (!string.IsNullOrEmpty(searchTerm))
                Query = Query.Where(m => m.text.ToLower().Contains(searchTerm.ToLower()));

            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return Data;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _productRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.ProductName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.ProductName
                        select new ComboBoxResult
                        {
                            text = pr.ProductName,
                            id = pr.ProductId.ToString()
                        }
              );

            var temp = list
               .Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = list.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return Data;
        }

        public ComboBoxPagedResult GetMachineList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _productRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.ProductName.ToLower().Contains(searchTerm.ToLower())))
                        && pr.ProductGroup.ProductType.ProductTypeName == ProductTypeConstants.Machine
                        orderby pr.ProductName
                        select new ComboBoxResult
                        {
                            text = pr.ProductName,
                            id = pr.ProductId.ToString()
                        }
              );

            var temp = list
               .Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = list.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return Data;
        }

        public ComboBoxResult GetValue(int Id)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Product> Products = from pr in _productRepository.Instance
                                          where pr.ProductId == Id
                                          select pr;

            ProductJson.id = Products.FirstOrDefault().ProductId.ToString();
            ProductJson.text = Products.FirstOrDefault().ProductName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Product> Products = from pr in _productRepository.Instance
                                              where pr.ProductId == temp
                                              select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Products.FirstOrDefault().ProductId.ToString(),
                    text = Products.FirstOrDefault().ProductName
                });
            }
            return ProductJson;
        }

        public bool CheckForNameExists(string Name)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _productRepository.Instance
                        where pr.ProductName == Name
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;

        }
        public bool CheckForNameExists(string Name, int Id)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _productRepository.Instance
                        where pr.ProductName == Name && pr.ProductId != Id
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }

    }

    public class ProductPrevProcess
    {
        public int ProcessId { get; set; }
    }
}
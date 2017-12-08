using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using Models.BasicSetup.Models;
using Models.BasicSetup.ViewModels;
using Infrastructure.IO;
using ProjLib.Constants;
using Models.Company.Models;

namespace Services.BasicSetup
{
    public interface IProductUidService : IDisposable
    {
        ProductUid Create(ProductUid p);
        void Delete(int id);
        void Delete(ProductUid p);
        IEnumerable<ProductUid> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductUid p);
        ProductUid Add(ProductUid p);
        IEnumerable<ProductUid> GetProductUidList();
        IEnumerable<ProductUid> GetProductUidList(int prodyctTypeId);
        Task<IEquatable<ProductUid>> GetAsync();
        Task<ProductUid> FindAsync(int id);
        ProductUid Find(string ProductUidName);
        ProductUid Find(int id);
        IEnumerable<ProductUid> FindForJobOrderLine(int id);
        IEnumerable<ProductUid> FindByGenLineId(int id, int DocTypeId);
        ProductUidDetail FGetProductUidDetail(string ProductUidNo);
        UIDValidationViewModel ValidateUID(string ProductUID, bool PostedInStock, int? GodownId);
        UIDValidationViewModel ValidateUIDOnReceive(string ProductUID, bool PostedInStock, int PersonId);
        List<ProductUid> GetBCForProductUidHeaderId(int id);
        bool IsProcessDone(string ProductUidName, int ProcessId);
        bool IsProcessDone(int ProductUidId, int ProcessId);
    }



    public class ProductUidService : IProductUidService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ProductUid> _productRepository;

        public ProductUidService(IUnitOfWork unitOfWork, IRepository<ProductUid> ProdUidRepo)
        {
            _unitOfWork = unitOfWork;
            _productRepository = ProdUidRepo;
        }        

        public ProductUidService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _productRepository = unitOfWork.Repository<ProductUid>();
        }


        public ProductUid Create(ProductUid p)
        {
            p.ObjectState = ObjectState.Added;
            _productRepository.Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _productRepository.Delete(id);
        }

        public void Delete(ProductUid p)
        {
            _productRepository.Delete(p);
        }

        public void Update(ProductUid p)
        {
            p.ObjectState = ObjectState.Modified;
            _productRepository.Update(p);
        }

        public IEnumerable<ProductUid> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _productRepository
                .Query()
                .Filter(q => !string.IsNullOrEmpty(q.ProductUidName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public IEnumerable<ProductUid> GetProductUidList()
        {
            var p = _productRepository.Query().Get();

            return p;

        }

        public IEnumerable<ProductUid> GetAccessoryList()
        {
            var p = _productRepository.Query().Get();
            return p;
        }



        public ProductUid Find(string ProductUidName)
        {
            ProductUid p = _productRepository.Query().Get().Where(i => i.ProductUidName == ProductUidName).FirstOrDefault();

            return p;
        }

        public ProductUid Find(int id)
        {

            return _productRepository.Find(id);
        }


        public IEnumerable<ProductUid> GetProductUidList(int productTypeId)
        {
            return _productRepository.Query().Get().OrderBy(m => m.ProductUIDId);
        }

        public ProductUid Add(ProductUid p)
        {
            _productRepository.Add(p);
            return p;
        }

        public IEnumerable<ProductUid> FindForJobOrderLine(int id)
        {
            return (from p in _productRepository.Instance
                    where p.ProductUidHeaderId == id
                    select p
                        ).ToList();
        }

        public IEnumerable<ProductUid> FindByGenLineId(int id, int DocTypeId)
        {
            return (from p in _productRepository.Instance
                    where p.GenLineId == id && p.GenDocTypeId == DocTypeId
                    select p
                        ).ToList();
        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<ProductUid>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductUid> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public ProductUidDetail FGetProductUidDetail(string ProductUidNo)
        {
            ProductUidDetail UidDetail = (from Pu in _productRepository.Instance
                                          join P in _unitOfWork.Repository<Product>().Instance on Pu.ProductId equals P.ProductId into ProductTable
                                          from Producttab in ProductTable.DefaultIfEmpty()
                                          join Pr in _unitOfWork.Repository<Process>().Instance on Pu.CurrenctProcessId equals Pr.ProcessId into ProcessTable
                                          from ProcessTab in ProcessTable.DefaultIfEmpty()
                                          join Pe in _unitOfWork.Repository<Person>().Instance on Pu.LastTransactionPersonId equals Pe.PersonID into PersonTable
                                          from PersonTab in PersonTable.DefaultIfEmpty()
                                          where Pu.ProductUidName == ProductUidNo
                                          select new ProductUidDetail
                                          {
                                              ProductUidId = Pu.ProductUIDId,
                                              ProductId = Pu.ProductId,
                                              ProductName = Producttab.ProductName,
                                              PrevProcessId = Pu.CurrenctProcessId,
                                              PrevProcessName = ProcessTab.ProcessName,
                                              LastTransactionDocNo = Pu.LastTransactionDocNo,
                                              LastTransactionDocDate = Pu.LastTransactionDocDate,
                                              LastTransactionPersonName = PersonTab.Name,
                                              CurrenctGodownId = Pu.CurrenctGodownId,
                                              Status = Pu.Status,
                                              GenDocTypeId = Pu.GenDocTypeId,
                                              DivisionId = Producttab.DivisionId
                                          }).FirstOrDefault();

            return UidDetail;
        }


        public UIDValidationViewModel ValidateUID(string ProductUID, bool PostedInStock, int? GodownId)
        {

            UIDValidationViewModel temp = new UIDValidationViewModel();
            var UID = (from p in _productRepository.Instance
                       where p.ProductUidName == ProductUID
                       select new UIDValidationViewModel
                       {
                           CurrenctGodownId = p.CurrenctGodownId,
                           CurrenctProcessId = p.CurrenctProcessId,
                           CurrentGodownName = p.CurrenctGodown.GodownName,
                           CurrentProcessName = p.CurrenctProcess.ProcessName,
                           GenDocDate = p.GenDocDate,
                           GenDocId = p.GenDocId,
                           GenLineId = p.GenLineId,
                           GenDocNo = p.GenDocNo,
                           GenDocTypeId = p.GenDocTypeId,
                           GenDocTypeName = p.GenDocType.DocumentTypeName,
                           GenPersonId = p.GenPersonId,
                           GenPersonName = p.GenPerson.Name,
                           IsActive = p.IsActive,
                           LastTransactionDocDate = p.LastTransactionDocDate,
                           LastTransactionDocId = p.LastTransactionDocId,
                           LastTransactionDocNo = p.LastTransactionDocNo,
                           LastTransactionDocTypeId = p.LastTransactionDocTypeId,
                           LastTransactionDocTypeName = p.LastTransactionDocType.DocumentTypeName,
                           LastTransactionPersonId = p.LastTransactionPersonId,
                           LastTransactionPersonName = p.LastTransactionPerson.Name,
                           ProductId = p.ProductId,
                           ProductName = p.Product.ProductName,
                           ProductUIDId = p.ProductUIDId,
                           ProductUidName = p.ProductUidName,
                           Status = p.Status,
                           LotNo = p.LotNo,
                           Dimension1Id = p.Dimension1Id,
                           Dimension1Name = p.Dimension1.Dimension1Name,
                           Dimension2Id = p.Dimension2Id,
                           Dimension2Name = p.Dimension2.Dimension2Name,
                           ProductUidHeaderId = p.ProductUidHeaderId,
                       }).FirstOrDefault();

            if (UID == null)
            {
                UID = new UIDValidationViewModel();
                UID.ErrorType = "InvalidID";
                UID.ErrorMessage = "Invalid ProductUID";
            }
            else //if (PostedInStock == true || UID.ProductUidHeaderId==null )
            {
                if (UID.CurrenctGodownId == null)
                {
                    UID.ErrorType = "GodownNull";
                    UID.ErrorMessage = "Product is not present in Godown " + (GodownId.HasValue ? _unitOfWork.Repository<Godown>().Find(GodownId.Value).GodownName : "") + "Status of Product " + UID.ProductName + " is " + UID.Status;

                }

                else if (!GodownId.HasValue || UID.CurrenctGodownId != GodownId)
                {
                    UID.ErrorType = "InvalidGodown";
                    UID.ErrorMessage = "Product is not present in Godown " + (GodownId.HasValue ? _unitOfWork.Repository<Godown>().Find(GodownId.Value).GodownName : "");
                }
                else
                {
                    UID.ErrorType = "Success";
                }
            }



            return UID;
        }


        public UIDValidationViewModel ValidateUIDOnReceive(string ProductUID, bool PostedInStock, int PersonId)
        {

            UIDValidationViewModel temp = new UIDValidationViewModel();
            var UID = (from p in _productRepository.Instance
                       where p.ProductUidName == ProductUID
                       select new UIDValidationViewModel
                       {
                           CurrenctGodownId = p.CurrenctGodownId,
                           CurrenctProcessId = p.CurrenctProcessId,
                           CurrentGodownName = p.CurrenctGodown.GodownName,
                           CurrentProcessName = p.CurrenctProcess.ProcessName,
                           GenDocDate = p.GenDocDate,
                           GenDocId = p.GenDocId,
                           GenDocNo = p.GenDocNo,
                           GenLineId = p.GenLineId,
                           GenDocTypeId = p.GenDocTypeId,
                           GenDocTypeName = p.GenDocType.DocumentTypeName,
                           GenPersonId = p.GenPersonId,
                           GenPersonName = p.GenPerson.Name,
                           IsActive = p.IsActive,
                           LastTransactionDocDate = p.LastTransactionDocDate,
                           LastTransactionDocId = p.LastTransactionDocId,
                           LastTransactionDocNo = p.LastTransactionDocNo,
                           LastTransactionDocTypeId = p.LastTransactionDocTypeId,
                           LastTransactionDocTypeName = p.LastTransactionDocType.DocumentTypeName,
                           LastTransactionPersonId = p.LastTransactionPersonId,
                           LastTransactionPersonName = p.LastTransactionPerson.Name,
                           ProductId = p.ProductId,
                           ProductName = p.Product.ProductName,
                           ProductUIDId = p.ProductUIDId,
                           ProductUidName = p.ProductUidName,
                           Status = p.Status,
                           LotNo = p.LotNo,
                           Dimension1Id = p.Dimension1Id,
                           Dimension1Name = p.Dimension1.Dimension1Name,
                           Dimension2Id = p.Dimension2Id,
                           Dimension2Name = p.Dimension2.Dimension2Name,

                       }).FirstOrDefault();



            if (UID == null)
            {
                UID = new UIDValidationViewModel();
                UID.ErrorType = "InvalidID";
                UID.ErrorMessage = "Invalid ProductUID";
            }
            else if (UID.Status != ProductUidStatusConstants.Issue)
            {
                UID.ErrorType = "InvalidID";
                UID.ErrorMessage = "ProductUID is already received/cancelled from this jobworker";
            }
            else if (PostedInStock == true)
            {
                if (UID.CurrenctGodownId != null)
                {
                    UID.ErrorType = "GodownNull";
                    UID.ErrorMessage = " Product " + UID.ProductName + " is already in Stock at Godown " + _unitOfWork.Repository<Godown>().Find(UID.CurrenctGodownId ?? 0).GodownName;

                }
                else
                {
                    UID.ErrorType = "Success";
                }
            }
            else if (UID.LastTransactionPersonId != PersonId)
            {
                UID.ErrorType = "InvalidGodown";
                UID.ErrorMessage = "Product does not belong to this Supplier ";
            }
            else
            {
                UID.ErrorType = "Success";
            }


            return UID;
        }


        public bool IsProcessDone(int ProductUidId, int ProcessId)
        {
            string ProcessString = "|" + ProcessId.ToString() + "|";

            var temp = (from P in _productRepository.Instance
                        where P.ProductUIDId == ProductUidId && P.ProcessesDone.Contains(ProcessString)
                        select new { ProductUidId = P.ProductUIDId }).FirstOrDefault();

            if (temp != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsProcessDone(string ProductUidName, int ProcessId)
        {
            string ProcessString = "|" + ProcessId.ToString() + "|";

            var temp = (from P in _productRepository.Instance
                        where P.ProductUidName == ProductUidName && P.ProcessesDone.Contains(ProcessString)
                        select new { ProductUidId = P.ProductUIDId }).FirstOrDefault();

            if (temp != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public List<ProductUid> GetBCForProductUidHeaderId(int id)
        {
            return (from p in _productRepository.Instance
                    where p.ProductUidHeaderId == id
                    select p).ToList();
        }


    }

    public class ProductUidDetail
    {
        public int ProductUidId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? PrevProcessId { get; set; }
        public string PrevProcessName { get; set; }
        public int? LastTransactionDocId { get; set; }
        public int? LastTransactionDocTypeId { get; set; }
        public string LastTransactionDocNo { get; set; }
        public DateTime? LastTransactionDocDate { get; set; }
        public int? LastTransactionPersonId { get; set; }
        public string LastTransactionPersonName { get; set; }
        public int? CurrenctGodownId { get; set; }
        public int? CurrenctProcessId { get; set; }
        public int? ProductInvoiceGroupId { get; set; }
        public string ProductInvoiceGroupName { get; set; }
        public string Status { get; set; }

        public int DivisionId { get; set; }

        public int? GenDocTypeId { get; set; }

        public decimal NField1 { get; set; }

    }
}

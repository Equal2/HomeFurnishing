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

namespace Service
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
        IEnumerable<ProductUidViewModel> GetPendingRoadPermitForms(int id);//SupplierId
        ProductUidDetail FGetProductUidDetail(string ProductUidNo);
        ProductUidDetail FGetProductUidLastValues(int ProductUidId, string OMSId);
        UIDValidationViewModel ValidateUID(string ProductUID, bool PostedInStock, int? GodownId);
        UIDValidationViewModel ValidateUID(string ProductUID);
        List<ProductUid> GetBCForProductUidHeaderId(int id);
        IQueryable<ProductUid> GetProductUidListMachine(int GenDocTypeId);
        IQueryable<UIDValidationViewModel> GetProductUidListMachineDetail(int GenDocTypeId);
        int NextId(int id, int GenDocTypeId);
        int PrevId(int id, int GenDocTypeId);
    }



    public class ProductUidService : IProductUidService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductUid> _productRepository;
        //private readonly Repository<ProductUidDimension> _productdimensionRepository;

        RepositoryQuery<ProductUid> productRepository;
        //RepositoryQuery<ProductUidDimension> productdimensionRepository;

        public ProductUidService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _productRepository = new Repository<ProductUid>(db);
            //_productdimensionRepository = new Repository<ProductUidDimension>(db);

            productRepository = new RepositoryQuery<ProductUid>(_productRepository);
            // productdimensionRepository = new RepositoryQuery<ProductUidDimension>(_productdimensionRepository);
        }



        public ProductUid Create(ProductUid p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductUid>().Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductUid>().Delete(id);
        }

        public void Delete(ProductUid p)
        {
            _unitOfWork.Repository<ProductUid>().Delete(p);
        }
        public IEnumerable<ProductUidViewModel> GetPendingRoadPermitForms(int id)//SupplierId
        {
            return (from p in db.ProductUid
                    join t in db.PurchaseGoodsReceiptHeader on p.ProductUIDId equals t.RoadPermitFormId into table
                    from tab in table.Where(m => m.RoadPermitFormId == null).DefaultIfEmpty()
                    orderby p.ProductUidName
                    select new ProductUidViewModel
                    {
                        ProductUIDId = p.ProductUIDId,
                        ProductUidName = p.ProductUidName
                    }
                        );
        }
        public void Update(ProductUid p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductUid>().Update(p);
        }

        public IEnumerable<ProductUid> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductUid>()
                .Query()
                .Filter(q => !string.IsNullOrEmpty(q.ProductUidName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public IEnumerable<ProductUid> GetProductUidList()
        {
            var p = _unitOfWork.Repository<ProductUid>().Query().Get();

            return p;

        }

        public IEnumerable<ProductUid> GetAccessoryList()
        {
            var p = _unitOfWork.Repository<ProductUid>().Query().Get();
            return p;
        }



        public ProductUid Find(string ProductUidName)
        {
            ProductUid p = _unitOfWork.Repository<ProductUid>().Query().Get().Where(i => i.ProductUidName == ProductUidName).FirstOrDefault();

            return p;
        }


        public ProductUid Find(int id)
        {

            return _unitOfWork.Repository<ProductUid>().Find(id);
        }


        public IEnumerable<ProductUid> GetProductUidList(int productTypeId)
        {
            return _unitOfWork.Repository<ProductUid>().Query().Get().OrderBy(m => m.ProductUIDId);
        }

        public ProductUid Add(ProductUid p)
        {
            _unitOfWork.Repository<ProductUid>().Add(p);
            return p;
        }

        public IQueryable<ProductUid> GetProductUidListMachine(int GenDocTypeId)
        {
            var pt = _unitOfWork.Repository<ProductUid>().Query().Get().OrderBy(m => m.ProductUidName).Where(m => m.GenDocTypeId == GenDocTypeId);

            return pt;
        }
        public IQueryable<UIDValidationViewModel> GetProductUidListMachineDetail(int GenDocTypeId)
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            return (from p in db.ProductUid
                    join g in db.Godown on p.CurrenctGodownId equals g.GodownId
                    orderby p.ProductUidName
                    where p.GenDocTypeId == GenDocTypeId && g.SiteId == SiteId
                    select new UIDValidationViewModel
                    {
                        ProductUIDId =p.ProductUIDId,
                        ProductUidName=p.ProductUidName,
                        ProductName=p.Product.ProductName,
                        CurrentGodownName=p.CurrenctGodown.GodownName,
                        GenDocTypeId=p.GenDocTypeId,
                        IsActive=p.IsActive,
                    });
        }

        public IEnumerable<ProductUid> FindForJobOrderLine(int id)
        {
            return (from p in db.ProductUid
                    where p.ProductUidHeaderId == id
                    select p
                        ).ToList();
        }

        public IEnumerable<ProductUid> FindByGenLineId(int id, int DocTypeId)
        {
            return (from p in db.ProductUid
                    where p.GenLineId == id && p.GenDocTypeId == DocTypeId
                    select p
                        ).ToList();
        }


        public void Dispose()
        {
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
            ProductUidDetail UidDetail = (from Pu in db.ProductUid
                                          join P in db.Product on Pu.ProductId equals P.ProductId into ProductTable
                                          from Producttab in ProductTable.DefaultIfEmpty()
                                          join Pr in db.Process on Pu.CurrenctProcessId equals Pr.ProcessId into ProcessTable
                                          from ProcessTab in ProcessTable.DefaultIfEmpty()
                                          join Pe in db.Persons on Pu.LastTransactionPersonId equals Pe.PersonID into PersonTable
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

        //To Do
        //Currently we are Retrieving Product Uid Last Values from old Software after the whold data will be migratted then this fumction will rectrived values from new Tables.

        public ProductUidDetail FGetProductUidLastValues(int ProductUidId, string OMSId)
        {
            SqlParameter SqlParameterProductUidId = new SqlParameter("@ProductUidId", ProductUidId);
            SqlParameter SqlParameterOMSId = new SqlParameter("@OMSId", OMSId);

            ProductUidDetail ProductUidLastValues = db.Database.SqlQuery<ProductUidDetail>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetProductUidLastValues @ProductUidId, @OMSId", SqlParameterProductUidId, SqlParameterOMSId).FirstOrDefault();

            return ProductUidLastValues;


        }


        public UIDValidationViewModel ValidateUID(string ProductUID, bool PostedInStock, int? GodownId)
        {

            UIDValidationViewModel temp = new UIDValidationViewModel();
            var UID = (from p in db.ProductUid
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
                           GenPersonName = p.GenPerson.Person.Name,
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
                    UID.ErrorMessage = "Product is not present in Godown " + (GodownId.HasValue ? new GodownService(_unitOfWork).Find(GodownId.Value).GodownName : "") + "Status of Product " + UID.ProductName + " is " + UID.Status;

                }

                else if (!GodownId.HasValue || UID.CurrenctGodownId != GodownId)
                {
                    UID.ErrorType = "InvalidGodown";
                    UID.ErrorMessage = "Product is not present in Godown " + (GodownId.HasValue ? new GodownService(_unitOfWork).Find(GodownId.Value).GodownName : "");
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
            var UID = (from p in db.ProductUid
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
                           GenPersonName = p.GenPerson.Person.Name,
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
                    UID.ErrorMessage = " Product " + UID.ProductName + " is already in Stock at Godown " + new GodownService(_unitOfWork).Find(UID.CurrenctGodownId ?? 0).GodownName;

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


        public UIDValidationViewModel ValidateUIDOnJobCancelMain(string ProductUID, bool PostedInStock, int PersonId)
        {

            UIDValidationViewModel temp = new UIDValidationViewModel();
            var UID = (from p in db.ProductUid
                       where p.ProductUidName == ProductUID
                       join t in db.ProductUidHeader on p.ProductUidHeaderId equals t.ProductUidHeaderId
                       join t2 in db.JobOrderLine on t.ProductUidHeaderId equals t2.ProductUidHeaderId
                       select new UIDValidationViewModel
                       {
                           CurrenctGodownId = p.CurrenctGodownId,
                           CurrenctProcessId = p.CurrenctProcessId,
                           CurrentGodownName = p.CurrenctGodown.GodownName,
                           CurrentProcessName = p.CurrenctProcess.ProcessName,
                           GenDocDate = p.GenDocDate,
                           GenDocId = p.GenDocId,
                           GenDocNo = p.GenDocNo,
                           GenLineId = t2.JobOrderLineId,
                           GenDocTypeId = p.GenDocTypeId,
                           GenDocTypeName = p.GenDocType.DocumentTypeName,
                           GenPersonId = p.GenPersonId,
                           GenPersonName = p.GenPerson.Person.Name,
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
            else if (UID.Status != ProductUidStatusConstants.Issue && UID.Status != ProductUidStatusConstants.Return)
            {
                UID.ErrorType = "InvalidID";
                UID.ErrorMessage = "ProductUID is already received/cancelled from this jobworker";
            }
            else if (PostedInStock == true)
            {
                if (UID.CurrenctGodownId != null)
                {
                    UID.ErrorType = "GodownNull";
                    UID.ErrorMessage = " Product " + UID.ProductName + " is already in Stock at Godown " + new GodownService(_unitOfWork).Find(UID.CurrenctGodownId ?? 0).GodownName;
                }
                else
                {
                    UID.ErrorType = "Success";
                }
            }
            else
            {
                UID.ErrorType = "Success";
            }


            return UID;

        }

        public UIDValidationViewModel ValidateUIDOnJobCancelBranch(string ProductUID, bool PostedInStock, int PersonId)
        {

            UIDValidationViewModel temp = new UIDValidationViewModel();
            var UID = (from p in db.ProductUid
                       where p.ProductUidName == ProductUID
                       join t in db.ProductUidHeader on p.ProductUidHeaderId equals t.ProductUidHeaderId
                       join t2 in db.JobOrderLine on t.ProductUidHeaderId equals t2.ProductUidHeaderId
                       select new UIDValidationViewModel
                       {
                           CurrenctGodownId = p.CurrenctGodownId,
                           CurrenctProcessId = p.CurrenctProcessId,
                           CurrentGodownName = p.CurrenctGodown.GodownName,
                           CurrentProcessName = p.CurrenctProcess.ProcessName,
                           GenDocDate = p.GenDocDate,
                           GenDocId = p.GenDocId,
                           GenDocNo = p.GenDocNo,
                           GenLineId = t2.JobOrderLineId,
                           GenDocTypeId = p.GenDocTypeId,
                           GenDocTypeName = p.GenDocType.DocumentTypeName,
                           GenPersonId = p.GenPersonId,
                           GenPersonName = p.GenPerson.Person.Name,
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
            else if (UID.Status != ProductUidStatusConstants.Issue && UID.Status != ProductUidStatusConstants.Return)
            {
                UID.ErrorType = "InvalidID";
                UID.ErrorMessage = "ProductUID is already received/cancelled from this jobworker";
            }
            else if (PostedInStock == true)
            {
                if (UID.CurrenctGodownId != null)
                {
                    UID.ErrorType = "GodownNull";
                    UID.ErrorMessage = " Product " + UID.ProductName + " is already in Stock at Godown " + new GodownService(_unitOfWork).Find(UID.CurrenctGodownId ?? 0).GodownName;
                }
                else
                {
                    UID.ErrorType = "Success";
                }
            }
            else if (UID.GenPersonId != PersonId)
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

        public UIDValidationViewModel ValidateUIDOnJobReceiveMain(string ProductUID, bool PostedInStock, int HeaderID)
        {
            var JobRecHead = new JobReceiveHeaderService(_unitOfWork).Find(HeaderID);

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(JobRecHead.DocTypeId, JobRecHead.DivisionId, JobRecHead.SiteId);

            int[] ContraDocTypes = { };
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
            {
                ContraDocTypes = settings.filterContraDocTypes.Split(',').Select(Int32.Parse).ToArray();
            }



            //var temp345 = from p in db.ProductUid
            //            where p.ProductUidName == ProductUID
            //            join t in db.JobReceiveLine on p.ProductUIDId equals t.ProductUidId into table
            //            from tab in table.DefaultIfEmpty()
            //            join RecLineStatus in db.JobReceiveLineStatus on tab.JobReceiveLineId equals RecLineStatus.JobReceiveLineId into RecLineStatTab
            //            from RecLineStat in RecLineStatTab.DefaultIfEmpty()
            //            join t3 in db.JobReceiveHeader.Where(m => m.SiteId == SiteId && m.DivisionId == DivisionId) on tab.JobReceiveHeaderId equals t3.JobReceiveHeaderId into table5
            //            from JRH in table5.DefaultIfEmpty()
            //            join t2 in db.JobOrderLine on p.ProductUidHeaderId equals t2.ProductUidHeaderId 
            //            join JOH in db.JobOrderHeader on t2.JobOrderHeaderId equals JOH.JobOrderHeaderId
            //            where ( JRH == null || (RecLineStat!=null && ( tab.Qty - RecLineStat.ReturnQty ?? 0) == 0)) && p.Status != ProductUidStatusConstants.Cancel && (( p.GenPersonId == p.LastTransactionPersonId) || p.CurrenctGodownId != null)
            //            && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : ContraDocTypes.Contains(JRH.DocTypeId)) && JOH.SiteId==SiteId && JOH.DivisionId==DivisionId && JOH.ProcessId==JRH.ProcessId
            //            select new UIDValidationViewModel
            //            {
            //                CurrenctGodownId = p.CurrenctGodownId,
            //                CurrenctProcessId = p.CurrenctProcessId,
            //                CurrentGodownName = p.CurrenctGodown.GodownName,
            //                CurrentProcessName = p.CurrenctProcess.ProcessName,
            //                GenDocDate = p.GenDocDate,
            //                GenDocId = p.GenDocId,
            //                GenDocNo = p.GenDocNo,
            //                GenLineId = t2.JobOrderLineId,
            //                GenDocTypeId = p.GenDocTypeId,
            //                GenDocTypeName = p.GenDocType.DocumentTypeName,
            //                GenPersonId = p.GenPersonId,
            //                GenPersonName = p.GenPerson.Person.Name,
            //                IsActive = p.IsActive,
            //                LastTransactionDocDate = p.LastTransactionDocDate,
            //                LastTransactionDocId = p.LastTransactionDocId,
            //                LastTransactionDocNo = p.LastTransactionDocNo,
            //                LastTransactionDocTypeId = p.LastTransactionDocTypeId,
            //                LastTransactionDocTypeName = p.LastTransactionDocType.DocumentTypeName,
            //                LastTransactionPersonId = p.LastTransactionPersonId,
            //                LastTransactionPersonName = p.LastTransactionPerson.Name,
            //                ProductId = p.ProductId,
            //                ProductName = p.Product.ProductName,
            //                ProductUIDId = p.ProductUIDId,
            //                ProductUidName = p.ProductUidName,
            //                Status = p.Status,
            //                LotNo = p.LotNo,
            //                Dimension1Id = p.Dimension1Id,
            //                Dimension1Name = p.Dimension1.Dimension1Name,
            //                Dimension2Id = p.Dimension2Id,
            //                Dimension2Name = p.Dimension2.Dimension2Name,
            //            };

            try
            {

                //var temp = (from p in db.ProductUid
                //            where p.ProductUidName == ProductUID
                //            join t in db.JobReceiveLine on p.ProductUIDId equals t.ProductUidId into table
                //            from tab in table.DefaultIfEmpty()
                //            join RecLineStatus in db.JobReceiveLineStatus on tab.JobReceiveLineId equals RecLineStatus.JobReceiveLineId into RecLineStatTab
                //            from RecLineStat in RecLineStatTab.DefaultIfEmpty()
                //            join t3 in db.JobReceiveHeader.Where(m => m.SiteId == SiteId && m.DivisionId == DivisionId) on tab.JobReceiveHeaderId equals t3.JobReceiveHeaderId into table5
                //            from JRH in table5.DefaultIfEmpty()
                //            join t2 in db.JobOrderLine on p.ProductUidHeaderId equals t2.ProductUidHeaderId
                //            join JOH in db.JobOrderHeader.Where(m => m.SiteId == SiteId && m.DivisionId == DivisionId) on t2.JobOrderHeaderId equals JOH.JobOrderHeaderId
                //            where (JRH == null || (RecLineStat != null && (tab.Qty - RecLineStat.ReturnQty ?? 0) == 0)) && p.Status != ProductUidStatusConstants.Cancel && ((p.GenPersonId == p.LastTransactionPersonId) || p.CurrenctGodownId != null)
                //             && JOH.ProcessId == JRH.ProcessId
                //            select new UIDValidationViewModel
                //            {
                //                CurrenctGodownId = p.CurrenctGodownId,
                //                CurrenctProcessId = p.CurrenctProcessId,
                //                CurrentGodownName = p.CurrenctGodown.GodownName,
                //                CurrentProcessName = p.CurrenctProcess.ProcessName,
                //                GenDocDate = p.GenDocDate,
                //                GenDocId = p.GenDocId,
                //                GenDocNo = p.GenDocNo,
                //                GenLineId = t2.JobOrderLineId,
                //                GenDocTypeId = p.GenDocTypeId,
                //                GenDocTypeName = p.GenDocType.DocumentTypeName,
                //                GenPersonId = p.GenPersonId,
                //                GenPersonName = p.GenPerson.Person.Name,
                //                IsActive = p.IsActive,
                //                LastTransactionDocDate = p.LastTransactionDocDate,
                //                LastTransactionDocId = p.LastTransactionDocId,
                //                LastTransactionDocNo = p.LastTransactionDocNo,
                //                LastTransactionDocTypeId = p.LastTransactionDocTypeId,
                //                LastTransactionDocTypeName = p.LastTransactionDocType.DocumentTypeName,
                //                LastTransactionPersonId = p.LastTransactionPersonId,
                //                LastTransactionPersonName = p.LastTransactionPerson.Name,
                //                ProductId = p.ProductId,
                //                ProductName = p.Product.ProductName,
                //                ProductUIDId = p.ProductUIDId,
                //                ProductUidName = p.ProductUidName,
                //                Status = p.Status,
                //                LotNo = p.LotNo,
                //                Dimension1Id = p.Dimension1Id,
                //                Dimension1Name = p.Dimension1.Dimension1Name,
                //                Dimension2Id = p.Dimension2Id,
                //                Dimension2Name = p.Dimension2.Dimension2Name,
                //            }).FirstOrDefault();


                var temp = (from p in db.ProductUid
                            where p.ProductUidName == ProductUID
                            join t in db.JobReceiveLine on p.ProductUIDId equals t.ProductUidId into table
                            from tab in table.DefaultIfEmpty()
                            join t3 in db.JobReceiveHeader.Where(m => m.SiteId == SiteId && m.DivisionId == DivisionId) on tab.JobReceiveHeaderId equals t3.JobReceiveHeaderId into table5
                            from JRH in table5.DefaultIfEmpty()
                            join t2 in db.JobOrderLine on p.ProductUidHeaderId equals t2.ProductUidHeaderId
                            join JOH in db.JobOrderHeader.Where(m => m.SiteId == SiteId && m.DivisionId == DivisionId) on t2.JobOrderHeaderId equals JOH.JobOrderHeaderId
                            join RecLineStatus in db.JobReceiveLineStatus on tab.JobReceiveLineId equals RecLineStatus.JobReceiveLineId into RecLineStatTab
                            from RecLineStat in RecLineStatTab.DefaultIfEmpty()
                            where (JRH == null || ((tab.Qty - (RecLineStat.ReturnQty ?? 0)) == 0)) && p.Status != ProductUidStatusConstants.Cancel && ((p.GenPersonId == p.LastTransactionPersonId) || p.CurrenctGodownId != null)
                            && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : ContraDocTypes.Contains(JRH.DocTypeId)) && JOH.ProcessId == JobRecHead.ProcessId
                            select new UIDValidationViewModel
                            {
                                CurrenctGodownId = p.CurrenctGodownId,
                                CurrenctProcessId = p.CurrenctProcessId,
                                CurrentGodownName = p.CurrenctGodown.GodownName,
                                CurrentProcessName = p.CurrenctProcess.ProcessName,
                                GenDocDate = p.GenDocDate,
                                GenDocId = p.GenDocId,
                                GenDocNo = p.GenDocNo,
                                GenLineId = t2.JobOrderLineId,
                                GenDocTypeId = p.GenDocTypeId,
                                GenDocTypeName = p.GenDocType.DocumentTypeName,
                                GenPersonId = p.GenPersonId,
                                GenPersonName = p.GenPerson.Person.Name,
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



                if (temp == null)
                {
                    temp = new UIDValidationViewModel();
                    temp.ErrorType = "InvalidID";
                    temp.ErrorMessage = "";

                    var BarCode = (from p in db.ProductUid
                                   where p.ProductUidName == ProductUID
                                   select p).FirstOrDefault();

                    if (BarCode != null)
                    {
                        if (BarCode.Status == ProductUidStatusConstants.Cancel)
                        {
                            temp.ErrorMessage += "Barcode already Cancelled.";
                        }
                        if (BarCode.GenPersonId == BarCode.LastTransactionPersonId)
                        {
                            temp.ErrorMessage += " Last transaction Person do not match.";
                        }
                        if (BarCode.CurrenctGodownId == null)
                        {
                            temp.ErrorMessage += " Barcode not present in godown.";
                        }

                        var Rec = (from p in db.JobReceiveLine
                                   join t in db.JobReceiveHeader.Where(m => m.SiteId == SiteId && m.DivisionId == DivisionId) on p.JobReceiveHeaderId equals t.JobReceiveHeaderId
                                   where p.ProductUidId == BarCode.ProductUIDId
                                   select p).FirstOrDefault();

                        if (Rec != null)
                        {
                            temp.ErrorMessage += " Is already received.";
                        }

                        var Order = (from p in db.JobOrderLine
                                     join t in db.JobOrderHeader.Where(m => m.SiteId == SiteId && m.DivisionId == DivisionId) on p.JobOrderHeaderId equals t.JobOrderHeaderId
                                     where p.ProductUidHeaderId == BarCode.ProductUidHeaderId
                                     select t).FirstOrDefault();
                        if (Order != null && Order.ProcessId != JobRecHead.ProcessId)
                        {
                            temp.ErrorMessage += " Process do not match.";
                        }


                    }
                    else
                    {
                        temp.ErrorMessage += "Invalid ProductUID";
                    }

                }
                else if (temp.GenPersonId != JobRecHead.JobWorkerId)
                {
                    temp.ErrorType = "InvalidGodown";
                    temp.ErrorMessage = "Product does not belong to this Supplier ";
                }
                else
                {
                    temp.ErrorType = "Success";
                }
                return temp;

            }
            catch (Exception ex)
            {
                UIDValidationViewModel temp = new UIDValidationViewModel();
                temp.ErrorType = "InvalidID";
                temp.ErrorMessage = "Error in validation";

                return temp;
            }


        }

        public UIDValidationViewModel ValidateUIDOnJobReceiveBranch(string ProductUID, bool PostedInStock, int HeaderID)
        {
            var JobRecHead = new JobReceiveHeaderService(_unitOfWork).Find(HeaderID);

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            //var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(JobRecHead.DocTypeId, JobRecHead.DivisionId, JobRecHead.SiteId);

            //int[] ContraDocTypes = { };
            //if (!string.IsNullOrEmpty(settings.filterContraDocTypes))
            //{
            //    ContraDocTypes = settings.filterContraDocTypes.Split(',').Select(Int32.Parse).ToArray();
            //}


            var UidStatus = (from p in db.ProductUid
                             where p.ProductUidName == ProductUID
                             select p.Status).FirstOrDefault();

            int PDDivisionId = (from P in db.ProductUid
                                where P.ProductUidName == ProductUID
                                join PD in db.Product on P.ProductId equals PD.ProductId
                                where PD.DivisionId == DivisionId
                                select PD.DivisionId
                           ).FirstOrDefault();

            UIDValidationViewModel temp = new UIDValidationViewModel();



            if(UidStatus==ProductUidStatusConstants.Issue)
            { 
            temp = (from p in db.ProductUid
                       where p.ProductUidName == ProductUID
                       join t in db.JobOrderHeader.Where(m => m.SiteId == SiteId && m.DivisionId == DivisionId) on new { x = p.LastTransactionDocTypeId.Value, y = p.LastTransactionDocNo } equals new { x = t.DocTypeId, y = t.DocNo } into table
                       from tab in table.DefaultIfEmpty()
                       join t2 in db.JobOrderLine on new { x = p.ProductUIDId, y = tab.JobOrderHeaderId } equals new { x = t2.ProductUidId.Value, y = t2.JobOrderHeaderId } into table2
                       from tab2 in table2.DefaultIfEmpty()
                       group new { p, tab2 } by tab2.JobOrderLineId into g
                       select new UIDValidationViewModel
                       {
                           CurrenctGodownId = g.Max(m => m.p.CurrenctGodownId),
                           CurrenctProcessId = g.Max(m => m.p.CurrenctProcessId),
                           CurrentGodownName = g.Max(m => m.p.CurrenctGodown.GodownName),
                           CurrentProcessName = g.Max(m => m.p.CurrenctProcess.ProcessName),
                           GenDocDate = g.Max(m => m.p.GenDocDate),
                           GenDocId = g.Max(m => m.p.GenDocId),
                           GenDocNo = g.Max(m => m.p.GenDocNo),
                           GenLineId = g.Max(m => m.p.GenLineId),
                           GenDocTypeId = g.Max(m => m.p.GenDocTypeId),
                           GenDocTypeName = g.Max(m => m.p.GenDocType.DocumentTypeName),
                           GenPersonId = g.Max(m => m.p.GenPersonId),
                           GenPersonName = g.Max(m => m.p.GenPerson.Person.Name),
                           LastTransactionDocDate = g.Max(m => m.p.LastTransactionDocDate),
                           LastTransactionDocId = g.Max(m => m.p.LastTransactionDocId),
                           LastTransactionDocNo = g.Max(m => m.p.LastTransactionDocNo),
                           LastTransactionDocTypeId = g.Max(m => m.p.LastTransactionDocTypeId),
                           LastTransactionDocTypeName = g.Max(m => m.p.LastTransactionDocType.DocumentTypeName),
                           LastTransactionPersonId = g.Max(m => m.p.LastTransactionPersonId),
                           LastTransactionPersonName = g.Max(m => m.p.LastTransactionPerson.Name),
                           LastTransactionDocLineId = g.Key,
                           ProductId = g.Max(m => m.p.ProductId),
                           ProductName = g.Max(m => m.p.Product.ProductName),
                           ProductUIDId = g.Max(m => m.p.ProductUIDId),
                           ProductUidName = g.Max(m => m.p.ProductUidName),
                           Status = g.Max(m => m.p.Status),
                           LotNo = g.Max(m => m.p.LotNo),
                           Dimension1Id = g.Max(m => m.p.Dimension1Id),
                           Dimension1Name = g.Max(m => m.p.Dimension1.Dimension1Name),
                           Dimension2Id = g.Max(m => m.p.Dimension2Id),
                           Dimension2Name = g.Max(m => m.p.Dimension2.Dimension2Name),
                           Branch = true
                       }).FirstOrDefault();

            }
            else if (UidStatus==ProductUidStatusConstants.Return)
            {
                temp = (from p in db.ProductUid
                        where p.ProductUidName == ProductUID
                        join t in db.JobReturnHeader.Where(m => m.SiteId == SiteId && m.DivisionId == DivisionId) on new { x = p.LastTransactionDocTypeId.Value, y = p.LastTransactionDocNo } equals new { x = t.DocTypeId, y = t.DocNo } into table
                        from tab in table.DefaultIfEmpty()
                        join jretl in db.JobReturnLine on tab.JobReturnHeaderId equals jretl.JobReturnHeaderId into jrtltable from retline in jrtltable.DefaultIfEmpty()
                        join jrecl in db.JobReceiveLine on new { x = p.ProductUIDId, y = retline.JobReceiveLineId } equals new { x = jrecl.ProductUidId.Value, y = jrecl.JobReceiveLineId } into rectable
                        from recline in rectable.DefaultIfEmpty()
                        join t2 in db.JobOrderLine on new { x = p.ProductUIDId, y = recline.JobOrderLineId ?? 0 } equals new { x = t2.ProductUidId.Value, y = t2.JobOrderLineId } into table2
                        from tab2 in table2.DefaultIfEmpty()
                        join jh in db.JobOrderHeader on tab2.JobOrderHeaderId equals jh.JobOrderHeaderId
                        group new { p, tab2, jh } by tab2.JobOrderLineId into g
                        select new UIDValidationViewModel
                        {
                            CurrenctGodownId = g.Max(m => m.p.CurrenctGodownId),
                            CurrenctProcessId = g.Max(m => m.p.CurrenctProcessId),
                            CurrentGodownName = g.Max(m => m.p.CurrenctGodown.GodownName),
                            CurrentProcessName = g.Max(m => m.p.CurrenctProcess.ProcessName),
                            GenDocDate = g.Max(m => m.p.GenDocDate),
                            GenDocId = g.Max(m => m.p.GenDocId),
                            GenDocNo = g.Max(m => m.p.GenDocNo),
                            GenLineId = g.Max(m => m.p.GenLineId),
                            GenDocTypeId = g.Max(m => m.p.GenDocTypeId),
                            GenDocTypeName = g.Max(m => m.p.GenDocType.DocumentTypeName),
                            GenPersonId = g.Max(m => m.p.GenPersonId),
                            GenPersonName = g.Max(m => m.p.GenPerson.Person.Name),
                            LastTransactionDocDate = g.Max(m => m.p.LastTransactionDocDate),
                            LastTransactionDocId = g.Max(m => m.p.LastTransactionDocId),
                            LastTransactionDocNo = g.Max(m => m.jh.DocNo),
                            LastTransactionDocTypeId = g.Max(m => m.jh.DocTypeId),
                            LastTransactionDocTypeName = g.Max(m => m.jh.DocType.DocumentTypeName),
                            LastTransactionPersonId = g.Max(m => m.p.LastTransactionPersonId),
                            LastTransactionPersonName = g.Max(m => m.p.LastTransactionPerson.Name),
                            LastTransactionDocLineId = g.Key,
                            ProductId = g.Max(m => m.p.ProductId),
                            ProductName = g.Max(m => m.p.Product.ProductName),
                            ProductUIDId = g.Max(m => m.p.ProductUIDId),
                            ProductUidName = g.Max(m => m.p.ProductUidName),
                            Status = g.Max(m => m.p.Status),
                            LotNo = g.Max(m => m.p.LotNo),
                            Dimension1Id = g.Max(m => m.p.Dimension1Id),
                            Dimension1Name = g.Max(m => m.p.Dimension1.Dimension1Name),
                            Dimension2Id = g.Max(m => m.p.Dimension2Id),
                            Dimension2Name = g.Max(m => m.p.Dimension2.Dimension2Name),
                            Branch = true
                        }).FirstOrDefault();

            }

            if (temp == null)
            {
                temp = new UIDValidationViewModel();
                temp.ErrorType = "InvalidID";
                temp.ErrorMessage = "Invalid ProductUID";
            }
            else if (PDDivisionId != DivisionId)
            {
                temp.ErrorType = "InvalidID";
                temp.ErrorMessage = "Barcode does not exist Current Division";
            }
            else if (temp.CurrenctProcessId != JobRecHead.ProcessId)
            {
                temp.ErrorType = "InvalidID";
                temp.ErrorMessage = "Barcode does not belong to this Process ";
            }
            else if (PostedInStock == true)
            {
                if (temp.CurrenctGodownId != null)
                {
                    temp.ErrorType = "GodownNull";
                    temp.ErrorMessage = " Product " + temp.ProductName + " is already in Stock at Godown " + new GodownService(_unitOfWork).Find(temp.CurrenctGodownId ?? 0).GodownName;

                }

                else if (temp.LastTransactionPersonId != JobRecHead.JobWorkerId)
                {
                    temp.ErrorType = "InvalidGodown";
                    temp.ErrorMessage = "Product does not belong to this Supplier ";
                }
                else
                {
                    temp.ErrorType = "Success";
                }
            }
            else
            {
                temp.ErrorType = "Success";
            }


            return temp;
        }

        public UIDValidationViewModel ValidateUID(string ProductUID)
        {

            UIDValidationViewModel temp = new UIDValidationViewModel();
            var UID = (from p in db.ProductUid
                       where p.ProductUidName == ProductUID
                       select new UIDValidationViewModel
                       {
                           ProductUIDId = p.ProductUIDId,
                           ProductUidName = p.ProductUidName,
                       }).FirstOrDefault();

            if (UID == null)
            {
                UID = new UIDValidationViewModel();
                UID.ErrorType = "InvalidID";
                UID.ErrorMessage = "Invalid ProductUID";
            }
            else
            {
                UID.ErrorType = "Success";
            }



            return UID;
        }

        public int NextId(int id, int GenDocTypeId)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProductUid
                        where p.GenDocTypeId == GenDocTypeId
                        orderby p.ProductUidName
                        select p.ProductUIDId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProductUid
                        where p.GenDocTypeId == GenDocTypeId
                        orderby p.ProductUidName
                        select p.ProductUIDId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id, int GenDocTypeId)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.ProductUid
                        where p.GenDocTypeId == GenDocTypeId
                        orderby p.ProductUidName
                        select p.ProductUIDId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProductUid
                        where p.GenDocTypeId == GenDocTypeId
                        orderby p.ProductUidName
                        select p.ProductUIDId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public bool IsProcessDone(int ProductUidId, int ProcessId)
        {
            string ProcessString = "|" + ProcessId.ToString() + "|";

            var temp = (from P in db.ProductUid
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



        public List<ProductUid> GetBCForProductUidHeaderId(int id)
        {
            return (from p in db.ProductUid
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

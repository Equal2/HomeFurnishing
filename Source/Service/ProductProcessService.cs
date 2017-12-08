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
using System.Xml.Linq;
using Model.ViewModel;
using AutoMapper;

namespace Service
{
    public interface IProductProcessService : IDisposable
    {
        ProductProcess Create(ProductProcess pt);
        void Delete(int id);
        void Delete(ProductProcess pt);
        ProductProcess GetProductProcess(int ptId);
        IEnumerable<ProductProcess> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProductProcess pt);
        ProductProcess Add(ProductProcess pt);
        IEnumerable<ProductProcess> GetProductProcessList();
        IEnumerable<ProductProcess> GetProductProcessList(int id);
        ProductProcess Find(int id);
        ProductProcess FindByProductProcess(int ProductId, int? ProcessId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id);

        Task<IEquatable<ProductProcess>> GetAsync();
        Task<ProductProcess> FindAsync(int id);
        IEnumerable<ProductProcess> GetProductProcessIdListByProductId(int ProductId);
        IEnumerable<ProductProcessViewModel> GetMaxProductProcessListForDesign(int Id);//ProductGroupId
       // IQueryable<ProductProcessViewModel> GetProductProcessListForProduct(int id);
        IEnumerable<ProductProcessViewModel> GetProductProcessForIndex(int Id);//ProductId
        bool CheckForProductDimensionExists(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, int ProductProcessId);
        bool CheckForProductDimensionExists(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId);
    }

    public class ProductProcessService : IProductProcessService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProductProcess> _ProductProcessRepository;
        RepositoryQuery<ProductProcess> ProductProcessRepository;
        public ProductProcessService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProductProcessRepository = new Repository<ProductProcess>(db);
            ProductProcessRepository = new RepositoryQuery<ProductProcess>(_ProductProcessRepository);
        }

        public ProductProcess GetProductProcess(int pt)
        {
            return ProductProcessRepository.Include(r => r.Product).Get().Where(i => i.ProductProcessId == pt).FirstOrDefault();
        }

        public ProductProcess Create(ProductProcess pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProductProcess>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProductProcess>().Delete(id);
        }

        public void Delete(ProductProcess pt)
        {
            _unitOfWork.Repository<ProductProcess>().Delete(pt);
        }

        public void Update(ProductProcess pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProductProcess>().Update(pt);
        }

        public IEnumerable<ProductProcess> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProductProcess>()
                .Query()
                //.OrderBy(q => q.OrderBy(c => c.Supplier ))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProductProcess> GetProductProcessList()
        {
            var pt = _unitOfWork.Repository<ProductProcess>().Query().Include(p => p.Product).Get();
            return pt;
        }

        public ProductProcess Add(ProductProcess pt)
        {
            _unitOfWork.Repository<ProductProcess>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProductProcess>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProductProcess> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<ProductProcess> GetProductProcessList(int id)
        {
            var pt = _unitOfWork.Repository<ProductProcess>().Query().Include(m => m.Product).Get().Where(m => m.ProductId == id).ToList();
            return pt;
        }
        public ProductProcess Find(int id)
        {
            return _unitOfWork.Repository<ProductProcess>().Find(id);
        }

        public ProductProcess FindByProductProcess(int ProductId, int? ProcessId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id)
        {
            var pt = _unitOfWork.Repository<ProductProcess>().Query().Get().Where(m => m.ProductId == ProductId && m.ProcessId == ProcessId && m.Dimension1Id == Dimension1Id && m.Dimension2Id == Dimension2Id && m.Dimension3Id == Dimension3Id && m.Dimension4Id == Dimension4Id).FirstOrDefault();
            return pt;
        }

        public IEnumerable<ProductProcess> GetProductProcessIdListByProductId(int ProductId)
        {
            var pt = _unitOfWork.Repository<ProductProcess>().Query().Get().Where(m => m.ProductId == ProductId).ToList();
            return pt;
        }      


        public bool UpdateProductRateGroupForDesign(int ProductGroupId, int ProductRateGroupId, int RateListHeaderId, string User, out XElement Modifications)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            var Products = (from p in db.Product
                            where p.ProductGroupId == ProductGroupId
                            select p).ToList();

            var ProductIds = Products.Select(m => m.ProductId).ToArray();

            var ExistingProducts = (from p in db.ProductProcess
                                    join t in db.RateListHeader on p.ProcessId equals t.ProcessId
                                    where t.RateListHeaderId == RateListHeaderId
                                    && ProductIds.Contains(p.ProductId)
                                    select p).ToList();


            foreach (var item in ExistingProducts)
            {
                ProductProcess ExRec = Mapper.Map<ProductProcess>(item);
                item.ProductRateGroupId = ProductRateGroupId;
                item.ModifiedBy = User;
                item.ModifiedDate = DateTime.Now;

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                    Obj = item,
                });


                item.ObjectState = Model.ObjectState.Modified;
                db.ProductProcess.Add(item);

            }

            Modifications = new ModificationsCheckService().CheckChanges(LogList);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;

        }




        public IEnumerable<ProductProcessViewModel> GetMaxProductProcessListForDesign(int Id)
        {
            var Temp = (from p in db.Product
                        join t in db.ProductProcess on p.ProductId equals t.ProductId into ProductProcessTable from ProductProcessTab in ProductProcessTable.DefaultIfEmpty()
                        where p.ProductGroupId == Id
                        group new { p, ProductProcessTab } by ProductProcessTab.ProductId into g
                        orderby g.Select(m => m.ProductProcessTab).Count() descending
                        select new{
                        
                            List=(from p in g.Select(m=>m.ProductProcessTab)
                                 orderby p.Sr
                                 select new ProductProcessViewModel{
                                     ProcessName=p.Process.ProcessName,
                                     ProcessId=p.ProcessId,
                                     ProductProcessId=p.ProductProcessId,
                                     Sr=p.Sr,
                                     ProductRateGroupId=p.ProductRateGroupId,
                                     ProudctRateGroupName=p.ProductRateGroup.ProductRateGroupName,
                                     Instructions=p.Instructions,
                                 }).ToList(),

                        }).FirstOrDefault();            
            return Temp.List;
        }

        public IEnumerable<ProductProcess> GetProductProcessListForDesign(int id)
        {


            return (from p in db.Product
                    join t in db.ProductProcess on p.ProductId equals t.ProductId
                    where p.ProductGroupId == id
                    select t).ToList();

        }


       
        //public IQueryable<ProductProcessViewModel> GetProductProcessListForProduct(int id)
        //{
           

        //    return (from p in db.ProductProcess
        //            where p.ProductId == id
        //            select new ProductProcessViewModel
        //            {

        //                ProductProcessId=p.ProductProcessId

        //            }
        //           );
        //}

        public bool UpdateProductRateGroupForProduct(int ProductId, int? Dimension1Id, int? Dimension2Id, int ProductRateGroupId, int RateListHeaderId, string User, out XElement Modifications)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            var ExistingProducts = (from p in db.ProductProcess
                                    join t in db.RateListHeader on p.ProcessId equals t.ProcessId
                                    where t.RateListHeaderId == RateListHeaderId
                                    && p.ProductId == ProductId && p.Dimension1Id == Dimension1Id && p.Dimension2Id == Dimension2Id
                                    select p).ToList();


            foreach (var item in ExistingProducts)
            {
                ProductProcess ExRec = Mapper.Map<ProductProcess>(item);
                item.ProductRateGroupId = ProductRateGroupId;
                item.ModifiedBy = User;
                item.ModifiedDate = DateTime.Now;

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                    Obj = item,
                });


                item.ObjectState = Model.ObjectState.Modified;
                db.ProductProcess.Add(item);

            }

            Modifications = new ModificationsCheckService().CheckChanges(LogList);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;

        }

        public IEnumerable<ProductProcessViewModel> GetProductProcessForIndex(int Id)
        {
            var Temp = (from p in db.ProductProcess 
                        where p.ProductId == Id
                        orderby p.Sr
                        select new ProductProcessViewModel
                        {
                            ProductProcessId = p.ProductProcessId,
                            ProcessName = p.Process.ProcessName,
                            ProcessId = p.ProcessId,
                            Sr = p.Sr,
                            ProductRateGroupId = p.ProductRateGroupId,
                            ProudctRateGroupName = p.ProductRateGroup.ProductRateGroupName,
                            QAGroupId = p.QAGroupId,
                            QAGroupName = p.QAGroup.QaGroupName,
                            Instructions = p.Instructions,
                        }).ToList();

            return Temp;
        }

        public bool CheckForProductDimensionExists(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, int ProductProcessId)
        {

            ProductProcess temp = (from p in db.ProductProcess
                              where p.ProductId == ProductId && p.Dimension1Id == Dimension1Id && p.Dimension2Id == Dimension2Id && p.Dimension3Id == Dimension3Id && p.Dimension4Id == Dimension4Id && p.ProcessId == ProcessId && p.ProductProcessId != ProductProcessId
                              select p).FirstOrDefault();
            if (temp != null)
                return true;
            else return false;
        }

        public bool CheckForProductDimensionExists(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId)
        {
            ProductProcess temp = (from p in db.ProductProcess
                              where p.ProductId == ProductId && p.Dimension1Id == Dimension1Id && p.Dimension2Id == Dimension2Id && p.Dimension3Id == Dimension3Id && p.Dimension4Id == Dimension4Id && p.ProcessId == ProcessId 
                              select p).FirstOrDefault();
            if (temp != null)
                return true;
            else return false;
        }

    }
}

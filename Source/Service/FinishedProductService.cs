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

namespace Service
{
    public interface IFinishedProductService : IDisposable
    {
        FinishedProduct Create(FinishedProduct p);
        void Delete(int id);
        void Delete(FinishedProduct p);
        IEnumerable<FinishedProduct> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(FinishedProduct p);
        FinishedProduct Add(FinishedProduct p);
        IQueryable<FinishedProduct> GetFinishedProductList();
        IQueryable<FinishedProduct> GetFinishedProductList(int prodyctTypeId,bool sample);
        Task<IEquatable<FinishedProduct>> GetAsync();
        Task<FinishedProduct> FindAsync(int id);
        FinishedProduct Find(string ProductName);
        FinishedProduct Find(int id);
        FinishedProduct GetFinishedProduct(int id);
        IEnumerable<FinishedProduct> GetAccessoryList();
        int NextId(int id,int tid);
        int PrevId(int id,int tid);
        IEnumerable<FinishedProduct> GetFinishedProductForGroup(int id);
    }

   

    public class FinishedProductService : IFinishedProductService
    {
        ApplicationDbContext db =new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public FinishedProductService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public FinishedProduct Create(FinishedProduct p)
        {
            p.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<FinishedProduct>().Insert(p);
            return p;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<FinishedProduct>().Delete(id);
        }

        public FinishedProduct GetFinishedProduct(int id)
        {
            return (from p in db.FinishedProduct
                    where p.ProductId == id
                    select p
                        ).FirstOrDefault();
        }

        public void Delete(FinishedProduct p)
        {
            _unitOfWork.Repository<FinishedProduct>().Delete(p);
        }

        public void Update(FinishedProduct p)
        {
            p.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<FinishedProduct>().Update(p);
        }

        public IEnumerable<FinishedProduct> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<FinishedProduct>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProductCode))
                .Filter(q => !string.IsNullOrEmpty(q.ProductName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }


        public IQueryable<FinishedProduct> GetFinishedProductList()
        {
            var tes = (from p in db.FinishedProduct
                     orderby p.ProductName
                     select p
                         );
            return tes;
        }

        public IEnumerable<FinishedProduct> GetFinishedProductForGroup(int id)
        {

            return (from p in db.FinishedProduct
                    where p.ProductGroupId == id
                    select p
                        ).ToList();

        }


        public IQueryable<FinishedProduct> GetFinishedProductList(int prodyctTypeId,bool sample)
        {

            return (from p in db.FinishedProduct
                    join t in db.ProductGroups on p.ProductGroupId equals t.ProductGroupId into table
                    from tab in table.DefaultIfEmpty()
                    where tab.ProductTypeId == prodyctTypeId && p.IsSample==sample
                    orderby p.ProductName
                    select p
                        );

        }

        public IEnumerable<FinishedProduct> GetAccessoryList()
        {
            var p = _unitOfWork.Repository<FinishedProduct>().Query().Get();
            return p;
        }



        public FinishedProduct Find(string ProductName)
        {
            
            FinishedProduct p = _unitOfWork.Repository<FinishedProduct>().Query().Get().Where(i => i.ProductName == ProductName).FirstOrDefault();
            
            return p;
        }

        public FinishedProduct Find(int id)
        {
            //return _unitOfWork.Repository<FinishedProduct>().Find(id);
            return db.FinishedProduct.Find(id);
        }

       

        public FinishedProduct Add(FinishedProduct p)
        {              
            _unitOfWork.Repository<FinishedProduct>().Add(p);
            return p;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<FinishedProduct>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<FinishedProduct> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<FinishedProductPrevProcess> FGetFinishedProductPrevProcess(int ProductId, int ProcessId)
        {


            int Sequence = (from L in db.ProcessSequenceLine
                           where L.ProcessSequenceHeaderId == (from P in db.FinishedProduct where P.ProductId == ProductId select new { ProcessSequenceHeaderId = P.ProcessSequenceHeaderId }).FirstOrDefault().ProcessSequenceHeaderId
                           && L.ProcessId == ProcessId
                           select new
                           {
                               Sequence = L.Sequence
                           }).FirstOrDefault().Sequence;


            IQueryable<FinishedProductPrevProcess> FinishedProductprevprocess = (from P in db.FinishedProduct
                                                                 join Ps in db.ProcessSequenceLine on P.ProcessSequenceHeaderId equals Ps.ProcessSequenceHeaderId into ProcessSequenceLineTable
                                                     from ProcessSequenceLineTab in ProcessSequenceLineTable.DefaultIfEmpty()
                                                     join Pr in db.Process on ProcessSequenceLineTab.ProcessId equals Pr.ProcessId into ProcessTable
                                                     from ProcessTab in ProcessTable.DefaultIfEmpty()
                                                     where P.ProductId == ProductId && ProcessSequenceLineTab.Sequence == Sequence
                                                     select new FinishedProductPrevProcess
                                                     {
                                                         ProductId = P.ProductId,
                                                         PrevProcessId = ProcessSequenceLineTab.ProcessId,
                                                         PrevProcessName = ProcessTab.ProcessName
                                                     });
            return FinishedProductprevprocess;
        }


        public int NextId(int id,int tid)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.FinishedProduct
                        join t in db.ProductGroups on p.ProductGroupId equals t.ProductGroupId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        where tab1.ProductTypeId == tid
                        orderby p.ProductName
                        select p.ProductId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.FinishedProduct
                        join t in db.ProductGroups on p.ProductGroupId equals t.ProductGroupId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        where tab1.ProductTypeId == tid
                        orderby p.ProductName
                        select p.ProductId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id,int tid)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.FinishedProduct
                        join t in db.ProductGroups on p.ProductGroupId equals t.ProductGroupId into table1 from tab1 in table1.DefaultIfEmpty()
                        where tab1.ProductTypeId==tid
                        orderby p.ProductName
                        select p.ProductId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.FinishedProduct
                        join t in db.ProductGroups on p.ProductGroupId equals t.ProductGroupId into table1
                        from tab1 in table1.DefaultIfEmpty()
                        where tab1.ProductTypeId == tid
                        orderby p.ProductName
                        select p.ProductId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

    }

    public class FinishedProductPrevProcess
    {
        public int ProductId { get; set; }
        public int PrevProcessId { get; set; }
        public string PrevProcessName { get; set; }
    }



}

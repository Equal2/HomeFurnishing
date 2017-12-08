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
using Model.ViewModel;

namespace Service
{
    public interface ISaleOrderQtyAmendmentLineService : IDisposable
    {
        SaleOrderQtyAmendmentLine Create(SaleOrderQtyAmendmentLine pt);
        void Delete(int id);
        void Delete(SaleOrderQtyAmendmentLine pt);
        SaleOrderQtyAmendmentLine Find(int id);
        IEnumerable<SaleOrderQtyAmendmentLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SaleOrderQtyAmendmentLine pt);
        SaleOrderQtyAmendmentLine Add(SaleOrderQtyAmendmentLine pt);
        IEnumerable<SaleOrderQtyAmendmentLine> GetSaleOrderQtyAmendmentLineList();
        IEnumerable<SaleOrderQtyAmendmentLineViewModel> GetSaleOrderQtyAmendmentLineForHeader(int id);//Header Id
        Task<IEquatable<SaleOrderQtyAmendmentLine>> GetAsync();
        Task<SaleOrderQtyAmendmentLine> FindAsync(int id);
        IEnumerable<SaleOrderQtyAmendmentLineViewModel> GetPurchaseOrderLineForMultiSelect(SaleOrderAmendmentFilterViewModel svm);
        SaleOrderQtyAmendmentLineViewModel GetSaleOrderQtyAmendmentLine(int id);//Line Id
        int NextId(int id);
        int PrevId(int id);
    }

    public class SaleOrderQtyAmendmentLineService : ISaleOrderQtyAmendmentLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SaleOrderQtyAmendmentLine> _SaleOrderQtyAmendmentLineRepository;
        RepositoryQuery<SaleOrderQtyAmendmentLine> SaleOrderQtyAmendmentLineRepository;
        public SaleOrderQtyAmendmentLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SaleOrderQtyAmendmentLineRepository = new Repository<SaleOrderQtyAmendmentLine>(db);
            SaleOrderQtyAmendmentLineRepository = new RepositoryQuery<SaleOrderQtyAmendmentLine>(_SaleOrderQtyAmendmentLineRepository);
        }


        public SaleOrderQtyAmendmentLine Find(int id)
        {
            return _unitOfWork.Repository<SaleOrderQtyAmendmentLine>().Find(id);
        }

        public SaleOrderQtyAmendmentLine Create(SaleOrderQtyAmendmentLine pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SaleOrderQtyAmendmentLine>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SaleOrderQtyAmendmentLine>().Delete(id);
        }

        public void Delete(SaleOrderQtyAmendmentLine pt)
        {
            _unitOfWork.Repository<SaleOrderQtyAmendmentLine>().Delete(pt);
        }

        public void Update(SaleOrderQtyAmendmentLine pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SaleOrderQtyAmendmentLine>().Update(pt);
        }

        public IEnumerable<SaleOrderQtyAmendmentLine> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SaleOrderQtyAmendmentLine>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SaleOrderQtyAmendmentLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public IEnumerable<SaleOrderQtyAmendmentLineViewModel> GetSaleOrderQtyAmendmentLineForHeader(int id)
        {
            return ( from p in db.SaleOrderQtyAmendmentLine
                         where p.SaleOrderAmendmentHeaderId==id
                         orderby p.SaleOrderQtyAmendmentLineId
                     select new SaleOrderQtyAmendmentLineViewModel
                     {
                         UnitId=p.SaleOrderLine.Product.Unit.UnitName,
                         ProductName=p.SaleOrderLine.Product.ProductName,
                         Qty=p.Qty,
                         Remark=p.Remark,
                         SaleOrderAmendmentHeaderId=p.SaleOrderAmendmentHeaderId,
                         SaleOrderDocNo=p.SaleOrderLine.SaleOrderHeader.DocNo,
                         SaleOrderLineId=p.SaleOrderLineId,
                         SaleOrderQtyAmendmentLineId=p.SaleOrderQtyAmendmentLineId,
                     }
                         );

        }

        public IEnumerable<SaleOrderQtyAmendmentLineViewModel> GetPurchaseOrderLineForMultiSelect(SaleOrderAmendmentFilterViewModel svm)
        {
            string[] ProductIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductId)) { ProductIdArr = svm.ProductId.Split(",".ToCharArray()); }
            else { ProductIdArr = new string[] { "NA" }; }

            string[] SaleOrderIdArr = null;
            if (!string.IsNullOrEmpty(svm.SaleOrderHeaderId)) { SaleOrderIdArr = svm.SaleOrderHeaderId.Split(",".ToCharArray()); }
            else { SaleOrderIdArr = new string[] { "NA" }; }

            string[] ProductGroupIdArr = null;
            if (!string.IsNullOrEmpty(svm.ProductGroupId)) { ProductGroupIdArr = svm.ProductGroupId.Split(",".ToCharArray()); }
            else { ProductGroupIdArr = new string[] { "NA" }; }

            var temp = (from p in db.SaleOrderHeader
                        join t1 in db.SaleOrderLine on p.SaleOrderHeaderId equals t1.SaleOrderHeaderId 
                        join product in db.Product on t1.ProductId equals product.ProductId into table2
                        from tab2 in table2.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(svm.ProductId) ? 1 == 1 : ProductIdArr.Contains(t1.ProductId.ToString()))
                            && (svm.BuyerId == 0 ? 1 == 1 : p.SaleToBuyerId == svm.BuyerId)
                        && (string.IsNullOrEmpty(svm.SaleOrderHeaderId) ? 1 == 1 : SaleOrderIdArr.Contains(p.SaleOrderHeaderId.ToString()))
                        && (string.IsNullOrEmpty(svm.ProductGroupId) ? 1 == 1 : ProductGroupIdArr.Contains(tab2.ProductGroupId.ToString()))
                        && t1.Qty > 0 && p.SaleToBuyerId== svm.BuyerId
                        select new SaleOrderQtyAmendmentLineViewModel
                        {
                            SaleOrderDocNo=p.DocNo,
                            CurrentQty=t1.Qty,                            
                            SaleOrderAmendmentHeaderDocNo = p.DocNo,
                            ProductName = tab2.ProductName,
                            ProductId = t1.ProductId,
                            SaleOrderAmendmentHeaderId = svm.SaleOrderAmendmentHeaderId,
                            SaleOrderLineId = t1.SaleOrderLineId
                        }
                        );
            return temp;
        }
        public SaleOrderQtyAmendmentLineViewModel GetSaleOrderQtyAmendmentLine(int id)
        {
            var temp= (from p in db.SaleOrderQtyAmendmentLine
                           where p.SaleOrderQtyAmendmentLineId==id
                       select new SaleOrderQtyAmendmentLineViewModel
                       {
                           ProductId=p.SaleOrderLine.ProductId,
                           Qty=p.Qty,
                           CurrentQty=p.SaleOrderLine.Qty,
                           Remark=p.Remark,
                           SaleOrderAmendmentHeaderDocNo=p.SaleOrderAmendmentHeader.DocNo,
                           SaleOrderAmendmentHeaderId=p.SaleOrderAmendmentHeaderId,
                           SaleOrderDocNo=p.SaleOrderLine.SaleOrderHeader.DocNo,
                           SaleOrderLineId=p.SaleOrderLineId,
                           SaleOrderQtyAmendmentLineId=p.SaleOrderQtyAmendmentLineId,
                           LockReason=p.LockReason,
                       }
                           ).FirstOrDefault();

            return temp;
               
        }

        public IEnumerable<SaleOrderQtyAmendmentLine> GetSaleOrderQtyAmendmentLineList()
        {
            var pt = _unitOfWork.Repository<SaleOrderQtyAmendmentLine>().Query().Get().OrderBy(m => m.SaleOrderQtyAmendmentLineId);

            return pt;
        }

        public SaleOrderQtyAmendmentLine Add(SaleOrderQtyAmendmentLine pt)
        {
            _unitOfWork.Repository<SaleOrderQtyAmendmentLine>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SaleOrderQtyAmendmentLine
                        orderby p.SaleOrderQtyAmendmentLineId
                        select p.SaleOrderQtyAmendmentLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SaleOrderQtyAmendmentLine
                        orderby p.SaleOrderQtyAmendmentLineId
                        select p.SaleOrderQtyAmendmentLineId).FirstOrDefault();
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

                temp = (from p in db.SaleOrderQtyAmendmentLine
                        orderby p.SaleOrderQtyAmendmentLineId
                        select p.SaleOrderQtyAmendmentLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SaleOrderQtyAmendmentLine
                        orderby p.SaleOrderQtyAmendmentLineId
                        select p.SaleOrderQtyAmendmentLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<SaleOrderQtyAmendmentLine>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SaleOrderQtyAmendmentLine> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

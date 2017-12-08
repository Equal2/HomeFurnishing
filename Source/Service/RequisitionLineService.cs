using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Model.ViewModels;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Model.ViewModel;


namespace Service
{
    public interface IRequisitionLineService : IDisposable
    {
        RequisitionLine Create(RequisitionLine s);
        void Delete(int id);
        void Delete(RequisitionLine s);
        RequisitionLineViewModel GetRequisitionLine(int id);
        RequisitionLine Find(int id);
        void Update(RequisitionLine s);
        IQueryable<RequisitionLineViewModel> GetRequisitionLineListForIndex(int RequisitionHeaderId);
        IEnumerable<RequisitionLineViewModel> GetRequisitionLineforDelete(int headerid);
        IEnumerable<RequisitionLineViewModel> GetPendingRequisitionLines(int ProductId, int PersonId);
        RequisitionLineViewModel GetRequsitionLineDetail(int RequistionLineId);
    }

    public class RequisitionLineService : IRequisitionLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public RequisitionLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public RequisitionLine Create(RequisitionLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RequisitionLine>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RequisitionLine>().Delete(id);
        }

        public void Delete(RequisitionLine s)
        {
            _unitOfWork.Repository<RequisitionLine>().Delete(s);
        }

        public void Update(RequisitionLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RequisitionLine>().Update(s);
        }

     
        public RequisitionLineViewModel GetRequisitionLine(int id)
        {
            return (from p in db.RequisitionLine                  
                    where p.RequisitionLineId == id
                    select new RequisitionLineViewModel
                    {
                        ProductId=p.ProductId,
                        DueDate = p.DueDate,                       
                        Qty = p.Qty,          
                        ProcessId=p.ProcessId,
                        UnitName=p.Product.Unit.UnitName,
                        Remark = p.Remark,
                        RequisitionHeaderId = p.RequisitionHeaderId,
                        RequisitionLineId = p.RequisitionLineId,                        
                        ProductName = p.Product.ProductName,
                       Dimension1Id=p.Dimension1Id,
                       Dimension1Name=p.Dimension1.Dimension1Name,
                       Dimension2Name=p.Dimension2.Dimension2Name,
                       Dimension2Id=p.Dimension2Id,
                       Specification=p.Specification,
                       LockReason=p.LockReason,
                    }

                        ).FirstOrDefault();
        }
        public RequisitionLine Find(int id)
        {
            return _unitOfWork.Repository<RequisitionLine>().Find(id);
        }
      

        public IQueryable<RequisitionLineViewModel> GetRequisitionLineListForIndex(int RequisitionHeaderId)
        {
            var temp = from p in db.RequisitionLine
                       join t in db.Dimension1 on p.Dimension1Id equals t.Dimension1Id into table from Dim1 in table.DefaultIfEmpty()
                       join t1 in db.Dimension2 on p.Dimension2Id equals t1.Dimension2Id into table1 from Dim2 in table1.DefaultIfEmpty()                       
                       orderby p.RequisitionLineId
                       where p.RequisitionHeaderId==RequisitionHeaderId
                       select new RequisitionLineViewModel
                       {
                           Specification=p.Specification,
                           Dimension1Name=Dim2.Dimension2Name,
                           Dimension2Name=Dim1.Dimension1Name,
                           Remark=p.Remark,
                           DueDate = p.DueDate,
                           ProductId=p.ProductId,
                           ProductName = p.Product.ProductName,
                           Qty = p.Qty,
                           UnitId=p.Product.UnitId,
                           UnitName=p.Product.Unit.UnitName,
                           ProcessName=p.Process.ProcessName,
                           RequisitionHeaderId = p.RequisitionHeaderId,
                           RequisitionLineId = p.RequisitionLineId
                       };
            return temp;
        }

        public IEnumerable<RequisitionLineViewModel> GetRequisitionLineforDelete(int headerid)
        {
            return (from p in db.RequisitionLine
                    where p.RequisitionHeaderId == headerid
                    select new RequisitionLineViewModel
                    {
                        RequisitionLineId = p.RequisitionLineId
                    }

                        );


        }

        public IEnumerable<RequisitionLineViewModel> GetPendingRequisitionLines(int ProductId, int PersonId)
        {
            List<RequisitionLineViewModel> Test=new List<RequisitionLineViewModel>();


            var tem2 = from p in db.ViewMaterialRequestBalance
                       join t in db.Product on p.ProductId equals t.ProductId into table
                       from prodtab in table.DefaultIfEmpty()
                       join t1 in db.Dimension1 on p.Dimension1Id equals t1.Dimension1Id into table1
                       from dimtab in table1.DefaultIfEmpty()
                       join t2 in db.Dimension2 on p.Dimension2Id equals t2.Dimension2Id into table2
                       from dim2tab in table2.DefaultIfEmpty()
                       where p.ProductId == ProductId && p.PersonId == PersonId && p.BalanceQty>0
                       select new RequisitionLineViewModel
                       {
                           Dimension1Name = dimtab.Dimension1Name,
                           Dimension2Name = dim2tab.Dimension2Name,
                           RequisitionHeaderDocNo=p.MaterialRequestNo,
                           RequisitionLineId=p.RequisitionLineId,
                           RequisitionHeaderId=p.RequisitionHeaderId,
                       };

            return tem2;

        }

        public RequisitionLineViewModel GetRequsitionLineDetail(int RequistionLineId)
        {
            RequisitionLineViewModel temp = new RequisitionLineViewModel();

            return (from b in db.ViewMaterialRequestBalance     
                    join t3 in db.RequisitionLine on b.RequisitionLineId equals t3.RequisitionLineId into table3 from requlinetab in table3.DefaultIfEmpty()
                    join t in db.Dimension1 on b.Dimension1Id equals t.Dimension1Id into table from dimtab in table.DefaultIfEmpty()
                    join t2 in db.Dimension2 on b.Dimension2Id equals t2.Dimension2Id into table2 from dim2tab in table2.DefaultIfEmpty()                    
                    join t4 in db.Product on b.ProductId equals t4.ProductId into table4 from prodtab in table4.DefaultIfEmpty()
                    where b.RequisitionLineId==RequistionLineId

                    select new RequisitionLineViewModel
                    {
                        Qty = b.BalanceQty,                        
                        Specification=requlinetab.Specification,
                        Dimension1Id = dimtab.Dimension1Id,
                        Dimension1Name = dimtab.Dimension1Name,
                        Dimension2Id = dim2tab.Dimension2Id,
                        Dimension2Name = dim2tab.Dimension2Name,                                                
                        UnitId = prodtab.UnitId,
                        ProcessId=requlinetab.ProcessId,
                        ProcessName=requlinetab.Process.ProcessName,
                    }).FirstOrDefault();
            
        }

        public void Dispose()
        {
        }
    }
}

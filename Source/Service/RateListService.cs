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
    public interface IRateListService : IDisposable
    {
        RateList Create(RateList pt);
        void Delete(int id);
        void Delete(RateList s);
        RateList Find(int Id);
        void Update(RateList s);
        IEnumerable<RateList> GetRateList();
        IEnumerable<RateListViewModel> GetRateListForIndex();
        IQueryable<RateListViewModel> GetWeavingRateListForIndex();
        RateListViewModel GetNewRateListForWeaving(int ProductGroupId, int PersonRateGroupId);
        RateListViewModel GetRateListVM(int id);
        int NextId(int id);
        int PrevId(int id);
        RateList GetRateListForDesign(int ProductGroupId);

    }

    public class RateListService : IRateListService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public RateListService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public RateList Find(int Id)
        {
            return _unitOfWork.Repository<RateList>().Find(Id);
        }

        public RateList Create(RateList s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RateList>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RateList>().Delete(id);
        }

        public void Delete(RateList s)
        {
            _unitOfWork.Repository<RateList>().Delete(s);
        }

        public void Update(RateList s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RateList>().Update(s);
        }


        public IEnumerable<RateList> GetRateList()
        {
            var pt = (from p in db.RateList
                      orderby p.RateListId
                      select p);            

            return pt;
        }

        public IEnumerable<RateListViewModel> GetRateListForIndex()
        {
            var pt = (from p in db.RateList
                      orderby p.RateListId
                      select new RateListViewModel
                      { 
                      DealUnitName=p.DealUnit.UnitName,
                      DocTypeName=p.DocType.DocumentTypeName,
                      PersonRateGroupName=p.PersonRateGroup.PersonRateGroupName,
                      ProcessName=p.Process.ProcessName,
                      ProductName=p.Product.ProductName,
                      Rate=p.Rate,
                      RateListId=p.RateListId,
                      WEF=p.WEF,
                      WeightageGreaterOrEqual=p.WeightageGreaterOrEqual,
                      
                      });

            return pt;
        }

        public IQueryable<RateListViewModel> GetWeavingRateListForIndex()
        {
            var ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId.ToString();
            int ProductTypeId = new ProductTypeService(_unitOfWork).Find(ProductTypeConstants.Rug).ProductTypeId;

           //var temp=(from p in db.ProductGroups                     
           //         join t in db.PersonRateGroup
           //         join t2 in db.RateList.Where(m=>m.DocTypeId==643) on new { x=p.ProductGroupId,y=t.PersonRateGroupId} equals new {x=t2.DocId ,y=t2.PersonRateGroupId } into table
           //         from tab in table.DefaultIfEmpty()
           //         select p)

            //var temp = (from p in db.ProductGroups
            //            from t in db.PersonRateGroup
            //            select new { p, t }).ToList();


            //(from p in db.ProductGroups
            // from t in db.PersonRateGroup.Where(m => m.ProcessId == 43)
            // join t2 in db.RateList on new { x= p.ProductGroupId, y=t.PersonRateGroupId } equals new { x=t2.DocId.Value ,y=t2.PersonRateGroupId.Value} into table from tab in table.DefaultIfEmpty()
            // where p.ProductTypeId == 1
            // select new { p.ProductGroupName, t.PersonRateGroupName }).ToList();



           var temp = (from p in db.ProductGroups
             from t in db.PersonRateGroup.Where(m => m.Processes.IndexOf(ProcessId)!=-1)
             join t2 in db.RateList on new { x= p.ProductGroupId, y=t.PersonRateGroupId } equals new { x=t2.DocId.Value ,y=t2.PersonRateGroupId.Value} into table from tab in table.DefaultIfEmpty()
             where p.ProductTypeId == ProductTypeId
             orderby p.ProductGroupName
             select new RateListViewModel { Design= p.ProductGroupName,ProductGroupId=p.ProductGroupId, PersonRateGroupName=t.PersonRateGroupName,PersonRateGroupId=t.PersonRateGroupId, Rate=tab.Rate, WEF=tab.WEF, RateListId=tab.RateListId });

            return (temp);


        }

        public RateListViewModel GetNewRateListForWeaving(int ProductGroupId,int PersonRateGroupId)
        {

            var ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId.ToString();
            int ProductTypeId = new ProductTypeService(_unitOfWork).Find(ProductTypeConstants.Rug).ProductTypeId;

            var temp = (from p in db.ProductGroups
                        from t in db.PersonRateGroup.Where(m => m.Processes.IndexOf(ProcessId)!=-1)
                        join t2 in db.RateList on new { x = p.ProductGroupId, y = t.PersonRateGroupId } equals new { x = t2.DocId.Value, y = t2.PersonRateGroupId.Value } into table
                        from tab in table.DefaultIfEmpty()
                        where p.ProductTypeId == ProductTypeId && p.ProductGroupId==ProductGroupId && t.PersonRateGroupId==PersonRateGroupId
                        orderby p.ProductGroupName
                        select new RateListViewModel { Design = p.ProductGroupName, ProductGroupId = p.ProductGroupId, PersonRateGroupName = t.PersonRateGroupName, PersonRateGroupId = t.PersonRateGroupId, Rate = tab.Rate, WEF = DateTime.Now, RateListId = tab.RateListId,Loss=tab.Loss,UnCountedQty=tab.UnCountedQty }).FirstOrDefault();

            return (temp);


        }

        public RateList GetRateListForDesign(int ProductGroupId)
        {
            return (from p in db.RateList
                    where p.DocId == ProductGroupId
                    select p).FirstOrDefault();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.RateList
                        orderby p.RateListId
                        select p.RateListId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.RateList
                        orderby p.RateListId
                        select p.RateListId).FirstOrDefault();
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

                temp = (from p in db.RateList
                        orderby p.RateListId
                        select p.RateListId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.RateList
                        orderby p.RateListId
                        select p.RateListId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public RateListViewModel GetRateListVM(int id)
        {
            return (from p in db.RateList
                    where p.RateListId == id
                    select new RateListViewModel
                    {
                        DealUnitId = p.DealUnitId,
                        DocId = p.DocId,
                        DocTypeId = p.DocTypeId,
                        PersonRateGroupId = p.PersonRateGroupId,
                        ProcessId = p.ProcessId,
                        ProductGroupId = p.DocId.Value,
                        ProductId = p.ProductId,
                        Rate = p.Rate,
                        RateListId = p.RateListId,
                        WEF = p.WEF,
                        WeightageGreaterOrEqual = p.WeightageGreaterOrEqual,
                        Loss=p.Loss,
                        UnCountedQty=p.UnCountedQty,

                    }).FirstOrDefault();


        }
        public void Dispose()
        {
        }

    }
}

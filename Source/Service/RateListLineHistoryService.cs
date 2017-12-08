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
    public interface IRateListLineHistoryService : IDisposable
    {
        RateListLineHistory Create(RateListLineHistory pt);
        void Delete(int id);
        void Delete(RateListLineHistory s);
        RateListLineHistory Find(int Id);
        void Update(RateListLineHistory s);
        IEnumerable<RateListLineHistory> GetRateListLineHistory();
        IEnumerable<RateListLineHistoryViewModel> GetRateListLineHistoryForIndex();
        //IQueryable<RateListLineHistoryViewModel> GetWeavingRateListLineHistoryForIndex();
        //RateListLineHistoryViewModel GetNewRateListLineHistoryForWeaving(int ProductGroupId, int PersonRateGroupId);
        RateListLineHistoryViewModel GetRateListLineHistoryVM(int id);
        int NextId(int id);
        int PrevId(int id);
        //RateListLineHistory GetRateListLineHistoryForDesign(int ProductGroupId);

    }

    public class RateListLineHistoryService : IRateListLineHistoryService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public RateListLineHistoryService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public RateListLineHistory Find(int Id)
        {
            return _unitOfWork.Repository<RateListLineHistory>().Find(Id);
        }

        public RateListLineHistory Create(RateListLineHistory s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RateListLineHistory>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RateListLineHistory>().Delete(id);
        }

        public void Delete(RateListLineHistory s)
        {
            _unitOfWork.Repository<RateListLineHistory>().Delete(s);
        }

        public void Update(RateListLineHistory s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RateListLineHistory>().Update(s);
        }


        public IEnumerable<RateListLineHistory> GetRateListLineHistory()
        {
            var pt = (from p in db.RateListLineHistory
                      orderby p.RateListLineId
                      select p);            

            return pt;
        }

        public IEnumerable<RateListLineHistoryViewModel> GetRateListLineHistoryForIndex()
        {
            var pt = (from p in db.RateListLineHistory
                      orderby p.RateListLineId
                      select new RateListLineHistoryViewModel
                      {                       
                      PersonRateGroupName=p.PersonRateGroup.PersonRateGroupName,
                      ProductName=p.Product.ProductName,
                      Rate=p.Rate,
                      RateListLineId=p.RateListLineId,                      
                      });

            return pt;
        }

        //public IQueryable<RateListLineHistoryViewModel> GetWeavingRateListLineHistoryForIndex()
        //{
        //    var ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId.ToString();
        //    int ProductTypeId = new ProductTypeService(_unitOfWork).Find(ProductTypeConstants.Rug).ProductTypeId;


        //   var temp = (from p in db.ProductGroups
        //     from t in db.PersonRateGroup.Where(m => m.Processes.IndexOf(ProcessId)!=-1)
        //     join t2 in db.RateListLineHistory on new { x= p.ProductGroupId, y=t.PersonRateGroupId } equals new { x=t2.DocId.Value ,y=t2.PersonRateGroupId.Value} into table from tab in table.DefaultIfEmpty()
        //     where p.ProductTypeId == ProductTypeId
        //     orderby p.ProductGroupName
        //     select new RateListLineHistoryViewModel { Design= p.ProductGroupName,ProductGroupId=p.ProductGroupId, PersonRateGroupName=t.PersonRateGroupName,PersonRateGroupId=t.PersonRateGroupId, Rate=tab.Rate, WEF=tab.WEF, RateListLineHistoryId=tab.RateListLineHistoryId });

        //    return (temp);


        //}

        //public RateListLineHistoryViewModel GetNewRateListLineHistoryForWeaving(int ProductGroupId,int PersonRateGroupId)
        //{

        //    var ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId.ToString();
        //    int ProductTypeId = new ProductTypeService(_unitOfWork).Find(ProductTypeConstants.Rug).ProductTypeId;

        //    var temp = (from p in db.ProductGroups
        //                from t in db.PersonRateGroup.Where(m => m.Processes.IndexOf(ProcessId)!=-1)
        //                join t2 in db.RateListLineHistory on new { x = p.ProductGroupId, y = t.PersonRateGroupId } equals new { x = t2.DocId.Value, y = t2.PersonRateGroupId.Value } into table
        //                from tab in table.DefaultIfEmpty()
        //                where p.ProductTypeId == ProductTypeId && p.ProductGroupId==ProductGroupId && t.PersonRateGroupId==PersonRateGroupId
        //                orderby p.ProductGroupName
        //                select new RateListLineHistoryViewModel { Design = p.ProductGroupName, ProductGroupId = p.ProductGroupId, PersonRateGroupName = t.PersonRateGroupName, PersonRateGroupId = t.PersonRateGroupId, Rate = tab.Rate, WEF = DateTime.Now, RateListLineHistoryId = tab.RateListLineHistoryId,Loss=tab.Loss,UnCountedQty=tab.UnCountedQty }).FirstOrDefault();

        //    return (temp);


        //}

        //public RateListLineHistory GetRateListLineHistoryForDesign(int ProductGroupId)
        //{
        //    return (from p in db.RateListLineHistory
        //            where p.DocId == ProductGroupId
        //            select p).FirstOrDefault();
        //}

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.RateListLineHistory
                        orderby p.RateListLineId
                        select p.RateListLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.RateListLineHistory
                        orderby p.RateListLineId
                        select p.RateListLineId).FirstOrDefault();
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

                temp = (from p in db.RateListLineHistory
                        orderby p.RateListLineId
                        select p.RateListLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.RateListLineHistory
                        orderby p.RateListLineId
                        select p.RateListLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public RateListLineHistoryViewModel GetRateListLineHistoryVM(int id)
        {
            return (from p in db.RateListLineHistory
                    where p.RateListLineId == id
                    select new RateListLineHistoryViewModel
                    {
                       Dimension1Id=p.Dimension1Id,
                       Dimension2Id=p.Dimension2Id,
                       Discount=p.Discount,
                       Incentive=p.Incentive,
                       Loss=p.Loss,
                       PersonRateGroupId=p.PersonRateGroupId,
                       PersonRateGroupName=p.PersonRateGroup.PersonRateGroupName,
                       ProductId=p.ProductId,
                       ProductName=p.Product.ProductName,
                       Rate=p.Rate,
                       RateListHeaderId=p.RateListHeaderId,
                       RateListLineId = p.RateListLineId,
                       RateListName=p.RateListHeader.RateListName,
                       UnCountedQty=p.UnCountedQty,                       

                    }).FirstOrDefault();


        }
        public void Dispose()
        {
        }

    }
}

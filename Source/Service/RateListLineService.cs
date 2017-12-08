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
using AutoMapper;
using System.Xml.Linq;

namespace Service
{
    public interface IRateListLineService : IDisposable
    {
        RateListLine Create(RateListLine pt);
        void Delete(int id);
        void Delete(RateListLine s);
        RateListLine Find(int Id);
        void Update(RateListLine s);
        IEnumerable<RateListLine> GetRateListLine();
        IEnumerable<RateListLineViewModel> GetRateListLineForIndex();
        //IQueryable<RateListLineViewModel> GetWeavingRateListLineForIndex();
        //RateListLineViewModel GetNewRateListLineForWeaving(int ProductGroupId, int PersonRateGroupId);
        RateListLineViewModel GetRateListLineVM(int id);
        int NextId(int id);
        int PrevId(int id);
        bool UpdateRateList(int LineId, string Type, decimal Value);
        //RateListLine GetRateListLineForDesign(int ProductGroupId);
        bool UpdateRateListLineForProduct(int ProductId, decimal Rate, int RateListHeaderId, string User, out XElement Modifications);
        bool UpdateRateListLineForProduct(int ProductId, decimal Rate, decimal Loss, decimal Weight, int RateListHeaderId, string User, out XElement Modifications);

    }

    public class RateListLineService : IRateListLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public RateListLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public RateListLine Find(int Id)
        {
            return _unitOfWork.Repository<RateListLine>().Find(Id);
        }

        public RateListLine Create(RateListLine s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RateListLine>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RateListLine>().Delete(id);
        }

        public void Delete(RateListLine s)
        {
            _unitOfWork.Repository<RateListLine>().Delete(s);
        }

        public void Update(RateListLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RateListLine>().Update(s);
        }


        public IEnumerable<RateListLine> GetRateListLine()
        {
            var pt = (from p in db.RateListLine
                      orderby p.RateListLineId
                      select p);

            return pt;
        }

        public IEnumerable<RateListLineViewModel> GetRateListLineForIndex()
        {
            var pt = (from p in db.RateListLine
                      orderby p.RateListLineId
                      select new RateListLineViewModel
                      {
                          PersonRateGroupName = p.PersonRateGroup.PersonRateGroupName,
                          ProductName = p.Product.ProductName,
                          Rate = p.Rate,
                          RateListLineId = p.RateListLineId,
                      });

            return pt;
        }

        //public IQueryable<RateListLineViewModel> GetWeavingRateListLineForIndex()
        //{
        //    var ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId.ToString();
        //    int ProductTypeId = new ProductTypeService(_unitOfWork).Find(ProductTypeConstants.Rug).ProductTypeId;


        //   var temp = (from p in db.ProductGroups
        //     from t in db.PersonRateGroup.Where(m => m.Processes.IndexOf(ProcessId)!=-1)
        //     join t2 in db.RateListLine on new { x= p.ProductGroupId, y=t.PersonRateGroupId } equals new { x=t2.DocId.Value ,y=t2.PersonRateGroupId.Value} into table from tab in table.DefaultIfEmpty()
        //     where p.ProductTypeId == ProductTypeId
        //     orderby p.ProductGroupName
        //     select new RateListLineViewModel { Design= p.ProductGroupName,ProductGroupId=p.ProductGroupId, PersonRateGroupName=t.PersonRateGroupName,PersonRateGroupId=t.PersonRateGroupId, Rate=tab.Rate, WEF=tab.WEF, RateListLineId=tab.RateListLineId });

        //    return (temp);


        //}

        //public RateListLineViewModel GetNewRateListLineForWeaving(int ProductGroupId,int PersonRateGroupId)
        //{

        //    var ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId.ToString();
        //    int ProductTypeId = new ProductTypeService(_unitOfWork).Find(ProductTypeConstants.Rug).ProductTypeId;

        //    var temp = (from p in db.ProductGroups
        //                from t in db.PersonRateGroup.Where(m => m.Processes.IndexOf(ProcessId)!=-1)
        //                join t2 in db.RateListLine on new { x = p.ProductGroupId, y = t.PersonRateGroupId } equals new { x = t2.DocId.Value, y = t2.PersonRateGroupId.Value } into table
        //                from tab in table.DefaultIfEmpty()
        //                where p.ProductTypeId == ProductTypeId && p.ProductGroupId==ProductGroupId && t.PersonRateGroupId==PersonRateGroupId
        //                orderby p.ProductGroupName
        //                select new RateListLineViewModel { Design = p.ProductGroupName, ProductGroupId = p.ProductGroupId, PersonRateGroupName = t.PersonRateGroupName, PersonRateGroupId = t.PersonRateGroupId, Rate = tab.Rate, WEF = DateTime.Now, RateListLineId = tab.RateListLineId,Loss=tab.Loss,UnCountedQty=tab.UnCountedQty }).FirstOrDefault();

        //    return (temp);


        //}

        //public RateListLine GetRateListLineForDesign(int ProductGroupId)
        //{
        //    return (from p in db.RateListLine
        //            where p.DocId == ProductGroupId
        //            select p).FirstOrDefault();
        //}

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.RateListLine
                        orderby p.RateListLineId
                        select p.RateListLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.RateListLine
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

                temp = (from p in db.RateListLine
                        orderby p.RateListLineId
                        select p.RateListLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.RateListLine
                        orderby p.RateListLineId
                        select p.RateListLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public RateListLineViewModel GetRateListLineVM(int id)
        {
            return (from p in db.RateListLine
                    where p.RateListLineId == id
                    select new RateListLineViewModel
                    {
                        Dimension1Id = p.Dimension1Id,
                        Dimension2Id = p.Dimension2Id,
                        Discount = p.Discount,
                        Incentive = p.Incentive,
                        Loss = p.Loss,
                        PersonRateGroupId = p.PersonRateGroupId,
                        PersonRateGroupName = p.PersonRateGroup.PersonRateGroupName,
                        ProductId = p.ProductId,
                        ProductName = p.Product.ProductName,
                        Rate = p.Rate,
                        RateListHeaderId = p.RateListHeaderId,
                        RateListLineId = p.RateListLineId,
                        RateListName = p.RateListHeader.RateListName,
                        UnCountedQty = p.UnCountedQty,

                    }).FirstOrDefault();


        }

        public bool UpdateRateList(int LineId, string Type, decimal Value)
        {

            var Line = (from p in db.RateListLine
                        where p.RateListLineId == LineId
                        select p).FirstOrDefault();

            if (Type.Contains("Rate"))
                Line.Rate = Value;
            else if (Type.Contains("Incentive"))
                Line.Incentive = Value;
            else if (Type.Contains("Discount"))
                Line.Discount = Value;

            Line.ObjectState = Model.ObjectState.Modified;

            db.RateListLine.Add(Line);

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

        public bool UpdateRateListLineForDesign(int ProductGroupId, decimal Rate, decimal Incentive, decimal Discount, decimal Loss, int RateListHeaderId, string User, out XElement Modifications)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            var Products = (from p in db.Product
                            where p.ProductGroupId == ProductGroupId
                            select p).ToList();

            var ProductIds = Products.Select(m => m.ProductId).ToArray();

            var ExistingProducts = (from p in db.RateListLine
                                    where p.RateListHeaderId == RateListHeaderId
                                    && ProductIds.Contains(p.ProductId.Value)
                                    select p).ToList();

            var PendingToCreate = (from p in ProductIds
                                   join t in ExistingProducts on p equals t.ProductId
                                   into table
                                   from tab in table.DefaultIfEmpty()
                                   where tab == null
                                   select p).ToList();

            foreach (var item in PendingToCreate)
            {
                RateListLine Rll = new RateListLine();
                Rll.ProductId = item;
                Rll.Rate = Rate;
                Rll.Incentive = Incentive;
                Rll.Discount = Discount;
                Rll.Loss = Loss;
                Rll.RateListHeaderId = RateListHeaderId;
                Rll.ModifiedBy = User;
                Rll.CreatedBy = User;
                Rll.ModifiedDate = DateTime.Now;
                Rll.CreatedDate = DateTime.Now;

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Rll,
                });


                Rll.ObjectState = Model.ObjectState.Added;
                db.RateListLine.Add(Rll);

            }

            foreach (var item in ExistingProducts)
            {
                RateListLine ExRec = Mapper.Map<RateListLine>(item);
                item.Rate = Rate;
                item.Incentive = Incentive;
                item.Discount = Discount;
                item.Loss= Loss;
                item.ModifiedBy = User;
                item.ModifiedDate = DateTime.Now;

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                    Obj = item,
                });


                item.ObjectState = Model.ObjectState.Modified;
                db.RateListLine.Add(item);



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


        public bool UpdateRateListLineForDesign(int ProductGroupId, decimal Rate, decimal Loss, decimal Weight, int RateListHeaderId, string User, out XElement Modifications)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            var Products = (from p in db.Product
                            where p.ProductGroupId == ProductGroupId
                            select p).ToList();

            var ProductIds = Products.Select(m => m.ProductId).ToArray();

            var ExistingProducts = (from p in db.RateListLine
                                    where p.RateListHeaderId == RateListHeaderId
                                    && ProductIds.Contains(p.ProductId.Value)
                                    select p).ToList();

            var PendingToCreate = (from p in ProductIds
                                   join t in ExistingProducts on p equals t.ProductId
                                   into table
                                   from tab in table.DefaultIfEmpty()
                                   where tab == null
                                   select p).ToList();

            foreach (var item in PendingToCreate)
            {
                RateListLine Rll = new RateListLine();
                Rll.ProductId = item;
                Rll.Rate = Rate;
                Rll.Loss = Loss;
                Rll.UnCountedQty = Weight;
                Rll.RateListHeaderId = RateListHeaderId;
                Rll.ModifiedBy = User;
                Rll.CreatedBy = User;
                Rll.ModifiedDate = DateTime.Now;
                Rll.CreatedDate = DateTime.Now;
                Rll.ObjectState = Model.ObjectState.Added;
                db.RateListLine.Add(Rll);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Rll,
                });


            }

            foreach (var item in ExistingProducts)
            {
                RateListLine ExRec = Mapper.Map<RateListLine>(item);
                item.Rate = Rate;
                item.Loss = Loss;
                item.UnCountedQty = Weight;
                item.ModifiedBy = User;
                item.ModifiedDate = DateTime.Now;
                item.ObjectState = Model.ObjectState.Modified;
                db.RateListLine.Add(item);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                    Obj = item,
                });


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


        public bool UpdateRateListLineForProduct(int ProductId, decimal Rate, int RateListHeaderId, string User, out XElement Modifications)
        {
            Modifications = null;
            if (ProductId == 0)
                return false;

            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            var ExistingProducts = (from p in db.RateListLine
                                    where p.RateListHeaderId == RateListHeaderId
                                    && p.ProductId == ProductId
                                    select p).FirstOrDefault();


            if (ExistingProducts == null)
            {
                RateListLine Rll = new RateListLine();
                Rll.ProductId = ProductId;
                Rll.Rate = Rate;
                Rll.RateListHeaderId = RateListHeaderId;
                Rll.ModifiedBy = User;
                Rll.CreatedBy = User;
                Rll.ModifiedDate = DateTime.Now;
                Rll.CreatedDate = DateTime.Now;
                Rll.ObjectState = Model.ObjectState.Added;
                db.RateListLine.Add(Rll);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Rll,
                });

            }
            else
            {
                RateListLine ExRec = Mapper.Map<RateListLine>(ExistingProducts);
                ExistingProducts.Rate = Rate;
                ExistingProducts.ModifiedBy = User;
                ExistingProducts.ModifiedDate = DateTime.Now;
                ExistingProducts.ObjectState = Model.ObjectState.Modified;
                db.RateListLine.Add(ExistingProducts);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                    Obj = ExistingProducts,
                });


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


        public bool UpdateRateListLineForProduct(int ProductId, decimal Rate, decimal Loss, decimal Weight, int RateListHeaderId, string User, out XElement Modifications)
        {
            Modifications = null;
            if (ProductId == 0)
                return false;

            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            var ExistingProducts = (from p in db.RateListLine
                                    where p.RateListHeaderId == RateListHeaderId
                                    && p.ProductId == ProductId
                                    select p).FirstOrDefault();


            if (ExistingProducts == null)
            {
                RateListLine Rll = new RateListLine();
                Rll.ProductId = ProductId;
                Rll.Rate = Rate;
                Rll.Loss = Loss;
                Rll.UnCountedQty = Weight;
                Rll.RateListHeaderId = RateListHeaderId;
                Rll.ModifiedBy = User;
                Rll.CreatedBy = User;
                Rll.ModifiedDate = DateTime.Now;
                Rll.CreatedDate = DateTime.Now;
                Rll.ObjectState = Model.ObjectState.Added;
                db.RateListLine.Add(Rll);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Rll,
                });

            }
            else
            {
                RateListLine ExRec = Mapper.Map<RateListLine>(ExistingProducts);
                ExistingProducts.Rate = Rate;
                ExistingProducts.Loss = Loss;
                ExistingProducts.UnCountedQty = Weight;
                ExistingProducts.ModifiedBy = User;
                ExistingProducts.ModifiedDate = DateTime.Now;
                ExistingProducts.ObjectState = Model.ObjectState.Modified;
                db.RateListLine.Add(ExistingProducts);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                    Obj = ExistingProducts,
                });


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

        public RateListLine GetRateListForDesign(int ProductGroupId, int ProcessId)
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from prod in db.Product
                    where prod.ProductGroupId == ProductGroupId
                    let ProdId = prod.ProductId
                    from p in db.RateListLine
                    join t in db.RateListHeader on p.RateListHeaderId equals t.RateListHeaderId
                    where p.ProductId == ProdId && t.ProcessId == ProcessId
                    && t.SiteId == SiteId && t.DivisionId == DivisionId
                    select p).FirstOrDefault();
        }


        public void Dispose()
        {
        }

    }
}

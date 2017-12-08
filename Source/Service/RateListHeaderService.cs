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
    public interface IRateListHeaderService : IDisposable
    {
        RateListHeader Create(RateListHeader pt);
        void Delete(int id);
        void Delete(RateListHeader pt);
        RateListHeader Find(string Name);
        RateListHeader Find(int id);
        void Update(RateListHeader pt);
        RateListHeader Add(RateListHeader pt);
        IQueryable<RateListHeaderViewModel> GetRateListHeaderList();
        RateListHeader GetRateListHeaderByName(string terms);
        int NextId(int id);
        int PrevId(int id);
        RateListHeaderViewModel GetRateListHeaderViewModel(int id);
    }

    public class RateListHeaderService : IRateListHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<RateListHeader> _RateListHeaderRepository;
        RepositoryQuery<RateListHeader> RateListHeaderRepository;

        public RateListHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _RateListHeaderRepository = new Repository<RateListHeader>(db);
            RateListHeaderRepository = new RepositoryQuery<RateListHeader>(_RateListHeaderRepository);
        }

        public RateListHeader GetRateListHeaderByName(string terms)
        {
            return (from p in db.RateListHeader
                    where p.RateListName == terms
                    select p).FirstOrDefault();
        }

        public RateListHeader Find(string Name)
        {
            return RateListHeaderRepository.Get().Where(i => i.RateListName == Name).FirstOrDefault();
        }


        public RateListHeader Find(int id)
        {
            return _unitOfWork.Repository<RateListHeader>().Find(id);
        }

        public RateListHeader Create(RateListHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RateListHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RateListHeader>().Delete(id);
        }

        public void Delete(RateListHeader pt)
        {
            _unitOfWork.Repository<RateListHeader>().Delete(pt);
        }

        public void Update(RateListHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RateListHeader>().Update(pt);
        }

        public IQueryable<RateListHeaderViewModel> GetRateListHeaderList()
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var pt = (from p in db.RateListHeader
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      orderby p.RateListName
                      select new RateListHeaderViewModel { 
                      
                          CalculateWeightageOn=p.CalculateWeightageOn,
                          CloseDate=p.CloseDate,
                          DealUnitId=p.DealUnitId,
                          DealUnitName=p.DealUnit.UnitName,
                          Description=p.Description,
                          DivisionId=p.DivisionId,
                          EffectiveDate=p.EffectiveDate,
                          MaxRate=p.MaxRate,
                          MinRate=p.MinRate,
                          ProcessId=p.ProcessId,
                          ProcessName=p.Process.ProcessName,
                          RateListHeaderId=p.RateListHeaderId,
                          RateListName=p.RateListName,
                          SiteId=p.SiteId,
                          Status=p.Status,
                          WeightageGreaterOrEqual=p.WeightageGreaterOrEqual,
                      
                      });

            return pt;
        }

        public IQueryable<RateListHeaderViewModel> GetDesignRateListHeader(int MenuId)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var ProcessIds = (from p in db.ProcessSettings
                              where p.RateListMenuId == MenuId
                              && p.SiteId == SiteId && p.DivisionId == DivisionId
                              select p.ProcessId).ToArray();

            var pt = (from p in db.RateListHeader
                      where p.SiteId == SiteId && p.DivisionId == DivisionId&& ProcessIds.Contains(p.ProcessId)
                      orderby p.RateListName
                      select new RateListHeaderViewModel
                      {

                          CalculateWeightageOn = p.CalculateWeightageOn,
                          CloseDate = p.CloseDate,
                          DealUnitId = p.DealUnitId,
                          DealUnitName = p.DealUnit.UnitName,
                          Description = p.Description,
                          DivisionId = p.DivisionId,
                          EffectiveDate = p.EffectiveDate,
                          MaxRate = p.MaxRate,
                          MinRate = p.MinRate,
                          ProcessId = p.ProcessId,
                          ProcessName = p.Process.ProcessName,
                          RateListHeaderId = p.RateListHeaderId,
                          RateListName = p.RateListName,
                          SiteId = p.SiteId,
                          Status = p.Status,
                          WeightageGreaterOrEqual = p.WeightageGreaterOrEqual,

                      });

            return pt;
        }

        public IQueryable<RateListHeaderViewModel> GetProductRateListHeader(int MenuId)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var ProcessIds = (from p in db.ProcessSettings
                              where p.RateListMenuId == MenuId
                              && p.SiteId == SiteId && p.DivisionId == DivisionId
                              select p.ProcessId).ToArray();

            var pt = (from p in db.RateListHeader
                      where p.SiteId == SiteId && p.DivisionId == DivisionId && ProcessIds.Contains(p.ProcessId)
                      orderby p.RateListName
                      select new RateListHeaderViewModel
                      {

                          CalculateWeightageOn = p.CalculateWeightageOn,
                          CloseDate = p.CloseDate,
                          DealUnitId = p.DealUnitId,
                          DealUnitName = p.DealUnit.UnitName,
                          Description = p.Description,
                          DivisionId = p.DivisionId,
                          EffectiveDate = p.EffectiveDate,
                          MaxRate = p.MaxRate,
                          MinRate = p.MinRate,
                          ProcessId = p.ProcessId,
                          ProcessName = p.Process.ProcessName,
                          RateListHeaderId = p.RateListHeaderId,
                          RateListName = p.RateListName,
                          SiteId = p.SiteId,
                          Status = p.Status,
                          WeightageGreaterOrEqual = p.WeightageGreaterOrEqual,

                      });

            return pt;
        }

        public IQueryable<RateListHeaderViewModel> GetProductRateGroupHeader(int MenuId)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var ProcessIds = (from p in db.ProcessSettings
                              where p.RateListMenuId == MenuId
                              && p.SiteId == SiteId && p.DivisionId == DivisionId
                              select p.ProcessId).ToArray();

            var pt = (from p in db.RateListHeader
                      where p.SiteId == SiteId && p.DivisionId == DivisionId && ProcessIds.Contains(p.ProcessId)
                      orderby p.RateListName
                      select new RateListHeaderViewModel
                      {

                          CalculateWeightageOn = p.CalculateWeightageOn,
                          CloseDate = p.CloseDate,
                          DealUnitId = p.DealUnitId,
                          DealUnitName = p.DealUnit.UnitName,
                          Description = p.Description,
                          DivisionId = p.DivisionId,
                          EffectiveDate = p.EffectiveDate,
                          MaxRate = p.MaxRate,
                          MinRate = p.MinRate,
                          ProcessId = p.ProcessId,
                          ProcessName = p.Process.ProcessName,
                          RateListHeaderId = p.RateListHeaderId,
                          RateListName = p.RateListName,
                          SiteId = p.SiteId,
                          Status = p.Status,
                          WeightageGreaterOrEqual = p.WeightageGreaterOrEqual,

                      });

            return pt;
        }

        public RateListHeader Add(RateListHeader pt)
        {
            _unitOfWork.Repository<RateListHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.RateListHeader
                        orderby p.RateListName
                        select p.RateListHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.RateListHeader
                        orderby p.RateListName
                        select p.RateListHeaderId).FirstOrDefault();
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

                temp = (from p in db.RateListHeader
                        orderby p.RateListName
                        select p.RateListHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.RateListHeader
                        orderby p.RateListName
                        select p.RateListHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public RateListHeaderViewModel GetRateListHeaderViewModel(int id)
        {

            var temp=(from p in db.RateListHeader
                    where p.RateListHeaderId == id
                    select new RateListHeaderViewModel
                    {
                        CalculateWeightageOn = p.CalculateWeightageOn,
                        CloseDate = p.CloseDate,
                        DealUnitId = p.DealUnitId,
                        Description = p.Description,
                        DivisionId = p.DivisionId,
                        EffectiveDate = p.EffectiveDate,
                        MaxRate = p.MaxRate,
                        MinRate = p.MinRate,
                        ProcessId = p.ProcessId,
                        RateListHeaderId = p.RateListHeaderId,
                        RateListName = p.RateListName,
                        SiteId = p.SiteId,
                        Status = p.Status,
                        WeightageGreaterOrEqual = p.WeightageGreaterOrEqual,

                    }).FirstOrDefault();

            var PersonRategroups = string.Join(",",(from p in db.RateListPersonRateGroup
                                    where p.RateListHeaderId == id
                                    select p.PersonRateGroupId).ToList());

            var ProductRategroups = string.Join(",", (from p in db.RateListProductRateGroup
                                                     where p.RateListHeaderId == id
                                                     select p.ProductRateGroupId).ToList());

            temp.PersonRateGroup = PersonRategroups;
            temp.ProductRateGroup = ProductRategroups;

            return temp;
        
        }


        public void Dispose()
        {
        }

    }
}

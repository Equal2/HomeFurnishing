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

namespace Service
{
    public interface IRateListProductRateGroupService : IDisposable
    {
        RateListProductRateGroup Create(RateListProductRateGroup pt);
        void Delete(int id);
        void Delete(RateListProductRateGroup pt);
        RateListProductRateGroup Find(int id);
        void Update(RateListProductRateGroup pt);
        RateListProductRateGroup Add(RateListProductRateGroup pt);
        IQueryable<RateListProductRateGroup> GetRateListProductRateGroupList(int Id);
    }

    public class RateListProductRateGroupService : IRateListProductRateGroupService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<RateListProductRateGroup> _RateListProductRateGroupRepository;
        RepositoryQuery<RateListProductRateGroup> RateListProductRateGroupRepository;

        public RateListProductRateGroupService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _RateListProductRateGroupRepository = new Repository<RateListProductRateGroup>(db);
            RateListProductRateGroupRepository = new RepositoryQuery<RateListProductRateGroup>(_RateListProductRateGroupRepository);
        }


        public RateListProductRateGroup Find(int id)
        {
            return _unitOfWork.Repository<RateListProductRateGroup>().Find(id);
        }

        public RateListProductRateGroup Create(RateListProductRateGroup pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RateListProductRateGroup>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RateListProductRateGroup>().Delete(id);
        }

        public void Delete(RateListProductRateGroup pt)
        {
            _unitOfWork.Repository<RateListProductRateGroup>().Delete(pt);
        }

        public void Update(RateListProductRateGroup pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RateListProductRateGroup>().Update(pt);
        }

        public IQueryable<RateListProductRateGroup> GetRateListProductRateGroupList(int Id)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var pt = _unitOfWork.Repository<RateListProductRateGroup>().Query().Get().Where(m=>m.RateListHeaderId==Id).OrderBy(m => m.RateListProductRateGroupId);

            return pt;
        }

        public RateListProductRateGroup Add(RateListProductRateGroup pt)
        {
            _unitOfWork.Repository<RateListProductRateGroup>().Insert(pt);
            return pt;
        }     
        public void Dispose()
        {
        }

    }
}

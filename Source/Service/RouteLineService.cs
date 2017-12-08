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


namespace Service
{
    public interface IRouteLineService : IDisposable
    {
        RouteLine Create(RouteLine s);
        void Delete(int id);
        void Delete(RouteLine s);
        RouteLine GetRouteLine(int id);
        RouteLine Find(int id);
        void Update(RouteLine s);
        IQueryable<RouteLineViewModel> GetRouteLineListForIndex(int RouteHeaderId);
    }

    public class RouteLineService : IRouteLineService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public RouteLineService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public RouteLine Create(RouteLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RouteLine>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RouteLine>().Delete(id);
        }

        public void Delete(RouteLine s)
        {
            _unitOfWork.Repository<RouteLine>().Delete(s);
        }

        public void Update(RouteLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RouteLine>().Update(s);
        }

        public RouteLine GetRouteLine(int id)
        {
            return _unitOfWork.Repository<RouteLine>().Query().Get().Where(m => m.RouteLineId == id).FirstOrDefault();
        }

        
        public RouteLine Find(int id)
        {
            return _unitOfWork.Repository<RouteLine>().Find(id);
        }

        public IQueryable<RouteLineViewModel> GetRouteLineListForIndex(int RouteId)
        {
            var temp = from L in db.RouteLine
                       join c in db.City on L.CityId equals c.CityId into CityTable
                       from CityTab in CityTable.DefaultIfEmpty()
                       where L.RouteId == RouteId
                       select new RouteLineViewModel
                       {
                           RouteLineId = L.RouteLineId,
                           RouteId = L.RouteId,
                           CityId = L.CityId,
                           CityName = CityTab.CityName,
                           CreatedBy = L.CreatedBy,
                           CreatedDate = L.CreatedDate,
                           ModifiedBy = L.ModifiedBy,
                           ModifiedDate = L.ModifiedDate
                       };
            return temp;
        }

        public void Dispose()
        {
        }
    }
}

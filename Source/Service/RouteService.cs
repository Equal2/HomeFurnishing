using Data.Infrastructure;
using Data.Models;
using Model;
using Model.Models;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Common;

namespace Service
{
    public interface IRouteService : IDisposable
    {
        Route Create(Route s);
        void Delete(int id);
        void Delete(Route s);
        IQueryable<Route> GetRouteList();
        Route Find(int id);
        Route Find(string RouteName);
        
        void Update(Route s);
        int NextId(int id);
        int PrevId(int id);
    }
    public class RouteService : IRouteService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public RouteService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

     public Route Create(Route s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Route>().Insert(s);
            return s;
        }

       public void Delete(int id)
     {
         _unitOfWork.Repository<Route>().Delete(id);
     }
       public void Delete(Route s)
        {
            _unitOfWork.Repository<Route>().Delete(s);
        }
       public void Update(Route s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Route>().Update(s);            
        }

       public int NextId(int id)
       {
           int temp = 0;
           if (id != 0)
           {
               temp = (from p in db.Route
                       orderby p.RouteName 
                       select p.RouteId).AsEnumerable().SkipWhile(p=>p!=id).Skip(1).FirstOrDefault();
           }
           else
           {
               temp = (from p in db.Route
                       orderby p.RouteName 
                       select p.RouteId).FirstOrDefault();
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

               temp = (from p in db.Route
                       orderby p.RouteName 
                       select p.RouteId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
           }
           else
           {
               temp = (from p in db.Route
                       orderby p.RouteName 
                       select p.RouteId).AsEnumerable().LastOrDefault();
           }
           if (temp != 0)
               return temp;
           else
               return id;
       }

       public Route Find(int id)
        {
            return _unitOfWork.Repository<Route>().Find(id);
        }

       public Route Find(string RouteName)
       {
           return _unitOfWork.Repository<Route>().Query().Get().Where(m => m.RouteName == RouteName).FirstOrDefault();
       }

       public IQueryable<Route> GetRouteList()
        {
            var temp = from p in db.Route
                       orderby p.RouteName 
                       select p;
            return temp;                             
        }

        public void Dispose()
        {
        }
    }
}

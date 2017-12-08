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
    public interface ISiteService : IDisposable
    {
        Site Create(Site pt);
        void Delete(int id);
        void Delete(Site s);
        Site Find(int Id);
        void Update(Site s);
        IEnumerable<Site> GetSiteList();
        IEnumerable<Site> GetSiteList(string RoleId);
        int NextId(int id);
        int PrevId(int id);
        Site FindByPerson(int id);

    }

    public class SiteService : ISiteService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public SiteService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Site Find(int id)
        {
            return _unitOfWork.Repository<Site>().Query().Include(m => m.City).Get().Where(m => m.SiteId == id).FirstOrDefault();     

        }

        public Site Create(Site s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Site>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Site>().Delete(id);
        }

        public void Delete(Site s)
        {
            _unitOfWork.Repository<Site>().Delete(s);
        }

        public void Update(Site s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Site>().Update(s);
        }


        public IEnumerable<Site> GetSiteList()
        {
            var pt = (from p in db.Site
                      orderby p.SiteName
                      select p);            

            return pt;
        }
        public IEnumerable<Site> GetSiteList(string RoleIds)
        {
            //var pt = (from p in db.Site
            //          join t in db.RolesSite on p.SiteId equals t.SiteId
            //          orderby p.SiteName
            //          where RoleIds.Contains(t.RoleId)                      
            //          select p);


            var temp = (from p in db.Site
                        join t in db.RolesSite on p.SiteId equals t.SiteId
                        where RoleIds.Contains(t.RoleId)
                        group p by p.SiteId into g
                        select g.FirstOrDefault()
                          );

            return temp;
        }
        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Site
                        orderby p.SiteName
                        select p.SiteId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Site
                        orderby p.SiteName
                        select p.SiteId).FirstOrDefault();
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

                temp = (from p in db.Site
                        orderby p.SiteName
                        select p.SiteId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Site
                        orderby p.SiteName
                        select p.SiteId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public Site FindByPerson(int id)
        {
            return (from p in db.Site
                    where p.PersonId == id
                    select p).FirstOrDefault();
        }

        public Site FindBySiteName(string SiteName)
        {
            return (from p in db.Site
                    where p.SiteName == SiteName
                    select p).FirstOrDefault();
        }

        public Site FindBySiteCode(string SiteCode)
        {
            return (from p in db.Site
                    where p.SiteCode == SiteCode
                    select p).FirstOrDefault();
        }

        public void Dispose()
        {
        }

    }
}

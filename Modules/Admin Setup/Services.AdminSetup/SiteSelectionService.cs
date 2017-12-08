using System;
using System.Linq;
using Infrastructure.IO;
using Models.Company.Models;
using System.Collections.Generic;
using AdminSetup.Models.Models;
using Data;
using Notifier.Models;

namespace Service
{
    public interface ISiteSelectionService : IDisposable
    {
        Site GetSite(int Id);
        Division GetDivision(int Id);
        IEnumerable<Site> GetSiteList();
        IEnumerable<Site> GetSiteList(string RoleId);
        IEnumerable<Division> GetDivisionList();
        IEnumerable<Division> GetDivisionList(string RoleIds);
        int GetNotificationCount(string UserName);
        IEnumerable<Godown> GetGodownList(int SiteId);
    }

    public class SiteSelectionService : ISiteSelectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SiteSelectionService(IUnitOfWork uow)
        {
            _unitOfWork = uow;
        }

        public Site GetSite(int Id)
        {
            var Site = (_unitOfWork.Repository<Site>().Query().Get().Where(m => m.SiteId == Id).FirstOrDefault());
            var City = _unitOfWork.Repository<City>().Query().Get().Where(m => m.CityId == Site.CityId).FirstOrDefault();

            Site.City = City;

            return Site;
        }

        public Division GetDivision(int Id)
        {
            return _unitOfWork.Repository<Division>().Find(Id);
        }

        public IEnumerable<Site> GetSiteList()
        {
            var pt = (from p in _unitOfWork.Repository<Site>().Instance
                      orderby p.SiteId
                      select p);

            return pt;
        }
        public IEnumerable<Site> GetSiteList(string RoleIds)
        {
            var temp = (from p in _unitOfWork.Repository<Site>().Instance
                        join t in _unitOfWork.Repository<RolesSite>().Instance on p.SiteId equals t.SiteId
                        where RoleIds.Contains(t.RoleId)
                        group p by p.SiteId into g
                        select g.FirstOrDefault()
                          );

            return temp;
        }

        public IEnumerable<Division> GetDivisionList()
        {
            var pt = _unitOfWork.Repository<Division>().Query().Get().OrderBy(m => m.DivisionId);

            return pt;
        }
        public IEnumerable<Division> GetDivisionList(string RoleIds)
        {
            var temp = from p in _unitOfWork.Repository<Division>().Instance
                       join t in _unitOfWork.Repository<RolesDivision>().Instance on p.DivisionId equals t.DivisionId
                       where RoleIds.Contains(t.RoleId)
                       group p by p.DivisionId into g
                       select g.FirstOrDefault();

            return temp;
        }

        public int GetNotificationCount(string UserName)
        {
            var Today = DateTime.Now.Date;

            using (LoginApplicationDbContext ldb = new LoginApplicationDbContext())
            {
                var NotificationCount = (from p in ldb.NotificationUser
                                         join t in ldb.Notification on p.NotificationId equals t.NotificationId
                                         where p.UserName == UserName && t.ExpiryDate >= Today && t.SeenDate == null && t.ReadDate == null
                                         select p).Count();

                return NotificationCount;
            }

        }

        public IEnumerable<Godown> GetGodownList(int SiteId)
        {
            return (from p in _unitOfWork.Repository<Godown>().Instance
                    where p.SiteId == SiteId && p.IsActive == true
                    orderby p.GodownName
                    select p).ToList();
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}

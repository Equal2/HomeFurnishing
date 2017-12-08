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
    public interface IPackingSettingService : IDisposable
    {
        PackingSetting Create(PackingSetting pt);
        void Delete(int id);
        void Delete(PackingSetting pt);
        PackingSetting Find(int id);
        IEnumerable<PackingSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PackingSetting pt);
        PackingSetting Add(PackingSetting pt);
        PackingSetting GetPackingSettingForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<PackingSettingsViewModel> GetPackingSettingList();
        Task<IEquatable<PackingSetting>> GetAsync();
        Task<PackingSetting> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class PackingSettingService : IPackingSettingService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PackingSetting> _PackingSettingRepository;
        RepositoryQuery<PackingSetting> PackingSettingRepository;
        public PackingSettingService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PackingSettingRepository = new Repository<PackingSetting>(db);
            PackingSettingRepository = new RepositoryQuery<PackingSetting>(_PackingSettingRepository);
        }

        public PackingSetting Find(int id)
        {
            return _unitOfWork.Repository<PackingSetting>().Find(id);
        }

        public PackingSetting GetPackingSettingForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.PackingSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public PackingSetting Create(PackingSetting pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PackingSetting>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PackingSetting>().Delete(id);
        }

        public void Delete(PackingSetting pt)
        {
            _unitOfWork.Repository<PackingSetting>().Delete(pt);
        }

        public void Update(PackingSetting pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PackingSetting>().Update(pt);
        }

        public IEnumerable<PackingSetting> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PackingSetting>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PackingSettingId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PackingSettingsViewModel> GetPackingSettingList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.PackingSettings
                      orderby p.PackingSettingId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new PackingSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          PackingSettingId=p.PackingSettingId,
                      }
                          ).ToList();

            return pt;
        }

        public PackingSetting Add(PackingSetting pt)
        {
            _unitOfWork.Repository<PackingSetting>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PackingSettings
                        orderby p.PackingSettingId
                        select p.PackingSettingId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PackingSettings
                        orderby p.PackingSettingId
                        select p.PackingSettingId).FirstOrDefault();
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

                temp = (from p in db.PackingSettings
                        orderby p.PackingSettingId
                        select p.PackingSettingId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PackingSettings
                        orderby p.PackingSettingId
                        select p.PackingSettingId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PackingSetting>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PackingSetting> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

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
    public interface ISiteDivisionSettingsService : IDisposable
    {
        SiteDivisionSettings Create(SiteDivisionSettings pt);
        void Delete(int id);
        void Delete(SiteDivisionSettings pt);
        SiteDivisionSettings Find(int id);
        IEnumerable<SiteDivisionSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(SiteDivisionSettings pt);
        SiteDivisionSettings Add(SiteDivisionSettings pt);
        SiteDivisionSettings GetSiteDivisionSettings(int SiteId, int DivisionId, DateTime DocDate);
        Task<IEquatable<SiteDivisionSettings>> GetAsync();
        Task<SiteDivisionSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
    }

    public class SiteDivisionSettingsService : ISiteDivisionSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<SiteDivisionSettings> _SiteDivisionSettingsRepository;
        RepositoryQuery<SiteDivisionSettings> SiteDivisionSettingsRepository;
        public SiteDivisionSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _SiteDivisionSettingsRepository = new Repository<SiteDivisionSettings>(db);
            SiteDivisionSettingsRepository = new RepositoryQuery<SiteDivisionSettings>(_SiteDivisionSettingsRepository);
        }

        public SiteDivisionSettings Find(int id)
        {
            return _unitOfWork.Repository<SiteDivisionSettings>().Find(id);
        }

        public SiteDivisionSettings GetSiteDivisionSettings(int SiteId, int DivisionId, DateTime DocDate)
        {
            return (from S in db.SiteDivisionSettings
                    where DocDate >= S.StartDate && DocDate <= (S.EndDate ?? DateTime.Now)
                    select S).FirstOrDefault();
        }
        public SiteDivisionSettings Create(SiteDivisionSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SiteDivisionSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SiteDivisionSettings>().Delete(id);
        }

        public void Delete(SiteDivisionSettings pt)
        {
            _unitOfWork.Repository<SiteDivisionSettings>().Delete(pt);
        }

        public void Update(SiteDivisionSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SiteDivisionSettings>().Update(pt);
        }

        public IEnumerable<SiteDivisionSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<SiteDivisionSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.SiteDivisionSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public SiteDivisionSettings Add(SiteDivisionSettings pt)
        {
            _unitOfWork.Repository<SiteDivisionSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.SiteDivisionSettings
                        orderby p.SiteDivisionSettingsId
                        select p.SiteDivisionSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.SiteDivisionSettings
                        orderby p.SiteDivisionSettingsId
                        select p.SiteDivisionSettingsId).FirstOrDefault();
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

                temp = (from p in db.SiteDivisionSettings
                        orderby p.SiteDivisionSettingsId
                        select p.SiteDivisionSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.SiteDivisionSettings
                        orderby p.SiteDivisionSettingsId
                        select p.SiteDivisionSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }



        public void Dispose()
        {
        }


        public Task<IEquatable<SiteDivisionSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SiteDivisionSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

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
    public interface IMaterialReceiveSettingsService : IDisposable
    {
        MaterialReceiveSettings Create(MaterialReceiveSettings pt);
        void Delete(int id);
        void Delete(MaterialReceiveSettings pt);
        MaterialReceiveSettings Find(int id);
        IEnumerable<MaterialReceiveSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialReceiveSettings pt);
        MaterialReceiveSettings Add(MaterialReceiveSettings pt);
        MaterialReceiveSettings GetMaterialReceiveSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<MaterialReceiveSettingsViewModel> GetMaterialReceiveSettingsList();
        Task<IEquatable<MaterialReceiveSettings>> GetAsync();
        Task<MaterialReceiveSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class MaterialReceiveSettingsService : IMaterialReceiveSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialReceiveSettings> _MaterialReceiveSettingsRepository;
        RepositoryQuery<MaterialReceiveSettings> MaterialReceiveSettingsRepository;
        public MaterialReceiveSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialReceiveSettingsRepository = new Repository<MaterialReceiveSettings>(db);
            MaterialReceiveSettingsRepository = new RepositoryQuery<MaterialReceiveSettings>(_MaterialReceiveSettingsRepository);
        }

        public MaterialReceiveSettings Find(int id)
        {
            return _unitOfWork.Repository<MaterialReceiveSettings>().Find(id);
        }
        public MaterialReceiveSettings GetMaterialReceiveSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.MaterialReceiveSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public MaterialReceiveSettings Create(MaterialReceiveSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialReceiveSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialReceiveSettings>().Delete(id);
        }

        public void Delete(MaterialReceiveSettings pt)
        {
            _unitOfWork.Repository<MaterialReceiveSettings>().Delete(pt);
        }

        public void Update(MaterialReceiveSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialReceiveSettings>().Update(pt);
        }

        public IEnumerable<MaterialReceiveSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialReceiveSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.MaterialReceiveSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<MaterialReceiveSettingsViewModel> GetMaterialReceiveSettingsList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.MaterialReceiveSettings
                      orderby p.MaterialReceiveSettingsId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new MaterialReceiveSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          MaterialReceiveSettingsId=p.MaterialReceiveSettingsId,
                      }
                          ).ToList();

            return pt;
        }

        public MaterialReceiveSettings Add(MaterialReceiveSettings pt)
        {
            _unitOfWork.Repository<MaterialReceiveSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialReceiveSettings
                        orderby p.MaterialReceiveSettingsId
                        select p.MaterialReceiveSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialReceiveSettings
                        orderby p.MaterialReceiveSettingsId
                        select p.MaterialReceiveSettingsId).FirstOrDefault();
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

                temp = (from p in db.MaterialReceiveSettings
                        orderby p.MaterialReceiveSettingsId
                        select p.MaterialReceiveSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialReceiveSettings
                        orderby p.MaterialReceiveSettingsId
                        select p.MaterialReceiveSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<MaterialReceiveSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialReceiveSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

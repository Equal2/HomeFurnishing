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
    public interface IMaterialRequestSettingsService : IDisposable
    {
        MaterialRequestSettings Create(MaterialRequestSettings pt);
        void Delete(int id);
        void Delete(MaterialRequestSettings pt);
        MaterialRequestSettings Find(int id);
        IEnumerable<MaterialRequestSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialRequestSettings pt);
        MaterialRequestSettings Add(MaterialRequestSettings pt);
        MaterialRequestSettings GetMaterialRequestSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<MaterialRequestSettingsViewModel> GetMaterialRequestSettingsList();
        Task<IEquatable<MaterialRequestSettings>> GetAsync();
        Task<MaterialRequestSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class MaterialRequestSettingsService : IMaterialRequestSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialRequestSettings> _MaterialRequestSettingsRepository;
        RepositoryQuery<MaterialRequestSettings> MaterialRequestSettingsRepository;
        public MaterialRequestSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialRequestSettingsRepository = new Repository<MaterialRequestSettings>(db);
            MaterialRequestSettingsRepository = new RepositoryQuery<MaterialRequestSettings>(_MaterialRequestSettingsRepository);
        }

        public MaterialRequestSettings Find(int id)
        {
            return _unitOfWork.Repository<MaterialRequestSettings>().Find(id);
        }
        public MaterialRequestSettings GetMaterialRequestSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.MaterialRequestSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public MaterialRequestSettings Create(MaterialRequestSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialRequestSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialRequestSettings>().Delete(id);
        }

        public void Delete(MaterialRequestSettings pt)
        {
            _unitOfWork.Repository<MaterialRequestSettings>().Delete(pt);
        }

        public void Update(MaterialRequestSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialRequestSettings>().Update(pt);
        }

        public IEnumerable<MaterialRequestSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialRequestSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.MaterialRequestSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<MaterialRequestSettingsViewModel> GetMaterialRequestSettingsList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.MaterialRequestSettings
                      orderby p.MaterialRequestSettingsId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new MaterialRequestSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          MaterialRequestSettingsId=p.MaterialRequestSettingsId,
                      }
                          ).ToList();

            return pt;
        }

        public MaterialRequestSettings Add(MaterialRequestSettings pt)
        {
            _unitOfWork.Repository<MaterialRequestSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialRequestSettings
                        orderby p.MaterialRequestSettingsId
                        select p.MaterialRequestSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialRequestSettings
                        orderby p.MaterialRequestSettingsId
                        select p.MaterialRequestSettingsId).FirstOrDefault();
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

                temp = (from p in db.MaterialRequestSettings
                        orderby p.MaterialRequestSettingsId
                        select p.MaterialRequestSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialRequestSettings
                        orderby p.MaterialRequestSettingsId
                        select p.MaterialRequestSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<MaterialRequestSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialRequestSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

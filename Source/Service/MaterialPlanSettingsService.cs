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
    public interface IMaterialPlanSettingsService : IDisposable
    {
        MaterialPlanSettings Create(MaterialPlanSettings pt);
        void Delete(int id);
        void Delete(MaterialPlanSettings pt);
        MaterialPlanSettings Find(int id);
        IEnumerable<MaterialPlanSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(MaterialPlanSettings pt);
        MaterialPlanSettings Add(MaterialPlanSettings pt);
        MaterialPlanSettings GetMaterialPlanSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<MaterialPlanSettingsViewModel> GetMaterialPlanSettingsList();
        Task<IEquatable<MaterialPlanSettings>> GetAsync();
        Task<MaterialPlanSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
        string GetBomProcedureForDocType(int DocTypeId);
        MaterialPlanSettings GetMaterialPlanSettings(int SiteId, int? DivisionId);
    }

    public class MaterialPlanSettingsService : IMaterialPlanSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<MaterialPlanSettings> _MaterialPlanSettingsRepository;
        RepositoryQuery<MaterialPlanSettings> MaterialPlanSettingsRepository;
        public MaterialPlanSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _MaterialPlanSettingsRepository = new Repository<MaterialPlanSettings>(db);
            MaterialPlanSettingsRepository = new RepositoryQuery<MaterialPlanSettings>(_MaterialPlanSettingsRepository);
        }

        public MaterialPlanSettings GetMaterialPlanSettings(int SiteId, int? DivisionId)
        {
            return _unitOfWork.Repository<MaterialPlanSettings>().Query().Get().Where(m => m.DivisionId == DivisionId && m.SiteId == SiteId).FirstOrDefault();
        }
        public MaterialPlanSettings Find(int id)
        {
            return _unitOfWork.Repository<MaterialPlanSettings>().Find(id);
        }
        public MaterialPlanSettings GetMaterialPlanSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.MaterialPlanSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public MaterialPlanSettings Create(MaterialPlanSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<MaterialPlanSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<MaterialPlanSettings>().Delete(id);
        }

        public void Delete(MaterialPlanSettings pt)
        {
            _unitOfWork.Repository<MaterialPlanSettings>().Delete(pt);
        }

        public void Update(MaterialPlanSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<MaterialPlanSettings>().Update(pt);
        }

        public IEnumerable<MaterialPlanSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<MaterialPlanSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.MaterialPlanSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<MaterialPlanSettingsViewModel> GetMaterialPlanSettingsList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.MaterialPlanSettings
                      orderby p.MaterialPlanSettingsId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new MaterialPlanSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          MaterialPlanSettingsId=p.MaterialPlanSettingsId,
                      }
                          ).ToList();

            return pt;
        }

        public MaterialPlanSettings Add(MaterialPlanSettings pt)
        {
            _unitOfWork.Repository<MaterialPlanSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.MaterialPlanSettings
                        orderby p.MaterialPlanSettingsId
                        select p.MaterialPlanSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanSettings
                        orderby p.MaterialPlanSettingsId
                        select p.MaterialPlanSettingsId).FirstOrDefault();
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

                temp = (from p in db.MaterialPlanSettings
                        orderby p.MaterialPlanSettingsId
                        select p.MaterialPlanSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.MaterialPlanSettings
                        orderby p.MaterialPlanSettingsId
                        select p.MaterialPlanSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public string GetBomProcedureForDocType(int DocTypeId)
        {
             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from p in db.MaterialPlanSettings
                    where p.DivisionId == DivisionId && p.SiteId == SiteId && p.DocTypeId == DocTypeId
                    select p.SqlProcConsumption
                        ).FirstOrDefault();


        }

        public void Dispose()
        {
        }


        public Task<IEquatable<MaterialPlanSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MaterialPlanSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

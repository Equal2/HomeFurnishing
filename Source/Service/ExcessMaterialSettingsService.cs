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
    public interface IExcessMaterialSettingsService : IDisposable
    {
        ExcessMaterialSettings Create(ExcessMaterialSettings pt);
        void Delete(int id);
        void Delete(ExcessMaterialSettings pt);
        ExcessMaterialSettings Find(int id);
        IEnumerable<ExcessMaterialSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ExcessMaterialSettings pt);
        ExcessMaterialSettings Add(ExcessMaterialSettings pt);
        ExcessMaterialSettings GetExcessMaterialSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<ExcessMaterialSettingsViewModel> GetExcessMaterialSettingsList();
        Task<IEquatable<ExcessMaterialSettings>> GetAsync();
        Task<ExcessMaterialSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);        
    }

    public class ExcessMaterialSettingsService : IExcessMaterialSettingsService
    {
        ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;

        public ExcessMaterialSettingsService(IUnitOfWorkForService unitOfWork,ApplicationDbContext Context)
        {
            _unitOfWork = unitOfWork;
            db = Context;
        }

        public ExcessMaterialSettings Find(int id)
        {
            return _unitOfWork.Repository<ExcessMaterialSettings>().Find(id);
        }
        public ExcessMaterialSettings GetExcessMaterialSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.ExcessMaterialSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public ExcessMaterialSettings Create(ExcessMaterialSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ExcessMaterialSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ExcessMaterialSettings>().Delete(id);
        }

        public void Delete(ExcessMaterialSettings pt)
        {
            _unitOfWork.Repository<ExcessMaterialSettings>().Delete(pt);
        }

        public void Update(ExcessMaterialSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ExcessMaterialSettings>().Update(pt);
        }

        public IEnumerable<ExcessMaterialSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ExcessMaterialSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ExcessMaterialSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ExcessMaterialSettingsViewModel> GetExcessMaterialSettingsList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.ExcessMaterialSettings
                      orderby p.ExcessMaterialSettingsId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new ExcessMaterialSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          ExcessMaterialSettingsId=p.ExcessMaterialSettingsId,
                      }
                          ).ToList();

            return pt;
        }

        public ExcessMaterialSettings Add(ExcessMaterialSettings pt)
        {
            _unitOfWork.Repository<ExcessMaterialSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ExcessMaterialSettings
                        orderby p.ExcessMaterialSettingsId
                        select p.ExcessMaterialSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ExcessMaterialSettings
                        orderby p.ExcessMaterialSettingsId
                        select p.ExcessMaterialSettingsId).FirstOrDefault();
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

                temp = (from p in db.ExcessMaterialSettings
                        orderby p.ExcessMaterialSettingsId
                        select p.ExcessMaterialSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ExcessMaterialSettings
                        orderby p.ExcessMaterialSettingsId
                        select p.ExcessMaterialSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public void Dispose()
        {
        }


        public Task<IEquatable<ExcessMaterialSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ExcessMaterialSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

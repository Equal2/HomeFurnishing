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
    public interface IProdOrderSettingsService : IDisposable
    {
        ProdOrderSettings Create(ProdOrderSettings pt);
        void Delete(int id);
        void Delete(ProdOrderSettings pt);
        ProdOrderSettings Find(int id);
        IEnumerable<ProdOrderSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ProdOrderSettings pt);
        ProdOrderSettings Add(ProdOrderSettings pt);
        ProdOrderSettings GetProdOrderSettingsForDocument(int DocTypeId,int DivisionId,int SiteId);
        IEnumerable<ProdOrderSettingsViewModel> GetProdOrderSettingsList();
        Task<IEquatable<ProdOrderSettings>> GetAsync();
        Task<ProdOrderSettings> FindAsync(int id);        
        int NextId(int id);
        int PrevId(int id);
        ProdOrderSettings GetProdOrderSettings(int SiteId, int? DivisionId);
    }

    public class ProdOrderSettingsService : IProdOrderSettingsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ProdOrderSettings> _ProdOrderSettingsRepository;
        RepositoryQuery<ProdOrderSettings> ProdOrderSettingsRepository;
        public ProdOrderSettingsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ProdOrderSettingsRepository = new Repository<ProdOrderSettings>(db);
            ProdOrderSettingsRepository = new RepositoryQuery<ProdOrderSettings>(_ProdOrderSettingsRepository);
        }

        public ProdOrderSettings GetProdOrderSettings(int SiteId, int? DivisionId)
        {
            return _unitOfWork.Repository<ProdOrderSettings>().Query().Get().Where(m => m.DivisionId == DivisionId && m.SiteId == SiteId).FirstOrDefault();
        }
        public ProdOrderSettings Find(int id)
        {
            return _unitOfWork.Repository<ProdOrderSettings>().Find(id);
        }
        public ProdOrderSettings GetProdOrderSettingsForDocument(int DocTypeId,int DivisionId,int SiteId)
        {
            return (from p in db.ProdOrderSettings
                    where p.DocTypeId == DocTypeId && p.DivisionId == DivisionId && p.SiteId == SiteId
                    select p
                        ).FirstOrDefault();
        }
        public ProdOrderSettings Create(ProdOrderSettings pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ProdOrderSettings>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ProdOrderSettings>().Delete(id);
        }

        public void Delete(ProdOrderSettings pt)
        {
            _unitOfWork.Repository<ProdOrderSettings>().Delete(pt);
        }

        public void Update(ProdOrderSettings pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ProdOrderSettings>().Update(pt);
        }

        public IEnumerable<ProdOrderSettings> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ProdOrderSettings>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ProdOrderSettingsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<ProdOrderSettingsViewModel> GetProdOrderSettingsList()
        {

             var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var pt = (from p in db.ProdOrderSettings
                      orderby p.ProdOrderSettingsId
                      where p.SiteId == SiteId && p.DivisionId == DivisionId
                      select new ProdOrderSettingsViewModel
                      {
                          DocTypeName=p.DocType.DocumentTypeName,
                          DivisionName=p.Division.DivisionName,
                          SiteName=p.Site.SiteName,
                          ProdOrderSettingsId=p.ProdOrderSettingsId,
                      }
                          ).ToList();

            return pt;
        }

        public ProdOrderSettings Add(ProdOrderSettings pt)
        {
            _unitOfWork.Repository<ProdOrderSettings>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ProdOrderSettings
                        orderby p.ProdOrderSettingsId
                        select p.ProdOrderSettingsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ProdOrderSettings
                        orderby p.ProdOrderSettingsId
                        select p.ProdOrderSettingsId).FirstOrDefault();
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

                temp = (from p in db.ProdOrderSettings
                        orderby p.ProdOrderSettingsId
                        select p.ProdOrderSettingsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ProdOrderSettings
                        orderby p.ProdOrderSettingsId
                        select p.ProdOrderSettingsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<ProdOrderSettings>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProdOrderSettings> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

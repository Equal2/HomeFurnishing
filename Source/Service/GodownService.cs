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
using Model.ViewModels;

namespace Service
{
    public interface IGodownService : IDisposable
    {
        Godown Create(Godown pt);
        void Delete(int id);
        void Delete(Godown pt);
        Godown Find(string Name);
        Godown Find(int id);
        IEnumerable<Godown> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Godown pt);
        Godown Add(Godown pt);
        IEnumerable<Godown> GetGodownList(int SiteId);
        IQueryable<ComboBoxResult> GetGodownForContraSiteFilters(int id, string term);
        // IEnumerable<Godown> GetGodownList(int buyerId);
        Task<IEquatable<Godown>> GetAsync();
        Task<Godown> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class GodownService : IGodownService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Godown> _GodownRepository;
        RepositoryQuery<Godown> GodownRepository;
        public GodownService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _GodownRepository = new Repository<Godown>(db);
            GodownRepository = new RepositoryQuery<Godown>(_GodownRepository);
        }

        public Godown Find(string Name)
        {
            return GodownRepository.Get().Where(i => i.GodownName == Name).FirstOrDefault();
        }


        public Godown Find(int id)
        {
            return _unitOfWork.Repository<Godown>().Find(id);
        }

        public Godown Create(Godown pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Godown>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Godown>().Delete(id);
        }

        public void Delete(Godown pt)
        {
            _unitOfWork.Repository<Godown>().Delete(pt);
        }

        public void Update(Godown pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Godown>().Update(pt);
        }

        public IEnumerable<Godown> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Godown>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.GodownName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Godown> GetGodownList(int SiteId)
        {
            var pt = _unitOfWork.Repository<Godown>().Query().Get().OrderBy(m => m.GodownName).Where(m => m.SiteId == SiteId);

            return pt;
        }

        public IQueryable<ComboBoxResult> GetGodownForContraSiteFilters(int id, string term)
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, DivisionId, SiteId);

            string[] ContraSites = null;
            if (!string.IsNullOrEmpty(settings.filterContraSites)) { ContraSites = settings.filterContraSites.Split(",".ToCharArray()); }
            else { ContraSites = new string[] { "NA" }; }

            return (from p in db.Godown 
                    where p.IsActive == true && (string.IsNullOrEmpty(term) ? 1 == 1 : p.GodownName.ToLower().Contains(term.ToLower()))
                    && (string.IsNullOrEmpty(settings.filterContraSites) ? p.SiteId == SiteId : ContraSites.Contains(p.SiteId.ToString()))
                    group p by p.GodownId into g
                    orderby g.Max(m => m.GodownName)
                    select new ComboBoxResult
                    {
                        text = g.Max(m => m.GodownName),
                        id = g.Key.ToString(),
                    });

        }

        public Godown Add(Godown pt)
        {
            _unitOfWork.Repository<Godown>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Godown
                        orderby p.GodownName
                        select p.GodownId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Godown
                        orderby p.GodownName
                        select p.GodownId).FirstOrDefault();
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

                temp = (from p in db.Godown
                        orderby p.GodownName
                        select p.GodownId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Godown
                        orderby p.GodownName
                        select p.GodownId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<Godown>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Godown> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

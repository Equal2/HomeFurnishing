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
    public interface IDrawBackTariffHeadService : IDisposable
    {
        DrawBackTariffHead Create(DrawBackTariffHead pt);
        void Delete(int id);
        void Delete(DrawBackTariffHead pt);
        IEnumerable<DrawBackTariffHead> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(DrawBackTariffHead pt);
        DrawBackTariffHead Add(DrawBackTariffHead pt);
        IEnumerable<DrawBackTariffHead> GetDrawBackTariffHeadList();

        // IEnumerable<DrawBackTariffHead> GetDrawBackTariffHeadList(int buyerId);
        Task<IEquatable<DrawBackTariffHead>> GetAsync();
        Task<DrawBackTariffHead> FindAsync(int id);
        DrawBackTariffHead Find(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class DrawBackTariffHeadService : IDrawBackTariffHeadService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<DrawBackTariffHead> _DrawBackTariffHeadRepository;
        RepositoryQuery<DrawBackTariffHead> DrawBackTariffHeadRepository;
        public DrawBackTariffHeadService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _DrawBackTariffHeadRepository = new Repository<DrawBackTariffHead>(db);
            DrawBackTariffHeadRepository = new RepositoryQuery<DrawBackTariffHead>(_DrawBackTariffHeadRepository);
        }

        public DrawBackTariffHead Find(int id)
        {
            return _unitOfWork.Repository<DrawBackTariffHead>().Find(id);
        }

        public DrawBackTariffHead Create(DrawBackTariffHead pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<DrawBackTariffHead>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<DrawBackTariffHead>().Delete(id);
        }

        public void Delete(DrawBackTariffHead pt)
        {
            _unitOfWork.Repository<DrawBackTariffHead>().Delete(pt);
        }

        public void Update(DrawBackTariffHead pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<DrawBackTariffHead>().Update(pt);
        }

        public IEnumerable<DrawBackTariffHead> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<DrawBackTariffHead>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DrawBackTariffHeadName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<DrawBackTariffHead> GetDrawBackTariffHeadList()
        {
            var pt = _unitOfWork.Repository<DrawBackTariffHead>().Query().Get().OrderBy(M=>M.DrawBackTariffHeadName).ToList();

            return pt;
        }

        public DrawBackTariffHead Add(DrawBackTariffHead pt)
        {
            _unitOfWork.Repository<DrawBackTariffHead>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.DrawBackTariffHead
                        orderby p.DrawBackTariffHeadName
                        select p.DrawBackTariffHeadId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.DrawBackTariffHead
                        orderby p.DrawBackTariffHeadName
                        select p.DrawBackTariffHeadId).FirstOrDefault();
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

                temp = (from p in db.DrawBackTariffHead
                        orderby p.DrawBackTariffHeadName
                        select p.DrawBackTariffHeadId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.DrawBackTariffHead
                        orderby p.DrawBackTariffHeadName
                        select p.DrawBackTariffHeadId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<DrawBackTariffHead>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DrawBackTariffHead> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

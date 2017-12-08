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
    public interface IPromoCodeService : IDisposable
    {
        PromoCode Create(PromoCode pt);
        void Delete(int id);
        void Delete(PromoCode pt);
        PromoCode Find(string Name);
        PromoCode Find(int id);
        IEnumerable<PromoCode> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PromoCode pt);
        PromoCode Add(PromoCode pt);
        IQueryable<PromoCode> GetPromoCodeList();
        Task<IEquatable<PromoCode>> GetAsync();
        Task<PromoCode> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class PromoCodeService : IPromoCodeService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PromoCode> _PromoCodeRepository;
        RepositoryQuery<PromoCode> PromoCodeRepository;
        public PromoCodeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PromoCodeRepository = new Repository<PromoCode>(db);
            PromoCodeRepository = new RepositoryQuery<PromoCode>(_PromoCodeRepository);
        }


        public PromoCode Find(string Name)
        {            
            return PromoCodeRepository.Get().Where(i => i.PromoCodeName == Name).FirstOrDefault();
        }


        public PromoCode Find(int id)
        {
            return _unitOfWork.Repository<PromoCode>().Find(id);            
        }

        public PromoCode Create(PromoCode pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PromoCode>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PromoCode>().Delete(id);
        }

        public void Delete(PromoCode pt)
        {
            _unitOfWork.Repository<PromoCode>().Delete(pt);
        }

        public void Update(PromoCode pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PromoCode>().Update(pt);
        }

        public IEnumerable<PromoCode> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PromoCode>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PromoCodeName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<PromoCode> GetPromoCodeList()
        {
            return ( from p in db.PromoCode
                         orderby p.PromoCodeName
                         select p
                         );
        }

        public PromoCode Add(PromoCode pt)
        {
            _unitOfWork.Repository<PromoCode>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PromoCode
                        orderby p.PromoCodeName
                        select p.PromoCodeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PromoCode
                        orderby p.PromoCodeName
                        select p.PromoCodeId).FirstOrDefault();
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

                temp = (from p in db.PromoCode
                        orderby p.PromoCodeName
                        select p.PromoCodeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PromoCode
                        orderby p.PromoCodeName
                        select p.PromoCodeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PromoCode>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PromoCode> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

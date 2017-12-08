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
    public interface IDeliveryTermsService : IDisposable
    {
        DeliveryTerms Create(DeliveryTerms pt);
        void Delete(int id);
        void Delete(DeliveryTerms pt);
        DeliveryTerms Find(string Name);
        DeliveryTerms Find(int id);
        IEnumerable<DeliveryTerms> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(DeliveryTerms pt);
        DeliveryTerms Add(DeliveryTerms pt);
        IEnumerable<DeliveryTerms> GetDeliveryTermsList();

        // IEnumerable<DeliveryTerms> GetDeliveryTermsList(int buyerId);
        Task<IEquatable<DeliveryTerms>> GetAsync();
        Task<DeliveryTerms> FindAsync(int id);
        DeliveryTerms GetDeliveryTermsByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class DeliveryTermsService : IDeliveryTermsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<DeliveryTerms> _DeliveryTermsRepository;
        RepositoryQuery<DeliveryTerms> DeliveryTermsRepository;
        public DeliveryTermsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _DeliveryTermsRepository = new Repository<DeliveryTerms>(db);
            DeliveryTermsRepository = new RepositoryQuery<DeliveryTerms>(_DeliveryTermsRepository);
        }
        public DeliveryTerms GetDeliveryTermsByName(string terms)
        {
            return (from p in db.DeliveryTerms
                    where p.DeliveryTermsName == terms
                    select p).FirstOrDefault();
        }

        public DeliveryTerms Find(string Name)
        {
            return DeliveryTermsRepository.Get().Where(i => i.DeliveryTermsName == Name).FirstOrDefault();
        }


        public DeliveryTerms Find(int id)
        {
            return _unitOfWork.Repository<DeliveryTerms>().Find(id);
        }

        public DeliveryTerms Create(DeliveryTerms pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<DeliveryTerms>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<DeliveryTerms>().Delete(id);
        }

        public void Delete(DeliveryTerms pt)
        {
            _unitOfWork.Repository<DeliveryTerms>().Delete(pt);
        }

        public void Update(DeliveryTerms pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<DeliveryTerms>().Update(pt);
        }

        public IEnumerable<DeliveryTerms> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<DeliveryTerms>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DeliveryTermsName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<DeliveryTerms> GetDeliveryTermsList()
        {
            var pt = _unitOfWork.Repository<DeliveryTerms>().Query().Get().OrderBy(m=>m.DeliveryTermsName);

            return pt;
        }

        public DeliveryTerms Add(DeliveryTerms pt)
        {
            _unitOfWork.Repository<DeliveryTerms>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.DeliveryTerms
                        orderby p.DeliveryTermsName
                        select p.DeliveryTermsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.DeliveryTerms
                        orderby p.DeliveryTermsName
                        select p.DeliveryTermsId).FirstOrDefault();
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

                temp = (from p in db.DeliveryTerms
                        orderby p.DeliveryTermsName
                        select p.DeliveryTermsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.DeliveryTerms
                        orderby p.DeliveryTermsName
                        select p.DeliveryTermsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<DeliveryTerms>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DeliveryTerms> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

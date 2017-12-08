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
    public interface IChargeService : IDisposable
    {
        Charge Create(Charge pt);
        void Delete(int id);
        void Delete(Charge pt);
        Charge Find(string Name);
        Charge Find(int id);
        IEnumerable<Charge> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Charge pt);
        Charge Add(Charge pt);
        IEnumerable<Charge> GetChargeList();

        // IEnumerable<Charge> GetChargeList(int buyerId);
        Task<IEquatable<Charge>> GetAsync();
        Task<Charge> FindAsync(int id);
        Charge GetChargeByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ChargeService : IChargeService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Charge> _ChargeRepository;
        RepositoryQuery<Charge> ChargeRepository;
        public ChargeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ChargeRepository = new Repository<Charge>(db);
            ChargeRepository = new RepositoryQuery<Charge>(_ChargeRepository);
        }
        public Charge GetChargeByName(string terms)
        {
            return (from p in db.Charge
                    where p.ChargeName == terms
                    select p).FirstOrDefault();
        }

        public Charge Find(string Name)
        {
            return ChargeRepository.Get().Where(i => i.ChargeName == Name).FirstOrDefault();
        }


        public Charge Find(int id)
        {
            return _unitOfWork.Repository<Charge>().Find(id);
        }

        public Charge Create(Charge pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Charge>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Charge>().Delete(id);
        }

        public void Delete(Charge pt)
        {
            _unitOfWork.Repository<Charge>().Delete(pt);
        }

        public void Update(Charge pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Charge>().Update(pt);
        }

        public IEnumerable<Charge> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Charge>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ChargeName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Charge> GetChargeList()
        {
            var pt = _unitOfWork.Repository<Charge>().Query().Get().OrderBy(m=>m.ChargeName);

            return pt;
        }

        public IEnumerable<Charge> GetCalculateOnListForFooter(int id)//CalculationId
        {
            return (from p in db.CalculationFooter
                    join t in db.Charge on p.ChargeId equals t.ChargeId into table
                    from tab in table.DefaultIfEmpty()
                    where p.CalculationId == id
                    orderby tab.ChargeName
                    select tab
                        );
        }

        public IEnumerable<Charge> GetCalculateOnListForProduct(int id)//CalculationId
        {
            return (from p in db.CalculationProduct
                    join t in db.Charge on p.ChargeId equals t.ChargeId into table
                    from tab in table.DefaultIfEmpty()
                    where p.CalculationId == id
                    orderby tab.ChargeName
                    select tab
                        );
        }

        public Charge Add(Charge pt)
        {
            _unitOfWork.Repository<Charge>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Charge
                        orderby p.ChargeName
                        select p.ChargeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Charge
                        orderby p.ChargeName
                        select p.ChargeId).FirstOrDefault();
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

                temp = (from p in db.Charge
                        orderby p.ChargeName
                        select p.ChargeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Charge
                        orderby p.ChargeName
                        select p.ChargeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Charge>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Charge> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

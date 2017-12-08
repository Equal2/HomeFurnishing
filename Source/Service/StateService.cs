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
    public interface IStateService : IDisposable
    {
        State Create(State pt);
        void Delete(int id);
        void Delete(State pt);
        State Find(string Name);
        State Find(int id);
        IEnumerable<State> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(State pt);
        State Add(State pt);
        IEnumerable<State> GetStateList();
        IEnumerable<State> GetStateListForCountryType(int Id);

        // IEnumerable<State> GetStateList(int buyerId);
        Task<IEquatable<State>> GetAsync();
        Task<State> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
    }

    public class StateService : IStateService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<State> _StateRepository;
        RepositoryQuery<State> StateRepository;
        public StateService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _StateRepository = new Repository<State>(db);
            StateRepository = new RepositoryQuery<State>(_StateRepository);
        }


        public State Find(string Name)
        {            
            return StateRepository.Get().Where(i => i.StateName == Name).FirstOrDefault();
        }


        public State Find(int id)
        {
            return _unitOfWork.Repository<State>().Find(id);            
        }

        public State Create(State pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<State>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<State>().Delete(id);
        }

        public void Delete(State pt)
        {
            _unitOfWork.Repository<State>().Delete(pt);
        }

        public void Update(State pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<State>().Update(pt);
        }

        public IEnumerable<State> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<State>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.StateName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<State> GetStateList()
        {
            var pt = _unitOfWork.Repository<State>().Query().Include(i => i.Country).Get().OrderBy(m=>m.StateName);

            return pt;
        }

        public IEnumerable<State> GetStateListForCountryType(int Id)
        {
            var pt = _unitOfWork.Repository<State>().Query().Get().Where(i => i.CountryId == Id);

            return pt;
        }


        public State Add(State pt)
        {
            _unitOfWork.Repository<State>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.State
                        orderby p.StateName
                        select p.StateId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.State
                        orderby p.StateName
                        select p.StateId).FirstOrDefault();
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

                temp = (from p in db.State
                        orderby p.StateName
                        select p.StateId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.State
                        orderby p.StateName
                        select p.StateId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<State>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<State> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

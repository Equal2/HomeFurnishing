using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Model.ViewModels;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;

namespace Service
{
    public interface IAgentService : IDisposable
    {
        Agent Create(Agent Agent);
        void Delete(int id);
        void Delete(Agent Agent);
        Agent GetAgent(int AgentId);
        IEnumerable<Agent> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Agent Agent);
        Agent Add(Agent Agent);
        IEnumerable<Agent> GetAgentList();
        Task<IEquatable<Agent>> GetAsync();
        Task<Agent> FindAsync(int id);
        Agent GetAgentByName(string Name);
        Agent Find(int id);
        AgentViewModel GetAgentViewModel(int id);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<AgentIndexViewModel> GetAgentListForIndex();
    }

    public class AgentService : IAgentService
    {
        private readonly IUnitOfWorkForService _unitOfWork;
        ApplicationDbContext db = new ApplicationDbContext();
        public AgentService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Agent GetAgentByName(string Agent)
        {
            return (from b in db.Agent
                    join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                    from PersonTab in PersonTable.DefaultIfEmpty()
                    where PersonTab.Name == Agent
                    select new Agent
                    {
                        PersonID = b.PersonID,
                    }).FirstOrDefault();
        }
        public Agent GetAgent(int AgentId)
        {
            return _unitOfWork.Repository<Agent>().Find(AgentId);
        }

        public Agent Find(int id)
        {
            return _unitOfWork.Repository<Agent>().Find(id);
        }

        public Agent Create(Agent Agent)
        {
            Agent.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Agent>().Insert(Agent);
            return Agent;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Agent>().Delete(id);
        }

        public void Delete(Agent Agent)
        {
            _unitOfWork.Repository<Agent>().Delete(Agent);
        }

        public void Update(Agent Agent)
        {
            Agent.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Agent>().Update(Agent);
        }

        public IEnumerable<Agent> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var Agent = _unitOfWork.Repository<Agent>()
                .Query().Include(m => m.Person)
                .OrderBy(q => q.OrderBy(c => c.Person.Name))
                .Filter(q => !string.IsNullOrEmpty(q.Person.Name))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return Agent;
        }

        public IEnumerable<Agent> GetAgentList()
        {
            var Agent = _unitOfWork.Repository<Agent>().Query().Include(m => m.Person)
                .Get()
                .Where(m => m.Person.IsActive == true).OrderBy(m => m.Person.Name);

            return Agent;
        }

        public Agent Add(Agent Agent)
        {
            _unitOfWork.Repository<Agent>().Insert(Agent);
            return Agent;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Agent>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Agent> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from b in db.Agent
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from b in db.Agent
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).FirstOrDefault();
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

                temp = (from b in db.Agent
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from b in db.Agent
                        join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                        from PersonTab in PersonTable.DefaultIfEmpty()
                        orderby PersonTab.Name
                        select b.PersonID).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public AgentViewModel GetAgentViewModel(int id)
        {
            AgentViewModel Agentviewmodel = (from b in db.Agent
                                             join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                                             from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                                             join p in db.Persons on BusinessEntityTab.PersonID equals p.PersonID into PersonTable
                                             from PersonTab in PersonTable.DefaultIfEmpty()
                                             join pa in db.PersonAddress on PersonTab.PersonID equals pa.PersonId into PersonAddressTable
                                             from PersonAddressTab in PersonAddressTable.DefaultIfEmpty()
                                             join ac in db.LedgerAccount on PersonTab.PersonID equals ac.PersonId into AccountTable
                                             from AccountTab in AccountTable.DefaultIfEmpty()
                                             where PersonTab.PersonID == id
                                             select new AgentViewModel
                                             {
                                                 PersonId = PersonTab.PersonID,
                                                 Name = PersonTab.Name,
                                                 Suffix = PersonTab.Suffix,
                                                 Code = PersonTab.Code,
                                                 Phone = PersonTab.Phone,
                                                 Mobile = PersonTab.Mobile,
                                                 Email = PersonTab.Email,
                                                 Address = PersonAddressTab.Address,
                                                 CityId = PersonAddressTab.CityId,
                                                 Zipcode = PersonAddressTab.Zipcode,
                                                 IsSisterConcern = BusinessEntityTab.IsSisterConcern,
                                                 IsActive = PersonTab.IsActive,
                                                 LedgerAccountGroupId = AccountTab.LedgerAccountGroupId,
                                                 CreatedBy = PersonTab.CreatedBy,
                                                 CreatedDate = PersonTab.CreatedDate,
                                                 PersonAddressID = PersonAddressTab.PersonAddressID,
                                                 AccountId = AccountTab.LedgerAccountId,
                                                 ImageFileName = PersonTab.ImageFileName,
                                                 ImageFolderName = PersonTab.ImageFolderName
                                             }).FirstOrDefault();

            var PersonRegistration = (from pp in db.PersonRegistration
                                 where pp.PersonId == id
                                 select new
                                 {
                                     PersonRegistrationId = pp.PersonRegistrationID,
                                     RregistrationType = pp.RegistrationType,
                                     RregistrationNo = pp.RegistrationNo
                                 }).ToList();


            return Agentviewmodel;
        }


        public IQueryable<AgentIndexViewModel> GetAgentListForIndex()
        {
            var temp = from b in db.Agent
                       join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                       from PersonTab in PersonTable.DefaultIfEmpty()
                       orderby PersonTab.Name
                       select new AgentIndexViewModel
                       {
                           PersonId = PersonTab.PersonID,
                           Name = PersonTab.Name
                       };
            return temp;
        }

    }

}

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
    public interface IPersonRateGroupService : IDisposable
    {
        PersonRateGroup Create(PersonRateGroup pt);
        void Delete(int id);
        void Delete(PersonRateGroup pt);
        PersonRateGroup Find(string Name);
        PersonRateGroup Find(int id);
        void Update(PersonRateGroup pt);
        PersonRateGroup Add(PersonRateGroup pt);
        IQueryable<PersonRateGroup> GetPersonRateGroupList();
        PersonRateGroup GetPersonRateGroupByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class PersonRateGroupService : IPersonRateGroupService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PersonRateGroup> _PersonRateGroupRepository;
        RepositoryQuery<PersonRateGroup> PersonRateGroupRepository;

        public PersonRateGroupService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PersonRateGroupRepository = new Repository<PersonRateGroup>(db);
            PersonRateGroupRepository = new RepositoryQuery<PersonRateGroup>(_PersonRateGroupRepository);
        }

        public PersonRateGroup GetPersonRateGroupByName(string terms)
        {
            return (from p in db.PersonRateGroup
                    where p.PersonRateGroupName == terms
                    select p).FirstOrDefault();
        }

        public PersonRateGroup Find(string Name)
        {
            return PersonRateGroupRepository.Get().Where(i => i.PersonRateGroupName == Name).FirstOrDefault();
        }


        public PersonRateGroup Find(int id)
        {
            return _unitOfWork.Repository<PersonRateGroup>().Find(id);
        }

        public PersonRateGroup Create(PersonRateGroup pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PersonRateGroup>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PersonRateGroup>().Delete(id);
        }

        public void Delete(PersonRateGroup pt)
        {
            _unitOfWork.Repository<PersonRateGroup>().Delete(pt);
        }

        public void Update(PersonRateGroup pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PersonRateGroup>().Update(pt);
        }

        public IQueryable<PersonRateGroup> GetPersonRateGroupList()
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var pt = _unitOfWork.Repository<PersonRateGroup>().Query().Get().Where(m => m.SiteId == SiteId && m.DivisionId == DivisionId).OrderBy(m => m.PersonRateGroupName);

            return pt;
        }

        public PersonRateGroup Add(PersonRateGroup pt)
        {
            _unitOfWork.Repository<PersonRateGroup>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PersonRateGroup
                        orderby p.PersonRateGroupName
                        select p.PersonRateGroupId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PersonRateGroup
                        orderby p.PersonRateGroupName
                        select p.PersonRateGroupId).FirstOrDefault();
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

                temp = (from p in db.PersonRateGroup
                        orderby p.PersonRateGroupName
                        select p.PersonRateGroupId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PersonRateGroup
                        orderby p.PersonRateGroupName
                        select p.PersonRateGroupId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }

    }
}

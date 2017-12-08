using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
//using AdminSetup.Models.Models;
//using AdminSetup.Models.ViewModels;
//using Infrastructure.IO;
using AutoMapper;
using Model.Models;
using Data.Infrastructure;
using Model.ViewModel;

namespace Service
{
    public interface IRolesDivisionService : IDisposable
    {
        RolesDivision Create(RolesDivision pt);
        void Delete(int id);
        void Delete(RolesDivision pt);
        RolesDivision Find(int ptId);
        void Update(RolesDivision pt);
        RolesDivision Add(RolesDivision pt);
        IEnumerable<RolesDivision> GetRolesDivisionList();
        RolesDivision Find(int DivisionId, string RoleId);
        IEnumerable<RolesDivisionViewModel> GetRolesDivisionList(string RoleId);
        void CreateRange(List<RolesDivisionViewModel> vm, string UserName);
    }

    public class RolesDivisionService : IRolesDivisionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<RolesDivision> _RolesDivisionRepository;

        public RolesDivisionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public RolesDivisionService(IUnitOfWork unitOfWork, IRepository<RolesDivision> RolesDivisionRepo)
        {
            _unitOfWork = unitOfWork;
            _RolesDivisionRepository = RolesDivisionRepo;
        }

        public RolesDivision Find(int pt)
        {
            return _unitOfWork.Repository<RolesDivision>().Find(pt);
        }

        public RolesDivision Create(RolesDivision pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RolesDivision>().Add(pt);
            return pt;
        }

        public void CreateRange(List<RolesDivisionViewModel> vm, string UserName)
        {
            foreach(var item in vm)
            {
                RolesDivision obj = Mapper.Map<RolesDivision>(item);
                obj.CreatedBy = UserName;
                obj.CreatedDate = DateTime.Now;
                obj.ModifiedBy = UserName;
                obj.ModifiedDate = DateTime.Now;
                obj.ObjectState = Model.ObjectState.Added;
                _RolesDivisionRepository.Add(obj);
            }
        }


        public void Delete(int id)
        {
            _unitOfWork.Repository<RolesDivision>().Delete(id);
        }

        public void Delete(RolesDivision pt)
        {
            _unitOfWork.Repository<RolesDivision>().Delete(pt);
        }

        public void Update(RolesDivision pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RolesDivision>().Update(pt);
        }

        public IEnumerable<RolesDivision> GetRolesDivisionList()
        {
            var pt = _unitOfWork.Repository<RolesDivision>().Query().Get();

            return pt;
        }


        public RolesDivision Add(RolesDivision pt)
        {
            _unitOfWork.Repository<RolesDivision>().Insert(pt);
            return pt;
        }

        public RolesDivision Find(int DivisionId, string RoleId)
        {
            return _unitOfWork.Repository<RolesDivision>().Query().Get().Where(m => m.RoleId == RoleId && m.DivisionId == DivisionId).FirstOrDefault();
        }

        public IEnumerable<RolesDivisionViewModel> GetRolesDivisionList(string RoleId)
        {
            return (from p in _RolesDivisionRepository.Instance
                    where p.RoleId == RoleId
                    select new RolesDivisionViewModel
                    {
                        DivisionId = p.DivisionId,
                        RoleId = p.RoleId,
                        RoleName = p.Role.Name,
                        RolesDivisionId = p.RolesDivisionId
                    });
        }

        public void Save()
        {
            _unitOfWork.Save();
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}

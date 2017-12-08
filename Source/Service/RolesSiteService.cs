using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
//using AdminSetup.Models.Models;
//using AdminSetup.Models.ViewModels;
//using Models.Company.Models;
//using Infrastructure.IO;
using AutoMapper;
using Data.Infrastructure;
using Model.ViewModel;
using Model.Models;

namespace Service
{
    public interface IRolesSiteService : IDisposable
    {
        RolesSite Create(RolesSite pt);
        void Delete(int id);
        void Delete(RolesSite pt);
        RolesSite Find(int ptId);
        void Update(RolesSite pt);
        RolesSite Add(RolesSite pt);
        IEnumerable<RolesSite> GetRolesSiteList();
        RolesSite Find(int SiteId, string RoleId);
        IEnumerable<RolesSiteViewModel> GetRolesSiteList(string RoleId);
        void CreateRange(List<RolesSiteViewModel> pt, string UserName);
        void Save();
    }

    public class RolesSiteService : IRolesSiteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<RolesSite> _RolesSiteRepository;

        public RolesSiteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public RolesSiteService(IUnitOfWork unitOfWork, IRepository<RolesSite> RolesSiteRepo)
        {
            _unitOfWork = unitOfWork;
            _RolesSiteRepository = RolesSiteRepo;
        }
        
        public RolesSite Find(int pt)
        {
            return _unitOfWork.Repository<RolesSite>().Find(pt);
        }

        public RolesSite Create(RolesSite pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RolesSite>().Add(pt);
            return pt;
        }

        public void CreateRange(List<RolesSiteViewModel> pt,string UserName)
        {

            foreach(var item in pt)
            {
                RolesSite obj = Mapper.Map<RolesSite>(item);
                obj.CreatedBy = UserName;
                obj.CreatedDate = DateTime.Now;
                obj.ModifiedBy = UserName;
                obj.ModifiedDate = DateTime.Now;
                obj.ObjectState = ObjectState.Added;
                _RolesSiteRepository.Add(obj);
            }

        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RolesSite>().Delete(id);
        }

        public void Delete(RolesSite pt)
        {
            _unitOfWork.Repository<RolesSite>().Delete(pt);
        }

        public void Update(RolesSite pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RolesSite>().Update(pt);
        }

        public IEnumerable<RolesSite> GetRolesSiteList()
        {
            var pt = _unitOfWork.Repository<RolesSite>().Query().Get();

            return pt;
        }

        public void Save()
        {
            _unitOfWork.Save();
        }

        public RolesSite Add(RolesSite pt)
        {
            _unitOfWork.Repository<RolesSite>().Insert(pt);
            return pt;
        }

        public RolesSite Find(int SiteId, string RoleId)
        {
            return _unitOfWork.Repository<RolesSite>().Query().Get().Where(m => m.RoleId == RoleId && m.SiteId == SiteId).FirstOrDefault();
        }

        public IEnumerable<RolesSiteViewModel> GetRolesSiteList(string RoleId)
        {

            return (from p in _RolesSiteRepository.Instance
                    where p.RoleId == RoleId
                    select new RolesSiteViewModel
                    {
                        RoleId = p.RoleId,
                        RoleName = p.Role.Name,
                        RolesSiteId = p.RolesSiteId,
                        SiteId = p.SiteId,
                        SiteName = p.Site.SiteName,
                    }
                        );

        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}

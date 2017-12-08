using System.Collections.Generic;
using System.Linq;
using Model.Tasks.Models;
using System;
using Model;
using Model.Tasks.ViewModel;
using Data.Infrastructure;
using AutoMapper;
using Presentation.ViewModels;
using Model.DatabaseViews;

namespace Service
{
    public interface IUserTeamService : IDisposable
    {
        /// <summary>
        /// *Service Function*
        /// This function will create the object and add a log entry
        /// </summary>
        /// <param name="vmUserTeam"></param>
        /// <param name="UserName"></param>
        UserTeamViewModel Create(UserTeamViewModel vmUserTeam, string UserName);

        /// <summary>
        /// *Service Function*
        /// This function will delete the object based on id, but did not add a log entry
        /// </summary>
        /// <param name="id">PrimaryKey of the record</param>
        void Delete(int id);

        /// <summary>
        /// *Service Function*
        /// This function will delete the object but did not add a log entry
        /// </summary>
        /// <param name="objUserTeam"></param>
        void Delete(UserTeam objUserTeam);

        /// <summary>
        /// *General Function*
        /// This function will delete the object based on ViewModel.Id and add a log entry
        /// </summary>
        /// <param name="vmReason">ReasonViewModel</param>
        /// <param name="UserName">Current User</param>
        void Delete(ReasonViewModel vmReason, string UserName);

        /// <summary>
        /// *Service Function*
        /// This function will return the object based on id
        /// </summary>
        /// <param name="id">Primarykey of the record</param>
        UserTeam Find(int id);

        /// <summary>
        /// *General Function*
        /// This function will update the view model object, based on id and add a log entry
        /// </summary>
        /// <param name="vmUserTeam"></param>
        /// <param name="UserName"></param>
        void Update(UserTeamViewModel vmUserTeam, string UserName);

        /// <summary>
        /// *General Function*
        /// This function will return all the UserTeams
        /// </summary>
        IEnumerable<UserTeam> GetUserTeamList();

        /// <summary>
        /// *General Function*
        /// This function will return the next consecutive id
        /// </summary>
        /// <param name="id"></param>
        int NextId(int id);

        /// <summary>
        /// *General Function*
        /// This function will return the previous id
        /// </summary>
        /// <param name="id"></param>
        int PrevId(int id);

        /// <summary>
        /// *General Function*
        /// This function will return the view model object, based on id
        /// </summary>
        /// <param name="id">Primarykey of the record</param>
        UserTeamViewModel FindViewModel(int id);

        /// <summary>
        /// *General Function*
        /// This function will return viewmodel objects of all the UserTeams
        /// </summary>
        /// <param name="Id">Project Id</param>
        /// <param name="UserId">User Id</param>
        IEnumerable<UserTeamViewModel> GetUserTeamListVM(int Id, string UserId);

        /// <summary>
        /// *General Function*
        /// This function will return viewmodel objects of all the Projects
        /// </summary>
        /// <param name="UserId"> </param>
        /// <returns>IEnum<ProjectIndexViewModel></returns>
        IEnumerable<ProjectIndexViewModel> GetProjectIndex(string UserId);

        /// <summary>
        /// *General Function*
        /// This function will return all user teams of the User
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns>IEnum<string></returns>
        IEnumerable<string> GetUsersTeam(string UserName);
    }

    public class UserTeamService : IUserTeamService
    {
        private IRepository<UserTeam> _UserTeamRepository;
        private IRepository<Project> _ProjectRepository;
        private IRepository<_Users> _UserRepository;
        private IUnitOfWork _uniOfWork;        
        public UserTeamService(IRepository<UserTeam> UserTeamRepository, IRepository<Project> ProjectRepository,
            IUnitOfWork unitofwork, IRepository<_Users> UserRepository)
        {
            _UserTeamRepository = UserTeamRepository;
            _ProjectRepository = ProjectRepository;
            _UserRepository = UserRepository;
            _uniOfWork = unitofwork;
        }

        public UserTeam Find(int id)
        {
            return _UserTeamRepository.Find(id);            
        }

        public UserTeamViewModel Create(UserTeamViewModel vmUserTeams, string UserName)
        {

            UserTeam objUserTeam = new UserTeam();

            objUserTeam = Mapper.Map<UserTeam>(vmUserTeams);

            objUserTeam.ModifiedBy = UserName;
            objUserTeam.CreatedBy = UserName;
            objUserTeam.CreatedDate = DateTime.Now;
            objUserTeam.ModifiedDate = DateTime.Now;
            objUserTeam.ObjectState = ObjectState.Added;
            _UserTeamRepository.Add(objUserTeam);

            _uniOfWork.Save();

            vmUserTeams.UserTeamId = objUserTeam.UserTeamId;

            return vmUserTeams;
        }

        public void Delete(int id)
        {
            _UserTeamRepository.Delete(id);
        }

        public void Delete(UserTeam objUserTeam)
        {
            objUserTeam.ObjectState = Model.ObjectState.Deleted;
            _UserTeamRepository.Delete(objUserTeam);
        }

        public void Delete(ReasonViewModel vmReason, string UserName)
        {            
            var objUserTeam = Find(vmReason.id);

            objUserTeam.ObjectState = Model.ObjectState.Deleted;
            _UserTeamRepository.Delete(objUserTeam);

            _uniOfWork.Save();          
        }


        public void Update(UserTeamViewModel vmUserTeam, string UserName)
        {
            UserTeam objUserTeam = Find(vmUserTeam.UserTeamId);

            objUserTeam.User = vmUserTeam.User;
            objUserTeam.ProjectId = vmUserTeam.ProjectId;
            objUserTeam.Srl = vmUserTeam.Srl;
            objUserTeam.TeamUser = vmUserTeam.TeamUser;
            objUserTeam.ModifiedDate = DateTime.Now;
            objUserTeam.ModifiedBy = UserName;
            objUserTeam.ObjectState = ObjectState.Modified;
            _UserTeamRepository.Update(objUserTeam);

            _uniOfWork.Save();
        }

        public IEnumerable<UserTeam> GetUserTeamList()
        {
            return _UserTeamRepository.Query().Get().ToList();
        }
        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from ut in _UserTeamRepository.Instance
                        orderby ut.User
                        select ut.UserTeamId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from ut in _UserTeamRepository.Instance
                        orderby ut.User
                        select ut.UserTeamId).FirstOrDefault();
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

                temp = (from ut in _UserTeamRepository.Instance
                        orderby ut.User
                        select ut.UserTeamId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from ut in _UserTeamRepository.Instance
                        orderby ut.User
                        select ut.UserTeamId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public UserTeamViewModel FindViewModel(int id)
        {
            return (from ut in _UserTeamRepository.Instance
                    where ut.UserTeamId == id
                    select new UserTeamViewModel
                    {
                        ProjectId = ut.ProjectId,
                        ProjectName = ut.Project.ProjectName,
                        Srl = ut.Srl,
                        TeamUser = ut.TeamUser,
                        User = ut.User,
                        UserTeamId = ut.UserTeamId,
                    }).FirstOrDefault();
        }
        public IEnumerable<UserTeamViewModel> GetUserTeamListVM(int Id, string UserId)
        {
            return (from ut in _UserTeamRepository.Instance
                    where ut.User == UserId && ut.ProjectId == Id
                    orderby ut.TeamUser
                    select new UserTeamViewModel
                    {
                        ProjectId = ut.ProjectId,
                        ProjectName = ut.Project.ProjectName,
                        Srl = ut.Srl,
                        TeamUser = ut.TeamUser,
                        User = ut.User,
                        UserTeamId = ut.UserTeamId,
                    }).ToList();
        }
    
        public IEnumerable<ProjectIndexViewModel> GetProjectIndex(string UserId)
        {
            return (from pr in _ProjectRepository.Instance
                    orderby pr.ProjectName
                    select new ProjectIndexViewModel
                    {
                        ProjectId = pr.ProjectId,
                        ProjectName = pr.ProjectName,
                        UserTeamCount = (from ut in _UserTeamRepository.Instance
                                         where ut.User == UserId && ut.ProjectId == pr.ProjectId
                                         select ut).Count(),
                    }).ToList();
        }



        public IEnumerable<string> GetUsersTeam(string UserName)
        {
            var List = (from ut in _UserTeamRepository.Instance
                        where ut.User == UserName
                        select ut.TeamUser).ToList();

            List.Add(UserName);
            return List;
        }

        public void Dispose()
        {
            _uniOfWork.Dispose();
        }
    }
}

using System.Linq;
using Model.Tasks.Models;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Components.Logging;
using Data.Infrastructure;
using Core.Common;
using Model.Tasks.ViewModel;
using Model.ViewModel;
using System.Collections.Generic;
using AutoMapper;
using System.Xml.Linq;
using Presentation.ViewModels;
using Model.Models;
using Model.ViewModels;

namespace Service
{
    public interface IProjectService : IDisposable
    {
        /// <summary>
        /// *Service Function*
        /// This function will create the object and add a log entry
        /// </summary>
        /// <param name="vmProj"></param>
        /// <param name="UserName">Current User</param>
        ProjectViewModel Create(ProjectViewModel vmProj, string UserName);

        /// <summary>
        /// *Service Function*
        /// This function will delete the object based on id without a log entry
        /// </summary>
        /// <param name="id">PrimaryKey of the record</param>
        void Delete(int id);

        /// <summary>
        /// *Service Function*
        /// This function will delete the object based on the object without a log entry
        /// </summary>
        /// <param name="objProj"></param>
        void Delete(Project objProj);

        /// <summary>
        /// *Service Function*
        /// This function will delete the object based on the object with a log entry
        /// </summary>
        /// <param name="vmReason"></param>
        void Delete(ReasonViewModel vmReason, string UserName);

        /// <summary>
        /// *Service Function*
        /// This function will return the object based on the name
        /// </summary>
        /// <param name="Name"></param>
        Project Find(string Name);

        /// <summary>
        /// *Service Function*
        /// This function will return the object based on the id
        /// </summary>
        /// <param name="id">PrimaryKey of the record</param>
        Project Find(int id);

        /// <summary>
        /// *General Function*
        /// This function will return the object based on the id
        /// </summary>
        /// <param name="id">PrimaryKey of the record</param>
        ProjectViewModel FindViewModel(int id);

        /// <summary>
        /// *General Function*
        /// This function will update the object and add a log entry
        /// </summary>
        /// <param name="vmProj"></param>
        /// <param name="UserName">Current User</param>
        void Update(ProjectViewModel vmProj, string UserName);

        /// <summary>
        /// *General Function*
        /// This function will fetch all the Projects
        /// </summary>
        IQueryable<ProjectViewModel> GetProjectList();

        /// <summary>
        /// *General Function*
        /// Not Implemented
        /// </summary>
        Task<IEquatable<Project>> GetAsync();

        /// <summary>
        /// *General Function*
        /// Not Implemented
        /// </summary>
        Task<Project> FindAsync(int id);

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
        /// This function will check for duplicate document no for new record
        /// </summary>
        /// <param name="docno">DocNo to check for duplicacy</param>
        bool CheckForDocNoExists(string docno);
        /// <summary>
        /// *General Function*
        /// This function will for check duplicate document no for existing record
        /// </summary>
        /// <param name="docno">DocNo to check for duplicacy</param>
        /// <param name="headerid">Primarykey of the record</param>
        bool CheckForDocNoExists(string docno, int headerid);

        #region HelpList Getter
        /// <summary>
        /// *General Function*
        /// This function will create the help list for Projects
        /// </summary>
        /// <param name="searchTerm">user search term</param>
        /// <param name="pageSize">no of records to fetch for each page</param>
        /// <param name="pageNum">current page size </param>
        /// <returns>ComboBoxPagedResult</returns>
        ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum); 
        #endregion

        #region HelpList Setters
        /// <summary>
        /// *General Function*
        /// This function will return the object in (Id,Text) format based on the Id
        /// </summary>
        /// <param name="Id">Primarykey of the record</param>
        /// <returns>ComboBoxResult</returns>
        ComboBoxResult GetValue(int Id);

        /// <summary>
        /// *General Function*
        /// This function will return list of object in (Id,Text) format based on the Ids
        /// </summary>
        /// <param name="Id">PrimaryKey of the record(',' seperated string)</param>
        /// <returns>List<ComboBoxResult></returns>
        List<ComboBoxResult> GetListCsv(string Id); 
        #endregion
    }

    public class ProjectService : IProjectService
    {
        private IRepository<Project> _ProjectRepository;        
        private IUnitOfWork _uniOfWork;
        private IModificationCheck _ModificationCheck;

        public ProjectService(IRepository<Project> ProjectRepository, IUnitOfWork unitofwork, IModificationCheck ModificationCheck)
        {
            _ProjectRepository = ProjectRepository;
            _uniOfWork = unitofwork;            
            _ModificationCheck = ModificationCheck;
        }

        public Project Find(string Name)
        {
            return _ProjectRepository.Instance.Where(i => i.ProjectName == Name).FirstOrDefault();
        }




        public Project Find(int id)
        {
            return _ProjectRepository.Find(id);
        }



        public ProjectViewModel FindViewModel(int id)
        {
            var objProj = _ProjectRepository.Find(id);
            return Mapper.Map<ProjectViewModel>(objProj);
        }



        public ProjectViewModel Create(ProjectViewModel vmProject, string UserName)
        {

            Project objProject = new Project();
            objProject = Mapper.Map<Project>(vmProject);

            objProject.CreatedBy = UserName;
            objProject.CreatedDate = DateTime.Now;
            objProject.ModifiedBy = UserName;
            objProject.ModifiedDate = DateTime.Now;
            objProject.ObjectState = ObjectState.Added;
            _ProjectRepository.Add(objProject);

            _uniOfWork.Save();

            vmProject.ProjectId = objProject.ProjectId;

            return vmProject;
        }

        public void Delete(int id)
        {
            Project Proj = Find(id);
            Proj.ObjectState = Model.ObjectState.Deleted;

            _ProjectRepository.Delete(Proj);
        }

        public void Delete(Project objProj)
        {
            objProj.ObjectState = Model.ObjectState.Deleted;
            _ProjectRepository.Delete(objProj);
        }

        public void Delete(ReasonViewModel vmReason, string UserName)
        {
            var objProj = Find(vmReason.id);

            _ProjectRepository.Delete(objProj);

            _uniOfWork.Save();
        }

        public void Update(ProjectViewModel vmProject, string UserName)
        {

            Project objProject = Find(vmProject.ProjectId);

            objProject.ProjectName = vmProject.ProjectName;
            objProject.IsActive = vmProject.IsActive;
            objProject.ModifiedDate = DateTime.Now;
            objProject.ModifiedBy = UserName;

            _ProjectRepository.Update(objProject);

            _uniOfWork.Save();
        }

        public IQueryable<ProjectViewModel> GetProjectList()
        {
            var pt = _ProjectRepository.Instance.OrderBy(m => m.ProjectName).Select(m => new ProjectViewModel { IsActive = m.IsActive, ProjectId = m.ProjectId, ProjectName = m.ProjectName });

            return pt;
        }


        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from pr in _ProjectRepository.Instance
                        orderby pr.ProjectName
                        select pr.ProjectId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from pr in _ProjectRepository.Instance
                        orderby pr.ProjectName
                        select pr.ProjectId).FirstOrDefault();
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

                temp = (from pr in _ProjectRepository.Instance
                        orderby pr.ProjectName
                        select pr.ProjectId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from pr in _ProjectRepository.Instance
                        orderby pr.ProjectName
                        select pr.ProjectId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }



        public bool CheckForDocNoExists(string docno)
        {

            var temp = (from pr in _ProjectRepository.Instance
                        where pr.ProjectName == docno
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;

        }
        public bool CheckForDocNoExists(string docno, int headerid)
        {
            var temp = (from pr in _ProjectRepository.Instance
                        where pr.ProjectName == docno && pr.ProjectId != headerid
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _ProjectRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.ProjectName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.ProjectName
                        select new ComboBoxResult
                        {
                            text = pr.ProjectName,
                            id = pr.ProjectId.ToString()
                        }
              );

            var temp = list
               .Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = list.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return Data;
        }

        public ComboBoxResult GetValue(int Id)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Project> Projects = from pr in _ProjectRepository.Instance
                                        where pr.ProjectId == Id
                                        select pr;

            ProductJson.id = Projects.FirstOrDefault().ProjectId.ToString();
            ProductJson.text = Projects.FirstOrDefault().ProjectName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Project> Projects = from pr in _ProjectRepository.Instance
                                            where pr.ProjectId == temp
                                            select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Projects.FirstOrDefault().ProjectId.ToString(),
                    text = Projects.FirstOrDefault().ProjectName
                });
            }
            return ProductJson;
        }


        public void Dispose()
        {
            _uniOfWork.Dispose();
        }


        public Task<IEquatable<Project>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Project> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
using System.Linq;
using Model.Tasks.Models;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Model.Tasks.ViewModel;
using Components.Logging;
using Data.Infrastructure;
using System.Collections.Generic;
using AutoMapper;
using Presentation.ViewModels;
using Model.ViewModels;

namespace Service
{
    public interface ITasksService : IDisposable
    {
        /// <summary>
        /// *Service Function*
        /// This function will create the object and add a log entry
        /// </summary>
        /// <param name="vmTasks"></param>
        /// <param name="UserName">CurrentUser</param>
        TasksViewModel Create(TasksViewModel vmTasks, string UserName);

        /// <summary>
        /// *Service Function*
        /// This function will delete the object based on id, but does not add a log entry
        /// </summary>
        /// <param name="id">PrimaryKey of the record</param>
        void Delete(int id);

        /// <summary>
        /// *Service Function*
        /// This function will delete the object but does not add a log entry
        /// </summary>
        /// <param name="objTask"></param>
        void Delete(Tasks objTask);

        /// <summary>
        /// *General Function*
        /// This function will delete the object based on ViewModel.Id and add a log entry
        /// </summary>
        /// <param name="vmReason">ReasonViewModel</param>
        /// <param name="UserName">Current User</param>
        void Delete(ReasonViewModel vmReason, string UserName);

        /// <summary>
        /// *Service Function*
        /// This function will return the object based on name
        /// </summary>
        /// <param name="Name"></param>
        Tasks Find(string Name);

        /// <summary>
        /// *Service Function*
        /// This function will return the object based on id
        /// </summary>
        /// <param name="id">Primary key of the record</param>
        Tasks Find(int id);

        /// <summary>
        /// *General Function*
        /// This function will return the view model object, based on id
        /// </summary>
        /// <param name="id">Primary key of the record</param>
        TasksViewModel FindViewModel(int id);

        ///// <summary>
        ///// *Service Function*
        ///// This function will return the object based on Name
        ///// </summary>
        ///// <param name="Name">DocumentType name</param>
        //DocumentType FindDocumentType(string Name);

        /// <summary>
        /// *General Function*
        /// This function will update the view model object, based on id
        /// </summary>
        /// <param name="vmTasks"></param>
        /// <param name="UserName">Current User</param>
        void Update(TasksViewModel vmTasks, string UserName);

        /// <summary>
        /// *General Function*
        /// This function will return all the Tasks of the User, based on UserName and Status
        /// </summary>
        /// <param name="User">Current User</param>
        /// <param name="Status">Task Status</param>
        IQueryable<TasksViewModel> GetTasksList(string User, string Status);

        /// <summary>
        /// *General Function*
        /// This function will return all Outbox Tasks of the User, based on UserName and Status
        /// </summary>
        /// <param name="User">Current User</param>
        /// <param name="Status">Task Status</param>
        IQueryable<TasksViewModel> GetOutBoxTasksList(string User, string Status);

        /// <summary>
        /// *Service Function*
        /// Not Implemented
        /// </summary>
        Task<IEquatable<Tasks>> GetAsync();

        /// <summary>
        /// *Service Function*
        /// Not Implemented
        /// </summary>
        Task<Tasks> FindAsync(int id);

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

        #region HelpList getters
        /// <summary>
        /// *General Function*
        /// This function will create the help list for Tasks
        /// </summary>
        /// <param name="searchTerm">user search term</param>
        /// <param name="pageSize">no of records to fetch for each page</param>
        /// <param name="pageNum">current page size </param>
        /// <param name="User">UserName</param>
        /// <returns>ComboBoxPagedResult</returns>
        ComboBoxPagedResult GetTaskList(string searchTerm, int pageSize, int pageNum, string User);

        #endregion

        #region HelpList setters
        /// <summary>
        /// *General Function*
        /// This function will return the object based on the Id
        /// </summary>
        /// <param name="Id">PrimaryKey of the record</param>
        /// <returns>ComboBoxResult</returns>
        ComboBoxResult GetTask(int Id);

        /// <summary>
        /// *General Function*
        /// This function will return list of object based on the Ids
        /// </summary>
        /// <param name="Id">PrimaryKey of the record(',' seperated string)</param>
        /// <returns>List<ComboBoxResult></returns>
        List<ComboBoxResult> GetTask(string Id); 
        #endregion
    }

    public class TasksService : ITasksService
    {
        private IRepository<Tasks> _TaskRepository;
        private IUnitOfWork _uniOfWork;
        private IModificationCheck _ModificationCheck;        
        public TasksService(IRepository<Tasks> TaskRepository, IUnitOfWork unitofwork, IModificationCheck ModificationCheck)
        {
            _TaskRepository = TaskRepository;
            _uniOfWork = unitofwork;
            _ModificationCheck = ModificationCheck;
        }

        public Tasks Find(string Name)
        {
            return _TaskRepository.Instance.Where(i => i.TaskTitle == Name).FirstOrDefault();
        }


        public Tasks Find(int id)
        {
            return _TaskRepository.Find(id);
        }

        public TasksViewModel FindViewModel(int id)
        {
            var obj = _TaskRepository.Find(id);
            return Mapper.Map<TasksViewModel>(obj);
        }

        public TasksViewModel Create(TasksViewModel vmTasks, string UserName)
        {

            Tasks objTasks = new Tasks();
            objTasks = Mapper.Map<Tasks>(vmTasks);

            objTasks.IsActive = true;
            objTasks.CreatedBy = UserName;
            objTasks.ModifiedBy = UserName;
            objTasks.ModifiedDate = DateTime.Now;
            objTasks.CreatedDate = DateTime.Now;

            objTasks.ObjectState = ObjectState.Added;
            _TaskRepository.Add(objTasks);

            _uniOfWork.Save();

            vmTasks.TaskId = objTasks.TaskId;

            return vmTasks;
        }

        public void Delete(int id)
        {
            _TaskRepository.Delete(id);
        }

        public void Delete(Tasks objTasks)
        {
            objTasks.ObjectState = Model.ObjectState.Deleted;
            _TaskRepository.Delete(objTasks);
        }

        public void Delete(ReasonViewModel vmReason, string UserName)
        {
            var objTasks = Find(vmReason.id);

            objTasks.ObjectState = Model.ObjectState.Deleted;
            _TaskRepository.Delete(objTasks);

            _uniOfWork.Save();
        }

        public void Update(TasksViewModel vmTasks, string UserName)
        {
            Tasks objTasks = Find(vmTasks.TaskId);

            objTasks.TaskTitle = vmTasks.TaskTitle;
            objTasks.ProjectId = vmTasks.ProjectId;
            objTasks.Status = vmTasks.Status;
            objTasks.TaskDescription = vmTasks.TaskDescription;
            objTasks.Priority = vmTasks.Priority;
            objTasks.DueDate = vmTasks.DueDate;
            objTasks.ForUser = vmTasks.ForUser;
            objTasks.ModifiedDate = DateTime.Now;
            objTasks.ModifiedBy = UserName;
            objTasks.ObjectState = ObjectState.Modified;
            _TaskRepository.Update(objTasks);

            _uniOfWork.Save();
        }

        public IQueryable<TasksViewModel> GetTasksList(string User, string Status)
        {
            var pt = (from tr in _TaskRepository.Instance
                      where tr.ForUser == User && ((Status == TaskStatusConstants.Close || Status == TaskStatusConstants.Complete) ? (tr.Status == TaskStatusConstants.Close || tr.Status == TaskStatusConstants.Complete) : (tr.Status != TaskStatusConstants.Close && tr.Status != TaskStatusConstants.Complete))
                      orderby tr.Priority descending, tr.DueDate ?? DateTime.MaxValue, tr.CreatedDate
                      select new TasksViewModel
                      {

                          DueDate = tr.DueDate,
                          ForUser = tr.CreatedBy,
                          IsActive = tr.IsActive,
                          Priority = tr.Priority,
                          ProjectId = tr.ProjectId,
                          ProjectName = tr.Project.ProjectName,
                          Status = tr.Status,
                          TaskDescription = tr.TaskDescription,
                          TaskId = tr.TaskId,
                          TaskTitle = tr.TaskTitle,
                          CreatedDate = tr.CreatedDate,
                      });

            return pt;
        }

        public IQueryable<TasksViewModel> GetOutBoxTasksList(string User, string Status)
        {
            var pt = (from tr in _TaskRepository.Instance
                      where tr.CreatedBy == User && User != tr.ForUser && ((Status == TaskStatusConstants.Close || Status == TaskStatusConstants.Complete) ? (tr.Status == TaskStatusConstants.Close || tr.Status == TaskStatusConstants.Complete) : (tr.Status != TaskStatusConstants.Close && tr.Status != TaskStatusConstants.Complete))
                      orderby tr.Priority descending, tr.DueDate ?? DateTime.MaxValue, tr.CreatedDate
                      select new TasksViewModel
                      {
                          DueDate = tr.DueDate,
                          ForUser = tr.ForUser,
                          IsActive = tr.IsActive,
                          Priority = tr.Priority,
                          ProjectId = tr.ProjectId,
                          ProjectName = tr.Project.ProjectName,
                          Status = tr.Status,
                          TaskDescription = tr.TaskDescription,
                          TaskId = tr.TaskId,
                          TaskTitle = tr.TaskTitle,
                          CreatedDate = tr.CreatedDate,
                      });

            return pt;
        }
        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from tr in _TaskRepository.Instance
                        orderby tr.TaskTitle
                        select tr.TaskId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from tr in _TaskRepository.Instance
                        orderby tr.TaskTitle
                        select tr.TaskId).FirstOrDefault();
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

                temp = (from tr in _TaskRepository.Instance
                        orderby tr.TaskTitle
                        select tr.TaskId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from tr in _TaskRepository.Instance
                        orderby tr.TaskTitle
                        select tr.TaskId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ComboBoxPagedResult GetTaskList(string searchTerm, int pageSize, int pageNum, string User)
        {

            var list = (from tr in _TaskRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (tr.TaskTitle.ToLower().Contains(searchTerm.ToLower())))
                        && tr.ForUser == User && tr.Status != (TaskStatusConstants.Close) && tr.Status != (TaskStatusConstants.Complete)
                        orderby tr.TaskTitle
                        select new ComboBoxResult
                        {
                            text = tr.TaskTitle,
                            id = tr.TaskId.ToString()
                        }
            );

            var temp = list.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = list.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return Data;

        }

        public ComboBoxResult GetTask(int Id)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<Tasks> Tasks = from tr in _TaskRepository.Instance
                                      where tr.TaskId == Id
                                      select tr;

            ProductJson.id = Tasks.FirstOrDefault().TaskId.ToString();
            ProductJson.text = Tasks.FirstOrDefault().TaskTitle;

            return ProductJson;
        }

        public List<ComboBoxResult> GetTask(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<Tasks> Tasks = from tr in _TaskRepository.Instance
                                          where tr.TaskId == temp
                                          select tr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Tasks.FirstOrDefault().TaskId.ToString(),
                    text = Tasks.FirstOrDefault().TaskTitle
                });
            }

            return ProductJson;
        }

        public void Dispose()
        {
            _uniOfWork.Dispose();
        }


        public Task<IEquatable<Tasks>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Tasks> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

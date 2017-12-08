using System.Linq;
using Model.Tasks.Models;
using System;
using Model;
using System.Threading.Tasks;
using Model.Tasks.ViewModel;
using Infrastructure.IO;
using AutoMapper;
using Components.Logging;
using Models.BasicSetup.ViewModels;

namespace Service
{
    public interface IDARService : IDisposable
    {
        /// <summary>
        /// *Service Function*
        /// This function will create the object and add a log entry
        /// </summary>
        /// <param name="vmDAR"></param>
        /// <param name="UserName"></param>
        DARViewModel Create(DARViewModel vmDAR, string UserName);

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
        /// <param name="objDAR"></param>
        void Delete(DAR objDAR);

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
        DAR Find(int id);


        /// <summary>
        /// *General Function*
        /// This function will return the view model object, based on id
        /// </summary>
        /// <param name="id">Primarykey of the record</param>
        DARViewModel FindViewModel(int id);

        /// <summary>
        /// *General Function*
        /// This function will update the view model object, based on id and add a log entry
        /// </summary>
        /// <param name="vmDAR"></param>
        /// <param name="UserName"></param>
        void Update(DARViewModel vmDAR, string UserName);

        /// <summary>
        /// *General Function*
        /// This function will return all the DARs of the User, based on UserName
        /// </summary>
        /// <param name="UserName"></param>
        IQueryable<DARViewModel> GetDARList(string UserName);

        /// <summary>
        /// *Service Function*
        /// Not Implemented
        /// </summary>
        Task<IEquatable<DAR>> GetAsync();

        /// <summary>
        /// *Service Function*
        /// Not Implemented
        /// </summary>
        /// <param name="id"></param>
        Task<DAR> FindAsync(int id);

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

    }

    public class DARService : IDARService
    {
        private IRepository<DAR> _DARRepository;
        private IRepository<Tasks> _TaskRepository;
        
        private IUnitOfWork _uniOfWork;
        public DARService(IRepository<DAR> darRepository, IUnitOfWork unitofwork, IRepository<Tasks> TaskRepository)
        {
            _DARRepository = darRepository;
            _TaskRepository = TaskRepository;
            _uniOfWork = unitofwork;
            
        }

        public DAR Find(int id)
        {
            return _DARRepository.Instance.Where(m => m.DARId == id).FirstOrDefault();
        }

        public DARViewModel FindViewModel(int id)
        {
            var obj= _DARRepository.Find(id);
            return Mapper.Map<DARViewModel>(obj);
        }

        public DARViewModel Create(DARViewModel vmDAR, string UserName)
        {
            DAR objDAR = new DAR();
            objDAR = Mapper.Map<DAR>(vmDAR);

            objDAR.CreatedBy = UserName;
            objDAR.CreatedDate = DateTime.Now;
            objDAR.ModifiedBy = UserName;
            objDAR.ModifiedDate = DateTime.Now;
            objDAR.ObjectState = ObjectState.Added;

            _DARRepository.Add(objDAR);

            if (objDAR.TaskId != 0)
            {
                Tasks objTasks = _TaskRepository.Find(objDAR.TaskId);
                objTasks.Status = objDAR.Status;
                objTasks.ModifiedBy = UserName;
                objTasks.ModifiedDate = DateTime.Now;
                _TaskRepository.Update(objTasks);
            }

            _uniOfWork.Save();

            vmDAR.DARId = objDAR.DARId;

            return vmDAR;
        }

        public void Delete(int id)
        {
            DAR objDAR = _DARRepository.Find(id);
            objDAR.ObjectState = Model.ObjectState.Deleted;
            _DARRepository.Delete(objDAR);
        }

        public void Delete(ReasonViewModel vm, string UserName)
        {
            DAR objDAR = _DARRepository.Find(vm.id);

            objDAR.ObjectState = Model.ObjectState.Deleted;
            
            _DARRepository.Delete(objDAR);

            _uniOfWork.Save();
        }

        public void Delete(DAR objDAR)
        {
            objDAR.ObjectState = Model.ObjectState.Deleted;
            _DARRepository.Delete(objDAR);
        }

        public void Update(DARViewModel vmDAR, string UserName)
        {

            DAR objDAR = Find(vmDAR.DARId);

            objDAR.DARDate = vmDAR.DARDate;
            objDAR.Description = vmDAR.Description;
            objDAR.DueDate = vmDAR.DueDate;
            objDAR.ForUser = vmDAR.ForUser;
            objDAR.FromTime = vmDAR.FromTime;
            objDAR.IsActive = vmDAR.IsActive;
            objDAR.Priority = vmDAR.Priority;
            objDAR.Status = vmDAR.Status;
            objDAR.TaskId = vmDAR.TaskId;
            objDAR.ToTime = vmDAR.ToTime;
            objDAR.WorkHours = vmDAR.WorkHours;

            objDAR.ModifiedDate = DateTime.Now;
            objDAR.ModifiedBy = UserName;
            objDAR.ObjectState = ObjectState.Modified;
            _DARRepository.Update(objDAR);


            if (objDAR.TaskId != 0)
            {
                Tasks objTAsks = _TaskRepository.Find(vmDAR.TaskId);
                objTAsks.Status = vmDAR.Status;
                objTAsks.ModifiedDate = DateTime.Now;
                objTAsks.ModifiedBy = UserName;
                _TaskRepository.Update(objTAsks);
            }
            _uniOfWork.Save();
        }




        public IQueryable<DARViewModel> GetDARList(string UserName)
        {
            var pt = (from dr in _DARRepository.Instance
                      where dr.ForUser == UserName
                      orderby dr.DARDate descending
                      select new DARViewModel
                      {
                          DARDate = dr.DARDate,
                          DARId = dr.DARId,
                          Description = dr.Description,
                          DueDate = dr.DueDate,
                          ForUser = dr.ForUser,
                          FromTime = dr.FromTime,
                          IsActive = dr.IsActive,
                          Priority = dr.Priority,
                          Status = dr.Status,
                          TaskName = dr.Task.TaskTitle,
                          ToTime = dr.ToTime,
                          WorkHours = dr.WorkHours
                      }
                );

            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from dr in _DARRepository.Instance
                        orderby dr.DARId
                        select dr.DARId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from dr in _DARRepository.Instance
                        orderby dr.DARId
                        select dr.DARId).FirstOrDefault();
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

                temp = (from dr in _DARRepository.Instance
                        orderby dr.DARId
                        select dr.DARId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from dr in _DARRepository.Instance
                        orderby dr.DARId
                        select dr.DARId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }
        public void Dispose()
        {
            _uniOfWork.Dispose();
        }


        public Task<IEquatable<DAR>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DAR> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

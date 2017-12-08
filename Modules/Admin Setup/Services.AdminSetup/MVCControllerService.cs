using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Infrastructure.IO;
using AdminSetup.Models.ViewModels;
using AutoMapper;
using AdminSetup.Models.Models;

namespace Service
{
    public interface IMvcControllerService : IDisposable
    {
        /// <summary>
        /// This Function will create an object and add a log entry
        /// </summary>
        /// <param name="vmMVCContr"></param>
        /// <param name="UserName"></param>
        /// <returns></returns>
        MvcControllerViewModel Create(MvcControllerViewModel vmMVCContr, string UserName);

        /// <summary>
        /// This function will delete the object based on the id, but does not add a log entry
        /// </summary>
        /// <param name="id"> Primary key of the record</param>
        void Delete(int id);

        /// <summary>
        /// This function deletes the object but does not add a log entry
        /// </summary>
        /// <param name="objMvcContr"></param>
        void Delete(MvcController objMvcContr);

        /// <summary>
        /// This function returns the object based on the name
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        MvcController Find(string Name);

        /// <summary>
        /// This function returns the view model object based on the name
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        MvcControllerViewModel FindViewModel(string Name);

        /// <summary>
        /// This function returns the object based on the Id
        /// </summary>
        /// <param name="id">Primary key of the record</param>
        /// <returns></returns>
        MvcController Find(int id);

        /// <summary>
        /// This function returns the view model object based on the Id
        /// </summary>
        /// <param name="id">Primary key of the record</param>
        /// <returns></returns>
        MvcControllerViewModel FindViewModel(int id);

        /// <summary>
        /// This function will update the viewmodel object and add a Log entry
        /// </summary>
        /// <param name="vmMVCContr"></param>
        /// <param name="UserName"> Curent User </param>
        void Update(MvcControllerViewModel vmMVCContr, string UserName);

        /// <summary>
        /// This function will return all the MvcConroller
        /// </summary>
        /// <returns></returns>
        IEnumerable<MvcControllerViewModel> GetMvcControllerList();

        /// <summary>
        /// *Service Function*
        /// Not Implemented
        /// </summary>
        Task<IEquatable<MvcController>> GetAsync();

        /// <summary>
        /// *Service Function*
        /// Not Implemented
        /// </summary>
        /// <param name="id"></param>
        Task<MvcController> FindAsync(int id);

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
        /// This function will update all the Controllers List in the project.
        /// </summary>
        void SyncControllersList(List<ControllerActionList> Controllers, string UserName);
    }

    public class MvcControllerService : IMvcControllerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<MvcController> _MvcControllerRepository;
        public MvcControllerService(IUnitOfWork unitOfWork, IRepository<MvcController> mvcControllerRepository)
        {
            _unitOfWork = unitOfWork;
            _MvcControllerRepository = mvcControllerRepository;
        }

        public MvcController Find(string Name)
        {
            return _MvcControllerRepository.Instance.Where(i => i.ControllerName == Name).FirstOrDefault();
        }


        public MvcController Find(int id)
        {
            return _MvcControllerRepository.Find(id);
        }


        public MvcControllerViewModel FindViewModel(string Name)
        {
            var obj = _MvcControllerRepository.Instance.Where(i => i.ControllerName == Name).FirstOrDefault();
            return Mapper.Map<MvcControllerViewModel>(obj);
        }


        public MvcControllerViewModel FindViewModel(int id)
        {
            var obj = _MvcControllerRepository.Find(id);
            return Mapper.Map<MvcControllerViewModel>(obj);
        }

        public MvcControllerViewModel Create(MvcControllerViewModel pt, string UserName)
        {

            MvcController obj = new MvcController();
            obj = Mapper.Map<MvcController>(pt);

            obj.CreatedBy = UserName;
            obj.CreatedDate = DateTime.Now;
            obj.ModifiedBy = UserName;
            obj.ModifiedDate = DateTime.Now;
            obj.ObjectState = Model.ObjectState.Added;

            _MvcControllerRepository.Add(obj);

            _unitOfWork.Save();

            pt.ControllerId = obj.ControllerId;

            return pt;
        }

        public void Delete(int id)
        {
            _MvcControllerRepository.Delete(id);
        }

        public void Delete(MvcController pt)
        {
            _MvcControllerRepository.Delete(pt);
        }

        public void Delete(ReasonViewModel vmReason, string UserName)
        {
            _MvcControllerRepository.Delete(vmReason.id);
        }

        public void Update(MvcControllerViewModel vmMVCContr, string UserName)
        {

            MvcController objMVCContr = Find(vmMVCContr.ControllerId);

            objMVCContr.ControllerName = vmMVCContr.ControllerName;
            objMVCContr.IsActive = vmMVCContr.IsActive;
            objMVCContr.ParentControllerId = vmMVCContr.ParentControllerId;
            objMVCContr.PubModuleName = vmMVCContr.PubModuleName;
            objMVCContr.ModifiedBy = UserName;
            objMVCContr.ModifiedDate = DateTime.Now;

            objMVCContr.ObjectState = Model.ObjectState.Modified;

            _MvcControllerRepository.Update(objMVCContr);

            _unitOfWork.Save();

        }

        public IEnumerable<MvcControllerViewModel> GetMvcControllerList()
        {
            var pt = _MvcControllerRepository.Query().Get().OrderBy(m => m.ControllerName).Select(m => new MvcControllerViewModel
            {
                ControllerId = m.ControllerId,
                ControllerName = m.ControllerName,
                IsActive = m.IsActive,
                ParentControllerId = m.ParentControllerId,
                ParentControllerName = m.ParentController.ControllerName,
                PubModuleName = m.PubModuleName,
            });

            return pt;
        }
       
        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _MvcControllerRepository.Instance
                        orderby p.ControllerName
                        select p.ControllerId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _MvcControllerRepository.Instance
                        orderby p.ControllerName
                        select p.ControllerId).FirstOrDefault();
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

                temp = (from p in _MvcControllerRepository.Instance
                        orderby p.ControllerName
                        select p.ControllerId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _MvcControllerRepository.Instance
                        orderby p.ControllerName
                        select p.ControllerId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void SyncControllersList(List<ControllerActionList> Controllers,string UserName)
        {

            List<ControllerActionList> List = new List<ControllerActionList>();
          
            #region ☺ControllerUpdateCode☺

            List<ControllerActionList> DBControllers = (from p in _MvcControllerRepository.Instance
                                                        select new ControllerActionList { ControllerName = p.ControllerName, IsActive = p.IsActive }
                          ).ToList();

            var PendingToUpdate = from p in Controllers
                                  join t in DBControllers on p.ControllerName equals t.ControllerName into table
                                  from left in table.DefaultIfEmpty()
                                  where left == null
                                  select p.ControllerName;

            var PendingToDeActivate = from p in DBControllers
                                      join t in Controllers on p.ControllerName equals t.ControllerName into table
                                      from right in table.DefaultIfEmpty()
                                      where right == null
                                      select p.ControllerName;

            var PendingToActivate = from p in Controllers
                                    join t in DBControllers.Where(m => m.IsActive == false) on p.ControllerName equals t.ControllerName
                                    select p.ControllerName;


            foreach (var item in PendingToUpdate)
            {
                MvcController temp = new MvcController();

                temp.ControllerName = item;
                temp.IsActive = true;
                temp.ObjectState = Model.ObjectState.Added;
                temp.CreatedBy = UserName;
                temp.CreatedDate = DateTime.Now;
                temp.ModifiedBy = UserName;
                temp.ModifiedDate = DateTime.Now;

                _MvcControllerRepository.Add(temp);
            }

            foreach (var item in PendingToDeActivate)
            {
                MvcController DeactivateRecord = Find(item);
                DeactivateRecord.IsActive = false;
                DeactivateRecord.ModifiedBy = UserName;
                DeactivateRecord.ModifiedDate = DateTime.Now;
                DeactivateRecord.ObjectState = Model.ObjectState.Modified;

                _MvcControllerRepository.Update(DeactivateRecord);
            }

            foreach (var item in PendingToActivate)
            {
                MvcController ActivateRecord = Find(item);
                ActivateRecord.IsActive = true;
                ActivateRecord.ModifiedBy = UserName;
                ActivateRecord.ModifiedDate = DateTime.Now;
                ActivateRecord.ObjectState = Model.ObjectState.Modified;
                _MvcControllerRepository.Update(ActivateRecord);
            }
            #endregion


            _unitOfWork.Save();

        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<MvcController>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MvcController> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

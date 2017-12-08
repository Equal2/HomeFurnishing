using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Infrastructure.IO;
using AdminSetup.Models.ViewModels;
using AdminSetup.Models.Models;

namespace Service
{
    public interface IControllerActionService : IDisposable
    {
        /// <summary>
        /// *Service Function*
        /// This function will create the object and add a log entry
        /// </summary>
        /// <param name="vmContrAction"></param>
        /// <param name="UserName">Current User</param>
        /// <returns></returns>
        ControllerAction Create(ControllerAction vmContrAction);

        /// <summary>
        /// *Service Function*
        /// This function will delete the object based on id, but did not add a log entry
        /// </summary>
        /// <param name="id">Primary key of the record</param>
        void Delete(int id);

        /// <summary>
        /// *Service Function*
        /// This function will delete the object, but did not add a log entry
        /// </summary>
        /// <param name="pt"></param>
        void Delete(ControllerAction pt);
        ControllerAction Find(int id);
        ControllerAction Find(string Name);
        ControllerAction Find(string Name,int ? ControllerId);
        void Update(ControllerAction pt);
        ControllerAction Add(ControllerAction pt);
        IEnumerable<ControllerAction> GetControllerActionList();
        void SyncActionList(List<ControllerActionList> Actions, string UserName);
    }

    public class ControllerActionService : IControllerActionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ControllerAction> _ControllerActionRepository;
        private readonly IRepository<MvcController> _MvcControllerRepository;
        public ControllerActionService(IUnitOfWork unitOfWork, IRepository<ControllerAction> ControllerAction, IRepository<MvcController> mvcControllerRepository)
        {
            _unitOfWork = unitOfWork;
            _ControllerActionRepository = ControllerAction;
            _MvcControllerRepository = mvcControllerRepository;
        }

        public ControllerAction Find(int id)
        {
            return _ControllerActionRepository.Find(id);
        }

        public ControllerAction Find(string Name)
        {
            return _unitOfWork.Repository<ControllerAction>().Query().Get().Where(m=>m.ActionName==Name).FirstOrDefault();
        }

        public ControllerAction Find(string Name, int? ControllerId)
        {
            return _unitOfWork.Repository<ControllerAction>().Query().Get().Where(m => m.ActionName == Name&& m.ControllerId==ControllerId).FirstOrDefault();
        }
        public ControllerAction Create(ControllerAction pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ControllerAction>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ControllerAction>().Delete(id);
        }

        public void Delete(ControllerAction pt)
        {
            _unitOfWork.Repository<ControllerAction>().Delete(pt);
        }

        public void Update(ControllerAction pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ControllerAction>().Update(pt);
        }

        public IEnumerable<ControllerAction> GetControllerActionList()
        {
            var pt = (from p in _ControllerActionRepository.Instance                      
                      select p
                          );

            return pt;
        }

        public ControllerAction Add(ControllerAction pt)
        {
            _unitOfWork.Repository<ControllerAction>().Insert(pt);
            return pt;
        }

        public void SyncActionList(List<ControllerActionList> Actions, string UserName)
        {

            List<ControllerActionList> DbControllers = new List<ControllerActionList>();

            DbControllers = (from p in _MvcControllerRepository.Instance
                             where p.IsActive == true
                             select new ControllerActionList
                             {
                                 ControllerName = p.ControllerName,
                                 ControllerId = p.ControllerId,
                             }).ToList();


            var MapControllerActions = (from p in Actions
                                        join t in DbControllers on p.ControllerName equals t.ControllerName
                                        select new ControllerActionList
                                        {
                                            ActionName = p.ActionName,
                                            ControllerId = t.ControllerId,
                                        }
                                 );



            List<ControllerActionList> DBActions = new List<ControllerActionList>();

            DBActions = (from p in _ControllerActionRepository.Instance
                         where p.IsActive == true
                         select new ControllerActionList
                         {
                             ActionId = p.ControllerActionId,
                             ActionName = p.ActionName,
                             ControllerId = p.ControllerId,
                             IsActive = p.IsActive
                         }).ToList();



            var PendingToUpdate = from p in MapControllerActions
                                  join t in DBActions on new { ActName = p.ActionName, ContId = p.ControllerId } equals new { ActName = t.ActionName, ContId = t.ControllerId } into table
                                  from left in table.DefaultIfEmpty()
                                  where left == null
                                  select new ControllerActionList
                                  {
                                      ActionName = p.ActionName,
                                      ControllerId = p.ControllerId
                                  };

            var PendingToDeActivate = from p in DBActions
                                      join t in MapControllerActions on new { ActName = p.ActionName, ContId = p.ControllerId } equals new { ActName = t.ActionName, ContId = t.ControllerId } into table
                                      from right in table.DefaultIfEmpty()
                                      where right == null
                                      select new ControllerActionList
                                      {
                                          ActionName = p.ActionName,
                                          ControllerId = p.ControllerId,
                                      };

            var PendingToActivate = from p in MapControllerActions
                                    join t in DBActions.Where(m => m.IsActive == false) on new { ActName = p.ActionName, ContId = p.ControllerId } equals new { ActName = t.ActionName, ContId = t.ControllerId }
                                    select new ControllerActionList
                                    {
                                        ActionName = p.ActionName,
                                        ControllerId = p.ControllerId
                                    };


            foreach (var item in PendingToUpdate)
            {
                ControllerAction temp = new ControllerAction();

                temp.ActionName = item.ActionName;
                temp.IsActive = true;
                temp.ControllerId = item.ControllerId;
                temp.ObjectState = Model.ObjectState.Added;
                temp.CreatedBy = UserName;
                temp.CreatedDate = DateTime.Now;
                temp.ModifiedBy = UserName;
                temp.ModifiedDate = DateTime.Now;

                temp.ObjectState = Model.ObjectState.Added;

                _ControllerActionRepository.Add(temp);
            }

            foreach (var item in PendingToDeActivate)
            {
                ControllerAction DeactivateRecord = Find(item.ActionName, item.ControllerId);
                DeactivateRecord.IsActive = false;
                DeactivateRecord.ModifiedBy = UserName;
                DeactivateRecord.ModifiedDate = DateTime.Now;
                DeactivateRecord.ObjectState = Model.ObjectState.Modified;
                _ControllerActionRepository.Update(DeactivateRecord);
            }

            foreach (var item in PendingToActivate)
            {
                ControllerAction ActivateRecord = Find(item.ActionName, item.ControllerId);
                ActivateRecord.IsActive = true;
                ActivateRecord.ModifiedBy = UserName;
                ActivateRecord.ModifiedDate = DateTime.Now;
                ActivateRecord.ObjectState = Model.ObjectState.Modified;
                _ControllerActionRepository.Update(ActivateRecord);
            }

            _unitOfWork.Save();

        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

    }
}

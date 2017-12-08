using System;
using Model.Models;
using Data.Infrastructure;
using Model.ViewModel;

namespace Components.Logging
{
    public class LogActivity : ILogger
    {
        IRepository<ActivityLog> _LogRepository;
        IUnitOfWork _unitOfWork;
        public LogActivity(IRepository<ActivityLog> LogRepository,IUnitOfWork unitOfWork)
        {
            _LogRepository = LogRepository;
            _unitOfWork = unitOfWork;
        }
        public void LogActivityDetail(ActiivtyLogViewModel lvm)
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            ActivityLog log = new ActivityLog()
            {
                DocTypeId = lvm.DocTypeId,
                DocLineId = lvm.DocLineId,
                ActivityType = lvm.ActivityType,
                CreatedBy = lvm.User,
                UserRemark = lvm.UserRemark,
                Modifications = lvm.xEModifications != null ? lvm.xEModifications.ToString() : "",
                CreatedDate = DateTime.Now,
                DocNo = lvm.DocNo,
                DocId = lvm.DocId,
                DocDate = lvm.DocDate,
                Narration = lvm.Narration,
                ControllerName = lvm.ControllerName,
                ActionName = lvm.ActionName,
                DocStatus = lvm.DocStatus,
                SiteId = SiteId,
                DivisionId = DivisionId,
                ObjectState = Model.ObjectState.Added,
            };

            _LogRepository.Add(log);
            _unitOfWork.Save();
        }
      
    }
}

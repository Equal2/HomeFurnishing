using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Core.Common;
using Data.Models;
using Service;
using Data.Infrastructure;
using Model.ViewModel;
using System.Linq;
using Model.ViewModels;
using Jobs.Helpers;

namespace Jobs.Controllers
{

    [Authorize]
    public class GatePassCheckController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();
        
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IGatePassHeaderService _GatePassHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public GatePassCheckController(IGatePassHeaderService GatePassHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _GatePassHeaderService = GatePassHeaderService;
            _exception = exec;
            _unitOfWork = unitOfWork;
            if (!JobOrderEvents.Initialized)
            {
                JobOrderEvents Obj = new JobOrderEvents();
            }

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult JobWorkerGatePasses(int SupplierId)
        {
            return Json(_GatePassHeaderService.GetActiveGatePassesForSupplier(SupplierId),JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PostGatePassCheck(int GatePassHeaderId)
        {
            bool Success = true;
            string ExceptionMessage = "";
            var GatepassRecord=_GatePassHeaderService.Find(GatePassHeaderId);

            GatepassRecord.Status = (int)StatusConstants.Closed;

            GatepassRecord.ObjectState = Model.ObjectState.Modified;

            _GatePassHeaderService.Update(GatepassRecord);

            try
            {
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                ExceptionMsg = ex.ToString();
                Success = false;
            }
            

            return Json(new { success = Success, Message=ExceptionMsg });
        }

        public ActionResult GetJobWorkersForActiveGatePass(string searchTerm, int pageSize, int pageNum)
        {
            var Query = _GatePassHeaderService.GetGatePassActiveJobWorkers(searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        
    }
}

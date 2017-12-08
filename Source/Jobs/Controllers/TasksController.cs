using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using Service;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using Model.Tasks.ViewModel;
using NotificationContents;
using System.Threading.Tasks;
using Components.ExceptionHandlers;
using Jobs.Helpers;
using Model.Models;
//using Models.Login.Models;

namespace Jobs.Controllers
{
    [Authorize]
    public class TasksController : System.Web.Mvc.Controller
    {
        private ITasksService _TaskService;
        private IExceptionHandler _exception;
        private IUserTeamService _UserTeamService;
        string TaskUser = (string)System.Web.HttpContext.Current.Session["TasksUserName"];
        public TasksController(ITasksService TaskService, IExceptionHandler ExceService, IUserTeamService UserTeamService)
        {
            _TaskService = TaskService;
            _exception = ExceService;
            _UserTeamService = UserTeamService;
        }
        // GET: /TasksMaster/

        public ActionResult Index(string Outbox, string Status)
        {
            ViewBag.Status = Status;

            ViewBag.User = (string.IsNullOrEmpty(TaskUser) ? User.Identity.Name : TaskUser);
            ViewBag.UserTeamList = _UserTeamService.GetUsersTeam(User.Identity.Name);

            if (Outbox == "Outbox")
            {
                var Tasks = _TaskService.GetOutBoxTasksList((string.IsNullOrEmpty(TaskUser) ? User.Identity.Name : TaskUser), Status);
                ViewBag.Outbox = Outbox;
                return View(Tasks);
            }
            else
            {
                var Tasks = _TaskService.GetTasksList((string.IsNullOrEmpty(TaskUser) ? User.Identity.Name : TaskUser), Status);
                return View(Tasks);
            }
        }
        public void PrepareViewBag()
        {
            List<SelectListItem> temp = new List<SelectListItem>();
            temp.Add(new SelectListItem { Text = Enum.GetName(typeof(SaleOrderPriority), -10), Value = ((int)(SaleOrderPriority.Low)).ToString() });
            temp.Add(new SelectListItem { Text = Enum.GetName(typeof(SaleOrderPriority), 0), Value = ((int)(SaleOrderPriority.Normal)).ToString() });
            temp.Add(new SelectListItem { Text = Enum.GetName(typeof(SaleOrderPriority), 10), Value = ((int)(SaleOrderPriority.High)).ToString() });

            ViewBag.PriorityList = new SelectList(temp, "Value", "Text");

            List<SelectListItem> tempStat = new List<SelectListItem>();
            tempStat.Add(new SelectListItem { Text = TaskStatusConstants.Open, Value = TaskStatusConstants.Open });
            tempStat.Add(new SelectListItem { Text = TaskStatusConstants.InProgress, Value = TaskStatusConstants.InProgress });
            tempStat.Add(new SelectListItem { Text = TaskStatusConstants.Complete, Value = TaskStatusConstants.Complete });
            tempStat.Add(new SelectListItem { Text = TaskStatusConstants.ReOpen, Value = TaskStatusConstants.ReOpen });
            tempStat.Add(new SelectListItem { Text = TaskStatusConstants.Close, Value = TaskStatusConstants.Close });

            ViewBag.StatList = new SelectList(tempStat, "Value", "Text");

        }
        // GET: /TasksMaster/Create

        public ActionResult Create()
        {
            TasksViewModel vmTasks = new TasksViewModel();

            string UserName = (string)TempData["DefaultUser"];

            if (string.IsNullOrEmpty(UserName))
                vmTasks.ForUser = (string.IsNullOrEmpty(TaskUser) ? User.Identity.Name : TaskUser);
            else
                vmTasks.ForUser = UserName;

            vmTasks.Priority = 0;
            PrepareViewBag();

            return View("Create", vmTasks);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(TasksViewModel vmTasks)
        {
            TempData["DefaultUser"] = vmTasks.ForUser;
            if (ModelState.IsValid)
            {
                if (vmTasks.TaskId <= 0)
                {

                    try
                    {
                        _TaskService.Create(vmTasks, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("Create", vmTasks);
                    }
                    if (User.Identity.Name != vmTasks.ForUser) NotifyUser(vmTasks.TaskId);

                    return RedirectToAction("Create").Success("Data saved successfully");
                }
                else
                {
                    try
                    {
                        _TaskService.Update(vmTasks, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("Create", vmTasks);
                    }

                    return RedirectToAction("Index").Success("Data saved successfully");
                }

            }
            PrepareViewBag();
            return View("Create", vmTasks);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            TasksViewModel vmTasks = _TaskService.FindViewModel(id);

            if (vmTasks == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag();
            return View("Create", vmTasks);
        }

        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TasksViewModel vmTasks = _TaskService.FindViewModel(id);
            if (vmTasks == null)
            {
                return HttpNotFound();
            }
            ReasonViewModel vmReason = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vmReason);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vmReason)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    _TaskService.Delete(vmReason, User.Identity.Name);
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Reason", vmReason);
                }

                return Json(new { success = true });
            }
            return PartialView("_Reason", vmReason);
        }

        [HttpGet]
        public ActionResult NextPage(int id)//CurrentHeaderId
        {
            var nextId = _TaskService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _TaskService.PrevId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }
        [HttpGet]
        public ActionResult Print()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }
        [HttpGet]
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        [HttpGet]
        public ActionResult Report()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }


        private async Task NotifyUser(int id)//TaskId
        {
            TasksViewModel vmTasks = _TaskService.FindViewModel(id);
            Notification objNotification = new Notification();

            objNotification.NotificationSubjectId = (int)NotificationSubjectConstants.TaskCreated;
            objNotification.CreatedBy = User.Identity.Name;
            objNotification.CreatedDate = DateTime.Now;
            objNotification.ExpiryDate = DateTime.Now.AddDays(7);
            objNotification.IsActive = true;
            objNotification.ModifiedBy = User.Identity.Name;
            objNotification.ModifiedDate = DateTime.Now;
            objNotification.NotificationUrl = "/Tasks/Index?SiteId=16&DivisionId=6";
            objNotification.UrlKey = "TaskDomain";
            objNotification.NotificationText = vmTasks.TaskTitle;

            TaskNotification nc = new TaskNotification();

            await nc.CreateTaskNotificationAsync(objNotification, vmTasks.ForUser);

        }

        public JsonResult SetTaskUser(string UserName)
        {
            if (UserName == User.Identity.Name)
                System.Web.HttpContext.Current.Session.Remove("TasksUserName");
            else
                System.Web.HttpContext.Current.Session["TasksUserName"] = UserName;

            return Json(new { Success = true });
        }

        public ActionResult GetTasks(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _TaskService.GetTaskList(searchTerm, pageSize, pageNum, User.Identity.Name),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleTasks(int Ids)
        {
            return Json(_TaskService.GetTask(Ids));
        }

        public JsonResult SetTasks(string Ids)
        {
            return Json(_TaskService.GetTask(Ids));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _TaskService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

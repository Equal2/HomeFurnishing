using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using Service;
using Presentation;
using Model.Tasks.ViewModel;
using Components.ExceptionHandlers;
using Services.Tasks.Constants;
using ProjLib.Constants;
using Models.BasicSetup.ViewModels;

namespace Web
{
    [Authorize]
    public class DARController : System.Web.Mvc.Controller
    {
        private IDARService _DarService;
        private IUserTeamService _UserTeamService;
        private ITasksService _TaskService;
        string DARUser = (string)System.Web.HttpContext.Current.Session["DARUserName"];

        IExceptionHandler _exception;
        public DARController(IDARService DarService, IUserTeamService UserTeamService,ITasksService TaskService, IExceptionHandler Exception)
        {
            _DarService = DarService;
            _UserTeamService = UserTeamService;
            _TaskService = TaskService;
            _exception = Exception;
        }
        // GET: /DARMaster/

        public ActionResult Index()
        {
            ViewBag.User = (string.IsNullOrEmpty(DARUser) ? User.Identity.Name : DARUser);
            ViewBag.UserTeamList = _UserTeamService.GetUsersTeam(User.Identity.Name);

            var DAR = (_DarService.GetDARList((string.IsNullOrEmpty(DARUser) ? User.Identity.Name : DARUser)));
            return View(DAR);
            //return RedirectToAction("Create");
        }
        public void PrepareViewBag()
        {
            List<SelectListItem> temp = new List<SelectListItem>();
            temp.Add(new SelectListItem { Text = Enum.GetName(typeof(PriorityConstants), -10), Value = ((int)(PriorityConstants.Low)).ToString() });
            temp.Add(new SelectListItem { Text = Enum.GetName(typeof(PriorityConstants), 0), Value = ((int)(PriorityConstants.Normal)).ToString() });
            temp.Add(new SelectListItem { Text = Enum.GetName(typeof(PriorityConstants), 10), Value = ((int)(PriorityConstants.High)).ToString() });

            ViewBag.PriorityList = new SelectList(temp, "Value", "Text");

            List<SelectListItem> tempStat = new List<SelectListItem>();
            tempStat.Add(new SelectListItem { Text = TaskStatusConstants.Open, Value = TaskStatusConstants.Open });
            tempStat.Add(new SelectListItem { Text = TaskStatusConstants.InProgress, Value = TaskStatusConstants.InProgress });
            tempStat.Add(new SelectListItem { Text = TaskStatusConstants.Complete, Value = TaskStatusConstants.Complete });
            tempStat.Add(new SelectListItem { Text = TaskStatusConstants.ReOpen, Value = TaskStatusConstants.ReOpen });
            tempStat.Add(new SelectListItem { Text = TaskStatusConstants.Close, Value = TaskStatusConstants.Close });

            ViewBag.StatList = new SelectList(tempStat, "Value", "Text");

        }
        // GET: /DARMaster/Create

        public ActionResult Create(int? id)
        {
            DARViewModel vmDAR = new DARViewModel();
            vmDAR.IsActive = true;
            vmDAR.DARDate = DateTime.Now;
            if (id.HasValue && id.Value > 0)
            {
                vmDAR.TaskId = id.Value;
                vmDAR.Status = _TaskService.Find(vmDAR.TaskId).Status;
            }
            PrepareViewBag();
            vmDAR.ForUser = (string.IsNullOrEmpty(DARUser) ? User.Identity.Name : DARUser);
            return View("Create", vmDAR);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(DARViewModel vmDAR)
        {
            if (vmDAR.WorkHours <= 0)
                ModelState.AddModelError("WorkHours", "Work hours field is required");

            if (ModelState.IsValid)
            {
                if (vmDAR.DARId <= 0)
                {

                    try
                    {
                        _DarService.Create(vmDAR, (string.IsNullOrEmpty(DARUser) ? User.Identity.Name : DARUser));
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("Create", vmDAR);
                    }

                    return RedirectToAction("Create").Success("Data saved successfully");
                }
                else
                {
                   
                    try
                    {
                        _DarService.Update(vmDAR, (string.IsNullOrEmpty(DARUser) ? User.Identity.Name : DARUser));
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("Create", vmDAR);
                    }

                  
                    return RedirectToAction("Index").Success("Data saved successfully");
                }

            }
            PrepareViewBag();
            return View("Create", vmDAR);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            DARViewModel vmDAR = _DarService.FindViewModel(id);

            if (vmDAR == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag();
            return View("Create", vmDAR);
        }

        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DARViewModel vmDAR = _DarService.FindViewModel(id);
            if (vmDAR == null)
            {
                return HttpNotFound();
            }
            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vm);
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
                    _DarService.Delete(vmReason,User.Identity.Name);
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
            var nextId = _DarService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _DarService.PrevId(id);
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

        public JsonResult SetTaskUser(string UserName)
        {
            if (UserName == User.Identity.Name)
                System.Web.HttpContext.Current.Session.Remove("DARUserName");
            else
                System.Web.HttpContext.Current.Session["DARUserName"] = UserName;

            return Json(new { Success = true });

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _DarService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

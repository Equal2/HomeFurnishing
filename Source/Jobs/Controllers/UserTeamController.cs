using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Service;
using Presentation.ViewModels;
using Presentation;
using Model.Tasks.ViewModel;
using Components.ExceptionHandlers;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class UserTeamController : System.Web.Mvc.Controller
    {
        private IUserTeamService _UserTeamService;
        private IExceptionHandler _exception;
        private IProjectService _ProjectService;
        public UserTeamController(IUserTeamService UserTeamService, IExceptionHandler ExceptionHandlingService, IProjectService ProjectService)
        {
            _UserTeamService = UserTeamService;
            _exception = ExceptionHandlingService;
            _ProjectService = ProjectService;
        }
        // GET: /UserTeamMaster/

        public ActionResult ProjectIndex(string UserId)
        {
            var Temp = _UserTeamService.GetProjectIndex(UserId).ToList();
            ViewBag.UserId = UserId;
            return View(Temp);
        }

        public ActionResult Index(int Id, string UserId)
        {
            var UserTeam = _UserTeamService.GetUserTeamListVM(Id, UserId);
            ViewBag.ProjectId = Id;
            ViewBag.ProjectName = _ProjectService.FindViewModel(Id).ProjectName;
            ViewBag.UserId = UserId;
            return View(UserTeam);
        }

        // GET: /UserTeamMaster/Create

        public ActionResult Create(int Id, string UserId)
        {
            UserTeamViewModel vmUserTeam = new UserTeamViewModel();
            vmUserTeam.ProjectId = Id;
            ViewBag.ProjectName = _ProjectService.FindViewModel(Id).ProjectName;
            vmUserTeam.User = UserId;
            ViewBag.ProjectId = Id;
            ViewBag.UserId = UserId;
            return View("Create", vmUserTeam);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(UserTeamViewModel vmUserTeam)
        {
            ViewBag.ProjectId = vmUserTeam.ProjectId;
            ViewBag.UserId = vmUserTeam.User;

            if (vmUserTeam.ProjectId <= 0)
                ModelState.AddModelError("ProjectId", "The Project field is required.");

            if (ModelState.IsValid)
            {
                if (vmUserTeam.UserTeamId <= 0)
                {
                    try
                    {
                        _UserTeamService.Create(vmUserTeam, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", vmUserTeam);
                    }

                    return RedirectToAction("Create", new { Id = vmUserTeam.ProjectId, UserId = vmUserTeam.User }).Success("Record saved successfully");
                }
                else
                {
                    try
                    {
                        _UserTeamService.Update(vmUserTeam, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", vmUserTeam);
                    }

                    return RedirectToAction("Index", new { Id = vmUserTeam.ProjectId, UserId = vmUserTeam.User }).Success("Record saved successfully");
                }

            }
            return View("Create", vmUserTeam);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            UserTeamViewModel vmUserTeam = _UserTeamService.FindViewModel(id);

            ViewBag.ProjectId = vmUserTeam.ProjectId;
            ViewBag.UserId = vmUserTeam.User;

            if (vmUserTeam == null)
            {
                return HttpNotFound();
            }
            return View("Create", vmUserTeam);
        }

        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserTeamViewModel vmUserTeam = _UserTeamService.FindViewModel(id);
            if (vmUserTeam == null)
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
                    _UserTeamService.Delete(vmReason, User.Identity.Name);
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Reason", vmReason);
                }

                return Json(new { success = true }).Success("Record Deleted Successfully");
            }
            return PartialView("_Reason", vmReason);
        }

        [HttpGet]
        public ActionResult NextPage(int id)//CurrentHeaderId
        {
            var nextId = _UserTeamService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _UserTeamService.PrevId(id);
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



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _UserTeamService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

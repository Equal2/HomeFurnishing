using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using Model.Tasks.Models;
using Service;
using Presentation;
using Model.Tasks.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Components.ExceptionHandlers;
using Presentation.Helper;
using Models.BasicSetup.ViewModels;

namespace Web
{
    [Authorize]
    public class ProjectController : System.Web.Mvc.Controller
    {
        IProjectService _ProjectService;
        IExceptionHandler _exception;

        public ProjectController(IProjectService ProjectService, IExceptionHandler exception)
        {
            _ProjectService = ProjectService;
            _exception = exception;
        }
        // GET: /ProjectMaster/

        public ActionResult Index()
        {
            var Project = _ProjectService.GetProjectList();
            return View(Project);
        }

        // GET: /ProjectMaster/Create

        public ActionResult Create()
        {
            ProjectViewModel vmProj = new ProjectViewModel();
            vmProj.IsActive = true;
            return View("Create", vmProj);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ProjectViewModel vmProj)
        {
            if (ModelState.IsValid)
            {
                if (vmProj.ProjectId <= 0)
                {
                    try
                    {
                        _ProjectService.Create(vmProj, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", vmProj);
                    }

                    return RedirectToAction("Create").Success("Data saved successfully");
                }
                else
                {

                    try
                    {
                        _ProjectService.Update(vmProj, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", vmProj);
                    }

                    return RedirectToAction("Index").Success("Data saved successfully");
                }

            }
            return View("Create", vmProj);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            ProjectViewModel vmProj = _ProjectService.FindViewModel(id);
            if (vmProj == null)
            {
                return HttpNotFound();
            }
            return View("Create", vmProj);
        }

        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectViewModel vmProj = _ProjectService.FindViewModel(id);
            if (vmProj == null)
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
                    _ProjectService.Delete(vmReason, User.Identity.Name);
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
            var nextId = _ProjectService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _ProjectService.PrevId(id);
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


        public JsonResult DuplicateCheckForCreate(string docno)
        {
            var Exists = (_ProjectService.CheckForDocNoExists(docno));
            return Json(new { returnvalue = Exists });
        }

        public JsonResult DuplicateCheckForEdit(string docno, int headerid)
        {
            var Exists = (_ProjectService.CheckForDocNoExists(docno, headerid));
            return Json(new { returnvalue = Exists });
        }

        public ActionResult GetProject(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _ProjectService.GetList(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProject(int Ids)
        {
            return Json(_ProjectService.GetValue(Ids));
        }

        public JsonResult SetProject(string Ids)
        {
            return Json(_ProjectService.GetListCsv(Ids));
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ProjectService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

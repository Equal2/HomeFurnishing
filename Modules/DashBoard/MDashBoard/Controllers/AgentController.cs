using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Surya.India.Model.Models;
using Surya.India.Model.ViewModels;
using Surya.India.Data.Models;
using Surya.India.Data.Infrastructure;
using Surya.India.Service;
using Surya.India.Web;
using AutoMapper;
using Surya.India.Presentation.ViewModels;
using Surya.India.Presentation;
using Surya.India.Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Text;
using System.Configuration;
using System.IO;
using ImageResizer;
using Surya.India.Model.ViewModel;

namespace Surya.India.Web.Controllers
{
   [Authorize]
    public class AgentController : System.Web.Mvc.Controller
    {
       private ApplicationDbContext db = new ApplicationDbContext();
        IAgentService _AgentService;
        IBusinessEntityService _BusinessEntityService;
        IPersonService _PersonService;
        IPersonAddressService _PersonAddressService;
        IAccountService _AccountService;
        IPersonProcessService _PersonProcessService;
        IPersonRegistrationService _PersonRegistrationService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public AgentController(IAgentService AgentService, IBusinessEntityService BusinessEntityService, IAccountService AccountService, IPersonService PersonService, IPersonRegistrationService PersonRegistrationService, IPersonAddressService PersonAddressService, IPersonProcessService PersonProcessService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _AgentService = AgentService;
            _PersonService = PersonService;
            _PersonAddressService = PersonAddressService;
            _BusinessEntityService = BusinessEntityService;
            _AccountService = AccountService;
            _PersonProcessService = PersonProcessService;
            _PersonRegistrationService = PersonRegistrationService;
            _ActivityLogService = ActivityLogService;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }
        // GET: /Order/
        public ActionResult Index()
        {
            var Agents = _AgentService.GetAgentListForIndex();
            return View(Agents);
        }

        [HttpGet]
        public ActionResult NextPage(int id, string name)//AgentId
        {
            var nextId = _AgentService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, string name)//AgentId
        {
            var nextId = _AgentService.PrevId(id);
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

        [HttpGet]
        public ActionResult Remove()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }




       public void PrepareViewBag()
        {
            ViewBag.CityList = new CityService(_unitOfWork).GetCityList().ToList();
            ViewBag.PersonList = new PersonService(_unitOfWork).GetPersonList().ToList();
            ViewBag.AccountGroupList = new LedgerAccountGroupService(_unitOfWork).GetLedgerAccountGroupList().ToList();
            ViewBag.TdsCategoryList = new TdsCategoryService(_unitOfWork).GetTdsCategoryList().ToList();
            ViewBag.TdsGroupList = new TdsGroupService(_unitOfWork).GetTdsGroupList().ToList();
        }

        public ActionResult Create()
        {
            AgentViewModel p = new AgentViewModel();
            p.IsActive = true;
            PrepareViewBag();
            return View("Create", p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AgentViewModel AgentVm)
        {
            if (AgentVm.LedgerAccountGroupId == 0)
            {
                PrepareViewBag();
                return View(AgentVm).Danger("Account Group field is required");
            }

            if (_PersonService.CheckDuplicate(AgentVm.Name, AgentVm.Suffix, AgentVm.PersonId) == true)
            {
                PrepareViewBag();
                return View(AgentVm).Danger("Combination of name and sufix is duplicate");
            }


            if (ModelState.IsValid)
            {
                if (AgentVm.PersonId == 0)
                {
                    Person person = Mapper.Map<AgentViewModel, Person>(AgentVm);
                    BusinessEntity businessentity = Mapper.Map<AgentViewModel, BusinessEntity>(AgentVm);
                    Agent Agent = Mapper.Map<AgentViewModel, Agent>(AgentVm);
                    PersonAddress personaddress = Mapper.Map<AgentViewModel, PersonAddress>(AgentVm);
                    LedgerAccount account = Mapper.Map<AgentViewModel, LedgerAccount>(AgentVm);

                    person.CreatedDate = DateTime.Now;
                    person.ModifiedDate = DateTime.Now;
                    person.CreatedBy = User.Identity.Name;
                    person.ModifiedBy = User.Identity.Name;
                    person.ObjectState = Model.ObjectState.Added;
                    new PersonService(_unitOfWork).Create(person);

                    _BusinessEntityService.Create(businessentity);
                    _AgentService.Create(Agent);

                    personaddress.AddressType = AddressTypeConstants.Work;
                    personaddress.CreatedDate = DateTime.Now;
                    personaddress.ModifiedDate = DateTime.Now;
                    personaddress.CreatedBy = User.Identity.Name;
                    personaddress.ModifiedBy = User.Identity.Name;
                    personaddress.ObjectState = Model.ObjectState.Added;
                    _PersonAddressService.Create(personaddress);


                    account.LedgerAccountName = person.Name;
                    account.LedgerAccountSuffix = person.Suffix;
                    account.CreatedDate = DateTime.Now;
                    account.ModifiedDate = DateTime.Now;
                    account.CreatedBy = User.Identity.Name;
                    account.ModifiedBy = User.Identity.Name;
                    account.ObjectState = Model.ObjectState.Added;
                    _AccountService.Create(account);


                    try
                    {
                        _unitOfWork.Save();
                    }
                  
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View(AgentVm);

                    }


                    #region

                    //Saving Images if any uploaded after UnitOfWorkSave

                    if (Request.Files[0] != null && Request.Files[0].ContentLength > 0)
                    {
                        //For checking the first time if the folder exists or not-----------------------------
                        string uploadfolder;
                        int MaxLimit;
                        int.TryParse(ConfigurationManager.AppSettings["MaxFileUploadLimit"], out MaxLimit);
                        var x = (from iid in db.Counter
                                 select iid).FirstOrDefault();
                        if (x == null)
                        {

                            uploadfolder = System.Guid.NewGuid().ToString();
                            Counter img = new Counter();
                            img.ImageFolderName = uploadfolder;
                            img.ModifiedBy = User.Identity.Name;
                            img.CreatedBy = User.Identity.Name;
                            img.ModifiedDate = DateTime.Now;
                            img.CreatedDate = DateTime.Now;
                            new CounterService(_unitOfWork).Create(img);
                            _unitOfWork.Save();
                        }

                        else
                        { uploadfolder = x.ImageFolderName; }


                        //For checking if the image contents length is greater than 100 then create a new folder------------------------------------

                        if (!Directory.Exists(System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder))) Directory.CreateDirectory(System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder));

                        int count = Directory.GetFiles(System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder)).Length;

                        if (count >= MaxLimit)
                        {
                            uploadfolder = System.Guid.NewGuid().ToString();
                            var u = new CounterService(_unitOfWork).Find(x.CounterId);
                            u.ImageFolderName = uploadfolder;
                            new CounterService(_unitOfWork).Update(u);
                            _unitOfWork.Save();
                        }


                        //Saving Thumbnails images:
                        Dictionary<string, string> versions = new Dictionary<string, string>();

                        //Define the versions to generate
                        versions.Add("_thumb", "maxwidth=100&maxheight=100"); //Crop to square thumbnail
                        versions.Add("_medium", "maxwidth=200&maxheight=200"); //Fit inside 400x400 area, jpeg

                        string temp2 = "";
                        string filename = System.Guid.NewGuid().ToString();
                        foreach (string filekey in System.Web.HttpContext.Current.Request.Files.Keys)
                        {

                            HttpPostedFile pfile = System.Web.HttpContext.Current.Request.Files[filekey];
                            if (pfile.ContentLength <= 0) continue; //Skip unused file controls.  

                            temp2 = Path.GetExtension(pfile.FileName);

                            string uploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder);
                            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                            string filecontent = Path.Combine(uploadFolder, AgentVm.Name + "_" + filename);

                            //pfile.SaveAs(filecontent);
                            ImageBuilder.Current.Build(new ImageJob(pfile, filecontent, new Instructions(), false, true));


                            //Generate each version
                            foreach (string suffix in versions.Keys)
                            {
                                if (suffix == "_thumb")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Thumbs");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, AgentVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, AgentVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                }

                            }

                            //var tempsave = _FinishedProductService.Find(pt.ProductId);

                            person.ImageFileName = AgentVm.Name + "_" + filename + temp2;
                            person.ImageFolderName = uploadfolder;
                            person.ObjectState = Model.ObjectState.Modified;
                            _PersonService.Update(person);
                            _unitOfWork.Save();
                        }

                    }

                    #endregion




                    //return RedirectToAction("Create").Success("Data saved successfully");
                    return RedirectToAction("Edit", new { id = person.PersonID }).Success("Data saved Successfully");
                }
                else
                {
                    //string tempredirect = (Request["Redirect"].ToString());
                    Person person = Mapper.Map<AgentViewModel, Person>(AgentVm);
                    BusinessEntity businessentity = Mapper.Map<AgentViewModel, BusinessEntity>(AgentVm);
                    Agent Agent = Mapper.Map<AgentViewModel, Agent>(AgentVm);
                    PersonAddress personaddress = _PersonAddressService.Find(AgentVm.PersonAddressID);
                    LedgerAccount account = _AccountService.Find(AgentVm.AccountId);
                    
                    StringBuilder logstring = new StringBuilder();                                    

                    person.ModifiedDate = DateTime.Now;
                    person.ModifiedBy = User.Identity.Name;
                    new PersonService(_unitOfWork).Update(person);


                    _BusinessEntityService.Update(businessentity);
                    _AgentService.Update(Agent);

                    personaddress.Address = AgentVm.Address;
                    personaddress.CityId = AgentVm.CityId;
                    personaddress.Zipcode = AgentVm.Zipcode;
                    personaddress.ModifiedDate = DateTime.Now;
                    personaddress.ModifiedBy = User.Identity.Name;
                    personaddress.ObjectState = Model.ObjectState.Modified;
                    _PersonAddressService.Update(personaddress);

                    account.LedgerAccountName = person.Name;
                    account.LedgerAccountSuffix = person.Suffix;
                    account.ModifiedDate = DateTime.Now;
                    account.ModifiedBy = User.Identity.Name;
                    _AccountService.Update(account);

                    ////Saving Activity Log::
                    ActivityLog al = new ActivityLog()
                    {
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocId = AgentVm.PersonId,
                        Narration = logstring.ToString(),
                        CreatedDate = DateTime.Now,
                        CreatedBy = User.Identity.Name,
                        //DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(DocumentCategoryConstants.ProcessSequence).DocumentTypeId,

                    };
                    new ActivityLogService(_unitOfWork).Create(al);
                    //End of Saving ActivityLog

                    try
                    {
                        _unitOfWork.Save();
                    }
                  
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", AgentVm);
                    }





                    #region

                    //Saving Image if file is uploaded
                    if (Request.Files[0] != null && Request.Files[0].ContentLength > 0)
                    {
                        string uploadfolder = AgentVm.ImageFolderName;
                        string tempfilename = AgentVm.ImageFileName;
                        if (uploadfolder == null)
                        {
                            var x = (from iid in db.Counter
                                     select iid).FirstOrDefault();
                            if (x == null)
                            {

                                uploadfolder = System.Guid.NewGuid().ToString();
                                Counter img = new Counter();
                                img.ImageFolderName = uploadfolder;
                                img.ModifiedBy = User.Identity.Name;
                                img.CreatedBy = User.Identity.Name;
                                img.ModifiedDate = DateTime.Now;
                                img.CreatedDate = DateTime.Now;
                                new CounterService(_unitOfWork).Create(img);
                                _unitOfWork.Save();
                            }
                            else
                            { uploadfolder = x.ImageFolderName; }

                        }
                        //Deleting Existing Images

                        var xtemp = System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/" + tempfilename);
                        if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/" + tempfilename)))
                        {
                            System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/" + tempfilename));
                        }

                        //Deleting Thumbnail Image:

                        if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Thumbs/" + tempfilename)))
                        {
                            System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Thumbs/" + tempfilename));
                        }

                        //Deleting Medium Image:
                        if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Medium/" + tempfilename)))
                        {
                            System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + uploadfolder + "/Medium/" + tempfilename));
                        }

                        //Saving Thumbnails images:
                        Dictionary<string, string> versions = new Dictionary<string, string>();

                        //Define the versions to generate
                        versions.Add("_thumb", "maxwidth=100&maxheight=100"); //Crop to square thumbnail
                        versions.Add("_medium", "maxwidth=200&maxheight=200"); //Fit inside 400x400 area, jpeg                            

                        string temp2 = "";
                        string filename = System.Guid.NewGuid().ToString();
                        foreach (string filekey in System.Web.HttpContext.Current.Request.Files.Keys)
                        {

                            HttpPostedFile pfile = System.Web.HttpContext.Current.Request.Files[filekey];
                            if (pfile.ContentLength <= 0) continue; //Skip unused file controls.    

                            temp2 = Path.GetExtension(pfile.FileName);

                            string uploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder);
                            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                            string filecontent = Path.Combine(uploadFolder, AgentVm.Name + "_" + filename);

                            //pfile.SaveAs(filecontent);

                            ImageBuilder.Current.Build(new ImageJob(pfile, filecontent, new Instructions(), false, true));

                            //Generate each version
                            foreach (string suffix in versions.Keys)
                            {
                                if (suffix == "_thumb")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Thumbs");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, AgentVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, AgentVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                }
                            }
                        }

                        person.ImageFileName = person.Name + "_" + filename + temp2;
                        person.ImageFolderName = uploadfolder;
                        _PersonService.Update(person);
                        _unitOfWork.Save();
                    }

                    #endregion  







                    return RedirectToAction("Index").Success("Data saved successfully");
                }
            }
            PrepareViewBag();
            return View(AgentVm);
        }


        public ActionResult Edit(int id)
        {
            AgentViewModel bvm = _AgentService.GetAgentViewModel(id);
            PrepareViewBag();
            if (bvm == null)
            {
                return HttpNotFound();
            }
            return View("Create", bvm);
        }


        // GET: /ProductMaster/Delete/5
        
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Agent Agent = _AgentService.GetAgent(id);
            if (Agent == null)
            {
                return HttpNotFound();
            }

            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {


            if(ModelState.IsValid)
            {
                Person person = new PersonService(_unitOfWork).Find(vm.id);
                BusinessEntity businessentiry = _BusinessEntityService.Find(vm.id);
                Agent Agent = _AgentService.Find(vm.id);
                
                ActivityLog al = new ActivityLog()
                {
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    DocId = vm.id,
                    UserRemark = vm.Reason,
                    Narration = "Agent is deleted with Name:" + person.Name,
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(DocumentCategoryConstants.SaleOrder).DocumentTypeId,
                    UploadDate = DateTime.Now,

                };
                new ActivityLogService(_unitOfWork).Create(al);

                //Then find all the Person Address associated with the above Person.
                PersonAddress personaddress = _PersonAddressService.GetShipAddressByPersonId(vm.id);
                _PersonAddressService.Delete(personaddress.PersonAddressID);


                IEnumerable<PersonContact> personcontact = new PersonContactService(_unitOfWork).GetPersonContactIdListByPersonId(vm.id);
                //Mark ObjectState.Delete to all the Person Contact For Above Person. 
                foreach (PersonContact item in personcontact)
                {
                    new PersonContactService(_unitOfWork).Delete(item.PersonContactID);
                }

                IEnumerable<PersonBankAccount> personbankaccount = new PersonBankAccountService(_unitOfWork).GetPersonBankAccountIdListByPersonId(vm.id);
                //Mark ObjectState.Delete to all the Person Contact For Above Person. 
                foreach (PersonBankAccount item in personbankaccount)
                {
                    new PersonBankAccountService(_unitOfWork).Delete(item.PersonBankAccountID);
                }

            // Now delete the Parent Agent
            _AgentService.Delete(Agent);
            _BusinessEntityService.Delete(businessentiry);
            new PersonService(_unitOfWork).Delete(person);


            try
            {
                _unitOfWork.Save();
            }
           
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return PartialView("_Reason", vm);
            }
            return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        public ActionResult AddToExisting()
        {
            return PartialView("AddToExisting");
        }

        [HttpPost]
        public ActionResult AddToExisting(AddToExistingContactViewModel svm)
        {
            Agent Agent = new Agent();
            Agent.PersonID = svm.PersonId;
            _AgentService.Create(Agent);
            
            try
            {
                _unitOfWork.Save();
            }
           
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return PartialView("_Create", svm);

            }

            return Json(new { success = true });
        }

        [HttpGet]
        public ActionResult ChooseContactType()
        {
            return PartialView("ChooseContactType");
        }
    }
}

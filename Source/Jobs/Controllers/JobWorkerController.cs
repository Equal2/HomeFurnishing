using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Model.ViewModels;
using Data.Models;
using Data.Infrastructure;
using Service;
using AutoMapper;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using System.Text;
using System.IO;
using ImageResizer;
using System.Configuration;
using Model.ViewModel;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
   [Authorize]
    public class JobWorkerController : System.Web.Mvc.Controller
    {
       private ApplicationDbContext db = new ApplicationDbContext();
        IJobWorkerService _JobWorkerService;
        IBusinessEntityService _BusinessEntityService;
        IPersonService _PersonService;
        IPersonAddressService _PersonAddressService;
        IAccountService _AccountService;
        IPersonProcessService _PersonProcessService;
        IPersonRegistrationService _PersonRegistrationService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobWorkerController(IJobWorkerService JobWorkerService, IBusinessEntityService BusinessEntityService, IAccountService AccountService, IPersonService PersonService, IPersonRegistrationService PersonRegistrationService, IPersonAddressService PersonAddressService, IPersonProcessService PersonProcessService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobWorkerService = JobWorkerService;
            _PersonService = PersonService;
            _PersonAddressService = PersonAddressService;
            _BusinessEntityService = BusinessEntityService;
            _AccountService = AccountService;
            _PersonProcessService = PersonProcessService;
            _PersonRegistrationService = PersonRegistrationService;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }
        // GET: /Order/
        public ActionResult Index()
        {
            var JobWorkers = _JobWorkerService.GetJobWorkerListForIndex();
            return View(JobWorkers);
        }

        [HttpGet]
        public ActionResult NextPage(int id, string name)//JobWorkerId
        {
            var nextId = _JobWorkerService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, string name)//JobWorkerId
        {
            var nextId = _JobWorkerService.PrevId(id);
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

            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.JobWorker);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        [HttpGet]
        public ActionResult Remove()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }




       public void PrepareViewBag()
        {
            ViewBag.SalesTaxGroupPartyList = new SalesTaxGroupPartyService(_unitOfWork).GetSalesTaxGroupPartyList().ToList();
            ViewBag.CityList = new CityService(_unitOfWork).GetCityList().ToList();
            ViewBag.CurrencyList = new CurrencyService(_unitOfWork).GetCurrencyList().ToList();
            ViewBag.PersonList = new PersonService(_unitOfWork).GetPersonList().ToList();
            ViewBag.TdsCategoryList = new TdsCategoryService(_unitOfWork).GetTdsCategoryList().ToList();
            ViewBag.TdsGroupList = new TdsGroupService(_unitOfWork).GetTdsGroupList().ToList();
            ViewBag.PersonRateGroupList = new PersonRateGroupService(_unitOfWork).GetPersonRateGroupList().ToList();
        }

        public ActionResult Create()
        {
            JobWorkerViewModel p = new JobWorkerViewModel();
            p.IsActive = true;
            p.Code = new PersonService(_unitOfWork).GetMaxCode();
            PrepareViewBag();
            return View("Create", p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JobWorkerViewModel JobWorkerVm)
        {
            string[] ProcessIdArr;
            if (JobWorkerVm.LedgerAccountGroupId == 0)
            {
                PrepareViewBag();
                return View(JobWorkerVm).Danger("Account Group field is required");
            }

            if (_PersonService.CheckDuplicate(JobWorkerVm.Name, JobWorkerVm.Suffix, JobWorkerVm.PersonId) == true)
            {
                PrepareViewBag();
                return View(JobWorkerVm).Danger("Combination of name and sufix is duplicate");
            }


            if (ModelState.IsValid)
            {
                if (JobWorkerVm.PersonId == 0)
                {
                    Person person = Mapper.Map<JobWorkerViewModel, Person>(JobWorkerVm);
                    BusinessEntity businessentity = Mapper.Map<JobWorkerViewModel, BusinessEntity>(JobWorkerVm);
                    JobWorker JobWorker = Mapper.Map<JobWorkerViewModel, JobWorker>(JobWorkerVm);
                    PersonAddress personaddress = Mapper.Map<JobWorkerViewModel, PersonAddress>(JobWorkerVm);
                    LedgerAccount account = Mapper.Map<JobWorkerViewModel, LedgerAccount>(JobWorkerVm);

                   
                    person.CreatedDate = DateTime.Now;
                    person.ModifiedDate = DateTime.Now;
                    person.CreatedBy = User.Identity.Name;
                    person.ModifiedBy = User.Identity.Name;
                    person.ObjectState = Model.ObjectState.Added;
                    new PersonService(_unitOfWork).Create(person);


                    string Divisions = JobWorkerVm.DivisionIds;
                    if (Divisions != null)
                    { Divisions = "|" + Divisions.Replace(",", "|,|") + "|"; }

                    businessentity.DivisionIds = Divisions;

                    string Sites = JobWorkerVm.SiteIds ;
                    if (Sites != null)
                    { Sites = "|" + Sites.Replace(",", "|,|") + "|"; }

                    businessentity.SiteIds = Sites;

                    _BusinessEntityService.Create(businessentity);
                    _JobWorkerService.Create(JobWorker);


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

                    //if (JobWorkerVm.ProcessIds != null &&  JobWorkerVm.ProcessIds != "")
                    //{
                    //    ProcessIdArr = JobWorkerVm.ProcessIds.Split(new Char[] { ',' });

                    //    for (int i = 0; i <= ProcessIdArr.Length - 1; i++)
                    //    {
                    //        PersonProcess personprocess = new PersonProcess();
                    //        personprocess.PersonId = JobWorker.PersonID;
                    //        personprocess.ProcessId = Convert.ToInt32(ProcessIdArr[i]);
                    //        personprocess.CreatedDate = DateTime.Now;
                    //        personprocess.ModifiedDate = DateTime.Now;
                    //        personprocess.CreatedBy = User.Identity.Name;
                    //        personprocess.ModifiedBy = User.Identity.Name;
                    //        personprocess.ObjectState = Model.ObjectState.Added;
                    //        _PersonProcessService.Create(personprocess);
                    //    }
                    //}

                    if (JobWorkerVm.PanNo != "" && JobWorkerVm.PanNo != null)
                    {
                        PersonRegistration personregistration = new PersonRegistration();
                        personregistration.RegistrationType = PersonRegistrationType.PANNo;
                        personregistration.RegistrationNo = JobWorkerVm.PanNo;
                        personregistration.CreatedDate = DateTime.Now;
                        personregistration.ModifiedDate = DateTime.Now;
                        personregistration.CreatedBy = User.Identity.Name;
                        personregistration.ModifiedBy = User.Identity.Name;
                        personregistration.ObjectState = Model.ObjectState.Added;
                        _PersonRegistrationService.Create(personregistration);
                    }

                    try
                    {
                        _unitOfWork.Save();
                    }
                 
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View(JobWorkerVm);

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

                            string filecontent = Path.Combine(uploadFolder, JobWorkerVm.Name + "_" + filename);

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
                                    string tfileName = Path.Combine(tuploadFolder, JobWorkerVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, JobWorkerVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                }

                            }

                            //var tempsave = _FinishedProductService.Find(pt.ProductId);

                            person.ImageFileName = JobWorkerVm.Name + "_" + filename + temp2;
                            person.ImageFolderName = uploadfolder;
                            person.ObjectState = Model.ObjectState.Modified;
                            _PersonService.Update(person);
                            _unitOfWork.Save();
                        }

                    }

                    #endregion
         





                    //return RedirectToAction("Create").Success("Data saved successfully");
                    return RedirectToAction("Edit", new { id = JobWorker.PersonID }).Success("Data saved Successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    //string tempredirect = (Request["Redirect"].ToString());
                    Person person = Mapper.Map<JobWorkerViewModel, Person>(JobWorkerVm);
                    BusinessEntity businessentity = Mapper.Map<JobWorkerViewModel, BusinessEntity>(JobWorkerVm);
                    JobWorker JobWorker = Mapper.Map<JobWorkerViewModel, JobWorker>(JobWorkerVm);
                    PersonAddress personaddress = _PersonAddressService.Find(JobWorkerVm.PersonAddressID);
                    LedgerAccount account = _AccountService.Find(JobWorkerVm.AccountId);
                    PersonRegistration PersonPan = _PersonRegistrationService.Find(JobWorkerVm.PersonRegistrationPanNoID);


                    PersonAddress ExRec = new PersonAddress();
                    ExRec = Mapper.Map<PersonAddress>(personaddress);

                    LedgerAccount ExRecLA = new LedgerAccount();
                    ExRecLA = Mapper.Map<LedgerAccount>(account);

                    PersonRegistration ExRecP = new PersonRegistration();
                    ExRecP = Mapper.Map<PersonRegistration>(PersonPan); 
                    
                    
                    StringBuilder logstring = new StringBuilder();

                    person.ModifiedDate = DateTime.Now;
                    person.ModifiedBy = User.Identity.Name;
                    new PersonService(_unitOfWork).Update(person);

                    string Divisions = JobWorkerVm.DivisionIds;
                    if (Divisions != null)
                    { Divisions = "|" + Divisions.Replace(",", "|,|") + "|"; }

                    businessentity.DivisionIds = Divisions;

                    string Sites = JobWorkerVm.SiteIds;
                    if (Sites != null)
                    { Sites = "|" + Sites.Replace(",", "|,|") + "|"; }

                    businessentity.SiteIds = Sites;

                    _BusinessEntityService.Update(businessentity);
                    _JobWorkerService.Update(JobWorker);

                    personaddress.Address = JobWorkerVm.Address;
                    personaddress.CityId = JobWorkerVm.CityId;
                    personaddress.Zipcode = JobWorkerVm.Zipcode;
                    personaddress.ModifiedDate = DateTime.Now;
                    personaddress.ModifiedBy = User.Identity.Name;
                    personaddress.ObjectState = Model.ObjectState.Modified;
                    _PersonAddressService.Update(personaddress);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = personaddress,
                    });
		

                    account.LedgerAccountName = person.Name;
                    account.LedgerAccountSuffix = person.Suffix;
                    account.LedgerAccountGroupId = JobWorkerVm.LedgerAccountGroupId; 
                    account.ModifiedDate = DateTime.Now;
                    account.ModifiedBy = User.Identity.Name;
                    _AccountService.Update(account);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecLA,
                        Obj = account,
                    });

                    //if (JobWorkerVm.ProcessIds != "" && JobWorkerVm.ProcessIds != null)
                    //{

                    //    IEnumerable<PersonProcess> personprocesslist = _PersonProcessService.GetPersonProcessList(JobWorkerVm.PersonId);

                    //    foreach (PersonProcess item in personprocesslist)
                    //    {
                    //        new PersonProcessService(_unitOfWork).Delete(item.PersonProcessId);
                    //    }


                        
                    //    ProcessIdArr = JobWorkerVm.ProcessIds.Split(new Char[] { ',' });

                    //    for (int i = 0; i <= ProcessIdArr.Length - 1; i++)
                    //    {
                    //        PersonProcess personprocess = new PersonProcess();
                    //        personprocess.PersonId = JobWorker.PersonID;
                    //        personprocess.ProcessId = Convert.ToInt32(ProcessIdArr[i]);
                    //        personprocess.CreatedDate = DateTime.Now;
                    //        personprocess.ModifiedDate = DateTime.Now;
                    //        personprocess.CreatedBy = User.Identity.Name;
                    //        personprocess.ModifiedBy = User.Identity.Name;
                    //        personprocess.ObjectState = Model.ObjectState.Added;
                    //        _PersonProcessService.Create(personprocess);
                    //    }
                    //}

                    if (JobWorkerVm.PanNo != null && JobWorkerVm.PanNo != "" )
                    {
                        if (PersonPan != null)
                        {
                            PersonPan.RegistrationNo = JobWorkerVm.PanNo;
                            _PersonRegistrationService.Update(PersonPan);

                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRecP,
                                Obj = PersonPan,
                            });

                        }
                        else
                        {
                            PersonRegistration personregistration = new PersonRegistration();
                            personregistration.PersonId = JobWorkerVm.PersonId;
                            personregistration.RegistrationType = PersonRegistrationType.PANNo;
                            personregistration.RegistrationNo = JobWorkerVm.PanNo;
                            personregistration.CreatedDate = DateTime.Now;
                            personregistration.ModifiedDate = DateTime.Now;
                            personregistration.CreatedBy = User.Identity.Name;
                            personregistration.ModifiedBy = User.Identity.Name;
                            personregistration.ObjectState = Model.ObjectState.Added;
                            _PersonRegistrationService.Create(personregistration);
                        }
                    }


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                 

                    try
                    {
                        _unitOfWork.Save();
                    }
                
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("Create", JobWorkerVm);
                    }

                    LogActivity.LogActivityDetail(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocCategoryConstants.JobWorker).DocumentTypeId,
                        DocId = JobWorkerVm.PersonId,
                        ActivityType = (int)ActivityTypeContants.Modified,                       
                        DocNo = JobWorkerVm.Name,
                        xEModifications = Modifications,                        
                    });
                    //End of Saving ActivityLog


                    #region

                    //Saving Image if file is uploaded
                    if (Request.Files[0] != null && Request.Files[0].ContentLength > 0)
                    {
                        string uploadfolder = JobWorkerVm.ImageFolderName;
                        string tempfilename = JobWorkerVm.ImageFileName;
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

                            string filecontent = Path.Combine(uploadFolder, JobWorkerVm.Name + "_" + filename);

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
                                    string tfileName = Path.Combine(tuploadFolder, JobWorkerVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, JobWorkerVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                }
                            }
                        }
                        var temsave = _PersonService.Find(person.PersonID);
                        temsave.ImageFileName = temsave.Name + "_" + filename + temp2;
                        temsave.ImageFolderName = uploadfolder;
                        _PersonService.Update(temsave);
                        _unitOfWork.Save();
                    }

                    #endregion  



                    return RedirectToAction("Index").Success("Data saved successfully");
                }
            }
            PrepareViewBag();
            return View(JobWorkerVm);
        }


        public ActionResult Edit(int id)
        {
            JobWorkerViewModel bvm = _JobWorkerService.GetJobWorkerViewModel(id);
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
            JobWorker JobWorker = _JobWorkerService.GetJobWorker(id);
            if (JobWorker == null)
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
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if(ModelState.IsValid)
            {
                Person person = new PersonService(_unitOfWork).Find(vm.id);
                BusinessEntity businessentiry = _BusinessEntityService.Find(vm.id);
                JobWorker jobworker = _JobWorkerService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = person,
                });

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = businessentiry,
                });

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = jobworker,
                });

                //Then find Ledger Account associated with the above Person.
                LedgerAccount ledgeraccount = _AccountService.GetLedgerAccountFromPersonId(vm.id);
                _AccountService.Delete(ledgeraccount.LedgerAccountId);

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

                IEnumerable<PersonProcess> personProcess = new PersonProcessService(_unitOfWork).GetPersonProcessIdListByPersonId(vm.id);
                //Mark ObjectState.Delete to all the Person Process For Above Person. 
                foreach (PersonProcess item in personProcess)
                {
                    new PersonProcessService(_unitOfWork).Delete(item.PersonProcessId);
                }

                IEnumerable<PersonRegistration> personregistration = new PersonRegistrationService(_unitOfWork).GetPersonRegistrationIdListByPersonId(vm.id);
                //Mark ObjectState.Delete to all the Person Registration For Above Person. 
                foreach (PersonRegistration item in personregistration)
                {
                    new PersonRegistrationService(_unitOfWork).Delete(item.PersonRegistrationID);
                }


            // Now delete the Parent JobWorker
                _JobWorkerService.Delete(jobworker);
                _BusinessEntityService.Delete(businessentiry);
                new PersonService(_unitOfWork).Delete(person);



                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

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


            LogActivity.LogActivityDetail(new ActiivtyLogViewModel
            {
                DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocCategoryConstants.JobWorker).DocumentTypeId,
                DocId = vm.id,
                ActivityType = (int)ActivityTypeContants.Deleted,
                UserRemark = vm.Reason,
                User = User.Identity.Name,
                DocNo = person.Name,
                xEModifications = Modifications
            });

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
            JobWorker jobworker = new JobWorker();
            jobworker.PersonID = svm.PersonId;
            _JobWorkerService.Create(jobworker);

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

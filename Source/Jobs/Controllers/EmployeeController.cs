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
using Jobs.Helpers;

namespace Jobs.Controllers.Controllers
{
   [Authorize]
    public class EmployeeController : System.Web.Mvc.Controller
    {
       private ApplicationDbContext db = new ApplicationDbContext();
        IEmployeeService _EmployeeService;
        IBusinessEntityService _BusinessEntityService;
        IPersonService _PersonService;
        IPersonAddressService _PersonAddressService;
        IAccountService _AccountService;
        IPersonProcessService _PersonProcessService;
        IPersonRegistrationService _PersonRegistrationService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public EmployeeController(IEmployeeService EmployeeService, IBusinessEntityService BusinessEntityService, IAccountService AccountService, IPersonService PersonService, IPersonRegistrationService PersonRegistrationService, IPersonAddressService PersonAddressService, IPersonProcessService PersonProcessService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _EmployeeService = EmployeeService;
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
            var Employees = _EmployeeService.GetEmployeeListForIndex();
            return View(Employees);
        }

        [HttpGet]
        public ActionResult NextPage(int id, string name)//EmployeeId
        {
            var nextId = _EmployeeService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, string name)//EmployeeId
        {
            var nextId = _EmployeeService.PrevId(id);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Employee );

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
            ViewBag.DesignationList = new DesignationService(_unitOfWork).GetDesignationList().ToList();
            ViewBag.DepartmentList = new DepartmentService(_unitOfWork).GetDepartmentList().ToList();
        }

        public ActionResult Create()
        {
            EmployeeViewModel p = new EmployeeViewModel();
            p.IsActive = true;
            p.Code = new PersonService(_unitOfWork).GetMaxCode();
            PrepareViewBag();
            return View("Create", p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EmployeeViewModel EmployeeVm)
        {
            string[] ProcessIdArr;
            if (EmployeeVm.LedgerAccountGroupId == 0)
            {
                PrepareViewBag();
                return View(EmployeeVm).Danger("Account Group field is required");
            }

            if (_PersonService.CheckDuplicate(EmployeeVm.Name, EmployeeVm.Suffix, EmployeeVm.PersonId) == true)
            {
                PrepareViewBag();
                return View(EmployeeVm).Danger("Combination of name and sufix is duplicate");
            }


            if (ModelState.IsValid)
            {
                if (EmployeeVm.PersonId == 0)
                {
                    Person person = Mapper.Map<EmployeeViewModel, Person>(EmployeeVm);
                    BusinessEntity businessentity = Mapper.Map<EmployeeViewModel, BusinessEntity>(EmployeeVm);
                    Employee Employee = Mapper.Map<EmployeeViewModel, Employee>(EmployeeVm);
                    PersonAddress personaddress = Mapper.Map<EmployeeViewModel, PersonAddress>(EmployeeVm);
                    LedgerAccount account = Mapper.Map<EmployeeViewModel, LedgerAccount>(EmployeeVm);

                    person.DocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.Employee).DocumentTypeId;
                    person.CreatedDate = DateTime.Now;
                    person.ModifiedDate = DateTime.Now;
                    person.CreatedBy = User.Identity.Name;
                    person.ModifiedBy = User.Identity.Name;
                    person.ObjectState = Model.ObjectState.Added;
                    new PersonService(_unitOfWork).Create(person);

                    string Divisions = EmployeeVm.DivisionIds;
                    if (Divisions != null)
                    { Divisions = "|" + Divisions.Replace(",", "|,|") + "|"; }

                    businessentity.DivisionIds = Divisions;

                    string Sites = EmployeeVm.SiteIds;
                    if (Sites != null)
                    { Sites = "|" + Sites.Replace(",", "|,|") + "|"; }

                    businessentity.SiteIds = Sites;

                    _BusinessEntityService.Create(businessentity);
                    _EmployeeService.Create(Employee);


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

                    if (EmployeeVm.ProcessIds != null && EmployeeVm.ProcessIds != "")
                    {
                        ProcessIdArr = EmployeeVm.ProcessIds.Split(new Char[] { ',' });

                        for (int i = 0; i <= ProcessIdArr.Length - 1; i++)
                        {
                            PersonProcess personprocess = new PersonProcess();
                            personprocess.PersonId = Employee.PersonID;
                            personprocess.ProcessId = Convert.ToInt32(ProcessIdArr[i]);
                            personprocess.CreatedDate = DateTime.Now;
                            personprocess.ModifiedDate = DateTime.Now;
                            personprocess.CreatedBy = User.Identity.Name;
                            personprocess.ModifiedBy = User.Identity.Name;
                            personprocess.ObjectState = Model.ObjectState.Added;
                            _PersonProcessService.Create(personprocess);
                        }
                    }

                    if (EmployeeVm.PanNo != "" && EmployeeVm.PanNo != null)
                    {
                        PersonRegistration personregistration = new PersonRegistration();
                        personregistration.RegistrationType = PersonRegistrationType.PANNo;
                        personregistration.RegistrationNo = EmployeeVm.PanNo;
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
                        return View(EmployeeVm);

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

                            string filecontent = Path.Combine(uploadFolder, EmployeeVm.Name + "_" + filename);

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
                                    string tfileName = Path.Combine(tuploadFolder, EmployeeVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, EmployeeVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                }

                            }

                            //var tempsave = _FinishedProductService.Find(pt.ProductId);

                            person.ImageFileName = EmployeeVm.Name+ "_" + filename + temp2;
                            person.ImageFolderName = uploadfolder;
                            person.ObjectState = Model.ObjectState.Modified;
                            _PersonService.Update(person);
                            _unitOfWork.Save();
                        }

                    }

                    #endregion



                    //return RedirectToAction("Create").Success("Data saved successfully");
                    return RedirectToAction("Edit", new { id = Employee.PersonID }).Success("Data saved Successfully");
                }
                else
                {
                    //string tempredirect = (Request["Redirect"].ToString());
                    Person person = Mapper.Map<EmployeeViewModel, Person>(EmployeeVm);
                    BusinessEntity businessentity = Mapper.Map<EmployeeViewModel, BusinessEntity>(EmployeeVm);
                    Employee Employee = Mapper.Map<EmployeeViewModel, Employee>(EmployeeVm);
                    PersonAddress personaddress = _PersonAddressService.Find(EmployeeVm.PersonAddressID);
                    LedgerAccount account = _AccountService.Find(EmployeeVm.AccountId);
                    PersonRegistration PersonPan = _PersonRegistrationService.Find(EmployeeVm.PersonRegistrationPanNoID);
                    
                    StringBuilder logstring = new StringBuilder();                 

                    person.ModifiedDate = DateTime.Now;
                    person.ModifiedBy = User.Identity.Name;
                    new PersonService(_unitOfWork).Update(person);

                    string Divisions = EmployeeVm.DivisionIds;
                    if (Divisions != null)
                    { Divisions = "|" + Divisions.Replace(",", "|,|") + "|"; }

                    businessentity.DivisionIds = Divisions;

                    string Sites = EmployeeVm.SiteIds;
                    if (Sites != null)
                    { Sites = "|" + Sites.Replace(",", "|,|") + "|"; }

                    businessentity.SiteIds = Sites;


                    _BusinessEntityService.Update(businessentity);
                    _EmployeeService.Update(Employee);

                    personaddress.Address = EmployeeVm.Address;
                    personaddress.CityId = EmployeeVm.CityId;
                    personaddress.Zipcode = EmployeeVm.Zipcode;
                    personaddress.ModifiedDate = DateTime.Now;
                    personaddress.ModifiedBy = User.Identity.Name;
                    personaddress.ObjectState = Model.ObjectState.Modified;
                    _PersonAddressService.Update(personaddress);

                    account.LedgerAccountName = person.Name;
                    account.LedgerAccountSuffix = person.Suffix;
                    account.ModifiedDate = DateTime.Now;
                    account.ModifiedBy = User.Identity.Name;
                    _AccountService.Update(account);


                    if (EmployeeVm.ProcessIds != "" && EmployeeVm.ProcessIds != null)
                    {

                        IEnumerable<PersonProcess> personprocesslist = _PersonProcessService.GetPersonProcessList(EmployeeVm.PersonId);

                        foreach (PersonProcess item in personprocesslist)
                        {
                            new PersonProcessService(_unitOfWork).Delete(item.PersonProcessId);
                        }



                        ProcessIdArr = EmployeeVm.ProcessIds.Split(new Char[] { ',' });

                        for (int i = 0; i <= ProcessIdArr.Length - 1; i++)
                        {
                            PersonProcess personprocess = new PersonProcess();
                            personprocess.PersonId = Employee.PersonID;
                            personprocess.ProcessId = Convert.ToInt32(ProcessIdArr[i]);
                            personprocess.CreatedDate = DateTime.Now;
                            personprocess.ModifiedDate = DateTime.Now;
                            personprocess.CreatedBy = User.Identity.Name;
                            personprocess.ModifiedBy = User.Identity.Name;
                            personprocess.ObjectState = Model.ObjectState.Added;
                            _PersonProcessService.Create(personprocess);
                        }
                    }
                 

                    if (EmployeeVm.PanNo != null && EmployeeVm.PanNo != "" )
                    {
                        if (PersonPan != null)
                        {
                            PersonPan.RegistrationNo = EmployeeVm.PanNo;
                            _PersonRegistrationService.Update(PersonPan);
                        }
                        else
                        {
                            PersonRegistration personregistration = new PersonRegistration();
                            personregistration.PersonId = EmployeeVm.PersonId;
                            personregistration.RegistrationType = PersonRegistrationType.PANNo;
                            personregistration.RegistrationNo = EmployeeVm.PanNo;
                            personregistration.CreatedDate = DateTime.Now;
                            personregistration.ModifiedDate = DateTime.Now;
                            personregistration.CreatedBy = User.Identity.Name;
                            personregistration.ModifiedBy = User.Identity.Name;
                            personregistration.ObjectState = Model.ObjectState.Added;
                            _PersonRegistrationService.Create(personregistration);
                        }
                    }



                    ////Saving Activity Log::
                    ActivityLog al = new ActivityLog()
                    {
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocId = EmployeeVm.PersonId,
                        Narration = logstring.ToString(),
                        CreatedDate = DateTime.Now,
                        CreatedBy = User.Identity.Name,
                        //DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.ProcessSequence).DocumentTypeId,

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
                        PrepareViewBag();
                        return View("Create", EmployeeVm);
                    }




                    #region

                    //Saving Image if file is uploaded
                    if (Request.Files[0] != null && Request.Files[0].ContentLength > 0)
                    {
                        string uploadfolder = EmployeeVm.ImageFolderName;
                        string tempfilename = EmployeeVm.ImageFileName;
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

                            string filecontent = Path.Combine(uploadFolder, EmployeeVm.Name + "_" + filename);

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
                                    string tfileName = Path.Combine(tuploadFolder, EmployeeVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, EmployeeVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                }
                            }
                        }
                        var temsave = _PersonService.Find(person.PersonID);
                        person.ImageFileName = temsave.Name+ "_" + filename + temp2;
                        temsave.ImageFolderName = uploadfolder;
                        _PersonService.Update(temsave);
                        _unitOfWork.Save();
                    }

                    #endregion  






                    return RedirectToAction("Index").Success("Data saved successfully");
                }
            }
            PrepareViewBag();
            return View(EmployeeVm);
        }


        public ActionResult Edit(int id)
        {
            EmployeeViewModel bvm = _EmployeeService.GetEmployeeViewModel(id);
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
            Employee Employee = _EmployeeService.GetEmployee(id);
            if (Employee == null)
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
                Employee Employee = _EmployeeService.Find(vm.id);

                ActivityLog al = new ActivityLog()
                {
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    DocId = vm.id,
                    UserRemark = vm.Reason,
                    Narration = "Employee is deleted with Name:" + person.Name,
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.SaleOrder).DocumentTypeId,
                    UploadDate = DateTime.Now,

                };
                new ActivityLogService(_unitOfWork).Create(al);

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


            // Now delete the Parent Employee
                _EmployeeService.Delete(Employee);
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
            Employee Employee = new Employee();
            Employee.PersonID = svm.PersonId;
            _EmployeeService.Create(Employee);

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

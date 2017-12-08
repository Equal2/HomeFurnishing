using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Text;
using System.Configuration;
using System.IO;
using ImageResizer;
using Model.ViewModel;
using Jobs.Helpers;

namespace Jobs.Controllers
{
   [Authorize]
    public class SupplierController : System.Web.Mvc.Controller
    {
       private ApplicationDbContext db = new ApplicationDbContext();
        ISupplierService _SupplierService;
        IBusinessEntityService _BusinessEntityService;
        IPersonService _PersonService;
        IPersonAddressService _PersonAddressService;
        IAccountService _AccountService;
        IPersonProcessService _PersonProcessService;
        IPersonRegistrationService _PersonRegistrationService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public SupplierController(ISupplierService SupplierService, IBusinessEntityService BusinessEntityService, IAccountService AccountService, IPersonService PersonService, IPersonRegistrationService PersonRegistrationService, IPersonAddressService PersonAddressService, IPersonProcessService PersonProcessService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SupplierService = SupplierService;
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
            var Suppliers = _SupplierService.GetSupplierListForIndex();
            return View(Suppliers);
        }

        [HttpGet]
        public ActionResult NextPage(int id, string name)//SupplierId
        {
            var nextId = _SupplierService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, string name)//SupplierId
        {
            var nextId = _SupplierService.PrevId(id);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Supplier);

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
            ViewBag.AccountGroupList = new LedgerAccountGroupService(_unitOfWork).GetLedgerAccountGroupList().ToList();
            ViewBag.TdsCategoryList = new TdsCategoryService(_unitOfWork).GetTdsCategoryList().ToList();
            ViewBag.TdsGroupList = new TdsGroupService(_unitOfWork).GetTdsGroupList().ToList();
            ViewBag.PersonRateGroupList = new PersonRateGroupService(_unitOfWork).GetPersonRateGroupList().ToList();
        }

        public ActionResult Create()
        {
            //SupplierViewModel p = new SupplierViewModel();
            //p.IsActive = true;
            //p.SiteIds = ((int)System.Web.HttpContext.Current.Session["SiteId"]).ToString();
            //p.DivisionIds= ((int)System.Web.HttpContext.Current.Session["DivisionId"]).ToString();
            //var rec=new LedgerAccountGroupService(_unitOfWork).Find(LedgerAccountGroupConstants.SundryCreditors);
            //if(rec!=null)
            //p.LedgerAccountGroupId = rec.LedgerAccountGroupId;
            //p.Code = _PersonService.GetMaxCode();
            //p.ProcessIds = (new ProcessService(_unitOfWork).Find(ProcessConstants.Purchase).ProcessId).ToString();

            //PrepareViewBag();
            //return View("Create", p);

            SupplierViewModel p = new SupplierViewModel();
            p.IsActive = true;
            p.Code = new PersonService(_unitOfWork).GetMaxCode();
            PrepareViewBag();
            return View("Create", p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SupplierViewModel SupplierVm)
        {
            string[] ProcessIdArr;
            if (SupplierVm.LedgerAccountGroupId == 0)
            {
                PrepareViewBag();
                return View(SupplierVm).Danger("Account Group field is required");
            }

            if (_PersonService.CheckDuplicate(SupplierVm.Name, SupplierVm.Suffix, SupplierVm.PersonId) == true)
            {
                PrepareViewBag();
                return View(SupplierVm).Danger("Combination of name and sufix is duplicate");
            }


            if (ModelState.IsValid)
            {
                if (SupplierVm.PersonId == 0)
                {
                    Person person = Mapper.Map<SupplierViewModel, Person>(SupplierVm);
                    BusinessEntity businessentity = Mapper.Map<SupplierViewModel, BusinessEntity>(SupplierVm);
                    Supplier Supp = Mapper.Map<SupplierViewModel, Supplier>(SupplierVm);
                    PersonAddress personaddress = Mapper.Map<SupplierViewModel, PersonAddress>(SupplierVm);
                    LedgerAccount account = Mapper.Map<SupplierViewModel, LedgerAccount>(SupplierVm);

                    person.CreatedDate = DateTime.Now;
                    person.ModifiedDate = DateTime.Now;
                    person.CreatedBy = User.Identity.Name;
                    person.ModifiedBy = User.Identity.Name;
                    person.ObjectState = Model.ObjectState.Added;
                    new PersonService(_unitOfWork).Create(person);


                    string Divisions = SupplierVm.DivisionIds;
                    if (Divisions != null)
                    { Divisions = "|" + Divisions.Replace(",", "|,|") + "|"; }

                    businessentity.DivisionIds = Divisions;

                    string Sites = SupplierVm.SiteIds;
                    if (Sites != null)
                    { Sites = "|" + Sites.Replace(",", "|,|") + "|"; }

                    businessentity.SiteIds = Sites;

                    _BusinessEntityService.Create(businessentity);
                    _SupplierService.Create(Supp);

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

                    if (SupplierVm.ProcessIds != null && SupplierVm.ProcessIds != "")
                    {
                        ProcessIdArr = SupplierVm.ProcessIds.Split(new Char[] { ',' });

                        for (int i = 0; i <= ProcessIdArr.Length - 1; i++)
                        {
                            PersonProcess personprocess = new PersonProcess();
                            personprocess.PersonId = person.PersonID;
                            personprocess.ProcessId = Convert.ToInt32(ProcessIdArr[i]);
                            personprocess.CreatedDate = DateTime.Now;
                            personprocess.ModifiedDate = DateTime.Now;
                            personprocess.CreatedBy = User.Identity.Name;
                            personprocess.ModifiedBy = User.Identity.Name;
                            personprocess.ObjectState = Model.ObjectState.Added;
                            _PersonProcessService.Create(personprocess);
                        }
                    }



                    if (!string.IsNullOrEmpty(SupplierVm.CstNo))
                    {
                        PersonRegistration personregistration = new PersonRegistration();
                        personregistration.RegistrationType = PersonRegistrationType.CstNo;
                        personregistration.RegistrationNo = SupplierVm.CstNo;
                        personregistration.CreatedDate = DateTime.Now;
                        personregistration.ModifiedDate = DateTime.Now;
                        personregistration.CreatedBy = User.Identity.Name;
                        personregistration.ModifiedBy = User.Identity.Name;
                        personregistration.ObjectState = Model.ObjectState.Added;
                        _PersonRegistrationService.Create(personregistration);
                    }

                    if (!string.IsNullOrEmpty(SupplierVm.TinNo))
                    {
                        PersonRegistration personregistration = new PersonRegistration();
                        personregistration.RegistrationType = PersonRegistrationType.TinNo;
                        personregistration.RegistrationNo = SupplierVm.TinNo;
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
                        return View(SupplierVm);

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

                            string filecontent = Path.Combine(uploadFolder, SupplierVm.Name + "_" + filename);

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
                                    string tfileName = Path.Combine(tuploadFolder, SupplierVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, SupplierVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                }

                            }

                            //var tempsave = _FinishedProductService.Find(pt.ProductId);

                            person.ImageFileName = SupplierVm.Name + "_" + filename + temp2;
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
                    Person person = Mapper.Map<SupplierViewModel, Person>(SupplierVm);
                    BusinessEntity businessentity = Mapper.Map<SupplierViewModel, BusinessEntity>(SupplierVm);
                    Supplier Supplier = Mapper.Map<SupplierViewModel, Supplier>(SupplierVm);
                    PersonAddress personaddress = _PersonAddressService.Find(SupplierVm.PersonAddressID);
                    LedgerAccount account = _AccountService.Find(SupplierVm.AccountId);
                    PersonRegistration PersonCst = _PersonRegistrationService.Find(SupplierVm.PersonRegistrationCstNoID);
                    PersonRegistration PersonTin = _PersonRegistrationService.Find(SupplierVm.PersonRegistrationTinNoID);
                    
                    StringBuilder logstring = new StringBuilder();


                    person.ModifiedDate = DateTime.Now;
                    person.ModifiedBy = User.Identity.Name;
                    new PersonService(_unitOfWork).Update(person);



                    string Divisions = SupplierVm.DivisionIds;
                    if (Divisions != null)
                    { Divisions = "|" + Divisions.Replace(",", "|,|") + "|"; }

                    businessentity.DivisionIds = Divisions;

                    string Sites = SupplierVm.SiteIds;
                    if (Sites != null)
                    { Sites = "|" + Sites.Replace(",", "|,|") + "|"; }

                    businessentity.SiteIds = Sites;

                    _BusinessEntityService.Update(businessentity);
                    _SupplierService.Update(Supplier);

                    personaddress.Address = SupplierVm.Address;
                    personaddress.CityId = SupplierVm.CityId;
                    personaddress.Zipcode = SupplierVm.Zipcode;
                    personaddress.ModifiedDate = DateTime.Now;
                    personaddress.ModifiedBy = User.Identity.Name;
                    personaddress.ObjectState = Model.ObjectState.Modified;
                    _PersonAddressService.Update(personaddress);

                    account.LedgerAccountName = person.Name;
                    account.LedgerAccountSuffix = person.Suffix;
                    account.LedgerAccountGroupId = SupplierVm.LedgerAccountGroupId; 
                    account.ModifiedDate = DateTime.Now;
                    account.ModifiedBy = User.Identity.Name;
                    _AccountService.Update(account);


                    if (SupplierVm.ProcessIds != "" && SupplierVm.ProcessIds != null)
                    {

                        IEnumerable<PersonProcess> personprocesslist = _PersonProcessService.GetPersonProcessList(SupplierVm.PersonId);

                        foreach (PersonProcess item in personprocesslist)
                        {
                            new PersonProcessService(_unitOfWork).Delete(item.PersonProcessId);
                        }



                        ProcessIdArr = SupplierVm.ProcessIds.Split(new Char[] { ',' });

                        for (int i = 0; i <= ProcessIdArr.Length - 1; i++)
                        {
                            PersonProcess personprocess = new PersonProcess();
                            personprocess.PersonId = Supplier.PersonID;
                            personprocess.ProcessId = Convert.ToInt32(ProcessIdArr[i]);
                            personprocess.CreatedDate = DateTime.Now;
                            personprocess.ModifiedDate = DateTime.Now;
                            personprocess.CreatedBy = User.Identity.Name;
                            personprocess.ModifiedBy = User.Identity.Name;
                            personprocess.ObjectState = Model.ObjectState.Added;
                            _PersonProcessService.Create(personprocess);
                        }
                    }


                    if (SupplierVm.CstNo != null && SupplierVm.CstNo != "")
                    {
                        if (PersonCst != null)
                        {
                            PersonCst.RegistrationNo = SupplierVm.CstNo;
                            _PersonRegistrationService.Update(PersonCst);
                        }
                        else
                        {
                            PersonRegistration personregistration = new PersonRegistration();
                            personregistration.PersonId = SupplierVm.PersonId;
                            personregistration.RegistrationType = PersonRegistrationType.CstNo;
                            personregistration.RegistrationNo = SupplierVm.CstNo;
                            personregistration.CreatedDate = DateTime.Now;
                            personregistration.ModifiedDate = DateTime.Now;
                            personregistration.CreatedBy = User.Identity.Name;
                            personregistration.ModifiedBy = User.Identity.Name;
                            personregistration.ObjectState = Model.ObjectState.Added;
                            _PersonRegistrationService.Create(personregistration);
                        }
                    }

                    if (SupplierVm.TinNo != null && SupplierVm.TinNo != "")
                    {
                        if (PersonTin != null)
                        {
                            PersonTin.RegistrationNo = SupplierVm.TinNo;
                            _PersonRegistrationService.Update(PersonTin);
                        }
                        else
                        {
                            PersonRegistration personregistration = new PersonRegistration();
                            personregistration.PersonId = SupplierVm.PersonId;
                            personregistration.RegistrationType = PersonRegistrationType.TinNo;
                            personregistration.RegistrationNo = SupplierVm.TinNo;
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
                        DocId = SupplierVm.PersonId,
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
                        return View("Create", SupplierVm);
                    }


                    #region

                    //Saving Image if file is uploaded
                    if (Request.Files[0] != null && Request.Files[0].ContentLength > 0)
                    {
                        string uploadfolder = SupplierVm.ImageFolderName;
                        string tempfilename = SupplierVm.ImageFileName;
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

                            string filecontent = Path.Combine(uploadFolder, SupplierVm.Name + "_" + filename);

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
                                    string tfileName = Path.Combine(tuploadFolder, SupplierVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, SupplierVm.Name + "_" + filename);

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
            return View(SupplierVm);
        }


        public ActionResult Edit(int id)
        {
            SupplierViewModel bvm = _SupplierService.GetSupplierViewModel(id);
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
            Supplier Supplier = _SupplierService.GetSupplier(id);
            if (Supplier == null)
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
                Supplier Supplier = _SupplierService.Find(vm.id);
                
                ActivityLog al = new ActivityLog()
                {
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    DocId = vm.id,
                    UserRemark = vm.Reason,
                    Narration = "Supplier is deleted with Name:" + person.Name,
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

            // Now delete the Parent Supplier
            _SupplierService.Delete(Supplier);
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
            Supplier Supplier = new Supplier();
            Supplier.PersonID = svm.PersonId;
            Supplier.SalesTaxGroupPartyId = null;
            _SupplierService.Create(Supplier);
            
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

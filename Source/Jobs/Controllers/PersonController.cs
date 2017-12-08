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
using Jobs.Constants.ChargeGroupPerson;

namespace Jobs.Controllers
{
   [Authorize]
    public class PersonController : System.Web.Mvc.Controller
    {
       private ApplicationDbContext db = new ApplicationDbContext();
        IBusinessEntityService _BusinessEntityService;
        IPersonService _PersonService;
        IPersonAddressService _PersonAddressService;
        IAccountService _AccountService;
        IPersonProcessService _PersonProcessService;
        IPersonRegistrationService _PersonRegistrationService;
        IPersonRoleService _PersonRoleService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        List<string> UserRoles = new List<string>();

        public PersonController(IPersonService PersonService, IBusinessEntityService BusinessEntityService, IAccountService AccountService, IPersonRegistrationService PersonRegistrationService, IPersonAddressService PersonAddressService, IPersonProcessService PersonProcessService, IPersonRoleService PersonRoleService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PersonService = PersonService;
            _PersonAddressService = PersonAddressService;
            _BusinessEntityService = BusinessEntityService;
            _AccountService = AccountService;
            _PersonProcessService = PersonProcessService;
            _PersonRegistrationService = PersonRegistrationService;
            _PersonRoleService = PersonRoleService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
        }

        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("Index", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }
        // GET: /Order/
        public ActionResult Index(int id)//DocTypeId 
        {
            var Persons = _PersonService.GetPersonListForIndex(id);
            PrepareViewBag(id);
            return View(Persons);
        }

        [HttpGet]
        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevIdWithoutDivisionAndSite(DocId, DocTypeId, User.Identity.Name, "", "Web.People", "PersonID", PrevNextConstants.Next);
            return Edit(nextId, DocTypeId);
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevIdWithoutDivisionAndSite(DocId, DocTypeId, User.Identity.Name, "", "Web.People", "PersonID", PrevNextConstants.Prev);
            return Edit(PrevId, DocTypeId);
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




       public void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;

            List<SelectListItem> AddressType = new List<SelectListItem>();
            AddressType.Add(new SelectListItem { Text = AddressTypeConstants.Permanent, Value = AddressTypeConstants.Permanent });
            AddressType.Add(new SelectListItem { Text = AddressTypeConstants.Temporary, Value = AddressTypeConstants.Temporary });
            AddressType.Add(new SelectListItem { Text = AddressTypeConstants.Work, Value = AddressTypeConstants.Work });
            AddressType.Add(new SelectListItem { Text = AddressTypeConstants.Office, Value = AddressTypeConstants.Office });
            AddressType.Add(new SelectListItem { Text = AddressTypeConstants.Godown, Value = AddressTypeConstants.Godown });

            ViewBag.AddressTypeList = new SelectList(AddressType, "Value", "Text");

        }

       public ActionResult Create(int id)
       {
            PersonViewModel p = new PersonViewModel();
            p.IsActive = true;
            //p.Code = new PersonService(_unitOfWork).GetMaxCode();

            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            p.DocTypeId = id;
            PrepareViewBag(id);

            var settings = new PersonSettingsService(_unitOfWork).GetPersonSettingsForDocument(id);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "PersonSettings", new { id = id }).Warning("Please create Person settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            p.DivisionIds = System.Web.HttpContext.Current.Session["DivisionId"].ToString();
            p.SiteIds = System.Web.HttpContext.Current.Session["SiteId"].ToString();
            p.PersonSettings = Mapper.Map<PersonSettings, PersonSettingsViewModel>(settings);
            p.LedgerAccountGroupId = settings.LedgerAccountGroupId;
            p.Code = new PersonService(_unitOfWork).FGetNewCode(p.DocTypeId, CurrentDivisionId, CurrentSiteId, settings.SqlProcPersonCode);


            return View("Create", p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PersonViewModel PersonVm)
        {
            //string[] ProcessIdArr;
            if (PersonVm.LedgerAccountGroupId == 0)
            {
                PrepareViewBag(PersonVm.DocTypeId);
                return View(PersonVm).Danger("Account Group field is required");
            }

            if (_PersonService.CheckDuplicate(PersonVm.Name, PersonVm.Suffix, PersonVm.PersonID) == true)
            {
                PrepareViewBag(PersonVm.DocTypeId);
                return View(PersonVm).Danger("Combination of name and sufix is duplicate");
            }

            
            var settings = new PersonSettingsService(_unitOfWork).GetPersonSettingsForDocument(PersonVm.DocTypeId);

            if (settings != null)
            {
                if (settings.isVisibleAddress == true && settings.isMandatoryAddress == true && (PersonVm.Address == null || PersonVm.Address == ""))
                {
                    ModelState.AddModelError("Address", "The Address field is required");
                }
                if (settings.isVisibleCity == true && settings.isMandatoryCity == true && (PersonVm.CityId == null || PersonVm.CityId == 0))
                {
                    ModelState.AddModelError("CityId", "The City field is required");
                }
                if (settings.isVisibleZipCode == true && settings.isMandatoryZipCode == true && (PersonVm.Zipcode == null || PersonVm.Zipcode == ""))
                {
                    ModelState.AddModelError("Zipcode", "The Zipcode field is required");
                }
                if (settings.isVisibleMobile == true && settings.isMandatoryMobile == true && (PersonVm.Mobile == null || PersonVm.Mobile == ""))
                {
                    ModelState.AddModelError("Mobile", "The Mobile field is required");
                }
                if (settings.isVisibleEMail == true && settings.isMandatoryEmail == true && (PersonVm.Email == null || PersonVm.Email == ""))
                {
                    ModelState.AddModelError("Email", "The Email field is required");
                }
                if (settings.isVisibleGstNo == true && settings.isMandatoryGstNo == true && (PersonVm.GstNo == null || PersonVm.GstNo == ""))
                {
                    ModelState.AddModelError("GstNo", "The GstNo field is required");
                }
                if (settings.isVisibleCstNo == true && settings.isMandatoryCstNo == true && (PersonVm.CstNo == null || PersonVm.CstNo == ""))
                {
                    ModelState.AddModelError("CstNo", "The CstNo field is required");
                }
                if (settings.isVisibleTinNo == true && settings.isMandatoryTinNo == true && (PersonVm.TinNo == null || PersonVm.TinNo == ""))
                {
                    ModelState.AddModelError("TinNo", "The TinNo field is required");
                }
                if (settings.isVisiblePanNo == true && settings.isMandatoryPanNo == true && (PersonVm.PanNo == null || PersonVm.PanNo == ""))
                {
                    ModelState.AddModelError("PanNo", "The PanNo field is required");
                }
                if (settings.isVisibleAadharNo == true && settings.isMandatoryAadharNo == true && (PersonVm.AadharNo == null || PersonVm.AadharNo == ""))
                {
                    ModelState.AddModelError("AadharNo", "The AadharNo field is required");
                }
                if (settings.isVisibleGuarantor == true && settings.isMandatoryGuarantor == true && (PersonVm.GuarantorId == null || PersonVm.GuarantorId == 0))
                {
                    ModelState.AddModelError("GuarantorId", "The Guarantor field is required");
                }
                if (settings.isVisibleTdsCategory == true && settings.isMandatoryTdsCategory == true && (PersonVm.TdsCategoryId == null || PersonVm.TdsCategoryId == 0))
                {
                    ModelState.AddModelError("TdsCategoryId", "The TdsCategory field is required");
                }
                if (settings.isVisibleTdsGroup == true && settings.isMandatoryTdsGroup == true && (PersonVm.TdsGroupId == null || PersonVm.TdsGroupId == 0))
                {
                    ModelState.AddModelError("TdsGroupId", "The TdsGroup field is required");
                }
                if (settings.isVisibleSalesTaxGroup == true && settings.isMandatorySalesTaxGroup == true && (PersonVm.SalesTaxGroupPartyId == null || PersonVm.SalesTaxGroupPartyId == 0))
                {
                    ModelState.AddModelError("SalesTaxGroupPartyId", "The SalesTaxGroup field is required");
                }
            }


            #region "GST Validation"

            string ChargeGroupPersonName = "";
            var TempChargeGroupPerson = db.ChargeGroupPerson.Find(PersonVm.SalesTaxGroupPartyId);
            if (TempChargeGroupPerson != null)
                ChargeGroupPersonName = TempChargeGroupPerson.ChargeGroupPersonName;


            if (ChargeGroupPersonName == ChargeGroupPersonConstants.StateRegistered.ChargeGroupPersonName ||
                    ChargeGroupPersonName == ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonName ||
                    ChargeGroupPersonName == ChargeGroupPersonConstants.StateComposition.ChargeGroupPersonName)
            {
                if (PersonVm.GstNo == null || PersonVm.GstNo == "")
                {
                    PrepareViewBag(PersonVm.DocTypeId);
                    return View(PersonVm).Danger("GST No. field is required");
                }
            }

            if (PersonVm.GstNo != null && PersonVm.GstNo != "")
            {
                if (PersonVm.SalesTaxGroupPartyId == null)
                {
                    PrepareViewBag(PersonVm.DocTypeId);
                    return View(PersonVm).Danger("Sales Tax Group Person field is required");
                }

                if (ChargeGroupPersonName == ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonName ||
                        ChargeGroupPersonName == ChargeGroupPersonConstants.ExStateUnRegistered.ChargeGroupPersonName)
                {
                    PrepareViewBag(PersonVm.DocTypeId);
                    return View(PersonVm).Danger("Sales Tax Group Person can not be Un-Registered if it has GST No.");
                }

                if (PersonVm.GstNo.Length != 15)
                {
                    PrepareViewBag(PersonVm.DocTypeId);
                    return View(PersonVm).Danger("GST No should be 15 characters long.");
                }

                if (PersonVm.CityId != null)
                {
                    string PersonStateCode = "";
                    var PersonState = (from C in db.City
                                       join S in db.State on C.StateId equals S.StateId into StateTable from StateTab in StateTable.DefaultIfEmpty()
                                       where C.CityId == PersonVm.CityId
                                       select StateTab).FirstOrDefault();
                    if (PersonState != null)
                    {
                        PersonStateCode = PersonState.StateCode;
                    }
                    if (PersonStateCode != "" && PersonStateCode != null)
                    {
                        if (PersonVm.GstNo.Substring(0, 2) != PersonStateCode)
                        {
                            PrepareViewBag(PersonVm.DocTypeId);
                            return View(PersonVm).Danger("First two characters of GST No. should be State Code.");
                        }
                    }
                }
            }
            #endregion


            if (ModelState.IsValid)
            {
                if (PersonVm.PersonID == 0)
                {
                    Person person = Mapper.Map<PersonViewModel, Person>(PersonVm);
                    BusinessEntity businessentity = Mapper.Map<PersonViewModel, BusinessEntity>(PersonVm);
                    PersonAddress personaddress = Mapper.Map<PersonViewModel, PersonAddress>(PersonVm);
                    LedgerAccount account = Mapper.Map<PersonViewModel, LedgerAccount>(PersonVm);

                   
                    person.CreatedDate = DateTime.Now;
                    person.ModifiedDate = DateTime.Now;
                    person.CreatedBy = User.Identity.Name;
                    person.ModifiedBy = User.Identity.Name;
                    person.ObjectState = Model.ObjectState.Added;
                    new PersonService(_unitOfWork).Create(person);

                    int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                    int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                    Site MainSite = new SiteService(_unitOfWork).FindBySiteName("Main");

                    string Divisions = PersonVm.DivisionIds;
                    if (Divisions != null)
                    { 
                        Divisions = "|" + Divisions.Replace(",", "|,|") + "|"; 
                    }
                    else
                    {
                        Divisions = "|" + CurrentDivisionId.ToString() + "|";
                    }

                    businessentity.DivisionIds = Divisions;

                    string Sites = PersonVm.SiteIds;
                    if (Sites != null)
                    {
                        Sites = "|" + Sites.Replace(",", "|,|") + "|";
                    }
                    else
                    {
                        Sites = "|" + CurrentSiteId.ToString() + "|";
                    }

                    businessentity.SiteIds = Sites;

                    _BusinessEntityService.Create(businessentity);


                    personaddress.AddressType = null;
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

                    if (settings.DefaultProcessId != null && settings.DefaultProcessId != 0)
                    {
                        PersonProcess personprocess = new PersonProcess();
                        personprocess.PersonId = person.PersonID;
                        personprocess.ProcessId = (int)settings.DefaultProcessId;
                        personprocess.CreatedDate = DateTime.Now;
                        personprocess.ModifiedDate = DateTime.Now;
                        personprocess.CreatedBy = User.Identity.Name;
                        personprocess.ModifiedBy = User.Identity.Name;
                        personprocess.ObjectState = Model.ObjectState.Added;
                        _PersonProcessService.Create(personprocess);
                    }

                    PersonRole personrole = new PersonRole();
                    personrole.PersonId = person.PersonID;
                    personrole.RoleDocTypeId = person.DocTypeId;
                    personrole.CreatedDate = DateTime.Now;
                    personrole.ModifiedDate = DateTime.Now;
                    personrole.CreatedBy = User.Identity.Name;
                    personrole.ModifiedBy = User.Identity.Name;
                    personrole.ObjectState = Model.ObjectState.Added;
                    _PersonRoleService.Create(personrole);

                    //if (PersonVm.ProcessIds != null &&  PersonVm.ProcessIds != "")
                    //{
                    //    ProcessIdArr = PersonVm.ProcessIds.Split(new Char[] { ',' });

                    //    for (int i = 0; i <= ProcessIdArr.Length - 1; i++)
                    //    {
                    //        PersonProcess personprocess = new PersonProcess();
                    //        personprocess.PersonID = Person.PersonID;
                    //        personprocess.ProcessId = Convert.ToInt32(ProcessIdArr[i]);
                    //        personprocess.CreatedDate = DateTime.Now;
                    //        personprocess.ModifiedDate = DateTime.Now;
                    //        personprocess.CreatedBy = User.Identity.Name;
                    //        personprocess.ModifiedBy = User.Identity.Name;
                    //        personprocess.ObjectState = Model.ObjectState.Added;
                    //        _PersonProcessService.Create(personprocess);
                    //    }
                    //}

                    if (PersonVm.PanNo != "" && PersonVm.PanNo != null)
                    {
                        PersonRegistration personregistration = new PersonRegistration();
                        personregistration.RegistrationType = PersonRegistrationType.PANNo;
                        personregistration.RegistrationNo = PersonVm.PanNo;
                        personregistration.CreatedDate = DateTime.Now;
                        personregistration.ModifiedDate = DateTime.Now;
                        personregistration.CreatedBy = User.Identity.Name;
                        personregistration.ModifiedBy = User.Identity.Name;
                        personregistration.ObjectState = Model.ObjectState.Added;
                        _PersonRegistrationService.Create(personregistration);
                    }

                    if (PersonVm.CstNo != "" && PersonVm.CstNo != null)
                    {
                        PersonRegistration personregistration = new PersonRegistration();
                        personregistration.RegistrationType = PersonRegistrationType.CstNo;
                        personregistration.RegistrationNo = PersonVm.CstNo;
                        personregistration.CreatedDate = DateTime.Now;
                        personregistration.ModifiedDate = DateTime.Now;
                        personregistration.CreatedBy = User.Identity.Name;
                        personregistration.ModifiedBy = User.Identity.Name;
                        personregistration.ObjectState = Model.ObjectState.Added;
                        _PersonRegistrationService.Create(personregistration);
                    }

                    if (PersonVm.GstNo != "" && PersonVm.GstNo != null)
                    {
                        PersonRegistration personregistration = new PersonRegistration();
                        personregistration.RegistrationType = PersonRegistrationType.GstNo;
                        personregistration.RegistrationNo = PersonVm.GstNo;
                        personregistration.CreatedDate = DateTime.Now;
                        personregistration.ModifiedDate = DateTime.Now;
                        personregistration.CreatedBy = User.Identity.Name;
                        personregistration.ModifiedBy = User.Identity.Name;
                        personregistration.ObjectState = Model.ObjectState.Added;
                        _PersonRegistrationService.Create(personregistration);
                    }

                    if (PersonVm.TinNo != "" && PersonVm.TinNo != null)
                    {
                        PersonRegistration personregistration = new PersonRegistration();
                        personregistration.RegistrationType = PersonRegistrationType.TinNo;
                        personregistration.RegistrationNo = PersonVm.TinNo;
                        personregistration.CreatedDate = DateTime.Now;
                        personregistration.ModifiedDate = DateTime.Now;
                        personregistration.CreatedBy = User.Identity.Name;
                        personregistration.ModifiedBy = User.Identity.Name;
                        personregistration.ObjectState = Model.ObjectState.Added;
                        _PersonRegistrationService.Create(personregistration);
                    }


                    if (PersonVm.AadharNo != "" && PersonVm.AadharNo != null)
                    {
                        PersonRegistration personregistration = new PersonRegistration();
                        personregistration.RegistrationType = PersonRegistrationType.AadharNo;
                        personregistration.RegistrationNo = PersonVm.AadharNo;
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
                        PrepareViewBag(PersonVm.DocTypeId);
                        return View(PersonVm);

                    }




                    #region "image saving"

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

                            string filecontent = Path.Combine(uploadFolder, PersonVm.Name + "_" + filename);

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
                                    string tfileName = Path.Combine(tuploadFolder, PersonVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, PersonVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                                }

                            }

                            //var tempsave = _FinishedProductService.Find(pt.ProductId);

                            person.ImageFileName = PersonVm.Name + "_" + filename + temp2;
                            person.ImageFolderName = uploadfolder;
                            person.ObjectState = Model.ObjectState.Modified;
                            _PersonService.Update(person);
                            _unitOfWork.Save();
                        }

                    }

                    #endregion
         





                    //return RedirectToAction("Create").Success("Data saved successfully");
                    return RedirectToAction("Edit", new { id = person.PersonID, DocTypeId = PersonVm.DocTypeId }).Success("Data saved Successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    //string tempredirect = (Request["Redirect"].ToString());
                    Person person = Mapper.Map<PersonViewModel, Person>(PersonVm);
                    BusinessEntity businessentity = Mapper.Map<PersonViewModel, BusinessEntity>(PersonVm);
                    PersonAddress personaddress = _PersonAddressService.Find(PersonVm.PersonAddressID);
                    LedgerAccount account = _AccountService.Find(PersonVm.AccountId);
                    PersonRegistration PersonPan = _PersonRegistrationService.Find(PersonVm.PersonRegistrationPanNoID ?? 0);
                    PersonRegistration PersonCst = _PersonRegistrationService.Find(PersonVm.PersonRegistrationCstNoID ?? 0);
                    PersonRegistration PersonGst = _PersonRegistrationService.Find(PersonVm.PersonRegistrationGstNoID ?? 0);
                    PersonRegistration PersonTin = _PersonRegistrationService.Find(PersonVm.PersonRegistrationTinNoID ?? 0);
                    PersonRegistration PersonAadhar = _PersonRegistrationService.Find(PersonVm.PersonRegistrationAadharNoID ?? 0);


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


                    int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                    int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];


                    string Divisions = PersonVm.DivisionIds;
                    if (Divisions != null)
                    { 
                        Divisions = "|" + Divisions.Replace(",", "|,|") + "|"; 
                    }
                    else
                    {
                        Divisions = "|" + CurrentDivisionId.ToString() + "|";
                    }

                    businessentity.DivisionIds = Divisions;

                    string Sites = PersonVm.SiteIds;
                    if (Sites != null)
                    { 
                        Sites = "|" + Sites.Replace(",", "|,|") + "|"; 
                    }
                    else
                    {
                        Sites = "|" + CurrentSiteId.ToString() + "|";
                    }

                    businessentity.SiteIds = Sites;

                    _BusinessEntityService.Update(businessentity);

                    personaddress.Address = PersonVm.Address;
                    personaddress.CityId = PersonVm.CityId;
                    personaddress.Zipcode = PersonVm.Zipcode;
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
                    account.LedgerAccountGroupId = PersonVm.LedgerAccountGroupId; 
                    account.ModifiedDate = DateTime.Now;
                    account.ModifiedBy = User.Identity.Name;
                    _AccountService.Update(account);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecLA,
                        Obj = account,
                    });


                    if (settings.DefaultProcessId != null && settings.DefaultProcessId != 0)
                    {
                        var temp = (from p in db.PersonProcess where p.PersonId == person.PersonID && p.ProcessId == settings.DefaultProcessId select p).FirstOrDefault();

                        if (temp == null)
                        {
                            PersonProcess personprocess = new PersonProcess();
                            personprocess.PersonId = person.PersonID;
                            personprocess.ProcessId = (int)settings.DefaultProcessId;
                            personprocess.CreatedDate = DateTime.Now;
                            personprocess.ModifiedDate = DateTime.Now;
                            personprocess.CreatedBy = User.Identity.Name;
                            personprocess.ModifiedBy = User.Identity.Name;
                            personprocess.ObjectState = Model.ObjectState.Added;
                            _PersonProcessService.Create(personprocess);
                        }
                    }

                    //if (PersonVm.ProcessIds != "" && PersonVm.ProcessIds != null)
                    //{

                    //    IEnumerable<PersonProcess> personprocesslist = _PersonProcessService.GetPersonProcessList(PersonVm.PersonID);

                    //    foreach (PersonProcess item in personprocesslist)
                    //    {
                    //        new PersonProcessService(_unitOfWork).Delete(item.PersonProcessId);
                    //    }


                        
                    //    ProcessIdArr = PersonVm.ProcessIds.Split(new Char[] { ',' });

                    //    for (int i = 0; i <= ProcessIdArr.Length - 1; i++)
                    //    {
                    //        PersonProcess personprocess = new PersonProcess();
                    //        personprocess.PersonID = Person.PersonID;
                    //        personprocess.ProcessId = Convert.ToInt32(ProcessIdArr[i]);
                    //        personprocess.CreatedDate = DateTime.Now;
                    //        personprocess.ModifiedDate = DateTime.Now;
                    //        personprocess.CreatedBy = User.Identity.Name;
                    //        personprocess.ModifiedBy = User.Identity.Name;
                    //        personprocess.ObjectState = Model.ObjectState.Added;
                    //        _PersonProcessService.Create(personprocess);
                    //    }
                    //}

                    if (PersonVm.PanNo != null && PersonVm.PanNo != "" )
                    {
                        if (PersonPan != null)
                        {
                            PersonPan.RegistrationNo = PersonVm.PanNo;
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
                            personregistration.PersonId = PersonVm.PersonID;
                            personregistration.RegistrationType = PersonRegistrationType.PANNo;
                            personregistration.RegistrationNo = PersonVm.PanNo;
                            personregistration.CreatedDate = DateTime.Now;
                            personregistration.ModifiedDate = DateTime.Now;
                            personregistration.CreatedBy = User.Identity.Name;
                            personregistration.ModifiedBy = User.Identity.Name;
                            personregistration.ObjectState = Model.ObjectState.Added;
                            _PersonRegistrationService.Create(personregistration);
                        }
                    }


                    if (PersonVm.CstNo != null && PersonVm.CstNo != "")
                    {
                        if (PersonCst != null)
                        {
                            PersonCst.RegistrationNo = PersonVm.CstNo;
                            _PersonRegistrationService.Update(PersonCst);

                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRecP,
                                Obj = PersonCst,
                            });

                        }
                        else
                        {
                            PersonRegistration personregistration = new PersonRegistration();
                            personregistration.PersonId = PersonVm.PersonID;
                            personregistration.RegistrationType = PersonRegistrationType.CstNo;
                            personregistration.RegistrationNo = PersonVm.CstNo;
                            personregistration.CreatedDate = DateTime.Now;
                            personregistration.ModifiedDate = DateTime.Now;
                            personregistration.CreatedBy = User.Identity.Name;
                            personregistration.ModifiedBy = User.Identity.Name;
                            personregistration.ObjectState = Model.ObjectState.Added;
                            _PersonRegistrationService.Create(personregistration);
                        }
                    }




                    //if (PersonVm.GstNo != null && PersonVm.GstNo != "")
                    //{
                        if (PersonGst != null)
                        {
                            PersonGst.RegistrationNo = PersonVm.GstNo;
                            _PersonRegistrationService.Update(PersonGst);

                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRecP,
                                Obj = PersonGst,
                            });

                        }
                        else
                        {
                            PersonRegistration personregistration = new PersonRegistration();
                            personregistration.PersonId = PersonVm.PersonID;
                            personregistration.RegistrationType = PersonRegistrationType.GstNo;
                            personregistration.RegistrationNo = PersonVm.GstNo;
                            personregistration.CreatedDate = DateTime.Now;
                            personregistration.ModifiedDate = DateTime.Now;
                            personregistration.CreatedBy = User.Identity.Name;
                            personregistration.ModifiedBy = User.Identity.Name;
                            personregistration.ObjectState = Model.ObjectState.Added;
                            _PersonRegistrationService.Create(personregistration);
                        }
                    //}




                    if (PersonVm.TinNo != null && PersonVm.TinNo != "")
                    {
                        if (PersonTin != null)
                        {
                            PersonTin.RegistrationNo = PersonVm.TinNo;
                            _PersonRegistrationService.Update(PersonTin);

                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRecP,
                                Obj = PersonTin,
                            });

                        }
                        else
                        {
                            PersonRegistration personregistration = new PersonRegistration();
                            personregistration.PersonId = PersonVm.PersonID;
                            personregistration.RegistrationType = PersonRegistrationType.TinNo;
                            personregistration.RegistrationNo = PersonVm.TinNo;
                            personregistration.CreatedDate = DateTime.Now;
                            personregistration.ModifiedDate = DateTime.Now;
                            personregistration.CreatedBy = User.Identity.Name;
                            personregistration.ModifiedBy = User.Identity.Name;
                            personregistration.ObjectState = Model.ObjectState.Added;
                            _PersonRegistrationService.Create(personregistration);
                        }
                    }




                    if (PersonVm.AadharNo != null && PersonVm.AadharNo != "")
                    {
                        if (PersonTin != null)
                        {
                            PersonTin.RegistrationNo = PersonVm.AadharNo;
                            _PersonRegistrationService.Update(PersonTin);

                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRecP,
                                Obj = PersonTin,
                            });

                        }
                        else
                        {
                            PersonRegistration personregistration = new PersonRegistration();
                            personregistration.PersonId = PersonVm.PersonID;
                            personregistration.RegistrationType = PersonRegistrationType.AadharNo;
                            personregistration.RegistrationNo = PersonVm.AadharNo;
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
                        PrepareViewBag(PersonVm.DocTypeId);
                        return View("Create", PersonVm);
                    }

                    LogActivity.LogActivityDetail(new ActiivtyLogViewModel
                    {
                        DocTypeId = PersonVm.DocTypeId,
                        DocId = PersonVm.PersonID,
                        ActivityType = (int)ActivityTypeContants.Modified,                       
                        DocNo = PersonVm.Name,
                        xEModifications = Modifications,                        
                    });
                    //End of Saving ActivityLog


                    #region

                    //Saving Image if file is uploaded
                    if (Request.Files[0] != null && Request.Files[0].ContentLength > 0)
                    {
                        string uploadfolder = PersonVm.ImageFolderName;
                        string tempfilename = PersonVm.ImageFileName;
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

                            string filecontent = Path.Combine(uploadFolder, PersonVm.Name + "_" + filename);

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
                                    string tfileName = Path.Combine(tuploadFolder, PersonVm.Name + "_" + filename);

                                    //Let the image builder add the correct extension based on the output file type
                                    //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                    ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                                }
                                else if (suffix == "_medium")
                                {
                                    string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                    if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                    //Generate a filename (GUIDs are best).
                                    string tfileName = Path.Combine(tuploadFolder, PersonVm.Name + "_" + filename);

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



                    return RedirectToAction("Index", new { id = PersonVm.DocTypeId }).Success("Data saved successfully");
                }
            }
            PrepareViewBag(PersonVm.DocTypeId);
            return View(PersonVm);
        }


        public ActionResult Edit(int id, int DocTypeId)
        {
            PersonViewModel bvm = _PersonService.GetPersonViewModelForEdit(id);
            bvm.DocTypeId = DocTypeId;
            PrepareViewBag(DocTypeId);
            if (bvm == null)
            {
                return HttpNotFound();
            }

            var settings = new PersonSettingsService(_unitOfWork).GetPersonSettingsForDocument(DocTypeId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "PersonSettings", new { id = DocTypeId }).Warning("Please create Person settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            bvm.PersonSettings = Mapper.Map<PersonSettings, PersonSettingsViewModel>(settings);

            return View("Create", bvm);
        }


        // GET: /ProductMaster/Delete/5
        
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person Person = _PersonService.GetPerson(id);
            if (Person == null)
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
                    ExObj = person,
                });

                #region "For Deleting Opening"
                int OpendingDocTypeId = 0;
                var OpendingDocType = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.Opening);
                if (OpendingDocType != null)
                    OpendingDocTypeId = OpendingDocType.DocumentTypeId;

                LedgerAccount PersonLedgerAccount = _AccountService.GetLedgerAccountFromPersonId(vm.id);

                var LedgerLineList = (from L in db.LedgerLine
                                      where L.LedgerAccountId == PersonLedgerAccount.LedgerAccountId
                                      && L.LedgerHeader.DocTypeId == OpendingDocTypeId
                                      select L).ToList();

                foreach (var LedgerLine in LedgerLineList)
                {
                    int LedgerHeaderId = LedgerLine.LedgerHeaderId;

                    IEnumerable<Ledger> LedgerList = (from L in db.Ledger where L.LedgerLineId == LedgerLine.LedgerLineId select L).ToList();
                    foreach (var Ledger in LedgerList)
                    {
                        new LedgerService(_unitOfWork).Delete(Ledger.LedgerId);
                    }
                    new LedgerLineService(_unitOfWork).Delete(LedgerLine.LedgerLineId);

                    var OtherLedgerLinesForLedgerHeader = (from L in db.LedgerLine where L.LedgerHeaderId == LedgerHeaderId && L.LedgerLineId != LedgerLine.LedgerLineId select L).FirstOrDefault();
                    if (OtherLedgerLinesForLedgerHeader == null)
                        new LedgerHeaderService(_unitOfWork).Delete(LedgerHeaderId);
                }
                #endregion


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


                IEnumerable<PersonRole> personRole = new PersonRoleService(_unitOfWork).GetPersonRoleIdListByPersonId(vm.id);
                //Mark ObjectState.Delete to all the Person Role For Above Person. 
                foreach (PersonRole item in personRole)
                {
                    new PersonRoleService(_unitOfWork).Delete(item.PersonRoleId);
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



                



            // Now delete the Parent Person
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
                DocTypeId = person.DocTypeId,
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
        public ActionResult AddToExisting(int id)//DocTypeId
        {
            AddToExistingContactViewModel vm = new AddToExistingContactViewModel();
            vm.DocTypeId = id;
            return PartialView("AddToExisting", vm);
        }

        [HttpPost]
        public ActionResult AddToExisting(AddToExistingContactViewModel svm)
        {
            PersonRole PersonRole = new PersonRole();
            PersonRole.PersonId = svm.PersonId;
            PersonRole.RoleDocTypeId = svm.DocTypeId;
            PersonRole.CreatedDate = DateTime.Now;
            PersonRole.ModifiedDate = DateTime.Now;
            PersonRole.CreatedBy = User.Identity.Name;
            PersonRole.ModifiedBy = User.Identity.Name;
            PersonRole.ObjectState = Model.ObjectState.Added;
            _PersonRoleService.Create(PersonRole);

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
        public ActionResult ChooseContactType(int id)//DocTypeId
        {
            PrepareViewBag(id);
            return PartialView("ChooseContactType");
        }

       public JsonResult GetCustom(string searchTerm, int pageSize, int pageNum, int filter)
       {
           var Query = (from p in db.Persons
                        join Pr in db.PersonRole on p.PersonID equals Pr.PersonId into PersonRoleTable from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (p.Name.ToLower().Contains(searchTerm.ToLower())))
                        && PersonRoleTab.RoleDocTypeId != filter
                        orderby p.Name
                        select new ComboBoxResult
                        {
                            text = p.Name ,
                            id = p.PersonID.ToString(),
                        });

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

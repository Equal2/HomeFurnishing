using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Model.ViewModel;
using System.Xml.Linq;
using Reports.Controllers;
using Model.ViewModels;

namespace Jobs.Controllers
{

    [Authorize]
    public class PersonCreationController : System.Web.Mvc.Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        


        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        public PersonCreationController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }






        public ActionResult _Create(int? id, int DocTypeId) 
        {
            PersonViewModel p = new PersonViewModel();
            p.IsActive = true;
            p.DocTypeId = DocTypeId;
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(DocTypeId).DocumentTypeName;

            if (id != null && id !=0)
            {
                p = GetPersonViewModel((int)id);
            }
            else
            {
                p.Code = new PersonService(_unitOfWork).GetMaxCode();
                p.Suffix = new PersonService(_unitOfWork).GetMaxCode();
            }

            var settings = new PersonSettingsService(_unitOfWork).GetPersonSettingsForDocument(DocTypeId);

            p.DivisionIds = System.Web.HttpContext.Current.Session["DivisionId"].ToString();
            p.SiteIds = System.Web.HttpContext.Current.Session["SiteId"].ToString();
            p.LedgerAccountGroupId = settings.LedgerAccountGroupId;

            return PartialView("_Create", p);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PersonViewModel PersonVm)
        {
            if (ModelState.IsValid)
            {
                if (PersonVm.PersonID == 0)
                {
                    Person person = Mapper.Map<PersonViewModel, Person>(PersonVm);
                    BusinessEntity businessentity = Mapper.Map<PersonViewModel, BusinessEntity>(PersonVm);
                    PersonAddress personaddress = Mapper.Map<PersonViewModel, PersonAddress>(PersonVm);
                    LedgerAccount account = Mapper.Map<PersonViewModel, LedgerAccount>(PersonVm);

                    person.IsActive = true;
                    person.CreatedDate = DateTime.Now;
                    person.ModifiedDate = DateTime.Now;
                    person.CreatedBy = User.Identity.Name;
                    person.ModifiedBy = User.Identity.Name;
                    person.ObjectState = Model.ObjectState.Added;
                    new PersonService(_unitOfWork).Create(person);


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


                    new  BusinessEntityService(_unitOfWork).Create(businessentity);

                    personaddress.AddressType = null;
                    personaddress.CreatedDate = DateTime.Now;
                    personaddress.ModifiedDate = DateTime.Now;
                    personaddress.CreatedBy = User.Identity.Name;
                    personaddress.ModifiedBy = User.Identity.Name;
                    personaddress.ObjectState = Model.ObjectState.Added;
                    new PersonAddressService(_unitOfWork).Create(personaddress);


                    account.LedgerAccountId = db.LedgerAccount.Max(i => i.LedgerAccountId) + 1;
                    account.LedgerAccountName = person.Name;
                    account.LedgerAccountSuffix = person.Suffix;
                    account.LedgerAccountGroupId = PersonVm.LedgerAccountGroupId;
                    account.IsActive = true;
                    account.CreatedDate = DateTime.Now;
                    account.ModifiedDate = DateTime.Now;
                    account.CreatedBy = User.Identity.Name;
                    account.ModifiedBy = User.Identity.Name;
                    account.ObjectState = Model.ObjectState.Added;
                    new LedgerAccountService(_unitOfWork).Create(account);


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
                        new PersonRegistrationService(_unitOfWork).Create(personregistration);
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
                        new PersonRegistrationService(_unitOfWork).Create(personregistration);
                    }

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
                        new PersonRegistrationService(_unitOfWork).Create(personregistration);
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
                        new PersonRegistrationService(_unitOfWork).Create(personregistration);
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
                        new PersonRegistrationService(_unitOfWork).Create(personregistration);
                    }


                    PersonRole personrole = new PersonRole();
                    personrole.PersonRoleId = -1;
                    personrole.PersonId = person.PersonID;
                    personrole.RoleDocTypeId = person.DocTypeId;
                    personrole.CreatedDate = DateTime.Now;
                    personrole.ModifiedDate = DateTime.Now;
                    personrole.CreatedBy = User.Identity.Name;
                    personrole.ModifiedBy = User.Identity.Name;
                    personrole.ObjectState = Model.ObjectState.Added;
                    new PersonRoleService(_unitOfWork).Create(personrole);

                    int ProspectDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.Prospect).DocumentTypeId;
                    if (person.DocTypeId == ProspectDocTypeId)
                    {
                        int CustomerDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.Customer).DocumentTypeId;

                        PersonRole personrole1 = new PersonRole();
                        personrole.PersonRoleId = -2;
                        personrole1.PersonId = person.PersonID;
                        personrole1.RoleDocTypeId = CustomerDocTypeId;
                        personrole1.CreatedDate = DateTime.Now;
                        personrole1.ModifiedDate = DateTime.Now;
                        personrole1.CreatedBy = User.Identity.Name;
                        personrole1.ModifiedBy = User.Identity.Name;
                        personrole1.ObjectState = Model.ObjectState.Added;
                        new PersonRoleService(_unitOfWork).Create(personrole1);
                    }


                    int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Sales).ProcessId;

                    PersonProcess personprocess = new PersonProcess();
                    personprocess.PersonId = person.PersonID;
                    personprocess.ProcessId = ProcessId;
                    personprocess.CreatedDate = DateTime.Now;
                    personprocess.ModifiedDate = DateTime.Now;
                    personprocess.CreatedBy = User.Identity.Name;
                    personprocess.ModifiedBy = User.Identity.Name;
                    personprocess.ObjectState = Model.ObjectState.Added;
                    new PersonProcessService(_unitOfWork).Create(personprocess);


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View(PersonVm);

                    }

                    return Json(new { success = true, PersonId = person.PersonID, Name = person.Name + ", " + person.Suffix + " [" + person.Code + "]" });
                    
                }
                else
                {
                    //string tempredirect = (Request["Redirect"].ToString());
                    Person person = Mapper.Map<PersonViewModel, Person>(PersonVm);
                    BusinessEntity businessentity = Mapper.Map<PersonViewModel, BusinessEntity>(PersonVm);
                    PersonAddress personaddress = new PersonAddressService(_unitOfWork).Find(PersonVm.PersonAddressID);
                    LedgerAccount account = new LedgerAccountService(_unitOfWork).Find(PersonVm.AccountId);
                    PersonRegistration PersonCst = new PersonRegistrationService(_unitOfWork).Find(PersonVm.PersonRegistrationCstNoID ?? 0);
                    PersonRegistration PersonTin = new PersonRegistrationService(_unitOfWork).Find(PersonVm.PersonRegistrationTinNoID ?? 0);
                    PersonRegistration PersonPAN = new PersonRegistrationService(_unitOfWork).Find(PersonVm.PersonRegistrationPanNoID ?? 0);
                    PersonRegistration PersonGst = new PersonRegistrationService(_unitOfWork).Find(PersonVm.PersonRegistrationGstNoID ?? 0);
                    PersonRegistration PersonAadhar = new PersonRegistrationService(_unitOfWork).Find(PersonVm.PersonRegistrationAadharNoID ?? 0);
                    


                    person.IsActive = true;
                    person.ModifiedDate = DateTime.Now;
                    person.ModifiedBy = User.Identity.Name;
                    new PersonService(_unitOfWork).Update(person);


                    new BusinessEntityService(_unitOfWork).Update(businessentity);

                    personaddress.Address = PersonVm.Address;
                    personaddress.CityId = PersonVm.CityId;
                    personaddress.Zipcode = PersonVm.Zipcode;
                    personaddress.ModifiedDate = DateTime.Now;
                    personaddress.ModifiedBy = User.Identity.Name;
                    personaddress.ObjectState = Model.ObjectState.Modified;
                    new PersonAddressService(_unitOfWork).Update(personaddress);

                    
                    account.LedgerAccountName = person.Name;
                    account.LedgerAccountSuffix = person.Suffix;
                    account.ModifiedDate = DateTime.Now;
                    account.ModifiedBy = User.Identity.Name;
                    new LedgerAccountService(_unitOfWork).Update(account);

                    if (PersonVm.CstNo != null && PersonVm.CstNo != "")
                    {
                        if (PersonCst != null)
                        {
                            PersonCst.RegistrationNo = PersonVm.CstNo;
                            new PersonRegistrationService(_unitOfWork).Update(PersonCst);
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
                            new PersonRegistrationService(_unitOfWork).Create(personregistration);
                        }
                    }

                    if (PersonVm.TinNo != null && PersonVm.TinNo != "")
                    {
                        if (PersonTin != null)
                        {
                            PersonTin.RegistrationNo = PersonVm.TinNo;
                            new PersonRegistrationService(_unitOfWork).Update(PersonTin);
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
                            new PersonRegistrationService(_unitOfWork).Create(personregistration);
                        }
                    }

                    if (PersonVm.PanNo != null && PersonVm.PanNo != "")
                    {
                        if (PersonPAN != null)
                        {
                            PersonPAN.RegistrationNo = PersonVm.PanNo;
                            new PersonRegistrationService(_unitOfWork).Update(PersonPAN);
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
                            new PersonRegistrationService(_unitOfWork).Create(personregistration);
                        }
                    }

                    if (PersonVm.GstNo != null && PersonVm.GstNo != "")
                    {
                        if (PersonGst != null)
                        {
                            PersonGst.RegistrationNo = PersonVm.GstNo;
                            new PersonRegistrationService(_unitOfWork).Update(PersonGst);
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
                            new PersonRegistrationService(_unitOfWork).Create(personregistration);
                        }
                    }

                    if (PersonVm.AadharNo != null && PersonVm.AadharNo != "")
                    {
                        if (PersonAadhar != null)
                        {
                            PersonAadhar.RegistrationNo = PersonVm.AadharNo;
                            new PersonRegistrationService(_unitOfWork).Update(PersonAadhar);
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
                            new PersonRegistrationService(_unitOfWork).Create(personregistration);
                        }
                    }


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", PersonVm);
                    }

                    return Json(new { success = true, PersonId = person.PersonID, Name = person.Name + ", " + person.Suffix + " [" + person.Code + "]" });
                }
            }
            return View(PersonVm);

        }



        public JsonResult GetEMailDetailJson(string Email)
        {
            var CustomerDetail = (from p in db.Persons
                                  where p.Email == Email
                                  select new 
                                  {
                                      PersonId = p.PersonID
                                  }).FirstOrDefault();

            PersonViewModel vm = null;
            List<PersonViewModel> PersonViewModelJson = new List<PersonViewModel>();
            if (CustomerDetail != null)
            {
                vm = GetPersonViewModel(CustomerDetail.PersonId);
                PersonViewModelJson.Add(vm);
                return Json(PersonViewModelJson);
            }
            else
            {
                return null;
            }
        }


        public PersonViewModel GetPersonViewModel(int id)
        {
            PersonViewModel PersonViewModel = (from bus in db.BusinessEntity
                                             join p in db.Persons on bus.PersonID equals p.PersonID into PersonTable
                                             from PersonTab in PersonTable.DefaultIfEmpty()
                                             join pa in db.PersonAddress on PersonTab.PersonID equals pa.PersonId into PersonAddressTable
                                             from PersonAddressTab in PersonAddressTable.DefaultIfEmpty()
                                             join ac in db.LedgerAccount on PersonTab.PersonID equals ac.PersonId into AccountTable
                                             from AccountTab in AccountTable.DefaultIfEmpty()
                                             where PersonTab.PersonID == id
                                              select new PersonViewModel
                                             {
                                                 PersonID = PersonTab.PersonID,
                                                 Name = PersonTab.Name,
                                                 Suffix = PersonTab.Suffix,
                                                 Code = PersonTab.Code,
                                                 Phone = PersonTab.Phone,
                                                 Mobile = PersonTab.Mobile,
                                                 Email = PersonTab.Email,
                                                 Address = PersonAddressTab.Address,
                                                 CityId = PersonAddressTab.CityId,
                                                 CityName = PersonAddressTab.City.CityName,
                                                 Zipcode = PersonAddressTab.Zipcode,
                                                 PersonRateGroupId = bus.PersonRateGroupId,
                                                 CreaditDays = bus.CreaditDays,
                                                 CreaditLimit = bus.CreaditLimit,
                                                 IsActive = PersonTab.IsActive,
                                                 SalesTaxGroupPartyId = bus.SalesTaxGroupPartyId,
                                                 CreatedBy = PersonTab.CreatedBy,
                                                 CreatedDate = PersonTab.CreatedDate,
                                                 SiteIds = bus.SiteIds,
                                                 Tags = PersonTab.Tags,
                                                 ImageFileName = PersonTab.ImageFileName,
                                                 ImageFolderName = PersonTab.ImageFolderName,
                                                 IsSisterConcern = (bool?)bus.IsSisterConcern ?? false,
                                                 AccountId = (int?)AccountTab.LedgerAccountId ?? 0,
                                                 PersonAddressID = (PersonAddressTab == null ? 0 : PersonAddressTab.PersonAddressID),
                                                 LedgerAccountGroupId = (int?)AccountTab.LedgerAccountGroupId ?? 0,
                                             }).FirstOrDefault();

            var PersonRegistration = (from pp in db.PersonRegistration
                                      where pp.PersonId == id
                                      select new
                                      {
                                          PersonRegistrationId = pp.PersonRegistrationID,
                                          RregistrationType = pp.RegistrationType,
                                          RregistrationNo = pp.RegistrationNo
                                      }).ToList();

            if (PersonRegistration != null)
            {
                foreach (var item in PersonRegistration)
                {
                    if (item.RregistrationType == PersonRegistrationType.CstNo)
                    {
                        PersonViewModel.PersonRegistrationCstNoID = item.PersonRegistrationId;
                        PersonViewModel.CstNo = item.RregistrationNo;
                    }

                    if (item.RregistrationType == PersonRegistrationType.TinNo)
                    {
                        PersonViewModel.PersonRegistrationTinNoID = item.PersonRegistrationId;
                        PersonViewModel.TinNo = item.RregistrationNo;
                    }

                    if (item.RregistrationType == PersonRegistrationType.PANNo)
                    {
                        PersonViewModel.PersonRegistrationPanNoID = item.PersonRegistrationId;
                        PersonViewModel.PanNo = item.RregistrationNo;
                    }

                    if (item.RregistrationType == PersonRegistrationType.GstNo)
                    {
                        PersonViewModel.PersonRegistrationGstNoID = item.PersonRegistrationId;
                        PersonViewModel.GstNo = item.RregistrationNo;
                    }

                    if (item.RregistrationType == PersonRegistrationType.AadharNo)
                    {
                        PersonViewModel.PersonRegistrationAadharNoID = item.PersonRegistrationId;
                        PersonViewModel.AadharNo = item.RregistrationNo;
                    }

                }
            }

            return PersonViewModel;
        }

    }
}

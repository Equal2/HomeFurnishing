using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using Core.Common;
using Model.ViewModel;
using System.Xml.Linq;
using AutoMapper;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class PersonSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IPersonSettingsService _PersonSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public PersonSettingsController(IPersonSettingsService PersonSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PersonSettingsService = PersonSettingsService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }

        private void PrepareViewBag(PersonSettingsViewModel s)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];



            ViewBag.id = s.DocTypeId;



        }


        // GET: /PersonSettingMaster/Create

        public ActionResult Create(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var settings = new PersonSettingsService(_unitOfWork).GetPersonSettings(id);

            if (settings == null)
            {
                PersonSettingsViewModel vm = new PersonSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.DocTypeId = id;
                PrepareViewBag(vm);
                return View("Create", vm);
            }
            else
            {
                PersonSettingsViewModel temp = AutoMapper.Mapper.Map<PersonSettings, PersonSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                PrepareViewBag(temp);
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(PersonSettingsViewModel vm)
        {
            PersonSettings pt = AutoMapper.Mapper.Map<PersonSettingsViewModel, PersonSettings>(vm);

            if (pt.LedgerAccountGroupId <= 0)
                ModelState.AddModelError("AccountGroupId", "Account Group field is required");

            if (ModelState.IsValid)
            {

                if (vm.PersonSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _PersonSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm);
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.PersonSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));



                    return RedirectToAction("Index", "Person", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PersonSettings temp = _PersonSettingsService.Find(pt.PersonSettingsId);

                    PersonSettings ExRec = Mapper.Map<PersonSettings>(temp);

                    temp.isVisibleAddress = pt.isVisibleAddress;
                    temp.isVisibleCity = pt.isVisibleCity;
                    temp.isVisibleZipCode = pt.isVisibleZipCode;
                    temp.isVisiblePhone = pt.isVisiblePhone;
                    temp.isVisibleMobile = pt.isVisibleMobile;
                    temp.isVisibleEMail = pt.isVisibleEMail;
                    temp.isVisibleGstNo = pt.isVisibleGstNo;
                    temp.isVisibleCstNo = pt.isVisibleCstNo;
                    temp.isVisibleTinNo = pt.isVisibleTinNo;
                    temp.isVisiblePanNo = pt.isVisiblePanNo;
                    temp.isVisibleAadharNo = pt.isVisibleAadharNo;
                    temp.isVisibleSalesTaxGroup = pt.isVisibleSalesTaxGroup;
                    temp.isVisibleGuarantor = pt.isVisibleGuarantor;
                    temp.isVisibleParent = pt.isVisibleParent;
                    temp.isVisibleTdsCategory = pt.isVisibleTdsCategory;
                    temp.isVisibleTdsGroup = pt.isVisibleTdsGroup;
                    temp.isVisibleCreditLimit = pt.isVisibleCreditLimit;
                    temp.isVisibleCreditDays = pt.isVisibleCreditDays;
                    temp.isVisibleWorkInDivision = pt.isVisibleWorkInDivision;
                    temp.isVisibleWorkInBranch = pt.isVisibleWorkInBranch;
                    temp.isVisibleTags = pt.isVisibleTags;
                    temp.isVisibleIsSisterConcern = pt.isVisibleIsSisterConcern;
                    temp.isVisibleContactPersonDetail = pt.isVisibleContactPersonDetail;
                    temp.isVisibleBankAccountDetail = pt.isVisibleBankAccountDetail;
                    temp.isVisiblePersonProcessDetail = pt.isVisiblePersonProcessDetail;
                    temp.isVisiblePersonAddressDetail = pt.isVisiblePersonAddressDetail;
                    temp.isVisiblePersonOpeningDetail = pt.isVisiblePersonOpeningDetail;
                    temp.isVisibleLedgerAccountGroup = pt.isVisibleLedgerAccountGroup;
                    temp.isMandatoryAddress = pt.isMandatoryAddress;
                    temp.isMandatoryCity = pt.isMandatoryCity;
                    temp.isMandatoryZipCode = pt.isMandatoryZipCode;
                    temp.isMandatoryMobile = pt.isMandatoryMobile;
                    temp.isMandatoryEmail = pt.isMandatoryEmail;
                    temp.isMandatoryPanNo = pt.isMandatoryPanNo;
                    temp.isMandatoryGstNo = pt.isMandatoryGstNo;
                    temp.isMandatoryCstNo = pt.isMandatoryCstNo;
                    temp.isMandatoryTinNo = pt.isMandatoryTinNo;
                    temp.isMandatoryAadharNo = pt.isMandatoryAadharNo;
                    temp.isMandatoryTdsCategory = pt.isMandatoryTdsCategory;
                    temp.isMandatoryTdsGroup = pt.isMandatoryTdsGroup;
                    temp.isMandatoryCreditDays = pt.isMandatoryCreditDays;
                    temp.isMandatoryCreditLimit = pt.isMandatoryCreditLimit;
                    temp.isMandatoryGuarantor = pt.isMandatoryGuarantor;
                    temp.isMandatorySalesTaxGroup = pt.isMandatorySalesTaxGroup;
                    temp.LedgerAccountGroupId = pt.LedgerAccountGroupId;
                    temp.DefaultProcessId = pt.DefaultProcessId;
                    temp.SqlProcPersonCode = pt.SqlProcPersonCode;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _PersonSettingsService.Update(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm);
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PersonSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "Person", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag(vm);
            return View("Create", vm);
        }




        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

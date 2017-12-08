using System.Collections.Generic;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using System;
using Model.Models;
using Data.Models;
using System.Collections;
using System.Reflection;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Jobs.Constants.DocumentType;
using Jobs.Constants.DocumentCategory;
using Jobs.Constants.Menu;
using Jobs.Constants.Module;
using Jobs.Constants.SubModule;
using Jobs.Constants.DocumentNature;
using Jobs.Constants.Site;
using Jobs.Constants.Division;
using Jobs.Constants.Company;
using Jobs.Constants.Godown;
using Jobs.Constants.Role;
using Jobs.Constants.User;
using Jobs.Constants.Country;
using Jobs.Constants.State;
using Jobs.Constants.City;
using Jobs.Constants.CompanySetting;
using Jobs.Constants.LedgerAccountGroup;
using Jobs.Constants.LedgerAccount;
using Jobs.Constants.ChargeType;
using Jobs.Constants.Charge;
using Jobs.Constants.Unit;
using Jobs.Constants.Currency;
using Jobs.Constants.ChargeGroupSetting;
using Jobs.Constants.ChargeGroupPerson;
using Jobs.Constants.TdsGroup;
using Jobs.Constants.TdsCategory;
using Jobs.Constants.TdsRate;
using Jobs.Constants.ChargeGroupProduct;
using Jobs.Constants.Department;
using Jobs.Constants.Designation;
using Jobs.Constants.ShipMethod;
using Jobs.Constants.Calculation;
using Jobs.Constants.CalculationFooter;
using Jobs.Constants.CalculationProduct;
using Jobs.Constants.PersonContactType;
using Jobs.Constants.Process;
using Jobs.Constants.DeliveryTerms;
using Jobs.Constants.ProductNature;
using Jobs.Constants.ProductType;

namespace Data.Models
{
    public class InitializeTables
    {
        string mQry = "";
        ApplicationDbContext db = new ApplicationDbContext();
        public UserManager<IdentityUser> UserManager { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }

        public InitializeTables()
        {
        }
        public void InsertData()
        {
            InsertDocumentNature();
            InsertDocumentCategories();
            InsertDocumentTypes();
            InsertModules();
            InsertSubModules();
            InsertMenus();
            InsertCountry();
            InsertState();
            InsertCity();
            InsertSite();
            InsertCompany();
            InsertCompanySetting();
            InsertDivision();
            InsertGodown();
            InsertLedgerAccountGroup();
            InsertLedgerAccount();
            InsertUnit();
            InsertCurrency();
            InsertProcess();
            InsertTdsGroup();
            InsertTdsCategory();
            InsertTdsRate();
            InsertDepartment();
            InsertDesignation();
            InsertShipMethod();
            InsertDeliveryTerms();
            InsertPersonContactType();
            InsertProductNature();
            InsertProductType();
            InsertChargeType();
            InsertCharge();
            InsertChargeGroupPerson();
            InsertChargeGroupProduct();
            InsertChargeGroupSetting();
            InsertCalculation();
            InsertCalculationProduct();
            InsertCalculationFooter();
            //InsertAspNetRole();
            //InsertUser();
            //InsertUserRole();
            InitializeUserTables(db);
        }
        public void InsertDocumentNature()
        {
            try
            {
                Type DocumentNatureConstantsType = typeof(DocumentNatureConstants);

                System.Type[] ChildClassCollection = DocumentNatureConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int DocumentNatureId = (int)ChildClass.GetField("DocumentNatureId").GetRawConstantValue();
                    if (db.DocumentNature.Find(DocumentNatureId) == null)
                    {
                        DocumentNature DocumentNature = new DocumentNature();
                        DocumentNature.DocumentNatureId = (int)ChildClass.GetField("DocumentNatureId").GetRawConstantValue();
                        DocumentNature.DocumentNatureName = (string)ChildClass.GetField("DocumentNatureName").GetRawConstantValue();
                        DocumentNature.IsActive = true;
                        DocumentNature.IsSystemDefine = true;
                        DocumentNature.ObjectState = Model.ObjectState.Added;
                        db.DocumentNature.Add(DocumentNature);
                    }
                    else
                    {
                        DocumentNature DocumentNature = db.DocumentNature.Find(DocumentNatureId);
                        DocumentNature.DocumentNatureName = (string)ChildClass.GetField("DocumentNatureName").GetRawConstantValue();
                        DocumentNature.IsActive = true;
                        DocumentNature.IsSystemDefine = true;
                        DocumentNature.ObjectState = Model.ObjectState.Modified;
                        db.DocumentNature.Add(DocumentNature);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertDocumentCategories()
        {
            try
            {
                Type DocumentCategoryConstantsType = typeof(DocumentCategoryConstants);

                System.Type[] ChildClassCollection = DocumentCategoryConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int DocumentCategoryId = (int)ChildClass.GetField("DocumentCategoryId").GetRawConstantValue();
                    if (db.DocumentCategory.Find(DocumentCategoryId) == null)
                    {
                        DocumentCategory DocumentCategory = new DocumentCategory();
                        DocumentCategory.DocumentCategoryId = (int)ChildClass.GetField("DocumentCategoryId").GetRawConstantValue();
                        DocumentCategory.DocumentCategoryName = (string)ChildClass.GetField("DocumentCategoryName").GetRawConstantValue();
                        DocumentCategory.IsActive = true;
                        DocumentCategory.IsSystemDefine = true;
                        DocumentCategory.ObjectState = Model.ObjectState.Added;
                        db.DocumentCategory.Add(DocumentCategory);
                    }
                    else
                    {
                        DocumentCategory DocumentCategory = db.DocumentCategory.Find(DocumentCategoryId);
                        DocumentCategory.DocumentCategoryName = (string)ChildClass.GetField("DocumentCategoryName").GetRawConstantValue();
                        DocumentCategory.IsActive = true;
                        DocumentCategory.IsSystemDefine = true;
                        DocumentCategory.ObjectState = Model.ObjectState.Modified;
                        db.DocumentCategory.Add(DocumentCategory);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertDocumentTypes()
        {
            try
            {
                Type DocumentTypeConstantsType = typeof(DocumentTypeConstants);

                System.Type[] ChildClassCollection = DocumentTypeConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int DocumentTypeId = (int)ChildClass.GetField("DocumentTypeId").GetRawConstantValue();

                    if (db.DocumentType.Find(DocumentTypeId) == null)
                    {
                        DocumentType DocumentType = new DocumentType();
                        DocumentType.DocumentTypeId = (int)ChildClass.GetField("DocumentTypeId").GetRawConstantValue();
                        DocumentType.DocumentTypeShortName = (string)ChildClass.GetField("DocumentTypeShortName").GetRawConstantValue();
                        DocumentType.DocumentTypeName = (string)ChildClass.GetField("DocumentTypeName").GetRawConstantValue();
                        DocumentType.DocumentCategoryId = (int)ChildClass.GetField("DocumentCategoryId").GetRawConstantValue();
                        DocumentType.DocumentNatureId = (int)ChildClass.GetField("DocumentNatureId").GetRawConstantValue();
                        DocumentType.Nature = (string)ChildClass.GetField("Nature").GetRawConstantValue();
                        DocumentType.PrintTitle = (string)ChildClass.GetField("PrintTitle").GetRawConstantValue();
                        DocumentType.IsActive = true;
                        DocumentType.IsSystemDefine = true;
                        DocumentType.CreatedBy = "System";
                        DocumentType.ModifiedBy = "System";
                        DocumentType.CreatedDate = System.DateTime.Now;
                        DocumentType.ModifiedDate = System.DateTime.Now;
                        DocumentType.ObjectState = Model.ObjectState.Added;
                        db.DocumentType.Add(DocumentType);
                    }
                    else
                    {
                        DocumentType DocumentType = db.DocumentType.Find(DocumentTypeId);
                        DocumentType.DocumentTypeShortName = (string)ChildClass.GetField("DocumentTypeShortName").GetRawConstantValue();
                        DocumentType.DocumentTypeName = (string)ChildClass.GetField("DocumentTypeName").GetRawConstantValue();
                        DocumentType.DocumentCategoryId = (int)ChildClass.GetField("DocumentCategoryId").GetRawConstantValue();
                        DocumentType.DocumentNatureId = (int)ChildClass.GetField("DocumentNatureId").GetRawConstantValue();
                        DocumentType.Nature = (string)ChildClass.GetField("Nature").GetRawConstantValue();
                        DocumentType.PrintTitle = (string)ChildClass.GetField("PrintTitle").GetRawConstantValue();
                        DocumentType.IsActive = true;
                        DocumentType.IsSystemDefine = true;
                        DocumentType.ModifiedBy = "System";
                        DocumentType.ModifiedDate = System.DateTime.Now;
                        DocumentType.ObjectState = Model.ObjectState.Modified;
                        db.DocumentType.Add(DocumentType);
                    }
                    db.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertModules()
        {
            try
            {
                Type ModuleConstantsType = typeof(ModuleConstants);

                System.Type[] ChildClassCollection = ModuleConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int ModuleId = (int)ChildClass.GetField("ModuleId").GetRawConstantValue();

                    if (db.MenuModule.Find(ModuleId) == null)
                    {
                        MenuModule Module = new MenuModule();
                        Module.ModuleId = (int)ChildClass.GetField("ModuleId").GetRawConstantValue();
                        Module.ModuleName = (string)ChildClass.GetField("ModuleName").GetRawConstantValue();
                        Module.Srl = (int)ChildClass.GetField("Srl").GetRawConstantValue();
                        Module.IconName = (string)ChildClass.GetField("IconName").GetRawConstantValue();
                        Module.IsActive = true;
                        Module.CreatedBy = "System";
                        Module.ModifiedBy = "System";
                        Module.CreatedDate = System.DateTime.Now;
                        Module.ModifiedDate = System.DateTime.Now;
                        Module.ObjectState = Model.ObjectState.Added;
                        db.MenuModule.Add(Module);
                    }
                    else
                    {
                        MenuModule Module = db.MenuModule.Find(ModuleId);
                        Module.ModuleName = (string)ChildClass.GetField("ModuleName").GetRawConstantValue();
                        Module.Srl = (int)ChildClass.GetField("Srl").GetRawConstantValue();
                        Module.IconName = (string)ChildClass.GetField("IconName").GetRawConstantValue();
                        Module.IsActive = true;
                        Module.ModifiedBy = "System";
                        Module.ModifiedDate = System.DateTime.Now;
                        Module.ObjectState = Model.ObjectState.Modified;
                        db.MenuModule.Add(Module);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertSubModules()
        {
            try
            {
                Type SubModuleConstantsType = typeof(SubModuleConstants);

                System.Type[] ChildClassCollection = SubModuleConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int SubModuleId = (int)ChildClass.GetField("SubModuleId").GetRawConstantValue();

                    if (db.MenuSubModule.Find(SubModuleId) == null)
                    {
                        MenuSubModule SubModule = new MenuSubModule();
                        SubModule.SubModuleId = (int)ChildClass.GetField("SubModuleId").GetRawConstantValue();
                        SubModule.SubModuleName = (string)ChildClass.GetField("SubModuleName").GetRawConstantValue();
                        SubModule.IconName = (string)ChildClass.GetField("IconName").GetRawConstantValue();
                        SubModule.CreatedBy = "System";
                        SubModule.ModifiedBy = "System";
                        SubModule.CreatedDate = System.DateTime.Now;
                        SubModule.ModifiedDate = System.DateTime.Now;
                        SubModule.ObjectState = Model.ObjectState.Added;
                        db.MenuSubModule.Add(SubModule);
                    }
                    else
                    {
                        MenuSubModule SubModule = db.MenuSubModule.Find(SubModuleId);
                        SubModule.SubModuleName = (string)ChildClass.GetField("SubModuleName").GetRawConstantValue();
                        SubModule.IconName = (string)ChildClass.GetField("IconName").GetRawConstantValue();
                        SubModule.ModifiedBy = "System";
                        SubModule.ModifiedDate = System.DateTime.Now;
                        SubModule.ObjectState = Model.ObjectState.Modified;
                        db.MenuSubModule.Add(SubModule);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertMenus()
        {
            try
            {
                Type MenuConstantsType = typeof(MenuConstants);

                System.Type[] ChildClassCollection = MenuConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int MenuId = (int)ChildClass.GetField("MenuId").GetRawConstantValue();

                    if (db.Menu.Find(MenuId) == null)
                    {
                        Menu Menu = new Menu();
                        Menu.MenuId = (int)ChildClass.GetField("MenuId").GetRawConstantValue();
                        Menu.MenuName = (string)ChildClass.GetField("MenuName").GetRawConstantValue();
                        Menu.Srl = (string)ChildClass.GetField("Srl").GetRawConstantValue();
                        Menu.IconName = (string)ChildClass.GetField("IconName").GetRawConstantValue();
                        Menu.Description = (string)ChildClass.GetField("Description").GetRawConstantValue();
                        Menu.ModuleId = (int)ChildClass.GetField("ModuleId").GetRawConstantValue();
                        Menu.SubModuleId = (int)ChildClass.GetField("SubModuleId").GetRawConstantValue();
                        Menu.ControllerName = (string)ChildClass.GetField("ControllerName").GetRawConstantValue();
                        Menu.ActionName = (string)ChildClass.GetField("ActionName").GetRawConstantValue();
                        Menu.RouteId = (string)ChildClass.GetField("RouteId").GetValue("RouteId");
                        Menu.URL = (string)ChildClass.GetField("URL").GetRawConstantValue();
                        Menu.IsVisible = (bool)ChildClass.GetField("IsVisible").GetRawConstantValue();
                        Menu.AreaName = (string)ChildClass.GetField("AreaName").GetRawConstantValue();
                        Menu.CreatedBy = "System";
                        Menu.ModifiedBy = "System";
                        Menu.CreatedDate = System.DateTime.Now;
                        Menu.ModifiedDate = System.DateTime.Now;
                        Menu.ObjectState = Model.ObjectState.Added;
                        db.Menu.Add(Menu);
                    }
                    else
                    {
                        Menu Menu = db.Menu.Find(MenuId);
                        Menu.MenuName = (string)ChildClass.GetField("MenuName").GetRawConstantValue();
                        Menu.Srl = (string)ChildClass.GetField("Srl").GetRawConstantValue();
                        Menu.IconName = (string)ChildClass.GetField("IconName").GetRawConstantValue();
                        Menu.Description = (string)ChildClass.GetField("Description").GetRawConstantValue();
                        Menu.ModuleId = (int)ChildClass.GetField("ModuleId").GetRawConstantValue();
                        Menu.SubModuleId = (int)ChildClass.GetField("SubModuleId").GetRawConstantValue();
                        Menu.ControllerName = (string)ChildClass.GetField("ControllerName").GetRawConstantValue();
                        Menu.ActionName = (string)ChildClass.GetField("ActionName").GetRawConstantValue();
                        Menu.RouteId = (string)ChildClass.GetField("RouteId").GetValue("RouteId");
                        Menu.URL = (string)ChildClass.GetField("URL").GetRawConstantValue();
                        Menu.IsVisible = (bool)ChildClass.GetField("IsVisible").GetRawConstantValue();
                        Menu.AreaName = (string)ChildClass.GetField("AreaName").GetRawConstantValue();
                        Menu.ModifiedBy = "System";
                        Menu.ModifiedDate = System.DateTime.Now;
                        Menu.ObjectState = Model.ObjectState.Modified;
                        db.Menu.Add(Menu);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertCountry()
        {
            try
            {
                Type CountryConstantsType = typeof(CountryConstants);

                System.Type[] ChildClassCollection = CountryConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int CountryId = (int)ChildClass.GetField("CountryId").GetRawConstantValue();
                    if (db.Country.Find(CountryId) == null)
                    {
                        Country Country = new Country();
                        Country.CountryId = (int)ChildClass.GetField("CountryId").GetRawConstantValue();
                        Country.CountryName = (string)ChildClass.GetField("CountryName").GetRawConstantValue();
                        Country.IsActive = true;
                        Country.CreatedBy = "System";
                        Country.ModifiedBy = "System";
                        Country.CreatedDate = System.DateTime.Now;
                        Country.ModifiedDate = System.DateTime.Now;
                        Country.ObjectState = Model.ObjectState.Added;
                        db.Country.Add(Country);
                    }
                    else
                    {
                        Country Country = db.Country.Find(CountryId);
                        Country.CountryName = (string)ChildClass.GetField("CountryName").GetRawConstantValue();
                        Country.IsActive = true;
                        Country.ModifiedBy = "System";
                        Country.ModifiedDate = System.DateTime.Now;
                        Country.ObjectState = Model.ObjectState.Modified;
                        db.Country.Add(Country);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertState()
        {
            try
            {
                Type StateConstantsType = typeof(StateConstants);

                System.Type[] ChildClassCollection = StateConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int StateId = (int)ChildClass.GetField("StateId").GetRawConstantValue();
                    if (db.State.Find(StateId) == null)
                    {
                        State State = new State();
                        State.StateId = (int)ChildClass.GetField("StateId").GetRawConstantValue();
                        State.StateCode = (string)ChildClass.GetField("StateCode").GetRawConstantValue();
                        State.StateName = (string)ChildClass.GetField("StateName").GetRawConstantValue();
                        State.CountryId = (int)ChildClass.GetField("CountryId").GetRawConstantValue();
                        State.IsActive = true;
                        State.CreatedBy = "System";
                        State.ModifiedBy = "System";
                        State.CreatedDate = System.DateTime.Now;
                        State.ModifiedDate = System.DateTime.Now;
                        State.ObjectState = Model.ObjectState.Added;
                        db.State.Add(State);
                    }
                    else
                    {
                        State State = db.State.Find(StateId);
                        State.StateName = (string)ChildClass.GetField("StateName").GetRawConstantValue();
                        State.StateCode = (string)ChildClass.GetField("StateCode").GetRawConstantValue();
                        State.CountryId = (int)ChildClass.GetField("CountryId").GetRawConstantValue();
                        State.IsActive = true;
                        State.ModifiedBy = "System";
                        State.ModifiedDate = System.DateTime.Now;
                        State.ObjectState = Model.ObjectState.Modified;
                        db.State.Add(State);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertCity()
        {
            try
            {
                Type CityConstantsType = typeof(CityConstants);

                System.Type[] ChildClassCollection = CityConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int CityId = (int)ChildClass.GetField("CityId").GetRawConstantValue();
                    if (db.City.Find(CityId) == null)
                    {
                        City City = new City();
                        City.CityId = (int)ChildClass.GetField("CityId").GetRawConstantValue();
                        City.CityName = (string)ChildClass.GetField("CityName").GetRawConstantValue();
                        City.StateId = (int)ChildClass.GetField("StateId").GetRawConstantValue();
                        City.IsActive = true;
                        City.CreatedBy = "System";
                        City.ModifiedBy = "System";
                        City.CreatedDate = System.DateTime.Now;
                        City.ModifiedDate = System.DateTime.Now;
                        City.ObjectState = Model.ObjectState.Added;
                        db.City.Add(City);
                    }
                    else
                    {
                        City City = db.City.Find(CityId);
                        City.CityName = (string)ChildClass.GetField("CityName").GetRawConstantValue();
                        City.StateId = (int)ChildClass.GetField("StateId").GetRawConstantValue();
                        City.IsActive = true;
                        City.ModifiedBy = "System";
                        City.ModifiedDate = System.DateTime.Now;
                        City.ObjectState = Model.ObjectState.Modified;
                        db.City.Add(City);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertSite()
        {
            try
            {
                Type SiteConstantsType = typeof(SiteConstants);

                System.Type[] ChildClassCollection = SiteConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int SiteId = (int)ChildClass.GetField("SiteId").GetRawConstantValue();
                    if (db.Site.Find(SiteId) == null)
                    {
                        Site Site = new Site();
                        Site.SiteId = (int)ChildClass.GetField("SiteId").GetRawConstantValue();
                        Site.SiteName = (string)ChildClass.GetField("SiteName").GetRawConstantValue();
                        Site.SiteCode = (string)ChildClass.GetField("SiteCode").GetRawConstantValue();
                        Site.CityId = (int)ChildClass.GetField("CityId").GetRawConstantValue();
                        Site.IsActive = true;
                        Site.IsSystemDefine = true;
                        Site.CreatedBy = "System";
                        Site.ModifiedBy = "System";
                        Site.CreatedDate = System.DateTime.Now;
                        Site.ModifiedDate = System.DateTime.Now;
                        Site.ObjectState = Model.ObjectState.Added;
                        db.Site.Add(Site);
                    }
                    else
                    {
                        Site Site = db.Site.Find(SiteId);
                        Site.SiteName = (string)ChildClass.GetField("SiteName").GetRawConstantValue();
                        Site.SiteCode = (string)ChildClass.GetField("SiteCode").GetRawConstantValue();
                        Site.CityId = (int)ChildClass.GetField("CityId").GetRawConstantValue();
                        Site.IsActive = true;
                        Site.IsSystemDefine = true;
                        Site.ModifiedBy = "System";
                        Site.ModifiedDate = System.DateTime.Now;
                        Site.ObjectState = Model.ObjectState.Modified;
                        db.Site.Add(Site);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertCompany()
        {
            try
            {
                Type CompanyConstantsType = typeof(CompanyConstants);

                System.Type[] ChildClassCollection = CompanyConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int CompanyId = (int)ChildClass.GetField("CompanyId").GetRawConstantValue();
                    if (db.Company.Find(CompanyId) == null)
                    {
                        Company Company = new Company();
                        Company.CompanyId = (int)ChildClass.GetField("CompanyId").GetRawConstantValue();
                        Company.CompanyName = (string)ChildClass.GetField("CompanyName").GetRawConstantValue();
                        Company.CreatedBy = "System";
                        Company.ModifiedBy = "System";
                        Company.CreatedDate = System.DateTime.Now;
                        Company.ModifiedDate = System.DateTime.Now;
                        Company.ObjectState = Model.ObjectState.Added;
                        db.Company.Add(Company);
                    }
                    else
                    {
                        Company Company = db.Company.Find(CompanyId);
                        Company.CompanyName = (string)ChildClass.GetField("CompanyName").GetRawConstantValue();
                        Company.ModifiedBy = "System";
                        Company.ModifiedDate = System.DateTime.Now;
                        Company.ObjectState = Model.ObjectState.Modified;
                        db.Company.Add(Company);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertCompanySetting()
        {
            try
            {
                Type CompanySettingConstantsType = typeof(CompanySettingConstants);

                System.Type[] ChildClassCollection = CompanySettingConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int CompanySettingId = (int)ChildClass.GetField("CompanySettingsId").GetRawConstantValue();
                    if (db.CompanySettings.Find(CompanySettingId) == null)
                    {
                        CompanySettings CompanySetting = new CompanySettings();
                        CompanySetting.CompanyId = (int)ChildClass.GetField("CompanyId").GetRawConstantValue();
                        CompanySetting.isVisibleMessage = (bool)ChildClass.GetField("isVisibleMessage").GetRawConstantValue();
                        CompanySetting.isVisibleTask = (bool)ChildClass.GetField("isVisibleTask").GetRawConstantValue();
                        CompanySetting.isVisibleNotification = (bool)ChildClass.GetField("isVisibleNotification").GetRawConstantValue();
                        CompanySetting.isVisibleGodownSelection = (bool)ChildClass.GetField("isVisibleGodownSelection").GetRawConstantValue();
                        CompanySetting.isVisibleCompanyName = (bool)ChildClass.GetField("isVisibleCompanyName").GetRawConstantValue();
                        CompanySetting.SiteCaption = (string)ChildClass.GetField("SiteCaption").GetRawConstantValue();
                        CompanySetting.DivisionCaption = (string)ChildClass.GetField("DivisionCaption").GetRawConstantValue();
                        CompanySetting.GodownCaption = (string)ChildClass.GetField("GodownCaption").GetRawConstantValue();
                        CompanySetting.CreatedBy = "System";
                        CompanySetting.ModifiedBy = "System";
                        CompanySetting.CreatedDate = System.DateTime.Now;
                        CompanySetting.ModifiedDate = System.DateTime.Now;
                        CompanySetting.ObjectState = Model.ObjectState.Added;
                        db.CompanySettings.Add(CompanySetting);
                    }
                    else
                    {
                        CompanySettings CompanySetting = db.CompanySettings.Find(CompanySettingId);
                        CompanySetting.isVisibleMessage = (bool)ChildClass.GetField("isVisibleMessage").GetRawConstantValue();
                        CompanySetting.isVisibleTask = (bool)ChildClass.GetField("isVisibleTask").GetRawConstantValue();
                        CompanySetting.isVisibleNotification = (bool)ChildClass.GetField("isVisibleNotification").GetRawConstantValue();
                        CompanySetting.isVisibleGodownSelection = (bool)ChildClass.GetField("isVisibleGodownSelection").GetRawConstantValue();
                        CompanySetting.isVisibleCompanyName = (bool)ChildClass.GetField("isVisibleCompanyName").GetRawConstantValue();
                        CompanySetting.SiteCaption = (string)ChildClass.GetField("SiteCaption").GetRawConstantValue();
                        CompanySetting.DivisionCaption = (string)ChildClass.GetField("DivisionCaption").GetRawConstantValue();
                        CompanySetting.GodownCaption = (string)ChildClass.GetField("GodownCaption").GetRawConstantValue();
                        CompanySetting.ModifiedBy = "System";
                        CompanySetting.ModifiedDate = System.DateTime.Now;
                        CompanySetting.ObjectState = Model.ObjectState.Modified;
                        db.CompanySettings.Add(CompanySetting);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertDivision()
        {
            try
            {
                Type DivisionConstantsType = typeof(DivisionConstants);

                System.Type[] ChildClassCollection = DivisionConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int DivisionId = (int)ChildClass.GetField("DivisionId").GetRawConstantValue();
                    if (db.Divisions.Find(DivisionId) == null)
                    {
                        Division Division = new Division();
                        Division.DivisionId = (int)ChildClass.GetField("DivisionId").GetRawConstantValue();
                        Division.DivisionName = (string)ChildClass.GetField("DivisionName").GetRawConstantValue();
                        Division.CompanyId = (int)ChildClass.GetField("CompanyId").GetRawConstantValue();
                        Division.IsActive = true;
                        Division.IsSystemDefine = true;
                        Division.CreatedBy = "System";
                        Division.ModifiedBy = "System";
                        Division.CreatedDate = System.DateTime.Now;
                        Division.ModifiedDate = System.DateTime.Now;
                        Division.ObjectState = Model.ObjectState.Added;
                        db.Divisions.Add(Division);
                    }
                    else
                    {
                        Division Division = db.Divisions.Find(DivisionId);
                        Division.DivisionName = (string)ChildClass.GetField("DivisionName").GetRawConstantValue();
                        Division.CompanyId = (int)ChildClass.GetField("CompanyId").GetRawConstantValue();
                        Division.IsActive = true;
                        Division.IsSystemDefine = true;
                        Division.ModifiedBy = "System";
                        Division.ModifiedDate = System.DateTime.Now;
                        Division.ObjectState = Model.ObjectState.Modified;
                        db.Divisions.Add(Division);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertGodown()
        {
            try
            {
                Type GodownConstantsType = typeof(GodownConstants);

                System.Type[] ChildClassCollection = GodownConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int GodownId = (int)ChildClass.GetField("GodownId").GetRawConstantValue();
                    if (db.Godown.Find(GodownId) == null)
                    {
                        Godown Godown = new Godown();
                        Godown.GodownId = (int)ChildClass.GetField("GodownId").GetRawConstantValue();
                        Godown.GodownName = (string)ChildClass.GetField("GodownName").GetRawConstantValue();
                        Godown.SiteId = (int)ChildClass.GetField("SiteId").GetRawConstantValue();
                        Godown.IsActive = true;
                        Godown.CreatedBy = "System";
                        Godown.ModifiedBy = "System";
                        Godown.CreatedDate = System.DateTime.Now;
                        Godown.ModifiedDate = System.DateTime.Now;
                        Godown.ObjectState = Model.ObjectState.Added;
                        db.Godown.Add(Godown);
                    }
                    else
                    {
                        Godown Godown = db.Godown.Find(GodownId);
                        Godown.GodownName = (string)ChildClass.GetField("GodownName").GetRawConstantValue();
                        Godown.SiteId = (int)ChildClass.GetField("SiteId").GetRawConstantValue();
                        Godown.IsActive = true;
                        Godown.ModifiedBy = "System";
                        Godown.ModifiedDate = System.DateTime.Now;
                        Godown.ObjectState = Model.ObjectState.Modified;
                        db.Godown.Add(Godown);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertLedgerAccountGroup()
        {
            try
            {
                Type LedgerAccountGroupConstantsType = typeof(LedgerAccountGroupConstants);

                System.Type[] ChildClassCollection = LedgerAccountGroupConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int LedgerAccountGroupId = (int)ChildClass.GetField("LedgerAccountGroupId").GetRawConstantValue();
                    if (db.LedgerAccountGroup.Find(LedgerAccountGroupId) == null)
                    {
                        LedgerAccountGroup LedgerAccountGroup = new LedgerAccountGroup();
                        LedgerAccountGroup.LedgerAccountGroupId = (int)ChildClass.GetField("LedgerAccountGroupId").GetRawConstantValue();
                        LedgerAccountGroup.LedgerAccountGroupName = (string)ChildClass.GetField("LedgerAccountGroupName").GetRawConstantValue();
                        LedgerAccountGroup.LedgerAccountType = (string)ChildClass.GetField("LedgerAccountType").GetRawConstantValue();
                        LedgerAccountGroup.LedgerAccountNature = (string)ChildClass.GetField("LedgerAccountNature").GetRawConstantValue();
                        LedgerAccountGroup.IsActive = true;
                        LedgerAccountGroup.IsSystemDefine = true;
                        LedgerAccountGroup.CreatedBy = "System";
                        LedgerAccountGroup.ModifiedBy = "System";
                        LedgerAccountGroup.CreatedDate = System.DateTime.Now;
                        LedgerAccountGroup.ModifiedDate = System.DateTime.Now;
                        LedgerAccountGroup.ObjectState = Model.ObjectState.Added;
                        db.LedgerAccountGroup.Add(LedgerAccountGroup);
                    }
                    else
                    {
                        LedgerAccountGroup LedgerAccountGroup = db.LedgerAccountGroup.Find(LedgerAccountGroupId);
                        LedgerAccountGroup.LedgerAccountGroupName = (string)ChildClass.GetField("LedgerAccountGroupName").GetRawConstantValue();
                        LedgerAccountGroup.LedgerAccountType = (string)ChildClass.GetField("LedgerAccountType").GetRawConstantValue();
                        LedgerAccountGroup.LedgerAccountNature = (string)ChildClass.GetField("LedgerAccountNature").GetRawConstantValue();
                        LedgerAccountGroup.IsActive = true;
                        LedgerAccountGroup.IsSystemDefine = true;
                        LedgerAccountGroup.ModifiedBy = "System";
                        LedgerAccountGroup.ModifiedDate = System.DateTime.Now;
                        LedgerAccountGroup.ObjectState = Model.ObjectState.Modified;
                        db.LedgerAccountGroup.Add(LedgerAccountGroup);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertLedgerAccount()
        {
            try
            {
                Type LedgerAccountConstantsType = typeof(LedgerAccountConstants);

                System.Type[] ChildClassCollection = LedgerAccountConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int LedgerAccountId = (int)ChildClass.GetField("LedgerAccountId").GetRawConstantValue();
                    if (db.LedgerAccount.Find(LedgerAccountId) == null)
                    {
                        LedgerAccount LedgerAccount = new LedgerAccount();
                        LedgerAccount.LedgerAccountId = (int)ChildClass.GetField("LedgerAccountId").GetRawConstantValue();
                        LedgerAccount.LedgerAccountName = (string)ChildClass.GetField("LedgerAccountName").GetRawConstantValue();
                        LedgerAccount.LedgerAccountSuffix = (string)ChildClass.GetField("LedgerAccountSuffix").GetRawConstantValue();
                        LedgerAccount.LedgerAccountGroupId = (int)ChildClass.GetField("LedgerAccountGroupId").GetRawConstantValue();
                        LedgerAccount.IsActive = true;
                        LedgerAccount.IsSystemDefine = true;
                        LedgerAccount.CreatedBy = "System";
                        LedgerAccount.ModifiedBy = "System";
                        LedgerAccount.CreatedDate = System.DateTime.Now;
                        LedgerAccount.ModifiedDate = System.DateTime.Now;
                        LedgerAccount.ObjectState = Model.ObjectState.Added;
                        db.LedgerAccount.Add(LedgerAccount);
                    }
                    else
                    {
                        LedgerAccount LedgerAccount = db.LedgerAccount.Find(LedgerAccountId);
                        LedgerAccount.LedgerAccountName = (string)ChildClass.GetField("LedgerAccountName").GetRawConstantValue();
                        LedgerAccount.LedgerAccountSuffix = (string)ChildClass.GetField("LedgerAccountSuffix").GetRawConstantValue();
                        LedgerAccount.LedgerAccountGroupId = (int)ChildClass.GetField("LedgerAccountGroupId").GetRawConstantValue();
                        LedgerAccount.IsActive = true;
                        LedgerAccount.IsSystemDefine = true;
                        LedgerAccount.ModifiedBy = "System";
                        LedgerAccount.ModifiedDate = System.DateTime.Now;
                        LedgerAccount.ObjectState = Model.ObjectState.Modified;
                        db.LedgerAccount.Add(LedgerAccount);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertUnit()
        {
            try
            {
                Type UnitConstantsType = typeof(UnitConstants);

                System.Type[] ChildClassCollection = UnitConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    string UnitId = (string)ChildClass.GetField("UnitId").GetRawConstantValue();
                    if (db.Units.Find(UnitId) == null)
                    {
                        Unit Unit = new Unit();
                        Unit.UnitId = (string)ChildClass.GetField("UnitId").GetRawConstantValue();
                        Unit.UnitName = (string)ChildClass.GetField("UnitName").GetRawConstantValue();
                        Unit.Symbol = (string)ChildClass.GetField("Symbol").GetRawConstantValue();
                        Unit.FractionName = (string)ChildClass.GetField("FractionName").GetRawConstantValue();
                        Unit.FractionUnits = (int)ChildClass.GetField("FractionUnits").GetRawConstantValue();
                        Unit.FractionSymbol = (string)ChildClass.GetField("FractionSymbol").GetRawConstantValue();
                        Unit.DecimalPlaces = (byte)ChildClass.GetField("DecimalPlaces").GetRawConstantValue();
                        Unit.DimensionUnitId = (string)ChildClass.GetField("DimensionUnitId").GetRawConstantValue();
                        Unit.IsActive = true;
                        Unit.CreatedBy = "System";
                        Unit.ModifiedBy = "System";
                        Unit.CreatedDate = System.DateTime.Now;
                        Unit.ModifiedDate = System.DateTime.Now;
                        Unit.ObjectState = Model.ObjectState.Added;
                        db.Units.Add(Unit);
                    }
                    else
                    {
                        Unit Unit = db.Units.Find(UnitId);
                        Unit.UnitName = (string)ChildClass.GetField("UnitName").GetRawConstantValue();
                        Unit.Symbol = (string)ChildClass.GetField("Symbol").GetRawConstantValue();
                        Unit.FractionName = (string)ChildClass.GetField("FractionName").GetRawConstantValue();
                        Unit.FractionUnits = (int)ChildClass.GetField("FractionUnits").GetRawConstantValue();
                        Unit.FractionSymbol = (string)ChildClass.GetField("FractionSymbol").GetRawConstantValue();
                        Unit.DecimalPlaces = (byte)ChildClass.GetField("DecimalPlaces").GetRawConstantValue();
                        Unit.DimensionUnitId = (string)ChildClass.GetField("DimensionUnitId").GetRawConstantValue();
                        Unit.IsActive = true;
                        Unit.ModifiedBy = "System";
                        Unit.ModifiedDate = System.DateTime.Now;
                        Unit.ObjectState = Model.ObjectState.Modified;
                        db.Units.Add(Unit);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertCurrency()
        {
            try
            {
                Type CurrencyConstantsType = typeof(CurrencyConstants);

                System.Type[] ChildClassCollection = CurrencyConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int ID = (int)ChildClass.GetField("ID").GetRawConstantValue();
                    if (db.Currency.Find(ID) == null)
                    {
                        Currency Currency = new Currency();
                        Currency.ID = (int)ChildClass.GetField("ID").GetRawConstantValue();
                        Currency.Name = (string)ChildClass.GetField("Name").GetRawConstantValue();
                        Currency.Symbol = (string)ChildClass.GetField("Symbol").GetRawConstantValue();
                        Currency.IsActive = true;
                        Currency.CreatedBy = "System";
                        Currency.ModifiedBy = "System";
                        Currency.CreatedDate = System.DateTime.Now;
                        Currency.ModifiedDate = System.DateTime.Now;
                        Currency.ObjectState = Model.ObjectState.Added;
                        db.Currency.Add(Currency);
                    }
                    else
                    {
                        Currency Currency = db.Currency.Find(ID);
                        Currency.Name = (string)ChildClass.GetField("Name").GetRawConstantValue();
                        Currency.Symbol = (string)ChildClass.GetField("Symbol").GetRawConstantValue();
                        Currency.IsActive = true;
                        Currency.ModifiedBy = "System";
                        Currency.ModifiedDate = System.DateTime.Now;
                        Currency.ObjectState = Model.ObjectState.Modified;
                        db.Currency.Add(Currency);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertProcess()
        {
            try
            {
                Type ProcessConstantsType = typeof(ProcessConstants);

                System.Type[] ChildClassCollection = ProcessConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int ProcessId = (int)ChildClass.GetField("ProcessId").GetRawConstantValue();
                    if (db.Process.Find(ProcessId) == null)
                    {
                        Process Process = new Process();
                        Process.ProcessId = (int)ChildClass.GetField("ProcessId").GetRawConstantValue();
                        Process.ProcessCode = (string)ChildClass.GetField("ProcessCode").GetRawConstantValue();
                        Process.ProcessName = (string)ChildClass.GetField("ProcessName").GetRawConstantValue();
                        Process.AccountId = (int)ChildClass.GetField("AccountId").GetRawConstantValue();
                        Process.IsActive = true;
                        Process.IsSystemDefine = true;
                        Process.CreatedBy = "System";
                        Process.ModifiedBy = "System";
                        Process.CreatedDate = System.DateTime.Now;
                        Process.ModifiedDate = System.DateTime.Now;
                        Process.ObjectState = Model.ObjectState.Added;
                        db.Process.Add(Process);
                    }
                    else
                    {
                        Process Process = db.Process.Find(ProcessId);
                        Process.ProcessCode = (string)ChildClass.GetField("ProcessCode").GetRawConstantValue();
                        Process.ProcessName = (string)ChildClass.GetField("ProcessName").GetRawConstantValue();
                        Process.AccountId = (int)ChildClass.GetField("AccountId").GetRawConstantValue();
                        Process.IsActive = true;
                        Process.IsSystemDefine = true;
                        Process.ModifiedBy = "System";
                        Process.ModifiedDate = System.DateTime.Now;
                        Process.ObjectState = Model.ObjectState.Modified;
                        db.Process.Add(Process);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertTdsGroup()
        {
            try
            {
                Type TdsGroupConstantsType = typeof(TdsGroupConstants);

                System.Type[] ChildClassCollection = TdsGroupConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int TdsGroupId = (int)ChildClass.GetField("TdsGroupId").GetRawConstantValue();
                    if (db.TdsGroup.Find(TdsGroupId) == null)
                    {
                        TdsGroup TdsGroup = new TdsGroup();
                        TdsGroup.TdsGroupId = (int)ChildClass.GetField("TdsGroupId").GetRawConstantValue();
                        TdsGroup.TdsGroupName = (string)ChildClass.GetField("TdsGroupName").GetRawConstantValue();
                        TdsGroup.CreatedBy = "System";
                        TdsGroup.ModifiedBy = "System";
                        TdsGroup.CreatedDate = System.DateTime.Now;
                        TdsGroup.ModifiedDate = System.DateTime.Now;
                        TdsGroup.ObjectState = Model.ObjectState.Added;
                        db.TdsGroup.Add(TdsGroup);
                    }
                    else
                    {
                        TdsGroup TdsGroup = db.TdsGroup.Find(TdsGroupId);
                        TdsGroup.TdsGroupName = (string)ChildClass.GetField("TdsGroupName").GetRawConstantValue();
                        TdsGroup.ModifiedBy = "System";
                        TdsGroup.ModifiedDate = System.DateTime.Now;
                        TdsGroup.ObjectState = Model.ObjectState.Modified;
                        db.TdsGroup.Add(TdsGroup);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertTdsCategory()
        {
            try
            {
                Type TdsCategoryConstantsType = typeof(TdsCategoryConstants);

                System.Type[] ChildClassCollection = TdsCategoryConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int TdsCategoryId = (int)ChildClass.GetField("TdsCategoryId").GetRawConstantValue();
                    if (db.TdsCategory.Find(TdsCategoryId) == null)
                    {
                        TdsCategory TdsCategory = new TdsCategory();
                        TdsCategory.TdsCategoryId = (int)ChildClass.GetField("TdsCategoryId").GetRawConstantValue();
                        TdsCategory.TdsCategoryName = (string)ChildClass.GetField("TdsCategoryName").GetRawConstantValue();
                        TdsCategory.CreatedBy = "System";
                        TdsCategory.ModifiedBy = "System";
                        TdsCategory.CreatedDate = System.DateTime.Now;
                        TdsCategory.ModifiedDate = System.DateTime.Now;
                        TdsCategory.ObjectState = Model.ObjectState.Added;
                        db.TdsCategory.Add(TdsCategory);
                    }
                    else
                    {
                        TdsCategory TdsCategory = db.TdsCategory.Find(TdsCategoryId);
                        TdsCategory.TdsCategoryName = (string)ChildClass.GetField("TdsCategoryName").GetRawConstantValue();
                        TdsCategory.ModifiedBy = "System";
                        TdsCategory.ModifiedDate = System.DateTime.Now;
                        TdsCategory.ObjectState = Model.ObjectState.Modified;
                        db.TdsCategory.Add(TdsCategory);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertTdsRate()
        {
            try
            {
                Type TdsRateConstantsType = typeof(TdsRateConstants);

                System.Type[] ChildClassCollection = TdsRateConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int TdsRateId = (int)ChildClass.GetField("TdsRateId").GetRawConstantValue();
                    if (db.TdsRate.Find(TdsRateId) == null)
                    {
                        TdsRate TdsRate = new TdsRate();
                        TdsRate.TdsRateId = (int)ChildClass.GetField("TdsRateId").GetRawConstantValue();
                        TdsRate.TdsCategoryId = (int)ChildClass.GetField("TdsCategoryId").GetRawConstantValue();
                        TdsRate.TdsGroupId = (int)ChildClass.GetField("TdsGroupId").GetRawConstantValue();
                        TdsRate.Percentage = Convert.ToDecimal(ChildClass.GetField("Percentage").GetRawConstantValue());
                        TdsRate.CreatedBy = "System";
                        TdsRate.ModifiedBy = "System";
                        TdsRate.CreatedDate = System.DateTime.Now;
                        TdsRate.ModifiedDate = System.DateTime.Now;
                        TdsRate.ObjectState = Model.ObjectState.Added;
                        db.TdsRate.Add(TdsRate);
                    }
                    else
                    {
                        TdsRate TdsRate = db.TdsRate.Find(TdsRateId);
                        TdsRate.TdsCategoryId = (int)ChildClass.GetField("TdsCategoryId").GetRawConstantValue();
                        TdsRate.TdsGroupId = (int)ChildClass.GetField("TdsGroupId").GetRawConstantValue();
                        TdsRate.Percentage = (Decimal)ChildClass.GetField("Percentage").GetRawConstantValue();
                        TdsRate.ModifiedBy = "System";
                        TdsRate.ModifiedDate = System.DateTime.Now;
                        TdsRate.ObjectState = Model.ObjectState.Modified;
                        db.TdsRate.Add(TdsRate);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertDepartment()
        {
            try
            {
                Type DepartmentConstantsType = typeof(DepartmentConstants);

                System.Type[] ChildClassCollection = DepartmentConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int DepartmentId = (int)ChildClass.GetField("DepartmentId").GetRawConstantValue();
                    if (db.Department.Find(DepartmentId) == null)
                    {
                        Department Department = new Department();
                        Department.DepartmentId = (int)ChildClass.GetField("DepartmentId").GetRawConstantValue();
                        Department.DepartmentName = (string)ChildClass.GetField("DepartmentName").GetRawConstantValue();
                        Department.CreatedBy = "System";
                        Department.ModifiedBy = "System";
                        Department.CreatedDate = System.DateTime.Now;
                        Department.ModifiedDate = System.DateTime.Now;
                        Department.ObjectState = Model.ObjectState.Added;
                        db.Department.Add(Department);
                    }
                    else
                    {
                        Department Department = db.Department.Find(DepartmentId);
                        Department.DepartmentName = (string)ChildClass.GetField("DepartmentName").GetRawConstantValue();
                        Department.ModifiedBy = "System";
                        Department.ModifiedDate = System.DateTime.Now;
                        Department.ObjectState = Model.ObjectState.Modified;
                        db.Department.Add(Department);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertDesignation()
        {
            try
            {
                Type DesignationConstantsType = typeof(DesignationConstants);

                System.Type[] ChildClassCollection = DesignationConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int DesignationId = (int)ChildClass.GetField("DesignationId").GetRawConstantValue();
                    if (db.Designation.Find(DesignationId) == null)
                    {
                        Designation Designation = new Designation();
                        Designation.DesignationId = (int)ChildClass.GetField("DesignationId").GetRawConstantValue();
                        Designation.DesignationName = (string)ChildClass.GetField("DesignationName").GetRawConstantValue();
                        Designation.CreatedBy = "System";
                        Designation.ModifiedBy = "System";
                        Designation.CreatedDate = System.DateTime.Now;
                        Designation.ModifiedDate = System.DateTime.Now;
                        Designation.ObjectState = Model.ObjectState.Added;
                        db.Designation.Add(Designation);
                    }
                    else
                    {
                        Designation Designation = db.Designation.Find(DesignationId);
                        Designation.DesignationName = (string)ChildClass.GetField("DesignationName").GetRawConstantValue();
                        Designation.ModifiedBy = "System";
                        Designation.ModifiedDate = System.DateTime.Now;
                        Designation.ObjectState = Model.ObjectState.Modified;
                        db.Designation.Add(Designation);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertShipMethod()
        {
            try
            {
                Type ShipMethodConstantsType = typeof(ShipMethodConstants);

                System.Type[] ChildClassCollection = ShipMethodConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int ShipMethodId = (int)ChildClass.GetField("ShipMethodId").GetRawConstantValue();
                    if (db.ShipMethod.Find(ShipMethodId) == null)
                    {
                        ShipMethod ShipMethod = new ShipMethod();
                        ShipMethod.ShipMethodId = (int)ChildClass.GetField("ShipMethodId").GetRawConstantValue();
                        ShipMethod.ShipMethodName = (string)ChildClass.GetField("ShipMethodName").GetRawConstantValue();
                        ShipMethod.CreatedBy = "System";
                        ShipMethod.ModifiedBy = "System";
                        ShipMethod.CreatedDate = System.DateTime.Now;
                        ShipMethod.ModifiedDate = System.DateTime.Now;
                        ShipMethod.ObjectState = Model.ObjectState.Added;
                        db.ShipMethod.Add(ShipMethod);
                    }
                    else
                    {
                        ShipMethod ShipMethod = db.ShipMethod.Find(ShipMethodId);
                        ShipMethod.ShipMethodName = (string)ChildClass.GetField("ShipMethodName").GetRawConstantValue();
                        ShipMethod.ModifiedBy = "System";
                        ShipMethod.ModifiedDate = System.DateTime.Now;
                        ShipMethod.ObjectState = Model.ObjectState.Modified;
                        db.ShipMethod.Add(ShipMethod);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertDeliveryTerms()
        {
            try
            {
                Type DeliveryTermsConstantsType = typeof(DeliveryTermsConstants);

                System.Type[] ChildClassCollection = DeliveryTermsConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int DeliveryTermsId = (int)ChildClass.GetField("DeliveryTermsId").GetRawConstantValue();
                    if (db.DeliveryTerms.Find(DeliveryTermsId) == null)
                    {
                        DeliveryTerms DeliveryTerms = new DeliveryTerms();
                        DeliveryTerms.DeliveryTermsId = (int)ChildClass.GetField("DeliveryTermsId").GetRawConstantValue();
                        DeliveryTerms.DeliveryTermsName = (string)ChildClass.GetField("DeliveryTermsName").GetRawConstantValue();
                        DeliveryTerms.PrintingDescription = (string)ChildClass.GetField("PrintingDescription").GetRawConstantValue();
                        DeliveryTerms.CreatedBy = "System";
                        DeliveryTerms.ModifiedBy = "System";
                        DeliveryTerms.CreatedDate = System.DateTime.Now;
                        DeliveryTerms.ModifiedDate = System.DateTime.Now;
                        DeliveryTerms.ObjectState = Model.ObjectState.Added;
                        db.DeliveryTerms.Add(DeliveryTerms);
                    }
                    else
                    {
                        DeliveryTerms DeliveryTerms = db.DeliveryTerms.Find(DeliveryTermsId);
                        DeliveryTerms.DeliveryTermsName = (string)ChildClass.GetField("DeliveryTermsName").GetRawConstantValue();
                        DeliveryTerms.PrintingDescription = (string)ChildClass.GetField("PrintingDescription").GetRawConstantValue();
                        DeliveryTerms.ModifiedBy = "System";
                        DeliveryTerms.ModifiedDate = System.DateTime.Now;
                        DeliveryTerms.ObjectState = Model.ObjectState.Modified;
                        db.DeliveryTerms.Add(DeliveryTerms);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertPersonContactType()
        {
            try
            {
                Type PersonContactTypeConstantsType = typeof(PersonContactTypeConstants);

                System.Type[] ChildClassCollection = PersonContactTypeConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int PersonContactTypeId = (int)ChildClass.GetField("PersonContactTypeId").GetRawConstantValue();
                    if (db.PersonContactType.Find(PersonContactTypeId) == null)
                    {
                        PersonContactType PersonContactType = new PersonContactType();
                        PersonContactType.PersonContactTypeId = (int)ChildClass.GetField("PersonContactTypeId").GetRawConstantValue();
                        PersonContactType.PersonContactTypeName = (string)ChildClass.GetField("PersonContactTypeName").GetRawConstantValue();
                        PersonContactType.CreatedBy = "System";
                        PersonContactType.ModifiedBy = "System";
                        PersonContactType.CreatedDate = System.DateTime.Now;
                        PersonContactType.ModifiedDate = System.DateTime.Now;
                        PersonContactType.ObjectState = Model.ObjectState.Added;
                        db.PersonContactType.Add(PersonContactType);
                    }
                    else
                    {
                        PersonContactType PersonContactType = db.PersonContactType.Find(PersonContactTypeId);
                        PersonContactType.PersonContactTypeName = (string)ChildClass.GetField("PersonContactTypeName").GetRawConstantValue();
                        PersonContactType.ModifiedBy = "System";
                        PersonContactType.ModifiedDate = System.DateTime.Now;
                        PersonContactType.ObjectState = Model.ObjectState.Modified;
                        db.PersonContactType.Add(PersonContactType);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }


        public void InsertProductNature()
        {
            try
            {
                Type ProductNatureConstantsType = typeof(ProductNatureConstants);

                System.Type[] ChildClassCollection = ProductNatureConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int ProductNatureId = (int)ChildClass.GetField("ProductNatureId").GetRawConstantValue();
                    if (db.ProductNature.Find(ProductNatureId) == null)
                    {
                        ProductNature ProductNature = new ProductNature();
                        ProductNature.ProductNatureId = (int)ChildClass.GetField("ProductNatureId").GetRawConstantValue();
                        ProductNature.ProductNatureName = (string)ChildClass.GetField("ProductNatureName").GetRawConstantValue();
                        ProductNature.IsActive = true;
                        ProductNature.IsSystemDefine = true;
                        ProductNature.CreatedBy = "System";
                        ProductNature.ModifiedBy = "System";
                        ProductNature.CreatedDate = System.DateTime.Now;
                        ProductNature.ModifiedDate = System.DateTime.Now;
                        ProductNature.ObjectState = Model.ObjectState.Added;
                        db.ProductNature.Add(ProductNature);
                    }
                    else
                    {
                        ProductNature ProductNature = db.ProductNature.Find(ProductNatureId);
                        ProductNature.ProductNatureName = (string)ChildClass.GetField("ProductNatureName").GetRawConstantValue();
                        ProductNature.IsActive = true;
                        ProductNature.IsSystemDefine = true;
                        ProductNature.ModifiedBy = "System";
                        ProductNature.ModifiedDate = System.DateTime.Now;
                        ProductNature.ObjectState = Model.ObjectState.Modified;
                        db.ProductNature.Add(ProductNature);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void InsertProductType()
        {
            try
            {
                Type ProductTypeConstantsType = typeof(ProductTypeConstants);

                System.Type[] ChildClassCollection = ProductTypeConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int ProductTypeId = (int)ChildClass.GetField("ProductTypeId").GetRawConstantValue();
                    if (db.ProductTypes.Find(ProductTypeId) == null)
                    {
                        ProductType ProductType = new ProductType();
                        ProductType.ProductTypeId = (int)ChildClass.GetField("ProductTypeId").GetRawConstantValue();
                        ProductType.ProductTypeName = (string)ChildClass.GetField("ProductTypeName").GetRawConstantValue();
                        ProductType.IsActive = true;
                        ProductType.IsSystemDefine = true;
                        ProductType.CreatedBy = "System";
                        ProductType.ModifiedBy = "System";
                        ProductType.CreatedDate = System.DateTime.Now;
                        ProductType.ModifiedDate = System.DateTime.Now;
                        ProductType.ObjectState = Model.ObjectState.Added;
                        db.ProductTypes.Add(ProductType);
                    }
                    else
                    {
                        ProductType ProductType = db.ProductTypes.Find(ProductTypeId);
                        ProductType.ProductTypeName = (string)ChildClass.GetField("ProductTypeName").GetRawConstantValue();
                        ProductType.IsActive = true;
                        ProductType.IsSystemDefine = true;
                        ProductType.ModifiedBy = "System";
                        ProductType.ModifiedDate = System.DateTime.Now;
                        ProductType.ObjectState = Model.ObjectState.Modified;
                        db.ProductTypes.Add(ProductType);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }


        public void InsertChargeType()
        {
            try
            {
                Type ChargeTypeConstantsType = typeof(ChargeTypeConstants);

                System.Type[] ChildClassCollection = ChargeTypeConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int ChargeTypeId = (int)ChildClass.GetField("ChargeTypeId").GetRawConstantValue();
                    if (db.ChargeType.Find(ChargeTypeId) == null)
                    {
                        ChargeType ChargeType = new ChargeType();
                        ChargeType.ChargeTypeId = (int)ChildClass.GetField("ChargeTypeId").GetRawConstantValue();
                        ChargeType.ChargeTypeName = (string)ChildClass.GetField("ChargeTypeName").GetRawConstantValue();
                        ChargeType.IsActive = true;
                        ChargeType.IsSystemDefine = true;
                        ChargeType.CreatedBy = "System";
                        ChargeType.ModifiedBy = "System";
                        ChargeType.CreatedDate = System.DateTime.Now;
                        ChargeType.ModifiedDate = System.DateTime.Now;
                        ChargeType.ObjectState = Model.ObjectState.Added;
                        db.ChargeType.Add(ChargeType);
                    }
                    else
                    {
                        ChargeType ChargeType = db.ChargeType.Find(ChargeTypeId);
                        ChargeType.ChargeTypeName = (string)ChildClass.GetField("ChargeTypeName").GetRawConstantValue();
                        ChargeType.IsActive = true;
                        ChargeType.IsSystemDefine = true;
                        ChargeType.ModifiedBy = "System";
                        ChargeType.ModifiedDate = System.DateTime.Now;
                        ChargeType.ObjectState = Model.ObjectState.Modified;
                        db.ChargeType.Add(ChargeType);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertCharge()
        {
            try
            {
                Type ChargeConstantsType = typeof(ChargeConstants);

                System.Type[] ChildClassCollection = ChargeConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int ChargeId = (int)ChildClass.GetField("ChargeId").GetRawConstantValue();
                    if (db.Charge.Find(ChargeId) == null)
                    {
                        Charge Charge = new Charge();
                        Charge.ChargeId = (int)ChildClass.GetField("ChargeId").GetRawConstantValue();
                        Charge.ChargeName = (string)ChildClass.GetField("ChargeName").GetRawConstantValue();
                        Charge.ChargeCode = (string)ChildClass.GetField("ChargeCode").GetRawConstantValue();
                        Charge.IsActive = true;
                        Charge.CreatedBy = "System";
                        Charge.ModifiedBy = "System";
                        Charge.CreatedDate = System.DateTime.Now;
                        Charge.ModifiedDate = System.DateTime.Now;
                        Charge.ObjectState = Model.ObjectState.Added;
                        db.Charge.Add(Charge);
                    }
                    else
                    {
                        Charge Charge = db.Charge.Find(ChargeId);
                        Charge.ChargeName = (string)ChildClass.GetField("ChargeName").GetRawConstantValue();
                        Charge.ChargeCode = (string)ChildClass.GetField("ChargeCode").GetRawConstantValue();
                        Charge.IsActive = true;
                        Charge.ModifiedBy = "System";
                        Charge.ModifiedDate = System.DateTime.Now;
                        Charge.ObjectState = Model.ObjectState.Modified;
                        db.Charge.Add(Charge);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertChargeGroupPerson()
        {
            try
            {
                Type ChargeGroupPersonConstantsType = typeof(ChargeGroupPersonConstants);

                System.Type[] ChildClassCollection = ChargeGroupPersonConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int ChargeGroupPersonId = (int)ChildClass.GetField("ChargeGroupPersonId").GetRawConstantValue();
                    if (db.ChargeGroupPerson.Find(ChargeGroupPersonId) == null)
                    {
                        ChargeGroupPerson ChargeGroupPerson = new ChargeGroupPerson();
                        ChargeGroupPerson.ChargeGroupPersonId = (int)ChildClass.GetField("ChargeGroupPersonId").GetRawConstantValue();
                        ChargeGroupPerson.ChargeGroupPersonName = (string)ChildClass.GetField("ChargeGroupPersonName").GetRawConstantValue();
                        ChargeGroupPerson.IsActive = true;
                        ChargeGroupPerson.CreatedBy = "System";
                        ChargeGroupPerson.ModifiedBy = "System";
                        ChargeGroupPerson.CreatedDate = System.DateTime.Now;
                        ChargeGroupPerson.ModifiedDate = System.DateTime.Now;
                        ChargeGroupPerson.ObjectState = Model.ObjectState.Added;
                        db.ChargeGroupPerson.Add(ChargeGroupPerson);
                    }
                    else
                    {
                        ChargeGroupPerson ChargeGroupPerson = db.ChargeGroupPerson.Find(ChargeGroupPersonId);
                        ChargeGroupPerson.ChargeGroupPersonName = (string)ChildClass.GetField("ChargeGroupPersonName").GetRawConstantValue();
                        ChargeGroupPerson.IsActive = true;
                        ChargeGroupPerson.ModifiedBy = "System";
                        ChargeGroupPerson.ModifiedDate = System.DateTime.Now;
                        ChargeGroupPerson.ObjectState = Model.ObjectState.Modified;
                        db.ChargeGroupPerson.Add(ChargeGroupPerson);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertChargeGroupProduct()
        {
            try
            {
                Type ChargeGroupProductConstantsType = typeof(ChargeGroupProductConstants);

                System.Type[] ChildClassCollection = ChargeGroupProductConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int ChargeGroupProductId = (int)ChildClass.GetField("ChargeGroupProductId").GetRawConstantValue();
                    if (db.ChargeGroupProduct.Find(ChargeGroupProductId) == null)
                    {
                        ChargeGroupProduct ChargeGroupProduct = new ChargeGroupProduct();
                        ChargeGroupProduct.ChargeGroupProductId = (int)ChildClass.GetField("ChargeGroupProductId").GetRawConstantValue();
                        ChargeGroupProduct.ChargeGroupProductName = (string)ChildClass.GetField("ChargeGroupProductName").GetRawConstantValue();
                        ChargeGroupProduct.IsActive = true;
                        ChargeGroupProduct.CreatedBy = "System";
                        ChargeGroupProduct.ModifiedBy = "System";
                        ChargeGroupProduct.CreatedDate = System.DateTime.Now;
                        ChargeGroupProduct.ModifiedDate = System.DateTime.Now;
                        ChargeGroupProduct.ObjectState = Model.ObjectState.Added;
                        db.ChargeGroupProduct.Add(ChargeGroupProduct);
                    }
                    else
                    {
                        ChargeGroupProduct ChargeGroupProduct = db.ChargeGroupProduct.Find(ChargeGroupProductId);
                        ChargeGroupProduct.ChargeGroupProductName = (string)ChildClass.GetField("ChargeGroupProductName").GetRawConstantValue();
                        ChargeGroupProduct.IsActive = true;
                        ChargeGroupProduct.ModifiedBy = "System";
                        ChargeGroupProduct.ModifiedDate = System.DateTime.Now;
                        ChargeGroupProduct.ObjectState = Model.ObjectState.Modified;
                        db.ChargeGroupProduct.Add(ChargeGroupProduct);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertChargeGroupSetting()
        {
            try
            {
                Type ChargeGroupSettingConstantsType = typeof(ChargeGroupSettingConstants);

                System.Type[] ChildClassCollection = ChargeGroupSettingConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int ChargeGroupSettingId = (int)ChildClass.GetField("ChargeGroupSettingId").GetRawConstantValue();
                    if (db.ChargeGroupSettings.Find(ChargeGroupSettingId) == null)
                    {
                        ChargeGroupSettings ChargeGroupSetting = new ChargeGroupSettings();
                        ChargeGroupSetting.ChargeGroupSettingsId = (int)ChildClass.GetField("ChargeGroupSettingId").GetRawConstantValue();
                        ChargeGroupSetting.ProcessId = (int)ChildClass.GetField("ProcessId").GetRawConstantValue();
                        ChargeGroupSetting.ChargeTypeId = (int)ChildClass.GetField("ChargeTypeId").GetRawConstantValue();
                        ChargeGroupSetting.ChargeGroupPersonId = (int)ChildClass.GetField("ChargeGroupPersonId").GetRawConstantValue();
                        ChargeGroupSetting.ChargeGroupProductId = (int)ChildClass.GetField("ChargeGroupProductId").GetRawConstantValue();
                        ChargeGroupSetting.ChargePer = Convert.ToDecimal(ChildClass.GetField("ChargePer").GetRawConstantValue());
                        ChargeGroupSetting.ChargeLedgerAccountId = (int?)ChildClass.GetField("ChargeLedgerAccountId").GetValue("ChargeLedgerAccountId");
                        ChargeGroupSetting.CreatedBy = "System";
                        ChargeGroupSetting.ModifiedBy = "System";
                        ChargeGroupSetting.CreatedDate = System.DateTime.Now;
                        ChargeGroupSetting.ModifiedDate = System.DateTime.Now;
                        ChargeGroupSetting.ObjectState = Model.ObjectState.Added;
                        db.ChargeGroupSettings.Add(ChargeGroupSetting);
                    }
                    else
                    {
                        ChargeGroupSettings ChargeGroupSetting = db.ChargeGroupSettings.Find(ChargeGroupSettingId);
                        ChargeGroupSetting.ProcessId = (int)ChildClass.GetField("ProcessId").GetRawConstantValue();
                        ChargeGroupSetting.ChargeTypeId = (int)ChildClass.GetField("ChargeTypeId").GetRawConstantValue();
                        ChargeGroupSetting.ChargeGroupPersonId = (int)ChildClass.GetField("ChargeGroupPersonId").GetRawConstantValue();
                        ChargeGroupSetting.ChargeGroupProductId = (int)ChildClass.GetField("ChargeGroupProductId").GetRawConstantValue();
                        ChargeGroupSetting.ChargePer = Convert.ToDecimal(ChildClass.GetField("ChargePer").GetRawConstantValue());
                        ChargeGroupSetting.ChargeLedgerAccountId = (int?)ChildClass.GetField("ChargeLedgerAccountId").GetValue("ChargeLedgerAccountId");
                        ChargeGroupSetting.ModifiedBy = "System";
                        ChargeGroupSetting.ModifiedDate = System.DateTime.Now;
                        ChargeGroupSetting.ObjectState = Model.ObjectState.Modified;
                        db.ChargeGroupSettings.Add(ChargeGroupSetting);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertCalculation()
        {
            try
            {
                Type CalculationConstantsType = typeof(CalculationConstants);

                System.Type[] ChildClassCollection = CalculationConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int CalculationId = (int)ChildClass.GetField("CalculationId").GetRawConstantValue();
                    if (db.Calculation.Find(CalculationId) == null)
                    {
                        Calculation Calculation = new Calculation();
                        Calculation.CalculationId = (int)ChildClass.GetField("CalculationId").GetRawConstantValue();
                        Calculation.CalculationName = (string)ChildClass.GetField("CalculationName").GetRawConstantValue();
                        Calculation.IsActive = true;
                        Calculation.CreatedBy = "System";
                        Calculation.ModifiedBy = "System";
                        Calculation.CreatedDate = System.DateTime.Now;
                        Calculation.ModifiedDate = System.DateTime.Now;
                        Calculation.ObjectState = Model.ObjectState.Added;
                        db.Calculation.Add(Calculation);
                    }
                    else
                    {
                        Calculation Calculation = db.Calculation.Find(CalculationId);
                        Calculation.CalculationName = (string)ChildClass.GetField("CalculationName").GetRawConstantValue();
                        Calculation.IsActive = true;
                        Calculation.ModifiedBy = "System";
                        Calculation.ModifiedDate = System.DateTime.Now;
                        Calculation.ObjectState = Model.ObjectState.Modified;
                        db.Calculation.Add(Calculation);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertCalculationProduct()
        {
            try
            {
                Type CalculationProductConstantsType = typeof(CalculationProductConstants);

                System.Type[] ChildClassCollection = CalculationProductConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int CalculationProductId = (int)ChildClass.GetField("CalculationProductId").GetRawConstantValue();

                    if (db.CalculationProduct.Find(CalculationProductId) == null)
                    {
                        CalculationProduct CalculationProduct = new CalculationProduct();
                        CalculationProduct.CalculationProductId = (int)ChildClass.GetField("CalculationProductId").GetRawConstantValue();
                        CalculationProduct.CalculationId = (int)ChildClass.GetField("CalculationId").GetValue("CalculationId");
                        CalculationProduct.Sr = (int)ChildClass.GetField("Sr").GetValue("Sr");
                        CalculationProduct.ChargeId = (int)ChildClass.GetField("ChargeId").GetValue("ChargeId");
                        CalculationProduct.AddDeduct = (byte?)ChildClass.GetField("AddDeduct").GetValue("AddDeduct");
                        CalculationProduct.AffectCost = (bool)ChildClass.GetField("AffectCost").GetValue("AffectCost");
                        CalculationProduct.ChargeTypeId = (int?)ChildClass.GetField("ChargeTypeId").GetValue("ChargeTypeId");
                        CalculationProduct.CalculateOnId = (int?)ChildClass.GetField("CalculateOnId").GetValue("CalculateOnId");
                        CalculationProduct.RateType = (byte)ChildClass.GetField("RateType").GetValue("RateType");
                        CalculationProduct.IncludedInBase = (bool)ChildClass.GetField("IncludedInBase").GetValue("IncludedInBase");
                        CalculationProduct.ParentChargeId = (int?)ChildClass.GetField("ParentChargeId").GetValue("ParentChargeId");
                        CalculationProduct.IsVisible = (bool)ChildClass.GetField("IsVisible").GetValue("IsVisible");
                        CalculationProduct.IsActive = (bool)ChildClass.GetField("IsActive").GetValue("IsActive");
                        CalculationProduct.IncludedCharges = (string)ChildClass.GetField("IncludedCharges").GetValue("IncludedCharges");
                        CalculationProduct.IncludedChargesCalculation = (string)ChildClass.GetField("IncludedChargesCalculation").GetValue("IncludedChargesCalculation");
                        CalculationProduct.CreatedBy = "System";
                        CalculationProduct.ModifiedBy = "System";
                        CalculationProduct.CreatedDate = System.DateTime.Now;
                        CalculationProduct.ModifiedDate = System.DateTime.Now;
                        CalculationProduct.ObjectState = Model.ObjectState.Added;
                        db.CalculationProduct.Add(CalculationProduct);
                    }
                    else
                    {
                        CalculationProduct CalculationProduct = db.CalculationProduct.Find(CalculationProductId);
                        CalculationProduct.CalculationId = (int)ChildClass.GetField("CalculationId").GetValue("CalculationId");
                        CalculationProduct.Sr = (int)ChildClass.GetField("Sr").GetValue("Sr");
                        CalculationProduct.ChargeId = (int)ChildClass.GetField("ChargeId").GetValue("ChargeId");
                        CalculationProduct.AddDeduct = (byte?)ChildClass.GetField("AddDeduct").GetValue("AddDeduct");
                        CalculationProduct.AffectCost = (bool)ChildClass.GetField("AffectCost").GetValue("AffectCost");
                        CalculationProduct.ChargeTypeId = (int?)ChildClass.GetField("ChargeTypeId").GetValue("ChargeTypeId");
                        CalculationProduct.CalculateOnId = (int?)ChildClass.GetField("CalculateOnId").GetValue("CalculateOnId");
                        CalculationProduct.RateType = (byte)ChildClass.GetField("RateType").GetValue("RateType");
                        CalculationProduct.IncludedInBase = (bool)ChildClass.GetField("IncludedInBase").GetValue("IncludedInBase");
                        CalculationProduct.ParentChargeId = (int?)ChildClass.GetField("ParentChargeId").GetValue("ParentChargeId");
                        CalculationProduct.IsVisible = (bool)ChildClass.GetField("IsVisible").GetValue("IsVisible");
                        CalculationProduct.IsActive = (bool)ChildClass.GetField("IsActive").GetValue("IsActive");
                        CalculationProduct.IncludedCharges = (string)ChildClass.GetField("IncludedCharges").GetValue("IncludedCharges");
                        CalculationProduct.IncludedChargesCalculation = (string)ChildClass.GetField("IncludedChargesCalculation").GetValue("IncludedChargesCalculation");
                        CalculationProduct.ModifiedBy = "System";
                        CalculationProduct.ModifiedDate = System.DateTime.Now;
                        CalculationProduct.ObjectState = Model.ObjectState.Modified;
                        db.CalculationProduct.Add(CalculationProduct);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void InsertCalculationFooter()
        {
            try
            {
                Type CalculationFooterConstantsType = typeof(CalculationFooterConstants);

                System.Type[] ChildClassCollection = CalculationFooterConstantsType.GetNestedTypes();

                foreach (System.Type ChildClass in ChildClassCollection)
                {
                    int CalculationFooterLineId = (int)ChildClass.GetField("CalculationFooterLineId").GetRawConstantValue();

                    if (db.CalculationFooter.Find(CalculationFooterLineId) == null)
                    {
                        CalculationFooter CalculationFooter = new CalculationFooter();
                        CalculationFooter.CalculationFooterLineId = (int)ChildClass.GetField("CalculationFooterLineId").GetRawConstantValue();
                        CalculationFooter.CalculationId = (int)ChildClass.GetField("CalculationId").GetValue("CalculationId");
                        CalculationFooter.Sr = (int)ChildClass.GetField("Sr").GetValue("Sr");
                        CalculationFooter.ChargeId = (int)ChildClass.GetField("ChargeId").GetValue("ChargeId");
                        CalculationFooter.AddDeduct = (byte?)ChildClass.GetField("AddDeduct").GetValue("AddDeduct");
                        CalculationFooter.AffectCost = (bool)ChildClass.GetField("AffectCost").GetValue("AffectCost");
                        CalculationFooter.ChargeTypeId = (int?)ChildClass.GetField("ChargeTypeId").GetValue("ChargeTypeId");
                        CalculationFooter.CalculateOnId = (int?)ChildClass.GetField("CalculateOnId").GetValue("CalculateOnId");
                        CalculationFooter.ProductChargeId = (int?)ChildClass.GetField("ProductChargeId").GetValue("ProductChargeId");
                        CalculationFooter.RateType = (byte)ChildClass.GetField("RateType").GetValue("RateType");
                        CalculationFooter.IncludedInBase = (bool)ChildClass.GetField("IncludedInBase").GetValue("IncludedInBase");
                        CalculationFooter.ParentChargeId = (int?)ChildClass.GetField("ParentChargeId").GetValue("ParentChargeId");
                        CalculationFooter.IsVisible = (bool)ChildClass.GetField("IsVisible").GetValue("IsVisible");
                        CalculationFooter.IsActive = (bool)ChildClass.GetField("IsActive").GetValue("IsActive");
                        CalculationFooter.IncludedCharges = (string)ChildClass.GetField("IncludedCharges").GetValue("IncludedCharges");
                        CalculationFooter.IncludedChargesCalculation = (string)ChildClass.GetField("IncludedChargesCalculation").GetValue("IncludedChargesCalculation");
                        CalculationFooter.CreatedBy = "System";
                        CalculationFooter.ModifiedBy = "System";
                        CalculationFooter.CreatedDate = System.DateTime.Now;
                        CalculationFooter.ModifiedDate = System.DateTime.Now;
                        CalculationFooter.ObjectState = Model.ObjectState.Added;
                        db.CalculationFooter.Add(CalculationFooter);
                    }
                    else
                    {
                        CalculationFooter CalculationFooter = db.CalculationFooter.Find(CalculationFooterLineId);
                        CalculationFooter.CalculationId = (int)ChildClass.GetField("CalculationId").GetValue("CalculationId");
                        CalculationFooter.Sr = (int)ChildClass.GetField("Sr").GetValue("Sr");
                        CalculationFooter.ChargeId = (int)ChildClass.GetField("ChargeId").GetValue("ChargeId");
                        CalculationFooter.AddDeduct = (byte?)ChildClass.GetField("AddDeduct").GetValue("AddDeduct");
                        CalculationFooter.AffectCost = (bool)ChildClass.GetField("AffectCost").GetValue("AffectCost");
                        CalculationFooter.ChargeTypeId = (int?)ChildClass.GetField("ChargeTypeId").GetValue("ChargeTypeId");
                        CalculationFooter.CalculateOnId = (int?)ChildClass.GetField("CalculateOnId").GetValue("CalculateOnId");
                        CalculationFooter.ProductChargeId = (int?)ChildClass.GetField("ProductChargeId").GetValue("ProductChargeId");
                        CalculationFooter.RateType = (byte)ChildClass.GetField("RateType").GetValue("RateType");
                        CalculationFooter.IncludedInBase = (bool)ChildClass.GetField("IncludedInBase").GetValue("IncludedInBase");
                        CalculationFooter.ParentChargeId = (int?)ChildClass.GetField("ParentChargeId").GetValue("ParentChargeId");
                        CalculationFooter.IsVisible = (bool)ChildClass.GetField("IsVisible").GetValue("IsVisible");
                        CalculationFooter.IsActive = (bool)ChildClass.GetField("IsActive").GetValue("IsActive");
                        CalculationFooter.IncludedCharges = (string)ChildClass.GetField("IncludedCharges").GetValue("IncludedCharges");
                        CalculationFooter.IncludedChargesCalculation = (string)ChildClass.GetField("IncludedChargesCalculation").GetValue("IncludedChargesCalculation");
                        CalculationFooter.ModifiedBy = "System";
                        CalculationFooter.ModifiedDate = System.DateTime.Now;
                        CalculationFooter.ObjectState = Model.ObjectState.Modified;
                        db.CalculationFooter.Add(CalculationFooter);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        private void InitializeUserTables(ApplicationDbContext db)
        {
            string mQry = "";

            mQry = " INSERT INTO Web.AspNetRoles (Id, Name) VALUES ('302b9430-498e-46e8-8ead-9e92867b7c9f', 'Admin') ";
            db.Database.ExecuteSqlCommand(mQry);

            mQry = " INSERT INTO Web.Users (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEndDateUtc, LockoutEnabled, AccessFailedCount, UserName, FirstName, LastName, Discriminator) " +
                    " VALUES ('99bddeed-49d4-45fd-8f2c-df57c78434dd', 'meet2arpit@rediffmail.com', 1, 'AIrF0x9YIMRYD8NbDwddsu59QH2AojxsDKdH3yxRYT180EEJqSrnTmWjF1fgxnSZ8g==', '432a52e7-c4f9-4ef5-81cf-7fdc75c7d964', NULL, 0, 0, NULL, 0, 1, 'admin', NULL, NULL, '1') ";
            db.Database.ExecuteSqlCommand(mQry);

            mQry = @"INSERT INTO Web.UserRoles (UserId, RoleId, ExpiryDate, DivisionId, SiteId, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate) 
                    VALUES ('99bddeed-49d4-45fd-8f2c-df57c78434dd', '302b9430-498e-46e8-8ead-9e92867b7c9f', 
                    " + SiteConstants.MainSite.SiteId + @",
                    " + DivisionConstants.MainDivision.DivisionId + @",
                    NULL, 'Admin', getdate(), 'Admin', getdate()) ";
            db.Database.ExecuteSqlCommand(mQry);
        }
        //public void InsertAspNetRole()
        //{
        //    try
        //    {
        //        Type AspNetRoleConstantsType = typeof(AspNetRoleConstants);

        //        System.Type[] ChildClassCollection = AspNetRoleConstantsType.GetNestedTypes();

        //        foreach (System.Type ChildClass in ChildClassCollection)
        //        {
        //            string AspNetRoleId = (string)ChildClass.GetField("Id").GetRawConstantValue();
        //            if (db.AspNetRole.Find(AspNetRoleId) == null)
        //            {
        //                string RoleName = (string)ChildClass.GetField("Name").GetRawConstantValue();
        //                RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
        //                var role = new IdentityRole(RoleName);
        //                var roleresult = RoleManager.CreateAsync(role);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //    }
        //}
        //public void InsertUser()
        //{
        //    try
        //    {
        //        Type UserConstantsType = typeof(UserConstants);

        //        System.Type[] ChildClassCollection = UserConstantsType.GetNestedTypes();

        //        foreach (System.Type ChildClass in ChildClassCollection)
        //        {
        //            string UserId = (string)ChildClass.GetField("Id").GetRawConstantValue();
        //            if (db.Users.Find(UserId) == null)
        //            {
        //                UserManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(db));
        //                var user = new ApplicationUser
        //                {
        //                    UserName = (string)ChildClass.GetField("UserName").GetRawConstantValue(),
        //                    EmailConfirmed = true,
        //                };
        //                UserManager.CreateAsync(user, null);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //    }
        //}
        //public void InsertUserRole()
        //{
        //    try
        //    {
        //        string UserId = (from R in db.AspNetRole where R.Name == AspNetRoleConstants.Admin.Name select R).for
        //        string UserName = User.Identity.GetUserName();
        //        string URoles = (string)System.Web.HttpContext.Current.Session["LoginUserRole"];


        //        Type UserRoleConstantsType = typeof(UserRoleConstants);

        //        System.Type[] ChildClassCollection = UserRoleConstantsType.GetNestedTypes();

        //        foreach (System.Type ChildClass in ChildClassCollection)
        //        {
        //            string UserRoleId = (string)ChildClass.GetField("UserId").GetRawConstantValue();
        //            if (db.UserRole.Find(UserRoleId) == null)
        //            {
        //                UserRole UserRole = new UserRole();
        //                UserRole.UserId = (string)ChildClass.GetField("UserId").GetRawConstantValue();
        //                UserRole.RoleId = (string)ChildClass.GetField("RoleId").GetRawConstantValue();
        //                db.UserRole.Add(UserRole);
        //            }
        //            db.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //    }
        //}
    }
}
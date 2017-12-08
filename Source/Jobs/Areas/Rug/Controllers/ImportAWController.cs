using System.Collections.Generic;
using System.Web.Mvc;
using Service;

//using ProjLib.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System;
using System.Data.OleDb;
using Jobs.Controllers;
using Data.Infrastructure;
using Data.Models;
using Model.ViewModels;
using Jobs.Constants.DocumentType;
using Jobs.Constants.LedgerAccountGroup;
using Jobs.Constants.ChargeGroupPerson;
using Model.ViewModel;
using Model.Models;
using AutoMapper;

namespace Module
{
    [Authorize]
    public class ImportAWController : Controller
    {
        string mQry = "";
        IModuleService _ModuleService;
        ApplicationDbContext db = new ApplicationDbContext();

        public ImportAWController(IModuleService mService)
        {
            _ModuleService = mService;
        }

        public ActionResult ImportData()
        {
            OleDbConnection conn = new OleDbConnection();
            var DBPath = "D:\\Akash\\Data\\OC\\Data 17-18\\Data.mdb;Jet OLEDB:Database Password=jpkcc;";

            conn = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0;"
                + "Data Source=" + DBPath);

            ImportAWService _ImportAWService = new ImportAWService(conn);

            new ImportRugDataService().InsertData();

            _ImportAWService.ImportColour();
            _ImportAWService.ImportBuyer();
            _ImportAWService.ImportContractor();
            _ImportAWService.ImportDyer();
            _ImportAWService.ImportFinishing();
            _ImportAWService.ImportSupplier();
            _ImportAWService.ImportShadeno();
            _ImportAWService.ImportWool1();
            _ImportAWService.ImportWool();
            _ImportAWService.ImportQuality();
            


            //conn.Open();

            //using (DataTable dt = new DataTable())
            //{
            //    mQry = "Select * From Colour";
            //    using (OleDbDataAdapter adapter = new OleDbDataAdapter(mQry, conn))
            //    {
            //        adapter.Fill(dt);
            //    }
            //}
            return RedirectToAction("Module", "Menu");
        }


        //public void ImportJobWorker()
        //{
        //    UnitOfWork _unitOfWork = new UnitOfWork(db);
        //    BusinessEntityService _BusinessEntityService = new BusinessEntityService(_unitOfWork);
        //    PersonService _PersonService = new PersonService(_unitOfWork);
        //    PersonAddressService _PersonAddressService = new PersonAddressService(_unitOfWork);
        //    AccountService _AccountService = new AccountService(_unitOfWork);
        //    PersonProcessService _PersonProcessService = new PersonProcessService(_unitOfWork);
        //    PersonRegistrationService _PersonRegistrationService = new PersonRegistrationService(_unitOfWork);
        //    PersonRoleService _PersonRoleService = new PersonRoleService(_unitOfWork);
        //    ExceptionHandlingService _exception = new ExceptionHandlingService();

        //    PersonController PC = new PersonController(_PersonService, _BusinessEntityService, _AccountService, _PersonRegistrationService, _PersonAddressService, _PersonProcessService, _PersonRoleService, _unitOfWork, _exception);
        //    PersonViewModel PersonVm = new PersonViewModel();


        //    OleDbConnection conn = new OleDbConnection();
        //    var DBPath = "D:\\Akash\\Data\\OC\\Data 17-18\\Data.mdb;Jet OLEDB:Database Password=jpkcc;";

        //    conn = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0;"
        //        + "Data Source=" + DBPath);

        //    DataTable DataFromSource = new DataTable();
        //    mQry = "Select * From Contractor";
        //    using (OleDbDataAdapter adapter = new OleDbDataAdapter(mQry, conn))
        //    {
        //        adapter.Fill(DataFromSource);
        //    }

        //    for (int i = 0; i <= DataFromSource.Rows.Count - 1; i++)
        //    {
        //        var settings = new PersonSettingsService(_unitOfWork).GetPersonSettingsForDocument(DocumentTypeConstants.JobWorker.DocumentTypeId);

        //        PersonVm.PersonSettings = Mapper.Map<PersonSettings, PersonSettingsViewModel>(settings);
        //        PersonVm.DocTypeId = DocumentTypeConstants.JobWorker.DocumentTypeId;
        //        PersonVm.Name = DataFromSource.Rows[i]["Name"].ToString();
        //        PersonVm.Address = DataFromSource.Rows[i]["ADDRESS"].ToString();
        //        PersonVm.Code = DataFromSource.Rows[i]["Code"].ToString();
        //        PersonVm.LedgerAccountGroupId = LedgerAccountGroupConstants.SundryDebtors.LedgerAccountGroupId;
        //        PersonVm.IsActive = true;
        //        PersonVm.IsSisterConcern = false;
        //        PersonVm.SalesTaxGroupPartyId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
        //        PersonVm.CreatedBy = "System";
        //        PersonVm.ModifiedBy = "System";
        //        PersonVm.CreatedDate = System.DateTime.Now;
        //        PersonVm.ModifiedDate = System.DateTime.Now;
        //        PC.Create(PersonVm);
        //    }
        //}
    }
}
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using Jobs.Constants.DocumentType;
using Jobs.Constants.ChargeGroupPerson;
using Jobs.Constants.LedgerAccountGroup;
using Jobs.Constants.Process;
using Model.ViewModels;
using Jobs.Constants.ChargeGroupProduct;
using Jobs.Constants.Unit;
using Jobs.Constants.Division;

namespace Service
{
    public interface IImportAWService : IDisposable
    {

    }

    public class ImportAWService : IImportAWService
    {
        string mQry = "";
        ApplicationDbContext db = new ApplicationDbContext();
        OleDbConnection _conn;
        static string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
        SqlConnection SqlDataConnection = new SqlConnection(ConnectionString);
        SqlTransaction SqlDataTransaction;

        string SystemUser = "System";
        public ImportAWService(OleDbConnection conn)
        {
            _conn = conn;
        }
        public void ImportColour()
        {
            SqlDataConnection.Open();
            _conn.Open();
            try
            {
                DataTable DataFromSource = new DataTable();
                mQry = "Select * From Colour";
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(mQry, _conn))
                {
                    adapter.Fill(DataFromSource);
                }

                for (int i = 0; i <= DataFromSource.Rows.Count - 1; i++)
                {
                    if ((int)ExecuteScaler("SELECT Count(*) AS Cnt FROM Web.Colours WHERE ColourName = '" + DataFromSource.Rows[i]["Colour"] + "'") == 0)
                    {
                        SqlDataTransaction = SqlDataConnection.BeginTransaction("");

                        mQry = @"INSERT INTO Web.Colours (ColourName, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, OMSId)
                                VALUES ('" + DataFromSource.Rows[i]["Colour"] + "', 1, '" + SystemUser + @"', '" + SystemUser + @"', getdate(), getdate(), NULL)";
                        ExecuteQuery(mQry);

                        SqlDataTransaction.Commit();
                    }
                }
                
                SqlDataConnection.Close();
                _conn.Close();
            }
            catch (Exception ex)
            {
                SqlDataTransaction.Rollback();
                SqlDataConnection.Close();
                _conn.Close();
                RecordError(ex);
            }
        }


        public void ImportShadeno()
        {
            SqlDataConnection.Open();
            _conn.Open();
            try
            {
                DataTable DataFromSource = new DataTable();
                mQry = "Select * From Shadeno";
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(mQry, _conn))
                {
                    adapter.Fill(DataFromSource);
                }

                for (int i = 0; i <= DataFromSource.Rows.Count - 1; i++)
                {
                    if ((int)ExecuteScaler("SELECT Count(*) AS Cnt FROM Web.Dimension1 WHERE Dimension1Name = '" + DataFromSource.Rows[i]["Shadeno"] + "'") == 0)
                    {
                        SqlDataTransaction = SqlDataConnection.BeginTransaction("");

                        mQry = @"INSERT INTO Web.Dimension1 (Dimension1Name, ProductTypeId, IsActive, Description, ReferenceDocTypeId, ReferenceDocId, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, OMSId)
                                Select '" + DataFromSource.Rows[i]["Shadeno"] + "' As Dimension1Name, Null As ProductTypeId, 1 As IsActive, '" + DataFromSource.Rows[i]["Shadeno"] + @"' As Description, 
                                Null As ReferenceDocTypeId, Null As ReferenceDocId, '" + SystemUser + @"' As CreatedBy, '" + SystemUser + @"' As ModifiedBy, 
                                getdate() As CreatedDate, getdate() As ModifiedDate, Null As OMSId ";
                        ExecuteQuery(mQry);

                        SqlDataTransaction.Commit();
                    }
                }
                SqlDataConnection.Close();
                _conn.Close();
            }
            catch (Exception ex)
            {
                SqlDataTransaction.Rollback();
                SqlDataConnection.Close();
                _conn.Close();
                RecordError(ex);
            }
        }


        public void ImportContractor()
        {
            SqlDataConnection.Open();
            _conn.Open();
             try
            {
                DataTable DataFromSource = new DataTable();
                mQry = "Select * From Contractor";
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(mQry, _conn))
                {
                    adapter.Fill(DataFromSource);
                }

                for (int i = 0; i <= DataFromSource.Rows.Count - 1; i++)
                {
                    PersonViewModel PersonVm = new PersonViewModel();
                    PersonVm.DocTypeId = DocumentTypeConstants.JobWorker.DocumentTypeId;
                    PersonVm.Name = DataFromSource.Rows[i]["Name"].ToString();
                    PersonVm.Address = DataFromSource.Rows[i]["ADDRESS"].ToString();
                    PersonVm.Code = DataFromSource.Rows[i]["Code"].ToString();
                    PersonVm.LedgerAccountGroupId = LedgerAccountGroupConstants.SundryDebtors.LedgerAccountGroupId;
                    PersonVm.ProcessId = ProcessConstants.Purchase.ProcessId;
                    PersonVm.IsActive = true;
                    PersonVm.IsSisterConcern = false;
                    PersonVm.SalesTaxGroupPartyId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
                    PersonVm.CreatedBy = SystemUser;
                    PersonVm.ModifiedBy = SystemUser;
                    PersonVm.CreatedDate = System.DateTime.Now;
                    PersonVm.ModifiedDate = System.DateTime.Now;
                    
                    SqlDataTransaction = SqlDataConnection.BeginTransaction("");

                    ImportPerson(PersonVm);

                    SqlDataTransaction.Commit();
                }
                SqlDataConnection.Close();
                _conn.Close();
            }
            catch (Exception ex)
            {
                SqlDataTransaction.Rollback();
                SqlDataConnection.Close();
                _conn.Close();
                RecordError(ex);
            }
        }
        public void ImportDyer()
        {
            SqlDataConnection.Open();
            _conn.Open();
            try
            {
                DataTable DataFromSource = new DataTable();
                mQry = "Select * From dyers";
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(mQry, _conn))
                {
                    adapter.Fill(DataFromSource);
                }

                for (int i = 0; i <= DataFromSource.Rows.Count - 1; i++)
                {
                    PersonViewModel PersonVm = new PersonViewModel();
                    PersonVm.DocTypeId = DocumentTypeConstants.JobWorker.DocumentTypeId;
                    PersonVm.Name = DataFromSource.Rows[i]["Name"].ToString();
                    PersonVm.Address = DataFromSource.Rows[i]["ADDRESS"].ToString();
                    PersonVm.Code = DataFromSource.Rows[i]["Code"].ToString();
                    PersonVm.LedgerAccountGroupId = LedgerAccountGroupConstants.SundryDebtors.LedgerAccountGroupId;
                    PersonVm.ProcessId = ProcessConstants.Purchase.ProcessId;
                    PersonVm.IsActive = true;
                    PersonVm.IsSisterConcern = false;
                    PersonVm.SalesTaxGroupPartyId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
                    PersonVm.CreatedBy = SystemUser;
                    PersonVm.ModifiedBy = SystemUser;
                    PersonVm.CreatedDate = System.DateTime.Now;
                    PersonVm.ModifiedDate = System.DateTime.Now;

                    SqlDataTransaction = SqlDataConnection.BeginTransaction("");

                    ImportPerson(PersonVm);

                    SqlDataTransaction.Commit();
                }
                SqlDataConnection.Close();
                _conn.Close();
            }
            catch (Exception ex)
            {
                SqlDataTransaction.Rollback();
                SqlDataConnection.Close();
                _conn.Close();
                RecordError(ex);
            }
        }
        public void ImportBuyer()
        {
            SqlDataConnection.Open();
            _conn.Open();
            try
            {
                DataTable DataFromSource = new DataTable();
                mQry = "Select * From Buyer";
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(mQry, _conn))
                {
                    adapter.Fill(DataFromSource);
                }

                for (int i = 0; i <= DataFromSource.Rows.Count - 1; i++)
                {
                    PersonViewModel PersonVm = new PersonViewModel();
                    PersonVm.DocTypeId = DocumentTypeConstants.Customer.DocumentTypeId;
                    PersonVm.Name = DataFromSource.Rows[i]["Name"].ToString();
                    PersonVm.Address = DataFromSource.Rows[i]["address1"].ToString();
                    PersonVm.Code = DataFromSource.Rows[i]["buyercode"].ToString();
                    PersonVm.LedgerAccountGroupId = LedgerAccountGroupConstants.SundryCreditors.LedgerAccountGroupId;
                    PersonVm.ProcessId = ProcessConstants.Sale.ProcessId;
                    PersonVm.IsActive = true;
                    PersonVm.IsSisterConcern = false;
                    PersonVm.SalesTaxGroupPartyId = ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId;
                    PersonVm.CreatedBy = SystemUser;
                    PersonVm.ModifiedBy = SystemUser;
                    PersonVm.CreatedDate = System.DateTime.Now;
                    PersonVm.ModifiedDate = System.DateTime.Now;

                    SqlDataTransaction = SqlDataConnection.BeginTransaction("");

                    ImportPerson(PersonVm);

                    SqlDataTransaction.Commit();
                }
                SqlDataConnection.Close();
                _conn.Close();
            }
            catch (Exception ex)
            {
                SqlDataTransaction.Rollback();
                SqlDataConnection.Close();
                _conn.Close();
                RecordError(ex);
            }
        }
        public void ImportSupplier()
        {
            SqlDataConnection.Open();
            _conn.Open();
            try
            {
                DataTable DataFromSource = new DataTable();
                mQry = "Select * From supplier";
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(mQry, _conn))
                {
                    adapter.Fill(DataFromSource);
                }

                for (int i = 0; i <= DataFromSource.Rows.Count - 1; i++)
                {
                    PersonViewModel PersonVm = new PersonViewModel();
                    PersonVm.DocTypeId = DocumentTypeConstants.Supplier.DocumentTypeId;
                    PersonVm.Name = DataFromSource.Rows[i]["Name"].ToString();
                    PersonVm.Address = DataFromSource.Rows[i]["Address"].ToString();
                    PersonVm.Code = DataFromSource.Rows[i]["code"].ToString();
                    PersonVm.LedgerAccountGroupId = LedgerAccountGroupConstants.SundryDebtors.LedgerAccountGroupId;
                    PersonVm.ProcessId = ProcessConstants.Purchase.ProcessId;
                    PersonVm.IsActive = true;
                    PersonVm.IsSisterConcern = false;
                    PersonVm.SalesTaxGroupPartyId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
                    PersonVm.CreatedBy = SystemUser;
                    PersonVm.ModifiedBy = SystemUser;
                    PersonVm.CreatedDate = System.DateTime.Now;
                    PersonVm.ModifiedDate = System.DateTime.Now;

                    SqlDataTransaction = SqlDataConnection.BeginTransaction("");


                    ImportPerson(PersonVm);

                    SqlDataTransaction.Commit();
                }
                SqlDataConnection.Close();
                _conn.Close();
            }
            catch (Exception ex)
            {
                SqlDataTransaction.Rollback();
                SqlDataConnection.Close();
                _conn.Close();
                RecordError(ex);
            }
        }
        public void ImportFinishing()
        {
            SqlDataConnection.Open();
            _conn.Open();
                try
                {
                    DataTable DataFromSource = new DataTable();
                    mQry = "Select * From finishing";
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(mQry, _conn))
                    {
                        adapter.Fill(DataFromSource);
                    }

                    for (int i = 0; i <= DataFromSource.Rows.Count - 1; i++)
                    {
                        PersonViewModel PersonVm = new PersonViewModel();
                        PersonVm.DocTypeId = DocumentTypeConstants.JobWorker.DocumentTypeId;
                        PersonVm.Name = DataFromSource.Rows[i]["Name"].ToString();
                        PersonVm.Address = DataFromSource.Rows[i]["Address"].ToString();
                        PersonVm.Code = DataFromSource.Rows[i]["code"].ToString();
                        PersonVm.LedgerAccountGroupId = LedgerAccountGroupConstants.SundryDebtors.LedgerAccountGroupId;
                        PersonVm.ProcessId = ProcessConstants.Purchase.ProcessId;
                        PersonVm.IsActive = true;
                        PersonVm.IsSisterConcern = false;
                        PersonVm.SalesTaxGroupPartyId = ChargeGroupPersonConstants.ExStateRegistered.ChargeGroupPersonId;
                        PersonVm.CreatedBy = SystemUser;
                        PersonVm.ModifiedBy = SystemUser;
                        PersonVm.CreatedDate = System.DateTime.Now;
                        PersonVm.ModifiedDate = System.DateTime.Now;

                        SqlDataTransaction = SqlDataConnection.BeginTransaction("");

                        ImportPerson(PersonVm);

                        SqlDataTransaction.Commit();
                    }
                    SqlDataConnection.Close();
                _conn.Close();
                }
                catch (Exception ex)
                {
                    SqlDataTransaction.Rollback();
                    SqlDataConnection.Close();
                    _conn.Close();
                    RecordError(ex);
                }
        }
        public void ImportPerson(PersonViewModel vm)
        {
            if ((int)ExecuteScaler("SELECT Count(*) AS Cnt FROM Web.People WHERE Code = '" + vm.Code + "'") == 0)
            {
                
                mQry = @"INSERT INTO Web.People (DocTypeId, Name, Suffix, Code, Description, Phone, Mobile, Email, IsActive, Tags, ImageFolderName, ImageFileName, IsSisterConcern, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, OMSId, Nature, ApplicationUser_Id)
                        Select " + DocumentTypeConstants.JobWorker.DocumentTypeId + " As  DocTypeId, '" + vm.Name + "' As Name, '" + vm.Address + "' As Suffix, '" + vm.Code + @"' As Code, 
                        Null As Description, Null As Phone, Null As Mobile, Null As Email, 1 As IsActive, Null As Tags, Null As ImageFolderName, 
                        Null As ImageFileName, 0 As IsSisterConcern, '" + vm.CreatedBy + @"' As CreatedBy, '" + vm.ModifiedBy + @"' As ModifiedBy, getdate() As CreatedDate, getdate() As ModifiedDate, 
                        Null As OMSId, Null As Nature, Null As ApplicationUser_Id ";
                ExecuteQuery(mQry);


                mQry = @"INSERT INTO Web.BusinessEntities (PersonID, ParentId, TdsCategoryId, TdsGroupId, GuarantorId, SalesTaxGroupPartyId, IsSisterConcern, PersonRateGroupId, ServiceTaxCategoryId, CreaditDays, CreaditLimit, DivisionIds, SiteIds, OMSId, Buyer_PersonID)
                        Select P.PersonID, Null As ParentId, Null As TdsCategoryId, Null As TdsGroupId, Null As GuarantorId,  " + ChargeGroupPersonConstants.StateUnRegistered.ChargeGroupPersonId + @" As SalesTaxGroupPartyId, 
                        0 As IsSisterConcern, Null As PersonRateGroupId, 
                        Null As ServiceTaxCategoryId, Null As CreaditDays, Null As CreaditLimit, Null As DivisionIds, Null As SiteIds, Null As OMSId, Null As Buyer_PersonID 
                        FROM Web.People P 
                        LEFT JOIN Web.BusinessEntities Be ON P.PersonID = Be.PersonID
                        WHERE Be.PersonID IS NULL And P.Code = '" + vm.Code + @"' ";
                ExecuteQuery(mQry);

                mQry = @"INSERT INTO Web.PersonAddresses (PersonId, AddressType, Address, CityId, AreaId, Zipcode, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, OMSId)
                        Select P.PersonId, Null As AddressType, Null As Address, Null As CityId, Null As AreaId, Null As Zipcode, '" + vm.CreatedBy + @"' As CreatedBy, '" + vm.ModifiedBy + @"' As ModifiedBy, getdate() As CreatedDate, getdate() As ModifiedDate, P.OMSId 
                        FROM Web.People P 
                        LEFT JOIN Web.PersonAddresses Be ON P.PersonID = Be.PersonID
                        WHERE Be.PersonID IS NULL And P.Code = '" + vm.Code + @"'  ";
                ExecuteQuery(mQry);

                mQry = @"INSERT INTO Web.LedgerAccounts (LedgerAccountId, LedgerAccountName, LedgerAccountSuffix, PersonId, LedgerAccountGroupId, IsActive, IsSystemDefine, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, OMSId, ProductId)
                        SELECT (Select Max(LedgerAccountId) + 1 From Web.LedgerAccounts) As LedgerAccountId, '" + vm.Name + @"' As LedgerAccountName, '" + vm.Address + @"' As LedgerAccountSuffix, 
                        P.PersonId As PersonId, " + vm.LedgerAccountGroupId + @" As LedgerAccountGroupId, 1 As IsActive, 
                        0 As IsSystemDefine, '" + SystemUser + @"' As CreatedBy, '" + SystemUser + @"' As ModifiedBy,  getdate() As CreatedDate, getdate() As ModifiedDate, Null As OMSId, Null As ProductId
                        FROM Web.People P 
                        LEFT JOIN Web.LedgerAccounts Be ON P.PersonID = Be.PersonID
                        WHERE Be.PersonID IS NULL And P.Code = '" + vm.Code + @"'  ";
                ExecuteQuery(mQry);

                mQry = @"INSERT INTO Web.PersonRoles (PersonId, RoleDocTypeId, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, OMSId)
                        Select P.PersonId, " + vm.DocTypeId + @" RoleDocTypeId, '" + SystemUser + @"' As CreatedBy, '" + SystemUser + @"' As ModifiedBy,  getdate() As CreatedDate, getdate() As ModifiedDate, Null As OMSId 
                        FROM Web.People P 
                        LEFT JOIN Web.PersonRoles Pr ON P.PersonID = Pr.PersonID
                        WHERE Pr.PersonID IS NULL And P.Code = '" + vm.Code + @"'  ";
                ExecuteQuery(mQry);

                mQry = @"INSERT INTO Web.PersonProcesses (PersonId, ProcessId, PersonRateGroupId, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, OMSId)
                        SELECT P.PersonId, " + vm.ProcessId + @"ProcessId, Null As PersonRateGroupId, '" + SystemUser + @"' As CreatedBy, '" + SystemUser + @"' As ModifiedBy,  getdate() As CreatedDate, getdate() As ModifiedDate, Null As OMSId
                        FROM Web.People P 
                        LEFT JOIN Web.PersonProcesses PP ON P.PersonID = Pp.PersonId
                        WHERE PP.PersonProcessId IS NULL And P.Code = '" + vm.Code + @"'  ";
                ExecuteQuery(mQry);

                mQry = @"INSERT INTO Web.PersonRegistrations (PersonId, RegistrationType, RegistrationNo, RegistrationDate, ExpiryDate, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, OMSId)
                        SELECT P.PersonId, 'Pan No' As RegistrationType, '" + vm.PanNo + @"' As RegistrationNo, Null As RegistrationDate, 
                        Null As ExpiryDate, '" + SystemUser + @"' As CreatedBy, '" + SystemUser + @"' As ModifiedBy,  getdate() As CreatedDate, getdate() As ModifiedDate, Null As OMSId
                        FROM Web.People P 
                        LEFT JOIN Web.PersonRegistrations PP ON P.PersonID = Pp.PersonId
                        WHERE PP.PersonRegistrationID IS NULL And P.Code = '" + vm.Code + @"'  ";
                ExecuteQuery(mQry);

            }
        }


        public void ImportWool1()
        {
                SqlDataConnection.Open();
            _conn.Open();
            try
            {
                DataTable DataFromSource = new DataTable();
                mQry = "Select * From wool1";
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(mQry, _conn))
                {
                    adapter.Fill(DataFromSource);
                }

                for (int i = 0; i <= DataFromSource.Rows.Count - 1; i++)
                {
                    if ((int)ExecuteScaler("SELECT Count(*) AS Cnt FROM Web.ProductGroups WHERE ProductGroupName = '" + DataFromSource.Rows[i]["Quality"] + "'") == 0)
                    {
                        SqlDataTransaction = SqlDataConnection.BeginTransaction("");

                        mQry = @"INSERT INTO Web.ProductGroups (ProductGroupName, ProductTypeId, IsSystemDefine, IsActive, ImageFolderName, ImageFileName, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, OMSId, Sr, DefaultSalesTaxProductCodeId, DefaultSalesTaxGroupProductId, RateDecimalPlaces)
                                SELECT '" + DataFromSource.Rows[i]["Quality"] + @"' ProductGroupName, 
                                (Select ProductTypeId From Web.ProductTypes Where ProductTypeName = 'Wool') As ProductTypeId, 0 As IsSystemDefine, 1 As IsActive, 
                                Null As ImageFolderName, Null As ImageFileName, '" + SystemUser + @"' As CreatedBy, '" + SystemUser + @"' As ModifiedBy, 
                                getdate() As CreatedDate, getdate() As ModifiedDate, Null As OMSId, Null As Sr, Null As DefaultSalesTaxProductCodeId, 
                                " + ChargeGroupProductConstants.GST5Per.ChargeGroupProductId + @" As DefaultSalesTaxGroupProductId, 3 As RateDecimalPlaces ";
                        ExecuteQuery(mQry);

                        SqlDataTransaction.Commit();
                    }
                }
                SqlDataConnection.Close();
                _conn.Close();
            }
            catch (Exception ex)
            {
                SqlDataTransaction.Rollback();
                SqlDataConnection.Close();
                _conn.Close();
                RecordError(ex);
            }
        }

        public void ImportWool()
        {
            SqlDataConnection.Open();
            _conn.Open();
            try
            {
                DataTable DataFromSource = new DataTable();
                mQry = "Select * From Wool";
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(mQry, _conn))
                {
                    adapter.Fill(DataFromSource);
                }

                for (int i = 0; i <= DataFromSource.Rows.Count - 1; i++)
                {
                    if ((int)ExecuteScaler("SELECT Count(*) AS Cnt FROM Web.Products WHERE ProductName = '" + DataFromSource.Rows[i]["Quality"] + "'") == 0)
                    {
                        SqlDataTransaction = SqlDataConnection.BeginTransaction("");

                        mQry = @"INSERT INTO Web.Products (ProductCode, ProductName, ProductDescription, ProductSpecification, StandardCost, SaleRate, ProductGroupId, ProductCategoryId, SalesTaxGroupProductId, SalesTaxProductCodeId, DrawBackTariffHeadId, UnitId, DivisionId, ImageFolderName, ImageFileName, GrossWeight, StandardWeight, Tags, CBM, ProfitMargin, CarryingCost, IsActive, IsSystemDefine, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, DefaultDimension1Id, DefaultDimension2Id, DefaultDimension3Id, DefaultDimension4Id, DiscontinueDate, DiscontinueReason, ReferenceDocTypeId, ReferenceDocId, OMSId)
                                Select '" + DataFromSource.Rows[i]["Quality"] + "' As ProductCode, '" + DataFromSource.Rows[i]["Quality"] + @"' As ProductName, 
                                '" + DataFromSource.Rows[i]["Quality"] + @"' As ProductDescription, Null As ProductSpecification, 0 As StandardCost, 0 As SaleRate, 
                                (Select ProductGroupId From Web.ProductGroups Where ProductGroupName = '" + DataFromSource.Rows[i]["MaterialType"] + @"') As ProductGroupId, 
                                Null As ProductCategoryId, " + ChargeGroupProductConstants.GST5Per.ChargeGroupProductId + @" As SalesTaxGroupProductId, Null As SalesTaxProductCodeId, 
                                Null As DrawBackTariffHeadId, '" + UnitConstants.KG.UnitId + @"' As UnitId, " + DivisionConstants.MainDivision.DivisionId + @" As DivisionId, 
                                Null As ImageFolderName, Null As ImageFileName, 
                                0 As GrossWeight, 0 As StandardWeight, Null As Tags, Null As CBM, 0 As ProfitMargin, 0 As CarryingCost, 1 As IsActive, 0 As IsSystemDefine, 
                                '" + SystemUser + @"' As CreatedBy, '" + SystemUser + @"' As ModifiedBy, getdate() As CreatedDate, getdate() As ModifiedDate, 
                                Null As DefaultDimension1Id, Null As DefaultDimension2Id, Null As DefaultDimension3Id, Null As DefaultDimension4Id, 
                                Null As DiscontinueDate, Null As DiscontinueReason, Null As ReferenceDocTypeId, 
                                Null As ReferenceDocId, Null As OMSId ";

                        ExecuteQuery(mQry);

                        SqlDataTransaction.Commit();
                    }
                }
                SqlDataConnection.Close();
                _conn.Close();
            }
            catch (Exception ex)
            {
                SqlDataTransaction.Rollback();
                SqlDataConnection.Close();
                _conn.Close();
                RecordError(ex);
            }
        }


        public void ImportQuality()
        {
            SqlDataConnection.Open();
            _conn.Open();
            try
            {
                DataTable DataFromSource = new DataTable();
                mQry = "Select * From Quality";
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(mQry, _conn))
                {
                    adapter.Fill(DataFromSource);
                }

                for (int i = 0; i <= DataFromSource.Rows.Count - 1; i++)
                {
                    if ((int)ExecuteScaler("SELECT Count(*) AS Cnt FROM Web.ProductQualities WHERE ProductQualityName = '" + DataFromSource.Rows[i]["Quality"] + "'") == 0)
                    {
                        SqlDataTransaction = SqlDataConnection.BeginTransaction("");

                        mQry = @"INSERT INTO Web.ProductQualities (ProductQualityName, IsActive, ProductTypeId, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, OMSId, Weight)
                                SELECT '" + DataFromSource.Rows[i]["Quality"] + @"' As ProductQualityName, 1 As IsActive, 
                                (Select ProductTypeId From Web.ProductTypes Where ProductTypeName = 'Wool') As ProductTypeId, 
                                '" + SystemUser + @"' As CreatedBy, '" + SystemUser + @"' As ModifiedBy, getdate() As CreatedDate, getdate() As ModifiedDate, 
                                Null As OMSId, " + DataFromSource.Rows[i]["weight"] + @" As Weight ";
                        ExecuteQuery(mQry);

                        SqlDataTransaction.Commit();
                    }
                }
                SqlDataConnection.Close();
                _conn.Close();
            }
            catch (Exception ex)
            {
                SqlDataTransaction.Rollback();
                SqlDataConnection.Close();
                _conn.Close();
                RecordError(ex);
            }
        }

        public void ExecuteQuery(string Qry)
        {
            SqlCommand cmd = new SqlCommand(Qry);
            cmd.Connection = SqlDataConnection;
            cmd.Transaction = SqlDataTransaction;
            cmd.ExecuteNonQuery();
        }
        public object ExecuteScaler(string Qry)
        {
            object val = null;
            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];

            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                SqlCommand cmd = new SqlCommand(Qry);
                cmd.Connection = sqlConnection;
                val = cmd.ExecuteScalar();
            }

            return val;
        }
        public void RecordError(Exception ex)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            mQry = @"INSERT INTO Web.ActivityLogs (DocId, ActivityType, Narration, UserRemark, CreatedBy, CreatedDate, DocStatus, SiteId, DivisionId)
                    SELECT 0 AS DocId, 1 AS ActivityType, 'Update Table Structure' AS Narration, '" + ex.Message.Replace("'", "") + "' AS UserRemark, '" + SystemUser + @"' AS CreatedBy, getdate() AS CreatedDate, 0 As DocStatus, " + CurrentSiteId + " As SiteId, " + CurrentDivisionId + " As DivisionId ";
            ExecuteQuery(mQry);
        }

        public void Dispose()
        {
        }

    }
}

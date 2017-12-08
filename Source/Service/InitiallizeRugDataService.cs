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
using Model.ViewModels;
using Jobs.Rug.Constants;

namespace Service
{
    public class ImportRugDataService
    {
        string mQry = "";
        ApplicationDbContext db = new ApplicationDbContext();

        public ImportRugDataService()
        {
        }
        public void InsertData()
        {
            InsertProductType();
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
                        ProductType.ProductNatureId = (int)ChildClass.GetField("ProductNatureId").GetRawConstantValue();
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
                        ProductType.ProductNatureId = (int)ChildClass.GetField("ProductNatureId").GetRawConstantValue();
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
                        Process.ProcessName = (string)ChildClass.GetField("ProcessName").GetRawConstantValue();
                        Process.ProcessCode = (string)ChildClass.GetField("ProcessCode").GetRawConstantValue();
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
                        Process.ProcessName = (string)ChildClass.GetField("ProcessName").GetRawConstantValue();
                        Process.ProcessCode = (string)ChildClass.GetField("ProcessCode").GetRawConstantValue();
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
    }
}

using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;

using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;

namespace Service
{
    public interface IDuplicateDocumentCheckService : IDisposable
    {

        bool CheckForDocNoExists(string table, string docno, int doctypeId);
        bool CheckForDocNoExists(string table, string docno, int doctypeId, int headerid);
    }

    public class DuplicateDocumentCheckService : IDuplicateDocumentCheckService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
        int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
        public bool CheckForDocNoExists(string table, string docno, int doctypeId)
        {

            switch (table)
            {
                case "Dimension1":
                    {
                        var temp = (from p in db.Dimension1
                                    where p.Dimension1Name == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "Dimension2":
                    {
                        var temp = (from p in db.Dimension2
                                    where p.Dimension2Name == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Dimension3":
                    {
                        var temp = (from p in db.Dimension3
                                    where p.Dimension3Name == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "Dimension4":
                    {
                        var temp = (from p in db.Dimension4
                                    where p.Dimension4Name == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "DesignConsumption":
                    {
                        var temp = (from p in db.Product
                                    where p.ProductName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "ProductConsumption":
                    {
                        var temp = (from p in db.Product
                                    where p.ProductName == docno + "-Bom"
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }


                case "Person":
                    {
                        var temp = (from p in db.Persons
                                    where p.Name == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "PersonRateGroup":
                    {
                        int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                        int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                        var temp = (from p in db.PersonRateGroup
                                    where p.PersonRateGroupName == docno && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductUidMachine":
                    {
                        
                        var temp = (from p in db.ProductUid
                                    where p.ProductUidName == docno && p.GenDocTypeId== doctypeId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductRateGroup":
                    {
                        int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                        int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                        var temp = (from p in db.ProductRateGroup
                                    where p.ProductRateGroupName == docno && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "RateListHeader":
                    {
                        int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                        int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                        var temp = (from p in db.RateListHeader
                                    where p.RateListName == docno && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "SaleOrderCancelHeader":
                    {
                        var temp = (from p in db.SaleOrderCancelHeader
                                    where p.DocNo == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "SaleOrderHeader":
                    {
                        var temp = (from p in db.SaleOrderHeader
                                    where p.DocNo == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "SaleEnquiryHeader":
                    {
                        var temp = (from p in db.SaleEnquiryHeader
                                    where p.DocNo == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "JobOrders":
                    {
                        var temp = (from p in db.JobOrderHeader
                                    where p.DocNo == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "JobOrderCostCenter":
                    {
                        var temp = (from p in db.JobOrderHeader
                                    where p.CostCenter.CostCenterName == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProdOrderHeader":
                    {
                        var temp = (from p in db.ProdOrderHeader
                                    where p.DocNo == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "SaleInvoiceHeader":
                    {
                        var temp = (from p in db.SaleInvoiceHeader
                                    where p.DocNo == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "MaterialPlanHeader":
                    {
                        var temp = (from p in db.MaterialPlanHeader
                                    where p.DocNo == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "PackingHeader":
                    {
                        var temp = (from p in db.PackingHeader
                                    where p.DocNo == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "AttendanceHeader":
                    {
                        var temp = (from p in db.AttendanceHeader
                                    where p.DocNo == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "OverTimeApplicationHeader":
                    {
                        var temp = (from p in db.OverTimeApplicationHeader
                                    where p.DocNo == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "GatePassHeaders":
                    {
                        var temp = (from p in db.GatePassHeader
                                    where p.DocNo == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "QAGroups":
                    {
                        var temp = (from p in db.QAGroup
                                    where p.QaGroupName == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) 
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "DispatchWaybillHeader":
                    {
                        var temp = (from p in db.DispatchWaybillHeader
                                    where p.DocNo == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }


                case "Division":
                    {
                        var temp = (from p in db.Divisions
                                    where p.DivisionName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "ShipMethod":
                    {
                        var temp = (from p in db.ShipMethod
                                    where p.ShipMethodName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Site":
                    {
                        var temp = (from p in db.Site
                                    where p.SiteName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Godown":
                    {
                        var temp = (from p in db.Godown
                                    where p.GodownName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "DeliveryTerms":
                    {
                        var temp = (from p in db.DeliveryTerms
                                    where p.DeliveryTermsName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Narration":
                    {
                        var temp = (from p in db.Narration
                                    where p.NarrationName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Country":
                    {
                        var temp = (from p in db.Country
                                    where p.CountryName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "State":
                    {
                        var temp = (from p in db.State
                                    where p.StateName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Reason":
                    {
                        var temp = (from p in db.Reason
                                    where p.ReasonName == docno && p.DocumentCategoryId == doctypeId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Process":
                    {
                        var temp = (from p in db.Process
                                    where p.ProcessName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "PersonContactType":
                    {
                        var temp = (from p in db.PersonContactType
                                    where p.PersonContactTypeName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductCategory":
                    {
                        var temp = (from p in db.ProductCategory
                                    where p.ProductCategoryName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductSizeType":
                    {
                        var temp = (from p in db.ProductSizeType
                                    where p.ProductSizeTypeName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductStyle":
                    {
                        var temp = (from p in db.ProductStyle
                                    where p.ProductStyleName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductShape":
                    {
                        var temp = (from p in db.ProductShape
                                    where p.ProductShapeName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductNature":
                    {
                        var temp = (from p in db.ProductNature
                                    where p.ProductNatureName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductType":
                    {
                        var temp = (from p in db.ProductTypes
                                    where p.ProductTypeName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "SalesTaxGroupParty":
                    {
                        var temp = (from p in db.SalesTaxGroupParty
                                    where p.SalesTaxGroupPartyName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "SalesTaxGroupProduct":
                    {
                        var temp = (from p in db.SalesTaxGroupProduct
                                    where p.SalesTaxGroupProductName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "DocumentType":
                    {
                        var temp = (from p in db.DocumentType
                                    where p.DocumentTypeName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Currency":
                    {
                        var temp = (from p in db.Currency
                                    where p.Name == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Size":
                    {
                        var temp = (from p in db.Size
                                    where p.SizeName == docno && p.ProductShapeId == doctypeId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Product":
                    {
                        var temp = (from p in db.Product
                                    where p.ProductName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "DocumentCategory":
                    {
                        var temp = (from p in db.DocumentCategory
                                    where p.DocumentCategoryName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "CostCenter":
                    {
                        var temp = (from p in db.CostCenter
                                    where p.CostCenterName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "AccountGroup":
                    {
                        var temp = (from p in db.LedgerAccountGroup
                                    where p.LedgerAccountGroupName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "LedgerAccounts":
                    {
                        var temp = (from p in db.LedgerAccount
                                    where p.LedgerAccountName + p.LedgerAccountSuffix == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ChargeTypes":
                    {
                        var temp = (from p in db.ChargeType
                                    where p.ChargeTypeName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ChargeGroupProducts":
                    {
                        var temp = (from p in db.ChargeGroupProduct
                                    where p.ChargeGroupProductName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ChargeGroupPersons":
                    {
                        var temp = (from p in db.ChargeGroupPerson
                                    where p.ChargeGroupPersonName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductGroup":
                    {
                        var temp = (from p in db.ProductGroups
                                    where p.ProductGroupName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductDesign":
                    {
                        var temp = (from p in db.ProductDesigns
                                    where p.ProductDesignName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductCollection":
                    {
                        var temp = (from p in db.ProductCollections
                                    where p.ProductCollectionName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductQuality":
                    {
                        var temp = (from p in db.ProductQuality
                                    where p.ProductQualityName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductInvoiceGroup":
                    {
                        var temp = (from p in db.ProductInvoiceGroup
                                    where p.ProductInvoiceGroupName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "JobReceiveHeaders":
                    {
                        var temp = (from p in db.JobReceiveHeader
                                    where p.DocNo == docno && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "LeaveType":
                    {
                        var temp = (from p in db.LeaveType
                                    where p.LeaveTypeName == docno
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                default: return false;
            }
        }
        public bool CheckForDocNoExists(string table, string docno, int doctypeId, int headerid)
        {

            switch (table)
            {
                case "Dimension1":
                    {
                        var temp = (from p in db.Dimension1
                                    where p.Dimension1Name == docno && p.Dimension1Id != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "Dimension2":
                    {
                        var temp = (from p in db.Dimension2
                                    where p.Dimension2Name == docno && p.Dimension2Id != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "Dimension3":
                    {
                        var temp = (from p in db.Dimension3
                                    where p.Dimension3Name == docno && p.Dimension3Id != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "Dimension4":
                    {
                        var temp = (from p in db.Dimension4
                                    where p.Dimension4Name == docno && p.Dimension4Id != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "DesignConsumption":
                    {
                        var temp = (from p in db.Product
                                    where p.ProductName == docno && p.ProductId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "ProductUidMachine":
                    {
                        var temp = (from p in db.ProductUid
                                    where p.ProductUidName == docno && p.ProductUIDId != headerid && p.GenDocTypeId== doctypeId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "ProductConsumption":
                    {
                        var temp = (from p in db.Product
                                    where p.ProductName == docno + "-Bom" && p.ProductId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "Person":
                    {
                        var temp = (from p in db.Persons
                                    where p.Code == docno && p.PersonID != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "PersonRateGroup":
                    {
                        var temp = (from p in db.PersonRateGroup
                                    where p.PersonRateGroupName == docno && p.PersonRateGroupId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductRateGroup":
                    {
                        var temp = (from p in db.ProductRateGroup
                                    where p.ProductRateGroupName == docno && p.ProductRateGroupId != headerid
                                    && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "RateListHeader":
                    {
                        var temp = (from p in db.RateListHeader
                                    where p.RateListName == docno && p.RateListHeaderId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "SaleOrderCancelHeader":
                    {
                        var temp = (from p in db.SaleOrderCancelHeader
                                    where p.DocNo == docno && p.SaleOrderCancelHeaderId != headerid && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "SaleOrderHeader":
                    {
                        var temp = (from p in db.SaleOrderHeader
                                    where p.DocNo == docno && p.SaleOrderHeaderId != headerid && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "SaleEnquiryHeader":
                    {
                        var temp = (from p in db.SaleEnquiryHeader
                                    where p.DocNo == docno && p.SaleEnquiryHeaderId != headerid && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "QAGroups":
                    {
                        var temp = (from p in db.QAGroup
                                    where p.QaGroupName == docno && p.QAGroupId != headerid && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) 
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "JobOrders":
                    {
                        var temp = (from p in db.JobOrderHeader
                                    where p.DocNo == docno && p.JobOrderHeaderId != headerid && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "JobOrderCostCenter":
                    {
                        var temp = (from p in db.JobOrderHeader
                                    where p.CostCenter.CostCenterName == docno && p.JobOrderHeaderId != headerid && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProdOrderHeader":
                    {
                        var temp = (from p in db.ProdOrderHeader
                                    where p.DocNo == docno && p.ProdOrderHeaderId != headerid && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "SaleInvoiceHeader":
                    {
                        var temp = (from p in db.SaleInvoiceHeader
                                    where p.DocNo == docno && p.SaleInvoiceHeaderId != headerid && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "MaterialPlanHeader":
                    {
                        var temp = (from p in db.MaterialPlanHeader
                                    where p.DocNo == docno && p.MaterialPlanHeaderId != headerid && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "PackingHeader":
                    {
                        var temp = (from p in db.PackingHeader
                                    where p.DocNo == docno && p.PackingHeaderId != headerid && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "DispatchWaybillHeader":
                    {
                        var temp = (from p in db.DispatchWaybillHeader
                                    where p.DocNo == docno && p.DispatchWaybillHeaderId != headerid && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "ShipMethod":
                    {
                        var temp = (from p in db.ShipMethod
                                    where p.ShipMethodName == docno && p.ShipMethodId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Division":
                    {
                        var temp = (from p in db.Divisions
                                    where p.DivisionName == docno && p.DivisionId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Site":
                    {
                        var temp = (from p in db.Site
                                    where p.SiteName == docno && p.SiteId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Godown":
                    {
                        var temp = (from p in db.Godown
                                    where p.GodownName == docno && p.GodownId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }

                case "Country":
                    {
                        var temp = (from p in db.Country
                                    where p.CountryName == docno && p.CountryId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "State":
                    {
                        var temp = (from p in db.State
                                    where p.StateName == docno && p.StateId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Process":
                    {
                        var temp = (from p in db.Process
                                    where p.ProcessName == docno && p.ProcessId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "DeliveryTerms":
                    {
                        var temp = (from p in db.DeliveryTerms
                                    where p.DeliveryTermsName == docno && p.DeliveryTermsId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Narration":
                    {
                        var temp = (from p in db.Narration
                                    where p.NarrationName == docno && p.NarrationId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "PersonContactType":
                    {
                        var temp = (from p in db.PersonContactType
                                    where p.PersonContactTypeName == docno && p.PersonContactTypeId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductSizeType":
                    {
                        var temp = (from p in db.ProductSizeType
                                    where p.ProductSizeTypeName == docno && p.ProductSizeTypeId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductShape":
                    {
                        var temp = (from p in db.ProductShape
                                    where p.ProductShapeName == docno && p.ProductShapeId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductNature":
                    {
                        var temp = (from p in db.ProductNature
                                    where p.ProductNatureName == docno && p.ProductNatureId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductType":
                    {
                        var temp = (from p in db.ProductTypes
                                    where p.ProductTypeName == docno && p.ProductTypeId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "SalesTaxGroupParty":
                    {
                        var temp = (from p in db.SalesTaxGroupParty
                                    where p.SalesTaxGroupPartyName == docno && p.SalesTaxGroupPartyId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "SalesTaxGroupProduct":
                    {
                        var temp = (from p in db.SalesTaxGroupProduct
                                    where p.SalesTaxGroupProductName == docno && p.SalesTaxGroupProductId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "DocumentType":
                    {
                        var temp = (from p in db.DocumentType
                                    where p.DocumentTypeName == docno && p.DocumentTypeId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Currency":
                    {
                        var temp = (from p in db.Currency
                                    where p.Name == docno && p.ID != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Size":
                    {
                        var temp = (from p in db.Size
                                    where p.SizeName == docno && p.ProductShapeId == doctypeId && p.SizeId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Product":
                    {
                        var temp = (from p in db.Product
                                    where p.ProductName == docno && p.ProductId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "DocumentCategory":
                    {
                        var temp = (from p in db.DocumentCategory
                                    where p.DocumentCategoryName == docno && p.DocumentCategoryId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "CostCemter":
                    {
                        var temp = (from p in db.CostCenter
                                    where p.CostCenterName == docno && p.CostCenterId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "AccountGroup":
                    {
                        var temp = (from p in db.LedgerAccountGroup
                                    where p.LedgerAccountGroupName == docno && p.LedgerAccountGroupId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "LedgerAccounts":
                    {
                        var temp = (from p in db.LedgerAccount
                                    where p.LedgerAccountName + p.LedgerAccountSuffix == docno && p.LedgerAccountId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                
                case "ChargeTypes":
                    {
                        var temp = (from p in db.ChargeType
                                    where p.ChargeTypeName == docno && p.ChargeTypeId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ChargeGroupProducts":
                    {
                        var temp = (from p in db.ChargeGroupProduct
                                    where p.ChargeGroupProductName == docno && p.ChargeGroupProductId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductStyle":
                    {
                        var temp = (from p in db.ProductStyle
                                    where p.ProductStyleName == docno && p.ProductStyleId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ChargeGroupPersons":
                    {
                        var temp = (from p in db.ChargeGroupPerson
                                    where p.ChargeGroupPersonName == docno && p.ChargeGroupPersonId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductGroup":
                    {
                        var temp = (from p in db.ProductGroups
                                    where p.ProductGroupName == docno && p.ProductGroupId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "GatePassHeaders":
                    {
                        var temp = (from p in db.GatePassHeader
                                    where p.DocNo == docno && p.GatePassHeaderId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductDesign":
                    {
                        var temp = (from p in db.ProductDesigns
                                    where p.ProductDesignName == docno && p.ProductDesignId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductCategory":
                    {
                        var temp = (from p in db.ProductCategory
                                    where p.ProductCategoryName == docno && p.ProductCategoryId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductCollection":
                    {
                        var temp = (from p in db.ProductCollections
                                    where p.ProductCollectionName == docno && p.ProductCollectionId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductQuality":
                    {
                        var temp = (from p in db.ProductQuality
                                    where p.ProductQualityName == docno && p.ProductQualityId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "ProductInvoiceGroup":
                    {
                        var temp = (from p in db.ProductInvoiceGroup
                                    where p.ProductInvoiceGroupName == docno && p.ProductInvoiceGroupId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "JobReceiveHeaders":
                    {
                        var temp = (from p in db.JobReceiveHeader
                                    where p.DocNo == docno && p.JobReceiveHeaderId != headerid && ((doctypeId == null) ? 1 == 1 : p.DocTypeId == doctypeId) && p.SiteId == SiteId && p.DivisionId == DivisionId
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
             
                case "LeaveType":
                    {
                        var temp = (from p in db.LeaveType
                                    where p.LeaveTypeName == docno && p.LeaveTypeId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                case "Reason":
                    {
                        var temp = (from p in db.Reason
                                    where p.ReasonName == docno && p.DocumentCategoryId == doctypeId && p.ReasonId != headerid
                                    select p).FirstOrDefault();
                        if (temp == null)
                            return false;
                        else
                            return true;
                    }
                default: return false;
            }
        }

        public void Dispose()
        {
        }

    }

}

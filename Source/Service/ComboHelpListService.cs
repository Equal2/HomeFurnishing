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
using Model.ViewModels;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
namespace Service
{
    public interface IComboHelpListService : IDisposable
    {
        IEnumerable<ComboBoxList> GetSiteHelpList();
        IEnumerable<ComboBoxList> GetGodownHelpList();
        IEnumerable<ComboBoxList> GetGateHelpList();
        IEnumerable<ComboBoxList> GetSiteGodownHelpList(int id);
        IEnumerable<ComboBoxList> GetCalculationHelpList();
        IEnumerable<ComboBoxList> GetPerkHelpList();
        IEnumerable<ComboBoxList> GetDocumentTypeHelpList();
        IEnumerable<ComboBoxList> GetDocumentCategoryHelpList();
        IEnumerable<ComboBoxList> GetDepartmentHelpList();
        IEnumerable<ComboBoxList> GetDivisionHelpList();
        IEnumerable<ComboBoxList> GetCurrencyHelpList();
        IEnumerable<ComboBoxList> GetBuyerHelpList();

        IEnumerable<ComboBoxList> GetShipMethodHelpList();

        IEnumerable<ComboBoxList> GetTransporterHelpList();
        IEnumerable<ComboBoxList> GetSupplierHelpList();
        IEnumerable<ComboBoxList> GetMenuHelpList();
        CustomComboBoxPagedResult GetSelect2HelpList(string SqlProcGet, string searchTerm, int pageSize, int pageNum);
        IEnumerable<ComboBoxList> GetJobWorkerHelpList();
        IEnumerable<ComboBoxList> GetJobWorkerHelpList_WithProcess(int ProcessId);
        IEnumerable<ComboBoxList> GetEmployeeHelpList();

        IEnumerable<ComboBoxList> GetPurchaseIndentHelpList();
        IEnumerable<ComboBoxList> GetPurchaseIndentCancelHelpList();
        IEnumerable<ComboBoxList> GetPurchaseOrderHelpList();
        IEnumerable<ComboBoxList> GetProdOrderHelpList();
        IEnumerable<ComboBoxList> GetBalanceProdOrderHelpList();
        IEnumerable<ComboBoxList> GetPurchaseOrderCancelHelpList();
        IEnumerable<ComboBoxList> GetPurchaseGoodsReceiptHelpList();
        IEnumerable<ComboBoxList> GetPurchaseGoodsReturnHelpList();
        IEnumerable<ComboBoxList> GetPurchaseInvoiceHelpList();
        IEnumerable<ComboBoxList> GetPurchaseInvoiceReturnHelpList();

        IEnumerable<ComboBoxList> GetProductNatureHelpList();
        IEnumerable<ComboBoxList> GetProductCategoryHelpList();
        IEnumerable<ComboBoxList> GetDimension1HelpList();
        IEnumerable<ComboBoxList> GetDimension2HelpList();
        IEnumerable<ComboBoxList> GetDimension3HelpList();
        IEnumerable<ComboBoxList> GetDimension4HelpList();

        IEnumerable<ComboBoxList> GetProductTypeHelpList();
        IEnumerable<ComboBoxList> GetProductCollectionHelpList();
        IEnumerable<ComboBoxList> GetProductQualityHelpList();
        IEnumerable<ComboBoxList> GetProductGroupHelpList(int? filter);
        IEnumerable<ComboBoxList> GetProductCategoryHelpList(int? filter);
        IEnumerable<ComboBoxList> GetProductGroupForRugHelpList();
        //IEnumerable<ComboBoxList> GetProductGroupWithTypeFilterHelpList(int filterid);
        IEnumerable<ComboBoxList> GetProductStyleHelpList();
        IEnumerable<ComboBoxList> GetProductDesignHelpList();
        IEnumerable<ComboBoxList> GetProductShapeHelpList();
        IEnumerable<ComboBoxList> GetProductSizeHelpList();
        IEnumerable<ComboBoxList> GetProductHelpList();
        IEnumerable<ComboBoxList> GetProductUidHelpList();
        IEnumerable<ComboBoxList> GetFinishedProductDivisionWiseHelpList();
        IEnumerable<ComboBoxList> GetProductInvoiceGroupHelpList();
        IEnumerable<ComboBoxList> GetProductInvoiceGroupDivisionWiseHelpList();
        IEnumerable<ComboBoxList> GetProductInvoiceGroupDivisionWiseExcludeSampleHelpList();
        IEnumerable<ComboBoxList> GetProductCustomGroupHelpList();
        IEnumerable<ComboBoxList> GetPersonCustomGroupHelpList();
        IEnumerable<ComboBoxList> GetTagHelpList();
        IEnumerable<ComboBoxList> GetSaleOrderHelpList();
        IEnumerable<ComboBoxList> GetSaleOrderDivisionWistHelpList();
        IEnumerable<ComboBoxList> GetProdOrderHelpList(int SiteId, int DivisionId);
        IEnumerable<ComboBoxList> GetProdOrderHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId);
        IEnumerable<ComboBoxList> GetProdOrderCancelHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId);
        IEnumerable<ComboBoxList> GetMaterialPlanHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId);


        IEnumerable<ComboBoxList> GetSaleOrderAmendmentHelpList();
        IEnumerable<ComboBoxList> GetSaleOrderCancelHelpList();
        IEnumerable<ComboBoxList> GetSaleInvoiceHelpList();
        IEnumerable<ComboBoxList> GetSaleOrderDocumentTypeHelpList();
        IEnumerable<ComboBoxList> GetSaleOrderPlanDocumentTypeHelpList();
        IEnumerable<ComboBoxList> GetSaleInvoiceDocumentTypeHelpList();
        IEnumerable<ComboBoxList> GetSaleOrderAmendmentDocumentTypeHelpList();
        IEnumerable<ComboBoxList> GetSaleOrderCancelDocumentTypeHelpList();
        IEnumerable<ComboBoxList> GetPurchaseOrderDocumentTypeHelpList();
        IEnumerable<ComboBoxList> GetPurchaseIndentDocumentTypeHelpList();
        IEnumerable<ComboBoxList> GetDocumentTypeHelpList(string TransactionDocCategoryConstants);
        IEnumerable<ComboBoxList> SqlProcsHelpList();
        IEnumerable<ComboBoxList> GetProductManufacturerHelpList();
        IEnumerable<ComboBoxList> GetProductDrawBackTarrifHelpList();
        IEnumerable<ComboBoxList> GetProductProcessSequenceHelpList();
        IEnumerable<ComboBoxList> GetReasonHelpList();
        IEnumerable<ComboBoxList> GetSizeHelpList();
        IEnumerable<ComboBoxList> GetProcessHelpList();
        IEnumerable<ComboBoxList> GetProcessWithChildProcessHelpList(int? filter);
        IEnumerable<ComboBoxList> GetMachineHelpList();
        //IEnumerable<ComboBoxList> GetCityHelpList();
        IQueryable<ComboBoxResult> GetCityHelpList(string term);
        IEnumerable<ComboBoxList> GetStateHelpList();
        IEnumerable<ComboBoxList> GetCountryHelpList();
        //IEnumerable<ComboBoxList> GetPersonHelpList();
        IQueryable<ComboBoxResult> GetPersonHelpList(string term);
        
        IEnumerable<ComboBoxList> GetPersonBEHelpList();
        IEnumerable<ComboBoxList> GetPersonRateGroupHelpList();
        IEnumerable<ComboBoxList> GetAccountGroupHelpList();
        IEnumerable<ComboBoxList> GetColourWaysHelpList();
        IEnumerable<ComboBoxList> GetColourWaysForStencilHelpList();
        IEnumerable<ComboBoxList> GetRugQualityHelpList();
        IEnumerable<ComboBoxList> GetRugCollectionHelpList();
        IEnumerable<ComboBoxList> GetProductConstructionHelpList();
        IEnumerable<ComboBoxList> GetAccountHelpList();
        IEnumerable<ComboBoxList> GetCostCenterHelpList();
        IEnumerable<ComboBoxList> GetCalculationFooterHelpList();
        IEnumerable<ComboBoxList> GetCalculationProductHelpList();
        IEnumerable<ComboBoxList> GetPackingHelpList();
        IEnumerable<ComboBoxList> GetAccountGroupsHelpList();
        IEnumerable<ComboBoxList> GetDesignPatternHelList();
        IEnumerable<ComboBoxList> GetDescriptionOfGoodsHelList();
        IEnumerable<ComboBoxList> GetColoursHelList();
        IEnumerable<ComboBoxList> GetProductContentHeaderHelList();
        IEnumerable<ComboBoxList> GetRawMaterialProductGroupHelpList();
        IEnumerable<ComboBoxList> GetOtherMaterialProductGroupHelpList();
        IEnumerable<ComboBoxList> GetOtherProductCategoryHelpList();
        IEnumerable<ComboBoxList> GetRawProductCategoryHelpList();
        IEnumerable<ComboBoxList> GetRawMaterialHelpList();
        IEnumerable<ComboBoxList> GetBomHelpList();
        IEnumerable<ComboBoxList> GetFinishedMaterialHelpList();
        IEnumerable<ComboBoxList> GetFinishedMaterialDivisionWiseHelpList();
        IEnumerable<ComboBoxList> GetRouteHelpList();
        IEnumerable<ComboBoxList> GetBomMaterialHelpList();
        IEnumerable<ComboBoxList> GetProductsHelpList(string term);
        IEnumerable<ComboBoxList> GetBarcodeGenerateDocIdHelpList();
        IEnumerable<ComboBoxList> GetSampleHelpList();

        IEnumerable<ComboBoxList> GetJobOrderHelpList(int SiteId, int DivisionId);
        IEnumerable<ComboBoxList> GetJobReceiveHelpList(int SiteId, int DivisionId);
        IEnumerable<ComboBoxList> GetJobInvoiceHelpList(int SiteId, int DivisionId);

        IEnumerable<ComboBoxList> GetCostCenterHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId);
        IQueryable<ComboBoxResult> GetCostCenterHelpListWithDocTypes(string term, int SiteId, int DivisionId);
        IQueryable<ComboBoxResult> GetChargeTypeList(string term);
        IQueryable<ComboBoxResult> GetChargeList(string term);
        IQueryable<ComboBoxResult> GetChargeListWithCode(string term);
        IEnumerable<ComboBoxList> GetJobOrderHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId);
        IEnumerable<ComboBoxList> GetJobOrderAmendmentHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId);
        IEnumerable<ComboBoxList> GetJobReceiveHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId);
        IEnumerable<ComboBoxList> GetJobInvoiceHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId);

        IEnumerable<ComboBoxList> GetStoreIssueReceiveHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId);
        IEnumerable<ComboBoxList> GetLedgerHeaderHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId);
        IQueryable<ComboBoxResult> GetJobWorkerHelpListWithProcessFilter(int Processid, string term);
        IQueryable<ComboBoxResult> GetEmployeeHelpListWithProcessFilter(int Processid, string term);
        IQueryable<ComboBoxResult> GetPersonHelpListWithProcessFilter(int? Processid, string term);
        IQueryable<ComboBoxResult> GetPersonRateGroupHelpList(string term, int filter);
        IQueryable<ComboBoxResult> GetProductRateGroupHelpList(string term, int filter);
        IQueryable<ComboBoxResult> GetControllerActionList(string term);
        IQueryable<ComboBoxResult> GetDocumentCategoryList(string term);
        IQueryable<ComboBoxResult> GetUsers(string term);
        IEnumerable<ComboBoxList> GetPersonForSaleHelpList(string term);
        IQueryable<ComboBoxResult> GetReasonHelpListWithDocTypeFilter(int DocTypeId, string term);
        IQueryable<ComboBoxResult> GetEmployeeHelpListWithDepartMentFilter(int DepartMentId, string term);
        IQueryable<ComboBoxResult> GetQAGroups(string term);
        IQueryable<ComboBoxResult> GetUnits(string term);
        IQueryable<ComboBoxResult> GetTdsGroups(string term);
        IQueryable<ComboBoxResult> GetTdsCategory(string term);
        IQueryable<ComboBoxResult> GetSalesTaxGroupParty(string term);
        IQueryable<ComboBoxResult> GetProductHelpListWithProductNatureFilter(int ProductNatureId, string term);
        IQueryable<ComboBoxResult> GetPersonRoles(string term);
        
        
        
        IQueryable<ComboBoxResult> GetDeliveryTerms(string term);
        IQueryable<ComboBoxResult> GetAddresses(string term);
        IQueryable<ComboBoxResult> GetCurrencies(string term);
        IQueryable<ComboBoxResult> GetSalesTaxGroupPerson(string term);
        IQueryable<ComboBoxResult> GetSalesTaxGroupProduct(string term);
        IQueryable<ComboBoxResult> GetChargeType(string term);
        IQueryable<ComboBoxResult> GetShipMethods(string term);
        IQueryable<ComboBoxResult> GetDocumentShipMethods(string term);
        IQueryable<ComboBoxResult> GetTransporters(string term);
        IQueryable<ComboBoxResult> GetAgents(string term);
        IQueryable<ComboBoxResult> GetFinanciers(string term, int? filter);
        IQueryable<ComboBoxResult> GetSalesExecutives(string term);
        IQueryable<ComboBoxResult> GetChargeGroupProducts(string term);
        IQueryable<ComboBoxResult> GetSalesTaxProductCodes(string term);
        IQueryable<ComboBoxResult> GetBinLocations(string term, int filter);
        IQueryable<ComboBoxResult> GetSites(string term);
        IQueryable<ComboBoxResult> GetDivisions(string term);
        IQueryable<ComboBoxResult> GetPerson(string term);
        IEnumerable<ComboBoxResult> GetProductIndexFilterParameter(string term);

        List<ComboBoxResult> SetSelct2Data(string Id, string SqlProcSet);
        ComboBoxResult SetSingleSelect2Data(int Id, string SqlProcSet);

        IQueryable<ComboBoxResult> GetLedgerAccountForGroup(string term, int? filter);

        IQueryable<ComboBoxResult> GetAdditionalCharges(string term);
        ComboBoxPagedResult GetRoles(string searchTerm, int pageSize, int pageNum);
        ComboBoxResult GetRole(string Id);
        List<ComboBoxResult> GetMultipleRoles(string Ids);


    }

    public class ComboHelpListService : IComboHelpListService
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public int Help()
        {
            return 0;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ComboBoxList> GetSiteHelpList()
        {
            IEnumerable<ComboBoxList> Sitelist = db.Site.OrderBy(m => m.SiteName).Select(m => new
                 ComboBoxList
            {
                Id = m.SiteId,
                PropFirst = m.SiteCode
            });
            return Sitelist;
        }

        public IEnumerable<ComboBoxList> GetGodownHelpList()
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            IEnumerable<ComboBoxList> Sitelist = db.Godown.Where(m => m.SiteId == SiteId && m.IsActive == true).OrderBy(m => m.GodownName).Select(m => new
                 ComboBoxList
            {
                Id = m.GodownId,
                PropFirst = m.GodownName
            });
            return Sitelist;
        }

        public IEnumerable<ComboBoxList> GetGateHelpList()
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            IEnumerable<ComboBoxList> Sitelist = db.Gate.Where(m => m.SiteId == SiteId).OrderBy(m => m.GateName).Select(m => new
                 ComboBoxList
            {
                Id = m.GateId,
                PropFirst = m.GateName
            });
            return Sitelist;
        }

        public IEnumerable<ComboBoxList> GetSiteGodownHelpList(int id)
        {
            IEnumerable<ComboBoxList> Sitelist = db.Godown.Where(m => m.SiteId == id).OrderBy(m => m.GodownName).Select(m => new
                 ComboBoxList
            {
                Id = m.GodownId,
                PropFirst = m.GodownName
            });
            return Sitelist;
        }


        public IQueryable<ComboBoxResult> GetProductHelpListWithProductNatureFilter(int ProductNatureId, string term)
        {
            var list = ( from P in db.Product
                        join pg in db.ProductGroups on P.ProductGroupId equals pg.ProductGroupId
                        join pt in db.ProductTypes on pg.ProductTypeId equals pt.ProductTypeId
                        where pt.ProductNatureId== ProductNatureId
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (P.ProductName.ToLower().Contains(term.ToLower())))
                         orderby P.ProductName
                        select new ComboBoxResult
                        {
                            id = P.ProductId.ToString(),
                            text = P.ProductName
                        }
                      );

            return list;
        }



        public IEnumerable<ComboBoxList> GetCalculationHelpList()
        {
            IEnumerable<ComboBoxList> Sitelist = db.Calculation.OrderBy(m => m.CalculationName).Select(m => new
                 ComboBoxList
            {
                Id = m.CalculationId,
                PropFirst = m.CalculationName
            });
            return Sitelist;
        }

        public IEnumerable<ComboBoxList> GetMenuHelpList()
        {
            IEnumerable<ComboBoxList> Sitelist = db.Menu.OrderBy(m => m.MenuName).Select(m => new
                 ComboBoxList
            {
                Id = m.MenuId,
                PropFirst = m.MenuName
            });
            return Sitelist;
        }
        public IEnumerable<ComboBoxList> GetPerkHelpList()
        {
            IEnumerable<ComboBoxList> Sitelist = db.Perk.OrderBy(m => m.PerkName).Select(m => new
     ComboBoxList
            {
                Id = m.PerkId,
                PropFirst = m.PerkName
            });
            return Sitelist;
        }
        public IEnumerable<ComboBoxList> GetDocumentTypeHelpList()
        {
            IEnumerable<ComboBoxList> Sitelist = db.DocumentType.OrderBy(m => m.DocumentTypeName).Select(m => new
            ComboBoxList
            {
                Id = m.DocumentTypeId,
                PropFirst = m.DocumentTypeName
            });
            return Sitelist;
        }

        public IEnumerable<ComboBoxList> GetDocumentCategoryHelpList()
        {
            IEnumerable<ComboBoxList> Sitelist = db.DocumentCategory.OrderBy(m => m.DocumentCategoryName).Select(m => new
            ComboBoxList
            {
                Id = m.DocumentCategoryId,
                PropFirst = m.DocumentCategoryName
            });
            return Sitelist;
        }

        public IEnumerable<ComboBoxList> GetDepartmentHelpList()
        {
            IEnumerable<ComboBoxList> Departmentlist = db.Department.OrderBy(m => m.DepartmentName).Select(m => new
                 ComboBoxList
            {
                Id = m.DepartmentId,
                PropFirst = m.DepartmentName
            });
            return Departmentlist;
        }

        public IEnumerable<ComboBoxList> GetBarcodeGenerateDocIdHelpList()
        {
            IEnumerable<ComboBoxList> BarcodeGenerateDocNolist = (from PU in db.ProductUid
                                                                  join DY in db.DocumentType on PU.GenDocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                                  join P in db.Persons on PU.GenPersonId equals P.PersonID into PersonTable
                                                                  from PersonTab in PersonTable.DefaultIfEmpty()
                                                                  where PU.GenDocId != null && PU.GenDocNo != null
                                                                  group new { PU, DocumentTypeTab, PersonTab } by new { PU.GenDocId } into Result
                                                                  orderby Result.Max(i => i.PU.GenDocNo)
                                                                  select new ComboBoxList
                                                                  {
                                                                      Id = (int)Result.Key.GenDocId,
                                                                      PropFirst = Result.Max(i => i.PU.GenDocNo),
                                                                      PropSecond = Result.Max(i => i.DocumentTypeTab.DocumentTypeShortName),
                                                                      PropThird = Result.Max(i => i.PersonTab.Name)
                                                                  });




            return BarcodeGenerateDocNolist;
        }

        public IEnumerable<ComboBoxList> GetProductConstructionHelpList()
        {
            int rugid = (from p in db.ProductTypes
                         where p.ProductTypeName == ProductTypeConstants.Rug
                         select p.ProductTypeId
                          ).FirstOrDefault();
            IEnumerable<ComboBoxList> ProdCategoryList = db.ProductCategory.Where(m => m.ProductTypeId == rugid).OrderBy(m => m.ProductCategoryName).Select(m => new ComboBoxList
            {
                Id = m.ProductCategoryId,
                PropFirst = m.ProductCategoryName
            });

            return ProdCategoryList;

        }
        public IEnumerable<ComboBoxList> GetRugCollectionHelpList()
        {
            int rugid = (from p in db.ProductTypes
                         where p.ProductTypeName == ProductTypeConstants.Rug
                         select p.ProductTypeId
                          ).FirstOrDefault();
            IEnumerable<ComboBoxList> productcollectionlist = db.ProductCollections.Where(m => m.ProductTypeId == rugid).OrderBy(m => m.ProductCollectionName).Select(m => new ComboBoxList
            {
                Id = m.ProductCollectionId,
                PropFirst = m.ProductCollectionName
            });
            return productcollectionlist;

        }
        public IEnumerable<ComboBoxList> GetRugQualityHelpList()
        {
            int rugid = (from p in db.ProductTypes
                         where p.ProductTypeName == ProductTypeConstants.Rug
                         select p.ProductTypeId
                           ).FirstOrDefault();
            IEnumerable<ComboBoxList> ProdCategoryList = db.ProductQuality.Where(m => m.ProductTypeId == rugid).OrderBy(m => m.ProductQualityName).Select(m => new ComboBoxList
            {
                Id = m.ProductQualityId,
                PropFirst = m.ProductQualityName
            });
            return ProdCategoryList;

        }
        public IEnumerable<ComboBoxList> GetAccountGroupsHelpList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.LedgerAccountGroup.OrderBy(m => m.LedgerAccountGroupName).Select(m => new ComboBoxList
            {
                Id = m.LedgerAccountGroupId,
                PropFirst = m.LedgerAccountGroupName
            });

            return ProdCategoryList;
        }
        public IEnumerable<ComboBoxList> GetDesignPatternHelList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.ProductDesignPattern.OrderBy(m => m.ProductDesignPatternName).Select(m => new ComboBoxList
            {
                Id = m.ProductDesignPatternId,
                PropFirst = m.ProductDesignPatternName
            });

            return ProdCategoryList;
        }
        public IEnumerable<ComboBoxList> GetDescriptionOfGoodsHelList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.DescriptionOfGoods.OrderBy(m => m.DescriptionOfGoodsId).Select(m => new ComboBoxList
            {
                Id = m.DescriptionOfGoodsId,
                PropFirst = m.DescriptionOfGoodsName
            });

            return ProdCategoryList;
        }
        public IEnumerable<ComboBoxList> GetColoursHelList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.Colour.OrderBy(m => m.ColourName).Select(m => new ComboBoxList
            {
                Id = m.ColourId,
                PropFirst = m.ColourName
            });

            return ProdCategoryList;
        }
        public IEnumerable<ComboBoxList> GetProductContentHeaderHelList()
        {
            IEnumerable<ComboBoxList> ProdContentList = (from H in db.ProductContentHeader
                                                         join L in db.ProductContentLine on H.ProductContentHeaderId equals L.ProductContentHeaderId into ProductContentLineTable
                                                         from ProductContentLineTab in ProductContentLineTable.DefaultIfEmpty()
                                                         where ProductContentLineTab.ProductContentLineId != null
                                                         group new { H } by new { H.ProductContentHeaderId } into g
                                                         orderby g.Max(i => i.H.ProductContentName)
                                                         select new ComboBoxList
                                                         {
                                                             Id = g.Key.ProductContentHeaderId,
                                                             PropFirst = g.Max(i => i.H.ProductContentName)
                                                         });

            //IEnumerable<ComboBoxList> ProdCategoryList = db.ProductContentHeader.OrderBy(m => m.ProductContentName).Select(m => new ComboBoxList
            //{
            //    Id = m.ProductContentHeaderId,
            //    PropFirst = m.ProductContentName
            //});

            return ProdContentList;
        }
        public IEnumerable<ComboBoxList> GetColourWaysHelpList()
        {
            int rugid = (from p in db.ProductTypes
                         where p.ProductTypeName == ProductTypeConstants.Rug
                         select p.ProductTypeId
                           ).FirstOrDefault();
            IEnumerable<ComboBoxList> ProdCategoryList = db.ProductDesigns.Where(m => m.ProductTypeId == rugid).OrderBy(m => m.ProductDesignName).Select(m => new ComboBoxList
            {
                Id = m.ProductDesignId,
                PropFirst = m.ProductDesignName
            });
            return ProdCategoryList;

        }

        public IEnumerable<ComboBoxList> GetColourWaysForStencilHelpList()
        {
            int stencilid = (from p in db.ProductTypes
                             where p.ProductTypeName == ProductTypeConstants.Rug
                             select p.ProductTypeId
                          ).FirstOrDefault();
            IEnumerable<ComboBoxList> ProdCategoryList = from p in db.ProductDesigns
                                                         join t in db.RugStencil on p.ProductDesignId equals t.ProductDesignId into table
                                                         from PD in table.DefaultIfEmpty()
                                                         where p.ProductTypeId == stencilid && PD.StencilId == null
                                                         orderby p.ProductDesignName
                                                         select new ComboBoxList
                                                         {
                                                             Id = p.ProductDesignId,
                                                             PropFirst = p.ProductDesignName
                                                         };


            return ProdCategoryList;
        }

        public IEnumerable<ComboBoxList> GetDivisionHelpList()
        {
            IEnumerable<ComboBoxList> Divisionlist = db.Divisions.OrderBy(m => m.DivisionName).Select(m => new
                 ComboBoxList
            {
                Id = m.DivisionId,
                PropFirst = m.DivisionName
            });
            return Divisionlist;


        }



        public IEnumerable<ComboBoxList> GetProcessHelpList()
        {
            IEnumerable<ComboBoxList> prodList = db.Process.Where(m => m.IsActive == true).OrderBy(m => m.ProcessName).Select(i => new ComboBoxList
            {
                Id = i.ProcessId,
                PropFirst = i.ProcessName,
            });

            return prodList;
        }

        //public IEnumerable<ComboBoxList> GetMachineHelpList()
        //{
        //    IEnumerable<ComboBoxList> prodList = db.Product.Where(m => m.IsActive == true && m.ProductGroup.ProductType.ProductNature.ProductNatureName == ProductNatureConstants.Machine)
        //        .OrderBy(m => m.ProductName).Select(i => new ComboBoxList
        //    {
        //        Id = i.ProductId,
        //        PropFirst = i.ProductName,
        //    });

        //    return prodList;
        //}

        public IEnumerable<ComboBoxList> GetMachineHelpList()
        {
            IEnumerable<ComboBoxList> prodList = db.ProductUid.Where(m => m.IsActive == true && m.Product.ProductGroup.ProductType.ProductNature.ProductNatureName == ProductNatureConstants.Machine)
                .OrderBy(m => m.ProductUidName).Select(i => new ComboBoxList
                {
                    Id = i.ProductUIDId,
                    PropFirst = i.ProductUidName,
                });

            return prodList;
        }

        public IEnumerable<ComboBoxList> GetProductCategoryHelpList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.ProductCategory.OrderBy(m => m.ProductCategoryName).Select(m => new ComboBoxList
            {
                Id = m.ProductCategoryId,
                PropFirst = m.ProductCategoryName
            });

            return ProdCategoryList;
        }

        public IEnumerable<ComboBoxList> GetDimension1HelpList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.Dimension1.OrderBy(m => m.Dimension1Name).Select(m => new ComboBoxList
            {
                Id = m.Dimension1Id,
                PropFirst = m.Dimension1Name
            });

            return ProdCategoryList;
        }

        public IEnumerable<ComboBoxList> GetDimension2HelpList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.Dimension2.OrderBy(m => m.Dimension2Name).Select(m => new ComboBoxList
            {
                Id = m.Dimension2Id,
                PropFirst = m.Dimension2Name
            });

            return ProdCategoryList;
        }

        public IEnumerable<ComboBoxList> GetDimension3HelpList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.Dimension3.OrderBy(m => m.Dimension3Name).Select(m => new ComboBoxList
            {
                Id = m.Dimension3Id,
                PropFirst = m.Dimension3Name
            });

            return ProdCategoryList;
        }

        public IEnumerable<ComboBoxList> GetDimension4HelpList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.Dimension4.OrderBy(m => m.Dimension4Name).Select(m => new ComboBoxList
            {
                Id = m.Dimension4Id,
                PropFirst = m.Dimension4Name
            });

            return ProdCategoryList;
        }
        public IEnumerable<ComboBoxList> GetRawProductCategoryHelpList()
        {
            int rawmatid = (from p in db.ProductNature
                            where p.ProductNatureName == ProductNatureConstants.Rawmaterial
                            select p.ProductNatureId).FirstOrDefault();

            IEnumerable<ComboBoxList> ProdCategoryList = db.ProductCategory.Where(m => m.ProductType.ProductNatureId == rawmatid).OrderBy(m => m.ProductCategoryName).Select(m => new ComboBoxList
            {
                Id = m.ProductCategoryId,
                PropFirst = m.ProductCategoryName
            });

            return ProdCategoryList;
        }
        public IEnumerable<ComboBoxList> GetOtherProductCategoryHelpList()
        {
            int rawmatid = (from p in db.ProductNature
                            where p.ProductNatureName == ProductNatureConstants.OtherMaterial
                            select p.ProductNatureId).FirstOrDefault();

            IEnumerable<ComboBoxList> ProdCategoryList = db.ProductCategory.Where(m => m.ProductType.ProductNatureId == rawmatid).OrderBy(m => m.ProductCategoryName).Select(m => new ComboBoxList
            {
                Id = m.ProductCategoryId,
                PropFirst = m.ProductCategoryName
            });

            return ProdCategoryList;
        }
        public IEnumerable<ComboBoxList> GetProductQualityHelpList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.ProductQuality.OrderBy(m => m.ProductQualityName).Select(m => new ComboBoxList
            {
                Id = m.ProductQualityId,
                PropFirst = m.ProductQualityName
            });
            return ProdCategoryList;
        }
        public IEnumerable<ComboBoxList> GetProductDesignHelpList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.ProductDesigns.OrderBy(m => m.ProductDesignName).Select(m => new ComboBoxList
            {
                Id = m.ProductDesignId,
                PropFirst = m.ProductDesignName
            });
            return ProdCategoryList;
        }
        public IEnumerable<ComboBoxList> GetProductInvoiceGroupHelpList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.ProductInvoiceGroup.OrderBy(m => m.ProductInvoiceGroupName).Select(m => new ComboBoxList
            {
                Id = m.ProductInvoiceGroupId,
                PropFirst = m.ProductInvoiceGroupName
            });
            return ProdCategoryList;
        }

        public IEnumerable<ComboBoxList> GetProductInvoiceGroupDivisionWiseHelpList()
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            IEnumerable<ComboBoxList> ProdCategoryList = db.ProductInvoiceGroup.Where(m => m.DivisionId == DivisionId && m.IsActive == true).OrderBy(m => m.ProductInvoiceGroupName).Select(m => new ComboBoxList
            {
                Id = m.ProductInvoiceGroupId,
                PropFirst = m.ProductInvoiceGroupName
            });
            return ProdCategoryList;
        }

        public IEnumerable<ComboBoxList> GetProductInvoiceGroupDivisionWiseExcludeSampleHelpList()
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            IEnumerable<ComboBoxList> ProdCategoryList = db.ProductInvoiceGroup.Where(m => m.DivisionId == DivisionId && m.IsSample == false && m.IsActive == true).OrderBy(m => m.ProductInvoiceGroupName).Select(m => new ComboBoxList
            {
                Id = m.ProductInvoiceGroupId,
                PropFirst = m.ProductInvoiceGroupName
            });
            return ProdCategoryList;
        }

        public IEnumerable<ComboBoxList> GetProductStyleHelpList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.ProductStyle.OrderBy(m => m.ProductStyleName).Select(m => new ComboBoxList
            {
                Id = m.ProductStyleId,
                PropFirst = m.ProductStyleName
            });
            return ProdCategoryList;
        }
        public IEnumerable<ComboBoxList> GetProductManufacturerHelpList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.Manufacturer.OrderBy(m => m.Person.Name).Select(m => new ComboBoxList
            {
                Id = m.PersonID,
                PropFirst = m.Person.Name
            });
            return ProdCategoryList;
        }
        public IEnumerable<ComboBoxList> GetProductDrawBackTarrifHelpList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.DrawBackTariffHead.OrderBy(m => m.DrawBackTariffHeadName).Select(m => new ComboBoxList
            {
                Id = m.DrawBackTariffHeadId,
                PropFirst = m.DrawBackTariffHeadName
            });
            return ProdCategoryList;
        }
        public IEnumerable<ComboBoxList> GetProductProcessSequenceHelpList()
        {
            IEnumerable<ComboBoxList> ProdCategoryList = db.ProcessSequenceHeader.OrderBy(m => m.ProcessSequenceHeaderName).Select(m => new ComboBoxList
            {
                Id = m.ProcessSequenceHeaderId,
                PropFirst = m.ProcessSequenceHeaderName
            });
            return ProdCategoryList;
        }
        public IEnumerable<ComboBoxList> GetSaleOrderHelpList()
        {
            IEnumerable<ComboBoxList> SaleOrderlist = db.SaleOrderHeader.OrderBy(m => m.DocNo).Select(m => new
                 ComboBoxList
            {
                Id = m.SaleOrderHeaderId,
                PropFirst = m.DocNo,
                PropSecond = m.BuyerOrderNo
            });
            return SaleOrderlist;
        }

        public IEnumerable<ComboBoxList> GetSaleOrderDivisionWistHelpList()
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            IEnumerable<ComboBoxList> SaleOrderlist = db.SaleOrderHeader.Where(m => m.SiteId == SiteId && m.DivisionId == DivisionId).OrderBy(m => m.DocNo).Select(m => new
                ComboBoxList
            {
                Id = m.SaleOrderHeaderId,
                PropFirst = m.DocNo,
                PropSecond = m.BuyerOrderNo
            });
            return SaleOrderlist;
        }

        public IEnumerable<ComboBoxList> GetProdOrderHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.ProdOrderHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1 && DocumentTypeTab.DocumentTypeName == TransactionDocTypeConstants && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.ProdOrderHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetProdOrderHelpList(int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.ProdOrderHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1 && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.ProdOrderHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetProdOrderCancelHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.ProdOrderCancelHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1 && DocumentTypeTab.DocumentTypeName == TransactionDocTypeConstants && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.ProdOrderCancelHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetMaterialPlanHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.MaterialPlanHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1 && DocumentTypeTab.DocumentTypeName == TransactionDocTypeConstants && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.MaterialPlanHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo
                                                  });



            return Helplist;
        }


        public IEnumerable<ComboBoxList> GetPurchaseIndentHelpList()
        {
            //IEnumerable<ComboBoxList> Helplist = db.PurchaseIndentHeader.OrderBy(m => m.DocNo).Select(m => new
            //    ComboBoxList
            //{
            //    Id = m.PurchaseIndentHeaderId,
            //    PropFirst = m.DocNo,
            //});
            //return SaleOrderlist;

            IEnumerable<ComboBoxList> Helplist = (from H in db.PurchaseIndentHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.PurchaseIndentHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo
                                                  });



            return Helplist;

        }

        public IEnumerable<ComboBoxList> GetPurchaseIndentCancelHelpList()
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.PurchaseIndentCancelHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.PurchaseIndentCancelHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo
                                                  });



            return Helplist;

        }

        public IEnumerable<ComboBoxList> GetPurchaseOrderHelpList()
        {
            //IEnumerable<ComboBoxList> PurchaseOrderlist = db.PurchaseOrderHeader.OrderBy(m => m.DocNo).Select(m => new
            //     ComboBoxList
            //{
            //    Id = m.PurchaseOrderHeaderId,
            //    PropFirst = m.DocNo
            //});
            //return PurchaseOrderlist;
            IEnumerable<ComboBoxList> Helplist = (from H in db.PurchaseOrderHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.PurchaseOrderHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetProdOrderHelpList()
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.ProdOrderHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.ProdOrderHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetBalanceProdOrderHelpList()
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.ViewProdOrderBalance
                                                  join t in db.ProdOrderHeader on H.ProdOrderHeaderId equals t.ProdOrderHeaderId
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1
                                                  group new { H, t } by new { H.ProdOrderHeaderId } into g
                                                  orderby g.Max(m => m.t.DocDate), g.Max(m => m.t.DocNo)
                                                  select new ComboBoxList
                                                  {
                                                      Id = g.Key.ProdOrderHeaderId,
                                                      PropFirst = g.Max(m => m.t.DocType.DocumentTypeShortName) + "-" + g.Max(m => m.t.DocNo)
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetPurchaseOrderCancelHelpList()
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.PurchaseOrderCancelHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.PurchaseOrderCancelHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetPurchaseGoodsReceiptHelpList()
        {
            //IEnumerable<ComboBoxList> SaleOrderlist = db.PurchaseGoodsReceiptHeader.OrderBy(m => m.DocNo).Select(m => new
            //    ComboBoxList
            //{
            //    Id = m.PurchaseGoodsReceiptHeaderId,
            //    PropFirst = m.DocNo,
            //});
            //return SaleOrderlist;

            IEnumerable<ComboBoxList> Helplist = (from H in db.PurchaseGoodsReceiptHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.PurchaseGoodsReceiptHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetPurchaseGoodsReturnHelpList()
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.PurchaseGoodsReturnHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.PurchaseGoodsReturnHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetPurchaseInvoiceHelpList()
        {
            //IEnumerable<ComboBoxList> PurchaseOrderlist = db.PurchaseInvoiceHeader.OrderBy(m => m.DocNo).Select(m => new
            //     ComboBoxList
            //{
            //    Id = m.PurchaseInvoiceHeaderId,
            //    PropFirst = m.DocNo
            //});
            //return PurchaseOrderlist;

            IEnumerable<ComboBoxList> Helplist = (from H in db.PurchaseInvoiceHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.PurchaseInvoiceHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetPurchaseInvoiceReturnHelpList()
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.PurchaseInvoiceReturnHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.PurchaseInvoiceReturnHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo
                                                  });



            return Helplist;
        }









        public IEnumerable<ComboBoxList> GetJobOrderHelpList(int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.JobOrderHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  join P in db.Persons on H.JobWorkerId equals P.PersonID into PersonTable
                                                  from PersonTab in PersonTable.DefaultIfEmpty()
                                                  where 1 == 1 && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.JobOrderHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo,
                                                      PropSecond = H.DocDate.ToString(),
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetJobReceiveHelpList(int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.JobReceiveHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  join P in db.Persons on H.JobWorkerId equals P.PersonID into PersonTable
                                                  from PersonTab in PersonTable.DefaultIfEmpty()
                                                  where 1 == 1 && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.JobReceiveHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo,
                                                      PropSecond = H.DocDate.ToString(),
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetJobInvoiceHelpList(int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.JobInvoiceHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1 && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.JobInvoiceHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo,
                                                      PropSecond = H.DocDate.ToString(),
                                                  });



            return Helplist;
        }


        public IEnumerable<ComboBoxList> GetCostCenterHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.CostCenter
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1 && DocumentTypeTab.DocumentTypeName == TransactionDocTypeConstants && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.CostCenterName
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.CostCenterId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.CostCenterName
                                                  });



            return Helplist;
        }

        public IQueryable<ComboBoxResult> GetCostCenterHelpListWithDocTypes(string term, int SiteId, int DivisionId)
        {
            IQueryable<ComboBoxResult> Helplist = (from H in db.CostCenter
                                                   join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                   from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                   where H.SiteId == SiteId && H.DivisionId == DivisionId && (string.IsNullOrEmpty(term) ? 1 == 1 : H.CostCenterName.ToLower().Contains(term.ToLower()))
                                                   orderby H.CostCenterName
                                                   select new ComboBoxResult
                                                 {
                                                     id = H.CostCenterId.ToString(),
                                                     text = H.CostCenterName + "-" + DocumentTypeTab.DocumentTypeShortName
                                                 });

            return Helplist;
        }
        public IQueryable<ComboBoxResult> GetChargeTypeList(string term)
        {
            IQueryable<ComboBoxResult> Helplist = (from H in db.ChargeType
                                                   where (string.IsNullOrEmpty(term) ? 1 == 1 : H.ChargeTypeName.ToLower().Contains(term.ToLower()))
                                                   orderby H.ChargeTypeName
                                                   select new ComboBoxResult
                                                   {
                                                       id = H.ChargeTypeId.ToString(),
                                                       text = H.ChargeTypeName
                                                   });

            return Helplist;
        }

        public IQueryable<ComboBoxResult> GetChargeList(string term)
        {
            IQueryable<ComboBoxResult> Helplist = (from H in db.Charge
                                                   where (string.IsNullOrEmpty(term) ? 1 == 1 : H.ChargeName.ToLower().Contains(term.ToLower()))
                                                   orderby H.ChargeName
                                                   select new ComboBoxResult
                                                   {
                                                       id = H.ChargeId.ToString(),
                                                       text = H.ChargeName
                                                   });

            return Helplist;
        }

        public IQueryable<ComboBoxResult> GetChargeListWithCode(string term)
        {
            IQueryable<ComboBoxResult> Helplist = (from H in db.Charge
                                                   where (string.IsNullOrEmpty(term) ? 1 == 1 : H.ChargeName.ToLower().Contains(term.ToLower()))
                                                   orderby H.ChargeName
                                                   select new ComboBoxResult
                                                   {
                                                       id = H.ChargeCode,
                                                       text = H.ChargeName
                                                   });

            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetJobOrderHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.JobOrderHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  join P in db.Persons on H.JobWorkerId equals P.PersonID into PersonTable
                                                  from PersonTab in PersonTable.DefaultIfEmpty()
                                                  where 1 == 1 && DocumentTypeTab.DocumentTypeName == TransactionDocTypeConstants && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.JobOrderHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo,
                                                      PropSecond = H.DocDate.ToString(),
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetJobOrderAmendmentHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.JobOrderAmendmentHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  join P in db.Persons on H.JobWorkerId equals P.PersonID into PersonTable
                                                  from PersonTab in PersonTable.DefaultIfEmpty()
                                                  where 1 == 1 && DocumentTypeTab.DocumentTypeName == TransactionDocTypeConstants && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.JobOrderAmendmentHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo,
                                                      PropSecond = H.DocDate.ToString(),
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetJobReceiveHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.JobReceiveHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  join P in db.Persons on H.JobWorkerId equals P.PersonID into PersonTable
                                                  from PersonTab in PersonTable.DefaultIfEmpty()
                                                  where 1 == 1 && DocumentTypeTab.DocumentTypeName == TransactionDocTypeConstants && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.JobReceiveHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo,
                                                      PropSecond = H.DocDate.ToString(),
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetJobInvoiceHelpList(string TransactionDocTypeConstants, int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.JobInvoiceHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  where 1 == 1 && DocumentTypeTab.DocumentTypeName == TransactionDocTypeConstants && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.JobInvoiceHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo,
                                                      PropSecond = H.DocDate.ToString(),
                                                  });



            return Helplist;
        }




        public IEnumerable<ComboBoxList> GetStoreIssueReceiveHelpList(string TransactionDocCategoryConstants, int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.StockHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  join DC in db.DocumentCategory on DocumentTypeTab.DocumentCategoryId equals DC.DocumentCategoryId into DocumentCategoryTable
                                                  from DocumentCategoryTab in DocumentCategoryTable.DefaultIfEmpty()
                                                  where 1 == 1 && DocumentCategoryTab.DocumentCategoryName == TransactionDocCategoryConstants && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.StockHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo,
                                                      PropSecond = H.DocDate.ToString(),
                                                  });



            return Helplist;
        }

        public IEnumerable<ComboBoxList> GetLedgerHeaderHelpList(string TransactionDocCategoryConstants, int SiteId, int DivisionId)
        {
            IEnumerable<ComboBoxList> Helplist = (from H in db.LedgerHeader
                                                  join DY in db.DocumentType on H.DocTypeId equals DY.DocumentTypeId into DocumentTypeTable
                                                  from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                  //where 1 == 1 && DocumentTypeTab.DocumentTypeName == TransactionDocTypeConstants && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  where 1 == 1 && H.SiteId == SiteId && H.DivisionId == DivisionId
                                                  orderby H.DocDate, H.DocNo
                                                  select new ComboBoxList
                                                  {
                                                      Id = H.LedgerHeaderId,
                                                      PropFirst = DocumentTypeTab.DocumentTypeShortName + "-" + H.DocNo,
                                                      PropSecond = H.DocDate.ToString(),
                                                  });



            return Helplist;
        }









        public IEnumerable<ComboBoxList> GetCurrencyHelpList()
        {
            IEnumerable<ComboBoxList> Currencylist = db.Currency.OrderBy(m => m.Name).Select(m => new
                 ComboBoxList
            {
                Id = m.ID,
                PropFirst = m.Name
            });
            return Currencylist;
        }

        public IEnumerable<ComboBoxList> GetBuyerHelpList()
        {
            //IEnumerable<ComboBoxList> buyerlist = db.Buyers.OrderBy(m => m.Name).Select(m => new
            //     ComboBoxList
            //     {
            //         Id = m.PersonID,
            //         PropFirst = m.Name
            //     });


            //IEnumerable<ComboBoxList> buyerlist = (from b in db.Buyer
            //                                       join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
            //                                       from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
            //                                       join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
            //                                       from PersonTab in PersonTable.DefaultIfEmpty()
            //                                       orderby PersonTab.Name
            //                                       select new ComboBoxList
            //                                       {
            //                                           Id = b.PersonID,
            //                                           PropFirst = PersonTab.Name + "|" + PersonTab.Code
            //                                       });

            int ProcessId = (from P in db.Process where P.ProcessName == ProcessConstants.Sales select P).FirstOrDefault().ProcessId;
            int BuyerDocTypeId = (from D in db.DocumentType where D.DocumentTypeName == MasterDocTypeConstants.Buyer select D).FirstOrDefault().DocumentTypeId;

            IEnumerable<ComboBoxList> buyerlist = (from p in db.Persons
                                                   join pp in db.PersonProcess on p.PersonID equals pp.PersonId into PersonProcessTable
                                                   from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                                                   join pr in db.PersonRole on p.PersonID equals pr.PersonId into PersonRoleTable
                                                   from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                                                   where PersonProcessTab.ProcessId == ProcessId
                                                   && PersonRoleTab.RoleDocTypeId == BuyerDocTypeId
                                                   && (p.IsActive == null ? 1 == 1 : p.IsActive == true)
                                                   orderby p.Name
                                                   select new ComboBoxList
                                                   {
                                                       Id = p.PersonID,
                                                       PropFirst = p.Name + ", " + p.Suffix + " [" + p.Code + "]"
                                                   });


            return buyerlist;
        }

        public IEnumerable<ComboBoxList> GetShipMethodHelpList()
        {
            IEnumerable<ComboBoxList> ShipMethodlist = (from b in db.ShipMethod
                                                        orderby b.ShipMethodName
                                                        select new ComboBoxList
                                                        {
                                                            Id = b.ShipMethodId,
                                                            PropFirst = b.ShipMethodName
                                                        });

            return ShipMethodlist;
        }

        public IEnumerable<ComboBoxList> GetSupplierHelpList()
        {
            IEnumerable<ComboBoxList> buyerlist = (from b in db.Supplier
                                                   join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                                                   from PersonTab in PersonTable.DefaultIfEmpty()
                                                   orderby PersonTab.Name
                                                   select new ComboBoxList
                                                   {
                                                       Id = b.PersonID,
                                                       PropFirst = PersonTab.Name
                                                   });




            return buyerlist;
        }


        public IEnumerable<ComboBoxList> GetJobWorkerHelpList()
        {
            //IEnumerable<ComboBoxList> JobWorkerlist = (from b in db.JobWorker
            //                                           join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
            //                                           from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
            //                                           join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
            //                                           from PersonTab in PersonTable.DefaultIfEmpty()
            //                                           where (PersonTab.IsActive == null ? 1 == 1 : PersonTab.IsActive == true)
            //                                           orderby PersonTab.Name
            //                                           select new ComboBoxList
            //                                           {
            //                                               Id = b.PersonID,
            //                                               PropFirst = PersonTab.Name
            //                                           });

            int JobWorkerDocTypeId = (from D in db.DocumentType where D.DocumentTypeName == MasterDocTypeConstants.JobWorker select D).FirstOrDefault().DocumentTypeId;

            IEnumerable<ComboBoxList> JobWorkerlist = (from p in db.Persons
                                                   join pp in db.PersonProcess on p.PersonID equals pp.PersonId into PersonProcessTable
                                                   from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                                                   join pr in db.PersonRole on p.PersonID equals pr.PersonId into PersonRoleTable
                                                   from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                                                       where PersonRoleTab.RoleDocTypeId == JobWorkerDocTypeId
                                                   && (p.IsActive == null ? 1 == 1 : p.IsActive == true)
                                                   orderby p.Name
                                                   select new ComboBoxList
                                                   {
                                                       Id = p.PersonID,
                                                       PropFirst = p.Name + ", " + p.Suffix + " [" + p.Code + "]"
                                                   });

            return JobWorkerlist;
        }

        public IEnumerable<ComboBoxList> GetJobWorkerHelpList_WithProcess(int ProcessId)
        {
            IEnumerable<ComboBoxList> JobWorkerlist = (from b in db.JobWorker
                                                       join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                                                       from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                                                       join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                                                       from PersonTab in PersonTable.DefaultIfEmpty()
                                                       join pp in db.PersonProcess on b.PersonID equals pp.PersonId into PersonProcessTable
                                                       from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                                                       where PersonProcessTab.ProcessId == ProcessId
                                                       && (PersonTab.IsActive == null ? 1 == 1 : PersonTab.IsActive == true)
                                                       orderby PersonTab.Name
                                                       select new ComboBoxList
                                                       {
                                                           Id = b.PersonID,
                                                           PropFirst = PersonTab.Name
                                                       });




            return JobWorkerlist;
        }

        public IEnumerable<ComboBoxList> GetEmployeeHelpList()
        {
            IEnumerable<ComboBoxList> Employeelist = (from b in db.Employee
                                                      join p in db.Persons on b.PersonID equals p.PersonID
                                                      orderby p.Name
                                                      select new ComboBoxList
                                                      {
                                                          Id = b.PersonID,
                                                          PropFirst = p.Name
                                                      });




            return Employeelist;
        }

        public IEnumerable<ComboBoxList> GetProductNatureHelpList()
        {
            IEnumerable<ComboBoxList> ProductNaturelist = db.ProductNature.OrderBy(m => m.ProductNatureName).Select(m => new ComboBoxList
            {
                Id = m.ProductNatureId,
                PropFirst = m.ProductNatureName
            });
            return ProductNaturelist;
        }

        public IEnumerable<ComboBoxList> GetProductTypeHelpList()
        {
            IEnumerable<ComboBoxList> producttypelist = db.ProductTypes.OrderBy(m => m.ProductTypeName).Select(m => new ComboBoxList
            {
                Id = m.ProductTypeId,
                PropFirst = m.ProductTypeName
            });
            return producttypelist;
        }
        public IEnumerable<ComboBoxList> GetProductCollectionHelpList()
        {
            IEnumerable<ComboBoxList> productcollectionlist = db.ProductCollections.OrderBy(m => m.ProductCollectionName).Select(m => new ComboBoxList
            {
                Id = m.ProductCollectionId,
                PropFirst = m.ProductCollectionName
            });
            return productcollectionlist;
        }

        public IEnumerable<ComboBoxList> GetProductGroupHelpList(int? filter)
        {



            //var temp = from p in db.ProductGroups
            //           join t in db.ProductTypes on p.ProductTypeId equals t.ProductTypeId into table1
            //           from tab1 in table1.DefaultIfEmpty()
            //           where ((filter == null || filter == 0) ? 1 == 1 : tab1.ProductTypeId == filter)
            //           orderby p.ProductGroupName, tab1.ProductTypeName
            //           select new ComboBoxList
            //           {
            //               Id = p.ProductGroupId,
            //               PropFirst = p.ProductGroupName,
            //               PropSecond = tab1.ProductTypeName
            //           };

            var temp = from p in db.ProductGroups
                       join t in db.ProductTypes on p.ProductTypeId equals t.ProductTypeId into table1
                       from tab1 in table1.DefaultIfEmpty()
                       where ((filter == null || filter == 0) ? 1 == 1 : tab1.ProductTypeId == filter)
                       orderby p.ProductGroupName, tab1.ProductTypeName
                       select new ComboBoxList
                       {
                           Id = p.ProductGroupId,
                           PropFirst = p.ProductGroupName
                       };

            return temp;
        }


        public IEnumerable<ComboBoxList> GetProductGroupForRugHelpList()
        {
            var temp = from p in db.ProductGroups
                       join t in db.ProductTypes on p.ProductTypeId equals t.ProductTypeId into table1
                       from tab1 in table1.DefaultIfEmpty()
                       where tab1.ProductTypeName == ProductTypeConstants.Rug
                       orderby p.ProductGroupName, tab1.ProductTypeName
                       select new ComboBoxList
                       {
                           Id = p.ProductGroupId,
                           PropFirst = p.ProductGroupName
                       };

            return temp;
        }

        public IEnumerable<ComboBoxList> GetProductGroupHelpListwithTypeFilter(int filter)
        {

            var temp = from p in db.ProductGroups
                       where p.ProductTypeId == filter
                       orderby p.ProductGroupName
                       select new ComboBoxList
                       {
                           Id = p.ProductGroupId,
                           PropFirst = p.ProductGroupName,
                       };

            return temp;
        }
        public IEnumerable<ComboBoxList> GetRawMaterialProductGroupHelpList()
        {
            int rawmatid = (from p in db.ProductNature
                            where p.ProductNatureName == ProductNatureConstants.Rawmaterial
                            select p.ProductNatureId).FirstOrDefault();
            var temp = from p in db.ProductGroups
                       join t in db.ProductTypes on p.ProductTypeId equals t.ProductTypeId into table1
                       from tab1 in table1.DefaultIfEmpty()
                       where tab1.ProductNatureId == rawmatid
                       orderby p.ProductGroupName, tab1.ProductTypeName
                       select new ComboBoxList
                       {
                           Id = p.ProductGroupId,
                           PropFirst = p.ProductGroupName,
                       };

            return temp;
        }

        public IEnumerable<ComboBoxList> GetOtherMaterialProductGroupHelpList()
        {
            var rawmatid = (from p in db.ProductNature
                            where p.ProductNatureName == ProductNatureConstants.OtherMaterial
                            select p.ProductNatureId).FirstOrDefault();
            var temp = from p in db.ProductGroups
                       join t in db.ProductTypes on p.ProductTypeId equals t.ProductTypeId into table1
                       from tab1 in table1.DefaultIfEmpty()
                       where tab1.ProductNatureId == rawmatid
                       orderby p.ProductGroupName, tab1.ProductTypeName
                       select new ComboBoxList
                       {
                           Id = p.ProductGroupId,
                           PropFirst = p.ProductGroupName,
                       };

            return temp;
        }

        public IEnumerable<ComboBoxList> GetProductShapeHelpList()
        {
            IEnumerable<ComboBoxList> productShapelist = db.ProductShape.OrderBy(m => m.ProductShapeName).Select(m => new ComboBoxList
            {
                Id = m.ProductShapeId,
                PropFirst = m.ProductShapeName
            });
            return productShapelist;
        }

        public IEnumerable<ComboBoxList> GetProductHelpList()
        {
            IEnumerable<ComboBoxList> prodList = db.Product.Where(m => m.IsActive == true).OrderBy(m => m.ProductName).ThenBy(m => m.ProductCode).Select(i => new ComboBoxList
            {
                Id = i.ProductId,
                PropFirst = i.ProductName,
                PropSecond = i.ProductCode,
            });

            return prodList;

        }
        public IEnumerable<ComboBoxList> GetProductUidHelpList()
        {
            IEnumerable<ComboBoxList> prodList = db.ProductUid.Where(m => m.IsActive == true).OrderBy(m => m.ProductUidName).Select(i => new ComboBoxList
            {
                Id = i.ProductUIDId,
                PropFirst = i.ProductUidName,
            });

            return prodList;
        }

        public IEnumerable<ComboBoxList> GetFinishedProductDivisionWiseHelpList()
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = from p in db.FinishedProduct
                       orderby p.ProductName
                       where p.DivisionId == DivisionId
                       select new ComboBoxList
                       {
                           Id = p.ProductId,
                           PropFirst = p.ProductName,
                       };

            return temp;
        }

        public IEnumerable<ComboBoxList> GetProductTagHelpList()
        {
            IEnumerable<ComboBoxList> prodList = db.Product.Where(m => m.IsActive == true).OrderBy(m => m.ProductName).ThenBy(m => m.ProductCode).Select(i => new ComboBoxList
            {
                Id = i.ProductId,
                PropFirst = i.ProductName,
            });

            return prodList;

        }




        public IEnumerable<ComboBoxList> GetProductCustomGroupHelpList()
        {
            IEnumerable<ComboBoxList> productCustomGrouplist = db.ProductCustomGroupHeader.OrderBy(m => m.ProductCustomGroupName).Select(m => new ComboBoxList
            {
                Id = m.ProductCustomGroupId,
                PropFirst = m.ProductCustomGroupName
            });
            return productCustomGrouplist;
        }
        public IEnumerable<ComboBoxList> GetTagHelpList()
        {
            IEnumerable<ComboBoxList> Taglist = db.Tag.Select(m => new ComboBoxList
            {
                Id = m.TagId,
                PropFirst = m.TagName
            });
            return Taglist;
        }


        public IEnumerable<ComboBoxList> GetSaleOrderAmendmentHelpList()
        {
            IEnumerable<ComboBoxList> saleorderAmendmentList = db.SaleOrderAmendmentHeader.OrderBy(m => m.DocNo).Select(m => new ComboBoxList
            {
                Id = m.SaleOrderAmendmentHeaderId,
                PropFirst = m.DocNo,
                PropSecond = m.DocNo
            });
            return saleorderAmendmentList;
        }
        public IEnumerable<ComboBoxList> GetSaleOrderCancelHelpList()
        {
            IEnumerable<ComboBoxList> saleorderCancelList = db.SaleOrderCancelHeader.OrderBy(m => m.DocNo).Select(m => new ComboBoxList
            {
                Id = m.SaleOrderCancelHeaderId,
                PropFirst = m.DocNo,
                PropSecond = m.DocNo
            });
            return saleorderCancelList;
        }

        public IEnumerable<ComboBoxList> GetSaleInvoiceHelpList()
        {

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //IEnumerable<ComboBoxList> saleInvoiceList = db.SaleInvoiceHeader.OrderBy(m => m.DocNo).Where(m => m.DivisionId == DivisionId && m.SiteId == SiteId).Select(m => new ComboBoxList
            IEnumerable<ComboBoxList> saleInvoiceList = db.SaleInvoiceHeader.OrderBy(m => m.DocNo).Select(m => new ComboBoxList
            {
                Id = m.SaleInvoiceHeaderId,
                PropFirst = m.DocNo,
                PropSecond = m.BillToBuyer.Person.Code  
            });

            return saleInvoiceList;
        }

        public IEnumerable<ComboBoxList> SqlProcsHelpList()
        {
            string connectionString = "Data Source=Admin-pc;Initial Catalog=SuryaIndia;Integrated Security=False;User Id=sa; pwd=";

            // your "normal" sql here
            string commandText = "select SPECIFIC_NAME as Code, SPECIFIC_NAME as Name from INFORMATION_SCHEMA.ROUTINES";

            List<string> result = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(commandText, connection);

                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
            }
            List<ComboBoxList> lis = new List<ComboBoxList>();

            foreach (var item in result)
            {
                lis.Add(new ComboBoxList()
                {
                    PropFirst = item,
                });
            }


            return lis;
        }


        public IEnumerable<ComboBoxList> GetSaleOrderDocumentTypeHelpList()
        {
            IEnumerable<ComboBoxList> documentlist = db.DocumentType.Where(m => m.DocumentCategory.DocumentCategoryName == TransactionDocCategoryConstants.SaleOrder).OrderBy(m => m.DocumentTypeName).Select(m => new ComboBoxList
            {
                Id = m.DocumentTypeId,
                PropFirst = m.DocumentTypeName
            });
            return documentlist;
        }

        public IEnumerable<ComboBoxList> GetSaleOrderPlanDocumentTypeHelpList()
        {
            IEnumerable<ComboBoxList> documentlist = db.DocumentType.Where(m => m.DocumentCategory.DocumentCategoryName == TransactionDocCategoryConstants.SaleOrderPlan).OrderBy(m => m.DocumentTypeName).Select(m => new ComboBoxList
            {
                Id = m.DocumentTypeId,
                PropFirst = m.DocumentTypeName
            });
            return documentlist;
        }

        public IEnumerable<ComboBoxList> GetDyedMaterialPlanForWeavingDocumentTypeHelpList()
        {
            IEnumerable<ComboBoxList> documentlist = db.DocumentType.Where(m => m.DocumentCategory.DocumentCategoryName == TransactionDocCategoryConstants.MaterialPlan).OrderBy(m => m.DocumentTypeName).Select(m => new ComboBoxList
            {
                Id = m.DocumentTypeId,
                PropFirst = m.DocumentTypeName
            });
            return documentlist;
        }

        public IEnumerable<ComboBoxList> GetSaleOrderAmendmentDocumentTypeHelpList()
        {
            IEnumerable<ComboBoxList> documentlist = db.DocumentType.Where(m => m.DocumentCategory.DocumentCategoryName == TransactionDocCategoryConstants.SaleOrderAmendment).OrderBy(m => m.DocumentTypeName).Select(m => new ComboBoxList
            {
                Id = m.DocumentTypeId,
                PropFirst = m.DocumentTypeName
            });
            return documentlist;
        }
        public IEnumerable<ComboBoxList> GetSaleOrderCancelDocumentTypeHelpList()
        {
            IEnumerable<ComboBoxList> documentlist = db.DocumentType.Where(m => m.DocumentCategory.DocumentCategoryName == TransactionDocCategoryConstants.SaleOrderCancel).OrderBy(m => m.DocumentTypeName).Select(m => new ComboBoxList
            {
                Id = m.DocumentTypeId,
                PropFirst = m.DocumentTypeName
            });
            return documentlist;
        }

        public IEnumerable<ComboBoxList> GetSaleInvoiceDocumentTypeHelpList()
        {
            IEnumerable<ComboBoxList> documentlist = db.DocumentType.Where(m => m.DocumentCategory.DocumentCategoryName == TransactionDocCategoryConstants.SaleInvoice).OrderBy(m => m.DocumentTypeName).Select(m => new ComboBoxList
            {
                Id = m.DocumentTypeId,
                PropFirst = m.DocumentTypeName
            });
            return documentlist;
        }

        public IEnumerable<ComboBoxList> GetPurchaseOrderDocumentTypeHelpList()
        {
            IEnumerable<ComboBoxList> documentlist = db.DocumentType.Where(m => m.DocumentCategory.DocumentCategoryName == TransactionDocCategoryConstants.PurchaseOrder).OrderBy(m => m.DocumentTypeName).Select(m => new ComboBoxList
            {
                Id = m.DocumentTypeId,
                PropFirst = m.DocumentTypeName
            });
            return documentlist;
        }

        public IEnumerable<ComboBoxList> GetPurchaseIndentDocumentTypeHelpList()
        {
            IEnumerable<ComboBoxList> documentlist = db.DocumentType.Where(m => m.DocumentCategory.DocumentCategoryName == TransactionDocCategoryConstants.PurchaseIndent).OrderBy(m => m.DocumentTypeName).Select(m => new ComboBoxList
            {
                Id = m.DocumentTypeId,
                PropFirst = m.DocumentTypeName
            });
            return documentlist;
        }

        public IEnumerable<ComboBoxList> GetDocumentTypeHelpList(string TransactionDocCategoryConstants)
        {
            IEnumerable<ComboBoxList> documentlist = db.DocumentType.Where(m => m.DocumentCategory.DocumentCategoryName == TransactionDocCategoryConstants).OrderBy(m => m.DocumentTypeName).Select(m => new ComboBoxList
            {
                Id = m.DocumentTypeId,
                PropFirst = m.DocumentTypeName
            });
            return documentlist;
        }


        //public IEnumerable<ComboBoxList> GetCityHelpList()
        //{
        //    IEnumerable<ComboBoxList> prodList = db.City.Where(m => m.IsActive == true).OrderBy(m => m.CityName).Select(i => new ComboBoxList
        //    {
        //        Id = i.CityId,
        //        PropFirst = i.CityName,
        //    });

        //    return prodList;
        //}

        public IQueryable<ComboBoxResult> GetCityHelpList(string term)
        {
            var list = (from P in db.City
                        where P.IsActive == true 
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : P.CityName.ToLower().Contains(term.ToLower()))
                        orderby P.CityName
                        select new ComboBoxResult
                        {
                            id = P.CityId.ToString(),
                            text = P.CityName,
                        }
              );

            return list;
        }

        public IEnumerable<ComboBoxList> GetStateHelpList()
        {
            IEnumerable<ComboBoxList> prodList = db.State.Where(m => m.IsActive == true).OrderBy(m => m.StateName).Select(i => new ComboBoxList
            {
                Id = i.StateId,
                PropFirst = i.StateName,
            });

            return prodList;
        }

        public IEnumerable<ComboBoxList> GetCountryHelpList()
        {
            IEnumerable<ComboBoxList> prodList = db.Country.Where(m => m.IsActive == true).OrderBy(m => m.CountryName).Select(i => new ComboBoxList
                {
                    Id = i.CountryId,
                    PropFirst = i.CountryName,
                });

            return prodList;
        }



        //public IEnumerable<ComboBoxList> GetPersonHelpList()
        //{
        //    IEnumerable<ComboBoxList> prodList = db.Persons.Where(m => m.IsActive == true).OrderBy(m => m.Name).Select(i => new ComboBoxList
        //    {
        //        Id = i.PersonID,
        //        PropFirst = i.Name + "|" + i.Code
        //    });

        //    return prodList;
        //}


        public IQueryable<ComboBoxResult> GetPersonHelpList(string term)
        {
            var list = (from P in db.Persons
                        where P.IsActive == true
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : P.Name.ToLower().Contains(term.ToLower()))
                        orderby P.Name
                        select new ComboBoxResult
                        {
                            id = P.PersonID.ToString(),
                            text = P.Name + ", " + P.Suffix + " [" + P.Code + "]"
                        }
              );

            return list;
        }




        public IEnumerable<ComboBoxList> GetPersonBEHelpList()
        {
            IEnumerable<ComboBoxList> prodList = from p in db.BusinessEntity
                                                 join t in db.Persons on p.PersonID equals t.PersonID into table
                                                 from tab in table.DefaultIfEmpty()
                                                 orderby tab.Name
                                                 select new ComboBoxList
            {
                Id = tab.PersonID,
                PropFirst = tab.Name,
            };

            return prodList;
        }


        public IEnumerable<ComboBoxList> GetPersonRateGroupHelpList()
        {
            IEnumerable<ComboBoxList> prodList = db.PersonRateGroup.Where(m => m.IsActive == true).OrderBy(m => m.PersonRateGroupName).Select(i => new ComboBoxList
            {
                Id = i.PersonRateGroupId,
                PropFirst = i.PersonRateGroupName,
            });

            return prodList;
        }


        public IEnumerable<ComboBoxList> GetAccountGroupHelpList()
        {
            IEnumerable<ComboBoxList> prodList = db.LedgerAccountGroup.Where(m => m.IsActive == true).OrderBy(m => m.LedgerAccountGroupName).Select(i => new ComboBoxList
            {
                Id = i.LedgerAccountGroupId,
                PropFirst = i.LedgerAccountGroupName,
            });

            return prodList;
        }



        public IEnumerable<ComboBoxList> GetAccountHelpList()
        {
            //IEnumerable<ComboBoxList> prodList = db.LedgerAccount.Where(m => m.IsActive == true).OrderBy(m => m.LedgerAccountName).Select(i => new ComboBoxList
            //{
            //    Id = i.LedgerAccountId,
            //    PropFirst = i.LedgerAccountName,
            //    PropSecond=i.Person.Suffix,
            //});

            IEnumerable<ComboBoxList> prodlist2 = from p in db.LedgerAccount
                                                  join t in db.Persons on p.PersonId equals t.PersonID into table
                                                  from tab in table.DefaultIfEmpty()
                                                  where p.IsActive == true
                                                  orderby p.LedgerAccountName
                                                  select new ComboBoxList
                                                  {
                                                      Id = p.LedgerAccountId,
                                                      PropFirst = p.LedgerAccountName,
                                                      PropSecond = tab.Suffix,
                                                  };


            return prodlist2;
        }
        public IEnumerable<ComboBoxList> GetCostCenterHelpList()
        {
            IEnumerable<ComboBoxList> prodList = db.CostCenter.Where(m => m.IsActive == true).OrderBy(m => m.CostCenterName).Select(i => new ComboBoxList
            {
                Id = i.CostCenterId,
                PropFirst = i.CostCenterName,
            });

            return prodList;
        }
        public IEnumerable<ComboBoxList> GetCalculationProductHelpList()
        {
            IEnumerable<ComboBoxList> prodList = from p in db.CalculationProduct
                                                 join t in db.Charge on p.ChargeId equals t.ChargeId
                                                 orderby t.ChargeName
                                                 select new ComboBoxList
                                                 {
                                                     Id = p.CalculationProductId,
                                                     PropFirst = t.ChargeName
                                                 };
            return prodList;
        }

        public IEnumerable<ComboBoxList> GetCalculationFooterHelpList()
        {
            IEnumerable<ComboBoxList> prodList = from p in db.CalculationFooter
                                                 join t in db.Charge on p.ChargeId equals t.ChargeId
                                                 orderby t.ChargeName
                                                 select new ComboBoxList
                                                 {
                                                     Id = p.CalculationFooterLineId,
                                                     PropFirst = t.ChargeName
                                                 };
            return prodList;
        }

        public IEnumerable<ComboBoxList> GetPackingHelpList()
        {
            IEnumerable<ComboBoxList> Packinglist = db.PackingHeader.OrderBy(m => m.DocNo).Select(m => new
                 ComboBoxList
            {
                Id = m.PackingHeaderId,
                PropFirst = m.DocNo,
            });
            return Packinglist;
        }


        public IEnumerable<ComboBoxList> GetRawMaterialHelpList()
        {
            var temp = from p in db.Product
                       join pc in db.ProductGroups on p.ProductGroupId equals pc.ProductGroupId into ProductGroupTable
                       from ProductGroupTab in ProductGroupTable.DefaultIfEmpty()
                       join pt in db.ProductTypes on ProductGroupTab.ProductTypeId equals pt.ProductTypeId into ProductTypeTable
                       from ProductTypeTab in ProductTypeTable.DefaultIfEmpty()
                       join pn in db.ProductNature on ProductTypeTab.ProductNatureId equals pn.ProductNatureId into ProductNatureTable
                       from ProductNatureTab in ProductNatureTable.DefaultIfEmpty()
                       where ProductNatureTab.ProductNatureName == ProductNatureConstants.Rawmaterial
                       orderby p.ProductName
                       select new ComboBoxList
                       {
                           Id = p.ProductId,
                           PropFirst = p.ProductName,
                       };

            return temp;
        }




        public IEnumerable<ComboBoxList> GetFinishedMaterialHelpList()
        {


            var temp = from p in db.FinishedProduct
                       orderby p.ProductName
                       select new ComboBoxList
                       {
                           Id = p.ProductId,
                           PropFirst = p.ProductName,
                       };

            return temp;
        }

        public IEnumerable<ComboBoxList> GetFinishedMaterialDivisionWiseHelpList()
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = from p in db.FinishedProduct
                       where p.DivisionId == DivisionId && p.IsActive == true
                       orderby p.ProductName
                       select new ComboBoxList
                       {
                           Id = p.ProductId,
                           PropFirst = p.ProductName,
                       };

            return temp;
        }




        public IEnumerable<ComboBoxList> GetTransporterHelpList()
        {
            IEnumerable<ComboBoxList> Transporterlist = (from b in db.Transporter
                                                         join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                                                         from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                                                         join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
                                                         from PersonTab in PersonTable.DefaultIfEmpty()
                                                         orderby PersonTab.Name
                                                         select new ComboBoxList
                                                         {
                                                             Id = b.PersonID,
                                                             PropFirst = PersonTab.Name
                                                         });
            return Transporterlist;
        }


        public IEnumerable<ComboBoxList> GetRouteHelpList()
        {
            IEnumerable<ComboBoxList> Routelist = (from b in db.Route
                                                   orderby b.RouteName
                                                   select new ComboBoxList
                                                   {
                                                       Id = b.RouteId,
                                                       PropFirst = b.RouteName
                                                   });
            return Routelist;
        }

        public IEnumerable<ComboBoxList> GetBomHelpList()
        {
            var temp = from p in db.Product
                       join pg in db.ProductGroups on p.ProductGroupId equals pg.ProductGroupId into ProductGroupTable
                       from ProductGroupTab in ProductGroupTable.DefaultIfEmpty()
                       join pt in db.ProductTypes on ProductGroupTab.ProductTypeId equals pt.ProductTypeId into ProductTypeTable
                       from ProductTypeTab in ProductTypeTable.DefaultIfEmpty()
                       where ProductTypeTab.ProductTypeName == ProductTypeConstants.Bom
                       orderby p.ProductName
                       select new ComboBoxList
                       {
                           Id = p.ProductId,
                           PropFirst = p.ProductName,
                       };

            return temp;
        }

        public IEnumerable<ComboBoxList> GetBomMaterialHelpList()
        {
            var temp = from p in db.Product
                       join pc in db.ProductGroups on p.ProductGroupId equals pc.ProductGroupId into ProductGroupTable
                       from ProductGroupTab in ProductGroupTable.DefaultIfEmpty()
                       join pt in db.ProductTypes on ProductGroupTab.ProductTypeId equals pt.ProductTypeId into ProductTypeTable
                       from ProductTypeTab in ProductTypeTable.DefaultIfEmpty()
                       join pn in db.ProductNature on ProductTypeTab.ProductNatureId equals pn.ProductNatureId into ProductNatureTable
                       from ProductNatureTab in ProductNatureTable.DefaultIfEmpty()
                       where (ProductNatureTab.ProductNatureName == ProductNatureConstants.Rawmaterial
                       && ProductTypeTab.ProductTypeName != ProductTypeConstants.Trace && ProductTypeTab.ProductTypeName != ProductTypeConstants.Map
                       && ProductTypeTab.ProductTypeName != ProductTypeConstants.OtherMaterial && p.IsActive == true)
                       || (ProductNatureTab.ProductNatureName == ProductNatureConstants.Bom && p.IsActive == true)
                       orderby p.ProductName
                       select new ComboBoxList
                       {
                           Id = p.ProductId,
                           PropFirst = p.ProductName,
                       };

            return temp;
        }


        public IEnumerable<ComboBoxList> GetProductsHelpList(string term)
        {
            var list = (from p in db.Product
                        where p.ProductName.ToLower().Contains(term.ToLower())
                        orderby p.ProductName
                        select new ComboBoxList
                        {
                            Id = p.ProductId,
                            PropFirst = p.ProductName,
                        }

                          ).Take(20);
            return list;
        }


        public IEnumerable<ComboBoxList> GetSampleHelpList()
        {
            var temp = from p in db.FinishedProduct
                       orderby p.ProductName
                       where p.IsSample == true
                       select new ComboBoxList
                       {
                           Id = p.ProductId,
                           PropFirst = p.ProductName,
                       };

            return temp;
        }

        public IEnumerable<ComboBoxList> GetProductSizeHelpList()
        {
            //IEnumerable<ComboBoxList> productSizelist = db.Size.Select(m => new ComboBoxList
            //{
            //    Id = m.SizeId,
            //    PropFirst = m.SizeName
            //});
            //return productSizelist;



            var temp = from p in db.Size
                       join ps in db.ProductShape on p.ProductShapeId equals ps.ProductShapeId into ProductShapeTable
                       from ProductShapeTab in ProductShapeTable.DefaultIfEmpty()
                       where p.IsActive == true
                       orderby p.SizeName
                       select new ComboBoxList
                       {
                           Id = p.SizeId,
                           PropFirst = p.SizeName + ProductShapeTab.ProductShapeShortName ?? "",
                       };

            return temp;
        }

        public IEnumerable<ComboBoxList> GetSizeHelpList()
        {
            var temp = from p in db.Size
                       join ps in db.ProductShape on p.ProductShapeId equals ps.ProductShapeId into ProductShapeTable
                       from ProductShapeTab in ProductShapeTable.DefaultIfEmpty()
                       where p.IsActive == true
                       orderby p.SizeName
                       select new ComboBoxList
                       {
                           Id = p.SizeId,
                           PropFirst = p.SizeName + ProductShapeTab.ProductShapeShortName ?? "",
                       };

            return temp;
        }

        public IEnumerable<ComboBoxList> GetReasonHelpList()
        {
            var temp = from p in db.Reason
                       where p.IsActive == true
                       orderby p.ReasonName
                       select new ComboBoxList
                       {
                           Id = p.ReasonId,
                           PropFirst = p.ReasonName,
                       };

            return temp;
        }


        public IEnumerable<ComboBoxList> GetPersonCustomGroupHelpList()
        {
            IEnumerable<ComboBoxList> PersonCustomGrouplist = db.PersonCustomGroupHeader.OrderBy(m => m.PersonCustomGroupName).Select(m => new ComboBoxList
            {
                Id = m.PersonCustomGroupId,
                PropFirst = m.PersonCustomGroupName
            });
            return PersonCustomGrouplist;
        }

        //public IEnumerable<CustomComboBoxPagedResult> GetSelect2HelpList(string SqlProcGet, string searchTerm, int pageSize, int pageNum)
        //{

        //    //int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
        //    //int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

        //    //SqlParameter SqlParameterIds = new SqlParameter("@Ids", DBNull.Value);
        //    //SqlParameter SqlParameterSearchString = new SqlParameter("@SearchString", searchTerm);
        //    //SqlParameter SqlParameterPageSize = new SqlParameter("@PageSize", pageSize);
        //    //SqlParameter SqlParameterPageNo = new SqlParameter("@PageNo", pageNum - 1);
        //    //SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);
        //    //SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);

        //    //IEnumerable<CustomComboBoxResult> Select2List = db.Database.SqlQuery<CustomComboBoxResult>(SqlProcGet + " @Ids, @SearchString, @PageSize, @PageNo, @SiteId, @DivisionId", SqlParameterIds, SqlParameterSearchString, SqlParameterPageSize, SqlParameterPageNo,SqlParameterSiteId,SqlParameterDivisionId).ToList();

        //    //return Select2List;

        //    int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
        //    int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

        //    //SqlParameter SqlParameterIds = new SqlParameter("@Ids", DBNull.Value);
        //    //SqlParameter SqlParameterSearchString = new SqlParameter("@SearchString", searchTerm);
        //    //SqlParameter SqlParameterPageSize = new SqlParameter("@PageSize", pageSize);
        //    //SqlParameter SqlParameterPageNo = new SqlParameter("@PageNo", pageNum - 1);
        //    //SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);
        //    //SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);

        //    if (SqlProcGet.Contains(" ") == true)
        //        SqlProcGet = SqlProcGet + " ,";

        //    string mQry;

        //    mQry = " " + SqlProcGet + " @SearchString = '" + searchTerm + "', @PageSize =" + pageSize.ToString() + ", @PageNo =" + (pageNum - 1).ToString() + ", @SiteId= " + SiteId.ToString() + ", @DivisionId= " + DivisionId.ToString();
        //    IEnumerable<CustomComboBoxPagedResult> Select2List = db.Database.SqlQuery<ComboBoxResult>(mQry).ToList();

        //    return Select2List;

        //}

        public IQueryable<ComboBoxResult> GetJobWorkerHelpListWithProcessFilter(int Processid, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            string DivId = "|" + CurrentDivisionId.ToString() + "|";
            string SiteId = "|" + CurrentSiteId.ToString() + "|";

            var list = (from p in db.Persons
                        join bus in db.BusinessEntity on p.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join pp in db.PersonProcess on p.PersonID equals pp.PersonId into PersonProcessTable
                        from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                        join pr in db.PersonRole on p.PersonID equals pr.PersonId into PersonRoleTable
                        from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                        where PersonProcessTab.ProcessId == Processid
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (p.Name.ToLower().Contains(term.ToLower()) || p.Code.ToLower().Contains(term.ToLower())))
                        && BusinessEntityTab.DivisionIds.IndexOf(DivId) != -1
                        && (p.IsActive == null ? 1 == 1 : p.IsActive == true)
                        && BusinessEntityTab.SiteIds.IndexOf(SiteId) != -1
                        orderby p.Name
                        select new ComboBoxResult
                        {
                            id = p.PersonID.ToString(),
                            text = p.Name + "|" + p.Code,
                        }
              );

            return list;
        }

        public IQueryable<ComboBoxResult> GetPersonHelpListWithProcessFilter(int? Processid, string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            string DivId = "|" + CurrentDivisionId.ToString() + "|";
            string SiteId = "|" + CurrentSiteId.ToString() + "|";

            var list = (from b in db.Persons
                        join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join pp in db.PersonProcess on b.PersonID equals pp.PersonId into PersonProcessTable
                        from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                        where (Processid.HasValue ? PersonProcessTab.ProcessId == Processid : 1 == 1)
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : b.Name.ToLower().Contains(term.ToLower()) || b.Code.ToLower().Contains(term.ToLower()))
                        && (BusinessEntityTab.DivisionIds == null ? 1 == 1 : BusinessEntityTab.DivisionIds.IndexOf(DivId) != -1)
                        && (BusinessEntityTab.SiteIds == null ? 1 == 1 : BusinessEntityTab.SiteIds.IndexOf(SiteId) != -1)
                        && (b.IsActive == null ? 1 == 1 : b.IsActive == true)
                        group b by b.PersonID into G
                        orderby G.Max(m => m.Name)
                        select new ComboBoxResult
                        {
                            id = G.Key.ToString(),
                            text = G.Max(m => m.Name) + "|" + G.Max(m => m.Code),
                        }
              );

            return list;
        }

        public IQueryable<ComboBoxResult> GetPersonRateGroupHelpList(string term, int filter)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            return (from p in db.PersonRateGroup
                    where p.DivisionId == CurrentDivisionId && p.SiteId == CurrentSiteId && p.Processes.IndexOf(filter.ToString()) != -1
                    orderby p.PersonRateGroupName
                    select new ComboBoxResult
                    {
                        id = p.PersonRateGroupId.ToString(),
                        text = p.PersonRateGroupName,
                    });

        }

        public IQueryable<ComboBoxResult> GetProductRateGroupHelpList(string term, int filter)
        {
            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            return (from p in db.ProductRateGroup
                    where p.DivisionId == CurrentDivisionId && p.SiteId == CurrentSiteId && p.Processes.IndexOf(filter.ToString()) != -1
                    orderby p.ProductRateGroupName
                    select new ComboBoxResult
                    {
                        id = p.ProductRateGroupId.ToString(),
                        text = p.ProductRateGroupName,
                    });

        }

        public IQueryable<ComboBoxResult> GetEmployeeHelpListWithProcessFilter(int Processid, string term)
        {
            

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            string DivId = "|" + CurrentDivisionId.ToString() + "|";
            string SiteId = "|" + CurrentSiteId.ToString() + "|";


            var list = (from E in db.Employee
                        join b in db.Persons on E.PersonID equals b.PersonID
                        join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join pp in db.PersonProcess on b.PersonID equals pp.PersonId into PersonProcessTable
                        from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                        where PersonProcessTab.ProcessId == Processid 
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (b.Name.ToLower().Contains(term.ToLower()) || b.Code.ToLower().Contains(term.ToLower())))
                        && BusinessEntityTab.DivisionIds.IndexOf(DivId) != -1
                        && BusinessEntityTab.SiteIds.IndexOf(SiteId) != -1
                        orderby b.Name
                        select new ComboBoxResult
                        {
                            id = b.PersonID.ToString(),
                            //text = b.Name + " | " + b.Code
                            text = b.Name + ", " + b.Suffix + " [" + b.Code + "]"
                        }
  );


            //var list = (from b in db.Persons
            //            join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
            //            from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
            //            join p in db.PersonRole on b.PersonID equals p.PersonId into PersonRoleTable
            //            from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
            //            join DT in db.DocumentType on PersonRoleTab.RoleDocTypeId equals DT.DocumentTypeId into DocumentTypeTable
            //            from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
            //            join pp in db.PersonProcess on b.PersonID equals pp.PersonId into PersonProcessTable
            //            from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
            //            where PersonProcessTab.ProcessId == Processid //&& DocumentTypeTab.DocumentTypeName=="Employee"
            //            && (string.IsNullOrEmpty(term) ? 1 == 1 : (b.Name.ToLower().Contains(term.ToLower()) || b.Code.ToLower().Contains(term.ToLower())))
            //            && BusinessEntityTab.DivisionIds.IndexOf(DivId) != -1
            //            && BusinessEntityTab.SiteIds.IndexOf(SiteId) != -1
            //            orderby b.Name
            //            select new ComboBoxResult
            //            {
            //                id = b.PersonID.ToString(),
            //                text = b.Name + " | " + b.Code
            //            }
            //  );

            //var list = (from b in db.Employee
            //            join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
            //            from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
            //            join p in db.Persons on b.PersonID equals p.PersonID into PersonTable
            //            from PersonTab in PersonTable.DefaultIfEmpty()
            //            join pp in db.PersonProcess on b.PersonID equals pp.PersonId into PersonProcessTable
            //            from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
            //            where PersonProcessTab.ProcessId == Processid
            //            && (string.IsNullOrEmpty(term) ? 1 == 1 : (PersonTab.Name.ToLower().Contains(term.ToLower()) || PersonTab.Code.ToLower().Contains(term.ToLower())))
            //            && BusinessEntityTab.DivisionIds.IndexOf(DivId) != -1
            //            && BusinessEntityTab.SiteIds.IndexOf(SiteId) != -1
            //            orderby PersonTab.Name
            //            select new ComboBoxResult
            //            {
            //                id = b.PersonID.ToString(),
            //                text = PersonTab.Name + " | " + PersonTab.Code
            //            }
            //  );

            return list;
        }

        public IQueryable<ComboBoxResult> GetControllerActionList(string term)
        {

            var list = (from p in db.ControllerAction
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (p.ControllerName.ToLower().Contains(term.ToLower())))
                        orderby p.ControllerName
                        select new ComboBoxResult
                                {
                                    text = p.ControllerName + "|" + p.ActionName,
                                    id = p.ControllerActionId.ToString()
                                }
                          );

            return list;
        }
        public IQueryable<ComboBoxResult> GetDocumentCategoryList(string term)
        {
            var list = (from p in db.DocumentCategory
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (p.DocumentCategoryName.ToLower().Contains(term.ToLower())))
                        orderby p.DocumentCategoryName
                        select new ComboBoxResult
                        {
                            text = p.DocumentCategoryName,
                            id = p.DocumentCategoryId.ToString()
                        }
              );

            return list;

        }

        public IQueryable<ComboBoxResult> GetUsers(string term)
        {

            var list = (from p in db.Users
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (p.UserName.ToLower().Contains(term.ToLower())))
                        orderby p.UserName
                        select new ComboBoxResult
                        {
                            text = p.UserName,
                            id = p.UserName,
                        }
            );

            return list;

        }

        public IEnumerable<ComboBoxList> GetPersonForSaleHelpList(string term)
        {

            int CurrentSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int CurrentDivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int ProcessId = (from P in db.Process where P.ProcessName == ProcessConstants.Sales select P).FirstOrDefault().ProcessId;

            string DivId = "|" + CurrentDivisionId.ToString() + "|";
            string SiteId = "|" + CurrentSiteId.ToString() + "|";


            IEnumerable<ComboBoxList> list = (from b in db.Persons
                                              join bus in db.BusinessEntity on b.PersonID equals bus.PersonID into BusinessEntityTable
                                              from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                                              join pp in db.PersonProcess on b.PersonID equals pp.PersonId into PersonProcessTable
                                              from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                                              where (PersonProcessTab.ProcessId == ProcessId)
                                              && (string.IsNullOrEmpty(term) ? 1 == 1 : b.Name.ToLower().Contains(term.ToLower()) || b.Code.ToLower().Contains(term.ToLower()))
                                              && BusinessEntityTab.DivisionIds.IndexOf(DivId) != -1
                                              && BusinessEntityTab.SiteIds.IndexOf(SiteId) != -1
                                              && (b.IsActive == null ? 1 == 1 : b.IsActive == true)
                                              group b by b.PersonID into G
                                              orderby G.Max(m => m.Name)
                                              select new ComboBoxList
                                              {
                                                  Id = G.Key,
                                                  PropFirst = G.Max(m => m.Name) + "|" + G.Max(m => m.Code),
                                              }).ToList();



            return list;
        }

         public IQueryable<ComboBoxResult> GetReasonHelpListWithDocTypeFilter(int DocTypeId, string term)
        {
            var DocumentType = (from D in db.DocumentType where D.DocumentTypeId == DocTypeId select D).FirstOrDefault();

            var list = (from R in db.Reason
                        where R.DocumentCategoryId == DocumentType.DocumentCategoryId
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (R.ReasonName.ToLower().Contains(term.ToLower())))
                        orderby R.ReasonName
                        select new ComboBoxResult
                        {
                            id = R.ReasonId.ToString(),
                            text = R.ReasonName
                        }
              );

            return list;
        }


        public IQueryable<ComboBoxResult> GetEmployeeHelpListWithDepartMentFilter(int DepartMentId, string term)
        {
            var list = (from b in db.Employee
                        join p in db.Persons on b.PersonID equals p.PersonID
                        where b.DepartmentID == DepartMentId
                        orderby p.Name
                        select new ComboBoxResult
                        {
                            id = p.PersonID.ToString(),
                            text = p.Name + " | " + p.Code
                        }
                      );

            return list;
        }


        public IQueryable<ComboBoxResult> GetQAGroups(string term)
        {

            var list = (from p in db.QAGroup
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (p.QaGroupName.ToLower().Contains(term.ToLower())))
                        orderby p.QaGroupName
                        select new ComboBoxResult
                        {
                            text = p.QaGroupName,
                            id = p.QAGroupId.ToString(),
                        }
            );

            return list;

        }

        public IQueryable<ComboBoxResult> GetUnits(string term)
        {

            var list = (from p in db.Units
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (p.UnitName.ToLower().Contains(term.ToLower())))
                        orderby p.UnitName
                        select new ComboBoxResult
                        {
                            text = p.UnitName,
                            id = p.UnitId,
                        }
            );

            return list;

        }

        public IQueryable<ComboBoxResult> GetTdsGroups(string term)
        {

            var list = (from p in db.TdsGroup
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (p.TdsGroupName.ToLower().Contains(term.ToLower())))
                        orderby p.TdsGroupName
                        select new ComboBoxResult
                        {
                            text = p.TdsGroupName,
                            id = p.TdsGroupId.ToString(),
                        }
            );

            return list;

        }

        public IQueryable<ComboBoxResult> GetTdsCategory(string term)
        {

            var list = (from p in db.TdsCategory
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (p.TdsCategoryName.ToLower().Contains(term.ToLower())))
                        orderby p.TdsCategoryName
                        select new ComboBoxResult
                        {
                            text = p.TdsCategoryName,
                            id = p.TdsCategoryId.ToString(),
                        }
            );

            return list;

        }


        public IQueryable<ComboBoxResult> GetSalesTaxGroupParty(string term)
        {

            var list = (from p in db.SalesTaxGroupParty
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (p.SalesTaxGroupPartyName.ToLower().Contains(term.ToLower())))
                        orderby p.SalesTaxGroupPartyName
                        select new ComboBoxResult
                        {
                            text = p.SalesTaxGroupPartyName,
                            id = p.SalesTaxGroupPartyId.ToString(),
                        }
            );

            return list;

        }

        public IQueryable<ComboBoxResult> GetPersonRoles(string term)
        {
            var list = (from D in db.DocumentType
                        join Dc in db.DocumentCategory on D.DocumentCategoryId equals Dc.DocumentCategoryId into DocumentCategoryTable from DocumentCategoryTab in DocumentCategoryTable.DefaultIfEmpty()
                        where DocumentCategoryTab.DocumentCategoryName == MasterDocCategoryConstants.Person
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (D.DocumentTypeName.ToLower().Contains(term.ToLower())))
                        orderby D.DocumentTypeName
                        select new ComboBoxResult
                        {
                            id = D.DocumentTypeId.ToString(),
                            text = D.DocumentTypeName
                        }
              );

            return list;
        }



        public IQueryable<ComboBoxResult> GetDeliveryTerms(string term)
        {
            var list = (from D in db.DeliveryTerms
                        where D.IsActive == true && (string.IsNullOrEmpty(term) ? 1 == 1 : (D.DeliveryTermsName.ToLower().Contains(term.ToLower())))
                        orderby D.DeliveryTermsName
                        select new ComboBoxResult
                        {
                            id = D.DeliveryTermsId.ToString(),
                            text = D.DeliveryTermsName
                        }
              );

            return list;
        }

        public IQueryable<ComboBoxResult> GetAddresses(string term)
        {
            var list = (from D in db.PersonAddress
                        join P in db.Persons on D.PersonId equals P.PersonID into PersonTable from PersonTab in PersonTable.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (D.Address.ToLower().Contains(term.ToLower()))
                        || string.IsNullOrEmpty(term) ? 1 == 1 : (PersonTab.Name.ToLower().Contains(term.ToLower())))
                        orderby PersonTab.Name
                        select new ComboBoxResult
                        {
                            id = D.PersonAddressID.ToString(),
                            text = PersonTab.Name,
                            AProp1 = D.Address
                        }
              );

            return list;
        }



        public IQueryable<ComboBoxResult> GetCurrencies(string term)
        {
            var list = (from D in db.Currency
                        where D.IsActive == true
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (D.Name.ToLower().Contains(term.ToLower())))
                        orderby D.Name
                        select new ComboBoxResult
                        {
                            id = D.ID.ToString(),
                            text = D.Name
                        }
              );

            return list;
        }


        public IQueryable<ComboBoxResult> GetSalesTaxGroupPerson(string term)
        {
            var list = (from D in db.ChargeGroupPerson
                        where D.IsActive == true
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (D.ChargeGroupPersonName.ToLower().Contains(term.ToLower())))
                        orderby D.ChargeGroupPersonName
                        select new ComboBoxResult
                        {
                            id = D.ChargeGroupPersonId.ToString(),
                            text = D.ChargeGroupPersonName
                        }
              );

            return list;
        }

        public IQueryable<ComboBoxResult> GetSalesTaxGroupProduct(string term)
        {
            var list = (from D in db.ChargeGroupProduct
                        where D.IsActive == true
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (D.ChargeGroupProductName.ToLower().Contains(term.ToLower())))
                        orderby D.ChargeGroupProductName
                        select new ComboBoxResult
                        {
                            id = D.ChargeGroupProductId.ToString(),
                            text = D.ChargeGroupProductName
                        }
              );

            return list;
        }


        public IQueryable<ComboBoxResult> GetChargeType(string term)
        {
            var list = (from D in db.ChargeType
                        where D.IsActive == true
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (D.ChargeTypeName.ToLower().Contains(term.ToLower())))
                        orderby D.ChargeTypeName
                        select new ComboBoxResult
                        {
                            id = D.ChargeTypeId.ToString(),
                            text = D.ChargeTypeName
                        }
              );

            return list;
        }

        public IQueryable<ComboBoxResult> GetPerson(string term)
        {
            var list = (from D in db.Persons
                        where D.IsActive == true
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (D.Name.ToLower().Contains(term.ToLower())))
                        orderby D.Name
                        select new ComboBoxResult
                        {
                            id = D.PersonID.ToString(),
                            text = D.Name
                        }
              );

            return list;
        }



        public IQueryable<ComboBoxResult> GetShipMethods(string term)
        {
            var list = (from D in db.ShipMethod
                        where D.IsActive == true
                        orderby D.ShipMethodName
                        select new ComboBoxResult
                        {
                            id = D.ShipMethodId .ToString(),
                            text = D.ShipMethodName
                        }
              );

            return list;
        }

        public IQueryable<ComboBoxResult> GetDocumentShipMethods(string term)
        {
            var list = (from D in db.DocumentShipMethod
                        where D.IsActive == true
                        orderby D.DocumentShipMethodName
                        select new ComboBoxResult
                        {
                            id = D.DocumentShipMethodId.ToString(),
                            text = D.DocumentShipMethodName
                        }
              );

            return list;
        }

        public IQueryable<ComboBoxResult> GetTransporters(string term)
        {
            int TransporterDocTypeId = 0;
            var DocumentType = (from D in db.DocumentType where D.DocumentTypeName == MasterDocTypeConstants.Transporter select D).FirstOrDefault();
            if (DocumentType != null)
            {
                TransporterDocTypeId = DocumentType.DocumentTypeId;
            }

            var list = (from D in db.Persons
                        join Pr in db.PersonRole on D.PersonID equals Pr.PersonId into PersonRoleTable from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                        where D.IsActive == true && PersonRoleTab.RoleDocTypeId == TransporterDocTypeId
                        orderby D.Name
                        select new ComboBoxResult
                        {
                            id = D.PersonID.ToString(),
                            text = D.Name
                        }
              );

            return list;
        }

        public IQueryable<ComboBoxResult> GetAgents(string term)
        {
            int AgentDocTypeId = 0;
            var DocumentType = (from D in db.DocumentType where D.DocumentTypeName == MasterDocTypeConstants.Agent select D).FirstOrDefault();
            if (DocumentType != null)
            {
                AgentDocTypeId = DocumentType.DocumentTypeId;
            }

            var list = (from D in db.Persons
                        join Pr in db.PersonRole on D.PersonID equals Pr.PersonId into PersonRoleTable
                        from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                        where D.IsActive == true && PersonRoleTab.RoleDocTypeId == AgentDocTypeId
                        orderby D.Name
                        select new ComboBoxResult
                        {
                            id = D.PersonID.ToString(),
                            text = D.Name
                        }
              );

            return list;
        }

        //public IQueryable<ComboBoxResult> GetFinanciers(string term)
        //{
        //    int FinancierDocTypeId = 0;
        //    var DocumentType = (from D in db.DocumentType where D.DocumentTypeName == MasterDocTypeConstants.Financier select D).FirstOrDefault();
        //    if (DocumentType != null)
        //    {
        //        FinancierDocTypeId = DocumentType.DocumentTypeId;
        //    }

        //    var list = (from D in db.Persons
        //                join Pr in db.PersonRole on D.PersonID equals Pr.PersonId into PersonRoleTable
        //                from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
        //                where D.IsActive == true && PersonRoleTab.RoleDocTypeId == FinancierDocTypeId
        //                orderby D.Name
        //                select new ComboBoxResult
        //                {
        //                    id = D.PersonID.ToString(),
        //                    text = D.Name
        //                }
        //      );

        //    return list;
        //}


        public IQueryable<ComboBoxResult> GetFinanciers(string term, int? filter)
        {
            int FinancierDocCategoryId = 0;
            var DocumentCategory = (from D in db.DocumentCategory where D.DocumentCategoryName == TransactionDocCategoryConstants.Financier select D).FirstOrDefault();
            if (DocumentCategory != null)
            {
                FinancierDocCategoryId = DocumentCategory.DocumentCategoryId;
            }

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            string DivIdStr = "|" + DivisionId.ToString() + "|";
            string SiteIdStr = "|" + SiteId.ToString() + "|";

            var list = (from D in db.Persons
                        join bus in db.BusinessEntity on D.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join Pr in db.PersonRole on D.PersonID equals Pr.PersonId into PersonRoleTable
                        from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                        join Dt in db.DocumentType on PersonRoleTab.RoleDocTypeId equals Dt.DocumentTypeId into DocumentTypeTable from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                        join Pp in db.PersonProcess on D.PersonID equals Pp.PersonId into PersonProcessTable from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                        where D.IsActive == true && DocumentTypeTab.DocumentCategoryId == FinancierDocCategoryId
                        && (filter == null ? 1 == 1 : PersonProcessTab.ProcessId == filter)
                        && (string.IsNullOrEmpty(BusinessEntityTab.DivisionIds) ? 1 == 1 : BusinessEntityTab.DivisionIds.IndexOf(DivIdStr) != -1)
                        && (string.IsNullOrEmpty(BusinessEntityTab.SiteIds) ? 1 == 1 : BusinessEntityTab.SiteIds.IndexOf(SiteIdStr) != -1)
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : D.Name.ToLower().Contains(term.ToLower())
                            || string.IsNullOrEmpty(term) ? 1 == 1 : D.Code.ToLower().Contains(term.ToLower())
                            || string.IsNullOrEmpty(term) ? 1 == 1 : D.Suffix.ToLower().Contains(term.ToLower()))
                        group new { D } by new { D.PersonID } into Result
                        orderby Result.Max(m => m.D.Name)
                        select new ComboBoxResult
                        {
                            id = Result.Key.PersonID.ToString(),
                            text = Result.Max(m => m.D.Name + ", " + m.D.Suffix + " [" + m.D.Code + "]"),
                        }
              );

            return list;
        }

        public IQueryable<ComboBoxResult> GetSalesExecutives(string term)
        {
            int SalesExecutiveDocTypeId = 0;
            var DocumentType = (from D in db.DocumentType where D.DocumentTypeName == MasterDocTypeConstants.SalesExecutive select D).FirstOrDefault();
            if (DocumentType != null)
            {
                SalesExecutiveDocTypeId = DocumentType.DocumentTypeId;
            }

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            string DivIdStr = "|" + DivisionId.ToString() + "|";
            string SiteIdStr = "|" + SiteId.ToString() + "|";

            var list = (from D in db.Persons
                        join bus in db.BusinessEntity on D.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join Pr in db.PersonRole on D.PersonID equals Pr.PersonId into PersonRoleTable
                        from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                        where D.IsActive == true && PersonRoleTab.RoleDocTypeId == SalesExecutiveDocTypeId
                            && (string.IsNullOrEmpty(BusinessEntityTab.DivisionIds) ? 1 == 1 : BusinessEntityTab.DivisionIds.IndexOf(DivIdStr) != -1)
                            && (string.IsNullOrEmpty(BusinessEntityTab.SiteIds) ? 1 == 1 : BusinessEntityTab.SiteIds.IndexOf(SiteIdStr) != -1)
                            && (string.IsNullOrEmpty(term) ? 1 == 1 : D.Name.ToLower().Contains(term.ToLower())
                            || string.IsNullOrEmpty(term) ? 1 == 1 : D.Code.ToLower().Contains(term.ToLower())
                            || string.IsNullOrEmpty(term) ? 1 == 1 : D.Suffix.ToLower().Contains(term.ToLower()))
                        orderby D.Name
                        select new ComboBoxResult
                        {
                            id = D.PersonID.ToString(),
                            text = D.Name + ", " + D.Suffix + " [" + D.Code + "]"
                        }
              );

            return list;
        }

        public IQueryable<ComboBoxResult> GetChargeGroupProducts(string term)
        {
            var list = (from D in db.ChargeGroupProduct
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (D.ChargeGroupProductName.ToLower().Contains(term.ToLower())))
                        orderby D.ChargeGroupProductName
                        select new ComboBoxResult
                        {
                            id = D.ChargeGroupProductId.ToString(),
                            text = D.ChargeGroupProductName
                        }
              );
            return list;
        }


        public IQueryable<ComboBoxResult> GetSalesTaxProductCodes(string term)
        {
            var list = (from D in db.SalesTaxProductCode
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (D.Code.ToLower().Contains(term.ToLower())))
                        orderby D.Code
                        select new ComboBoxResult
                        {
                            id = D.SalesTaxProductCodeId.ToString(),
                            text = D.Code
                        }
              );
            return list;
        }

        public IQueryable<ComboBoxResult> GetBinLocations(string term, int filter)
        {
            var list = (from D in db.BinLocation
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (D.BinLocationName.ToLower().Contains(term.ToLower())))
                        && D.GodownId == filter
                        orderby D.BinLocationName
                        select new ComboBoxResult
                        {
                            id = D.BinLocationId.ToString(),
                            text = D.BinLocationName
                        }
              );
            return list;
        }

        public IEnumerable<ComboBoxList> GetProductCategoryHelpList(int? filter)
        {
            var temp = from p in db.ProductCategory
                       join t in db.ProductTypes on p.ProductTypeId equals t.ProductTypeId into table1
                       from tab1 in table1.DefaultIfEmpty()
                       where ((filter == null || filter == 0) ? 1 == 1 : tab1.ProductTypeId == filter)
                       orderby p.ProductCategoryName, tab1.ProductTypeName
                       select new ComboBoxList
                       {
                           Id = p.ProductCategoryId,
                           PropFirst = p.ProductCategoryName
                       };

            return temp;
        }

        public IEnumerable<ComboBoxList> GetProcessWithChildProcessHelpList(int? filter)
        {
            //var temp1 = from p in db.Process
            //           where ((filter == null || filter == 0) ? 1 == 1 : p.ParentProcessId == filter)
            //           orderby p.ProcessName
            //           select new ComboBoxList
            //           {
            //               Id = p.ProcessId,
            //               PropFirst = p.ProcessName
            //           };

            //var temp2 = from p in db.Process
            //            where p.ProcessId == filter
            //            orderby p.ProcessName
            //            select new ComboBoxList
            //            {
            //                Id = p.ProcessId,
            //                PropFirst = p.ProcessName
            //            };

            //var temp = temp1.Union(temp2).Distinct().OrderBy(i => i.PropFirst);

            SqlParameter SqlParameterProcessId = new SqlParameter("@ProcessId", filter ?? 0);

            IEnumerable<ComboBoxList> ProcessList = db.Database.SqlQuery<ComboBoxList>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spGetHelpListProcessWithChildProcess @ProcessId", SqlParameterProcessId).ToList();

            return ProcessList;
        }

        public IQueryable<ComboBoxResult> GetSites(string term)
        {
            var list = (from D in db.Site
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (D.SiteName.ToLower().Contains(term.ToLower())))
                        orderby D.SiteName
                        select new ComboBoxResult
                        {
                            id = D.SiteId.ToString(),
                            text = D.SiteName
                        }
              );
            return list;
        }

        public IQueryable<ComboBoxResult> GetDivisions(string term)
        {
            var list = (from D in db.Divisions
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (D.DivisionName.ToLower().Contains(term.ToLower())))
                        orderby D.DivisionName
                        select new ComboBoxResult
                        {
                            id = D.DivisionId.ToString(),
                            text = D.DivisionName
                        }
              );
            return list;
        }






        public IEnumerable<ComboBoxResult> GetProductIndexFilterParameter(string term)
        {
            List<ComboBoxResult> ResultList = new List<ComboBoxResult>();
            ResultList.Add(new ComboBoxResult { id = IndexFilterParameterConstants.All, text = IndexFilterParameterConstants.All });
            ResultList.Add(new ComboBoxResult { id = IndexFilterParameterConstants.Active, text = IndexFilterParameterConstants.Active });
            ResultList.Add(new ComboBoxResult { id = IndexFilterParameterConstants.InActive, text = IndexFilterParameterConstants.InActive });
            ResultList.Add(new ComboBoxResult { id = IndexFilterParameterConstants.Discontinue, text = IndexFilterParameterConstants.Discontinue });


            var list = (from D in ResultList
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (D.text.ToLower().Contains(term.ToLower())))
                        orderby D.text
                        select new ComboBoxResult
                        {
                            id = D.id,
                            text = D.text
                        }
              );
            return list;
        }

        public IQueryable<ComboBoxResult> GetLedgerAccountForGroup(string term, int? filter)
        {
            SqlParameter SqlParameterLedgerAccountGroupId = new SqlParameter("@LedgerAccountGroupId", filter);

            if (filter == null)
            {
                SqlParameterLedgerAccountGroupId.Value = DBNull.Value;
            }

            IQueryable<ComboBoxResult> LedgerAccountList = db.Database.SqlQuery<ComboBoxResult>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spGetHelpListLedgerAccountForGroup @LedgerAccountGroupId", SqlParameterLedgerAccountGroupId).ToList().AsQueryable();

            var list = (from D in LedgerAccountList
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : D.text.ToLower().Contains(term.ToLower()))
                        orderby D.text
                        select new ComboBoxResult
                        {
                            id = D.id .ToString(),
                            text = D.text
                        }
              );

            return list;
        }

        public IQueryable<ComboBoxResult> GetAdditionalCharges(string term)
        {
            int AdditionalChargesProductNatureId = 0;
            var ProductNature = (from Pt in db.ProductNature where Pt.ProductNatureName == ProductNatureConstants.AdditionalCharges select Pt).FirstOrDefault();
            if (ProductNature != null)
            {
                AdditionalChargesProductNatureId = ProductNature.ProductNatureId;
            }

            var list = (from P in db.Product
                        where P.IsActive == true && P.ProductGroup.ProductType.ProductNatureId == AdditionalChargesProductNatureId
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : P.ProductName.ToLower().Contains(term.ToLower()))
                        orderby P.ProductName
                        select new ComboBoxResult
                        {
                            id = P.ProductId.ToString(),
                            text = P.ProductName,
                        }
              );

            return list;
        }


        public ComboBoxPagedResult GetRoles(string searchTerm, int pageSize, int pageNum)
        {

            var Query = (from ur in db.Roles
                         where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (ur.Name.ToLower().Contains(searchTerm.ToLower())))
                         && ur.Name != "SysAdmin"
                         orderby ur.Name
                         select new ComboBoxResult
                         {
                             text = ur.Name,
                             id = ur.Id,
                         }
            );

            var records = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = records;
            Data.Total = count;

            return Data;

        }



        public ComboBoxResult GetRole(string Id)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            var Roles = from p in db.Roles
                                        where p.Id == Id
                                        select p;

            ProductJson.id = Roles.FirstOrDefault().Id;
            ProductJson.text = Roles.FirstOrDefault().Name;

            return ProductJson;
        }

        public List<ComboBoxResult> GetMultipleRoles(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                string temp = subStr[i];
                var Roles = from p in db.Roles
                                            where p.Id == temp
                                            select p;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = Roles.FirstOrDefault().Id,
                    text = Roles.FirstOrDefault().Name
                });
            }

            return ProductJson;
        }


        #region "For Sql Procedure Help"

        public CustomComboBoxPagedResult GetSelect2HelpList(string SqlProcGet, string searchTerm, int pageSize, int pageNum)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            if (SqlProcGet.Contains(" ") == true)
                SqlProcGet = SqlProcGet + " ,";

            string mQry;

            mQry = " " + SqlProcGet + " @SearchString = '" + searchTerm + "', @PageSize =" + pageSize.ToString() + ", @PageNo =" + (pageNum - 1).ToString() + ", @SiteId= " + SiteId.ToString() + ", @DivisionId= " + DivisionId.ToString();
            IEnumerable<CustomComboBoxResult> Select2List = db.Database.SqlQuery<CustomComboBoxResult>(mQry).ToList();

            CustomComboBoxPagedResult pagedAttendees = new CustomComboBoxPagedResult();
            pagedAttendees.Results = Select2List.ToList();
            pagedAttendees.Total = Select2List.Count() > 0 ? Select2List.FirstOrDefault().RecCount : 0;

            return pagedAttendees;

        }


        public List<ComboBoxResult> SetSelct2Data(string Id, string SqlProcSet)
        {
            if (SqlProcSet.Contains(" ") == true)
                SqlProcSet = SqlProcSet + " ,";


            List<ComboBoxResult> ProductJson = db.Database.SqlQuery<ComboBoxResult>(" " + SqlProcSet + " @Ids = \'" + Id + "\'").ToList();

            return ProductJson;
        }

        public ComboBoxResult SetSingleSelect2Data(int Id, string SqlProcSet)
        {
            SqlParameter SqlParameterDocId = new SqlParameter("@Ids", Id);
            ComboBoxResult ProductJson = db.Database.SqlQuery<ComboBoxResult>(" " + SqlProcSet + " @Ids ", SqlParameterDocId).FirstOrDefault();

            return ProductJson;
        }

        #endregion


    }
}


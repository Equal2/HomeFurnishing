using System.Collections.Generic;
using System.Linq;
using Data.Infrastructure;
using Model.ViewModel;
using System;
using Data.Models;
using System.Data.SqlClient;
using Model.ViewModels;

namespace Service
{
    public interface IDisplay_SaleOrderBalanceService : IDisposable
    {
        IQueryable<ComboBoxResult> GetFilterFormat(string term, int? filter);
    
        IEnumerable<SaleOrderBalancelOrderNoWiseViewModel> SaleOrderBalanceDetail(SaleDisplayFilterSettings Settings);
    }

    public class Display_SaleOrderBalanceService : IDisplay_SaleOrderBalanceService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

      

        public Display_SaleOrderBalanceService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;            
        }

        public IQueryable<ComboBoxResult> GetFilterFormat(string term, int? filter)
        {
            List<ComboBoxResult> ResultList = new List<ComboBoxResult>();
            ResultList.Add(new ComboBoxResult { id = SaleReportFormat.BuyerWise, text = SaleReportFormat.BuyerWise });
            ResultList.Add(new ComboBoxResult { id = SaleReportFormat.MonthWise, text = SaleReportFormat.MonthWise });
            ResultList.Add(new ComboBoxResult { id = SaleReportFormat.ProdTypeWise, text = SaleReportFormat.ProdTypeWise });
            ResultList.Add(new ComboBoxResult { id = SaleReportFormat.ProductNatureWiseSummary, text = SaleReportFormat.ProductNatureWiseSummary });
            ResultList.Add(new ComboBoxResult { id = SaleReportFormat.OrderNoWise, text = SaleReportFormat.OrderNoWise });
            

            var list = (from D in ResultList
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (D.text.ToLower().Contains(term.ToLower())))
                        orderby D.text
                        select new ComboBoxResult
                        {
                            id = D.id,
                            text = D.text
                        }
             );
            return list.AsQueryable();
        }


     
      
        
        public IEnumerable<SaleOrderBalancelOrderNoWiseViewModel> SaleOrderBalanceDetail(SaleDisplayFilterSettings Settings)
        {
            var FormatSetting = (from H in Settings.SaleDisplayFilterParameters where H.ParameterName == "Format" select H).FirstOrDefault();
            var SiteSetting = (from H in Settings.SaleDisplayFilterParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in Settings.SaleDisplayFilterParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in Settings.SaleDisplayFilterParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in Settings.SaleDisplayFilterParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var ProductNatureSetting = (from H in Settings.SaleDisplayFilterParameters where H.ParameterName == "ProductNature" select H).FirstOrDefault();
            var ProductTypeSetting = (from H in Settings.SaleDisplayFilterParameters where H.ParameterName == "ProductType" select H).FirstOrDefault();
            var BuyerSetting = (from H in Settings.SaleDisplayFilterParameters where H.ParameterName == "Buyer" select H).FirstOrDefault();
            var ProductCategorySetting = (from H in Settings.SaleDisplayFilterParameters where H.ParameterName == "ProductCategory" select H).FirstOrDefault(); 
            var TextHiddenSetting = (from H in Settings.SaleDisplayFilterParameters where H.ParameterName == "TextHidden" select H).FirstOrDefault();



            string Format = FormatSetting.Value;
            string SiteId = SiteSetting.Value;
            string DivisionId = DivisionSetting.Value;
            string FromDate = FromDateSetting.Value;
            string ToDate = ToDateSetting.Value;
            string ProductNature = ProductNatureSetting.Value;
            string ProductType = ProductTypeSetting.Value;
            string Buyer = BuyerSetting.Value;
            string ProductCategory = ProductCategorySetting.Value;
            string TextHidden = TextHiddenSetting.Value;


            string mQry, mCondStr;


            mCondStr = "";
            if (!string.IsNullOrEmpty(SiteId)) mCondStr += " AND H.SiteId = @Site ";
            if (!string.IsNullOrEmpty(DivisionId)) mCondStr += " AND H.DivisionId = @Division ";
            if (!string.IsNullOrEmpty(FromDate)) mCondStr += " AND H.DocDate >= @FromDate ";
            if (!string.IsNullOrEmpty(ToDate)) mCondStr += " AND H.DocDate <= @ToDate ";
            if (!string.IsNullOrEmpty(ProductNature)) mCondStr += " AND PT.ProductNatureId = @ProductNature ";
            if (!string.IsNullOrEmpty(ProductType)) mCondStr += " AND PT.ProductTypeId = @ProductType ";
            if (!string.IsNullOrEmpty(Buyer)) mCondStr += " AND H.BillToBuyerId = @Buyer ";
            if (!string.IsNullOrEmpty(ProductCategory)) mCondStr += " AND P.ProductCategoryId = @ProductCategory ";



            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterProductNature = new SqlParameter("@ProductNature", !string.IsNullOrEmpty(ProductNature) ? ProductNature : (object)DBNull.Value);
            SqlParameter SqlParameterProductType = new SqlParameter("@ProductType", !string.IsNullOrEmpty(ProductType) ? ProductType : (object)DBNull.Value);
            SqlParameter SqlParameterProductCategory = new SqlParameter("@ProductCategory", !string.IsNullOrEmpty(ProductCategory) ? ProductCategory : (object)DBNull.Value);
            SqlParameter SqlParameterJobWorker = new SqlParameter("@Buyer", !string.IsNullOrEmpty(Buyer) ? Buyer : (object)DBNull.Value);
            





            mQry = @"With CteSaleOrderBalance as
                            (
                                            SELECT 
                                            H.SaleOrderHeaderId,
                                            H.DocTypeId,
                                            DT.DocumentTypeShortName+'-'+H.DocNo as DocNo,
                                            H.DocDate,
                                            H.DueDate,
                                            H.BillToBuyerId,
                                            Pp.Name+' | '+Pp.Code as BuyerName,
                                            L.ProductId,
                                            P.ProductName,
                                            L.Specification,
                                            U.UnitName,
                                            'Sq. Yard' AS DealUnitName,
                                            PG.ProductTypeId, PT.ProductTypeName, PT.ProductNatureId, PN.ProductNatureName,
                                            isnull(L.Qty,0)-isnull(LC.Qty,0)+isnull(LA.Qty,0) AS OrderQty,
                                            isnull(L.Qty,0)-isnull(LC.Qty,0)+isnull(LA.Qty,0)-isnull(PL.Qty,0) AS BalanceQty,
                                            (isnull(L.Qty,0)-isnull(LC.Qty,0)+isnull(LA.Qty,0)-isnull(PL.Qty,0))*(SELECT SqYard FROM  Web.[FuncConvertSqFeetToSqYard] (UC.StandardSizeArea) ) AS BalanceDealQty,
                                            (isnull(L.Qty,0)-isnull(LC.Qty,0)+isnull(LA.Qty,0)-isnull(PL.Qty,0))*(CASE WHEN isnull(L.Qty,0) = 0 THEN L.Rate ELSE L.Amount / isnull(L.Qty,0) END) AS BalanceAmount,
                                            C.Name AS CurrencyName,
                                            (CASE WHEN isnull(L.Qty,0) = 0 THEN L.Rate ELSE L.Amount / isnull(L.Qty,0) END) AS RatePerQty,
                                            D1.Dimension1Name,
                                            D2.Dimension2Name,
                                            D3.Dimension3Name,
                                            D4.Dimension4Name,
                                            PC.ProductCategoryId,
                                            PC.ProductCategoryName,
                                            D.Dimension1Caption,D.Dimension2Caption,D.Dimension3Caption,D.Dimension4Caption
                                            FROM(SELECT * FROM  Web.SaleOrderHeaders) H
                                            LEFT JOIN Web.SaleOrderLines L ON L.SaleOrderHeaderId=H.SaleOrderHeaderId
                                            LEFT JOIN 
                                            (
	                                            SELECT L.SaleOrderLineId,sum(isnull(L.Qty,0)) AS Qty FROM Web.SaleOrderCancelLines L
	                                            GROUP BY L.SaleOrderLineId
                                            ) LC ON LC.SaleOrderLineId=L.SaleOrderLineId
                                            LEFT JOIN 
                                            (
	                                            SELECT L.SaleOrderLineId,sum(isnull(L.Qty,0)) AS Qty FROM Web.SaleOrderQtyAmendmentLines L
	                                            GROUP BY L.SaleOrderLineId
                                            ) LA ON  LA.SaleOrderLineId=L.SaleOrderLineId
                                            LEFT JOIN 
                                            (
	                                            SELECT Pl.SaleOrderLineId,sum(isnull(PL.Qty,0)) AS Qty FROM Web.SaleDispatchLines L  WITH (Nolock)
	                                            LEFT JOIN Web.PackingLines Pl WITH (Nolock) ON L.PackingLineId = Pl.PackingLineId
	                                            GROUP BY Pl.SaleOrderLineId
                                            ) PL ON PL.SaleOrderLineId=L.SaleOrderLineId
                                            LEFT JOIN Web.DocumentTypes DT WITH (Nolock)  ON DT.DocumentTypeId=H.DocTypeId
                                            LEFT JOIN Web.Products P WITH (Nolock) ON P.ProductId=L.ProductId
                                            LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId=P.ProductGroupId
                                            LEFT JOIN Web.ProductTypes PT WITH (Nolock) ON PT.ProductTypeId=PG.ProductTypeId
                                            LEFT JOIN Web.ProductNatures PN WITH (Nolock) ON PN.ProductNatureId=PT.ProductNatureId
                                            LEFT JOIN web.People Pp WITH (Nolock) ON Pp.PersonID=H.BillToBuyerId
                                            LEFT JOIN web.Units U WITH (Nolock) ON U.UnitId=P.UnitId
                                            LEFT JOIN Web.Currencies C WITH (Nolock) ON C.ID=H.CurrencyId
                                            LEFT JOIN web.Dimension1 D1 WITH (Nolock) ON D1.Dimension1Id=L.Dimension1Id
                                            LEFT JOIN web.Dimension2 D2 WITH (Nolock) ON D2.Dimension2Id=L.Dimension2Id
                                            LEFT JOIN web.Dimension3 D3 WITH (Nolock) ON D3.Dimension3Id=L.Dimension3Id
                                            LEFT JOIN web.Dimension4 D4 WITH (Nolock) ON D4.Dimension4Id=L.Dimension4Id
                                            LEFT JOIN Web.ViewRugSize UC WITH (Nolock) ON UC.ProductId = L.ProductId
                                            LEFT JOIN Web.ProductCategories PC WITH (Nolock) ON PC.ProductCategoryId=P.ProductCategoryId 
                                            LEFT JOIN Web.DivisionSettings D WITH (Nolock) ON D.DivisionId=H.DivisionId
                                            WHERE 1=1 AND (isnull(L.Qty,0)-isnull(LC.Qty,0)+isnull(LA.Qty,0)-isnull(PL.Qty,0)) >0 "
                                    + mCondStr + ") ";

            if (Format == SaleReportFormat.OrderNoWise || Format =="" || string.IsNullOrEmpty(Format))
            {
                mQry += @"  Select H.SaleOrderHeaderId, H.DocTypeId, H.DocNo,format(H.DocDate,'dd/MMM/yyyy') as DocDate,format(H.DueDate,'dd/MMM/yyyy') as DueDate, H.BuyerName,H.ProductName, H.Specification, H.UnitName,
                            H.CurrencyName,H.DealUnitName,H.OrderQty,H.BalanceQty, H.BalanceDealQty,H.BalanceAmount,H.RatePerQty,H.Dimension1Name,H.Dimension2Name,H.Dimension3Name ,H.Dimension4Name,H.Dimension1Caption,H.Dimension2Caption,H.Dimension3Caption,HDimension4Caption
                            From CteSaleOrderBalance H 
                            Order By H.DocDate, H.DocTypeId, H.DocNo
                        ";
            }
            else if (Format == SaleReportFormat.BuyerWise)
            {
                
                   mQry += @"  Select H.BillToBuyerId, Max(H.BuyerName) BuyerName,Sum(H.OrderQty) as OrderQty,Sum(H.BalanceQty) as BalanceQty, Sum(H.BalanceDealQty) as BalanceDealQty, Sum(H.BalanceAmount) as BalanceAmount,'Order No Wise' as Format,
                              max(H.UnitName) as UnitName,max(H.DealUnitName) as DealUnitName,max(H.CurrencyName) as CurrencyName
                            From CteSaleOrderBalance H 
                            Group By H.BillToBuyerId
                            Order By Max(H.BuyerName) 
                         ";
            }
            else if (Format == SaleReportFormat.ProdTypeWise)
            {
                mQry += @"  Select H.ProductTypeId, Max(H.ProductTypeName) ProductTypeName,Sum(H.OrderQty) as OrderQty,Sum(H.BalanceQty) as BalanceQty, Sum(H.BalanceDealQty) as BalanceDealQty, Sum(H.BalanceAmount) as BalanceAmount,'Order No Wise' as Format,
                             max(H.UnitName) as UnitName,max(H.DealUnitName) as DealUnitName,max(H.CurrencyName) as CurrencyName                            
                            From CteSaleOrderBalance H 
                            Group By H.ProductTypeId
                            Order By Max(H.ProductTypeName)
                         ";
            }
            else if (Format == SaleReportFormat.ProductNatureWiseSummary)
            {
                mQry += @"  Select H.ProductNatureId, Max(H.ProductNatureName) ProductNatureName,Sum(H.OrderQty) as OrderQty,Sum(H.BalanceQty) as BalanceQty, Sum(H.BalanceDealQty) as BalanceDealQty, Sum(H.BalanceAmount) as BalanceAmount,'Order No Wise' as Format,
                            max(H.UnitName) as UnitName,max(H.DealUnitName) as DealUnitName,max(H.CurrencyName) as CurrencyName                            
                            From CteSaleOrderBalance H 
                            Group By H.ProductNatureId
                            Order By  Max(H.ProductNatureName)
                         ";
            }
            else if (Format == SaleReportFormat.MonthWise )
            {
                mQry += @"  Select format(Max(DATEADD(dd,-(DAY(H.DocDate)-1),H.DocDate)),'dd/MMM/yyyy') AS FromDate,format(Max(DATEADD(dd,-(DAY(DATEADD(mm,1,H.DocDate))),DATEADD(mm,1,H.DocDate))),'dd/MMM/yyyy') as ToDate, Max(Right(Convert(Varchar,H.DocDate,106),8)) as Month,Sum(H.OrderQty) as OrderQty,Sum(H.BalanceQty) as BalanceQty, Sum(H.BalanceDealQty) as BalanceDealQty, Sum(H.BalanceAmount) as BalanceAmount,'Order No Wise' as Format,
                            max(H.UnitName) as UnitName,max(H.DealUnitName) as DealUnitName,max(H.CurrencyName) as CurrencyName                             
                            From CteSaleOrderBalance H 
                            Group By Substring(convert(NVARCHAR,H.DocDate,11),0,6)
                            Order by Substring(convert(NVARCHAR,H.DocDate,11),0,6)
                         ";
            }
            else if (Format == SaleReportFormat.ConstructionWise)
            {
                mQry += @"  Select H.ProductCategoryId, Max(H.ProductCategoryName) ProductCategoryName,Sum(H.OrderQty) as OrderQty,Sum(H.BalanceQty) as BalanceQty, Sum(H.BalanceDealQty) as BalanceDealQty, Sum(H.BalanceAmount) as BalanceAmount,'Order No Wise' as Format,
                            max(H.UnitName) as UnitName,max(H.DealUnitName) as DealUnitName,max(H.CurrencyName) as CurrencyName                            
                            From CteSaleOrderBalance H 
                            Group By H.ProductCategoryId
                            Order By  Max(H.ProductCategoryName)
                         ";
            }
            IEnumerable<SaleOrderBalancelOrderNoWiseViewModel> TrialBalanceSummaryList = db.Database.SqlQuery<SaleOrderBalancelOrderNoWiseViewModel>(mQry, SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterProductNature, SqlParameterProductType, SqlParameterJobWorker).ToList();


            return TrialBalanceSummaryList;

        }

      
        public void Dispose()
        {
        }
    }

     public class Display_SaleOrderBalanceViewModel
    {
        public string Format { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string ProductNature { get; set; }
        public string ProductType { get; set;}
        public string ProductCategory { get; set; }
        public string Buyer { get; set; }
        public string SiteIds { get; set; }
        public string DivisionIds { get; set; }  
        public string TextHidden { get; set; }


    }

    [Serializable()]
    public class SaleDisplayFilterSettings
    {
        public string Format { get; set; }
        public List<SaleDisplayFilterParameters> SaleDisplayFilterParameters { get; set; }
    }

    [Serializable()]
    public class SaleDisplayFilterParameters
    {
        public string ParameterName { get; set; }
        public bool IsApplicable { get; set; }
        public string Value { get; set; }
    }

    public class SaleReportFormat
    {
        public const string BuyerWise = "Buyer Wise Summary";
        public const string MonthWise = "Month Wise Summary";
        public const string ProdTypeWise = "Product Type Wise Summary";
        public const string ProductNatureWiseSummary = "Product Nature Wise Summary";
        public const string OrderNoWise = "Order No Wise";
        public const string ConstructionWise = "Construction Wise";
    }
    public class SaleOrderBalancelOrderNoWiseViewModel
    {
        public int? SaleOrderHeaderId { get; set; }
        public int? DocTypeId { get; set; }
        public string DocNo { get; set; }
        public string DocDate { get; set; }
        public string DueDate { get; set; }
        public int? BillToBuyerId { get; set; }
        public string BuyerName { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public string Specification { get; set; }
        public string UnitName { get; set; }
        public string DealUnitName { get; set; }
        public int? ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public int? ProductNatureId { get; set; }
        public string ProductNatureName { get; set; }
        public decimal ? OrderQty { get; set; } 
        public decimal?    BalanceQty { get; set; } 
        public decimal?    BalanceDealQty { get; set; } 
        public decimal?     BalanceAmount { get; set; } 
        public string    CurrencyName { get; set; } 
        public decimal?    RatePerQty { get; set; } 
        public string    Dimension1Name { get; set; } 
        public string    Dimension2Name { get; set; } 
        public string    Dimension3Name { get; set; } 
        public string    Dimension4Name { get; set; }
        public string Dimension1Caption { get; set; }
        public string  Dimension2Caption { get; set; }
        public string Dimension3Caption { get; set; }
        public string Dimension4Caption { get; set; }
        public int? ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public string Month { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Format { get; set; }

    }

}


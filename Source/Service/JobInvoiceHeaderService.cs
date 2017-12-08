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
using Model.ViewModel;
using System.Data.Entity.SqlServer;
using Model.ViewModels;
using System.Data.SqlClient;

namespace Service
{
    public interface IJobInvoiceHeaderService : IDisposable
    {
        JobInvoiceHeader Create(JobInvoiceHeader pt);
        void Delete(int id);
        void Delete(JobInvoiceHeader pt);
        JobInvoiceHeader Find(string Name);
        JobInvoiceHeader Find(int id);
        IEnumerable<JobInvoiceHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobInvoiceHeader pt);
        JobInvoiceHeader Add(JobInvoiceHeader pt);
        JobInvoiceHeaderViewModel GetJobInvoiceHeader(int id);//HeadeRId
        IQueryable<JobInvoiceHeaderViewModel> GetJobInvoiceHeaderList(int id, string Uname);
        IQueryable<JobInvoiceHeaderViewModel> GetJobInvoiceHeaderListPendingToSubmit(int id, string Uname);
        IQueryable<JobInvoiceHeaderViewModel> GetJobInvoiceHeaderListPendingToReview(int id, string Uname);
        Task<IEquatable<JobInvoiceHeader>> GetAsync();
        Task<JobInvoiceHeader> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        string GetMaxDocNo();
        IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term, int? ProcessId = null);
        PersonViewModel GetJobWorkerDetail(int id);
        IEnumerable<DocumentTypeHeaderAttributeViewModel> GetDocumentHeaderAttribute(int id);
		IEnumerable<PrintViewModel> JobInvoicePrint(string HeaderId,string UserName);
    }

    public class JobInvoiceHeaderService : IJobInvoiceHeaderService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobInvoiceHeader> _JobInvoiceHeaderRepository;
        RepositoryQuery<JobInvoiceHeader> JobInvoiceHeaderRepository;
        public JobInvoiceHeaderService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobInvoiceHeaderRepository = new Repository<JobInvoiceHeader>(db);
            JobInvoiceHeaderRepository = new RepositoryQuery<JobInvoiceHeader>(_JobInvoiceHeaderRepository);
        }

        public JobInvoiceHeader Find(string Name)
        {
            return JobInvoiceHeaderRepository.Get().Where(i => i.DocNo == Name).FirstOrDefault();
        }


        public JobInvoiceHeader Find(int id)
        {
            return _unitOfWork.Repository<JobInvoiceHeader>().Find(id);
        }

        public JobInvoiceHeader Create(JobInvoiceHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobInvoiceHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobInvoiceHeader>().Delete(id);
        }

        public void Delete(JobInvoiceHeader pt)
        {
            _unitOfWork.Repository<JobInvoiceHeader>().Delete(pt);
        }

        public void Update(JobInvoiceHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobInvoiceHeader>().Update(pt);
        }

        public IEnumerable<JobInvoiceHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobInvoiceHeader>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }
        public JobInvoiceHeaderViewModel GetJobInvoiceHeader(int id)
        {
            return (from p in db.JobInvoiceHeader
                    where p.JobInvoiceHeaderId == id
                    select new JobInvoiceHeaderViewModel
                    {
                        JobInvoiceHeaderId = p.JobInvoiceHeaderId,
                        DivisionId = p.DivisionId,
                        DocNo = p.DocNo,
                        DocDate = p.DocDate,
                        ProcessId=p.ProcessId,                        
                        DocTypeId = p.DocTypeId,
                        JobWorkerDocNo=p.JobWorkerDocNo,
                        JobWorkerDocDate=p.JobWorkerDocDate,
                        GovtInvoiceNo=p.GovtInvoiceNo,
                        Remark = p.Remark,
                        SiteId = p.SiteId,
                        Status = p.Status,
                        JobWorkerId=p.JobWorkerId,
                        FinancierId = p.FinancierId,
                        JobReceiveHeaderId=p.JobReceiveHeaderId,
                        SalesTaxGroupPersonId = p.SalesTaxGroupPersonId,
                        ModifiedBy=p.ModifiedBy,
                        LockReason=p.LockReason,
                        CreatedBy=p.CreatedBy,
                        CreatedDate=p.CreatedDate,
                    }

                        ).FirstOrDefault();
        }

        public IEnumerable<PrintViewModel> JobInvoicePrint(string HeaderId,string UserName)
        {
            string mQry;
            int   SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];            
            SqlParameter SqlParameterHeaderId = new SqlParameter("@Id", !string.IsNullOrEmpty(HeaderId) ? HeaderId : (object)DBNull.Value);
            
            mQry = @" DECLARE @TotalAmount DECIMAL 
	                SET @TotalAmount = (SELECT Max(Amount) FROM Web.JobInvoiceHeaderCharges WHERE HeaderTableId = @Id AND ChargeId = 34 ) 
	 
	                DECLARE @ReturnAmount DECIMAL
	                DECLARE @DebitAmount DECIMAL
	                DECLARE @CreaditAmount DECIMAL
	  
	                SELECT 
	                @ReturnAmount=sum(CASE WHEN CT.ChargeTypeName IN ('Amount','CGST','SGST','IGST') AND JIRH.Nature='Return' THEN isnull(H.Amount,0) ELSE 0 END ),
	                @DebitAmount=sum(CASE WHEN JIRH.Nature='Debit Note' AND C.ChargeName='Net Amount' THEN isnull(H.Amount,0) ELSE 0 END ),
	                @CreaditAmount=sum(CASE WHEN JIRH.Nature='Credit Note' AND C.ChargeName='Net Amount' THEN isnull(H.Amount,0) ELSE 0 END )
	                FROM Web.JobInvoiceReturnLineCharges H
	                LEFT JOIN Web.ChargeTypes CT WITH (Nolock) ON CT.ChargeTypeId=H.ChargeTypeId
	                LEFT JOIN web.Charges C WITH (Nolock) ON C.ChargeId=H.ChargeId
	                LEFT JOIN Web.JobInvoiceReturnLines JIRL WITH (Nolock) ON JIRL.JobInvoiceReturnLineId=H.LineTableId
	                LEFT JOIN Web.JobInvoiceLines JIL WITH (Nolock) ON JIL.JobInvoiceLineId=JIRL.JobInvoiceLineId
	                LEFT JOIN Web.JobInvoiceReturnHeaders JIRH WITH (Nolock) ON JIRH.JobInvoiceReturnHeaderId=JIRL.JobInvoiceReturnHeaderId
	                WHERE JIL.JobInvoiceHeaderId=@Id  
	  
	                SET @TotalAmount=isnull(@TotalAmount,0)-isnull(@ReturnAmount,0)-isnull(@DebitAmount,0)+isnull(@CreaditAmount,0)
	  
	  
	  
	  
	  
	                DECLARE @UnitDealCnt INT	
	                SELECT 
	                @UnitDealCnt=sum(CASE WHEN JOL.UnitId != JOL.DealunitId THEN 1 ELSE 0 END )
	                FROM Web.JobInvoiceLines L WITH (Nolock) 
	                LEFT JOIN web.JobReceiveLines JRL WITH (Nolock) ON JRL.JobReceiveLineId=L.JobReceiveLineId
	                LEFT JOIN Web.JobOrderLines JOL WITH (Nolock) ON JOL.JobOrderLineId=JRL.JobOrderLineId
	                WHERE L.JobInvoiceHeaderId=@Id
  
	
	                DECLARE @DocDate DATETIME
	                DECLARE @Site INT 
	                DECLARE @Division INT 
                    SELECT  @DocDate=DocDate,@Site=SiteId,@Division=DivisionId FROM Web.JobInvoiceHeaders WHERE JobInvoiceHeaderId=@Id

	
	
	                SELECT 
	                --Header Table Fields	
	                H.JobInvoiceHeaderId AS HeaderTableId,H.DocTypeId,H.DocNo,DocIdCaption+' No' AS DocIdCaption ,
	                H.SiteId,H.DivisionId,format(H.DocDate,'dd/MMM/yy') as DocDate,DTS.DocIdCaption +' Date' AS DocDateCaption ,	
   	                H.JobWorkerDocNo AS PartyDocNo,	DTS.PartyCaption +' Doc No' AS PartyDocCaption,format(H.JobWorkerDocDate,'dd/MMM/yy') AS PartyDocDate,
                    DTS.PartyCaption +' Doc Date' AS PartyDocDateCaption,H.CreditDays,isnull(H.Remark,' ') AS HeaderRemark,DT.DocumentTypeShortName,	
	                H.ModifiedBy +' ' + Replace(replace(convert(NVARCHAR, H.ModifiedDate, 106), ' ', '/'),'/20','/') + substring (convert(NVARCHAR,H.ModifiedDate),13,7) AS ModifiedBy,
	                format(H.ModifiedDate,'dd/MMM/yy') as ModifiedDate,(CASE WHEN Isnull(H.Status,0)=0 OR Isnull(H.Status,0)=8 THEN 0 ELSE 1 END)  AS Status,
	                isnull(CUR.Name,'') AS CurrencyName,(CASE WHEN SPR.[Party GST NO] IS NULL THEN 'Yes' ELSE 'No' END ) AS ReverseChargeYN,
	                VDC.CompanyName,P.Name AS PartyName, DTS.PartyCaption AS  PartyNameCaption, P.Suffix AS PartySuffix,' ' AS PartyPrefix,	
	                isnull(PA.Address,'')+' '+isnull(C.CityName,'')+','+isnull(PA.ZipCode,'')+(CASE WHEN isnull(CS.StateName,'') <> isnull(S.StateName,'') AND SPR.[Party GST NO] IS NOT NULL THEN ',State : '+isnull(S.StateName,'')+(CASE WHEN S.StateCode IS NULL THEN '' ELSE ', Code : '+S.StateCode END)    ELSE '' END ) AS PartyAddress,
	                isnull(S.StateName,'') AS PartyStateName,isnull(S.StateCode,'') AS PartyStateCode,	
	                isnull(P.Mobile,'') AS PartyMobileNo,
	                isnull(SPR.[Party TIN NO],'') AS PartyTinNo,isnull(SPR.[Party PAN NO],'') AS PartyPanNo,isnull(SPR.[Party AADHAR NO],'') AS PartyAadharNo,isnull(SPR.[Party GST NO],'') AS PartyGSTNo,isnull(SPR.[Party CST NO],'') AS PartCSTNo,
	                --Plan Detail
	                JRH.DocNo  AS ContraDocNo,DTS.ContraDocTypeCaption AS ContraDocTypeCaption,
	                --Caption Fields	
	                DTS.SignatoryleftCaption,DTS.SignatoryMiddleCaption,DTS.SignatoryRightCaption,
	                --Line Table
	                PD.ProductName,DTS.ProductCaption,U.UnitName,convert(int,U.DecimalPlaces) as DecimalPlaces,DU.UnitName AS DealUnitName,DTS.DealQtyCaption,convert(int,DU.DecimalPlaces) AS DealDecimalPlaces,
	                isnull(L.Qty,0) AS Qty,isnull(L.Rate,0) AS Rate,isnull(L.Amount,0) AS Amount,isnull(L.DealQty,0) AS DealQty,
	                D1.Dimension1Name,DTS.Dimension1Caption,D2.Dimension2Name,DTS.Dimension2Caption,D3.Dimension3Name,DTS.Dimension3Caption,D4.Dimension4Name,DTS.Dimension4Caption,
                    DTS.SpecificationCaption,(CASE WHEN DTS.PrintSpecification >0 THEN   JRL.Specification ELSE '' END)  AS Specification,L.Remark AS LineRemark,
	                --L.DiscountPer AS DiscountPer,L.DiscountAmt AS DiscountAmt,
	                Convert(DECIMAL(18,2),L.RateDiscountPer) AS DiscountPer,Convert(DECIMAL(18,2),L.RateDiscountAmt) AS DiscountAmt,
	                --STC.Code AS SalesTaxProductCodes,
	                (CASE WHEN H.ProcessId IN (26,28) THEN  STC.Code ELSE PSSTC.Code END)  AS SalesTaxProductCodes ,
	                (CASE WHEN DTS.PrintProductGroup >0 THEN isnull(PG.ProductGroupName,'') ELSE '' END)+(CASE WHEN DTS.PrintProductdescription >0 THEN isnull(','+PD.Productdescription,'') ELSE '' END) AS ProductGroupName,
	                DTS.ProductGroupCaption,isnull(CGPD.PrintingDescription,isnull(CGPD.ChargeGroupProductName,' ')) AS ChargeGroupProductName,
	
                   --Receive Lines
	                 DTS.ProductUidCaption,PU.ProductUidName,
	                (CASE WHEN isnull(JRL.LossQty,0) >0 THEN isnull(JRL.LossQty,0) ELSE NULL END)  AS LossQty,
	                (CASE WHEN isnull(JRL.Qty,0) <> isnull(L.Qty,0) THEN CASE WHEN isnull(JRL.Qty,0) <> 0 THEN isnull(JRL.Qty,0) ELSE NULL END   ELSE NULL END) AS RecQty,
	                JRL.LotNo AS LotNo, 
	
	                --Formula Fields
	                isnull(@TotalAmount,0) AS NetAmount,  
	                isnull(@ReturnAmount,0) AS ReturnAmount,
	                isnull(@DebitAmount,0) AS DebitAmount,
	                isnull(@CreaditAmount,0) AS CreaditAmount,   	
	                --SalesTaxGroupPersonId
	                CGP.ChargeGroupPersonName,
	                --Other Fields
	                @UnitDealCnt  AS DealUnitCount,		
	                (CASE WHEN Isnull(H.Status,0)=0 OR Isnull(H.Status,0)=8 THEN 'Provisional ' +isnull(DT.PrintTitle,DT.DocumentTypeName) ELSE isnull(DT.PrintTitle,DT.DocumentTypeName) END) AS ReportTitle, 
                     SalesTaxGroupProductCaption,
	                SDS.SalesTaxProductCodeCaption,'"+ UserName + @"' as PrintedBy,convert(varchar,getdate()) as PrintDate
	                FROM Web.JobInvoiceHeaders H WITH (Nolock)
	                LEFT JOIN web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId=H.DocTypeId
	                LEFT JOIN Web._DocumentTypeSettings DTS WITH (Nolock) ON DTS.DocumentTypeId=DT.DocumentTypeId
	                LEFT JOIN Web.JobInvoiceSettings JIS WITH (Nolock) ON JIS.DocTypeId=DT.DocumentTypeId AND JIS.SiteId =H.siteid AND H.DivisionId=JIS.DivisionId
	                LEFT JOIN web.ViewDivisionCompany VDC WITH (Nolock) ON VDC.DivisionId=H.DivisionId
	                LEFT JOIN Web.Sites SI WITH (Nolock) ON SI.SiteId=H.SiteId
	                LEFT JOIN Web.Divisions DIV WITH (Nolock) ON DIV.DivisionId=H.DivisionId	
	                LEFT JOIN Web.Companies Com ON Com.CompanyId = DIV.CompanyId
	                LEFT JOIN Web.Cities CC WITH (Nolock) ON CC.CityId=Com.CityId
	                LEFT JOIN Web.States CS WITH (Nolock) ON CS.StateId=CC.StateId
	                LEFT JOIN Web.Processes PS WITH (Nolock) ON PS.ProcessId=H.ProcessId
	                LEFT JOIN Web.SalesTaxProductCodes PSSTC WITH (Nolock) ON PSSTC.SalesTaxProductCodeId=PS.SalesTaxProductCodeId
	                LEFT JOIN Web.People P WITH (Nolock) ON P.PersonID=H.JobWorkerId
	                LEFT JOIN (SELECT TOP 1 * FROM web.SiteDivisionSettings WHERE @DocDate BETWEEN StartDate AND IsNull(EndDate,getdate()) AND SiteId=@Site AND DivisionId=@Division ORDER BY StartDate) SDS  ON H.DivisionId = SDS.DivisionId AND H.SiteId = SDS.SiteId	
   	                LEFT JOIN (SELECT * FROM Web.PersonAddresses WITH (nolock) WHERE AddressType IS NULL) PA ON PA.PersonId = P.PersonID 
	                LEFT JOIN Web.Cities C WITH (nolock) ON C.CityId = PA.CityId
	                LEFT JOIN Web.States S WITH (Nolock) ON S.StateId=C.StateId
	                LEFT JOIN web.ChargeGroupPersons CGP WITH (Nolock) ON CGP.ChargeGroupPersonId=H.SalesTaxGroupPersonId
	                LEFT JOIN Web.Currencies CUR WITH (Nolock) ON CUR.Id=H.CurrencyId
  	                LEFT JOIN Web.JobInvoiceLines L WITH (Nolock) ON L.JobInvoiceHeaderId=H.JobInvoiceHeaderId
	                LEFT JOIN Web.JobReceiveLines JRL WITH (Nolock) ON JRL.JobReceiveLineId=L.JobReceiveLineId
	                LEFT JOIN web.ProductUids PU WITH (Nolock) ON PU.ProductUidId=JRL.ProductUidId	
	                LEFT JOIN Web.JobReceiveHeaders JRH WITH (Nolock) ON JRH.JobReceiveHeaderId=JRL.JobReceiveHeaderId
	                LEFT JOIN Web.JobOrderLines JOL WITH (Nolock) ON JOL.JobOrderLineId=JRL.JobOrderLineId
   	                LEFT JOIN Web.ProdOrderLines POl WITH (Nolock) ON POl.ProdOrderLineId=JOL.ProdOrderLineId
                    LEFT JOIN Web.ProdOrderHeaders POH WITH (Nolock) ON POH.ProdOrderHeaderId=POL.ProdOrderHeaderId
	                LEFT JOIN web.Products PD WITH (Nolock) ON PD.ProductId=isnull(JOL.ProductId,JRL.ProductId)
	                LEFT JOIN web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId=PD.ProductGroupid
	                LEFT JOIN Web.SalesTaxProductCodes STC WITH (Nolock) ON STC.SalesTaxProductCodeId= IsNull(PD.SalesTaxProductCodeId, Pg.DefaultSalesTaxProductCodeId)
	                LEFT JOIN Web.Dimension1 D1 WITH (Nolock) ON D1.Dimension1Id=JOL.Dimension1Id
	                LEFT JOIN web.Dimension2 D2 WITH (Nolock) ON D2.Dimension2Id=JOL.Dimension2Id
	                LEFT JOIN web.Dimension3 D3 WITH (Nolock) ON D3.Dimension3Id=JOL.Dimension3Id
	                LEFT JOIN Web.Dimension4 D4 WITH (nolock) ON D4.Dimension4Id=JOL.Dimension4Id
	                LEFT JOIN web.Units U WITH (Nolock) ON U.UnitId=PD.UnitId
	                LEFT JOIN web.Units DU WITH (Nolock) ON DU.UnitId=JOL.DealUnitId
	                LEFT JOIN Web.Std_PersonRegistrations SPR WITH (Nolock) ON SPR.CustomerId=H.JobWorkerId
	                LEFT JOIN web.ChargeGroupProducts CGPD WITH (Nolock) ON L.SalesTaxGroupProductId = CGPD.ChargeGroupProductId	
   	                WHERE H.JobInvoiceHeaderId=@Id
   	                ORDER BY L.Sr";

            


            IEnumerable<PrintViewModel> PrintJobinvocie = db.Database.SqlQuery<PrintViewModel>(mQry, SqlParameterHeaderId).ToList();
            IEnumerable<PrintCalculationViewModel> Calculation=PrintCalculation(HeaderId, "Web.JobInvoiceHeaderCharges", "Web.JobInvoiceLineCharges", "Web.JobInvoiceLines");
            IEnumerable<PrintCompanyViewModel> Companydetail = PrintCompany(SiteId, DivisionId);
            IEnumerable<PrintGSTViewModel> GstDetail = PrintGST(HeaderId, "Web.JobInvoiceLines", "Web.JobInvoiceLineCharges", "L.JobInvoiceLineId","Web.JobInvoiceHeaders", "JobInvoiceHeaderId");

            PrintJobinvocie.FirstOrDefault().CalculationHeader = Calculation.ToList();
            PrintJobinvocie.FirstOrDefault().CompanyDetail = Companydetail.ToList();
            PrintJobinvocie.FirstOrDefault().GstDetail = GstDetail.ToList();
                        
            return PrintJobinvocie;

        }

        public IEnumerable<PrintCalculationViewModel>PrintCalculation(string HeaderId, string HeadertableName,string LineChargesTableName,string LineTableName)
        {
            string mQry; 

             SqlParameter SqlParameterHeaderId = new SqlParameter("@Id", !string.IsNullOrEmpty(HeaderId) ? HeaderId : (object)DBNull.Value);
            SqlParameter SqlParameterHeaderTableName = new SqlParameter("@HeaderTableName", !string.IsNullOrEmpty(HeadertableName) ? HeadertableName : (object)DBNull.Value);
            SqlParameter SqlParameterLineChargesTableName = new SqlParameter("@LineChargeTableName", !string.IsNullOrEmpty(LineChargesTableName) ? LineChargesTableName : (object)DBNull.Value);
            SqlParameter SqlParameterLineTableName = new SqlParameter("@LineTableName", !string.IsNullOrEmpty(LineTableName) ? LineTableName : (object)DBNull.Value);

            mQry = @"  DECLARE @StrGrossAmount AS NVARCHAR(50)  
                        DECLARE @StrBasicExciseDuty AS NVARCHAR(50)  
                        DECLARE @StrExciseECess AS NVARCHAR(50)  
                        DECLARE @StrExciseHECess AS NVARCHAR(50)  

                        DECLARE @StrSalesTaxTaxableAmt AS NVARCHAR(50)  
                        DECLARE @StrVAT AS NVARCHAR(50)  
                        DECLARE @StrSAT AS NVARCHAR(50) 
                        DECLARE @StrCST AS NVARCHAR(50) 

                        SET @StrGrossAmount = 'Gross Amount'
                        SET @StrBasicExciseDuty = 'Basic Excise Duty'
                        SET @StrExciseECess ='Excise ECess'
                        SET @StrExciseHECess = 'Excise HECess'

                        SET @StrSalesTaxTaxableAmt = 'Sales Tax Taxable Amt'
                        SET @StrVAT = 'VAT'
                        SET @StrSAT = 'SAT'
                        SET @StrCST = 'CST'

                        DECLARE @Qry NVARCHAR(Max);
                        SET @Qry = '
		                        DECLARE @GrossAmount AS DECIMAL 
		                        DECLARE @BasicExciseDutyAmount AS DECIMAL 
		                        DECLARE @SalesTaxTaxableAmt AS DECIMAL 
		
		                        SELECT @GrossAmount = sum ( CASE WHEN C.ChargeName = ''' + @StrGrossAmount + ''' THEN  H.Amount  ELSE 0 END ) ,
		                        @BasicExciseDutyAmount = sum( CASE WHEN C.ChargeName = ''' + @StrBasicExciseDuty + ''' THEN  H.Amount  ELSE 0 END ) ,
		                        @SalesTaxTaxableAmt = sum( CASE WHEN C.ChargeName = ''' + @StrSalesTaxTaxableAmt + ''' THEN  H.Amount  ELSE 0 END )
		                        FROM ' + @HeaderTableName + ' H
		                        LEFT JOIN web.ChargeTypes CT ON CT.ChargeTypeId = H.ChargeTypeId 
		                        LEFT JOIN web.Charges C ON C.ChargeId = H.ChargeId 
		                        WHERE H.Amount <> 0 AND H.HeaderTableId	= ' + Convert(Varchar,@Id ) + '
		                        GROUP BY H.HeaderTableId
		
		
		                        SELECT H.Id, H.HeaderTableId, H.Sr, C.ChargeName, H.Amount, H.ChargeTypeId,  CT.ChargeTypeName, 
		                        --CASE WHEN C.ChargeName = ''Vat'' THEN ( H.Amount*100/ @GrossAmount ) ELSE H.Rate End  AS Rate,
		                        CASE 
		                        WHEN @SalesTaxTaxableAmt>0 And C.ChargeName IN ( ''' + @StrVAT + ''',''' + @StrSAT + ''',''' + @StrCST+ ''')  THEN ( H.Amount*100/ @SalesTaxTaxableAmt   ) 
		                        WHEN @GrossAmount>0 AND C.ChargeName IN ( ''' + @StrBasicExciseDuty + ''')  THEN ( H.Amount*100/ @GrossAmount   ) 
		                        WHEN  @BasicExciseDutyAmount>0 AND  C.ChargeName IN ( ''' + @StrExciseECess + ''', ''' +@StrExciseHECess+ ''')  THEN ( H.Amount*100/ @BasicExciseDutyAmount   ) 
		                        ELSE 0 End  AS Rate,
		                        ''StdDocPrintSub_CalculationHeader.rdl'' AS ReportName,
		                        ''Transaction Charges'' AS ReportTitle     
		                        FROM  ' + @HeaderTableName + '  H
		                        LEFT JOIN web.ChargeTypes CT ON CT.ChargeTypeId = H.ChargeTypeId 
		                        LEFT JOIN web.Charges C ON C.ChargeId = H.ChargeId 
		                        WHERE  ( isnull(H.ChargeTypeId,0) <> ''4'' OR C.ChargeName = ''Net Amount'') AND H.Amount <> 0
		                        AND H.HeaderTableId	= ' + Convert(Varchar,@Id ) + ''

	                            DECLARE @TmpData TABLE
	                            (
	                            id INT,
	                            HeaderTableId INT,
	                            Sr INT,
	                            ChargeName NVARCHAR(50),
	                            Amount DECIMAL(18,4),
	                            ChargeTypeId INT,
	                            ChargeTypeName NVARCHAR(50),
	                            Rate DECIMAL(38,20),
	                            ReportName nVarchar(255),
	                            ReportTitle nVarchar(255)
	                            );
	
	
	                            Insert Into @TmpData EXEC(@Qry)
	                            SELECT id,HeaderTableId,Sr,ChargeName,Amount,ChargeTypeId,ChargeTypeName,Rate,ReportName ,
	                            'STDDocPrintSub_GSTSummary ' + convert(NVARCHAR,@Id) + ', '+CHAR(39) +@LineTableName+CHAR(39)+', '+CHAR(39) + @LineChargeTableName +CHAR(39) AS SubReportProcList
	                            FROM @TmpData
	                            ORDER BY Sr	";



            IEnumerable<PrintCalculationViewModel> PrintCalculation = db.Database.SqlQuery<PrintCalculationViewModel>(mQry, SqlParameterHeaderId, SqlParameterHeaderTableName, SqlParameterLineChargesTableName, SqlParameterLineTableName).ToList();


            return PrintCalculation;

        }

        public IEnumerable<PrintCompanyViewModel> PrintCompany(int SiteId, int DivisionId)
        {
            string mQry;
            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", !string.IsNullOrEmpty(SiteId.ToString()) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", !string.IsNullOrEmpty(DivisionId.ToString()) ? DivisionId : (object)DBNull.Value);
            
            mQry = @"  SELECT (SELECT SiteCode  FROM Web.Sites WHERE SiteId = @SiteId) SiteName,
                        D.DivisionName, C.CompanyName, C.Address AS CompanyAddress,
                        CT.CityName+','+CNT.CountryName AS CompanyCity,C.Phone AS Phone, C.TinNo AS CompanyTinNo, C.CstNo AS CompanyCSTNo,
                         C.Fax  AS CompanyFaxNo,
                        ST.StateName,  C.LogoFolderName, C.LogoFileName, D.ReportHeaderTextRight1, D.ReportHeaderTextRight2,
                        D.ReportHeaderTextRight3, D.ReportHeaderTextRight4 ,
                        C.PanNo,
                        C.IECNo,
                        D.LogoBlob
                        FROM web.Divisions  D
                        LEFT JOIN Web.Companies C ON C.CompanyId = D.CompanyId
                        LEFT JOIN web.Cities CT ON CT.CityId = C.CityId
                        LEFT JOIN web.States ST ON ST.StateId = CT.StateId 
                        LEFT JOIN web.Countries CNT ON CNT.CountryId = ST.CountryId
                        WHERE D.DivisionId=@DivisionId ";



            IEnumerable<PrintCompanyViewModel> PrintCompanydetail = db.Database.SqlQuery<PrintCompanyViewModel>(mQry, SqlParameterSiteId, SqlParameterDivisionId).ToList();


            return PrintCompanydetail;

        }
        public IEnumerable<PrintGSTViewModel> PrintGST(string HeaderId,string LineTableName,string LineChargesTableName,string LineTableFieldName,string HeaderTableName,string HeaderFieldName)
        {
            string mQry;

            SqlParameter SqlParameterHeaderId = new SqlParameter("@Id", !string.IsNullOrEmpty(HeaderId) ? HeaderId : (object)DBNull.Value);
            SqlParameter SqlParameterLineTableName = new SqlParameter("@LineTableName", !string.IsNullOrEmpty(LineTableName) ? LineTableName : (object)DBNull.Value);
            SqlParameter SqlParameterLineChargesTableName = new SqlParameter("@LineChargeTableName", !string.IsNullOrEmpty(LineChargesTableName) ? LineChargesTableName : (object)DBNull.Value);
            SqlParameter SqlParameterLineTableFieldName = new SqlParameter("@LineTableFieldName", !string.IsNullOrEmpty(LineTableFieldName) ? LineTableFieldName : (object)DBNull.Value);
            SqlParameter SqlParameterHeaderTableName = new SqlParameter("@HeaderTableName", !string.IsNullOrEmpty(HeaderTableName) ? HeaderTableName : (object)DBNull.Value);
            SqlParameter SqlParameterHeaderFieldName = new SqlParameter("@HeaderFieldName", !string.IsNullOrEmpty(HeaderFieldName) ? HeaderFieldName : (object)DBNull.Value);


            mQry = @"  DECLARE @Qry NVARCHAR(Max);
                        SET @Qry = '
                        SELECT  
                        --CASE WHEN PS.ProcessName IN (''Purchase'',''Sale'') THEN isnull(STGP.PrintingDescription,STGP.ChargeGroupProductName) ELSE PS.GSTPrintDesc END as ChargeGroupProductName, 
                        isnull(STGP.PrintingDescription,STGP.ChargeGroupProductName) as ChargeGroupProductName, 
                        Sum(CASE WHEN ct.ChargeTypeName =''Sales Taxable Amount'' THEN lc.Amount ELSE 0 End) AS TaxableAmount,
                        Sum(CASE WHEN ct.ChargeTypeName =''IGST'' THEN lc.Amount ELSE 0 End) AS IGST,
                        Sum(CASE WHEN ct.ChargeTypeName =''CGST'' THEN lc.Amount ELSE 0 End) AS CGST,
                        Sum(CASE WHEN ct.ChargeTypeName =''SGST'' THEN lc.Amount ELSE 0 End) AS SGST,
                        Sum(CASE WHEN ct.ChargeTypeName =''GST Cess'' THEN lc.Amount ELSE 0 End) AS GSTCess,
                        ''StdDocPrintSub_GSTSummary.rdl'' AS ReportName,
                        NULL  AS SubReportProcList
                        FROM '+@LineTableName +' L
                        LEFT JOIN '+@LineChargeTableName+' LC ON '+@LineTableFieldName+' = LC.LineTableId 
                        LEFT JOIN '+@HeaderTableName+' H ON H.'+@HeaderFieldName+' = L.'+@HeaderFieldName+'
                        LEFT JOIN Web.Processes PS WITH (Nolock) ON PS.ProcessId=H.ProcessId
                        LEFT JOIN Web.Charges C ON C.ChargeId=LC.ChargeId
                        LEFT JOIN web.ChargeTypes CT ON LC.ChargeTypeId = CT.ChargeTypeId 
                        LEFT JOIN web.ChargeGroupProducts STGP ON L.SalesTaxGroupProductId = STGP.ChargeGroupProductId
                        WHERE L.'+@HeaderFieldName+' ='+ Convert(Varchar,@Id )+'
                        GROUP BY isnull(STGP.PrintingDescription,STGP.ChargeGroupProductName)
                        --GROUP BY CASE WHEN PS.ProcessName IN (''Purchase'',''Sale'') THEN isnull(STGP.PrintingDescription,STGP.ChargeGroupProductName) ELSE PS.GSTPrintDesc END '

                        EXEC(@Qry); ";



            IEnumerable<PrintGSTViewModel> PrintGSTdetail = db.Database.SqlQuery<PrintGSTViewModel>(mQry, SqlParameterHeaderId, SqlParameterLineTableName, SqlParameterLineChargesTableName, SqlParameterLineTableFieldName, SqlParameterHeaderTableName, SqlParameterHeaderFieldName).ToList();
            
            return PrintGSTdetail;

        }

        public JobInvoiceHeader Add(JobInvoiceHeader pt)
        {
            _unitOfWork.Repository<JobInvoiceHeader>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.JobInvoiceHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobInvoiceHeaderId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobInvoiceHeaderId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.JobInvoiceHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobInvoiceHeaderId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.JobInvoiceHeader
                        orderby p.DocDate descending, p.DocNo descending
                        select p.JobInvoiceHeaderId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<JobInvoiceHeader>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<JobInvoiceHeaderViewModel> GetJobInvoiceHeaderList(int id, string Uname)
        {

            int SiteId=(int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId=(int)System.Web.HttpContext.Current.Session["DivisionId"];

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];


            var TempJobInvoiceHeaderCharges = from H in db.JobInvoiceHeader
                                               join Hc in db.JobInvoiceHeaderCharges on H.JobInvoiceHeaderId equals Hc.HeaderTableId into JobInvoiceHeaderChargesTable
                                               from JobInvoiceHeaderChargesTab in JobInvoiceHeaderChargesTable.DefaultIfEmpty()
                                               join C in db.Charge on JobInvoiceHeaderChargesTab.ChargeId equals C.ChargeId into ChargeTable
                                               from ChargeTab in ChargeTable.DefaultIfEmpty()
                                               where H.SiteId == SiteId && H.DivisionId == DivisionId && H.DocTypeId == id && ChargeTab.ChargeName == "Net Amount"
                                               select new
                                               {
                                                   JobInvoiceHeaderId = H.JobInvoiceHeaderId,
                                                   NetAmount = JobInvoiceHeaderChargesTab.Amount
                                               };


            return (from p in db.JobInvoiceHeader
                    join Hc in TempJobInvoiceHeaderCharges on p.JobInvoiceHeaderId equals Hc.JobInvoiceHeaderId into JobInvoiceHeaderChargesTable
                    from JobInvoiceHeaderChargesTab in JobInvoiceHeaderChargesTable.DefaultIfEmpty()
                    orderby p.DocDate descending, p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == id
                    select new JobInvoiceHeaderViewModel
                    {
                        DocDate=p.DocDate,
                        DocNo=p.DocNo,
                        DocTypeName=p.DocType.DocumentTypeName,
                        JobInvoiceHeaderId=p.JobInvoiceHeaderId,
                        Remark=p.Remark,
                        Status=p.Status,
                        JobWorkerDocNo=p.JobWorkerDocNo,
                        JobWorkerName=p.JobWorker.Name + ", " + p.JobWorker.Suffix + " [" + p.JobWorker.Code + "]",
                        JobWorkerDocDate = p.JobWorkerDocDate,
                        ModifiedBy=p.ModifiedBy,
                        ReviewCount=p.ReviewCount,
                        GodownName = p.JobReceiveHeader.Godown.GodownName,
                        ReviewBy=p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(Uname, p.ReviewBy) > 0),
                        TotalQty = p.JobInvoiceLines.Sum(m => m.Qty),
                        TotalAmount = JobInvoiceHeaderChargesTab.NetAmount ?? (p.JobInvoiceLines.Sum(m => m.Amount)),
                        DecimalPlaces = (from o in p.JobInvoiceLines
                                         join rl in db.JobReceiveLine on o.JobReceiveLineId equals rl.JobReceiveLineId
                                         join ol in db.JobOrderLine on rl.JobOrderLineId equals ol.JobOrderLineId
                                         join prod in db.Product on ol.ProductId equals prod.ProductId
                                         join u in db.Units on prod.UnitId equals u.UnitId
                                         select u.DecimalPlaces).Max(),
                    });
        }


        public IQueryable<JobInvoiceHeaderViewModel> GetJobInvoiceHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobInvoiceHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in JobOrderHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Import || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<JobInvoiceHeaderViewModel> GetJobInvoiceHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var JobOrderHeader = GetJobInvoiceHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in JobOrderHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                   select p;
            return PendingToReview;

        }


        public IQueryable<ComboBoxResult> GetCustomPerson(int Id, string term, int? ProcessId = null)
        {
            int DocTypeId = Id;
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(DocTypeId, DivisionId, SiteId);

            string[] PersonRoles = null;
            if (!string.IsNullOrEmpty(settings.filterPersonRoles)) { PersonRoles = settings.filterPersonRoles.Split(",".ToCharArray()); }
            else { PersonRoles = new string[] { "NA" }; }

            string DivIdStr = "|" + DivisionId.ToString() + "|";
            string SiteIdStr = "|" + SiteId.ToString() + "|";

            var list = (from p in db.Persons
                        join bus in db.BusinessEntity on p.PersonID equals bus.PersonID into BusinessEntityTable
                        from BusinessEntityTab in BusinessEntityTable.DefaultIfEmpty()
                        join pp in db.PersonProcess on p.PersonID equals pp.PersonId into PersonProcessTable
                        from PersonProcessTab in PersonProcessTable.DefaultIfEmpty()
                        join pr in db.PersonRole on p.PersonID equals pr.PersonId into PersonRoleTable
                        from PersonRoleTab in PersonRoleTable.DefaultIfEmpty()
                        //where PersonProcessTab.ProcessId == settings.ProcessId
                        where (ProcessId == null ? PersonProcessTab.ProcessId == settings.ProcessId : PersonProcessTab.ProcessId == ProcessId)
                        && (string.IsNullOrEmpty(term) ? 1 == 1 : (p.Name.ToLower().Contains(term.ToLower()) || p.Code.ToLower().Contains(term.ToLower())))
                        && (string.IsNullOrEmpty(settings.filterPersonRoles) ? 1 == 1 : PersonRoles.Contains(PersonRoleTab.RoleDocTypeId.ToString()))
                        && BusinessEntityTab.DivisionIds.IndexOf(DivIdStr) != -1
                        && BusinessEntityTab.SiteIds.IndexOf(SiteIdStr) != -1
                        && (p.IsActive == null ? 1 == 1 : p.IsActive == true)
                        group new { p } by new { p.PersonID } into Result
                        orderby Result.Max(m => m.p.Name)
                        select new ComboBoxResult
                        {
                            id = Result.Key.PersonID.ToString(),
                            text = Result.Max(m => m.p.Name + ", " + m.p.Suffix + " [" + m.p.Code + "]"),
                        }
              );

            return list;
        }

        public PersonViewModel GetJobWorkerDetail(int id)
        {
            var temp = (from b in db.BusinessEntity
                        where b.PersonID == id
                        select new PersonViewModel
                        {
                            SalesTaxGroupPartyId = b.SalesTaxGroupPartyId,
                            SalesTaxGroupPartyName = b.SalesTaxGroupParty.ChargeGroupPersonName
                        }).FirstOrDefault();

            return (temp);
        }

        public IEnumerable<DocumentTypeHeaderAttributeViewModel> GetDocumentHeaderAttribute(int id)
        {
            var Header = db.JobInvoiceHeader.Find(id);

            var temp = from Dta in db.DocumentTypeHeaderAttribute
                       join Ha in db.JobInvoiceHeaderAttributes on Dta.DocumentTypeHeaderAttributeId equals Ha.DocumentTypeHeaderAttributeId into HeaderAttributeTable
                       from HeaderAttributeTab in HeaderAttributeTable.Where(m => m.HeaderTableId == id).DefaultIfEmpty()
                       where (Dta.DocumentTypeId == Header.DocTypeId)
                       select new DocumentTypeHeaderAttributeViewModel
                       {
                           ListItem = Dta.ListItem,
                           DataType = Dta.DataType,
                           Value = HeaderAttributeTab.Value,
                           Name = Dta.Name,
                           DocumentTypeHeaderAttributeId = Dta.DocumentTypeHeaderAttributeId,
                       };

            return temp;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<JobInvoiceHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobInvoiceHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

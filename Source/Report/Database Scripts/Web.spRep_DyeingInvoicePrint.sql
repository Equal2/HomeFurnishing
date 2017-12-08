USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_DyeingInvoicePrint]    Script Date: 7/7/2015 11:56:54 AM ******/

IF OBJECT_ID ('[Web].[spRep_DyeingInvoicePrint]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_DyeingInvoicePrint]	
GO
/****** Object:  StoredProcedure [Web].[spRep_DyeingInvoicePrint]    Script Date: 7/7/2015 11:56:54 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure  [Web].[spRep_DyeingInvoicePrint](@Id INT)
	As
	BEGIN
	DECLARE @ReportName NVARCHAR (Max)
	DECLARE @TotalAmount DECIMAL 
    SET @TotalAmount = (SELECT Max(Amount) FROM Web.JobInvoiceHeaderCharges WHERE HeaderTableId = @Id AND ChargeId = 34 ) 
		
	DECLARE @DebitNoteAmount DECIMAL 
    SET @DebitNoteAmount = 
    (
				
		SELECT sum(DL.Amount) AS DebitAmount 
		FROM web.JobInvoiceHeaders H WITH (Nolock) 
		LEFT JOIN web.Ledgers L WITH (Nolock)  ON L.LedgerHeaderId = H.LedgerHeaderId
		LEFT JOIN web.LedgerLines DL WITH (Nolock) ON DL.ReferenceId = L.LedgerId 
		LEFT JOIN web.LedgerHeaders LH ON LH.LedgerHeaderId = DL.LedgerHeaderId 
		WHERE H.JobInvoiceHeaderId = @Id
		AND LH.DocTypeId = 559

	) 
     	
     	
     	IF @DebitNoteAmount > 0 
		  SET @ReportName = 'DyeingInvoicePrint_WithDebitNote' 	  
		ELSE
     	  SET @ReportName = 'DyeingInvoicePrint' 	  
     	        	
     	
     	
     	
		SELECT H.jobInvoiceHeaderId,P.Name,PA.Address,City.CityName AS CityName, P.Mobile AS MobileNo
		,(SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =L.JobWorkerId   AND RegistrationType ='Tin No' ) AS TinNo
		  ,DT.DocumentTypeShortName +'-'+ H.DocNo AS InvoiceNo,replace(convert(VARCHAR, H.DocDate, 106), ' ', '/')  AS InvoicedateDate,
		 H.JobWorkerDocNo AS DyersDocNo,replace(convert(VARCHAR, H.JobWorkerDocDate, 106), ' ', '/') AS DyersDocDate,isnull(H.CreditDays,0) AS CreditDayes,
		 DTJR.DocumentTypeShortName+'-'+JRH.DocNo AS RectNo,replace(convert(VARCHAR, JRH.DocDate, 106), ' ', '/') AS RectDate,DTJO.DocumentTypeShortName+'-'+JOH.DocNo AS OrderNo,
		 PD.ProductName,D2.Dimension2Name,D1.Dimension1Name, 'spRep_TransactionCharges '  + convert(NVARCHAR,@Id) + ', "web.JobInvoiceHeaderCharges" ' AS SubReportProcList,		 
		convert(DECIMAL(18,4),isnull(JRL.Qty,0))+convert(DECIMAL(18,4),isnull(JRL.LossQty,0)) AS QtyDyed,JRL.Qty,JRL.LossQty,JRL.PassQty,L.Rate,L.Amount,
		@TotalAmount as NetAmount, @DebitNoteAmount AS DebitNoteAmount, isnull(@TotalAmount,0) -  isnull(@DebitNoteAmount,0) NetPayableAmount, 
		U.UnitName AS Unit, isnull(U.DecimalPlaces,0) AS DecimalPlaces ,H.Siteid,H.DivisionId,'Dyeing Invoice' AS ReportTitle,@ReportName +'.rdl' AS ReportName,
		 'Web.JobInvoiceHeaderCharges' AS ChargesTableName,WC.CompanyName AS CompanyName,H.Remark AS HeaderRemark,		 
		 DN.DocDate AS DebitNoteDate, DN.DocNo AS DebitNotNo, DN.Narration AS DebitNoteReason
		 FROM  
		(SELECT * FROM web._JobInvoiceHeaders WITH (nolock) WHERE jobInvoiceHeaderId=  @Id) H 
		LEFT JOIN web._JobInvoiceLines L WITH (nolock) ON L.JobInvoiceHeaderId=H.jobInvoiceHeaderId
		LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
		LEFT JOIN web.JobWorkers JW WITH (nolock) ON JW.PersonId=L.JobWorkerId
		LEFT JOIN Web.People P WITH (nolock) ON P.PersonID = JW.PersonId
		LEFT JOIN Web.PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
		LEFT JOIN Web.Cities City WITH (nolock) ON City.CityId = PA.CityId
		LEFT JOIN Web._JobReceiveLines JRL ON JRL.JobReceiveLineId=L.JobReceiveLineId
		LEFT JOIN Web._JobReceiveHeaders JRH ON JRL.JobReceiveHeaderId=JRH.JobReceiveHeaderId
		LEFT JOIN Web.DocumentTypes DTJR WITH (nolock) ON DTJR.DocumentTypeId=JRH.DocTypeId 
		LEFT JOIN Web._JobOrderLines JOL WITH (nolock) ON  JOL.JoborderLineId=JRL.JobOrderLineId  
		LEFT JOIN Web._JobOrderHeaders JOH WITH (nolock) ON JOL.JobOrderHeaderId=JOH.JobOrderHeaderId
		LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = JOL.ProductId 
		LEFT JOIN web.Dimension1 D1  WITH (nolock) ON D1.Dimension1Id=JOL.Dimension1Id
		LEFT JOIN web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id=JOL.Dimension2Id 
		LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = PD.UnitId 
		LEFT JOIN Web.DocumentTypes DTJO WITH (nolock) ON DTJO.DocumentTypeId=JOH.DocTypeId 
	   	LEFT JOIN Web.Divisions WD WITH (nolock) ON WD.DivisionId=H.DivisionId
		LEFT JOIN web.Companies WC WITH (nolock) ON WC.CompanyId=WD.CompanyId
		LEFT JOIN  
		(		
			SELECT L.LedgerHeaderId, LH.DocDate, LH.DocNo, LH.Narration,  LH.Remark, DL.Remark AS LineRemark,  DL.Amount AS DebitAmount		
			FROM ( SELECT * FROM web.JobInvoiceHeaders H WITH (Nolock) WHERE H.JobInvoiceHeaderId = @Id )   H
			LEFT JOIN web.Ledgers L WITH (Nolock)  ON L.LedgerHeaderId = H.LedgerHeaderId
			LEFT JOIN web.LedgerLines DL WITH (Nolock) ON DL.ReferenceId = L.LedgerId 
			LEFT JOIN web.LedgerHeaders LH ON LH.LedgerHeaderId = DL.LedgerHeaderId 		
			WHERE  LH.DocTypeId = 559
		)  DN ON DN.LedgerHeaderId=H.LedgerHeaderId
		WHERE H.jobInvoiceHeaderId	= @Id
		ORDER BY L.JobInvoiceLineId
	End
GO



USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_JobGoodsReturnPrint]    Script Date: 24/Jul/2015 11:42:45 AM ******/

IF OBJECT_ID ('[Web].[spRep_JobGoodsReturnPrint]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_JobGoodsReturnPrint]  
GO
/****** Object:  StoredProcedure [Web].[spRep_JobGoodsReturnPrint]    Script Date: 24/Jul/2015 11:42:45 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE Procedure [Web].[spRep_JobGoodsReturnPrint](@Id INT)
	as
	begin
	
	DECLARE @Dimension1IdCnt INT 
    DECLARE @Dimension2IdCnt INT


   SELECT @Dimension1IdCnt =  Count(*) FROM Web._JobReturnLines L
   LEFT JOIN web.jobreceivelines JRL WITH (nolock) ON JRL.JobReceiveLineId=L.JobReceiveLineId
   LEFT JOIN Web.joborderlines JOL WITH (nolock) ON JOL.JobOrderLineId=JRL.JobOrderLineId
   WHERE JOL.Dimension1Id IS NOT NULL AND L.JobReturnHeaderId= @Id

   
   SELECT @Dimension2IdCnt =  Count(*) FROM Web._JobReturnLines L
   LEFT JOIN web.jobreceivelines JRL WITH (nolock) ON JRL.JobReceiveLineId=L.JobReceiveLineId
   LEFT JOIN Web.joborderlines JOL WITH (nolock) ON JOL.JobOrderLineId=JRL.JobOrderLineId
   WHERE JOL.Dimension2Id IS NOT NULL AND L.JobReturnHeaderId= @Id
   

	select H.JobReturnHeaderId, DT.DocumentTypeName AS ReportTitle, DT.DocumentTypeShortName, DT.DocumentTypeShortName +'-'+ H.DocNo AS DocNo,H.DocDate,H.Remark AS HeaderRemark,
	DTJRH.DocumentTypeShortName +'-'+ JRH.DocNo AS ChallanNo1,JRH.DocNo AS ChallanNo,P.Name,PA.Address,P.Mobile,City.CityName,SH.GatePassHeaderId,
	( SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =H.JobWorkerId  AND RegistrationType ='TIN No' ) AS TinNo, 
	R.ReasonName,L.JobReturnLineId, PD.ProductName,L.Qty AS Qty, L.Remark AS Remark,H.SiteId,H.DivisionId,U.UnitName AS Unit, isnull(U.DecimalPlaces,0) AS DecimalPlaces,
	G.GodownName,JOL.Specification, NULL AS SubReportProcList ,
	 CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
     CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name,
	 CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'JobGoodsReturnPrint_DoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'JobGoodsReturnPrint_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'JobGoodsReturnPrint_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'JobGoodsReturnPrint.rdl'
	END AS ReportName
	from( SELECT * FROM Web._JobReturnHeaders WITH (nolock) WHERE JobReturnHeaderId	= @Id ) H
	LEFT JOIN Web._JobReturnLines L WITH (nolock) ON L.JobReturnHeaderId = H.JobReturnHeaderId 
	LEFT JOIN [Web].DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
	LEFT JOIN web.jobreceivelines JRL WITH (nolock) ON JRL.JobReceiveLineId=L.JobReceiveLineId
	LEFT JOIN Web.JobReceiveHeaders JRH WITH (nolock) ON JRH.JobReceiveHeaderId=JRL.JobReceiveHeaderId
	LEFT JOIN [Web].DocumentTypes DTJRH WITH (nolock) ON DTJRH.DocumentTypeId = JRH.DocTypeId 
	LEFT JOIN Web.People P WITH (nolock) ON P.PersonId=H.JobWorkerId
	LEFT JOIN [Web].PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
	LEFT JOIN [Web].Cities City WITH (nolock) ON City.CityId = PA.CityId 
	LEFT JOIN Web.joborderlines JOL WITH (nolock) ON JOL.JobOrderLineId=JRL.JobOrderLineId
	LEFT JOIN [Web].Products PD WITH (nolock) ON PD.ProductId = JOL.ProductId 
	LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = PD.UnitId 
	LEFT JOIN Web.Reasons R WITH (Nolock) ON R.ReasonId=H.ReasonId
	LEFT JOIN Web.Godowns G WITH (Nolock) ON G.GodownId=H.GodownId
	LEFT JOIN  Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = JOL.Dimension1Id  
   LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = JOL.Dimension2Id
   LEFT Join web.StockHeaders SH WITH (nolock) ON SH.StockHeaderId=H.StockHeaderId
   WHERE H.JobReturnHeaderId	= @Id 
ORDER BY L.JobReturnLineId
	END

GO



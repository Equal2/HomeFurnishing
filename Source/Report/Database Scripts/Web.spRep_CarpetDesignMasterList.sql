USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_CarpetDesignMasterList]    Script Date: 7/7/2015 11:49:20 AM ******/


IF OBJECT_ID ('[Web].[spRep_CarpetDesignMasterList]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_CarpetDesignMasterList]	
GO

/****** Object:  StoredProcedure [Web].[spRep_CarpetDesignMasterList]    Script Date: 7/7/2015 11:49:20 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [Web].[spRep_CarpetDesignMasterList] 
@DivisionId VARCHAR(Max) = NULL,
@ProductdesignId VARCHAR(Max) = NULL,
@colourId VARCHAR(Max) = NULL,
@Productcollectionid VARCHAR(Max) = NULL,
@ProductqualityId VARCHAR(Max) = NULL,
@ProductCategoryId VARCHAR(Max) = NULL,
@IsSample NVARCHAR(3) = NULL,
@IsActive NVARCHAR(3)=NULL
AS	
BEGIN

SELECT P.ProductName, WPD.ProductdesignName,WC.ColourName,WP.ProductCollectionName,WPQ.ProductQualityName
,WPC.ProductCategoryName,P.DivisionId,
'MasterCarpetDesignlistReport.rdl' AS ReportName,
NULL AS SubReportProcList,
NULL AS SiteId ,
'Carpet Design Master List' AS ReportTitle 
 FROM web.FinishedProduct WFP
LEFT JOIN web.ProductDesigns WPD ON WFP.ProductDesignId=WPD.ProductDesignId
LEFT JOIN Web.Colours WC ON WFP.colourId=WC.ColourId
LEFT JOIN web.ProductCollections WP ON WFP.ProductCollectionId=WP.ProductCollectionId
LEFT JOIN web.ProductQualities WPQ ON WFP.ProductqualityId=WPQ.productqualityid
LEFT JOIN Web.ProductCategories WPC ON WFP.ProductCategoryId=WPC.ProductCategoryId
LEFT JOIN web.Products P ON WFP.ProductId=p.ProductId
LEFT JOIN Web.Divisions D ON P.DivisionId=D.DivisionId 
WHERE 1=1 AND WPC.ProductTypeId='1'--for the rug
AND ( @ProductdesignId is null or WFP.ProductDesignId IN (SELECT Items FROM [dbo].[Split] (@ProductdesignId, ',')))
AND ( @colourId is null or WFP.colourId IN (SELECT Items FROM [dbo].[Split] (@colourId, ',')))
AND ( @Productcollectionid is null or WFP.ProductCollectionId IN (SELECT Items FROM [dbo].[Split] (@Productcollectionid, ',')))
AND ( @ProductqualityId is null or WFP.ProductqualityId IN (SELECT Items FROM [dbo].[Split] (@ProductqualityId, ',')))
AND ( @ProductCategoryId is null or WFP.ProductCategoryId IN (SELECT Items FROM [dbo].[Split] (@ProductCategoryId, ',')))
AND ( @DivisionId is null or P.DivisionId IN (SELECT Items FROM [dbo].[Split] (@DivisionId, ',')))
AND ( @IsSample IS NULL OR CASE WHEN WFP.IsSample = 0 THEN 'No' ELSE 'Yes' END = @IsSample)
AND ( @IsActive IS NULL OR CASE WHEN WPD.IsActive = 0 THEN 'No' ELSE 'Yes' END = @IsActive)
End
GO



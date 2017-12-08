IF OBJECT_ID ('[Web].[Mig_FinishedProduct_Add]') IS NOT NULL
	DROP PROCEDURE [Web].[Mig_FinishedProduct_Add]	
GO


Create PROCEDURE [Web].[Mig_FinishedProduct_Add]
	@Id INT 
AS 
BEGIN 
UPDATE RUG_Design 
SET Construction = Pc.ProductCategoryName , 
	Carpet_Collection = PCol.ProductCollectionName, 
	Carpet_Style = PSt.ProductStyleName, 
	Carpet_Colour = Col.ColourName , 
	QualityCode = Q.Code, 
	PileWeight = Pq.Weight, 
 	ProcessSequence = Psq.Code, 
 	Contents = PCon.ProductContentName,
 	Div_Code = D.Div_Code,
 	ItemGroups = CASE WHEN CharIndex(PCon.ProductContentName,'Wool')>0 THEN '|Woolen Yarn|,|Silk|,|Polyster Yarn|'
 					  WHEN CharIndex(PCon.ProductContentName,'Jute')>0 THEN '|Jute Yarn|'
 					  WHEN CharIndex(PCon.ProductContentName,'Polyester')>0 THEN '|Woolen Yarn|,|Silk|,|Polyster Yarn|'
 					  WHEN CharIndex(PCon.ProductContentName,'Viscose')>0  THEN '|Woolen Yarn|,|Silk|,|Polyster Yarn|'
 					  WHEN CharIndex(PCon.ProductContentName,'Leather')>0  THEN '|Other|'
 					  WHEN CharIndex(PCon.ProductContentName,'Acrylic')>0  THEN '|Other|'
 					  WHEN CharIndex(PCon.ProductContentName,'Cotton')>0  THEN '|Cotton Yarn|,|Cotton Cone|'
 					  WHEN CharIndex(PCon.ProductContentName,'Jute + Leather')>0  THEN '|Other|'
 					  WHEN CharIndex(PCon.ProductContentName,'Polyster + Silk')>0  THEN '|Other|' 					  
 					  WHEN CharIndex(PCon.ProductContentName,'50% Jute & 50% Wool')>0  THEN '|Jute Yarn|,|Woolen Yarn|'
 					  WHEN CharIndex(PCon.ProductContentName,'100 Percent Pet')>0  THEN '|Polyster Yarn|'
 					  ELSE '|Woolen Yarn|,|Silk|,|Polyster Yarn|'
 					  END 
FROM Web.Products H 
LEFT JOIN Web.FinishedProduct Fp ON H.ProductId = Fp.ProductId
LEFT JOIN Web.ProductCategories Pc ON Fp.ProductCategoryId = Pc.ProductCategoryId
LEFT JOIN Web.ProductCollections PCol ON Fp.ProductCollectionId = PCol.ProductCollectionId
LEFT JOIN Web.ProductStyles PSt ON Fp.ProductStyleId = Pst.ProductStyleId
LEFT JOIN Web.Colours Col ON Fp.ColourId = Col.ColourId
LEFT JOIN Web.ProductContentHeaders PCon ON Fp.FaceContentId = PCon.ProductContentHeaderId
LEFT JOIN Web.ProductQualities Pq ON Fp.ProductQualityId = Pq.ProductQualityId
LEFT JOIN dbo.RUG_Quality Q On  'Product Quality-' + Convert(Varchar,Fp.ProductQualityId) = Q.OMSId 
LEFT JOIN dbo.Division D On  'Division-' + Convert(Varchar,H.DivisionId) = D.OMSId 
LEFT JOIN dbo.ProcessSequence Psq On  'Process Sequence-' + Convert(Varchar,Fp.ProcessSequenceHeaderId) = Psq.OMSId 
WHERE RUG_Design.OMSId = 'Product Group-' + Convert(VARCHAR,H.ProductGroupId)
AND H.ProductId = @Id


UPDATE ItemGroup
SET ItemGroup.ItemCategory = Q.Code, 
ItemGroup.Div_Code = D.Div_Code
FROM Web.Products H 
LEFT JOIN Web.FinishedProduct Fp ON H.ProductId = Fp.ProductId
LEFT JOIN dbo.RUG_Quality Q On  'Product Quality-' + Convert(Varchar,Fp.ProductQualityId) = Q.OMSId 
LEFT JOIN dbo.Division D On  'Division-' + Convert(Varchar,H.DivisionId) = D.OMSId 
WHERE ItemGroup.OMSId = 'Product Group-' + Convert(VARCHAR,H.ProductGroupId)
AND H.ProductId = @Id



UPDATE Item
SET Item.ItemCategory = Q.Code,
Item.ProcessSequence = Psq.Code,
Item.ItemInvoiceGroup = Iig.Code,
Item.IsSample  = FP.IsSample 
FROM Web.Products H 
LEFT JOIN Web.FinishedProduct Fp ON H.ProductId = Fp.ProductId
LEFT JOIN dbo.RUG_Quality Q On  'Product Quality-' + Convert(Varchar,Fp.ProductQualityId) = Q.OMSId 
LEFT JOIN dbo.ProcessSequence Psq On  'Process Sequence-' + Convert(Varchar,Fp.ProcessSequenceHeaderId) = Psq.OMSId 
LEFT JOIN dbo.ItemInvoiceGroup Iig On  'Product Invoice Group-' + Convert(Varchar,Fp.ProductInvoiceGroupId) = Iig.OMSId 
WHERE Item.OMSId = 'Product-' + Convert(VARCHAR,H.ProductId)
AND H.ProductId = @Id

End
GO
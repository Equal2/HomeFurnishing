IF object_id ('[Web].[ProcCompanyDetail]') IS NOT NULL 
DROP Procedure  [Web].[ProcCompanyDetail]
GO 

CREATE Procedure  [Web].[ProcCompanyDetail]
(
@SiteId INT, 
@DivisionId INT = NULL  
)
AS 
BEGIN

IF ( @DivisionId IS NOT NULL )
BEGIN
	SET @DivisionId = @DivisionId	
	SELECT  (SELECT SiteCode  FROM Web.Sites WHERE SiteId = @SiteId) SiteName, 
	VDC.DivisionName, VDC.CompanyName, VDC.CompanyAddress, VDC.CompanyCity,
	VDC.CompanyPhoneNo, VDC.CompanyTinNo, VDC.CompanyCSTNo, VDC.CompanyFaxNo 
	FROM web.ViewDivisionCompany VDC
	WHERE VDC.DivisionId  = @DivisionId
END
ELSE
BEGIN 
	SELECT @DivisionId = DefaultDivisionId   FROM Web.Sites WHERE SiteId = @SiteId
	
	SELECT  (SELECT SiteCode  FROM Web.Sites WHERE SiteId = @SiteId) SiteName, 
	'Consolidated' AS DivisionName, VDC.CompanyName, VDC.CompanyAddress, VDC.CompanyCity,
	VDC.CompanyPhoneNo, VDC.CompanyTinNo, VDC.CompanyCSTNo, VDC.CompanyFaxNo 
	FROM web.ViewDivisionCompany VDC
	WHERE VDC.DivisionId  = @DivisionId

END 
End
GO

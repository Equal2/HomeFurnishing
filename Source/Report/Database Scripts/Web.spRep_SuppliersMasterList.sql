USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_SuppliersMasterList]    Script Date: 6/17/2015 6:05:06 PM ******/

IF OBJECT_ID ('[Web].[spRep_SuppliersMasterList]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_SuppliersMasterList]	
GO
/****** Object:  StoredProcedure [Web].[spRep_SuppliersMasterList]    Script Date: 6/17/2015 6:05:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [Web].[spRep_SuppliersMasterList] 
@City VARCHAR(Max) = NULL,
@State VARCHAR(Max) = NULL,
@Country VARCHAR(Max) = NULL
AS
Begin
SELECT WP.Name,WPA.Address+' '+WC.CityName+' '+WPA.Zipcode+','+' '+ S.StateName+','+WCOUNTRIES.CountryName AS Address, WP.Mobile,WP.Email,
'MasterSupplierListReport.rdl' AS ReportName,NULL AS SubReportProcList,NULL AS SiteId,'Supplier Master List' AS ReportTitle,
NULL AS DivisionId FROM Web.Suppliers WS
 LEFT JOIN web.People WP ON WS.PersonID=WP.PersonID
 LEFT JOIN web.PersonAddresses WPA ON WPA.PersonId=WP.PersonID
 LEFT JOIN Web.Cities WC ON WPA.CityId=WC.CityId
 LEFT JOIN Web.states S ON WC.StateId=S.StateId
 LEFT JOIN Web.Countries WCOUNTRIES ON S.CountryId=WCOUNTRIES.CountryId
 WHERE 1=1 
AND ( @City is null or WPA.CityId IN (SELECT Items FROM [dbo].[Split] (@City, ',')))
AND ( @State is null or WC.StateId IN (SELECT Items FROM [dbo].[Split] (@State, ',')))
AND ( @Country is null or S.CountryId IN (SELECT Items FROM [dbo].[Split] (@Country, ',')))
End
GO



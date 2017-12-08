USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_JobWorkerMasterList]    Script Date: 7/7/2015 11:47:30 AM ******/



IF OBJECT_ID ('[Web].[spRep_JobWorkerMasterList]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_JobWorkerMasterList]	
GO


/****** Object:  StoredProcedure [Web].[spRep_JobWorkerMasterList]    Script Date: 7/7/2015 11:47:30 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [Web].[spRep_JobWorkerMasterList] 
@City VARCHAR(Max) = NULL,
@State VARCHAR(Max) = NULL,
@Country VARCHAR(Max) = NULL,
@processId VARCHAR(10)=NULL
AS
BEGIN

SELECT WP.Name,WPA.Address+' '+WC.CityName+' '+WPA.Zipcode+','+' '+ S.StateName+','+WCOUNTRIES.CountryName AS Address, WP.Mobile,WP.Email,
'MasterJobWorkerlist.rdl' AS ReportName,NULL AS SubReportProcList,NULL AS SiteId,'Job Worker Master List' AS ReportTitle,
NULL AS DivisionId FROM Web.JobWorkers WJK
 LEFT JOIN web.People WP ON WJK.PersonID=WP.PersonID
 LEFT JOIN Web.PersonProcesses WPP ON WJK.PersonID=WPP.PersonId
 LEFT JOIN web.PersonAddresses WPA ON WPA.PersonId=WP.PersonID
 LEFT JOIN Web.Cities WC ON WPA.CityId=WC.CityId
 LEFT JOIN Web.states S ON WC.StateId=S.StateId
 LEFT JOIN Web.Countries WCOUNTRIES ON S.CountryId=WCOUNTRIES.CountryId
 WHERE 1=1 
AND ( @City is null or WPA.CityId IN (SELECT Items FROM [dbo].[Split] (@City, ',')))
AND ( @State is null or WC.StateId IN (SELECT Items FROM [dbo].[Split] (@State, ',')))
AND ( @Country is null or S.CountryId IN (SELECT Items FROM [dbo].[Split] (@Country, ',')))
AND WPP.ProcessId=@processId
End
GO



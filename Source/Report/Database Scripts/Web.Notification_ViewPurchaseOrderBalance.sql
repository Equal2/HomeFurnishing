USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[Notification_ViewPurchaseOrderBalance]    Script Date: 04/Aug/2015 11:55:50 AM ******/
IF OBJECT_ID ('[Web].[Notification_ViewPurchaseOrderBalance]') IS NOT NULL
	DROP PROCEDURE [Web].[Notification_ViewPurchaseOrderBalance]	
GO

/****** Object:  StoredProcedure [Web].[Notification_ViewPurchaseOrderBalance]    Script Date: 04/Aug/2015 11:55:50 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [Web].[Notification_ViewPurchaseOrderBalance]
as
begin 
DECLARE @SubjectId INT
  DECLARE @NotificationText NVARCHAR(Max)
  DECLARE @NotificationURL NVARCHAR(Max)=Null
  DECLARE @ExpiryDate SMALLDATETIME=Null 
  DECLARE @ForUsers NVARCHAR(Max) 
  DECLARE @CreatedBy NVARCHAR(Max)  
  DECLARE @Cnt INT=0
  DECLARE @NotiMsg NVARCHAR(Max)
  declare @Days int
SET @Days='-2'
SET @SubjectId='1'

DECLARE curP CURSOR FOR 

SELECT   
(SELECT U.UserName + ','  FROM Web.AspNetUserRoles UR LEFT JOIN web.Users U ON U.Id = UR.UserId  WHERE UR.RoleId 
=(SELECT id FROM Web.AspNetRoles WHERE Name =
     CASE WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='TUFTED' THEN 'Purchase Manager (Finished Tufted)'
	 	 WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='KELIM' THEN 'Purchase Manager (Finished Kelim)'
	 	 WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='KNOTTED' THEN 'Purchase Manager (Finished Knotted)'
	 	 WHEN max(PN.ProductNatureName) ='Raw Material'      THEN 'Purchase Manager (Raw)'
	 	 WHEN max(PN.ProductNatureName) ='Other Material'    THEN 'Purchase Manager (Other)'
	 END ) FOR XML path('') ) AS USERS,
	 /*max(PN.ProductNatureName) AS ProductNatureName,max(D.DivisionName) AS DivisionName,*/
	count(DISTINCT PurchaseOrderNo),
(SELECT DISTINCT POB1.PurchaseOrderNo +','
FROM Web._ViewPurchaseOrderBalance POB1
LEFT JOIN Web.Products P1 ON POB1.ProductId=P1.ProductId
LEFT JOIN web.ProductGroups PG1 ON PG1.ProductGroupId=P1.ProductGroupId
LEFT JOIN Web.Producttypes PT1 ON PG1.ProductTypeId=PT1.ProductTypeId
WHERE DATEDIFF(day,duedate,getdate())=@Days
AND PT.ProductNatureId=PT1.ProductNatureId AND POB.DivisionId =POB1.DivisionId 
AND POB.SiteId =POB1.SiteId 
FOR XML PATH('')
) 
FROM Web._ViewPurchaseOrderBalance POB
LEFT JOIN Web.Products P ON POB.ProductId=P.ProductId
LEFT JOIN web.ProductGroups PG ON PG.ProductGroupId=P.ProductGroupId
LEFT JOIN Web.Producttypes PT ON PG.ProductTypeId=PT.ProductTypeId
LEFT JOIN Web.ProductNatures PN ON PN.ProductNatureId=PT.ProductNatureId
LEFT JOIN Web.divisions D ON D.DivisionId=POB.DivisionId
WHERE DATEDIFF(day,duedate,getdate())=@Days
GROUP BY PT.ProductNatureId, POB.DivisionId,POB.SiteId 

OPEN curP
Fetch Next From curP Into @ForUsers, @Cnt,@NotiMsg
While @@Fetch_Status = 0 Begin
SET @NotificationText=convert(VARCHAR,@Cnt)+'  '+'Purchase Order is due in next'+convert(VARCHAR,@Days) +' '+'  Days' +' '+'['+ LEFT(@NotiMsg,len(@NotiMsg)-1) +']';
if(@Cnt>0 and @ForUsers is not null)
begin
EXEC Logindb.dbo.CreateNotification @SubjectId , @NotificationText ,@NotificationURL,@ExpiryDate,@ForUsers,@CreatedBy
end
Fetch Next From curP Into @ForUsers, @Cnt,@NotiMsg
End
Close curP
Deallocate curP
             /* for the 1 to 7 days*/

set @Cnt=0
DECLARE curP1 CURSOR FOR 
SELECT   
(SELECT U.UserName + ','  FROM Web.AspNetUserRoles UR LEFT JOIN web.Users U ON U.Id = UR.UserId  WHERE UR.RoleId 
=(SELECT id FROM Web.AspNetRoles WHERE Name =
     CASE WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='TUFTED' THEN 'Purchase Manager (Finished Tufted)'
	 	 WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='KELIM' THEN 'Purchase Manager (Finished Kelim)'
	 	 WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='KNOTTED' THEN 'Purchase Manager (Finished Knotted)'
	 	 WHEN max(PN.ProductNatureName) ='Raw Material'      THEN 'Purchase Manager (Raw)'
	 	 WHEN max(PN.ProductNatureName) ='Other Material'    THEN 'Purchase Manager (Other)'
	 END ) FOR XML path('') ) AS USERS,
	 /*max(PN.ProductNatureName) AS ProductNatureName,max(D.DivisionName) AS DivisionName,*/
	count(DISTINCT PurchaseOrderNo),
(SELECT DISTINCT POB1.PurchaseOrderNo +','
FROM Web._ViewPurchaseOrderBalance POB1
LEFT JOIN Web.Products P1 ON POB1.ProductId=P1.ProductId
LEFT JOIN web.ProductGroups PG1 ON PG1.ProductGroupId=P1.ProductGroupId
LEFT JOIN Web.Producttypes PT1 ON PG1.ProductTypeId=PT1.ProductTypeId
WHERE DATEDIFF(day,duedate,getdate()) <=7 AND DATEDIFF(day,duedate,getdate())>=1
AND PT.ProductNatureId=PT1.ProductNatureId AND POB.DivisionId =POB1.DivisionId 
AND POB.SiteId =POB1.SiteId 
FOR XML PATH('')
) 
FROM Web._ViewPurchaseOrderBalance POB
LEFT JOIN Web.Products P ON POB.ProductId=P.ProductId
LEFT JOIN web.ProductGroups PG ON PG.ProductGroupId=P.ProductGroupId
LEFT JOIN Web.Producttypes PT ON PG.ProductTypeId=PT.ProductTypeId
LEFT JOIN Web.ProductNatures PN ON PN.ProductNatureId=PT.ProductNatureId
LEFT JOIN Web.divisions D ON D.DivisionId=POB.DivisionId
WHERE DATEDIFF(day,duedate,getdate()) <=7 AND DATEDIFF(day,duedate,getdate())>=1
GROUP BY PT.ProductNatureId, POB.DivisionId,POB.SiteId 

OPEN curP1
Fetch Next From curP1 Into @ForUsers, @Cnt,@NotiMsg
While @@Fetch_Status = 0 Begin
SET @NotificationText=convert(VARCHAR,@Cnt)+'  '+'Purchase Order is  1 week Over due'+' '+'['+ LEFT(@NotiMsg,len(@NotiMsg)-1) +']';
if(@Cnt>0 and @ForUsers is not null)
begin
EXEC Logindb.dbo.CreateNotification @SubjectId , @NotificationText ,@NotificationURL,@ExpiryDate,@ForUsers,@CreatedBy
end
Fetch Next From curP1 Into @ForUsers, @Cnt,@NotiMsg
End
Close curP1
Deallocate curP1
/* for the 8 to 14 week*/

set @Cnt=0
set @ForUsers=null
DECLARE curP2 CURSOR FOR 
SELECT   
(SELECT U.UserName + ','  FROM Web.AspNetUserRoles UR LEFT JOIN web.Users U ON U.Id = UR.UserId  WHERE UR.RoleId 
=(SELECT id FROM Web.AspNetRoles WHERE Name =
     CASE WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='TUFTED' THEN 'Purchase Manager (Finished Tufted)'
	 	 WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='KELIM' THEN 'Purchase Manager (Finished Kelim)'
	 	 WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='KNOTTED' THEN 'Purchase Manager (Finished Knotted)'
	 	 WHEN max(PN.ProductNatureName) ='Raw Material'      THEN 'Purchase Manager (Raw)'
	 	 WHEN max(PN.ProductNatureName) ='Other Material'    THEN 'Purchase Manager (Other)'
	 END ) FOR XML path('') ) AS USERS,
	 /*max(PN.ProductNatureName) AS ProductNatureName,max(D.DivisionName) AS DivisionName,*/
	count(DISTINCT PurchaseOrderNo),
(SELECT DISTINCT POB1.PurchaseOrderNo +','
FROM Web._ViewPurchaseOrderBalance POB1
LEFT JOIN Web.Products P1 ON POB1.ProductId=P1.ProductId
LEFT JOIN web.ProductGroups PG1 ON PG1.ProductGroupId=P1.ProductGroupId
LEFT JOIN Web.Producttypes PT1 ON PG1.ProductTypeId=PT1.ProductTypeId
WHERE DATEDIFF(day,duedate,getdate()) <=14 AND DATEDIFF(day,duedate,getdate())>=8
AND PT.ProductNatureId=PT1.ProductNatureId AND POB.DivisionId =POB1.DivisionId 
AND POB.SiteId =POB1.SiteId 
FOR XML PATH('')
) 
FROM Web._ViewPurchaseOrderBalance POB
LEFT JOIN Web.Products P ON POB.ProductId=P.ProductId
LEFT JOIN web.ProductGroups PG ON PG.ProductGroupId=P.ProductGroupId
LEFT JOIN Web.Producttypes PT ON PG.ProductTypeId=PT.ProductTypeId
LEFT JOIN Web.ProductNatures PN ON PN.ProductNatureId=PT.ProductNatureId
LEFT JOIN Web.divisions D ON D.DivisionId=POB.DivisionId
WHERE DATEDIFF(day,duedate,getdate()) <=14 AND DATEDIFF(day,duedate,getdate())>=8
GROUP BY PT.ProductNatureId, POB.DivisionId,POB.SiteId 

OPEN curP2
Fetch Next From curP2 Into @ForUsers, @Cnt,@NotiMsg
While @@Fetch_Status = 0 Begin
SET @NotificationText=convert(VARCHAR,@Cnt)+'  '+'Purchase Order is  2 week Over due'+' '+'['+ LEFT(@NotiMsg,len(@NotiMsg)-1) +']';
if(@Cnt>0 and @ForUsers is not null)
begin
EXEC Logindb.dbo.CreateNotification @SubjectId , @NotificationText ,@NotificationURL,@ExpiryDate,@ForUsers,@CreatedBy
end
Fetch Next From curP2 Into @ForUsers, @Cnt,@NotiMsg
End
Close curP2
Deallocate curP2
/* for the 15 to 21  3week*/

set @Cnt=0
set @ForUsers=null
DECLARE curP3 CURSOR FOR 
SELECT   
(SELECT U.UserName + ','  FROM Web.AspNetUserRoles UR LEFT JOIN web.Users U ON U.Id = UR.UserId  WHERE UR.RoleId 
=(SELECT id FROM Web.AspNetRoles WHERE Name =
     CASE WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='TUFTED' THEN 'Purchase Manager (Finished Tufted)'
	 	 WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='KELIM' THEN 'Purchase Manager (Finished Kelim)'
	 	 WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='KNOTTED' THEN 'Purchase Manager (Finished Knotted)'
	 	 WHEN max(PN.ProductNatureName) ='Raw Material'      THEN 'Purchase Manager (Raw)'
	 	 WHEN max(PN.ProductNatureName) ='Other Material'    THEN 'Purchase Manager (Other)'
	 END ) FOR XML path('') ) AS USERS,
	 /*max(PN.ProductNatureName) AS ProductNatureName,max(D.DivisionName) AS DivisionName,*/
	count(DISTINCT PurchaseOrderNo),
(SELECT DISTINCT POB1.PurchaseOrderNo +','
FROM Web._ViewPurchaseOrderBalance POB1
LEFT JOIN Web.Products P1 ON POB1.ProductId=P1.ProductId
LEFT JOIN web.ProductGroups PG1 ON PG1.ProductGroupId=P1.ProductGroupId
LEFT JOIN Web.Producttypes PT1 ON PG1.ProductTypeId=PT1.ProductTypeId
WHERE DATEDIFF(day,duedate,getdate()) <=21 AND DATEDIFF(day,duedate,getdate())>=15
AND PT.ProductNatureId=PT1.ProductNatureId AND POB.DivisionId =POB1.DivisionId 
AND POB.SiteId =POB1.SiteId 
FOR XML PATH('')
) 
FROM Web._ViewPurchaseOrderBalance POB
LEFT JOIN Web.Products P ON POB.ProductId=P.ProductId
LEFT JOIN web.ProductGroups PG ON PG.ProductGroupId=P.ProductGroupId
LEFT JOIN Web.Producttypes PT ON PG.ProductTypeId=PT.ProductTypeId
LEFT JOIN Web.ProductNatures PN ON PN.ProductNatureId=PT.ProductNatureId
LEFT JOIN Web.divisions D ON D.DivisionId=POB.DivisionId
WHERE DATEDIFF(day,duedate,getdate()) <=21 AND DATEDIFF(day,duedate,getdate())>=15
GROUP BY PT.ProductNatureId, POB.DivisionId,POB.SiteId 

OPEN curP3
Fetch Next From curP3 Into @ForUsers, @Cnt,@NotiMsg
While @@Fetch_Status = 0 Begin
SET @NotificationText=convert(VARCHAR,@Cnt)+'  '+'Purchase Order is  3 week Over due'+' '+'['+ LEFT(@NotiMsg,len(@NotiMsg)-1) +']';
if(@Cnt>0 and @ForUsers is not null)
begin
EXEC Logindb.dbo.CreateNotification @SubjectId , @NotificationText ,@NotificationURL,@ExpiryDate,@ForUsers,@CreatedBy
end
Fetch Next From curP3 Into @ForUsers, @Cnt,@NotiMsg
End
Close curP3
Deallocate curP3
/* for the 22 to 28  4week*/

set @Cnt=0
set @ForUsers=null
DECLARE curP4 CURSOR FOR 
SELECT   
(SELECT U.UserName + ','  FROM Web.AspNetUserRoles UR LEFT JOIN web.Users U ON U.Id = UR.UserId  WHERE UR.RoleId 
=(SELECT id FROM Web.AspNetRoles WHERE Name =
     CASE WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='TUFTED' THEN 'Purchase Manager (Finished Tufted)'
	 	 WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='KELIM' THEN 'Purchase Manager (Finished Kelim)'
	 	 WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='KNOTTED' THEN 'Purchase Manager (Finished Knotted)'
	 	 WHEN max(PN.ProductNatureName) ='Raw Material'      THEN 'Purchase Manager (Raw)'
	 	 WHEN max(PN.ProductNatureName) ='Other Material'    THEN 'Purchase Manager (Other)'
	 END ) FOR XML path('') ) AS USERS,
	 /*max(PN.ProductNatureName) AS ProductNatureName,max(D.DivisionName) AS DivisionName,*/
	count(DISTINCT PurchaseOrderNo),
(SELECT DISTINCT POB1.PurchaseOrderNo +','
FROM Web._ViewPurchaseOrderBalance POB1
LEFT JOIN Web.Products P1 ON POB1.ProductId=P1.ProductId
LEFT JOIN web.ProductGroups PG1 ON PG1.ProductGroupId=P1.ProductGroupId
LEFT JOIN Web.Producttypes PT1 ON PG1.ProductTypeId=PT1.ProductTypeId
WHERE DATEDIFF(day,duedate,getdate()) <=28 AND DATEDIFF(day,duedate,getdate())>=22
AND PT.ProductNatureId=PT1.ProductNatureId AND POB.DivisionId =POB1.DivisionId 
AND POB.SiteId =POB1.SiteId 
FOR XML PATH('')
) 
FROM Web._ViewPurchaseOrderBalance POB
LEFT JOIN Web.Products P ON POB.ProductId=P.ProductId
LEFT JOIN web.ProductGroups PG ON PG.ProductGroupId=P.ProductGroupId
LEFT JOIN Web.Producttypes PT ON PG.ProductTypeId=PT.ProductTypeId
LEFT JOIN Web.ProductNatures PN ON PN.ProductNatureId=PT.ProductNatureId
LEFT JOIN Web.divisions D ON D.DivisionId=POB.DivisionId
WHERE DATEDIFF(day,duedate,getdate()) <=28 AND DATEDIFF(day,duedate,getdate())>=22
GROUP BY PT.ProductNatureId, POB.DivisionId,POB.SiteId 

OPEN curP4
Fetch Next From curP4 Into @ForUsers, @Cnt,@NotiMsg
While @@Fetch_Status = 0 Begin
SET @NotificationText=convert(VARCHAR,@Cnt)+'  '+'Purchase Order is  4 week Over due'+' '+'['+ LEFT(@NotiMsg,len(@NotiMsg)-1) +']';
if(@Cnt>0 and @ForUsers is not null)
begin
EXEC Logindb.dbo.CreateNotification @SubjectId , @NotificationText ,@NotificationURL,@ExpiryDate,@ForUsers,@CreatedBy
end
Fetch Next From curP4 Into @ForUsers, @Cnt,@NotiMsg
End
Close curP4
Deallocate curP4
/* for the > 28  4week*/

set @Cnt=0
set @ForUsers=null
DECLARE curP5 CURSOR FOR 
SELECT   
(SELECT U.UserName + ','  FROM Web.AspNetUserRoles UR LEFT JOIN web.Users U ON U.Id = UR.UserId  WHERE UR.RoleId 
=(SELECT id FROM Web.AspNetRoles WHERE Name =
     CASE WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='TUFTED' THEN 'Purchase Manager (Finished Tufted)'
	 	 WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='KELIM' THEN 'Purchase Manager (Finished Kelim)'
	 	 WHEN max(PN.ProductNatureName) ='Finished Material' AND max(D.DivisionName) ='KNOTTED' THEN 'Purchase Manager (Finished Knotted)'
	 	 WHEN max(PN.ProductNatureName) ='Raw Material'      THEN 'Purchase Manager (Raw)'
	 	 WHEN max(PN.ProductNatureName) ='Other Material'    THEN 'Purchase Manager (Other)'
	 END ) FOR XML path('') ) AS USERS,
	 /*max(PN.ProductNatureName) AS ProductNatureName,max(D.DivisionName) AS DivisionName,*/
	count(DISTINCT PurchaseOrderNo),
(SELECT DISTINCT POB1.PurchaseOrderNo +','
FROM Web._ViewPurchaseOrderBalance POB1
LEFT JOIN Web.Products P1 ON POB1.ProductId=P1.ProductId
LEFT JOIN web.ProductGroups PG1 ON PG1.ProductGroupId=P1.ProductGroupId
LEFT JOIN Web.Producttypes PT1 ON PG1.ProductTypeId=PT1.ProductTypeId
WHERE DATEDIFF(day,duedate,getdate())>28
AND PT.ProductNatureId=PT1.ProductNatureId AND POB.DivisionId =POB1.DivisionId 
AND POB.SiteId =POB1.SiteId 
FOR XML PATH('')
) 
FROM Web._ViewPurchaseOrderBalance POB
LEFT JOIN Web.Products P ON POB.ProductId=P.ProductId
LEFT JOIN web.ProductGroups PG ON PG.ProductGroupId=P.ProductGroupId
LEFT JOIN Web.Producttypes PT ON PG.ProductTypeId=PT.ProductTypeId
LEFT JOIN Web.ProductNatures PN ON PN.ProductNatureId=PT.ProductNatureId
LEFT JOIN Web.divisions D ON D.DivisionId=POB.DivisionId
WHERE  DATEDIFF(day,duedate,getdate())>28
GROUP BY PT.ProductNatureId, POB.DivisionId,POB.SiteId 

OPEN curP5
Fetch Next From curP5 Into @ForUsers, @Cnt,@NotiMsg
While @@Fetch_Status = 0 Begin
SET @NotificationText=convert(VARCHAR,@Cnt)+'  '+'Purchase Order is  4 + week Over due'+' '+'['+ LEFT(@NotiMsg,len(@NotiMsg)-1) +']';
if(@Cnt>0 and @ForUsers is not null)
begin
EXEC Logindb.dbo.CreateNotification @SubjectId , @NotificationText ,@NotificationURL,@ExpiryDate,@ForUsers,@CreatedBy
end
Fetch Next From curP5 Into @ForUsers, @Cnt,@NotiMsg
End
Close curP5
Deallocate curP5
end





GO



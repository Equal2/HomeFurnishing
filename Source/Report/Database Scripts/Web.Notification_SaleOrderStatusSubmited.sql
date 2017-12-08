USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[Notification_SaleOrderStatusSubmited]    Script Date: 04/Aug/2015 11:53:59 AM ******/
IF OBJECT_ID ('[Web].[Notification_SaleOrderStatusSubmited]') IS NOT NULL
	DROP PROCEDURE [Web].[Notification_SaleOrderStatusSubmited]	
GO
/****** Object:  StoredProcedure [Web].[Notification_SaleOrderStatusSubmited]    Script Date: 04/Aug/2015 11:53:59 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [Web].[Notification_SaleOrderStatusSubmited]
as
begin

 DECLARE @RoleName VARCHAR(100)
  DECLARE @SubjectId INT
  DECLARE @NotificationText NVARCHAR(Max)
  DECLARE @NotificationURL NVARCHAR(Max)=Null
  DECLARE @ExpiryDate SMALLDATETIME=Null 
  DECLARE @ForUsers NVARCHAR(Max) 
  DECLARE @CreatedBy NVARCHAR(Max)  
  DECLARE @Cnt INT=0
  DECLARE @NotiMsg NVARCHAR(Max)
  declare @Days int
  SET @SubjectId ='1'
  SET @CreatedBy ='Admin' 
  SET @RoleName='General Manager'
  set @Days='2'


SELECT @Cnt=count(DISTINCT H.DocNo),@NotiMsg=(SELECT  DISTINCT H.DocNo +',' FROM web._SaleOrderHeaders H LEFT JOIN Web.Activitylogs AL ON H.SaleOrderHeaderId=AL.DocId WHERE  H.Status IN  ('1','3') AND datediff(Day,AL.CreatedDate ,getdate())=@Days FOR XML path (''))
 FROM web._SaleOrderHeaders H LEFT JOIN Web.Activitylogs AL ON H.SaleOrderHeaderId=AL.DocId
WHERE  H.Status IN  ('1','3')
AND datediff(Day,AL.CreatedDate ,getdate()) =@Days

SET @NotificationText=convert(VARCHAR,@Cnt)+'  '+'Sale Order are not Approved from '+convert(VARCHAR,@Days) +' '+'  Days' +' '+'['+ LEFT(@NotiMsg,len(@NotiMsg)-1) +']';
SET  @ForUsers=(select UserName + ',' from web.users U1 where Id In (select UserId from  web.AspNetUserRoles where RoleId=(select Id from web.AspNetRoles where Name=@RoleName))FOR XML path (''))

if(@Cnt>0 and @ForUsers is not null)
begin
EXEC Logindb.dbo.CreateNotification @SubjectId , @NotificationText ,@NotificationURL,@ExpiryDate,@ForUsers,@CreatedBy
end

/*for 4 days*/
SET @RoleName='Managing Director'
set @Days='4'
set @Cnt=0

SELECT @Cnt=count(DISTINCT H.DocNo),@NotiMsg=(SELECT  DISTINCT H.DocNo +',' FROM web._SaleOrderHeaders H LEFT JOIN Web.Activitylogs AL ON H.SaleOrderHeaderId=AL.DocId WHERE  H.Status IN  ('1','3') AND datediff(Day,AL.CreatedDate ,getdate())=@Days FOR XML path (''))
 FROM web._SaleOrderHeaders H LEFT JOIN Web.Activitylogs AL ON H.SaleOrderHeaderId=AL.DocId
WHERE  H.Status IN  ('1','3')
AND datediff(Day,AL.CreatedDate ,getdate()) =@Days

SET @NotificationText=convert(VARCHAR,@Cnt)+'  '+'Sale Order are not Approved from '+convert(VARCHAR,@Days) +' '+'  Days' +' '+'['+ LEFT(@NotiMsg,len(@NotiMsg)-1) +']';
SET  @ForUsers=(select UserName + ',' from web.users U1 where Id In (select UserId from  web.AspNetUserRoles where RoleId=(select Id from web.AspNetRoles where Name=@RoleName))FOR XML path (''))

if(@Cnt>0 and @ForUsers is not null)
begin
EXEC Logindb.dbo.CreateNotification @SubjectId , @NotificationText ,@NotificationURL,@ExpiryDate,@ForUsers,@CreatedBy
end
/* for the 4+ days*/
set @Cnt=0
SELECT @Cnt=count(DISTINCT H.DocNo),@NotiMsg=(SELECT  DISTINCT H.DocNo +',' FROM web._SaleOrderHeaders H LEFT JOIN Web.Activitylogs AL ON H.SaleOrderHeaderId=AL.DocId WHERE  H.Status IN  ('1','3') AND datediff(Day,AL.CreatedDate ,getdate())>@Days FOR XML path (''))
 FROM web._SaleOrderHeaders H LEFT JOIN Web.Activitylogs AL ON H.SaleOrderHeaderId=AL.DocId
WHERE  H.Status IN  ('1','3')
AND datediff(Day,AL.CreatedDate ,getdate()) >@Days

SET @NotificationText=convert(VARCHAR,@Cnt)+'  '+'Sale Order are not Approved from '+convert(VARCHAR,@Days) +'+'+'  Days' +' '+'['+ LEFT(@NotiMsg,len(@NotiMsg)-1) +']';
SET  @ForUsers=(select UserName + ',' from web.users U1 where Id In (select UserId from  web.AspNetUserRoles where RoleId=(select Id from web.AspNetRoles where Name=@RoleName))FOR XML path (''))

if(@Cnt>0 and @ForUsers is not null)
begin
EXEC Logindb.dbo.CreateNotification @SubjectId , @NotificationText ,@NotificationURL,@ExpiryDate,@ForUsers,@CreatedBy
end


end


GO



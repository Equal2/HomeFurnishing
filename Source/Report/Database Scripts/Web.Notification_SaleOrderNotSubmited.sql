USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[Notification_SaleOrderNotSubmited]    Script Date: 04/Aug/2015 11:50:54 AM ******/

IF OBJECT_ID ('[Web].[Notification_SaleOrderNotSubmited]') IS NOT NULL
	DROP PROCEDURE [Web].[Notification_SaleOrderNotSubmited]	
GO
/****** Object:  StoredProcedure [Web].[Notification_SaleOrderNotSubmited]    Script Date: 04/Aug/2015 11:50:54 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [Web].[Notification_SaleOrderNotSubmited]
AS
BEGIN	

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
  
 /* for the sales Manager*/
SET @RoleName='Sales Manager'
set @Days='2'
SELECT  @Cnt=count(H.DocNo), 
@NotiMsg=(SELECT H1.DocNo + ','  FROM Web._SaleOrderHeaders H1  WHERE datediff(Day,H1.CreatedDate ,getdate())=datediff(Day,Max(H.CreatedDate),getdate()) FOR XML path ('')) 
FROM Web._SaleOrderHeaders H  
WHERE Status=0	AND datediff(Day,CreatedDate ,getdate())=@Days

SET @NotificationText=convert(VARCHAR,@Cnt)+'  '+'Sale Order are not Submitted from '+convert(VARCHAR,@Days) +' '+'  Days' +' '+'['+ LEFT(@NotiMsg,len(@NotiMsg)-1) +']';
SET  @ForUsers=(select UserName + ',' from web.users U1 where Id In (select UserId from  web.AspNetUserRoles where RoleId=(select Id from web.AspNetRoles where Name=@RoleName))FOR XML path (''))

if(@Cnt>0 and @ForUsers is not null)
begin
EXEC Logindb.dbo.CreateNotification @SubjectId , @NotificationText ,@NotificationURL,@ExpiryDate,@ForUsers,@CreatedBy
end

 /* for the General manger*/
SET @RoleName='General Manager'
set @Days='4'
set @Cnt=0
SELECT  @Cnt=count(H.DocNo), 
@NotiMsg=(SELECT H1.DocNo + ','  FROM Web._SaleOrderHeaders H1  WHERE datediff(Day,H1.CreatedDate ,getdate())=datediff(Day,max(H.CreatedDate),getdate()) FOR XML path ('')) 
FROM Web._SaleOrderHeaders H  
WHERE Status=0	AND datediff(Day,CreatedDate ,getdate())=@Days

SET @NotificationText=convert(VARCHAR,@Cnt)+'  '+'Sale Order are not Submitted from '+convert(VARCHAR,@Days) +' '+'  Days' +' '+'['+ LEFT(@NotiMsg,len(@NotiMsg)-1) +']';
SET  @ForUsers=(select UserName + ',' from web.users U1 where Id In (select UserId from  web.AspNetUserRoles where RoleId=(select Id from web.AspNetRoles where Name=@RoleName))FOR XML path (''))

if(@Cnt>0 and @ForUsers is not null)
begin
EXEC Logindb.dbo.CreateNotification @SubjectId , @NotificationText ,@NotificationURL,@ExpiryDate,@ForUsers,@CreatedBy
end

 /* for the Managing Director*/
SET @RoleName='Managing Director'
set @Days='6'
set @Cnt=0
SELECT  @Cnt=count(H.DocNo), 
@NotiMsg=(SELECT H1.DocNo + ','  FROM Web._SaleOrderHeaders H1  WHERE datediff(Day,H1.CreatedDate ,getdate())=datediff(Day,max(H.CreatedDate),getdate()) FOR XML path ('')) 
FROM Web._SaleOrderHeaders H  
WHERE  Status=0	AND datediff(Day,CreatedDate ,getdate())=@Days

SET @NotificationText=convert(VARCHAR,@Cnt)+'  '+'Sale Order are not Submitted from '+convert(VARCHAR,@Days) +' '+'  Days' +' '+'['+ LEFT(@NotiMsg,len(@NotiMsg)-1) +']';
SET  @ForUsers=(select UserName + ',' from web.users U1 where Id In (select UserId from  web.AspNetUserRoles where RoleId=(select Id from web.AspNetRoles where Name=@RoleName))FOR XML path (''))

if(@Cnt>0 and @ForUsers is not null)
begin
EXEC Logindb.dbo.CreateNotification @SubjectId , @NotificationText ,@NotificationURL,@ExpiryDate,@ForUsers,@CreatedBy
end

/*for the date>6*/

SET @RoleName='Managing Director'
set @Days='6'
set @Cnt=0
SELECT  @Cnt=count(H.DocNo), 
@NotiMsg=(SELECT H1.DocNo + ','  FROM Web._SaleOrderHeaders H1  WHERE  H1.Status=0	AND datediff(Day,H1.CreatedDate ,getdate())>@Days FOR XML path ('')) 
FROM Web._SaleOrderHeaders H  
WHERE  H.Status=0	AND datediff(Day,H.CreatedDate ,getdate())>@Days

SET @NotificationText=convert(VARCHAR,@Cnt)+'  '+'Sale Order are not Submitted from '+convert(VARCHAR,@Days) +'+'+'  Days' +' '+'['+ LEFT(@NotiMsg,len(@NotiMsg)-1) +']';
SET  @ForUsers=(select UserName + ',' from web.users U1 where Id In (select UserId from  web.AspNetUserRoles where RoleId=(select Id from web.AspNetRoles where Name=@RoleName))FOR XML path (''))

if(@Cnt>0 and @ForUsers is not null)
begin
EXEC Logindb.dbo.CreateNotification @SubjectId , @NotificationText ,@NotificationURL,@ExpiryDate,@ForUsers,@CreatedBy
end

end





GO



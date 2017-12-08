USE [LoginDB]
GO

/****** Object:  StoredProcedure [dbo].[CreateNotification]    Script Date: 04/Aug/2015 11:59:41 AM ******/

IF OBJECT_ID ('[dbo].[CreateNotification]') IS NOT NULL
	DROP PROCEDURE [dbo].[CreateNotification]	
GO
/****** Object:  StoredProcedure [dbo].[CreateNotification]    Script Date: 04/Aug/2015 11:59:41 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateNotification] 
		@SubjectId INT, 
		@NotificationText NVARCHAR(Max), 
		@NotificationURL NVARCHAR(Max)=Null, 
		@ExpiryDate SMALLDATETIME=Null, 
		@ForUsers NVARCHAR(Max), 
		@CreatedBy NVARCHAR(Max)		
as

DECLARE @TblIdentity TABLE (Id int)

INSERT INTO dbo.Notifications (NotificationSubjectId, NotificationText, NotificationUrl, ExpiryDate, IsActive, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate) OUTPUT Inserted.NotificationId INTO @TblIdentity(Id)
VALUES (@SubjectId, @NotificationText, @NotificationURL, @ExpiryDate, 1, @CreatedBy, @CreatedBy, Getdate(), GetDate())


INSERT INTO dbo.NotificationUsers (NotificationId, UserName)
SELECT Id AS NotificationId, Items AS UserName
FROM @TblIdentity, (SELECT * FROM dbo.[Split](@ForUsers,',')) AS x
GO



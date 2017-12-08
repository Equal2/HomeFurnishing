USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[DailyNotifications]    Script Date: 04/Aug/2015 11:58:00 AM ******/

IF OBJECT_ID ('[Web].[DailyNotifications]') IS NOT NULL
	DROP PROCEDURE [Web].[DailyNotifications]	
GO
/****** Object:  StoredProcedure [Web].[DailyNotifications]    Script Date: 04/Aug/2015 11:58:00 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [Web].[DailyNotifications]
as
begin

Execute Web.Notification_SaleOrderNotSubmited 
Execute Web.Notification_SaleOrderStatusSubmited
Execute Web.Notification_ViewPurchaseOrderBalance
end
GO



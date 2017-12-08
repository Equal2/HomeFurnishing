/* first view */
USE [RUG]
GO
IF OBJECT_ID ('[Web].[_MaterialPlanHeaders]') IS NOT NULL
DROP VIEW [Web].[_MaterialPlanHeaders]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Web].[_MaterialPlanHeaders] as
SELECT * FROM web.MaterialPlanHeaders
GO
/* Second View */
USE [RUG]
GO
IF OBJECT_ID ('[Web].[_MaterialPlanLines]') IS NOT NULL
DROP VIEW [Web].[_MaterialPlanLines]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Web].[_MaterialPlanLines] as
SELECT * FROM web.MaterialPlanLines
GO
/*Third view*/
USE [RUG]
GO
IF OBJECT_ID ('[Web].[_PackingHeaders]') IS NOT NULL
DROP VIEW [Web].[_PackingHeaders]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Web].[_PackingHeaders] as
SELECT * FROM web.PackingHeaders
GO
/* Fourth view*/
USE [RUG]
GO
IF OBJECT_ID ('[Web].[_PackingLines]') IS NOT NULL
DROP VIEW [Web].[_PackingLines]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Web].[_PackingLines] as
SELECT * FROM web.PackingLines
GO
/* Jobreturn */
USE [RUG]
GO
IF OBJECT_ID ('[Web].[_JobReturnLines]') IS NOT NULL
DROP VIEW [Web].[_JobReturnLines]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Web].[_JobReturnLines] as
SELECT * FROM web.JobReturnLines
GO
/* JobreturnHeader */
USE [RUG]
GO
IF OBJECT_ID ('[Web].[_jobreturnheaders]') IS NOT NULL
DROP VIEW [Web].[_jobreturnheaders]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Web].[_jobreturnheaders] as
SELECT * FROM web.jobreturnheaders
GO
/* saleOrderheader*/
USE [RUG]
GO
IF OBJECT_ID ('[Web].[_SaleOrderHeaders]') IS NOT NULL
DROP VIEW [Web].[_SaleOrderHeaders]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Web].[_SaleOrderHeaders] as
SELECT * FROM web.SaleOrderHeaders
GO
/* View Purchase Orderbalance*/

USE [RUG]
GO
IF OBJECT_ID ('[Web].[_ViewPurchaseOrderBalance]') IS NOT NULL
DROP VIEW [Web].[_ViewPurchaseOrderBalance]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Web].[_ViewPurchaseOrderBalance] as
SELECT * FROM web.ViewPurchaseOrderBalance
GO
/* PurchaseGoodsReceiptLine */

USE [RUG]
GO
IF OBJECT_ID ('[Web].[_PurchaseGoodsReceiptLines]') IS NOT NULL
DROP VIEW [Web].[_PurchaseGoodsReceiptLines]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [Web].[_PurchaseGoodsReceiptLines] as
SELECT * FROM web.PurchaseGoodsReceiptLines
GO


USE [RUG]
GO

/****** Object:  Table [Web].[LeaveTypes]    Script Date: 18/Aug/2015 12:16:29 PM ******/


IF OBJECT_ID ('[Web].[LeaveTypes]') IS NOT NULL
	DROP TABLE [Web].[LeaveTypes]
GO
/****** Object:  Table [Web].[LeaveTypes]    Script Date: 18/Aug/2015 12:16:29 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [Web].[LeaveTypes](
	[LeaveTypeId] [int] IDENTITY(1,1) NOT NULL,
	[LeaveTypeName] [nvarchar](50) NULL,
	[SiteId] [int] NULL,
	[IsActive] [bit] NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedDate] [datetime] NULL,
	[OMSId] [nvarchar](50) NULL
) ON [PRIMARY]

GO



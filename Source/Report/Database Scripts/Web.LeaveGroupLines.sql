USE [RUG]
GO

/****** Object:  Table [Web].[LeaveGroupLines]    Script Date: 18/Aug/2015 6:34:43 PM ******/


IF OBJECT_ID ('[Web].[LeaveGroupLines]') IS NOT NULL
	DROP TABLE [Web].[LeaveGroupLines]
GO
/****** Object:  Table [Web].[LeaveGroupLines]    Script Date: 18/Aug/2015 6:34:43 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [Web].[LeaveGroupLines](
	[LeaveGroupLineId] [bigint] IDENTITY(1,1) NOT NULL,
	[LeaveGroupId] [bigint] NULL,
	[leaveTypeId] [bigint] NULL,
	[PaymentPer] [decimal](18, 0) NULL,
	[LeaveAllowed] [int] NULL,
	[MaxContinueLeaveAllowed] [int] NULL,
	[IsCarryForward] [bit] NULL,
	[IsEncashment] [bit] NULL,
	[IsHolydayIncluded] [bit] NULL,
	[IsAllowedNegetive] [bit] NULL,
	[CreatedBy] [varchar](50) NULL,
	[ModifiedBy] [varchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedDate] [datetime] NULL,
	[OMSId] [varchar](50) NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO



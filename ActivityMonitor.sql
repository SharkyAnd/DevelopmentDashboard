USE [DevelopmentDashboard]
GO
/****** Object:  Table [dbo].[ActivityMonitor]    Script Date: 07.03.2016 18:17:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActivityMonitor](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Moment] [datetime] NOT NULL,
	[Type] [nvarchar](150) NOT NULL,
 CONSTRAINT [PK_ActivityMonitor] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ActivityMonitorValues]    Script Date: 07.03.2016 18:17:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActivityMonitorValues](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ParentId] [bigint] NOT NULL,
	[Property] [nvarchar](150) NOT NULL,
	[Value] [nvarchar](500) NOT NULL,
 CONSTRAINT [PK_ActivityMonitorValues] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

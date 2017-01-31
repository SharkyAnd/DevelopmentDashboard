USE [DevelopmentDashboard]
GO

/****** Object:  Table [dbo].[WebUsers]    Script Date: 13.05.2016 1:10:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[WebUsers](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [varchar](250) NULL,
	[Email] [varchar](250) NULL,
	[PasswordHash] [varchar](max) NULL,
	[LastVisitDate] [datetime] NULL,
	[AddedDate] [datetime] NULL,
 CONSTRAINT [PK_WebUsers] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[WebUsers]  WITH CHECK ADD  CONSTRAINT [FK_WebUsers_WebUsers] FOREIGN KEY([id])
REFERENCES [dbo].[WebUsers] ([id])
GO

ALTER TABLE [dbo].[WebUsers] CHECK CONSTRAINT [FK_WebUsers_WebUsers]
GO



USE [DevelopmentDashboard]
GO

/****** Object:  Table [dbo].[ConfirmationMails]    Script Date: 13.05.2016 1:09:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[ConfirmationMails](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ActivationCode] [varchar](max) NULL,
	[SenderUserId] [int] NULL,
	[RecieverUserId] [int] NULL,
	[Activated] [bit] NULL,
	[ActivateDate] [datetime] NULL,
	[SendDate] [datetime] NULL,
 CONSTRAINT [PK_ConfirmationMails] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[ConfirmationMails]  WITH CHECK ADD  CONSTRAINT [FK_ConfirmationMails_UsersReciever] FOREIGN KEY([RecieverUserId])
REFERENCES [dbo].[WebUsers] ([id])
GO

ALTER TABLE [dbo].[ConfirmationMails] CHECK CONSTRAINT [FK_ConfirmationMails_UsersReciever]
GO

ALTER TABLE [dbo].[ConfirmationMails]  WITH CHECK ADD  CONSTRAINT [FK_ConfirmationMails_UsersSender] FOREIGN KEY([SenderUserId])
REFERENCES [dbo].[WebUsers] ([id])
GO

ALTER TABLE [dbo].[ConfirmationMails] CHECK CONSTRAINT [FK_ConfirmationMails_UsersSender]
GO



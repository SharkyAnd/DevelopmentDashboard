USE [DevelopmentDashboard]
GO

/****** Object:  Table [dbo].[PermissionsByPublicAccessRules]    Script Date: 26.05.2016 1:35:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PermissionsByPublicAccessRules](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[RuleId] [int] NULL,
	[PermissionId] [int] NULL,
 CONSTRAINT [PK_PermissionsByPublicAccessRules] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[PermissionsByPublicAccessRules]  WITH CHECK ADD  CONSTRAINT [FK_PermissionsByPublicAccessRules_Permissions] FOREIGN KEY([PermissionId])
REFERENCES [dbo].[Permissions] ([id])
GO

ALTER TABLE [dbo].[PermissionsByPublicAccessRules] CHECK CONSTRAINT [FK_PermissionsByPublicAccessRules_Permissions]
GO

ALTER TABLE [dbo].[PermissionsByPublicAccessRules]  WITH CHECK ADD  CONSTRAINT [FK_PermissionsByPublicAccessRules_PublicAccessRules] FOREIGN KEY([RuleId])
REFERENCES [dbo].[PublicAccessRules] ([id])
GO

ALTER TABLE [dbo].[PermissionsByPublicAccessRules] CHECK CONSTRAINT [FK_PermissionsByPublicAccessRules_PublicAccessRules]
GO



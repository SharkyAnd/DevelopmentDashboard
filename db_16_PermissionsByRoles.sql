USE [DevelopmentDashboard]
GO

/****** Object:  Table [dbo].[PermissionsByRoles]    Script Date: 21.04.2016 20:45:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PermissionsByRoles](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [int] NULL,
	[PermissionId] [int] NULL,
 CONSTRAINT [PK_PermissionsByRoles] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[PermissionsByRoles]  WITH CHECK ADD  CONSTRAINT [FK_PermissionsByRoles_Roles] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([Id])
GO

ALTER TABLE [dbo].[PermissionsByRoles] CHECK CONSTRAINT [FK_PermissionsByRoles_Roles]
GO



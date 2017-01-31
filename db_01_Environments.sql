USE [DevelopmentDashboard]
GO

CREATE TABLE [dbo].[Environments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](250) NULL,
	[Value] [varchar](250) NULL,
	[ConfigName] [varchar](250) NULL,
	[Description] [varchar](MAX) NULL,
 CONSTRAINT [PK_Environments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

grant SELECT, INSERT, DELETE, UPDATE on Environments to DDUser
go

select * from Environments order by Id
go



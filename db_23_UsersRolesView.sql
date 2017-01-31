USE [DevelopmentDashboard]
GO

/****** Object:  View [dbo].[UsersRolesView]    Script Date: 22.06.2016 11:42:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[UsersRolesView]
AS
SELECT        ur.id, u.UserName, r.Name, CASE WHEN u.UserName IS NOT NULL AND r.Name IS NOT NULL THEN 1 ELSE 0 END AS UserInRole
FROM            dbo.WebUsers AS u FULL OUTER JOIN
                         dbo.UsersRoles AS ur ON u.id = ur.UserId FULL OUTER JOIN
                         dbo.Roles AS r ON ur.RoleId = r.Id

GO





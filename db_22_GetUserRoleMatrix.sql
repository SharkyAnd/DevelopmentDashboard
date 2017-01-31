USE [DevelopmentDashboard]
GO

/****** Object:  StoredProcedure [dbo].[GetUserRoleMatrix]    Script Date: 22.06.2016 11:34:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[GetUserRoleMatrix]

AS
BEGIN
	DECLARE @cols AS NVARCHAR(MAX),
    @query  AS NVARCHAR(MAX)

select @cols = STUFF((SELECT ',' + QUOTENAME(Name) 
                    from UsersRolesView
                    group by Name                   
            FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)') 
        ,1,1,'')

set @query = N'SELECT UserName,
       ' + @cols + N' from 
             (
                select UserName, Name, UserInRole
                from UsersRolesView
            ) x
            pivot 
            (
                max(UserInRole)
                for Name in (' + @cols + N')
            ) p WHERE UserName IS NOT NULL '

exec sp_executesql @query;
END

GO



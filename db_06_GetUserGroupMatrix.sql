USE [DevelopmentDashboard]
GO

/****** Object:  StoredProcedure [dbo].[GetUserGroupMatrix]    Script Date: 07.03.2016 19:58:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[GetUserGroupMatrix]

AS
BEGIN
	DECLARE @cols AS NVARCHAR(MAX),
    @query  AS NVARCHAR(MAX)

select @cols = STUFF((SELECT ',' + QUOTENAME(Name) 
                    from UserGroupView
                    group by Name                   
            FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)') 
        ,1,1,'')

set @query = N'SELECT UserName,
       ' + @cols + N' from 
             (
                select UserName, Name, UserInGroup
                from UserGroupView
            ) x
            pivot 
            (
                max(UserInGroup)
                for Name in (' + @cols + N')
            ) p WHERE UserName IS NOT NULL '

exec sp_executesql @query;
END

GO



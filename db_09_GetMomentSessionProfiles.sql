USE [DevelopmentDashboard]
GO

/****** Object:  StoredProcedure [dbo].[GetMomentSessionProfiles]    Script Date: 14.06.2016 9:55:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetMomentSessionProfiles]
    @DateFrom datetime,
	@DateTo datetime,
    @Interval TINYINT = 1,
	@AllSessions bit = 0,
	@GroupName varchar(100) = 'minute',
	@MachineNames AS dbo.StringsArray READONLY,
	@UserNames AS dbo.StringsArray READONLY
AS
BEGIN
    SET NOCOUNT ON;
		drop table dates

    DECLARE 
        @IntervalCount INT, @StartDate SMALLDATETIME;
	if @GroupName = 'minute'
		SELECT	
			@StartDate = DATEADD(MINUTE, -1, MIN(ChunkBegin)), 
			@IntervalCount = (DATEDIFF(MINUTE, MIN(ChunkBegin), MAX(ChunkBegin)) 
            + @Interval) / @Interval
        FROM SessionActivityProfiles
		WHERE ChunkBegin between @DateFrom and @DateTo
	else if @GroupName = 'hour'
		SELECT	
			@StartDate = DATEADD(HOUR, -1, MIN(ChunkBegin)), 
			@IntervalCount = (DATEDIFF(HOUR, MIN(ChunkBegin), MAX(ChunkBegin)) 
            + @Interval) / @Interval
        FROM SessionActivityProfiles
		WHERE ChunkBegin between @DateFrom and @DateTo
	else
		SELECT	
			@StartDate = DATEADD(DAY, -1, MIN(CAST(FORMAT(ChunkBegin, 'yyyy-MM-dd 00:00:00') as datetime))), 
			@IntervalCount = (DATEDIFF(DAY, MIN(ChunkBegin), MAX(ChunkBegin)) 
            + @Interval) / @Interval
        FROM SessionActivityProfiles
		WHERE ChunkBegin between @DateFrom and @DateTo
	
        SELECT
		CASE 
			WHEN @GroupName = 'minute' THEN  DATEADD(MINUTE, @Interval*(n.n-1), @StartDate)
			WHEN @GroupName = 'hour' THEN  DATEADD(HOUR, @Interval*(n.n-1), @StartDate)
			ELSE DATEADD(DAY, @Interval*(n.n-1), @StartDate)
		END AS s,
		CASE            
			WHEN @GroupName = 'minute' THEN DATEADD(MINUTE, @Interval*(n.n), @StartDate)
			WHEN @GroupName = 'hour' THEN DATEADD(HOUR, @Interval*(n.n), @StartDate)
			ELSE DATEADD(DAY, @Interval*(n.n), @StartDate)
		END	 AS e into dates		
        FROM
        (
          SELECT 
            TOP (@IntervalCount) ROW_NUMBER() OVER (ORDER BY o.[object_id])
            FROM sys.all_objects AS o CROSS JOIN sys.all_columns AS c
            ORDER BY o.[object_id]
        ) AS n(n)
    
    SELECT DISTINCT l.UserName +' ('+l.MachineName+')' AS UserId, ChunkInterval = d.s, Connected = COUNT(DISTINCT s.SessionId), 
	Active = COUNT(DISTINCT CASE WHEN s.IsUserActive = 1 THEN s.SessionId ELSE null END) 
    FROM dates AS d
    LEFT OUTER JOIN SessionActivityProfiles AS s
        ON s.ChunkBegin >= d.s AND s.ChunkBegin < d.e 
		LEFT JOIN Logins l ON s.SessionId = l.Id
		WHERE l.UserName IS NOT NULL AND l.UserName IN (SELECT * FROM @UserNames) AND l.MachineName IN (SELECT * FROM @MachineNames)

    GROUP BY l.UserName +' ('+l.MachineName+')',d.s
    ORDER BY d.s;
END

GO



USE [DevelopmentDashboard]
GO

/****** Object:  StoredProcedure [dbo].[GetGroupedIntervals]    Script Date: 14.06.2016 9:54:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetGroupedIntervals]
	@DateFrom datetime,
	@DateTo datetime,
    @Interval TINYINT = 1,
	@AllSessions bit = 0,
	@GroupName varchar(100) = 'minute',
	@UserNames AS dbo.StringsArray READONLY
AS
BEGIN
    SET NOCOUNT ON;
	DROP TABLE dates;
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
		END as s,
		CASE            
			WHEN @GroupName = 'minute' THEN DATEADD(MINUTE, @Interval*(n.n), @StartDate)
			WHEN @GroupName = 'hour' THEN DATEADD(HOUR, @Interval*(n.n), @StartDate)
			ELSE DATEADD(DAY, @Interval*(n.n), @StartDate)
		END as e into dates	
        FROM
        (
          SELECT 
            TOP (@IntervalCount) ROW_NUMBER() OVER (ORDER BY o.[object_id])
            FROM sys.all_objects AS o CROSS JOIN sys.all_columns AS c
            ORDER BY o.[object_id]
        ) AS n(n)
    
    SELECT ChunkInterval = d.s, 
	Connected = COUNT(DISTINCT l.UserName + ' - '+l.MachineName), 
	Active = COUNT(DISTINCT CASE WHEN s.IsUserActive = 1 THEN l.UserName + ' - '+l.MachineName ELSE null END) 
    FROM dates AS d
    LEFT OUTER JOIN SessionActivityProfiles AS s
        ON s.ChunkBegin >= d.s AND s.ChunkBegin < d.e
		LEFT OUTER JOIN Logins l ON s.SessionId = l.Id
	WHERE l.UserName IN (SELECT * FROM @UserNames)
    GROUP BY d.s
    ORDER BY d.s;
END

GO



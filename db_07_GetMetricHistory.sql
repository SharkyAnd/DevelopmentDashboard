USE [DevelopmentDashboard]
GO

/****** Object:  StoredProcedure [dbo].[GetMetricHistory]    Script Date: 21.03.2016 18:05:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetMetricHistory]
AS
BEGIN
	DECLARE @cols AS NVARCHAR(MAX),
    @query  AS NVARCHAR(MAX)

select @cols = STUFF((SELECT ',' + QUOTENAME(MetricName) 
                    from MetricHistoryView
                    group by MetricName                   
            FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)') 
        ,1,1,'')

set @query = N'SELECT MetricsCalculationMoment
      ,RevisionNumber, RevisionMessage
      ,ProjectName, UserName, ' + @cols + N' from 
             (
                select MetricsCalculationMoment
      ,RevisionNumber, RevisionMessage
      ,ProjectName, UserName, MetricName, MetricValue
                from MetricHistoryView
            ) x
            pivot 
            (
                max(MetricValue)
                for MetricName in (' + @cols + N')
            ) p '

exec sp_executesql @query;
END

GO



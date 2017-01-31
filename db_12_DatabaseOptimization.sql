use DevelopmentDashboard
go

CREATE STATISTICS [_dta_stat_1925581898_6_4] ON [dbo].[CodeMetricsHistory]([ProjectName], [RevisionNumber])

CREATE STATISTICS [_dta_stat_1925581898_4_1_6] ON [dbo].[CodeMetricsHistory]([RevisionNumber], [Id], [ProjectName])

CREATE STATISTICS [_dta_stat_1925581898_1_6] ON [dbo].[CodeMetricsHistory]([Id], [ProjectName])

CREATE STATISTICS [_dta_stat_277576027_3_2] ON [dbo].[LocalBuilds]([UserName], [Moment])

--DROP INDEX [NonClusteredIndex-20160314-094130] ON [dbo].[Logins]

CREATE STATISTICS [_dta_stat_309576141_4_5_3_2] ON [dbo].[Logins]([SessionBegin], [SessionEnd], [MachineName], [UserName])


CREATE STATISTICS [_dta_stat_309576141_4_2_5] ON [dbo].[Logins]([SessionBegin], [UserName], [SessionEnd])

CREATE STATISTICS [_dta_stat_309576141_1_4_5_2] ON [dbo].[Logins]([Id], [SessionBegin], [SessionEnd], [UserName])


CREATE STATISTICS [_dta_stat_1957582012_2_3] ON [dbo].[MetricsValues]([CodeMetricsHistory_Id], [MetricName])

CREATE STATISTICS [_dta_stat_1461580245_5_6] ON [dbo].[Revisions]([Moment], [UserId])

CREATE STATISTICS [_dta_stat_1461580245_3_6] ON [dbo].[Revisions]([RevisionNumber], [UserId])

CREATE STATISTICS [_dta_stat_1461580245_2_6_1] ON [dbo].[Revisions]([RepositoryId], [UserId], [Id])

--DROP INDEX [NonClusteredIndex-ChunkBegin] ON [dbo].[SessionActivityProfiles]

--DROP INDEX [NonClusteredIndex-SessionId] ON [dbo].[SessionActivityProfiles]

CREATE CLUSTERED INDEX [_dta_index_SessionActivityProfiles_c_19_1109578991__K1] ON [dbo].[SessionActivityProfiles]
(
	[SessionId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

CREATE STATISTICS [_dta_stat_1109578991_1_4] ON [dbo].[SessionActivityProfiles]([SessionId], [IsUserActive])

CREATE NONCLUSTERED INDEX [_dta_index_SessionActivityProfiles_19_1109578991__K2_K4_1] ON [dbo].[SessionActivityProfiles]
(
	[ChunkBegin] ASC,
	[IsUserActive] ASC
)
INCLUDE ( 	[SessionId]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

--DROP INDEX [Roots] ON [dbo].[Tasks]
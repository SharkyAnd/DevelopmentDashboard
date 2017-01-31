USE [DevelopmentDashboard]
GO

/****** Object:  View [dbo].[ActivitySummary]    Script Date: 02.03.2016 21:55:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Script for SelectTopNRows command from SSMS  ******/
CREATE VIEW [dbo].[ActivitySummary]
AS
SELECT        CASE WHEN rev.UserName IS NULL THEN CASE WHEN ses.UserName IS NULL THEN CASE WHEN pub.UserName IS NULL 
                         THEN lb.UserName ELSE pub.UserName END ELSE ses.UserName END ELSE rev.UserName END AS UserName, CASE WHEN rev.Moment IS NULL THEN CASE WHEN ses.Moment IS NULL 
                         THEN CASE WHEN pub.Moment IS NULL THEN lb.Moment ELSE pub.Moment END ELSE ses.Moment END ELSE rev.Moment END AS Moment, rev.CommitsCount, ses.ActiveHours, ses.LinearHours, 
                         ses.SessionsCount, pub.PublicationsCount, lb.LocalBuildsCount
FROM            (SELECT        u.UserName, CAST(FORMAT(r.Moment, 'yyyy-MM-dd') AS datetime) AS Moment, COUNT(r.Id) AS CommitsCount
                          FROM            dbo.Revisions AS r FULL OUTER JOIN
                                                    dbo.Users AS u ON r.UserId = u.Id
                          GROUP BY u.UserName, CAST(FORMAT(r.Moment, 'yyyy-MM-dd') AS datetime)) AS rev FULL OUTER JOIN
                             (SELECT        UserName, CAST(FORMAT(SessionBegin, 'yyyy-MM-dd') AS datetime) AS Moment, SUM(ActiveHours) AS ActiveHours, SUM(CASE WHEN SessionEnd IS NULL THEN DATEDIFF(MINUTE, 
                                                         SessionBegin, GETDATE()) ELSE DATEDIFF(MINUTE, SessionBegin, SessionEnd) END) AS LinearHours, COUNT(Id) AS SessionsCount
                               FROM            dbo.Logins
                               GROUP BY UserName, CAST(FORMAT(SessionBegin, 'yyyy-MM-dd') AS datetime)) AS ses ON rev.Moment = ses.Moment AND rev.UserName = ses.UserName FULL OUTER JOIN
                             (SELECT        UserName, CAST(FORMAT(Moment, 'yyyy-MM-dd') AS datetime) AS Moment, COUNT(Id) AS PublicationsCount
                               FROM            dbo.Publications
                               GROUP BY UserName, CAST(FORMAT(Moment, 'yyyy-MM-dd') AS datetime)) AS pub ON rev.Moment = pub.Moment AND rev.UserName = pub.UserName FULL OUTER JOIN
                             (SELECT        UserName, CAST(FORMAT(Moment, 'yyyy-MM-dd') AS datetime) AS Moment, COUNT(Id) AS LocalBuildsCount
                               FROM            dbo.LocalBuilds
                               GROUP BY UserName, CAST(FORMAT(Moment, 'yyyy-MM-dd') AS datetime)) AS lb ON rev.Moment = lb.Moment AND rev.UserName = lb.UserName

GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[20] 4[10] 2[51] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "rev"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 119
               Right = 209
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ses"
            Begin Extent = 
               Top = 6
               Left = 247
               Bottom = 136
               Right = 417
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "pub"
            Begin Extent = 
               Top = 6
               Left = 454
               Bottom = 119
               Right = 641
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "lb"
            Begin Extent = 
               Top = 6
               Left = 679
               Bottom = 119
               Right = 861
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ActivitySummary'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ActivitySummary'
GO



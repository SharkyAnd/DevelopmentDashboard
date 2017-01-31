using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevelopmentDashboardCore.Models;
using System.Data;
using Utils;
using System.Data.SqlClient;

namespace DevelopmentDashboardCore.AppData
{
    public class SummaryProvider
    {
        private DatabaseUtils dbu = new DatabaseUtils(DevelopmentDashboardConfig.Instance.ConnectionString);
        public List<ActivitySummaryModel> GetActivitySummary(DateTime dateFrom, DateTime dateTo, string userNames)
        {
            List<ActivitySummaryModel> activitySummary = new List<ActivitySummaryModel>();
            try
            {
                string query = @"SELECT * FROM dbo.ActivitySummary";

                DataTable dt = dbu.ExecuteDataTable(query, null);

                activitySummary = dt.AsEnumerable().Select(r => new ActivitySummaryModel
                {
                    UserName = r["UserName"] == DBNull.Value ? null : (string)r["UserName"],
                    Moment = r["Moment"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["Moment"]),
                    ActiveHours = r["ActiveHours"] == DBNull.Value ? 0 : Convert.ToDouble(r["ActiveHours"]),
                    LinearHours = r["LinearHours"] == DBNull.Value ? 0 : Convert.ToDouble(r["LinearHours"]) / 60,
                    SessionsCount = r["SessionsCount"] == DBNull.Value ? 0 : Convert.ToDouble(r["SessionsCount"]),
                    CommitsCount = r["CommitsCount"] == DBNull.Value ? 0 : Convert.ToDouble(r["CommitsCount"]),
                    PublicationsCount = r["PublicationsCount"] == DBNull.Value ? 0 : Convert.ToDouble(r["PublicationsCount"]),
                    LocalBuildsCount = r["LocalBuildsCount"] == DBNull.Value ? 0 : Convert.ToDouble(r["LocalBuildsCount"])
                }).Where(asm => asm.Moment >= dateFrom && asm.Moment <= dateTo && userNames.Split(',').Contains(asm.UserName)).ToList();

                return activitySummary;
            }
            catch (Exception ex)
            {
                return activitySummary;
            }
        }

        public IEnumerable<CodeMetricHistory> GetCodeMetricsHistorySimple(int? take, int? skip)
        {
            string query = @"SELECT cmh.Id, cmh.MetricsCalculationMoment, cmh.MetricsCalculationMachine, cmh.RevisionNumber, 
                           r.RevisionMessage, u.UserName AS RevisionAuthor, cmh.ProjectGroup, cmh.ProjectName,
                           cmh.MeasurementTool, mv.MetricName, mv.MetricValue 
                           FROM CodeMetricsHistory cmh 
                           LEFT JOIN MetricsValues mv ON cmh.Id = mv.CodeMetricsHistory_Id 
                           LEFT JOIN Revisions r ON cmh.RevisionNumber = r.RevisionNumber 
						   LEFT JOIN Users u ON r.UserId = u.Id 
                           WHERE MetricName IS NOT NULL AND ProjectName <> '' 
                           ORDER BY cmh.Id desc";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (take.HasValue)
            {
                query += @"OFFSET @Skip ROWS
                           FETCH NEXT @Take ROWS ONLY;";
                parameters.Add("@Skip", skip);
                parameters.Add("@Take", take);
            }

            DataTable dt = dbu.ExecuteDataTable(query, parameters.Count > 0 ? parameters : null);

            return dt.AsEnumerable().Select(r => new CodeMetricHistory()
            {
                Id = Convert.ToInt64(r["Id"]),
                CalculationMoment = Convert.ToDateTime(r["MetricsCalculationMoment"]),
                CalculationMachine = r["MetricsCalculationMachine"] == DBNull.Value ? null : (string)r["MetricsCalculationMachine"],
                ProjectGroup = r["ProjectGroup"] == DBNull.Value ? null : (string)r["ProjectGroup"],
                RevisionNumber = r["RevisionNumber"] == DBNull.Value ? null : (string)r["RevisionNumber"],
                RevisionAuthor = r["RevisionAuthor"] == DBNull.Value ? null : (string)r["RevisionAuthor"],
                ProjectName = r["ProjectName"] == DBNull.Value ? null : (string)r["ProjectName"],
                MeasurementTool = r["MeasurementTool"] == DBNull.Value ? null : (string)r["MeasurementTool"],
                MetricName = r["MetricName"] == DBNull.Value ? null : (string)r["MetricName"],
                MetricValue = r["MetricValue"] == DBNull.Value ? 0 : Convert.ToDouble(r["MetricValue"]),
                RevisionMessage = r["RevisionMessage"] == DBNull.Value ? null : (string)r["RevisionMessage"]
            });
        }

        public int GetCodeMetricsHistoryCount()
        {
            string query = @"SELECT COUNT(cmh.Id) 
                           FROM CodeMetricsHistory cmh 
                           LEFT JOIN MetricsValues mv ON cmh.Id = mv.CodeMetricsHistory_Id 
                           LEFT JOIN Revisions r ON cmh.RevisionNumber = r.RevisionNumber 
						   LEFT JOIN Users u ON r.UserId = u.Id 
                           WHERE MetricName IS NOT NULL AND ProjectName <> ''";
            int count = 0;

            dbu.ExecuteRead(query,
                                null, delegate(SqlDataReader sdr)
                                {
                                    count = sdr.GetInt32(0);
                                }, -1);

            return count;
        }

        public CodeMetricsHistoryExpandedModel GetCodeMetricsHistoryExpanded()
        {
            CodeMetricsHistoryExpandedModel codeMetricsHistoryModel = new CodeMetricsHistoryExpandedModel();
            codeMetricsHistoryModel.MetricNames = new List<string>();

            string query = @"GetMetricHistory";

            DataTable dt = dbu.ExecuteDataTable(query, null, true);

            foreach (DataColumn item in dt.Columns)
            {
                if (item.ColumnName != "MetricsCalculationMoment" && item.ColumnName != "RevisionNumber" && item.ColumnName != "RevisionMessage"
                    && item.ColumnName != "ProjectName" && item.ColumnName != "UserName")
                    codeMetricsHistoryModel.MetricNames.Add(item.ColumnName);
            }

            codeMetricsHistoryModel.MetricsHistoryData = dt.AsEnumerable().Select(r =>
            {
                List<double> metricsValues = new List<double>();
                CodeMetricsHistoryData model = new CodeMetricsHistoryData()
                {
                    CalculationMoment = Convert.ToDateTime(r["MetricsCalculationMoment"]),
                    RevisionNumber = r["RevisionNumber"] == DBNull.Value ? null : (string)r["RevisionNumber"],
                    RevisionMessage = r["RevisionMessage"] == DBNull.Value ? null : (string)r["RevisionMessage"],
                    ProjectName = r["ProjectName"] == DBNull.Value ? null : (string)r["ProjectName"],
                    UserName = r["UserName"] == DBNull.Value ? null : (string)r["UserName"]
                };

                foreach (string item in codeMetricsHistoryModel.MetricNames)
                    metricsValues.Add(r[item] == DBNull.Value ? 0 : Convert.ToDouble(r[item]));

                model.MetricValues = metricsValues;

                return model;
            }).ToList();

            return codeMetricsHistoryModel;
        }
    }
}

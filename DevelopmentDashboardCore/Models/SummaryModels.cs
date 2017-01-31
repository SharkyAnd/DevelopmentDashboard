using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopmentDashboardCore.Models
{
    public class ActivitySummaryModel
    {
        public string UserName { get; set; }
        public DateTime? Moment { get; set; }
        public double ActiveHours { get; set; }
        public double LinearHours { get; set; }
        public double SessionsCount { get; set; }
        public double CommitsCount { get; set; }
        public double PublicationsCount { get; set; }
        public double LocalBuildsCount { get; set; }
    }

    public class UsersByGroup
    {
        public string key { get; set; }
        public string[] items { get; set; }
    }

    public class CodeMetricsHistoryExpandedModel
    {
        public List<CodeMetricsHistoryData> MetricsHistoryData { get; set; }
        public List<string> MetricNames { get; set; }
    }

    public class CodeMetricsHistoryData
    {
        public DateTime? CalculationMoment { get; set; }
        public string RevisionNumber { get; set; }
        public string ProjectName { get; set; }
        public string UserName { get; set; }
        public string RevisionMessage { get; set; }
        public List<double> MetricValues { get; set; }
    }

    public class CodeMetricHistory
    {
        public long Id { get; set; }
        public DateTime? CalculationMoment { get; set; }
        public string CalculationMachine { get; set; }
        public string RevisionNumber { get; set; }
        public string RevisionAuthor { get; set; }
        public string ProjectGroup { get; set; }
        public string ProjectName { get; set; }
        public string MeasurementTool { get; set; }
        public string MetricName { get; set; }
        public double MetricValue { get; set; }
        public string RevisionMessage { get; set; }
    }
}

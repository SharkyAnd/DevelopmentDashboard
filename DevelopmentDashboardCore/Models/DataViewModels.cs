using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevelopmentDashboardCore.WebConfigurations;

namespace DevelopmentDashboardCore.Models
{
    public class LocalBuild
    {
        public int Id { get; set; }
        public DateTime? Moment { get; set; }
        public string UserName { get; set; }
        public string MachineName { get; set; }
        public string Project { get; set; }
        public string RevisionNumber { get; set; }
        public int ErrorsCount { get; set; }
        public string Comment { get; set; }
        public string BuildPath { get; set; }
        public string RevisionAuthor { get; set; }
        public string RevisionMessage { get; set; }
    }

    public class Publication
    {
        public int Id { get; set; }
        public DateTime? Moment { get; set; }
        public string UserName { get; set; }
        public string SourceMachine { get; set; }
        public string TargetMachine { get; set; }
        public string Project { get; set; }
        public string Url { get; set; }
        public string Comment { get; set; }
        public string ProjectFullPath { get; set; }
        public string RevisionNumber { get; set; }
        public string RevisionAuthor { get; set; }
        public string RevisionMessage { get; set; }
    }

    public class Revision
    {
        public int Id { get; set; }
        public string RepositoryName { get; set; }
        public DateTime Moment { get; set; }
        public string RevisionNumber { get; set; }
        public string RevisionMessage { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class Session
    {

        public int Id { get; set; }
        public string UserName { get; set; }
        public string MachineName { get; set; }
        public DateTime? SessionBegin { get; set; }
        public DateTime? SessionEnd { get; set; }
        public DateTime? LastInputTime { get; set; }
        public double ActiveHours { get; set; }
        public string SessionState { get; set; }
        public double UtcOffset { get; set; }
        public string ClientName { get; set; }
        public string ClientDisplayDetails { get; set; }
        public string ClientIPAddress { get; set; }
        public int ClientBuildNumber { get; set; }
        public double LinearHours { get; set; }
        public double ActiveTime { get; set; }
    }

    public class ActiveSession : Session
    {
        public string UserActivity { get; set; }

        public ActiveSession(Session session)
        {
            this.Id = session.Id;
            this.UserName = session.UserName;
            this.MachineName = session.MachineName;
            this.SessionBegin = session.SessionBegin;
            this.SessionEnd = session.SessionEnd;
            this.LastInputTime = session.LastInputTime;
            this.ActiveHours = session.ActiveHours;
            this.SessionState = session.SessionState;
            this.UtcOffset = session.UtcOffset;
            this.ClientName = session.ClientName;
            this.ClientDisplayDetails = session.ClientDisplayDetails;
            this.ClientIPAddress = session.ClientIPAddress;
            this.ClientBuildNumber = session.ClientBuildNumber;
            this.LinearHours = session.LinearHours;
            this.ActiveTime = session.ActiveTime;
            this.UserActivity = session.LastInputTime == null || (
                AppData.DateTimeConvertUtils.
                ConvertTimeByUtcOffset(DateTime.Now, session.UtcOffset).Value - session.LastInputTime.Value.AddHours(session.UtcOffset)).
                TotalMinutes > DDWebConfig.Instance.CheckActiveStatusTimeInMinutes ? "Not active now" : "ActiveNow";
        }
    }
}

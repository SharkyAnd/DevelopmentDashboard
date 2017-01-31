using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopmentDashboardCore.Models
{
    public class MainPageUser
    {
        public string UserName { get; set; }
        public double DayActiveHours { get; set; }
        public double WeekActiveHours { get; set; }
        public DateTime? LastInputTime { get; set; }
        public int LastInputTimeUtcOffset { get; set; }
        public int WeekCommitsCount { get; set; }
        public string Status { get; set; }
    }

    public class LastSession
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public DateTime? SessionBegin { get; set; }
        public DateTime? SessionEnd { get; set; }
        public double ActiveHours { get; set; }
        public DateTime? LastInputTime { get; set; }
        public double ClientUtcOffset { get; set; }
        public string SessionState { get; set; }
        public string Status { get; set; }
    }

    public class LastCommit
    {
        public DateTime? Moment { get; set; }
        public string UserName { get; set; }
        public string RevisionNumber { get; set; }
        public string RevisionMessage { get; set; }
    }
}

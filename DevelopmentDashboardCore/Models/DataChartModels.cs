using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopmentDashboardCore.Models
{
    public class ConnectedAndActiveSessionModel
    {
        public double ConnectedSessions { get; set; }
        public double ActiveSessions { get; set; }
        public DateTime Moment { get; set; }
    }

    public class UserActiveHours
    {
        public string UserName { get; set; }
        public DateTime SessionBegin { get; set; }
        public DateTime SessionEnd { get; set; }
        public double ActiveHours { get; set; }
    }

    public class MomentSessionProfile
    {
        public string UserId { get; set; }
        public int SessionsCount { get; set; }
        public DateTime Moment { get; set; }
    }

    public class SessionActivityProfile
    {
        public string SessionId { get; set; }
        public DateTime ChunkPoint { get; set; }
        public int? Position { get; set; }
        public string UserName { get; set; }
    }
}

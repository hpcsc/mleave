using MLeave.Models;
using System.Collections.Generic;

namespace MLeave.ViewModels
{
    public class OverviewModel
    {
        public List<UpcomingLeaves> UpcomingLeaves { get; set; }
        public List<LeaveEntitlement> LeaveEntitlement { get; set; }

        public List<Leave> Leaves { get; set; }
        public User User { get; set; }

        public OverviewModel()
        {
            UpcomingLeaves = new List<UpcomingLeaves>();
            LeaveEntitlement = new List<LeaveEntitlement>();
        }
    }

    public class UpcomingLeaves
    {
        public string Type { get; set; }
        public string Reason { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }

    public class LeaveEntitlement
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public double Left { get; set; }
        public double Accepted { get; set; }
        public double Pending { get; set; }
    }
}

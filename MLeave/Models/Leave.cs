using System;

namespace MLeave.Models
{
    public class Leave
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string LeaveType { get; set; }
        public int Hours { get; set; }
        public string Reason { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsHalfDay { get; set; }
        public bool? IsOnMorning { get; set; }
        public DateTime? Date { get; set; }
        public string CreatedById { get; set; }
    }
}

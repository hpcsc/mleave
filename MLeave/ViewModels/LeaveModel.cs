
namespace MLeave.ViewModels
{
    public class LeaveModel
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string LeaveType { get; set; }
        public int Hours { get; set; }
        public string Reason { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsHalfDay { get; set; }
        public bool? IsOnMorning { get; set; }
        public string Date { get; set; }
        public string CreatedById { get; set; }
        public string CreatedByName { get; set; }
        public string CreatedByProfileImage { get; set; }
    }
}

using System.Collections.Generic;

namespace MLeave.ViewModels
{
    public class ApplyLeaveInputModel
    {
        public string Email { get; set; }
        public List<string> Dates { get; set; }
        public string LeaveCodeName { get; set; }
        public string Reason { get; set; }
    }
}

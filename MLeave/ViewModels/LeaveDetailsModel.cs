using System;
using System.Collections.Generic;

namespace MLeave.ViewModels
{
    public class LeaveDetailsModel
    {
        public DateTime Date { get; set; }
        public List<LeaveModel> Leaves { get; set; }
        public List<HolidayModel> Holidays { get; set; }
    }
}

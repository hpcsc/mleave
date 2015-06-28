using MLeave.Data;
using MLeave.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MLeave.Controllers
{
    public class UserController : ControllerBase
    {
        // GET: User
        public async Task<ActionResult> Projects(string email)
        {
            var userRepository = new UserRepository();
            var user = await userRepository.FindByEmail(email);

            var projectRepository = new ProjectRepository();
            var projects = await projectRepository.FindProjectsForUser(user.Id);

            return JsonNet(projects);
        }

        public async Task<ActionResult> Leaves(string email, int year, int month)
        {
            var userRepository = new UserRepository();
            var user = await userRepository.FindByEmail(email);

            var scheduleRepository = new ScheduleRepository();
            var leaves = await scheduleRepository.FindLeavesByMonth(user.Id, year, month);

            return JsonNet(leaves);
        }

        private static readonly CultureInfo _cultureInfo = new CultureInfo("en-us");
        [HttpPost]
        public async Task<ActionResult> Apply(ApplyLeaveInputModel input)
        {
            if(input.Dates == null || !input.Dates.Any())
            {
                return Fail("No dates specified");
            }

            var dates = new List<DateTime>();
            string dateParseError = string.Empty;
            input.Dates.ForEach(d =>
            {
                DateTime parsedDate;
                if (!DateTime.TryParseExact(d, "dd/MM/yyyy", _cultureInfo, DateTimeStyles.AssumeUniversal, out parsedDate))
                {
                    dateParseError = "Invalid date format, expected date format is dd/MM/yyyy";
                }
                else
                {
                    dates.Add(parsedDate);
                }
            });

            if(!string.IsNullOrWhiteSpace(dateParseError))
            {
                return Fail(dateParseError);
            }

            if(dates.Any(d => d < DateTime.Now))
            {
                return Fail("One or more dates are in the past and are invalid");
            }

            if(string.IsNullOrWhiteSpace(input.Email))
            {
                return Fail("Email is required");
            }

            var userRepository = new UserRepository();
            var user = await userRepository.FindByEmail(input.Email);
            if(user == null)
            {
                return Fail("No user with that email found");
            }

            
            var scheduleRepository = new ScheduleRepository();
            string duplicateError = string.Empty;

            foreach (var d in dates)
            {
                var existingLeaves = await scheduleRepository.FindLeavesByMonth(user.Id, d.Year, d.Month);
                if (existingLeaves != null && existingLeaves.Any(l =>
                    l.Date.Value.Year == d.Year &&
                    l.Date.Value.Month == d.Month &&
                    l.Date.Value.Day == d.Day))
                {
                    duplicateError = "One or more dates are already applied before";
                    break;
                }
            }

            if(!string.IsNullOrWhiteSpace(duplicateError))
            {
                return Fail(duplicateError);
            }

            var leaveTypeRepository = new LeaveTypeRepository();
            var leaveType = await leaveTypeRepository.FindLeaveTypeByCodeName(input.LeaveCodeName);
            if(string.IsNullOrWhiteSpace(input.LeaveCodeName) || leaveType == null)
            {
                return Fail("Invalid leave code name");
            }

            if(string.IsNullOrWhiteSpace(input.Reason))
            {
                return Fail("Reason is required");
            }

            try
            {
                await scheduleRepository.ApplyLeave(user.Id, user.Profile.Name, dates, input.LeaveCodeName, input.Reason);
            }
            catch(Exception ex)
            {
                return Fail("Server error: " + ex.ToString());
            }

            return JsonNet(new {
                Success = true
            });
        }

        private ActionResult Fail(string error)
        {
            return JsonNet(new
            {
                Success = false,
                Error = error
            });
        }
    }
}
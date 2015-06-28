using MLeave.Data;
using MLeave.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MLeave.Controllers
{
    public class ProjectController : ControllerBase
    {
        // GET: Project
        public async Task<ActionResult> Index(string id, int year, int month)
        {
            var projectRepository = new ProjectRepository();
            var memberIds = await projectRepository.FindMembersInProject(id);

            var scheduleRepository = new ScheduleRepository();
            var leaves = await scheduleRepository.FindLeavesByMonthForProjectMembers(memberIds, year, month);
            var leaveCount = new Dictionary<DateTime, ProjectLeaveCountModel>();
            leaves.ForEach(l =>
                {
                    if (!leaveCount.ContainsKey(l.Date.Value))
                    {
                        leaveCount[l.Date.Value] = new ProjectLeaveCountModel
                            {
                                Date = l.Date.Value
                            };
                    }

                    leaveCount[l.Date.Value].Count++;
                });

            var userRepository = new UserRepository();
            var countryCodes = await userRepository.FindDistinctCountryCodeForMembers(memberIds);
            var holidayRepository = new HolidayRepository();
            var holidays = await holidayRepository.FindHolidaysForCountries(year, countryCodes);

            holidays.ForEach(h =>
            {
                h.Date = DateTime.SpecifyKind(h.Date, DateTimeKind.Utc);

                if (h.Date.Month == month)
                {
                    if (!leaveCount.ContainsKey(h.Date))
                    {
                        leaveCount[h.Date] = new ProjectLeaveCountModel
                        {
                            Date = h.Date
                        };
                    }

                    leaveCount[h.Date].Count++;
                }
            });

            return JsonNet(leaveCount.Values);
        }

        public async Task<ActionResult> LeaveDetails(string id, int year, int month, int day)
        {
            var projectRepository = new ProjectRepository();
            var memberIds = await projectRepository.FindMembersInProject(id);

            var scheduleRepository = new ScheduleRepository();
            var leaves = await scheduleRepository.FindLeavesByMonthForProjectMembers(memberIds, year, month);

            var leaveTypeRepository = new LeaveTypeRepository();
            var leaveTypes = await leaveTypeRepository.FindAll();

            var model = new LeaveDetailsModel();
            model.Date = new DateTime(year, month, day);
            model.Leaves = leaves.Where(l => l.Date.Value.Day == day)
                                 .Select(l => new LeaveModel
                                    {
                                        CreatedById = l.CreatedById,
                                        Date = l.Date.Value.ToString("dd MMMM yyyy"),
                                        Hours = l.Hours,
                                        Id = l.Id,
                                        IsApproved = l.IsApproved,
                                        IsHalfDay = l.IsHalfDay,
                                        IsOnMorning = l.IsOnMorning,
                                        LeaveType = l.LeaveType,
                                        Reason = l.Reason,
                                        Type = LookUpLeaveName(l.LeaveType, leaveTypes)
                                    })
                                    .ToList();

            var userRepository = new UserRepository();
            var users = await userRepository.FindByIds(memberIds);
            var userLookUp = users.ToDictionary(u => u.Id);
            model.Leaves.ForEach(l => {
                if(userLookUp.ContainsKey(l.CreatedById))
                {
                    var user = userLookUp[l.CreatedById];
                    l.CreatedByName = user.Profile.Name;
                    l.CreatedByProfileImage = user.GoogleProfile.ProfileImageUrl;
                }
            });

            var countryCodes = await userRepository.FindDistinctCountryCodeForMembers(memberIds);
            var holidayRepository = new HolidayRepository();
            var holidays = await holidayRepository.FindHolidaysForCountries(year, countryCodes);

            model.Holidays = holidays.Where(h => h.Date.Month == month && h.Date.Day == day)
                .Select(h => new HolidayModel
                {
                    Country = h.Country,
                    Name = h.Name,
                    Date = h.Date.ToString("dd MMMM yyyy")
                }).ToList();
            var usersByCountry = users.GroupBy(u => u.Profile.CountryCode);
            foreach (var byCountry in usersByCountry)
            {
                if(byCountry.Count() > 1)
                {
                    var holiday = model.Holidays.FirstOrDefault(h => h.Country == byCountry.Key);
                    if(holiday != null)
                    {
                        model.Holidays.AddRange(Enumerable.Repeat(holiday, byCountry.Count() - 1));
                    }
                }
            }

            return JsonNet(model);
        }

        private string LookUpLeaveName(string codeName, List<Models.LeaveType> leaveTypes)
        {
            var leaveType = leaveTypes.FirstOrDefault(l => l.LeaveCodeName == codeName);
            return leaveType == null ? string.Empty : leaveType.Name;
        }
    }
}
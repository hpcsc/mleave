using MLeave.Data;
using MLeave.Models;
using MLeave.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MLeave.Controllers
{
    public class OverviewController : ControllerBase
    {
        // GET: Overview
        public async Task<ActionResult> Index(string email)
        {
            var userRepository = new UserRepository();
            var user = await userRepository.FindByEmail(email);

            var scheduleRepository = new ScheduleRepository();
            var leaves = await scheduleRepository.FindAllLeaves(user.Id);

            var leaveTypeRepository = new LeaveTypeRepository();
            var allLeaveTypes = await leaveTypeRepository.FindAll();
            var leaveTypesLookUp = allLeaveTypes.ToDictionary(l => l.LeaveCodeName);

            var model = new OverviewModel
            {
                Leaves = leaves,
                User = user
            };

            model.LeaveEntitlement.Add(GetLeaveEntitlement(user, leaves, leaveTypesLookUp["annual_leave"], user.LeaveQuotas.AnnualLeave));
            model.LeaveEntitlement.Add(GetLeaveEntitlement(user, leaves, leaveTypesLookUp["sick_leave"], user.LeaveQuotas.SickLeave));
            model.LeaveEntitlement.Add(GetLeaveEntitlement(user, leaves, leaveTypesLookUp["training_leave"], user.LeaveQuotas.TrainingLeave));
            model.LeaveEntitlement.Add(GetLeaveEntitlement(user, leaves, leaveTypesLookUp["compassionate_leave"], user.LeaveQuotas.CompassionateLeave));
            model.LeaveEntitlement.Add(GetLeaveEntitlement(user, leaves, leaveTypesLookUp["maternity_leave"], user.LeaveQuotas.MaternityLeave));
            model.LeaveEntitlement.Add(GetLeaveEntitlement(user, leaves, leaveTypesLookUp["paternity_leave"], user.LeaveQuotas.PaternityLeave));
            model.LeaveEntitlement.Add(GetLeaveEntitlement(user, leaves, leaveTypesLookUp["child_care_leave"], user.LeaveQuotas.ChildCareLeave));
            model.LeaveEntitlement.Add(GetLeaveEntitlement(user, leaves, leaveTypesLookUp["extended_child_care_leave"], user.LeaveQuotas.ExtendedChildCareLeave));
            model.LeaveEntitlement.Add(GetLeaveEntitlement(user, leaves, leaveTypesLookUp["ns_leave"], user.LeaveQuotas.NSLeave));

            var upcomingLeaves = leaves.Where(l => l.Date > DateTime.Now).OrderBy(l => l.Date).ToList();
            //var upcomingLeaves = leaves.OrderBy(l => l.Date).ToList();

            var groupedUpcomingLeaves = upcomingLeaves.GroupBy(l => l.LeaveType);
            foreach (var group in groupedUpcomingLeaves)
            {
                var groupLeaves = group.ToList();
                var currentUpcoming = new UpcomingLeaves { Type = group.Key };
                DateTime currentDate = groupLeaves[0].Date.Value;
                currentUpcoming.From = FormatDate(currentDate);
                currentUpcoming.Reason = groupLeaves[0].Reason;

                if (groupLeaves.Count == 1)
                {
                    currentUpcoming.To = FormatDate(currentDate);
                    model.UpcomingLeaves.Add(currentUpcoming);
                }
                else
                {
                    int nextLeaveIndex = 1;
                    string reason = groupLeaves[nextLeaveIndex].Reason;
                    DateTime nextLeave = groupLeaves[nextLeaveIndex++].Date.Value;
                    
                    while (nextLeaveIndex <= groupLeaves.Count)
                    {
                        while (nextLeaveIndex <= groupLeaves.Count &&
                            (currentDate.AddDays(1) == nextLeave || 
                            currentDate.AddDays(1).DayOfWeek == DayOfWeek.Saturday || 
                            currentDate.AddDays(1).DayOfWeek == DayOfWeek.Sunday))
                        {
                            if (nextLeaveIndex < groupLeaves.Count && 
                                currentDate.AddDays(1) == nextLeave)
                            {
                                nextLeave = groupLeaves[nextLeaveIndex++].Date.Value;
                            }
                            currentDate = currentDate.AddDays(1);
                        }

                        if (currentDate != nextLeave)
                        {
                            currentUpcoming.To = FormatDate(currentDate);
                            model.UpcomingLeaves.Add(currentUpcoming);

                            currentDate = nextLeave;
                            currentUpcoming = new UpcomingLeaves
                            {
                                Type = group.Key,
                                From = FormatDate(currentDate),
                                Reason = reason
                            };
                        }

                        if (nextLeaveIndex < groupLeaves.Count)
                        {
                            reason = groupLeaves[nextLeaveIndex].Reason;
                            nextLeave = groupLeaves[nextLeaveIndex].Date.Value;
                            
                        }

                        nextLeaveIndex++;
                    }


                    if (currentUpcoming != null)
                    {
                        currentUpcoming.To = FormatDate(nextLeave);
                        model.UpcomingLeaves.Add(currentUpcoming);
                    }

                }
            }

            //upcomingLeaves.ForEach(l => model.UpcomingLeaves.Add(new UpcomingLeaves
            //    {
            //        Type = l.LeaveType,
            //        From = FormatDate(l.Date.Value),
            //        To = FormatDate(l.Date.Value)
            //    }));

            return JsonNet(model);
        }

        private string FormatDate(DateTime date)
        {
            return date.ToString("dd MMMM yyyy");
        }

        private static LeaveEntitlement GetLeaveEntitlement(User user, List<Leave> leaves, LeaveType leaveType, double taken)
        {
            var accepted = leaves.Where(l => l.LeaveType == leaveType.LeaveCodeName && !l.IsApproved.HasValue || l.IsApproved.Value).ToList();

            return new LeaveEntitlement
            {
                Name = leaveType.Name,
                Type = leaveType.LeaveCodeName,
                Accepted = leaves.Where(l => l.LeaveType == leaveType.LeaveCodeName && (!l.IsApproved.HasValue || l.IsApproved.Value)).Sum(l => l.Hours) / 8.0,
                Pending = leaves.Where(l => l.LeaveType == leaveType.LeaveCodeName && l.IsApproved.HasValue && !l.IsApproved.Value && l.Date > DateTime.Now).Sum(l => l.Hours) / 8.0,
                Left = leaveType.Value - taken
            };
        }
    }
}
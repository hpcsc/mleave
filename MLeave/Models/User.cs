
using System;
using System.Collections.Generic;
namespace MLeave.Models
{
    public class User
    {
        public string Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public UserProfile Profile { get; set; }

        public UserProfileLeaveQuotas LeaveQuotas { get; set; }

        public GoogleProfile GoogleProfile { get; set; }
    }

    public class UserProfile
    {
        public string CountryCode { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public List<Role> Roles { get; set; }
    }

    public class Role
    {
        public string Type { get; set; }
        public string Position { get; set; }
    }

    public class GoogleProfile
    {
        public string ProfileImageUrl { get; set; }
    }

    public class UserProfileLeaveQuotas
    {
        public double AnnualLeave { get; set; }
        public double SickLeave { get; set; }
        public double TrainingLeave { get; set; }
        public double CompassionateLeave { get; set; }
        public double MaternityLeave { get; set; }
        public double PaternityLeave { get; set; }
        public double ChildCareLeave { get; set; }
        public double ExtendedChildCareLeave { get; set; }
        public double NSLeave { get; set; }
    }
}

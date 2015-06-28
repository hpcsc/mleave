using MLeave.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MLeave.Data
{
    public class UserRepository : RepositoryBase
    {
        public async Task<List<User>> FindAll()
        {
            var collection = GetBsonCollection("users");

            var users = await collection.Find(new BsonDocument()).Project(b => ParseUser(b)).ToListAsync();

            return users;
        }

        public async Task<User> FindByEmail(string email)
        {
            var collection = GetBsonCollection("users");
            var filter = Builders<BsonDocument>.Filter.Eq("profile.email", email);

            return await collection.Find(filter)
                .Project(b => ParseUser(b))
                .FirstOrDefaultAsync();
        }

        public async Task<List<User>> FindByIds(List<string> memberIds)
        {
            var collection = GetBsonCollection("users");
            var filter = Builders<BsonDocument>.Filter.In("_id", memberIds);

            var users = await collection.Find(filter)
                .Project(b => ParseUser(b))
                .ToListAsync();

            return users;
        }

        public async Task<List<string>> FindDistinctCountryCodeForMembers(List<string> memberIds)
        {
            var collection = GetBsonCollection("users");
            var filter = Builders<BsonDocument>.Filter.In("_id", memberIds);

            var users = await collection.Find(filter)
                .Project(b => ParseUser(b))
                .ToListAsync();

            return users.Where(u => u.Profile != null)
                        .Select(u => u.Profile.CountryCode)
                        .Distinct()
                        .ToList();
        }

        private User ParseUser(BsonDocument b)
        {
            return new User
                {
                    Id = b["_id"].ToString(),
                    Profile = new UserProfile
                    {
                        Name = GetStringValue(b["profile"], "name"),
                        Email = GetStringValue(b["profile"], "email"),
                        CountryCode = GetStringValue(b["profile"], "countryCode"),
                        Roles = GetRoles(b["profile"])
                    },
                    LeaveQuotas = GetLeaveQuotats(b, DateTime.Now.Year.ToString()),
                    GoogleProfile = new GoogleProfile
                    {
                        ProfileImageUrl = GetProfileImage(b)
                    }
                };
        }

        private string GetProfileImage(BsonDocument document)
        {
            if(document == null || !document.Contains("services"))
            {
                return null;
            }

            var services = document["services"].AsBsonDocument;
            if(services == null || !services.Contains("google"))
            {
                return null;
            }

            var google = services["google"].AsBsonDocument;
            return GetStringValue(google, "picture");
        }

        private UserProfileLeaveQuotas GetLeaveQuotats(BsonDocument source, string year)
        {
            if(source == null || !source.Contains("leave_quotas"))
            {
                return null;
            }

            var leave_quotas = source["leave_quotas"].AsBsonDocument;
            if(leave_quotas == null || !leave_quotas.Contains(year))
            {
                return null;
            }

            var quotas = new UserProfileLeaveQuotas();
            quotas.AnnualLeave = GetDoubleValue(leave_quotas[year], "annual_leave");
            quotas.SickLeave = GetDoubleValue(leave_quotas[year], "sick_leave");
            quotas.MaternityLeave = GetDoubleValue(leave_quotas[year], "maternity_leave");
            quotas.PaternityLeave = GetDoubleValue(leave_quotas[year], "paternity_leave");
            quotas.ChildCareLeave = GetDoubleValue(leave_quotas[year], "child_care_leave");
            quotas.ExtendedChildCareLeave = GetDoubleValue(leave_quotas[year], "extended_child_care_leave");
            quotas.TrainingLeave = GetDoubleValue(leave_quotas[year], "training_leave");
            quotas.CompassionateLeave = GetDoubleValue(leave_quotas[year], "compassionate_leave");
            quotas.NSLeave = GetDoubleValue(leave_quotas[year], "ns_leave");

            return quotas;
        }

        private List<Role> GetRoles(BsonValue source)
        {
            var profile = source.AsBsonDocument;
            if(profile == null || !profile.Contains("roles"))
            {
                return null;
            }

            var array = profile["roles"].AsBsonArray;

            return array.Select(p => new Role
                                {
                                    Position = p.ToString()
                                }).ToList();
        }
    }
}

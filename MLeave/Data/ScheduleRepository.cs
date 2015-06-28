
using MLeave.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MLeave.Data
{
    public class ScheduleRepository : RepositoryBase
    {
        public async Task<List<Leave>> FindAllLeaves(string userId)
        {
            var start = new DateTime(DateTime.Now.Year, 1, 1);
            var end = start.AddYears(1);

            var collection = GetBsonCollection("schedules");
            var filters = new List<FilterDefinition<BsonDocument>>();
            filters.Add(Builders<BsonDocument>.Filter.Eq("createBy.id", userId));
            filters.Add(Builders<BsonDocument>.Filter.Eq("tag", "Leave"));
            filters.Add(Builders<BsonDocument>.Filter.Gte("date", start));
            filters.Add(Builders<BsonDocument>.Filter.Lte("date", end));
            //filters.Add(Builders<BsonDocument>.Filter.Gte("date", DateTime.Now.AddMonths(-3)));
            var filter = Builders<BsonDocument>.Filter.And(filters);

            var sort = Builders<BsonDocument>.Sort.Descending("date");

            return await collection.Find(filter)
                .Sort(sort)
                .Project(b => ParseLeave(b))
                .ToListAsync();
        }

        public async Task<List<Leave>> FindLeavesByMonth(string userId, int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddDays(DateTime.DaysInMonth(year, month));

            var collection = GetBsonCollection("schedules");
            var filters = new List<FilterDefinition<BsonDocument>>();
            filters.Add(Builders<BsonDocument>.Filter.Eq("createBy.id", userId));
            filters.Add(Builders<BsonDocument>.Filter.Eq("tag", "Leave"));
            filters.Add(Builders<BsonDocument>.Filter.Gte("date", start));
            filters.Add(Builders<BsonDocument>.Filter.Lte("date", end));
            var filter = Builders<BsonDocument>.Filter.And(filters);

            //var sort = Builders<BsonDocument>.Sort.Descending("date");

            return await collection.Find(filter)
                //.Sort(sort)
                .Project(b => ParseLeave(b))
                .ToListAsync();
        }

        public async Task<List<Leave>> FindLeavesByMonthForProjectMembers(List<string> memberIds, int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddDays(DateTime.DaysInMonth(year, month));

            var collection = GetBsonCollection("schedules");
            var filters = new List<FilterDefinition<BsonDocument>>();
            filters.Add(Builders<BsonDocument>.Filter.In("createBy.id", memberIds));
            filters.Add(Builders<BsonDocument>.Filter.Eq("tag", "Leave"));
            filters.Add(Builders<BsonDocument>.Filter.Gte("date", start));
            filters.Add(Builders<BsonDocument>.Filter.Lte("date", end));
            var filter = Builders<BsonDocument>.Filter.And(filters);

            return await collection.Find(filter)
                .Project(b => ParseLeave(b))
                .ToListAsync();
        }

        public async Task ApplyLeave(string userId, string userName, List<DateTime> dates, string leaveCodeName, string reason)
        {
            var collection = GetBsonCollection("schedules");

            await collection.InsertManyAsync(dates.Select(d => CreateBsonDocument(userId, userName, d, leaveCodeName, reason)));
        }

        private BsonDocument CreateBsonDocument(string userId, string userName, DateTime date, string leaveCodeName, string reason)
        {
            var leave = new BsonDocument();

            leave.Add("_id", BsonObjectId.GenerateNewId().ToString());
            leave.Add("tag", "Leave");
            leave.Add("type", "leave");

            var createdBy = new BsonDocument();
            createdBy.Add("id", userId);
            createdBy.Add("name", userName);

            leave.Add("createBy", createdBy);
            leave.Add("engineer", createdBy);

            leave.Add("hours", 8);
            leave.Add("reason", reason);
            leave.Add("leave_type", leaveCodeName);
            leave.Add("isApproved", false);
            leave.Add("date", date);
            leave.Add("created_on", DateTime.Now);

            return leave;
        }

        private Leave ParseLeave(BsonDocument document)
        {
            return new Leave
            {
                Id = GetStringValue(document, "_id"),
                Type = GetStringValue(document, "type"),
                LeaveType = GetStringValue(document, "leave_type"),
                Hours = GetIntValue(document, "hours"),
                Reason = GetStringValue(document, "reason"),
                IsApproved = GetBoolValue(document, "isApproved"),
                IsHalfDay = GetBoolValue(document, "halfDay"),
                IsOnMorning = GetBoolValue(document, "isOnMorning"),
                Date = GetDateTimeValue(document, "date"),
                CreatedById = GetCreatedById(document)
            };
        }

        private string GetCreatedById(BsonDocument document)
        {
            if(document == null || !document.Contains("createBy"))
            {
                return string.Empty;
            }

            var createdBy = document["createBy"].AsBsonDocument;
            return GetStringValue(createdBy, "id");
        }
    }
}

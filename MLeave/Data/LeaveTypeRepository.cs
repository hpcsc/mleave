using MLeave.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MLeave.Data
{
    public class LeaveTypeRepository : RepositoryBase
    {
        public async Task<List<LeaveType>> FindAll()
        {
            var collection = GetBsonCollection("leave_types");

            return await collection.Find(new BsonDocument())
                .Project(b => ParseLeaveType(b))
                .ToListAsync();
        }

        public async Task<LeaveType> FindLeaveTypeByCodeName(string leaveCodeName)
        {
            var collection = GetBsonCollection("leave_types");
            var filters = new List<FilterDefinition<BsonDocument>>();
            filters.Add(Builders<BsonDocument>.Filter.Eq("leave_code_name", leaveCodeName));
            var filter = Builders<BsonDocument>.Filter.And(filters);

            var leaveType = await collection.Find(filter).FirstOrDefaultAsync();

            return leaveType == null ? null : ParseLeaveType(leaveType);
        }

        private LeaveType ParseLeaveType(BsonDocument document)
        {
            return new LeaveType
            {
                Name = GetStringValue(document, "name"),
                LeaveCodeName = GetStringValue(document, "leave_code_name"),
                Value = double.Parse(GetStringOrDoubleValue(document, "value"))
            };
        }
    }
}

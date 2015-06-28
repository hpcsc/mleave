using MLeave.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MLeave.Data
{
    public class ProjectRepository : RepositoryBase
    {
        public async Task<List<Project>> FindProjectsForUser(string userId)
        {
            var collection = GetBsonCollection("projects");
            var filters = new List<FilterDefinition<BsonDocument>>();
            filters.Add(Builders<BsonDocument>.Filter.Eq("teamMemberIds.userId", userId));
            filters.Add(Builders<BsonDocument>.Filter.Eq("isClosed", false));
            var filter = Builders<BsonDocument>.Filter.And(filters);

            var sort = Builders<BsonDocument>.Sort.Descending("_id");

            return await collection.Find(filter)
                .Sort(sort)
                .Limit(20)
                .Project(b => ParseProject(b))
                .ToListAsync();
        }

        public async Task<List<string>> FindMembersInProject(string projectId)
        {
            var collection = GetBsonCollection("projects");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", projectId);

            var project = await collection.Find(filter)
                .FirstOrDefaultAsync();

            return ParseMembers(project);
        }

        private List<string> ParseMembers(BsonDocument document)
        {
            if(document == null || !document.Contains("teamMemberIds"))
            {
                return new List<string>();
            }

            var teamMemberIds = document["teamMemberIds"].AsBsonArray;
            return teamMemberIds.Select(i => GetStringValue(i, "userId")).ToList();
        }

        private Project ParseProject(BsonDocument document)
        {
            return new Project
            {
                Id = GetStringValue(document, "_id"),
                Name = GetStringValue(document, "name"),
                MemberIds = ParseMembers(document)
            };
        }
    }
}

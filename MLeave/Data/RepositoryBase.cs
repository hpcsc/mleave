using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Configuration;

namespace MLeave.Data
{
    public abstract class RepositoryBase
    {
        protected IMongoCollection<BsonDocument> GetBsonCollection(string name)
        {
            var client = new MongoClient(ConfigurationManager.AppSettings["MongoConnectionString"]);
            var database = client.GetDatabase(ConfigurationManager.AppSettings["MongoDatabaseName"]);
            var collection = database.GetCollection<BsonDocument>(name);
            return collection;
        }

        protected string GetStringOrDoubleValue(BsonValue source, string name)
        {
            var document = source.AsBsonDocument;

            if (document != null && document.Contains(name))
            {
                return document[name].IsDouble ? 
                    document[name].AsDouble.ToString() :
                    (string)document[name];
            }

            return null;
        }

        protected string GetStringValue(BsonValue source, string name)
        {
            var document = source.AsBsonDocument;

            if (document != null && document.Contains(name))
            {
                return (string)document[name];
            }

            return null;
        }

        protected int GetIntValue(BsonValue source, string name)
        {
            var document = source.AsBsonDocument;

            if (document != null && document.Contains(name))
            {
                return (int)document[name];
            }

            return 0;
        }

        protected double GetDoubleValue(BsonValue source, string name)
        {
            var document = source.AsBsonDocument;

            if (document != null && document.Contains(name))
            {
                return document[name].IsInt32 ?
                    document[name].AsInt32 :
                    document[name].AsDouble;
            }

            return 0;
        }

        protected bool? GetBoolValue(BsonValue source, string name)
        {
            var document = source.AsBsonDocument;

            if (document != null && document.Contains(name))
            {
                return (bool)document[name];
            }

            return null;
        }

        protected DateTime? GetDateTimeValue(BsonValue source, string name)
        {
            var document = source.AsBsonDocument;

            if (document != null && document.Contains(name))
            {
                return (DateTime)document[name];
            }

            return null;
        }
    }
}

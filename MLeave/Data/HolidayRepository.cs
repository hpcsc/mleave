using MLeave.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MLeave.Data
{
    public class HolidayRepository : RepositoryBase
    {
        public async Task<List<Holiday>> FindAll(int year, string countryCode)
        {
            var collection = GetBsonCollection("holidays");
            var filters = new List<FilterDefinition<BsonDocument>>();
            filters.Add(Builders<BsonDocument>.Filter.Eq("country", countryCode));
            filters.Add(Builders<BsonDocument>.Filter.Eq("year", year));
            var filter = Builders<BsonDocument>.Filter.And(filters);

            var holidays = await collection.Find(filter).FirstOrDefaultAsync();

            return ParseHolidays(holidays);
        }

        public async Task<List<Holiday>> FindHolidaysForCountries(int year, List<string> countryCodes)
        {
            var collection = GetBsonCollection("holidays");
            var filters = new List<FilterDefinition<BsonDocument>>();
            filters.Add(Builders<BsonDocument>.Filter.In("country", countryCodes));
            filters.Add(Builders<BsonDocument>.Filter.Eq("year", year));
            var filter = Builders<BsonDocument>.Filter.And(filters);

            var holidays = await collection.Find(filter).ToListAsync();

            return holidays.SelectMany(h => ParseHolidays(h)).ToList();
        }

        private List<Holiday> ParseHolidays(BsonDocument document)
        {
            if(document == null || !document.Contains("holidays"))
            {
                return new List<Holiday>();
            }

            var holidays = document["holidays"].AsBsonArray;

            return holidays.Select(b => ParseHoliday(b.AsBsonDocument, 
                                                    GetStringValue(document, "country")))
                            .ToList();
        }

        private Holiday ParseHoliday(BsonDocument document, string country)
        {
            if(document == null)
            {
                return null;
            }

            return new Holiday
            {
                Name = GetStringValue(document, "name"),
                Date = ParseDate(GetStringValue(document, "date")),
                Country = country
            };
        }

        private static readonly CultureInfo _culture = new CultureInfo("en-us");
        private DateTime ParseDate(string date)
        {
            return DateTime.ParseExact(date, "yyyy-MM-dd", _culture);
        }
    }
}

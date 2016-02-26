using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Data.Entity.Spatial;

namespace server_api
{
    public class DbGeographyConverter : JsonConverter
    {
        private const string LATITUDE_KEY = "lat";
        private const string LONGITUDE_KEY = "lnt";

        public override bool CanConvert(Type objectType)
        {
            return objectType.Equals(typeof(DbGeography));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default(DbGeography);

            var jObject = JObject.Load(reader);

            if (!jObject.HasValues || (jObject.Property(LATITUDE_KEY) == null || jObject.Property(LONGITUDE_KEY) == null))
                return default(DbGeography);

            string wkt = string.Format(CultureInfo.InvariantCulture, "POINT({1} {0})", jObject[LATITUDE_KEY], jObject[LONGITUDE_KEY]);

            DbGeography returnValue = DbGeography.FromText(wkt, DbGeography.DefaultCoordinateSystemId);

            return returnValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            object blalue = value;
            DbGeography dbGeography = value as DbGeography;

            double lat = dbGeography.Latitude.Value;
            double lng = dbGeography.Longitude.Value;

            serializer.Serialize(writer, dbGeography == null || dbGeography.IsEmpty ? null : new { lat = dbGeography.Latitude.Value, lng = dbGeography.Longitude.Value });
        }
    }
}
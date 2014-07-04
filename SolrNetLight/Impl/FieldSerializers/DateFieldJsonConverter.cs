using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SolrNetLight.Impl.FieldSerializers
{
    public class DateFieldJsonConverter : JsonConverter
    {
        public static readonly string DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.FFF'Z'";
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            DateTime dateTime = new DateTime();
            dateTime = DateTime.ParseExact((string)reader.Value, DateTimeFormat, CultureInfo.InvariantCulture);

            return dateTime;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTime dateTime = (DateTime)value;
            writer.WriteValue(dateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
        }
    }
}

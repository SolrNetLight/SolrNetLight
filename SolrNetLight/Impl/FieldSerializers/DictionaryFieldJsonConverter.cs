using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SolrNetLight.Impl.FieldSerializers
{
    public class DictionaryFieldJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IDictionary);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dictionary = value as Dictionary<string, string>;
            //writer.WritePropertyName("phone_mobile");
            writer.WriteValue(JsonConvert.SerializeObject(dictionary));
            
        }
    }
}

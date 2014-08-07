using Newtonsoft.Json;
using System;

namespace KinectEx.Json
{
    public class JointTypeExConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jointType = value as JointTypeEx;

            if (jointType != null)
            {
                writer.WriteValue(jointType.Name);
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType != typeof(JointTypeEx))
                return null;

            if (reader.TokenType != JsonToken.String)
                return null;

            var value = reader.Value as String;

            if (JointTypeEx.ByName.ContainsKey(value))
                return (JointTypeEx)value;
            
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(JointTypeEx))
                return true;
            else
                return false;
        }
    }
}

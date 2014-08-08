using Newtonsoft.Json;
using System;

namespace KinectEx.Json
{
    /// <summary>
    /// Json.Net converter that serializes a <c>BoneTypeEx</c> object as a single string
    /// and deserializes it to the appropriate static instance.
    /// </summary>
    public class BoneTypeExConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var boneType = value as BoneTypeEx;

            if (boneType != null)
            {
                writer.WriteValue(boneType.Name);
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType != typeof(BoneTypeEx))
                return null;

            if (reader.TokenType != JsonToken.String)
                return null;

            var value = reader.Value as String;

            if (BoneTypeEx.ByName.ContainsKey(value))
                return (BoneTypeEx)value;
            
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(BoneTypeEx))
                return true;
            else
                return false;
        }
    }
}

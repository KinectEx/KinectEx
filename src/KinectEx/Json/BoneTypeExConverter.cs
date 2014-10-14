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
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
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

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
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

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(BoneTypeEx))
                return true;
            else
                return false;
        }
    }
}

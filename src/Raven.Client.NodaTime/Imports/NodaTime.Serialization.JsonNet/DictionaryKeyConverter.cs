// Copyright 2012 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Raven.Imports.NodaTime.Serialization.JsonNet
{
    /// <summary>
    /// Json.NET converter that enables using Noda Time types as a dictionary key.
    /// You must also register a converter for that type.
    /// </summary>
    /// <typeparam name="T">The Noda Time type that will be used as a dictionary key.</typeparam>
    public class DictionaryKeyConverter<T> : JsonConverter
        where T : struct
    {
        public override bool CanConvert(Type objectType)
        {
            // Use this converter if the type is an IDictionary<T,?>
            return IsIDictionaryOfT(objectType)
                || objectType.GetInterfaces().Any(IsIDictionaryOfT);
        }

        private static bool IsIDictionaryOfT(Type x)
        {
            return x.GetTypeInfo().IsGenericType 
                && typeof(IDictionary<,>).IsAssignableFrom(x.GetGenericTypeDefinition()) 
                && x.GetGenericArguments()[0] == typeof(T);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Create an intermediate dictionary that uses string keys.
            var valueType = value.GetType().GetGenericArguments()[1];
            var intermediateDictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType);
            var intermediateDictionary = (IDictionary) Activator.CreateInstance(intermediateDictionaryType);

            // Populate the intermediate dictionary.
            foreach (DictionaryEntry pair in (IDictionary) value)
            {
                // This will use any converters registered with the serializer.
                var key = JToken.FromObject(pair.Key, serializer).ToObject<string>();
                intermediateDictionary.Add(key, pair.Value);
            }

            // Write the intermediate dictionary.
            serializer.Serialize(writer, intermediateDictionary);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Handle nulls
            if (reader.TokenType == JsonToken.Null)
                return null;

            // Create an intermediate dictionary that uses string keys.
            var valueType = objectType.GetGenericArguments()[1];
            var intermediateDictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType);
            var intermediateDictionary = (IDictionary) Activator.CreateInstance(intermediateDictionaryType);

            // Read from JSON and populate the intermediate dictionary.
            serializer.Populate(reader, intermediateDictionary);

            // Create the dictionary we want to return, and populate it.
            var finalDictionaryType =
                IsIDictionaryOfT(objectType)
                ? typeof(Dictionary<,>).MakeGenericType(objectType.GetGenericArguments()[0], valueType) // we cannot instantiate an IDictionary, so fallback to a Dictionary<T,?>
                : objectType; // it is not an IDictionary, so create the real objectType

            var finalDictionary = (IDictionary) Activator.CreateInstance(finalDictionaryType);
            foreach (DictionaryEntry pair in intermediateDictionary)
            {
                // This will use any converters registered with the serializer.
                var key = JToken.FromObject(pair.Key).ToObject<T>(serializer);
                finalDictionary.Add(key, pair.Value);
            }

            // Return the final dictionary.
            return finalDictionary;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Alexa.NET.Request;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace PinballBot
{
    public static class AmazonAlexaExtensions
    {
        public static readonly JsonSerializer _serializer = new JsonSerializer();

        public static SkillRequest BuildSkillRequest(this HttpRequest request)
        {
            using (var sr = new StreamReader(request.Body))
            using (var reader = new JsonTextReader(sr))
            {
                return _serializer.Deserialize<SkillRequest>(reader);
            }
        }

        public static string GetValueAsString(this IDictionary<string, Slot> dictionary, string name, string defaultValue = null)
        {
            if(!dictionary.ContainsKey(name))
            {
                return defaultValue;
            }

            return dictionary[name].Value;
        }
    }
}

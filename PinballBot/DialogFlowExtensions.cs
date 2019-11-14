using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Newtonsoft.Json;

namespace PinballBot
{
    public static class DialogFlowExtensions
    {
        const string DataKey = "data";

        const int MaxLifespan = 100;
        const int NoLifespan = 0;

        // A Protobuf JSON parser configured to ignore unknown fields. This makes
        // the action robust against new fields being introduced by Dialogflow.
        private static readonly JsonParser _jsonParser =
            new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

        public static WebhookRequest BuildWebhookRequest(this HttpRequest request)
        {

            using (var sr = new StreamReader(request.Body))
            //using (var reader = new JsonTextReader(sr))
            {
                return _jsonParser.Parse<WebhookRequest>(sr);
                //return Serializer.Deserialize<WebhookRequest>(reader);
            }
        }

        public static WebhookResponse BuildBaseResponse(this WebhookRequest webhookRequest)
        {
            var webhookResponse = new WebhookResponse();

            var dataContext = webhookRequest.QueryResult.OutputContexts.GetContext($"{webhookRequest.Session}/contexts/{DataKey}");

            if (dataContext != null)
            {
                webhookResponse.OutputContexts.Add(dataContext);
            }

            return webhookResponse;
        }

        public static Context GetContext(this IEnumerable<Context> contexts, string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("Name must be not empty", nameof(name));

            return contexts?.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        public static WebhookResponse AddToSession(this WebhookResponse webhookResponse, string sessionId, string key, Value value, int? maxLifespan = null)
        {
            Context outputContext = null;

            var outPutContextName = $"{sessionId}/contexts/{DataKey}";

            var lifespan = maxLifespan ?? MaxLifespan;

            if (webhookResponse?.OutputContexts.GetContext(outPutContextName) == null)
            {
                outputContext = new Context
                {
                    Name = outPutContextName
                };

                webhookResponse.OutputContexts.Add(outputContext);
            }
            else
            {
                outputContext = webhookResponse.OutputContexts.First(x => x.Name.Equals(outPutContextName, StringComparison.OrdinalIgnoreCase));
            }

            outputContext.LifespanCount = lifespan;

            if(outputContext.Parameters == null)
            {
                outputContext.Parameters = new Struct();
            }

            outputContext.Parameters.Fields[key] = value;

            return webhookResponse;
        }

        public static string GetValueAsString(this MapField<string, Value> mapField, string key, string defaultValue = null)
        {
            if (!mapField.ContainsKey(key))
                return defaultValue != null ? defaultValue : null;

            return mapField[key].StringValue;
        }

        public static bool GetValueAsBool(this MapField<string, Value> mapField, string key, bool? defaultValue = null)
        {
            if (!mapField.ContainsKey(key))
                return defaultValue.HasValue ? defaultValue.Value : default(bool);

            return mapField[key].BoolValue;
        }

        public static double GetValueAsDouble(this MapField<string, Value> mapField, string key, double? defaultValue = null)
        {
            if (!mapField.ContainsKey(key))
                return defaultValue.HasValue ? defaultValue.Value : default(double);

            return mapField[key].NumberValue;
        }

        public static Value ToValue(this bool value)
        {
            return Value.ForBool(value);
        }

        public static Value ToValue(this double value)
        {
            return Value.ForNumber(value);
        }

        public static Value ToValue(this string value)
        {
            return Value.ForString(value);
        }
    }
}

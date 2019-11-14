using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET;
using Alexa.NET.Request.Type;

namespace PinballBot
{
    public static class PinballBot
    {
        private static readonly PinballMapApiClient.PinballMapApiClient _apiClient = new PinballMapApiClient.PinballMapApiClient();

        [FunctionName(nameof(PinballBotDialogflow))]
        public static async Task<IActionResult> PinballBotDialogflow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //log.LogInformation("C# HTTP trigger function processed a request.");

            //string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            var webhookRequest = req.BuildWebhookRequest();

            var intent = webhookRequest.OriginalDetectIntentRequest;

            System.Diagnostics.Debug.WriteLine($"{intent}");

            var response = webhookRequest.BuildBaseResponse();

            response.AddToSession(webhookRequest.Session, "session", webhookRequest.Session.ToValue());

            if(webhookRequest?.QueryResult?.Intent?.DisplayName?.Equals("find_machines_near_location") ?? false)
            {
                var city = webhookRequest?.QueryResult?.Parameters?.Fields["geo-city"].StringValue;

                var apiResponse = await _apiClient.GetLocationsClosestByAddress(city, true, 5);
                var location = apiResponse.Result.Locations.FirstOrDefault();
                response.FulfillmentText = $"It looks like the closest location is {location.Name}";
            }
            else
            {
                response.FulfillmentText = "There was no response";
            }


            return new OkObjectResult(response);
        }

        [FunctionName(nameof(PinballBotAmazonAlexa))]
        public static async Task<IActionResult> PinballBotAmazonAlexa(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var skillRequest = req.BuildSkillRequest();

            SkillResponse response = null;

            if (skillRequest.Request is LaunchRequest)
            {
                response = ResponseBuilder.Ask("Welcome to Pinball Info", new Reprompt("Would you like to find pinball machines?"));
            }
            else if(skillRequest.Request is IntentRequest intentRequest)
            {
                if(intentRequest.Intent.Name.Equals("find_machines_near_location"))
                {
                    var city = intentRequest.Intent.Slots.GetValueAsString("geo_city");
                    var apiResponse = await _apiClient.GetLocationsClosestByAddress(city, true, 5);
                    var location = apiResponse.Result.Locations.FirstOrDefault();
                    response = ResponseBuilder.Tell($"It looks like the closest location is {location.Name}");

                }
            }

            if(response == null)
            {
                response = ResponseBuilder.Tell("not sure what to do here boss...Try again...");
            }

            return new OkObjectResult(response);
        }
    }
}

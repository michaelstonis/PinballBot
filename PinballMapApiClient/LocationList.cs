using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PinballMapApiClient
{
    public class LocationList
    {
        [JsonProperty("locations")]
        public IEnumerable<Location> Locations { get; set; }
    }
}

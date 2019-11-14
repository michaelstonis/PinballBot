using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PinballMapApiClient
{
    public class RegionList
    {
        [JsonProperty("regions")]
        public IEnumerable<Region> Regions { get; set; }
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PinballMapApiClient
{
    public partial class Location
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("zip")]
        public long Zip { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lon")]
        public double Longitude { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("zone_id")]
        public long? ZoneId { get; set; }

        [JsonProperty("region_id")]
        public long RegionId { get; set; }

        [JsonProperty("location_type_id")]
        public long? LocationTypeId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("operator_id")]
        public long? OperatorId { get; set; }

        [JsonProperty("date_last_updated")]
        public DateTimeOffset DateLastUpdated { get; set; }

        [JsonProperty("last_updated_by_user_id")]
        public long? LastUpdatedByUserId { get; set; }

        [JsonProperty("is_stern_army")]
        public bool? IsSternArmy { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("last_updated_by_username")]
        public string LastUpdatedByUsername { get; set; }

        [JsonProperty("num_machines")]
        public long NumMachines { get; set; }

        [JsonProperty("location_machine_xrefs")]
        public List<LocationMachineXref> LocationMachineXrefs { get; set; }
    }
}

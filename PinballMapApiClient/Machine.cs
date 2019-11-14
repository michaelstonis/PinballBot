using System;
using Newtonsoft.Json;

namespace PinballMapApiClient
{
    public partial class Machine
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("is_active")]
        public bool? IsActive { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("ipdb_link")]
        public string IpdbLink { get; set; }

        [JsonProperty("year")]
        public long Year { get; set; }

        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; }

        [JsonProperty("machine_group_id")]
        public long? MachineGroupId { get; set; }

        [JsonProperty("ipdb_id")]
        public long? IpdbId { get; set; }

        [JsonProperty("opdb_id")]
        public string OpdbId { get; set; }
    }
}

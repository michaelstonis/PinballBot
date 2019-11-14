using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PinballMapApiClient
{
    public partial class LocationMachineXref
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty("location_id")]
        public long LocationId { get; set; }

        [JsonProperty("machine_id")]
        public long MachineId { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; }

        [JsonProperty("condition_date")]
        public DateTimeOffset? ConditionDate { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("user_id")]
        public long? UserId { get; set; }

        [JsonProperty("machine_score_xrefs_count")]
        public long? MachineScoreXrefsCount { get; set; }

        [JsonProperty("last_updated_by_username")]
        public string LastUpdatedByUsername { get; set; }

        [JsonProperty("machine_conditions")]
        public List<MachineCondition> MachineConditions { get; set; }
    }
}

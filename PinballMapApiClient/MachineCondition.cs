using System;
using Newtonsoft.Json;

namespace PinballMapApiClient
{
    public partial class MachineCondition
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("location_machine_xref_id")]
        public long LocationMachineXrefId { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("user_id")]
        public long? UserId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }
}

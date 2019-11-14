using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PinballMapApiClient
{
    public partial class Region
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("motd")]
        public string Motd { get; set; }

        [JsonProperty("lat")]
        public string Latitude { get; set; }

        [JsonProperty("lon")]
        public string Longitude { get; set; }

        [JsonProperty("n_search_no")]
        public long? NSearchNo { get; set; }

        [JsonProperty("default_search_type")]
        public string DefaultSearchType { get; set; }

        [JsonProperty("should_email_machine_removal")]
        public bool ShouldEmailMachineRemoval { get; set; }

        [JsonProperty("should_auto_delete_empty_locations")]
        public bool ShouldAutoDeleteEmptyLocations { get; set; }

        [JsonProperty("send_digest_comment_emails")]
        public bool SendDigestCommentEmails { get; set; }

        [JsonProperty("send_digest_removal_emails")]
        public bool SendDigestRemovalEmails { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("effective_radius")]
        public long EffectiveRadius { get; set; }

        [JsonProperty("primary_email_contact")]
        public string PrimaryEmailContact { get; set; }

        [JsonProperty("all_admin_email_addresses")]
        public List<string> AllAdminEmailAddresses { get; set; }
    }
}

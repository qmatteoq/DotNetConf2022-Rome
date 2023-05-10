using Newtonsoft.Json;

namespace TicketBot.Models
{
    public class TicketData
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("context")]
        public Context Context { get; set; }
    }

    public class Data
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("msteams")]
        public Msteams Msteams { get; set; }

        [JsonProperty("ticketTitle")]
        public string TicketTitle { get; set; }

        [JsonProperty("ticketDescription")]
        public string TicketDescription { get; set; }
    }

    public class Msteams
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Context
    {
        [JsonProperty("theme")]
        public string Theme { get; set; }
    }
}

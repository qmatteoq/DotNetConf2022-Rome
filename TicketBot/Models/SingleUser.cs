using Newtonsoft.Json;

namespace TicketBot.Models
{
    public class SingleUser
    {
        [JsonProperty("data")]
        public User User { get; set; }

        [JsonProperty("support")]
        public Support Support { get; set; }
    }
}

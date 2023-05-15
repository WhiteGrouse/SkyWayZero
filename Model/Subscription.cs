using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SkyWayZero.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public record SubscriptionSummary
	{
        public string Id { get; init; }
        public string PublicationId { get; init; }
        public string SubscriberId { get; init; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public record Subscription : SubscriptionSummary
	{
		public string ChannelId { get; init; }
		public string PublisherId { get; init; }
		public ContentType ContentType { get; init; }
	}
}


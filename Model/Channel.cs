using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SkyWayZero.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public record ChannelSummary
    {
        public string Id { get; init; }
        public int Version { get; init; }
        public string Metadata { get; init; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public record Channel : ChannelSummary
	{
        public string Name { get; init; }
        public IEnumerable<Member> Members { get; init; }
        public IEnumerable<Publication> Publications { get; init; }
        public IEnumerable<Subscription> Subscriptions { get; init; }
	}
}


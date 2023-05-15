using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SkyWayZero.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public record PublicationSummary
	{
        public string Id { get; init; }
        public string PublisherId { get; init; }
        public string Origin { get; init; }
        public ContentType ContentType { get; init; }
        public string Metadata { get; set; }
        public IEnumerable<Codec> CodecCapabilities { get; init; }
        public IEnumerable<Encoding> Encodings { get; init; }
        public bool IsEnabled { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public record Publication : PublicationSummary
	{
        public string ChannelId { get; init; }
    }

	public enum ContentType
	{
		Audio,
		Video,
		Data,
	}
}


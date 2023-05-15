using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SkyWayZero.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public record Encoding
	{
        public string Id { get; init; }
        public int? MaxBitrate { get; init; }
        public int? ScaleResolutionDownBy { get; init; }
        public int? MaxFramerate { get; init; }
    }
}


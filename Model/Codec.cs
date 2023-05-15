using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SkyWayZero.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public record Codec
	{
		public string MimeType { get; init; }
	}
}


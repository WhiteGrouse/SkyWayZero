using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SkyWayZero.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public record Member
	{
		public string Id { get; init; }
		public string Name { get; init; }
		public MemberType Type { get; init; }
		public string Subtype { get; init; }
		public string Metadata { get; set; }
	}

    public enum MemberType
	{
		Person,
		Bot,
	}
}


using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SkyWayZero.Model;

namespace SkyWayZero.Rtc
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public record Event<T>(string Type, string AppId, T Data) where T : EventData;

    public abstract record EventData;
    public record ChannelCreated(ChannelSummary Channel) : EventData;
    public record ChannelDeleted(ChannelSummary Channel) : EventData;
    public record ChannelMetadataUpdated(ChannelSummary Channel) : EventData;
    public record MemberAdded(ChannelSummary Channel, Member Member) : EventData;
    public record MemberRemoved(ChannelSummary Channel, Member Member) : EventData;
    public record MemberMetadataUpdate(ChannelSummary Channel, Member Member, string Metadata) : EventData;
    public record StreamPublished(ChannelSummary Channel, PublicationSummary Publication) : EventData;
    public record StreamUnpublished(ChannelSummary Channel, PublicationSummary Publication) : EventData;
    public record PublicationMetadataUpdated(ChannelSummary Channel, PublicationSummary Publication) : EventData;
    public record PublicationEnabled(ChannelSummary Channel, PublicationSummary Publication) : EventData;
    public record PublicationDisabled(ChannelSummary Channel, PublicationSummary Publication) : EventData;
    public record StreamSubscribed(ChannelSummary Channel, SubscriptionSummary Subscription) : EventData;
    public record StreamUnsubscribed(ChannelSummary Channel, SubscriptionSummary Subscription) : EventData;
}


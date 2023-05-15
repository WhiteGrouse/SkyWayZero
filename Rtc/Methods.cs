using System;
using System.Collections.Generic;
using SkyWayZero.Model;

namespace SkyWayZero.Rtc
{
    public record RequestParams
    {
        public string AppId { get; set; }
        public string AuthToken { get; set; }
    }

    public record ResponseResult;

    public record AddMemberRequest(string ChannelId, string Name, string Type, string Subtype, long? TtlSec) : RequestParams;
    public record AddMemberResponse(string MemberId, string Version) : ResponseResult;

    public record CreateChannelRequest(string Name, string Metadata) : RequestParams;
    public record CreateChannelResponse(ChannelSummary Channel) : ResponseResult;

    public record DeleteChannelRequest(string Id) : RequestParams;
    public record DeleteChannelResponse() : ResponseResult;

    public record DisablePublicationRequest(string ChannelId, string PublicationId) : RequestParams;
    public record DisablePublicationResponse(int Version) : ResponseResult;

    public record EnablePublicationRequest(string ChannelId, string PublicationId) : RequestParams;
    public record EnablePublicationResponse(int Version) : ResponseResult;

    public record FindOrCreateChannelRequest(string Name, string Metadata) : RequestParams;
    public record FindOrCreateChannelResponse(Channel Channel) : ResponseResult;

    public record GetChannelRequest(string Id) : RequestParams;
    public record GetChannelResponse(Channel Channel) : ResponseResult;

    public record GetChannelByNameRequest(string name) : RequestParams;
    public record GetChannelByNameResponse(Channel Channel) : ResponseResult;

    public record GetServerUnixtimeRequest : RequestParams;
    public record GetServerUnixtimeResponse(long Unixtime) : ResponseResult;

    public record PublishStreamRequest(
        string ChannelId,
        string PublisherId,
        ContentType ContentType,
        string Metadata,
        string Origin,
        IEnumerable<Codec> CodecCapabilities,
        IEnumerable<Encoding> Encodings) : RequestParams;

    public record PublishStreamResponse(string Id, int Version) : ResponseResult;

    public record RemoveMemberRequest(string ChannelId, string Id) : RequestParams;
    public record RemoveMemberResponse(int Version) : ResponseResult;

    public record SubscribeStreamRequest(string ChannelId, string SubscriberId, string PublicationId) : RequestParams;
    public record SubscribeStreamResponse(string Id, int Version) : ResponseResult;

    public record UnpublishStreamRequest(string ChannelId, string PublicationId) : RequestParams;
    public record UnpublishStreamResponse(int Version) : ResponseResult;

    public record UnsubscribeStreamRequest(string ChannelId, string SubscriptionId) : RequestParams;
    public record UnsubscribeStreamResponse(int Version) : ResponseResult;

    public record UpdateChannelMetadataRequest(string Id, string Metadata) : RequestParams;
    public record UpdateChannelMetadataResponse(int Version) : ResponseResult;

    public record UpdateMemberMetadataRequest(string ChannelId, string MemberId, string Metadata) : RequestParams;
    public record UpdateMemberMetadataResponse(int Version) : ResponseResult;

    public record UpdateMemberTtlRequest(string ChannelId, string MemberId, long TtlSec) : RequestParams;
    public record UpdateMemberTtlResponse(int Version) : ResponseResult;

    public record UpdatePublicationMetadataRequest(string ChannelId, string PublicationId, string Metadata) : RequestParams;
    public record UpdatePublicationMetadataResponse(int Version) : ResponseResult;

    public record SubscribeChannelEventsRequest(string ChannelId, int Offset) : RequestParams;
    public record SubscribeChannelEventsResponse : ResponseResult;
}


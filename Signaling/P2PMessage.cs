namespace SkyWayZero.Signaling
{
    public record P2PMessage<T>(string Kind, T Payload);

    public record SessionDescription(string Type, string Sdp);

    public record SdpMetadata(string PublicationId, string StreamId, string Mid);

    public record CandidateInfo(string Candidate, string SdpMid, int SdpMLineIndex, string UsernameFragment);

    public record SenderProducePayload(SessionDescription Sdp, string PublicationId, SdpMetadata Info);

    public record ReceiverAnswerPayload(SessionDescription Sdp);

    public record IceCandidatePayload(CandidateInfo Candidate, string Role);
}
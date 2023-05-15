using System;
namespace SkyWayZero.Signaling
{
	public record SignalingEventData
	{
		public string Event { get; set; }
		public string EventId { get; set; }
		public object Payload { get; set; } = null;
	}

	public record SignalingMessagePayload<T>
	{
		public T Data { get; set; }
		public Peer Dst { get; set; }
		public string RequestEventId { get; set; }
		public Peer Src { get; set; }
	}
    public record AcknowledgePayload(string EventId, bool Ok);
	public record SendRequestSignalingMessagePayload(Data Data, Peer Dst, string RequestEventId, Peer Src);
	public record SendResponseSignalingMessagePayload(Peer Dst, string RequestEventId)
	{
		private object Data = new object();
	}


    public record Peer(string Id, string Name, string Type = null, string Subtype = null);
	public record Data
	{
		public string Chunk { get; set; }
		public string Id { get; set; }
		public int Length { get; set; }
		public int Offset { get; set; }
		public string Type { get; set; }
	}

	public record CandidateEventArgs(string SrcId, string SrcName, CandidateInfo Candidate);

	public record OfferEventArgs(string SrcId, string SrcName, SessionDescription Sdp);
}


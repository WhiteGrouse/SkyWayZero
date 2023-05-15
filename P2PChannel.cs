using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using SkyWayZero.Model;
using SkyWayZero.Rtc;
using SkyWayZero.Signaling;
using UnityEngine;

namespace SkyWayZero
{
	public class P2PChannel
	{
		private RtcConfig _config;
		private RtcClient _rtcClient;
		private SignalingClient _signalingClient;
		private string _channelId;
		private string _memberId;

		private CancellationTokenSource _tokenSource;
		private UniTask _updateMemberTtlLoopTask;

        public static async UniTask<P2PChannel> Open(RtcConfig config, string channelName)
		{
			var rtcClient = new RtcClient(config);
			rtcClient.Connect();
			var unixtime = await rtcClient.GetServerUnixtime();
			var channelId = await rtcClient.CreateChannel(channelName);
			await rtcClient.DeleteChannel(channelId);
			await Task.Delay(1000);
            channelId = await rtcClient.CreateChannel(channelName);
            var memberId = await rtcClient.AddMember(channelId, "UnityClient", unixtime / 1000 + 30);

			var signalingClient = new SignalingClient(config, channelId, channelName, memberId, "UnityClient");

			return new P2PChannel(config, rtcClient, signalingClient, channelId, memberId);
        }

		private P2PChannel(RtcConfig config, RtcClient rtcClient, SignalingClient signalingClient, string channelId, string memberId)
		{
			_config = config;
			_rtcClient = rtcClient;
			_signalingClient = signalingClient;
			_channelId = channelId;
			_memberId = memberId;

            _rtcClient.OnEvent += OnEvent;

            _signalingClient.OnOffer += _OnOffer;
            _signalingClient.OnRemoteIceCandidate += _OnRemoteIceCandidate;
            
			_tokenSource = new CancellationTokenSource();
			_updateMemberTtlLoopTask = UniTask.Create(() => UpdateMemberTtlLoop(_tokenSource.Token));
		}

        public async UniTask Close()
		{
			_tokenSource.Cancel();
			await _updateMemberTtlLoopTask;
			await _signalingClient.Close();
			await _rtcClient.RemoveMember(_channelId, _memberId);
			await _rtcClient.DeleteChannel(_channelId);
			_rtcClient.Close();
			Debug.Log("Closed");
		}

		public async UniTask Subscribe(string publicationId)
		{
			var subscriptionId = await _rtcClient.Subscribe(_channelId, _memberId, publicationId);
			//
		}

		public async UniTask SendAnswer(string dstId, string dstName, string sdp)
		{
			await _signalingClient.SendAnswer(dstId, dstName, sdp);
		}

		public async UniTask SendIceCandidate(string dstId, string dstName, string candidate, string userFragment)
		{
			await _signalingClient.SendIceCandidate(dstId, dstName, candidate, userFragment);
		}

        public event EventHandler<VideoStreamPublished> OnVideoStreamPublished;
        public event EventHandler<VideoStreamUnpublished> OnVideoStreamUnpublished;
		public event EventHandler<DataStreamPublished> OnDataStreamPublished;
		public event EventHandler<DataStreamUnpublished> OnDataStreamUnpublished;
		public event EventHandler<OfferEventArgs> OnOffer;
		public event EventHandler<CandidateEventArgs> OnRemoteIceCandidate;

		private async UniTask UpdateMemberTtlLoop(CancellationToken cancellationToken)
		{
			try
			{
                while (!cancellationToken.IsCancellationRequested)
                {
                    var unixtime = await _rtcClient.GetServerUnixtime();
                    await _rtcClient.UpdateMemberTtl(_channelId, _memberId, unixtime / 1000 + 30);
                    await Task.Delay(10000, _tokenSource.Token);
                }
            }
			catch (TaskCanceledException)
			{
				//nop
			}
		}

        private void OnEvent(object sender, Rtc.EventData e)
        {
            ChannelSummary channel = (e as dynamic).Channel;
            if (channel.Id != _channelId)
                return;

			if (e is StreamPublished)
			{
				var ev = (StreamPublished)e;
				var type = ev.Publication.ContentType;
				if (type == ContentType.Video)
					OnVideoStreamPublished?.Invoke(this, new VideoStreamPublished(ev.Publication.Id));
				else if (type == ContentType.Data)
					OnDataStreamPublished?.Invoke(this, new DataStreamPublished(ev.Publication.Id));
			}
			else if (e is StreamUnpublished)
			{
				var ev = (StreamUnpublished)e;
				var type = ev.Publication.ContentType;
				if (type == ContentType.Video)
					OnVideoStreamUnpublished?.Invoke(this, new VideoStreamUnpublished(ev.Publication.Id));
				else if (type == ContentType.Data)
					OnDataStreamUnpublished?.Invoke(this, new DataStreamUnpublished(ev.Publication.Id));
            }
        }

        private void _OnOffer(object sender, OfferEventArgs e)
        {
	        OnOffer?.Invoke(this, e);
        }

        private void _OnRemoteIceCandidate(object sender, CandidateEventArgs e)
        {
	        OnRemoteIceCandidate?.Invoke(this, e);
        }
    }

	public record VideoStreamPublished(string publicationId);
	public record DataStreamPublished(string publicationId);
	public record VideoStreamUnpublished(string publicationId);
	public record DataStreamUnpublished(string publicationId);
}


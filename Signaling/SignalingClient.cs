using System;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using WebSocketSharp;

namespace SkyWayZero.Signaling
{
	public class SignalingClient
	{
        public RtcConfig Config { get; }

        private string _memberId;

        private RequestTaskCollection<string> _ackTasks;
        private RequestTaskCollection<string> _waitTasks;
        private WebSocket _ws;

        private CancellationTokenSource _tokenSource;
        private UniTask _checkConnectivityLoopTask;

        public SignalingClient(RtcConfig config, string channelId, string channelName, string memberId, string memberName)
        {
            Config = config;

            _memberId = memberId;

            _ackTasks = new RequestTaskCollection<string>();
            _waitTasks = new RequestTaskCollection<string>();
            var builder = new StringBuilder();
            builder.Append($"channelId={channelId}&");
            builder.Append($"channelName={channelName}&");
            builder.Append($"memberId={memberId}&");
            builder.Append($"memberName={memberName}&");
            builder.Append($"platform=javascript&");
            builder.Append($"version=0.2.0-beta.0");
            _ws = new WebSocket($"wss://signaling.skyway.ntt.com/v1/ws?{builder}", $"SkyWayAuthToken!{Config.Token}");
            _ws.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
            _ws.OnMessage += OnMessage;

            _ws.Connect();
        }

        public async UniTask Close()
        {
            _tokenSource.Cancel();
            await _checkConnectivityLoopTask;
            _ws.Close();
        }

        private async UniTask CheckConnectivityLoop()
        {
            try
            {
                while (!_tokenSource.IsCancellationRequested)
                {
                    await CheckConnectivity();
                    await UniTask.Delay(30000, DelayType.Realtime, PlayerLoopTiming.Update, _tokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                //nop
            }
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            var obj = JObject.Parse(e.Data);
            string eventType = obj.Value<string>("event")!;
            if(eventType == "open")
            {
                _tokenSource = new CancellationTokenSource();
                _checkConnectivityLoopTask = UniTask.Create(CheckConnectivityLoop);
            }
            else if(eventType == "acknowledge")
            {
                _ackTasks.Response(obj["payload"]!.Value<string>("eventId")!, e.Data);
            }
            else if(eventType == "sendRequestSignalingMessage")
            {
                string eventId = obj["payload"]!.Value<string>("requestEventId")!;
                var payload = obj["payload"]!.ToObject<SignalingMessagePayload<Data>>()!;
                UniTask.Create(() => Response(payload.Src!.Id, payload.Src.Name, eventId)).Forget();
                OnSignalingMessage(payload.Src, payload);
            }
            else if(eventType == "sendResponseSignalingMessage")
            {
                _waitTasks.Response(obj["payload"]!.Value<string>("requestEventId")!, e.Data);
            }

            //Debug.Log($"Received: {e.Data}");
        }

        private void OnSignalingMessage(Peer src, SignalingMessagePayload<Data> message)
        {
            var obj = JObject.Parse(message.Data.Chunk);
            string kind = obj.Value<string>("kind")!;
            if (kind == "senderProduceMessage")
            {
                var payload = obj["payload"].ToObject<SenderProducePayload>();
                OnOffer?.Invoke(this, new OfferEventArgs(src.Id, src.Name, payload.Sdp));
            }
            else if (kind == "iceCandidateMessage")
            {
                var payload = obj["payload"].ToObject<IceCandidatePayload>();
                OnRemoteIceCandidate?.Invoke(this, new CandidateEventArgs(src.Id, src.Name, payload.Candidate));
            }
        }

        public event EventHandler<CandidateEventArgs> OnRemoteIceCandidate;
        public event EventHandler<OfferEventArgs> OnOffer;

        private object _lockObj = new object();
        private string GenerateEventId()
        {
            lock (_lockObj)
            {
                return Guid.NewGuid().ToString();
            }
        }

        private async UniTask CheckConnectivity()
        {
            var id = GenerateEventId();
            var (_, serverAck) = _ackTasks.Request(id);
            var json = JsonConvert.SerializeObject(new SignalingEventData
            {
                Event = "checkConnectivity",
                EventId = id,
                Payload = new object(),
            }, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            });
            //Debug.Log($"Sent: {json}");
            _ws.Send(json);
            await serverAck;
        }

        public async UniTask SendAnswer(string dstId, string dstName, string sdp)
        {
            var payload = new ReceiverAnswerPayload(new SessionDescription("answer", sdp));
            var message = new P2PMessage<ReceiverAnswerPayload>("receiverAnswerMessage", payload);
            var json = JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            });
            await Request(dstId, dstName, json);
        }

        public async UniTask SendIceCandidate(string dstId, string dstName, string candidate, string userFragment)
        {
            var info = new CandidateInfo(candidate, "0", 0, userFragment);
            var payload = new IceCandidatePayload(info, "receiver");
            var message = new P2PMessage<IceCandidatePayload>("iceCandidateMessage", payload);
            var json = JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            });
            await Request(dstId, dstName, json);
        }
        
        public async UniTask Request(string dstId, string dstName, string data)
        {
            var id = GenerateEventId();
            var (_, serverAck) = _ackTasks.Request(id);
            var (_, remoteAck) = _waitTasks.Request(id);
            var json = JsonConvert.SerializeObject(new SignalingEventData
            {
                Event = "sendRequestSignalingMessage",
                EventId = id,
                Payload = new SignalingMessagePayload<Data>
                {
                    Data = new Data
                    {
                        Chunk = data,
                        Id = GenerateEventId(),
                        Length = 0,
                        Offset = 0,
                        Type = "signalingMessage",
                    },
                    Dst = new Peer(dstId, dstName, "person", "person"),
                }
            }, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            });
            //Debug.Log($"Sent: {json}");
            _ws.Send(json);
            await UniTask.WhenAll(serverAck, remoteAck);
        }

        private async UniTask Response(string dstId, string dstName, string eventId)
        {
            var id = GenerateEventId();
            var (_, serverAck) = _ackTasks.Request(id);
            var json = JsonConvert.SerializeObject(new SignalingEventData
            {
                Event = "sendResponseSignalingMessage",
                EventId = id,
                Payload = new SignalingMessagePayload<object>
                {
                    Data = new object(),
                    Dst = new Peer(dstId, dstName),
                    RequestEventId = eventId,
                },
            }, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            });
            //Debug.Log($"Sent: {json}");
            _ws.Send(json);
            await serverAck;
        }
    }
}


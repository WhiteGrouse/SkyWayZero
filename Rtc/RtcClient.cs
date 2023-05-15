using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SkyWayZero.Model;
using SkyWayZero;
using UnityEngine;
using WebSocketSharp;

namespace SkyWayZero.Rtc
{
	public class RtcClient : IDisposable
	{
        public RtcConfig Config { get; }

        private RequestTaskCollection<string> _waitTasks;
        private WebSocket _ws;

        private Dictionary<string, Action<JToken>> _eventHandlers;

        public RtcClient(RtcConfig config)
		{
            Config = config;

            _waitTasks = new RequestTaskCollection<string>();
            _ws = new WebSocket($"wss://rtc-api.skyway.ntt.com/ws", Config.Token);
            _ws.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
            _ws.OnMessage += OnMessage;

            _eventHandlers = new Dictionary<string, Action<JToken>>
            {
                { "ChannelCreated", (data) => OnEvent?.Invoke(this, data.ToObject<ChannelCreated>()!) },
                { "ChannelDeleted", (data) => OnEvent?.Invoke(this, data.ToObject<ChannelDeleted>()!) },
                { "ChannelMetadataUpdated", (data) => OnEvent?.Invoke(this, data.ToObject<ChannelMetadataUpdated>()!) },
                { "MemberAdded", (data) => OnEvent?.Invoke(this, data.ToObject<MemberAdded>()!) },
                { "MemberRemoved", (data) => OnEvent?.Invoke(this, data.ToObject<MemberRemoved>()!) },
                { "MemberMetadataUpdate", (data) => OnEvent?.Invoke(this, data.ToObject<MemberMetadataUpdate>()!) },
                { "StreamPublished", (data) => OnEvent?.Invoke(this, data.ToObject<StreamPublished>()!) },
                { "StreamUnpublished", (data) => OnEvent?.Invoke(this, data.ToObject<StreamUnpublished>()!) },
                { "PublicationMetadataUpdated", (data) => OnEvent?.Invoke(this, data.ToObject<PublicationMetadataUpdated>()!) },
                { "PublicationEnabled", (data) => OnEvent?.Invoke(this, data.ToObject<PublicationEnabled>()!) },
                { "PublicationDisabled", (data) => OnEvent?.Invoke(this, data.ToObject<PublicationDisabled>()!) },
                { "StreamSubscribed", (data) => OnEvent?.Invoke(this, data.ToObject<StreamSubscribed>()!) },
                { "StreamUnsubscribed", (data) => OnEvent?.Invoke(this, data.ToObject<StreamUnsubscribed>()!) },
            };
        }

        public void Connect()
		{
            _ws.Connect();
        }

		public void Close()
		{
            _ws.Close();
            _waitTasks.Dispose();
        }

        public void Dispose() => Close();

        public event EventHandler<EventData> OnEvent;

        public async UniTask<long> GetServerUnixtime()
        {
            var request = new GetServerUnixtimeRequest();
            var result = await RequestAsync<GetServerUnixtimeResponse>("getServerUnixtime", request);
            return result.Unixtime;
        }

        public async UniTask<string> CreateChannel(string channelName)
        {
            var request = new CreateChannelRequest(channelName, null);
            var result = await RequestAsync<CreateChannelResponse>("findOrCreateChannel", request);
            return result.Channel.Id;
        }

        public async UniTask DeleteChannel(string channelId)
        {
            var request = new DeleteChannelRequest(channelId);
            await RequestAsync<DeleteChannelResponse>("deleteChannel", request);
        }

        public async UniTask<string> AddMember(string channelId, string memberName, long ttlSec)
        {
            var request = new AddMemberRequest(channelId, memberName, "Person", "Person", ttlSec);
            var result = await RequestAsync<AddMemberResponse>("addMember", request);
            return result.MemberId;
        }

        public async UniTask RemoveMember(string channelId, string memberId)
        {
            var request = new RemoveMemberRequest(channelId, memberId);
            await RequestAsync<RemoveMemberResponse>("removeMember", request);
        }

        public async UniTask UpdateMemberTtl(string channelId, string memberId, long ttlSec)
        {
            var request = new UpdateMemberTtlRequest(channelId, memberId, ttlSec);
            await RequestAsync<UpdateMemberTtlResponse>("updateMemberTtl", request);
        }

        public async UniTask<string> Subscribe(string channelId, string subscriberId, string publicationId)
        {
            var request = new SubscribeStreamRequest(channelId, subscriberId, publicationId);
            var result = await RequestAsync<SubscribeStreamResponse>("subscribeStream", request);
            return result.Id;
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            var obj = JObject.Parse(e.Data);
            if (obj.ContainsKey("id"))//Result
            {
                _waitTasks.Response(obj.Value<int>("id"), e.Data);
            }
            else if (obj.Value<string>("method") == "channelEventNotification")//Notification
            {
                string type = obj["params"]!["type"]!.Value<string>()!;
                if (_eventHandlers.ContainsKey(type))
                {
                    _eventHandlers[type](obj["params"]!["data"]!);
                }
            }

            //Debug.Log($"Received: {e.Data}");
        }

        private async UniTask<T> RequestAsync<T>(string method, RequestParams parameter) where T : ResponseResult
        {
            parameter.AppId = Config.AppId;
            parameter.AuthToken = Config.Token;
            var (requestId, task) = _waitTasks.Request();
            var json = JsonConvert.SerializeObject(new JsonRpcRequest
            {
                id = requestId,
                jsonrpc = "2.0",
                method = method,
                parameter = parameter,
            }, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            });
            //Debug.Log($"Sent: {json}");
            _ws.Send(json);
            var resultJson = await task;
            var result = JsonConvert.DeserializeObject<JsonRpcResponse<T>>(resultJson);
            if (result!.error != null)
                throw new JsonRpcException(result.error.code, result.error.message);
            return result.result!;
        }
    }

    public record RtcEvent;
}


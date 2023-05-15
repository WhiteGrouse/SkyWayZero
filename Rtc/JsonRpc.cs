using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SkyWayZero.Rtc
{
    record JsonRpcRequest
    {
        public int? id { get; init; }
        public string jsonrpc { get; init; }
        public string method { get; init; }

        [JsonProperty("params")]
        public RequestParams parameter { get; init; }
    }

    record JsonRpcResponse<T> where T: ResponseResult
    {
        public int? id { get; init; }
        public string jsonrpc { get; init; }

        public JsonRpcError error { get; init; }
        public T result { get; init; }
    }

    record JsonRpcError
    {
        public int code { get; init; }
        public string message { get; init; }
        //"data": { "subject": "" }
    }

    public class JsonRpcException : Exception
    {
        public int Code { get; init; }

        public JsonRpcException(int code, string message) : base(message)
        {
            Code = code;
        }
    }
}


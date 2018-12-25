//#define DisplayHeartbeats
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FireLynx.Trading.Bitfinex.WebSockets.V2.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SuperSocket.ClientEngine;
using WebSocket4Net;
using ErrorEventArgs = SuperSocket.ClientEngine.ErrorEventArgs;

namespace FireLynx.Trading.Bitfinex.WebSockets.V2
{
    public partial class BitfinexWebSocketClient
    {
        //public static bool Debug = false;

        public bool RequireVersion2 { get; set; } = true;

        JsonSerializerSettings serializerSettings;

        #region Construction

        public BitfinexWebSocketClient()
        {
            serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        #endregion

        #region Events

        public event Action<string, TickerEvent> TickerReceived;

        #endregion

        #region State

        WebSocket websocket;
        int? serverVersion = null;

        public Dictionary<int, Channel> Channels => channels;
        private Dictionary<int, Channel> channels = new Dictionary<int, Channel>();

        #region IsConnected

        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                if (isConnected == value)
                {
                    return;
                }
                isConnected = value;
            }
        }
        private bool isConnected;

        public void Connect()
        {
            if (IsConnected) return;

            websocket = new WebSocket("wss://api.bitfinex.com/ws/2");
            websocket.Opened += new EventHandler(Websocket_Opened);
            websocket.Error += new EventHandler<ErrorEventArgs>(Websocket_Error);
            websocket.Closed += new EventHandler(Websocket_Closed);
            websocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(Websocket_MessageReceived);
            websocket.Open();
        }


        public void Disconnect()
        {
            if (websocket != null)
            {
                websocket.Close();
                websocket = null;
                channels.Clear();
            }
        }

        #endregion
        #endregion

        #region Network Event Handlers

        private void OnTradingTickerEvent(Channel channel, TickerEvent tte)
        {
            //Debug.WriteLine(channel.pair + ": " + tte.LastPrice + " vol: " + tte.Volume + " (b: " + tte.Bid + " <" + tte.Spread + "> a: " + tte.Ask + ")  " + tte.DailyChangePercent + "%");
            TickerReceived?.Invoke(channel.pair, tte);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //Debug.WriteLine("[in] " + e.Message);

            if (e.Message.StartsWith("["))
            {
                var jarr = JArray.Parse(e.Message);

                int chanId = jarr[0].Value<int>();

                Channel channel;
                if (!channels.TryGetValue(chanId, out channel)) channel = null;

                if (channel?.pair != null)
                {
                    if (jarr[1].Type == JTokenType.String && jarr[1].Value<string>() == "hb")
                    {
#if DisplayHeartbeats
                            Debug.Write(".");
#endif
                        return;
                    }

                    try
                    {
                        OnTradingTickerEvent(channel, new TickerEvent(jarr[1] as JArray));
                        return;
                    }
                    catch (Exception) { Debug.WriteLine("[exception trying to handle ticker message.  probably not a ticker message]"); }
                }
                {
                    Debug.WriteLine($"[UNKNOWN CHANNEL MESSAGE {channel}] ");
                    for (int i = 1; i < jarr.Count; i++)
                    {
                        Debug.WriteLine($" - " + jarr[i]);
                    }
                    Debug.WriteLine("----");
                }

            }
            else
            {
                var jobj = JObject.Parse(e.Message);

                Debug.WriteLine("[" + jobj["event"].Value<string>() + "]");

                switch (jobj["event"].Value<string>())
                {
                    case null:
                        Debug.WriteLine("[no event] " + e.Message);
                        break;
                    case "subscribed":
                        OnSubscribed(jobj);
                        break;
                    case "pong":
                        OnPong(JsonConvert.DeserializeObject<Pong>(e.Message));
                        break;
                    case "info":
                        OnInfo(JsonConvert.DeserializeObject<Info>(e.Message));
                        break;
                    case "error":
                        Debug.WriteLine("[ERROR] " + e.Message);
                        break;
                    default:
                        Debug.WriteLine("[UNKNOWN] " + e.Message);
                        break;
                }
            }
        }

        private void Websocket_Closed(object sender, EventArgs e)
        {
            Debug.WriteLine("[closed connection] ");
            Disconnected?.Invoke();
        }

        private void Websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Debug.WriteLine("[error] " + e.Exception);
        }

        private void Websocket_Opened(object sender, EventArgs e)
        {
            Debug.WriteLine("[opened] ");
            Ping();
            Opened?.Invoke();
        }

        public event Action Opened;
        public event Action Disconnected;
        #endregion

        #region Outbound messages

        private void Send(object msg)
        {
            websocket.Send(JsonConvert.SerializeObject(msg, serializerSettings));
        }
        private void Send(string msg)
        {
            websocket.Send(msg);
        }

        public void Ping()
        {
            Send(new Ping());
            //websocket.Send(JsonConvert.SerializeObject(new Ping(), serializerSettings));
        }

        public void Conf(ConfFlags flags)
        {
            Send(new Conf { Flags = flags });
        }

        public void SubscribeTickerSymbol(string symbol)
        {
            Send(@"{""event"":""subscribe"",""channel"":""ticker"",""symbol"":""" + symbol + @"""}");
        }
        public void UnsubscribeTickerSymbol(string symbol)
        {
            // TODO
            //Send(@"{""event"":""unsubscribe"",""channel"":""ticker"",""symbol"":""" + symbol + @"""}");
        }
        #endregion

        #region Inbound Messages

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnInfo(Info msg)
        {
            serverVersion = msg.version;
            if(msg.code == 20051)
            {
                Debug.WriteLine("[20051] UNTESTED -- server requested reconnect");
                Disconnect();
                Connect();
                return;
            }
            if (msg.version != 2 && RequireVersion2)
            {
                throw new Exception("RequireVersion2 is true but version provided by server is " + msg.version);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnPong(Pong pong)
        {
        }

        private void OnSubscribed(JObject jObject)
        {
            Channel channel = null;
            switch (jObject["channel"].Value<string>())
            {
                case "ticker":

                    channels.Add(jObject["chanId"].Value<int>(), channel = new Channel { pair = jObject["pair"].Value<string>() });
                    Debug.WriteLine("[subscribed] " + channel);
                    break;
                default:
                    Debug.WriteLine("[UNKNOWN SUBSCRIBED] " + jObject);
                    break;
            }
            //Debug.WriteLine("[subscribed] " + msg.chanId + " " + msg.channel);
        }

        #endregion

        
    }

}

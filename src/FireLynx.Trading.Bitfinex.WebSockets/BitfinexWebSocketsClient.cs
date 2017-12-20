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
    public class BitfinexWebSocketClient
    {
        public bool RequireVersion2 { get; set; } = true;

        JsonSerializerSettings serializerSettings;
        public BitfinexWebSocketClient()
        {
            serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();


        }

        #region State

        WebSocket websocket;
        int? serverVersion = null;

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
            }
        }
        #endregion

        #endregion

        #region Network Event Handlers

        public class TradingTickerEvent
        {
            public enum TickerFields : int
            {
                BID,
                BID_SIZE,
                ASK,
                ASK_SIZE,
                DAILY_CHANGE,
                DAILY_CHANGE_PERC,
                LAST_PRICE,
                VOLUME,
                HIGH,
                LOW
            }

            public TradingTickerEvent() { }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TradingTickerEvent(JArray arr)
            {
                Bid = arr[(int)TickerFields.BID].Value<decimal>();
                BidSize = arr[(int)TickerFields.BID_SIZE].Value<decimal>();
                Ask = arr[(int)TickerFields.ASK].Value<decimal>();
                AskSize = arr[(int)TickerFields.ASK_SIZE].Value<decimal>();
                //DailyChange = arr[(int)TickerFields.DAILY_CHANGE].Value<decimal>();
                DailyChangePercent = arr[(int)TickerFields.DAILY_CHANGE_PERC].Value<decimal>();
                LastPrice = arr[(int)TickerFields.LAST_PRICE].Value<decimal>();
                Volume = arr[(int)TickerFields.VOLUME].Value<decimal>();
                //High = arr[(int)TickerFields.HIGH].Value<decimal>();
                //Low = arr[(int)TickerFields.LOW].Value<decimal>();
            }

            public decimal Bid { get; set; }
            public decimal BidSize { get; set; }
            public decimal Ask { get; set; }
            public decimal AskSize { get; set; }
            public decimal Spread { get { return Ask - Bid; } }
            public decimal DailyChange { get; set; }
            public decimal DailyChangePercent { get; set; }
            public decimal LastPrice { get; set; }
            public decimal Volume { get; set; }
            public decimal High { get; set; }
            public decimal Low { get; set; }

        }

        private void OnTradingTickerEvent(Channel channel, TradingTickerEvent tte)
        {
            Console.WriteLine(channel.pair + ": " + tte.LastPrice + " vol: " + tte.Volume + " (b: " + tte.Bid + " <" + tte.Spread + "> a: " + tte.Ask + ")  " + tte.DailyChangePercent + "%");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //Console.WriteLine("[in] " + e.Message);

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
                            Console.Write(".");
#endif

                        return;
                    }

                    try
                    {
                        OnTradingTickerEvent(channel, new TradingTickerEvent(jarr[1] as JArray));
                        return;
                    }
                    catch (Exception) { Console.WriteLine("[exception trying to handle ticker message.  probably not a ticker message]"); }
                }
                {
                    Console.WriteLine($"[UNKNOWN CHANNEL MESSAGE {channel}] ");
                    for (int i = 1; i < jarr.Count; i++)
                    {
                        Console.WriteLine($" - " + jarr[i]);
                    }
                    Console.WriteLine("----");
                }

            }
            else
            {

                var jobj = JObject.Parse(e.Message);


                Debug.WriteLine("[" + jobj["event"].Value<string>() + "]");

                switch (jobj["event"].Value<string>())
                {
                    case null:
                        Console.WriteLine("[no event] " + e.Message);
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
                        Console.WriteLine("[ERROR] " + e.Message);
                        break;
                    default:
                        Console.WriteLine("[UNKNOWN] " + e.Message);
                        break;
                }
            }
        }



        private void Websocket_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("[closed connection] ");
            Disconnected?.Invoke();
        }

        private void Websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine("[error] " + e.Exception);
        }

        private void Websocket_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("[opened] ");
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
#endregion

#region Inbound Messages

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnInfo(Info msg)
        {
            serverVersion = msg.version;
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
                    Console.WriteLine("[subscribed] " + channel);
                    break;
                default:
                    Console.WriteLine("[UNKNOWN SUBSCRIBED] " + jObject);
                    break;
            }
            //Console.WriteLine("[subscribed] " + msg.chanId + " " + msg.channel);
        }



#endregion

        private Dictionary<int, Channel> channels = new Dictionary<int, Channel>();
    }

    public class Channel
    {
        public string pair { get; set; }

        public override string ToString()
        {
            return pair ?? "(null pair)";
        }
    }

}

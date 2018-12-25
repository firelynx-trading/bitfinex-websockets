using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;

namespace FireLynx.Trading.Bitfinex.WebSockets.V2
{

    public class TickerEvent
    {
        private enum TickerFields : int
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

        public TickerEvent() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TickerEvent(JArray arr)
        {
            Bid = arr[(int)TickerFields.BID].Value<decimal>();
            BidSize = arr[(int)TickerFields.BID_SIZE].Value<decimal>();
            Ask = arr[(int)TickerFields.ASK].Value<decimal>();
            AskSize = arr[(int)TickerFields.ASK_SIZE].Value<decimal>();
            DailyChange = arr[(int)TickerFields.DAILY_CHANGE].Value<decimal>();
            DailyChangePercent = arr[(int)TickerFields.DAILY_CHANGE_PERC].Value<decimal>();
            LastPrice = arr[(int)TickerFields.LAST_PRICE].Value<decimal>();
            Volume = arr[(int)TickerFields.VOLUME].Value<decimal>();
            High = arr[(int)TickerFields.HIGH].Value<decimal>();
            Low = arr[(int)TickerFields.LOW].Value<decimal>();
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

}

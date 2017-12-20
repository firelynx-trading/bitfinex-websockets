using System;
using System.Collections.Generic;
using System.Text;

namespace FireLynx.Trading.Bitfinex.WebSockets.V2.Messages
{
    public class BaseMessage
    {
        public string @event { get; set; }
        public int cid { get; set; }

        public BaseMessage() { }
        public BaseMessage(string @event) { this.@event = @event; }
    }
}


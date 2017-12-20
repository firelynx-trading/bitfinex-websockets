using System;
using System.Collections.Generic;
using System.Text;

namespace FireLynx.Trading.Bitfinex.WebSockets.V2.Messages
{
    public class Error : BaseMessage
    {
        public string channel { get; set; }
        public string symbol { get; set; }
        public string msg { get; set; }

        // Subscribe:
        //10300 : Subscription failed(generic)
        //10301 : Already subscribed
        //10302 : Unknown channel 

        // Unsubscribe:
        //10400 : Subscription failed(generic)
        //10401 : Not subscribed
        public int code { get; set; }

        public string pair { get; set; }
    }
}

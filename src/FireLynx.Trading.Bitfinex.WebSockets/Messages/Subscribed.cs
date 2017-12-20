using System;
using System.Collections.Generic;
using System.Text;

namespace FireLynx.Trading.Bitfinex.WebSockets.V2.Messages
{
    public class Subscribed : BaseMessage
    {
        public Subscribed() : base("subscribed") { }

         public string channel { get; set; }
         public string chanId { get; set; }

    }
}

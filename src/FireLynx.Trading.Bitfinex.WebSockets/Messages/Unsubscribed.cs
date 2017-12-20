using System;
using System.Collections.Generic;
using System.Text;

namespace FireLynx.Trading.Bitfinex.WebSockets.V2.Messages
{
    public class Unsubscribed : BaseMessage
    {
        public Unsubscribed() : base("unsubscribed") { }

         public string channel { get; set; }
         public string chanId { get; set; }

    }
}

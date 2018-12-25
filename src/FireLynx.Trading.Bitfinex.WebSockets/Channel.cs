//#define DisplayHeartbeats

namespace FireLynx.Trading.Bitfinex.WebSockets.V2
{
    public class Channel
    {
        public string pair { get; set; }

        public override string ToString()
        {
            return pair ?? "(null pair)";
        }
    }

}

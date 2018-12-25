namespace FireLynx.Trading.Bitfinex.WebSockets.V2.Messages
{
    #region Server Messages

    public class Info : BaseMessage
    {
        public Info() : base("info") { }

        public int version { get; set; }
        public int code { get; set; }
    }

    #endregion

}


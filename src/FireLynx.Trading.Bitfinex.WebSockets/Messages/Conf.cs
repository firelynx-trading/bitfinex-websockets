using System;
using System.Collections.Generic;
using System.Text;

namespace FireLynx.Trading.Bitfinex.WebSockets.V2.Messages
{

    public enum ConfFlags
    {
        /// <summary>
        /// Enable all decimal as strings.
        /// </summary>
        DEC_S = 8,


       /// <summary>
       /// Enable all times as date strings.
       /// </summary>
        TIME_S = 32,



        /// <summary>
        /// Enable sequencing BETA FEATURE
        /// </summary>
        SEQ_ALL = 65536,

        /// <summary>
        /// Enable checksum for every book iteration. Checks the top 25 entries for each side of book. Checksum is a signed int.
        /// </summary>
        CHECKSUM = 131072,

    }
    public class Conf : BaseMessage
    {
        public Conf() : base("conf") { }

        public ConfFlags Flags { get; set; }
    }
}

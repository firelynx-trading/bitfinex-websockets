using System;
using System.Threading;

namespace FireLynx.Trading.Bitfinex.WebSockets.V2.ConsoleApp
{
    public class App
    {
        BitfinexWebSocketClient client;
        ManualResetEventSlim done = new ManualResetEventSlim(false);

        public void Run()
        {
            client = new BitfinexWebSocketClient();
            Console.Write("Connecting...");
            client.Connect();

            client.Opened += OnOpened;
            client.Disconnected += () => { done.Set(); };
            Console.WriteLine("  connected.  Press a key to disconnect.");
            Console.Read();
            client.Disconnect();
            done.Wait();
        }

        public static string[] Symbols = new string[] { };

        private void OnOpened()
        {
            foreach (var s in Symbols)
            {
                client.SubscribeTickerSymbol("t" + s);
            }
        }
    }

}

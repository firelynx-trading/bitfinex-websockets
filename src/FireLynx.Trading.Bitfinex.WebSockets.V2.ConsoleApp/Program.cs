using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.CommandLineUtils;

namespace FireLynx.Trading.Bitfinex.WebSockets.V2.ConsoleApp
{
    class Program
    {
        public const string ProcessName = "bitfinex";

        static void Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = ProcessName
            };
            app.HelpOption("-?|-h|--help");

            string nameArgumentText = "The name of a symbol.  Examples: BTCUSD LTCUSD";

            app.Command("ticker", (command) =>
            {
                command.Description = "Show tickers for the specified symbols.";
                command.HelpOption("-?|-h|--help");

                var nameArgument = command.Argument("<symbol> [<symbol> ...] ", nameArgumentText, true);

                command.OnExecute(() =>
                {
                    if (nameArgument?.Values.FirstOrDefault() == null) { app.ShowHelp(); return 1; }
                    App.Symbols = nameArgument.Values.ToArray();
                    new App().Run();
                    return 0;
                });
            });

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 1;
            });

            app.Execute(args);
        }
    }

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

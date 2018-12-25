using System.Linq;
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

}

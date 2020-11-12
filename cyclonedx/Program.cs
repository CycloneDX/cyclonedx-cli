using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace CycloneDX.CLI
{
    partial class Program
    {
        public static async Task<int> Main(string[] args)
        {
            RootCommand rootCommand = new RootCommand();
            
            ConfigureConvertCommand(rootCommand);

            return await rootCommand.InvokeAsync(args);
        }
    }
}

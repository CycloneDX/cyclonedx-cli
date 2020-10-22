using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using CycloneDX.Json;
using CycloneDX.Models;
using CycloneDX.Xml;

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

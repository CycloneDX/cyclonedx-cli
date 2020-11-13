using System;
using System.CommandLine;
using System.Threading.Tasks;
using CycloneDX.CLI.Models;

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

        public static BomFormat InputFormatHelper(string inputFile, InputFormat inputFormat)
        {
            BomFormat inputBomFormat = BomFormat.Unsupported;

            if (inputFormat == InputFormat.autodetect)
            {
                if (string.IsNullOrEmpty(inputFile))
                {
                    Console.Error.WriteLine("Unable to auto-detect input stream format, please specify a value for --input-format");
                }
                inputBomFormat = Utils.DetectFileFormat(inputFile);
                if (inputBomFormat == BomFormat.Unsupported)
                {
                    Console.Error.WriteLine("Unable to auto-detect input format from input filename");
                }
            }
            else
            {
                inputBomFormat = (BomFormat)inputFormat;
            }

            return inputBomFormat;
        }
    }
}

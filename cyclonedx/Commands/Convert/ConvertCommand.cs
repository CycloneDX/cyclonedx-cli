using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CycloneDX.Models;
using CycloneDX.CLI.Commands;
using CycloneDX.CLI.Models;

namespace CycloneDX.CLI
{
    partial class Program
    {
        internal static void ConfigureConvertCommand(RootCommand rootCommand)
        {
            var subCommand = new Command("convert");
            subCommand.Add(new Option<string>("--input-file"));
            subCommand.Add(new Option<string>("--output-file"));
            subCommand.Add(new Option<InputFormat>("--input-format"));
            subCommand.Add(new Option<ConvertOutputFormat>("--output-format"));
            subCommand.Handler = CommandHandler.Create<string, string, InputFormat, ConvertOutputFormat>(Convert);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Convert(string inputFile, string outputFile, InputFormat inputFormat, ConvertOutputFormat outputFormat)
        {
            var inputBomFormat = InputFormatHelper(inputFile, inputFormat);
            if (inputBomFormat == BomFormat.Unsupported) return (int)ExitCode.ParameterValidationError;

            BomFormat outputBomFormat = BomFormat.Unsupported;
            string inputBomString;
            Bom inputBom;
            string outputBomString;

            if (outputFormat == ConvertOutputFormat.autodetect)
            {
                if (string.IsNullOrEmpty(outputFile))
                {
                    Console.Error.WriteLine("You must specify a value for --output-format when standard output is used");
                    return (int)ExitCode.ParameterValidationError;
                }
                outputBomFormat = Utils.DetectFileFormat(inputFile);
                if (outputBomFormat == BomFormat.Unsupported)
                {
                    Console.Error.WriteLine("Unable to auto-detect output format from output filename");
                    return (int)ExitCode.ParameterValidationError;
                }
            }
            else
            {
                outputBomFormat = (BomFormat)outputFormat;
            }

            if (!string.IsNullOrEmpty(inputFile))
            {
                inputBomString = File.ReadAllText(inputFile);
            }
            else if (Console.IsInputRedirected)
            {
                var sb = new StringBuilder();
                string nextLine;
                do
                {
                    nextLine = Console.ReadLine();
                    sb.AppendLine(nextLine);
                } while (nextLine != null);
                inputBomString = sb.ToString();
            }
            else
            {
                Console.Error.WriteLine("You must specify a value for --input-file or pipe in an SBOM");
                return (int)ExitCode.ParameterValidationError;
            }
            
            inputBom = Utils.BomDeserializer(inputBomString, inputBomFormat);
            outputBomString = Utils.BomSerializer(inputBom, outputBomFormat);

            if (string.IsNullOrEmpty(outputFile))
            {
                Console.Write(outputBomString);
            }
            else
            {
                Console.WriteLine("Writing output file...");
                await File.WriteAllTextAsync(outputFile, outputBomString);
            }

            return (int)ExitCode.Ok;
        }
    }
}

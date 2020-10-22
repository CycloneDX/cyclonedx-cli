using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using CycloneDX.Json;
using CycloneDX.Models;
using CycloneDX.Xml;

namespace CycloneDX.CLI
{
    partial class Program
    {
        internal static void ConfigureConvertCommand(RootCommand rootCommand)
        {
            var subCommand = new Command("convert");
            subCommand.Add(new Argument<string>("input-file"));
            subCommand.Add(new Argument<string>("output-file"));
            subCommand.Handler = CommandHandler.Create<string, string>(Convert);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Convert(string inputFile, string outputFile)
        {
            Bom inputBom;
            string outputBomString;

            var inputFileFormat = Utils.FileFormatFromFilename(inputFile);
            var outputFileFormat = Utils.FileFormatFromFilename(outputFile);

            Console.WriteLine("Reading input file...");
            var inputBomString = await File.ReadAllTextAsync(inputFile);

            try
            {
                inputBom = Utils.BomDeserializer(inputBomString, inputFileFormat);
            }
            catch (UnsupportedFormatException)
            {
                Console.WriteLine("Unsupported input file format");
                return (int)ExitCode.UnsupportedFormat;
            }

            try
            {
                outputBomString = Utils.BomSerializer(inputBom, outputFileFormat);
            }
            catch (UnsupportedFormatException)
            {
                Console.WriteLine("Unsupported output file format");
                return (int)ExitCode.UnsupportedFormat;
            }

            Console.WriteLine("Writing output file...");
            await File.WriteAllTextAsync(outputFile, outputBomString);

            return (int)ExitCode.Ok;
        }
    }
}

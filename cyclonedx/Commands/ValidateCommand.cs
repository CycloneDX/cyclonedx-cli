using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CycloneDX.Models;
using CycloneDX.Json;
using CycloneDX.Xml;

namespace CycloneDX.CLI
{
    internal static class ValidateCommand
    {
        public enum InputFormat
        {
            autodetect,
            json,
            json_v1_2,
            xml,
            xml_v1_2,
            xml_v1_1,
            xml_v1_0,
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812: Avoid uninstantiated internal classes")]
        public class Options
        {
            public string InputFile { get; set; }
            public InputFormat InputFormat { get; set; }
            public bool FailOnErrors { get; set; }
        }

        internal static void Configure(RootCommand rootCommand)
        {
            var subCommand = new Command("validate", "Validate an SBOM");
            subCommand.Add(new Option<string>("--input-file", "Input SBOM filename, will read from stdin if no value provided."));
            subCommand.Add(new Option<InputFormat>("--input-format", "Specify input file format."));
            subCommand.Add(new Option<bool>("--fail-on-errors", "Fail on validation errors (return a non-zero exit code)"));
            subCommand.Handler = CommandHandler.Create<Options>(Validate);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Validate(Options options)
        {
            ValidateInputFormat(options);
            if (options.InputFormat == InputFormat.autodetect)
            {
                Console.Error.WriteLine("Unable to auto-detect input format");
                return (int)ExitCode.ParameterValidationError;
            }

            var inputBom = ReadInput(options);
            if (inputBom == null)
            {
                Console.Error.WriteLine("Error reading input, you must specify a value for --input-file or pipe in content");
                return (int)ExitCode.IOError;
            }

            SchemaVersion schemaVersion = SchemaVersion.v1_2;

            switch (options.InputFormat)
            {
                case InputFormat.xml_v1_1:
                    schemaVersion = SchemaVersion.v1_1;
                    break;
                case InputFormat.xml_v1_0:
                    schemaVersion = SchemaVersion.v1_0;
                    break;
            }

            ValidationResult validationResult;

            if (options.InputFormat.ToString().StartsWith("json", StringComparison.InvariantCulture))
            {
                Console.WriteLine("Validating JSON SBOM...");
                validationResult = await Json.Validator.Validate(inputBom, schemaVersion);
            }
            else
            {
                Console.WriteLine("Validating XML SBOM...");
                validationResult = await Json.Validator.Validate(inputBom, schemaVersion);
            }

            if (validationResult.Messages != null)
            foreach (var message in validationResult.Messages)
            {
                Console.WriteLine(message);
            }

            if (options.FailOnErrors && !validationResult.Valid)
            {
                return (int)ExitCode.OkFail;
            }

            return (int)ExitCode.Ok;
        }

        static void ValidateInputFormat(Options options)
        {
            if (options.InputFormat == InputFormat.autodetect && !string.IsNullOrEmpty(options.InputFile))
            {
                if (options.InputFile.EndsWith(".json", StringComparison.InvariantCulture))
                {
                    options.InputFormat = InputFormat.json;
                }
                else if (options.InputFile.EndsWith(".xml", StringComparison.InvariantCulture))
                {
                    options.InputFormat = InputFormat.xml;
                }
            }
            
            if (options.InputFormat == InputFormat.json)
            {
                options.InputFormat = InputFormat.json_v1_2;
            }
            else if (options.InputFormat == InputFormat.xml)
            {
                options.InputFormat = InputFormat.xml_v1_2;
            }
        }

        static string ReadInput(Options options)
        {
            string inputString = null;
            if (!string.IsNullOrEmpty(options.InputFile))
            {
                inputString = File.ReadAllText(options.InputFile);
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
                inputString = sb.ToString();
            }
            return inputString;
        }
    }
}

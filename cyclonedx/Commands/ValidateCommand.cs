using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Json.Schema;
using CycloneDX.Models.v1_2;
using CycloneDX.Json;
using CycloneDX.CLI.Commands;
using CycloneDX.CLI.Models;

namespace CycloneDX.CLI
{
    internal class ValidateCommand
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

            var validated = false;

            if (options.InputFormat.ToString().StartsWith("json"))
            {
                Console.WriteLine("Validating JSON SBOM...");
                validated = await ValidateJson(options, inputBom);
            }
            else if (options.InputFormat.ToString().StartsWith("xml"))
            {
                Console.WriteLine("Validating XML SBOM...");
                validated = ValidateXml(options, inputBom);
            }

            if (options.FailOnErrors && !validated)
            {
                return (int)ExitCode.OkFail;
            }

            return (int)ExitCode.Ok;
        }

        static void ValidateInputFormat(Options options)
        {
            if (options.InputFormat == InputFormat.autodetect && !string.IsNullOrEmpty(options.InputFile))
            {
                if (options.InputFile.EndsWith(".json"))
                {
                    options.InputFormat = InputFormat.json;
                }
                else if (options.InputFile.EndsWith(".xml"))
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

        static bool ValidateXml(Options options, string sbomContents)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            var schemaVersion = options.InputFormat.ToString().Substring(5).Replace('_', '.');
            Console.WriteLine($"Using schema v{schemaVersion}");

            var schemaContent = Assembly.GetExecutingAssembly().GetManifestResourceStream($"cyclonedx.Schemas.bom-{schemaVersion}.xsd");
            var spdxSchemaContent = Assembly.GetExecutingAssembly().GetManifestResourceStream($"cyclonedx.Schemas.spdx.xsd");

            settings.Schemas.Add(XmlSchema.Read(schemaContent, null));
            settings.Schemas.Add(XmlSchema.Read(spdxSchemaContent, null));

            settings.ValidationType = ValidationType.Schema;
            
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(sbomContents);
            writer.Flush();
            stream.Position = 0;

            XmlReader reader = XmlReader.Create(stream, settings);
            XmlDocument document = new XmlDocument();

            try
            {
                document.Load(reader);

                Console.WriteLine("SBOM successfully validated");
                return true;
            }
            catch (XmlSchemaValidationException exc)
            {
                var lineInfo = ((IXmlLineInfo)reader);
                if (lineInfo.HasLineInfo()) {
                    Console.Error.WriteLine($"Validation failed at line number {lineInfo.LineNumber} and position {lineInfo.LinePosition}: {exc.Message}");
                }
                else
                {
                    Console.Error.WriteLine($"Validation failed at position {stream.Position}: {exc.Message}");
                }
                return false;
            }
        }

        static async Task<bool> ValidateJson(Options options, string sbomContents)
        {
            var schemaVersion = options.InputFormat.ToString().Substring(6).Replace('_', '.');
            Console.WriteLine($"Using schema v{schemaVersion}");
            var schemaContent = Assembly.GetExecutingAssembly().GetManifestResourceStream($"cyclonedx.Schemas.bom-{schemaVersion}.schema.json");
            var spdxSchemaContent = Assembly.GetExecutingAssembly().GetManifestResourceStream($"cyclonedx.Schemas.spdx.schema.json");

            var schema = await JsonSchema.FromStream(schemaContent);
            var spdxSchema = await JsonSchema.FromStream(spdxSchemaContent);

            SchemaRegistry.Global.Register(new Uri("file://spdx.schema.json"), spdxSchema);

            var jsonDocument = JsonDocument.Parse(sbomContents);
            var validationOptions = new ValidationOptions
            {
                OutputFormat = OutputFormat.Detailed
            };

            var result = schema.Validate(jsonDocument.RootElement, validationOptions);
            if (result.IsValid)
            {
                Console.WriteLine("SBOM successfully validated");
                return true;
            }
            else
            {
                Console.WriteLine($"Validation failed: {result.Message}");
                Console.WriteLine(result.SchemaLocation);

                if (result.NestedResults != null)
                {
                    var nestedResults = new Queue<ValidationResults>(result.NestedResults);

                    while (nestedResults.Count > 0)
                    {
                        var nestedResult = nestedResults.Dequeue();

                        if (
                            !string.IsNullOrEmpty(nestedResult.Message)
                            && nestedResult.NestedResults != null
                            && nestedResult.NestedResults.Count > 0)
                        {
                            Console.WriteLine($"{nestedResult.InstanceLocation}: {nestedResult.Message}");
                        }
                        
                        if (nestedResult.NestedResults != null)
                        {
                            foreach (var newNestedResult in nestedResult.NestedResults)
                            {
                                nestedResults.Enqueue(newNestedResult);
                            }
                        }
                    }
                }

                return false;
            }
        }
    }
}

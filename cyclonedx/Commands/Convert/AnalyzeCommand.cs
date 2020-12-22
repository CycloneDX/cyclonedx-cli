using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CycloneDX.Models.v1_2;
using CycloneDX.CLI.Commands;
using CycloneDX.CLI.Models;

namespace CycloneDX.CLI
{
    partial class Program
    {
        internal static void ConfigureAnalyzeCommand(RootCommand rootCommand)
        {
            var subCommand = new Command("analyze", "Analyze an SBOM file");
            subCommand.Add(new Option<string>("--input-file", "Input SBOM filename, will read from stdin if no value provided."));
            subCommand.Add(new Option<InputFormat>("--input-format", "Specify input file format."));
            subCommand.Add(new Option<StandardOutputFormat>("--output-format", "Specify output file format (defaults to text)."));
            subCommand.Add(new Option<bool>("--multiple-component-versions", "Report components that have multiple versions in use."));
            subCommand.Handler = CommandHandler.Create<string, InputFormat, StandardOutputFormat, bool>(Analyze);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Analyze(
            string inputFile, InputFormat inputFormat, StandardOutputFormat outputFormat,
            bool multipleComponentVersions)
        {
            var inputBomFormat = InputFormatHelper(inputFile, inputFormat);
            if (inputBomFormat == BomFormat.Unsupported) return (int)ExitCode.ParameterValidationError;

            var inputBomString = InputFileHelper(inputFile);
            if (inputBomString == null) return (int)ExitCode.ParameterValidationError;
            
            var inputBom = Utils.BomDeserializer(inputBomString, inputBomFormat);

            var result = new AnalyzeResult();

            if (multipleComponentVersions)
            {
                result.MultipleComponentVersions = new List<List<Component>>();

                var componentCache = new Dictionary<string, List<Component>>();
                foreach (var component in inputBom.Components)
                {
                    if (!componentCache.ContainsKey(component.Name))
                    {
                        componentCache[component.Name] = new List<Component>();
                    }
                    componentCache[component.Name].Add(component);
                }

                foreach (var componentEntry in componentCache)
                {
                    if (componentEntry.Value.Count > 1)
                    {
                        var firstVersion = componentEntry.Value.First().Version;
                        foreach (var component in componentEntry.Value)
                        {
                            if (component.Version != firstVersion)
                            {
                                result.MultipleComponentVersions.Add(componentEntry.Value);
                                break;
                            }
                        }
                    }
                }
            }

            string outputString;

            if (outputFormat == StandardOutputFormat.json)
            {
                outputString = JsonSerializer.Serialize(result);
            }
            else
            {
                var sb = new StringBuilder();

                if (result.MultipleComponentVersions != null)
                {
                    sb.AppendLine("Components with multiple versions:");
                    sb.AppendLine();
                    if (result.MultipleComponentVersions.Count == 0)
                    {
                        sb.AppendLine("None");
                    }
                    else
                    {
                        foreach (var componentEntry in result.MultipleComponentVersions)
                        {
                            sb.Append(componentEntry.First().Name);
                            sb.Append(" versions:");
                            foreach (var component in componentEntry)
                            {
                                sb.Append(" ");
                                sb.Append(component.Version);
                            }
                        }
                    }
                    sb.AppendLine();
                }

                outputString = sb.ToString();
            }

            Console.Write(outputString);

            return (int)ExitCode.Ok;
        }
    }
}

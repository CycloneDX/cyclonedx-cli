using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CycloneDX.Models.v1_3;
using CycloneDX.Json;
using CycloneDX.Utils;
using CycloneDX.CLI.Commands;
using CycloneDX.CLI.Models;

namespace CycloneDX.CLI
{
    partial class Program
    {
        internal static void ConfigureDiffCommand(RootCommand rootCommand)
        {
            var subCommand = new Command("diff", "Generate a BOM diff");
            subCommand.Add(new Argument<string>("from-file", "From BOM filename."));
            subCommand.Add(new Argument<string>("to-file", "To BOM filename."));
            subCommand.Add(new Option<InputFormat>("--from-format", "Specify from file format."));
            subCommand.Add(new Option<InputFormat>("--to-format", "Specify to file format."));
            subCommand.Add(new Option<StandardOutputFormat>("--output-format", "Specify output format (defaults to text)."));
            subCommand.Add(new Option<bool>("--component-versions", "Report component versions that have been added, removed or modified."));
            subCommand.Handler = CommandHandler.Create<string, string, InputFormat, InputFormat, StandardOutputFormat, bool>(Diff);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Diff(
            string fromFile, string toFile, InputFormat fromFormat, InputFormat toFormat, StandardOutputFormat outputFormat,
            bool componentVersions)
        {
            var fromBomFormat = InputFormatHelper(fromFile, fromFormat);
            var toBomFormat = InputFormatHelper(toFile, toFormat);
            if (fromBomFormat == BomFormat.Unsupported || toBomFormat == BomFormat.Unsupported) return (int)ExitCode.ParameterValidationError;

            var fromBomString = await File.ReadAllTextAsync(fromFile);
            var toBomString = await File.ReadAllTextAsync(toFile);
            
            var fromBom = CLIUtils.BomDeserializer(fromBomString, fromBomFormat);
            var toBom = CLIUtils.BomDeserializer(toBomString, toBomFormat);

            var result = new DiffResult();

            if (componentVersions)
            {
                result.ComponentVersions = CycloneDXUtils.ComponentVersionDiff(fromBom, toBom);
            }

            if (outputFormat == StandardOutputFormat.json)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreNullValues = true,
                };

                options.Converters.Add(new Json.Converters.v1_2.ComponentTypeConverter());
                options.Converters.Add(new Json.Converters.v1_2.DataFlowConverter());
                options.Converters.Add(new Json.Converters.v1_2.DateTimeConverter());
                options.Converters.Add(new Json.Converters.v1_2.DependencyConverter());
                options.Converters.Add(new Json.Converters.v1_2.ExternalReferenceTypeConverter());
                options.Converters.Add(new Json.Converters.v1_2.HashAlgorithmConverter());
                options.Converters.Add(new Json.Converters.v1_2.IssueClassificationConverter());
                options.Converters.Add(new Json.Converters.v1_2.LicenseConverter());
                options.Converters.Add(new Json.Converters.v1_2.PatchClassificationConverter());

                Console.WriteLine(JsonSerializer.Serialize(result, options));
            }
            else
            {
                if (result.ComponentVersions != null)
                {
                    Console.WriteLine("Component versions that have changed:");
                    Console.WriteLine();

                    var changes = false;
                    foreach (var entry in result.ComponentVersions)
                    {
                        var componentDiffItem = entry.Value;
                        if (componentDiffItem.Added.Count > 0 || componentDiffItem.Removed.Count > 0)
                        {
                            changes = true;
                            foreach (var component in componentDiffItem.Removed)
                            {
                                Console.WriteLine($"- {component.Group} {component.Name} @ {component.Version}");
                            }
                            foreach (var component in componentDiffItem.Unchanged)
                            {
                                Console.WriteLine($"= {component.Group} {component.Name} @ {component.Version}");
                            }
                            foreach (var component in componentDiffItem.Added)
                            {
                                Console.WriteLine($"+ {component.Group} {component.Name} @ {component.Version}");
                            }
                            Console.WriteLine();
                        }
                    }

                    if (!changes)
                    {
                        Console.WriteLine("None");
                    }
                    
                    Console.WriteLine();
                }
            }

            return (int)ExitCode.Ok;
        }
    }
}

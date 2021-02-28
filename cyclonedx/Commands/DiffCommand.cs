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
using CycloneDX.Json;
using CycloneDX.CLI.Commands;
using CycloneDX.CLI.Models;

namespace CycloneDX.CLI
{
    partial class Program
    {
        internal static void ConfigureDiffCommand(RootCommand rootCommand)
        {
            var subCommand = new Command("diff", "Generate an SBOM diff");
            subCommand.Add(new Argument<string>("from-file", "From SBOM filename."));
            subCommand.Add(new Argument<string>("to-file", "To SBOM filename."));
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
                result.ComponentVersions = new Dictionary<string, DiffItem<Component>>();

                // make a copy of components that are still to be processed
                var fromComponents = new List<Component>(fromBom.Components);
                var toComponents = new List<Component>(toBom.Components);
                
                // unchanged component versions
                // loop over the toBom and fromBom Components list as we will be modifying the fromComponents list
                foreach (var fromComponent in fromBom.Components)
                {
                    // if component version is in both SBOMs
                    if (toBom.Components.Count(toComponent =>
                            toComponent.Group == fromComponent.Group
                            && toComponent.Name == fromComponent.Name
                            && toComponent.Version == fromComponent.Version
                        ) > 0)
                    {
                        var componentIdentifier = $"{fromComponent.Group}:{fromComponent.Name}";

                        if (!result.ComponentVersions.ContainsKey(componentIdentifier))
                        {
                            result.ComponentVersions.Add(componentIdentifier, new DiffItem<Component>());
                        }

                        result.ComponentVersions[componentIdentifier].Unchanged.Add(fromComponent);

                        fromComponents.RemoveAll(c => c.Group == fromComponent.Group && c.Name == fromComponent.Name && c.Version == fromComponent.Version);
                        toComponents.RemoveAll(c => c.Group == fromComponent.Group && c.Name == fromComponent.Name && c.Version == fromComponent.Version);
                    }
                }

                // added component versions
                foreach (var component in new List<Component>(toComponents))
                {
                    var componentIdentifier = $"{component.Group}:{component.Name}";
                    if (!result.ComponentVersions.ContainsKey(componentIdentifier))
                    {
                        result.ComponentVersions.Add(componentIdentifier, new DiffItem<Component>());
                    }

                    result.ComponentVersions[componentIdentifier].Added.Add(component);
                }

                // removed components versions
                foreach (var component in new List<Component>(fromComponents))
                {
                    var componentIdentifier = $"{component.Group}:{component.Name}";
                    if (!result.ComponentVersions.ContainsKey(componentIdentifier))
                    {
                        result.ComponentVersions.Add(componentIdentifier, new DiffItem<Component>());
                    }

                    result.ComponentVersions[componentIdentifier].Removed.Add(component);
                }

            }

            if (outputFormat == StandardOutputFormat.json)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreNullValues = true,
                };

                options.Converters.Add(new Json.v1_2.Converters.ComponentTypeConverter());
                options.Converters.Add(new Json.v1_2.Converters.DataFlowConverter());
                options.Converters.Add(new Json.v1_2.Converters.DateTimeConverter());
                options.Converters.Add(new Json.v1_2.Converters.DependencyConverter());
                options.Converters.Add(new Json.v1_2.Converters.ExternalReferenceTypeConverter());
                options.Converters.Add(new Json.v1_2.Converters.HashAlgorithmConverter());
                options.Converters.Add(new Json.v1_2.Converters.IssueClassificationConverter());
                options.Converters.Add(new Json.v1_2.Converters.LicenseConverter());
                options.Converters.Add(new Json.v1_2.Converters.PatchClassificationConverter());

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

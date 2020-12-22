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

            var fromBomString = File.ReadAllText(fromFile);
            var toBomString = File.ReadAllText(toFile);
            
            var fromBom = Utils.BomDeserializer(fromBomString, fromBomFormat);
            var toBom = Utils.BomDeserializer(toBomString, toBomFormat);

            var result = new DiffResult();

            if (componentVersions)
            {
                result.ComponentVersions = new DiffItem<Component>();

                // there is some complexity here, components could be included
                // multiple times, with the same or different versions

                // make a copy of components that are still to be processed
                var fromComponents = new List<Component>(fromBom.Components);
                var toComponents = new List<Component>(toBom.Components);
                
                // ditch component versions that are in both
                foreach (var component in fromBom.Components)
                {
                    if (toBom.Components.Count(c => c.Name == component.Name && c.Version == component.Version) > 0)
                    {
                        fromComponents.RemoveAll(c => c.Name == component.Name && c.Version == component.Version);
                        toComponents.RemoveAll(c => c.Name == component.Name && c.Version == component.Version);
                    }
                }

                // find obviously added components
                // take a copy of toComponents as we are modifying it
                foreach (var component in new List<Component>(toComponents))
                {
                    if (fromComponents.Count(c => c.Name == component.Name) == 0)
                    {
                        result.ComponentVersions.Added.Add(component);
                        toComponents.RemoveAll(c => c.Name == component.Name && c.Version == component.Version);
                    }
                }

                // find obviously removed components
                // take a copy of fromComponents as we are modifying it
                foreach (var component in new List<Component>(fromComponents))
                {
                    if (toComponents.Count(c => c.Name == component.Name) == 0)
                    {
                        result.ComponentVersions.Removed.Add(component);
                        fromComponents.RemoveAll(c => c.Name == component.Name && c.Version == component.Version);
                    }
                }

                // now we should have modified components left over
                //
                // but this situation is possible (or the reverse)
                //
                // From:
                // Component A v1.0.0
                //
                // To:
                // Component A v1.0.1
                // Component A v1.0.2
                //
                // (╯°□°）╯︵ ┻━┻
                //
                // we'll treat the first match as modified, and any others as
                // added or removed

                foreach (var fromComponent in new List<Component>(fromComponents))
                {
                    var toComponent = toComponents.First(c => c.Name == fromComponent.Name);
                    if (toComponent != null)
                    {
                        result.ComponentVersions.Modified.Add(new ModifiedDiffItem<Component>
                        {
                            From = fromComponent,
                            To = toComponent
                        });
                        
                        fromComponents.RemoveAll(c => c.Name == fromComponent.Name && c.Version == fromComponent.Version);
                        toComponents.RemoveAll(c => c.Name == toComponent.Name && c.Version == toComponent.Version);
                    }
                }
                // now everything left over we'll treat as added or removed
                result.ComponentVersions.Removed.AddRange(fromComponents);
                result.ComponentVersions.Added.AddRange(toComponents);
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
                    if (
                        result.ComponentVersions.Removed.Count == 0
                        && result.ComponentVersions.Modified.Count == 0
                        && result.ComponentVersions.Removed.Count == 0
                    )
                    {
                        Console.WriteLine("None");
                    }
                    else
                    {
                        foreach (var component in result.ComponentVersions.Added)
                        {
                            Console.WriteLine($"+ {component.Name}@{component.Version}");
                            Console.WriteLine();
                        }
                        foreach (var component in result.ComponentVersions.Modified)
                        {
                            Console.WriteLine($"- {component.From.Name}@{component.From.Version}");
                            Console.WriteLine($"+ {component.To.Name}@{component.To.Version}");
                            Console.WriteLine();
                        }
                        foreach (var component in result.ComponentVersions.Removed)
                        {
                            Console.WriteLine($"- {component.Name}@{component.Version}");
                            Console.WriteLine();
                        }
                    }
                    Console.WriteLine();
                }
            }

            return (int)ExitCode.Ok;
        }
    }
}

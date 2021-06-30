// This file is part of CycloneDX CLI Tool
//
// Licensed under the Apache License, Version 2.0 (the “License”);
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an “AS IS” BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// SPDX-License-Identifier: Apache-2.0
// Copyright (c) Patrick Dwyer. All Rights Reserved.
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

            await using var fromBomStream = File.OpenRead(fromFile);
            await using var toBomStream = File.OpenRead(toFile);
            
            var fromBom = CLIUtils.BomDeserializer(fromBomStream, fromBomFormat);
            var toBom = CLIUtils.BomDeserializer(toBomStream, toBomFormat);

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

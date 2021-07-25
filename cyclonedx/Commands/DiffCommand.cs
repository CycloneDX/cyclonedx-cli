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
// Copyright (c) OWASP Foundation. All Rights Reserved.
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using System.Threading.Tasks;
using CycloneDX.Cli.Models;
using CycloneDX.Utils;

namespace CycloneDX.Cli
{
    internal static class DiffCommand
    {
        public class Options
        {
            public string FromFile { get; set; }    
            public string ToFile { get; set; }
            public StandardInputOutputBomFormat FromFormat { get; set; }
            public StandardInputOutputBomFormat ToFormat { get; set; }
            public StandardCommandOutputFormat OutputFormat { get; set; }
            public bool ComponentVersions { get; set; }
        }
        
        internal static void Configure(RootCommand rootCommand)
        {
            var subCommand = new Command("diff", "Generate a BOM diff");
            subCommand.Add(new Argument<string>("from-file", "From BOM filename."));
            subCommand.Add(new Argument<string>("to-file", "To BOM filename."));
            subCommand.Add(new Option<StandardInputOutputBomFormat>("--from-format", "Specify from file format."));
            subCommand.Add(new Option<StandardInputOutputBomFormat>("--to-format", "Specify to file format."));
            subCommand.Add(new Option<StandardCommandOutputFormat>("--output-format", "Specify output format (defaults to text)."));
            subCommand.Add(new Option<bool>("--component-versions", "Report component versions that have been added, removed or modified."));
            subCommand.Handler = CommandHandler.Create<Options>(Diff);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Diff(Options options)
        {
            var fromBom = CliUtils.InputBomHelper(options.FromFile, options.FromFormat);
            if (fromBom == null) return (int)ExitCode.ParameterValidationError;
            var toBom = CliUtils.InputBomHelper(options.ToFile, options.ToFormat);
            if (toBom == null) return (int)ExitCode.ParameterValidationError;

            var result = new DiffResult();

            if (options.ComponentVersions)
            {
                result.ComponentVersions = CycloneDXUtils.ComponentVersionDiff(fromBom, toBom);
            }

            if (options.OutputFormat == StandardCommandOutputFormat.json)
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreNullValues = true,
                };

                jsonOptions.Converters.Add(new Json.Converters.v1_2.ComponentTypeConverter());
                jsonOptions.Converters.Add(new Json.Converters.v1_2.DataFlowConverter());
                jsonOptions.Converters.Add(new Json.Converters.v1_2.DateTimeConverter());
                jsonOptions.Converters.Add(new Json.Converters.v1_2.DependencyConverter());
                jsonOptions.Converters.Add(new Json.Converters.v1_2.ExternalReferenceTypeConverter());
                jsonOptions.Converters.Add(new Json.Converters.v1_2.HashAlgorithmConverter());
                jsonOptions.Converters.Add(new Json.Converters.v1_2.IssueClassificationConverter());
                jsonOptions.Converters.Add(new Json.Converters.v1_2.LicenseConverter());
                jsonOptions.Converters.Add(new Json.Converters.v1_2.PatchClassificationConverter());

                Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
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

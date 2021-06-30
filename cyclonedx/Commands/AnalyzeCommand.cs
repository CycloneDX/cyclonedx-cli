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
using CycloneDX.CLI.Commands;
using CycloneDX.CLI.Models;
using CycloneDX.Utils;

namespace CycloneDX.CLI
{
    partial class Program
    {
        internal static void ConfigureAnalyzeCommand(RootCommand rootCommand)
        {
            var subCommand = new Command("analyze", "Analyze a BOM file");
            subCommand.Add(new Option<string>("--input-file", "Input BOM filename, will read from stdin if no value provided."));
            subCommand.Add(new Option<InputFormat>("--input-format", "Specify input file format."));
            subCommand.Add(new Option<StandardOutputFormat>("--output-format", "Specify output format (defaults to text)."));
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

            var inputBomStream = InputFileHelper(inputFile);
            if (inputBomStream == null) return (int)ExitCode.ParameterValidationError;
            
            var inputBom = CLIUtils.BomDeserializer(inputBomStream, inputBomFormat);

            var result = new AnalyzeResult();

            if (multipleComponentVersions)
            {
                result.MultipleComponentVersions = CycloneDXUtils.MultipleComponentVersions(inputBom);
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
                if (inputBom.Metadata?.Component != null)
                {
                    var component = inputBom.Metadata.Component;
                    Console.WriteLine($"Analysis results for {component.Name}@{component.Version}:");
                }
                else
                {
                    Console.WriteLine("Analysis results:");
                }
                Console.WriteLine();

                if (result.MultipleComponentVersions != null)
                {
                    Console.WriteLine("Components with multiple versions:");
                    Console.WriteLine();
                    if (result.MultipleComponentVersions.Count == 0)
                    {
                        Console.WriteLine("None");
                    }
                    else
                    {
                        foreach (var componentEntry in result.MultipleComponentVersions)
                        {
                            Console.Write(componentEntry.Key);
                            Console.Write(" versions:");
                            foreach (var component in componentEntry.Value)
                            {
                                Console.Write(" ");
                                Console.Write(component.Version);
                            }
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

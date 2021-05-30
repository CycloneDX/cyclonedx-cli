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
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using CycloneDX.Json;
using CycloneDX.Models.v1_3;
using CycloneDX.Xml;
using CycloneDX.Utils;

namespace CycloneDX.CLI
{
    partial class Program
    {
        public class MergeCommandOptions
        {
            public List<string> InputFiles { get; set; } = new List<string>();
            public string OutputFile { get; set; }
            public StandardInputOutputSbomFormat InputFormat { get; set; }
            public StandardInputOutputSbomFormat OutputFormat { get; set; }
        }

        internal static void ConfigureMergeCommand(RootCommand rootCommand)
        {
            var subCommand = new Command("merge", "Merge two or more BOMs");
            subCommand.Add(new Option<List<string>>("--input-files", "Input BOM filenames (separate filenames with a space)."));
            subCommand.Add(new Option<string>("--output-file", "Output BOM filename, will write to stdout if no value provided."));
            subCommand.Add(new Option<StandardInputOutputSbomFormat>("--input-format", "Specify input file format."));
            subCommand.Add(new Option<StandardInputOutputSbomFormat>("--output-format", "Specify output file format."));
            subCommand.Handler = CommandHandler.Create<MergeCommandOptions>(Merge);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Merge(MergeCommandOptions options)
        {
            Contract.Requires(options != null);

            var outputToConsole = string.IsNullOrEmpty(options.OutputFile);

            var outputFormat = options.OutputFormat;
            if (outputFormat == StandardInputOutputSbomFormat.autodetect)
            {
                if (options.OutputFile != null && options.OutputFile.EndsWith(".json", StringComparison.InvariantCulture))
                {
                    outputFormat = StandardInputOutputSbomFormat.json;
                }
                else if (options.OutputFile != null && options.OutputFile.EndsWith(".xml", StringComparison.InvariantCulture))
                {
                    outputFormat = StandardInputOutputSbomFormat.xml;
                }
                else
                {
                    Console.WriteLine($"Unable to auto-detect output format");
                    return (int)ExitCode.ParameterValidationError;
                }

            }

            var outputBom = new Bom();

            foreach (var inputFilename in options.InputFiles)
            {
                if (!outputToConsole) Console.WriteLine($"Processing input file {inputFilename}");
                var inputFormat = options.InputFormat;
                if (inputFormat == StandardInputOutputSbomFormat.autodetect)
                {
                    if (inputFilename.EndsWith(".json", StringComparison.InvariantCulture))
                    {
                        inputFormat = StandardInputOutputSbomFormat.json;
                    }
                    else if (inputFilename.EndsWith(".xml", StringComparison.InvariantCulture))
                    {
                        inputFormat = StandardInputOutputSbomFormat.xml;
                    }
                    else
                    {
                        Console.WriteLine($"Unable to auto-detect format of {inputFilename}");
                        return (int)ExitCode.ParameterValidationError;
                    }

                }

                var bomContents = await File.ReadAllTextAsync(inputFilename);

                Bom inputBom;
                if (inputFormat == StandardInputOutputSbomFormat.json)
                {
                    inputBom = Json.Deserializer.Deserialize(bomContents);
                }
                else
                {
                    inputBom = Xml.Deserializer.Deserialize(bomContents);
                }

                outputBom = CycloneDXUtils.Merge(outputBom, inputBom);
                if (inputBom.Components != null && !outputToConsole)
                    Console.WriteLine($"    Contains {inputBom.Components.Count} components");
            }

            string outputBomString;
            if (outputFormat == StandardInputOutputSbomFormat.json)
            {
                outputBomString = Json.Serializer.Serialize(outputBom);
            }
            else
            {
                outputBomString = Xml.Serializer.Serialize(outputBom);
            }

            if (outputToConsole)
            {
                Console.WriteLine(outputBomString);
            }
            else
            {
                Console.WriteLine("Writing output file...");
                Console.WriteLine($"    Total {outputBom.Components.Count} components");
                await File.WriteAllTextAsync(options.OutputFile, outputBomString);
            }

            return (int)ExitCode.Ok;
        }
    }
}

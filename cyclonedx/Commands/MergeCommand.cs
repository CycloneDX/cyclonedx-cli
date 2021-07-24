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
using System.Threading.Tasks;
using CycloneDX.Models.v1_3;
using CycloneDX.Utils;

namespace CycloneDX.Cli
{
    public static class MergeCommand
    {
        public class Options
        {
            public List<string> InputFiles { get; set; }
            public string OutputFile { get; set; }
            public StandardInputOutputBomFormat InputFormat { get; set; }
            public StandardInputOutputBomFormat OutputFormat { get; set; }
            public bool Hierarchical { get; set; }
        }
        
        public static void Configure(RootCommand rootCommand)
        {
            var subCommand = new Command("merge", "Merge two or more BOMs");
            subCommand.Add(new Option<List<string>>("--input-files", "Input BOM filenames (separate filenames with a space)."));
            subCommand.Add(new Option<string>("--output-file", "Output BOM filename, will write to stdout if no value provided."));
            subCommand.Add(new Option<StandardInputOutputBomFormat>("--input-format", "Specify input file format."));
            subCommand.Add(new Option<StandardInputOutputBomFormat>("--output-format", "Specify output file format."));
            subCommand.Add(new Option<bool>("--hierarchical", "Perform a hierarchical merge."));
            subCommand.Handler = CommandHandler.Create<Options>(Merge);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Merge(Options options)
        {
            var outputToConsole = string.IsNullOrEmpty(options.OutputFile);
            
            if (options.OutputFormat == StandardInputOutputBomFormat.autodetect) options.OutputFormat = CliUtils.AutoDetectBomFormat(options.OutputFile);
            if (options.OutputFormat == StandardInputOutputBomFormat.autodetect)
            {
                Console.WriteLine($"Unable to auto-detect output format");
                return (int)ExitCode.ParameterValidationError;
            }

            var inputBoms = InputBoms(options.InputFiles, options.InputFormat, outputToConsole);
            var outputBom = options.Hierarchical ? CycloneDXUtils.HierarchicalMerge(inputBoms) : CycloneDXUtils.FlatMerge(inputBoms);
            outputBom.Version = 1;

            if (!outputToConsole)
            {
                Console.WriteLine("Writing output file...");
                Console.WriteLine($"    Total {outputBom.Components.Count} components");
            }

            return CliUtils.OutputBomHelper(outputBom, options.OutputFormat, options.OutputFile);
        }

        private static IEnumerable<Bom> InputBoms(IEnumerable<string> inputFilenames, StandardInputOutputBomFormat inputFormat, bool outputToConsole)
        {
            foreach (var inputFilename in inputFilenames)
            {
                if (!outputToConsole) Console.WriteLine($"Processing input file {inputFilename}");
                var inputBom = CliUtils.InputBomHelper(inputFilename, inputFormat);
                if (inputBom.Components != null && !outputToConsole)
                    Console.WriteLine($"    Contains {inputBom.Components.Count} components");
                yield return inputBom;
            }
        }
    }
}

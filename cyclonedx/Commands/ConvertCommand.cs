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
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace CycloneDX.Cli
{
    public static class ConvertCommand
    {
        // WARNING: keep this in sync with StandardInputOutputBomFormat, csv should be last
        public enum InputFormat
        {
            autodetect,
            xml,
            json,
            protobuf,
            csv
        }

        // WARNING: keep this in sync with StandardInputOutputBomFormat, csv, spdx and specific versions should be last
        public enum OutputFormat
        {
            autodetect,
            xml,
            json,
            protobuf,
            csv,
            spdxtag,
            xml_v1_0,
            xml_v1_1,
            xml_v1_2,
            xml_v1_3,
            json_v1_2,
            json_v1_3,
            protobuf_v1_3,
            spdxtag_v2_1,
            spdxtag_v2_2
        }

        public class Options
        {
            public string InputFile { get; set; }
            public string OutputFile { get; set; }
            public InputFormat InputFormat { get; set; }
            public OutputFormat OutputFormat { get; set; }
        }
        
        internal static void Configure(RootCommand rootCommand)
        {
            var subCommand = new Command("convert", "Convert between different BOM formats");
            subCommand.Add(new Option<string>("--input-file", "Input BOM filename, will read from stdin if no value provided."));
            subCommand.Add(new Option<string>("--output-file", "Output BOM filename, will write to stdout if no value provided."));
            subCommand.Add(new Option<InputFormat>("--input-format", "Specify input file format."));
            subCommand.Add(new Option<OutputFormat>("--output-format", "Specify output file format."));
            subCommand.Handler = CommandHandler.Create<Options>(Convert);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Convert(Options options)
        {
            var inputBom = CliUtils.InputBomHelper(options.InputFile, options.InputFormat);
            if (inputBom == null) return (int)ExitCode.ParameterValidationError;

            if (options.OutputFormat == OutputFormat.autodetect)
            {
                if (string.IsNullOrEmpty(options.OutputFile))
                {
                    Console.Error.WriteLine("You must specify a value for --output-format when standard output is used");
                    return (int)ExitCode.ParameterValidationError;
                }

                options.OutputFormat = CliUtils.AutoDetectConvertCommandOutputBomFormat(options.OutputFile);
                
                if (options.OutputFormat == OutputFormat.autodetect)
                {
                    Console.Error.WriteLine("Unable to auto-detect output format from output filename");
                    return (int)ExitCode.ParameterValidationError;
                }
            }

            return CliUtils.OutputBomHelper(inputBom, options.OutputFormat, options.OutputFile);
        }
    }
}

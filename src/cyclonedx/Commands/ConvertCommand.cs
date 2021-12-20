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
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using CycloneDX.Cli;

namespace CycloneDX.Cli.Commands
{
    public static class ConvertCommand
    {
        internal static void Configure(RootCommand rootCommand)
        {
            Contract.Requires(rootCommand != null);
            var subCommand = new Command("convert", "Convert between different BOM formats");
            subCommand.Add(new Option<string>("--input-file", "Input BOM filename, will read from stdin if no value provided."));
            subCommand.Add(new Option<string>("--output-file", "Output BOM filename, will write to stdout if no value provided."));
            subCommand.Add(new Option<ConvertInputFormat>("--input-format", "Specify input file format."));
            subCommand.Add(new Option<ConvertOutputFormat>("--output-format", "Specify output file format."));
            subCommand.Handler = CommandHandler.Create<ConvertCommandOptions>(Convert);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Convert(ConvertCommandOptions options)
        {
            Contract.Requires(options != null);
            var inputBom = await CliUtils.InputBomHelper(options.InputFile, options.InputFormat).ConfigureAwait(false);
            if (inputBom == null) return (int)ExitCode.ParameterValidationError;

            if (options.OutputFormat == ConvertOutputFormat.autodetect)
            {
                if (string.IsNullOrEmpty(options.OutputFile))
                {
                    await Console.Error.WriteLineAsync("You must specify a value for --output-format when standard output is used");
                    return (int)ExitCode.ParameterValidationError;
                }

                options.OutputFormat = CliUtils.AutoDetectConvertCommandOutputBomFormat(options.OutputFile);
                
                if (options.OutputFormat == ConvertOutputFormat.autodetect)
                {
                    await Console.Error.WriteLineAsync("Unable to auto-detect output format from output filename");
                    return (int)ExitCode.ParameterValidationError;
                }
            }

            return await CliUtils.OutputBomHelper(inputBom, options.OutputFormat, options.OutputFile).ConfigureAwait(false);
        }
    }
}

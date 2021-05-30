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
using System.IO;
using System.Threading.Tasks;
using CycloneDX.CLI.Commands;
using CycloneDX.CLI.Models;
using CycloneDX.Models.v1_3;

namespace CycloneDX.CLI
{
    partial class Program
    {
        internal static void ConfigureConvertCommand(RootCommand rootCommand)
        {
            var subCommand = new Command("convert", "Convert between different BOM formats");
            subCommand.Add(new Option<string>("--input-file", "Input BOM filename, will read from stdin if no value provided."));
            subCommand.Add(new Option<string>("--output-file", "Output BOM filename, will write to stdout if no value provided."));
            subCommand.Add(new Option<InputFormat>("--input-format", "Specify input file format."));
            subCommand.Add(new Option<ConvertOutputFormat>("--output-format", "Specify output file format."));
            subCommand.Handler = CommandHandler.Create<string, string, InputFormat, ConvertOutputFormat>(Convert);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Convert(string inputFile, string outputFile, InputFormat inputFormat, ConvertOutputFormat outputFormat)
        {
            var inputBomFormat = InputFormatHelper(inputFile, inputFormat);
            if (inputBomFormat == BomFormat.Unsupported) return (int)ExitCode.ParameterValidationError;

            BomFormat outputBomFormat = BomFormat.Unsupported;
            string inputBomString;
            Bom inputBom;
            string outputBomString;

            if (outputFormat == ConvertOutputFormat.autodetect)
            {
                if (string.IsNullOrEmpty(outputFile))
                {
                    Console.Error.WriteLine("You must specify a value for --output-format when standard output is used");
                    return (int)ExitCode.ParameterValidationError;
                }
                outputBomFormat = CLIUtils.DetectFileFormat(outputFile);
                if (outputBomFormat == BomFormat.Unsupported)
                {
                    Console.Error.WriteLine("Unable to auto-detect output format from output filename");
                    return (int)ExitCode.ParameterValidationError;
                }
            }
            else
            {
                outputBomFormat = (BomFormat)outputFormat;
            }

            inputBomString = await InputFileHelper(inputFile);
            if (inputBomString == null) return (int)ExitCode.ParameterValidationError;
            
            inputBom = CLIUtils.BomDeserializer(inputBomString, inputBomFormat);
            outputBomString = CLIUtils.BomSerializer(inputBom, outputBomFormat);

            if (string.IsNullOrEmpty(outputFile))
            {
                Console.Write(outputBomString);
            }
            else
            {
                Console.WriteLine("Writing output file...");
                await File.WriteAllTextAsync(outputFile, outputBomString);
            }

            return (int)ExitCode.Ok;
        }
    }
}

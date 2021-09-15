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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CycloneDX.Models;

namespace CycloneDX.Cli.Commands
{
    internal static class ValidateCommand
    {
        public enum InputFormat
        {
            autodetect,
            json,
            json_v1_3,
            json_v1_2,
            xml,
            xml_v1_3,
            xml_v1_2,
            xml_v1_1,
            xml_v1_0,
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812: Avoid uninstantiated internal classes")]
        public class Options
        {
            public string InputFile { get; set; }
            public InputFormat InputFormat { get; set; }
            public bool FailOnErrors { get; set; }
        }

        public static void Configure(RootCommand rootCommand)
        {
            var subCommand = new Command("validate", "Validate a BOM");
            subCommand.Add(new Option<string>("--input-file", "Input BOM filename, will read from stdin if no value provided."));
            subCommand.Add(new Option<InputFormat>("--input-format", "Specify input file format."));
            subCommand.Add(new Option<bool>("--fail-on-errors", "Fail on validation errors (return a non-zero exit code)"));
            subCommand.Handler = CommandHandler.Create<Options>(Validate);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Validate(Options options)
        {
            ValidateInputFormatValue(options);
            if (options.InputFormat == InputFormat.autodetect)
            {
                await Console.Error.WriteLineAsync("Unable to auto-detect input format").ConfigureAwait(false);
                return (int)ExitCode.ParameterValidationError;
            }

            var inputBom = ReadInput(options);
            if (inputBom == null)
            {
                await Console.Error.WriteLineAsync("Error reading input, you must specify a value for --input-file or pipe in content").ConfigureAwait(false);
                return (int)ExitCode.IOError;
            }

            var schemaVersion = SchemaVersion.v1_3;

            switch (options.InputFormat)
            {
                case InputFormat.xml_v1_2:
                case InputFormat.json_v1_2:
                    schemaVersion = SchemaVersion.v1_2;
                    break;
                case InputFormat.xml_v1_1:
                    schemaVersion = SchemaVersion.v1_1;
                    break;
                case InputFormat.xml_v1_0:
                    schemaVersion = SchemaVersion.v1_0;
                    break;
            }

            ValidationResult validationResult;

            if (options.InputFormat.ToString().StartsWith("json", StringComparison.InvariantCulture))
            {
                Console.WriteLine("Validating JSON BOM...");
                validationResult = Json.Validator.Validate(inputBom, schemaVersion);
            }
            else
            {
                Console.WriteLine("Validating XML BOM...");
                validationResult = await Xml.Validator.Validate(inputBom, schemaVersion).ConfigureAwait(false);
            }

            if (validationResult.Messages != null)
            foreach (var message in validationResult.Messages)
            {
                Console.WriteLine(message);
            }

            if (options.FailOnErrors && !validationResult.Valid)
            {
                return (int)ExitCode.OkFail;
            }
            
            Console.WriteLine("BOM validated successfully.");

            return (int)ExitCode.Ok;
        }

        private static void ValidateInputFormatValue(Options options)
        {
            if (options.InputFormat == InputFormat.autodetect && !string.IsNullOrEmpty(options.InputFile))
            {
                if (options.InputFile.EndsWith(".json", StringComparison.InvariantCulture))
                {
                    options.InputFormat = InputFormat.json;
                }
                else if (options.InputFile.EndsWith(".xml", StringComparison.InvariantCulture))
                {
                    options.InputFormat = InputFormat.xml;
                }
            }
            
            if (options.InputFormat == InputFormat.json)
            {
                options.InputFormat = InputFormat.json_v1_3;
            }
            else if (options.InputFormat == InputFormat.xml)
            {
                options.InputFormat = InputFormat.xml_v1_3;
            }
        }

        private static string ReadInput(Options options)
        {
            string inputString = null;
            if (!string.IsNullOrEmpty(options.InputFile))
            {
                inputString = File.ReadAllText(options.InputFile);
            }
            else if (Console.IsInputRedirected)
            {
                var sb = new StringBuilder();
                string nextLine;
                do
                {
                    nextLine = Console.ReadLine();
                    sb.AppendLine(nextLine);
                } while (nextLine != null);
                inputString = sb.ToString();
            }
            return inputString;
        }
    }
}

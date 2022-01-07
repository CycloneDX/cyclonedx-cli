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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CycloneDX.Models;

namespace CycloneDX.Cli.Commands
{
    public static class ValidateCommand
    {

        public static void Configure(RootCommand rootCommand)
        {
            Contract.Requires(rootCommand != null);
            var subCommand = new Command("validate", "Validate a BOM");
            subCommand.Add(new Option<string>("--input-file", "Input BOM filename, will read from stdin if no value provided."));
            subCommand.Add(new Option<ValidationBomFormat>("--input-format", "Specify input file format."));
            subCommand.Add(new Option<SpecificationVersion?>("--input-version", "Specify input file specification version (defaults to v1.4)"));
            subCommand.Add(new Option<bool>("--fail-on-errors", "Fail on validation errors (return a non-zero exit code)"));
            subCommand.Handler = CommandHandler.Create<ValidateCommandOptions>(Validate);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Validate(ValidateCommandOptions options)
        {
            Contract.Requires(options != null);
            ValidateInputFormatValue(options);
            if (options.InputFormat == ValidationBomFormat.autodetect)
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

            if (!options.InputVersion.HasValue)
            {
                options.InputVersion = SpecificationVersion.v1_4;
            }

            ValidationResult validationResult = null;

            switch (options.InputFormat)
            {
                case ValidationBomFormat.xml:
                    Console.WriteLine("Validating XML BOM...");
                    validationResult = Xml.Validator.Validate(inputBom, options.InputVersion.Value);
                    break;
                case ValidationBomFormat.json:
                    Console.WriteLine("Validating JSON BOM...");
                    validationResult = Json.Validator.Validate(inputBom, options.InputVersion.Value);
                    break;
            }

            if (validationResult == null)
            {
                Console.WriteLine("There was an issue with the supplied parameters. Unable to check validity of BOM.");
                return (int)ExitCode.ParameterValidationError;
            }
            else
            {
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
        }

        private static void ValidateInputFormatValue(ValidateCommandOptions options)
        {
            if (options.InputFormat == ValidationBomFormat.autodetect && !string.IsNullOrEmpty(options.InputFile))
            {
                if (options.InputFile.EndsWith(".json", StringComparison.InvariantCulture))
                {
                    options.InputFormat = ValidationBomFormat.json;
                }
                else if (options.InputFile.EndsWith(".xml", StringComparison.InvariantCulture))
                {
                    options.InputFormat = ValidationBomFormat.xml;
                }
            }
        }

        private static string ReadInput(ValidateCommandOptions options)
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

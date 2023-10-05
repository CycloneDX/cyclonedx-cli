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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using CycloneDX.Models;
using CycloneDX.Utils;
using System.IO;
using System.Collections.Immutable;

namespace CycloneDX.Cli.Commands
{
    public static class RenameEntityCommand
    {
        public static void Configure(RootCommand rootCommand)
        {
            Contract.Requires(rootCommand != null);
            var subCommand = new System.CommandLine.Command("rename-entity", "Rename an entity identified by a \"bom-ref\" (including back-references to it) in the BOM document");
            subCommand.Add(new Option<string>("--input-file", "Input BOM filename."));
            subCommand.Add(new Option<string>("--output-file", "Output BOM filename, will write to stdout if no value provided."));
            subCommand.Add(new Option<string>("--old-ref", "Old value of \"bom-ref\" entity identifier (or \"ref\" values or certain list items pointing to it)."));
            subCommand.Add(new Option<string>("--new-ref", "New value of \"bom-ref\" entity identifier (or \"ref\" values or certain list items pointing to it)."));
            subCommand.Add(new Option<CycloneDXBomFormat>("--input-format", "Specify input file format."));
            subCommand.Add(new Option<CycloneDXBomFormat>("--output-format", "Specify output file format."));
            subCommand.Handler = CommandHandler.Create<RenameEntityCommandOptions>(RenameEntity);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> RenameEntity(RenameEntityCommandOptions options)
        {
            Contract.Requires(options != null);
            var outputToConsole = string.IsNullOrEmpty(options.OutputFile);

            if (options.OutputFormat == CycloneDXBomFormat.autodetect)
            {
                options.OutputFormat = CliUtils.AutoDetectBomFormat(options.OutputFile);
            }
            if (options.OutputFormat == CycloneDXBomFormat.autodetect)
            {
                Console.WriteLine($"Unable to auto-detect output format");
                return (int)ExitCode.ParameterValidationError;
            }

            Console.WriteLine($"Loading input document...");
            if (!outputToConsole) Console.WriteLine($"Processing input file {options.InputFile}");
            var bom = await CliUtils.InputBomHelper(options.InputFile, options.InputFormat).ConfigureAwait(false);

            if (bom is null)
            {
                Console.WriteLine($"Empty or absent input document");
                return (int)ExitCode.ParameterValidationError;
            }

            Console.WriteLine($"Beginning Bom walk to discover all identifiers (this can take a while)");
            BomWalkResult bwr = bom.WalkThis();

            Console.WriteLine($"Beginning Bom walk rename processing (this can take a while)");
            if (bom.RenameBomRef(options.OldRef, options.NewRef, bwr))
            {
                Console.WriteLine($"Did not encounter any issues during the rename operation");
            }
            else
            {
                Console.WriteLine($"Rename operation failed non-fatally (e.g. old ref name not mentioned in the Bom document)");
            }

            // Ensure that the modified document has its own identity
            // (new SerialNumber, Version=1, Timestamp...) and its Tools
            // collection refers to this library and the program/tool
            // like cyclonedx-cli which consumes it:
            bom.BomMetadataUpdate(true);
            bom.BomMetadataReferThisToolkit();

            if (!outputToConsole)
            {
                Console.WriteLine("Writing output file...");
                Console.WriteLine($"    Total {bom.Components?.Count ?? 0} components, {bom.Dependencies?.Count ?? 0} dependencies");
            }

            int res = await CliUtils.OutputBomHelper(bom, options.OutputFormat, options.OutputFile).ConfigureAwait(false);
            return res;
        }
    }
}

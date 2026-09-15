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
using System.IO;
using System.Threading.Tasks;
using CycloneDX.Models;
using CycloneDX.Utils;
using System.CommandLine.NamingConventionBinder;

namespace CycloneDX.Cli.Commands
{
    internal static class MergeCommand
    {
        public static void Configure(RootCommand rootCommand)
        {
            Contract.Requires(rootCommand != null);
            var subCommand = new System.CommandLine.Command("merge", "Merge two or more BOMs")
            {
                new Option<List<string>>("--input-files", "Input BOM filenames (separate filenames with a space).") { AllowMultipleArgumentsPerToken = true },
                new Option<List<string>>("--input-files-list", "One or more text file(s) with input BOM filenames (one per line). Combined with --input-files, useful to exceed OS/shell command-line length limits when merging many BOMs.") { AllowMultipleArgumentsPerToken = true },
                new Option<List<string>>("--input-files-nul-list", "One or more text-like file(s) with input BOM filenames (separated by 0x00 characters, e.g. from `find -print0`).") { AllowMultipleArgumentsPerToken = true },
                new Option<string>("--output-file", "Output BOM filename, will write to stdout if no value provided."),
                new Option<CycloneDXBomFormat>("--input-format", "Specify input file format."),
                new Option<CycloneDXBomFormat>("--output-format", "Specify output file format."),
                new Option<SpecificationVersion>("--output-version", "Specify output BOM specification version."),
                new Option<bool>("--hierarchical", "Perform a hierarchical merge."),
                new Option<string>("--group", "Provide the group of software the merged BOM describes."),
                new Option<string>("--name", "Provide the name of software the merged BOM describes (required for hierarchical merging)."),
                new Option<string>("--version", "Provide the version of software the merged BOM describes (required for hierarchical merging)."),
                new Option<bool>("--validate-output", "Validate the merged document before writing it, and do not write it if validation fails."),
                new Option<bool>("--validate-output-relaxed", "Validate the merged document, but still write it (for troubleshooting) even if validation fails."),
                new Option<bool>("--strip-empty-lists", "Omit empty list properties (e.g. \"licenses\": [], \"dependsOn\": []) from the output instead of writing them out. Schema-valid either way; this just avoids redundant clutter."),
#if NET8_0_OR_GREATER
                new Option<ComponentConflictResolution>("--component-conflict-resolution", "How to resolve two equivalent (same type/name/version/group/purl) but not-identical Components, e.g. differing only by Scope. Default: squash, preferring the more permissive Scope."),
                new Option<bool>("--attach-dangling-components", "Attach any components no dependsOn edge reaches (grouped by Scope into synthetic components) so consumers that walk the dependency graph from the subject, rather than scanning the flat components list, don't silently miss them."),
                new Option<string>("--attach-dangling-components-ref", "Existing bom-ref to attach dangling components under (with --attach-dangling-components). Defaults to the merge subject if not given or not found."),
#endif
            };
            subCommand.Handler = CommandHandler.Create<MergeCommandOptions>(Merge);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Merge(MergeCommandOptions options)
        {
            Contract.Requires(options != null);
            var outputToConsole = string.IsNullOrEmpty(options.OutputFile);

            if (options.Hierarchical && (options.Name is null || options.Version is null))
            {
                Console.WriteLine($"Name and version must be specified when performing a hierarchical merge.");
                return (int)ExitCode.ParameterValidationError;
            }

            if (options.OutputFormat == CycloneDXBomFormat.autodetect) options.OutputFormat = CliUtils.AutoDetectBomFormat(options.OutputFile);
            if (options.OutputFormat == CycloneDXBomFormat.autodetect)
            {
                Console.WriteLine($"Unable to auto-detect output format");
                return (int)ExitCode.ParameterValidationError;
            }

            var inputBoms = await InputBoms(DetermineInputFiles(options), options.InputFormat, outputToConsole).ConfigureAwait(false);

            Component bomSubject = null;
            if (options.Group != null || options.Name != null || options.Version != null)
                bomSubject = new Component
                {
                    Type = Component.Classification.Application,
                    Group = options.Group,
                    Name = options.Name,
                    Version = options.Version,
                };

#if NET8_0_OR_GREATER
            var mergeStrategy = MergeStrategy.Default();
            if (options.ComponentConflictResolution.HasValue)
            {
                mergeStrategy.ComponentConflictResolution = options.ComponentConflictResolution.Value;
            }
#endif

            Bom outputBom;
            if (options.Hierarchical)
            {
#if NET8_0_OR_GREATER
                outputBom = CycloneDXUtils.HierarchicalMerge(inputBoms, bomSubject, mergeStrategy);
#else
                outputBom = CycloneDXUtils.HierarchicalMerge(inputBoms, bomSubject);
#endif
            }
            else
            {
#if NET8_0_OR_GREATER
                outputBom = CycloneDXUtils.FlatMerge(inputBoms, bomSubject, mergeStrategy);
#else
                outputBom = CycloneDXUtils.FlatMerge(inputBoms, bomSubject);
#endif
                if (outputBom.Metadata is null) outputBom.Metadata = new Metadata();
                if (bomSubject is null)
                {
                    // otherwise use the first non-null component from the input
                    // BOMs as the default; note CleanupMetadataComponent below,
                    // since that same component may also already be present
                    // in outputBom.Components.
                    foreach (var bom in inputBoms)
                    {
                        if(bom.Metadata != null && bom.Metadata.Component != null)
                        {
                            outputBom.Metadata.Component = bom.Metadata.Component;
                            break;
                        }
                    }
                }
            }

#if NET8_0_OR_GREATER
            outputBom = CycloneDXUtils.CleanupMetadataComponent(outputBom, mergeStrategy);
            outputBom = CycloneDXUtils.CleanupEmptyLists(outputBom);

            if (options.AttachDanglingComponents)
            {
                var attached = outputBom.AttachDanglingComponents(options.AttachDanglingComponentsRef);
                if (attached.Count > 0)
                {
                    var totalAttached = 0;
                    foreach (var bucket in attached.Values) totalAttached += bucket.Count;
                    Console.WriteLine($"Attached {totalAttached} component(s) unreachable from the dependency graph, in {attached.Count} scope bucket(s):");
                    foreach (var bucket in attached)
                    {
                        Console.WriteLine($"    {bucket.Key}: {bucket.Value.Count} component(s)");
                    }
                }
            }
#endif

            // FlatMerge/HierarchicalMerge never set SpecVersion on their
            // result, so it defaults to v1_0 unless assigned here. Apply the
            // requested --output-version (or the library's current version,
            // matching OutputBomHelper's own default) before the
            // --validate-output check below, so validation reflects the
            // spec version that will actually be written -- rather than
            // OutputBomHelper silently overriding it afterwards, by which
            // point validation has already run against the wrong target.
            outputBom.SpecVersion = options.OutputVersion ?? SpecificationVersionHelpers.CurrentVersion;

            // Ensure that the merged document has its own identity (new
            // SerialNumber, Version=1, Timestamp...) and that its Tools
            // collection records the library and program that produced it.
#if NET8_0_OR_GREATER
            outputBom.BomMetadataUpdate(true);
            outputBom.BomMetadataReferThisToolkit();
#else
            outputBom.Version = 1;
            outputBom.SerialNumber = "urn:uuid:" + System.Guid.NewGuid().ToString();
            if (outputBom.Metadata == null)
            {
                outputBom.Metadata = new Metadata();
            }
            if (outputBom.Metadata.Timestamp == null)
            {
                outputBom.Metadata.Timestamp = DateTime.Now;
            }
#endif

            if (options.StripEmptyLists)
            {
                // Applied before validation so the validated document
                // matches what OutputBomHelper actually writes below.
                CycloneDXUtils.CleanupEmptyListsDeep(outputBom);
            }

            ValidationResult validationResult = null;
            if (options.ValidateOutput || options.ValidateOutputRelaxed)
            {
                Console.WriteLine("Validating merged BOM...");
                validationResult = Json.Validator.Validate(Json.Serializer.Serialize(outputBom), outputBom.SpecVersion);

                if (validationResult.Messages != null)
                {
                    foreach (var message in validationResult.Messages)
                    {
                        Console.WriteLine(message);
                    }
                }

                if (validationResult.Valid)
                {
                    Console.WriteLine("Merged BOM validated successfully.");
                }
                else
                {
                    Console.WriteLine("Merged BOM is not valid.");
                    if (!options.ValidateOutputRelaxed)
                    {
                        Console.WriteLine("NOT writing output file...");
                        return (int)ExitCode.SignatureFailedVerification;
                    }
                }
            }

            if (!outputToConsole)
            {
                Console.WriteLine("Writing output file...");
                Console.WriteLine($"    Total {outputBom.Components?.Count ?? 0} components");
            }

            var res = await CliUtils.OutputBomHelper(outputBom, (ConvertFormat)options.OutputFormat, options.OutputVersion, options.OutputFile, options.StripEmptyLists).ConfigureAwait(false);
            if (validationResult != null && !validationResult.Valid)
            {
                // Relaxed mode: the file was still written above, but the
                // command as a whole should still report failure.
                return (int)ExitCode.SignatureFailedVerification;
            }
            return res;
        }

        /// <summary>
        /// Combines --input-files with any filenames listed inside
        /// --input-files-list (one per line) and --input-files-nul-list
        /// (0x00-separated) files, deduplicating as it goes. Lets callers
        /// exceed OS/shell command-line length or argument-count limits
        /// when merging many BOMs, by passing a generated list file
        /// instead of one --input-files argument per BOM.
        /// </summary>
        private static List<string> DetermineInputFiles(MergeCommandOptions options)
        {
            var inputFiles = options.InputFiles != null ? new List<string>(options.InputFiles) : new List<string>();

            if (options.InputFilesList != null)
            {
                foreach (var oneList in options.InputFilesList)
                {
                    Console.WriteLine($"Adding to input file list from {oneList}");
                    var count = 0;
                    foreach (var line in File.ReadAllLines(oneList))
                    {
                        if (string.IsNullOrEmpty(line) || inputFiles.Contains(line))
                        {
                            continue;
                        }
                        inputFiles.Add(line);
                        count++;
                    }
                    Console.WriteLine($"Got {count} new entries from {oneList}");
                }
            }

            if (options.InputFilesNulList != null)
            {
                foreach (var oneList in options.InputFilesNulList)
                {
                    Console.WriteLine($"Adding to input file list from {oneList}");
                    var count = 0;
                    foreach (var line in File.ReadAllText(oneList).Split('\0'))
                    {
                        if (string.IsNullOrEmpty(line) || inputFiles.Contains(line))
                        {
                            continue;
                        }
                        inputFiles.Add(line);
                        count++;
                    }
                    Console.WriteLine($"Got {count} new entries from {oneList}");
                }
            }

            Console.WriteLine($"Determined {inputFiles.Count} input file(s) to merge");
            return inputFiles;
        }

        private static async Task<IEnumerable<Bom>> InputBoms(IEnumerable<string> inputFilenames, CycloneDXBomFormat inputFormat, bool outputToConsole)
        {
            var boms = new List<Bom>();
            foreach (var inputFilename in inputFilenames)
            {
                if (!outputToConsole) Console.WriteLine($"Processing input file {inputFilename}");
                var inputBom = await CliUtils.InputBomHelper(inputFilename, inputFormat).ConfigureAwait(false);
                if (inputBom.Components != null && !outputToConsole)
                    Console.WriteLine($"    Contains {inputBom.Components.Count} components");
                //TODO: figure out how to implement async iterators, if possible at all
                boms.Add(inputBom);
            }
            return boms;
        }
    }
}

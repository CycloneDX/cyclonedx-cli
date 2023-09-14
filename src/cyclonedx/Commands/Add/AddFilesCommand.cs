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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AntPathMatching;
using CycloneDX.Models;
using CycloneDX.Cli.Commands;

namespace CycloneDX.Cli.Commands.Add
{
    public static class AddFilesCommand
    {
        public static void Configure(System.CommandLine.Command rootCommand)
        {
            Contract.Requires(rootCommand != null);
            var subCommand = new System.CommandLine.Command("files", "Add files to a BOM");
            subCommand.Add(new Option<string>("--input-file", "Input BOM filename."));
            subCommand.Add(new Option<bool>("--no-input", "Use this option to indicate that there is no input BOM."));
            subCommand.Add(new Option<string>("--output-file", "Output BOM filename, will write to stdout if no value provided."));
            subCommand.Add(new Option<CycloneDXBomFormat>("--input-format", "Specify input file format."));
            subCommand.Add(new Option<CycloneDXBomFormat>("--output-format", "Specify output file format."));
            subCommand.Add(new Option<string>("--base-path", "Base path for directory to process (defaults to current working directory if omitted)."));
            subCommand.Add(new Option<List<string>>("--include", "Apache Ant style path and file patterns to specify what to include (defaults to all files, separate patterns with a space)."));
            subCommand.Add(new Option<List<string>>("--exclude", "Apache Ant style path and file patterns to specify what to exclude (defaults to none, separate patterns with a space)."));
            subCommand.Handler = CommandHandler.Create<AddFilesCommandOptions>(AddFiles);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> AddFiles(AddFilesCommandOptions options)
        {
            Contract.Requires(options != null);
            var outputToConsole = string.IsNullOrEmpty(options.OutputFile);

            var thisTool = new Component
            {
                Name = "CycloneDX CLI",
                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
            };

            var bom = options.NoInput ? new Bom() : await CliUtils.InputBomHelper(options.InputFile, options.InputFormat).ConfigureAwait(false);
            if (bom == null) return (int)ExitCode.ParameterValidationError;

            if (bom.SerialNumber is null) bom.SerialNumber = "urn:uuid:" + System.Guid.NewGuid().ToString();
            if (bom.Metadata is null) bom.Metadata = new Metadata();
            bom.Metadata.Timestamp = DateTime.UtcNow;
            if (bom.Metadata.Tools is null) bom.Metadata.Tools = new ToolChoices();
            if (bom.Metadata.Tools.Components is null) bom.Metadata.Tools.Components = new List<Component>();
            if (!bom.Metadata.Tools.Components.Exists(tool => tool.Name == thisTool.Name && tool.Version == thisTool.Version))
                bom.Metadata.Tools.Components.Add(thisTool);

            if (options.OutputFormat == CycloneDXBomFormat.autodetect) options.OutputFormat = CliUtils.AutoDetectBomFormat(options.OutputFile);
            if (options.OutputFormat == CycloneDXBomFormat.autodetect)
            {
                Console.WriteLine($"Unable to auto-detect output format");
                return (int)ExitCode.ParameterValidationError;
            }

            if (string.IsNullOrEmpty(options.BasePath))
            {
                options.BasePath = Directory.GetCurrentDirectory();
            }

            options.BasePath = Path.GetFullPath(options.BasePath);
            if (!Path.EndsInDirectorySeparator(options.BasePath)) options.BasePath += Path.DirectorySeparatorChar;

            if (!outputToConsole) Console.WriteLine($"Processing base path {options.BasePath}");

            if (options.Include == null) options.Include = new List<string> { "**/**" };
            if (options.Exclude == null) options.Exclude = new List<string>();

            var files = new HashSet<string>();

            foreach (var includePattern in options.Include)
            {
                if (!outputToConsole) Console.WriteLine($"Processing include pattern {includePattern}");
                var ant = new Ant(includePattern);
                var antDir = new AntDirectory(ant);
                var matchingFiles = antDir.SearchRecursively(options.BasePath);
                foreach (var file in matchingFiles)
                {
                    files.Add(file);
                }
            }

            foreach (var excludePattern in options.Exclude)
            {
                if (!outputToConsole) Console.WriteLine($"Processing exclude pattern {excludePattern}");
                var ant = new Ant(excludePattern);
                files.RemoveWhere(s => ant.IsMatch(s));
            }

            if (files.Count > 0)
            {
                if (bom.Components == null) bom.Components = new List<Component>();

                var existingFiles = bom.Components.Where<Component>(component => component.Type == Component.Classification.File);

                foreach (var file in files)
                {
                    var baseFilename = Path.GetFileName(file);
                    if (!existingFiles.Any<Component>(component => component.Name == baseFilename))
                    {
                        if (!outputToConsole) Console.WriteLine($"Adding file {baseFilename}");
                        var fullPath = Path.Combine(options.BasePath, file);
                        var fileComponent = new Component
                        {
                            Type = Component.Classification.File,
                            Name = baseFilename,
                            Hashes = GetFileHashes(fullPath),
                        };
                        var shortHash = fileComponent.Hashes.First(h => h.Alg == Hash.HashAlgorithm.SHA_1).Content.Substring(0, 12);
                        fileComponent.Version = $"0.0.0-{shortHash}";
                        bom.Components.Add(fileComponent);
                    }
                    else
                    {
                        if (!outputToConsole) Console.WriteLine($"Skipping file {baseFilename} as it is already in the BOM");
                    }
                }
            }

            if (!outputToConsole) Console.WriteLine("Writing output file...");

            return await CliUtils.OutputBomHelper(bom, options.OutputFormat, options.OutputFile).ConfigureAwait(false);
        }

        private static List<Hash> GetFileHashes(string filename)
        {
            var hashes = new List<Hash>();
            hashes.Add(GetSha1Hash(filename));
            hashes.Add(GetSha256Hash(filename));
            hashes.Add(GetSha384Hash(filename));
            hashes.Add(GetSha512Hash(filename));
            return hashes;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA5350")]
        private static Hash GetSha1Hash(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open))
            using (var bs = new BufferedStream(fs))
            {
                using (var sha1 = SHA1.Create())
                {
                    byte[] hash = sha1.ComputeHash(bs);
                    StringBuilder formatted = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }
                    return new Hash
                    {
                        Alg = Hash.HashAlgorithm.SHA_1,
                        Content = formatted.ToString().ToLowerInvariant()
                    };
                }
            }
        }

        private static Hash GetSha256Hash(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open))
            using (var bs = new BufferedStream(fs))
            {
                using (var sha1 = SHA256.Create())
                {
                    byte[] hash = sha1.ComputeHash(bs);
                    StringBuilder formatted = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }
                    return new Hash
                    {
                        Alg = Hash.HashAlgorithm.SHA_256,
                        Content = formatted.ToString().ToLowerInvariant()
                    };
                }
            }
        }

        private static Hash GetSha384Hash(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open))
            using (var bs = new BufferedStream(fs))
            {
                using (var sha1 = SHA384.Create())
                {
                    byte[] hash = sha1.ComputeHash(bs);
                    StringBuilder formatted = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }
                    return new Hash
                    {
                        Alg = Hash.HashAlgorithm.SHA_384,
                        Content = formatted.ToString().ToLowerInvariant()
                    };
                }
            }
        }

        private static Hash GetSha512Hash(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open))
            using (var bs = new BufferedStream(fs))
            {
                using (var sha512 = SHA512.Create())
                {
                    byte[] hash = sha512.ComputeHash(bs);
                    StringBuilder formatted = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }
                    return new Hash
                    {
                        Alg = Hash.HashAlgorithm.SHA_512,
                        Content = formatted.ToString().ToLowerInvariant()
                    };
                }
            }
        }
    }
}

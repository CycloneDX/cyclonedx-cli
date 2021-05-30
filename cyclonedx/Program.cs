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
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CycloneDX.CLI.Models;

namespace CycloneDX.CLI
{
    public partial class Program
    {
        private const string CycloneDX = @"
   ______           __                 ____ _  __    ________    ____
  / ____/_  _______/ /___  ____  ___  / __ \ |/ /   / ____/ /   /  _/
 / /   / / / / ___/ / __ \/ __ \/ _ \/ / / /   /   / /   / /    / /  
/ /___/ /_/ / /__/ / /_/ / / / /  __/ /_/ /   |   / /___/ /____/ /   
\____/\__, /\___/_/\____/_/ /_/\___/_____/_/|_|   \____/_____/___/   
     /____/                                                          
        ";

        public static async Task<int> Main(string[] args)
        {
            Contract.Requires(args != null);
            
            if (args.Length == 0)
            {
                Console.WriteLine(CycloneDX);
            }

            RootCommand rootCommand = new RootCommand();
            
            ConfigureAnalyzeCommand(rootCommand);
            ConfigureConvertCommand(rootCommand);
            ConfigureDiffCommand(rootCommand);
            ConfigureMergeCommand(rootCommand);
            ValidateCommand.Configure(rootCommand);

            return await rootCommand.InvokeAsync(args);
        }

        public static BomFormat InputFormatHelper(string inputFile, InputFormat inputFormat)
        {
            if (inputFormat == InputFormat.autodetect)
            {
                if (string.IsNullOrEmpty(inputFile))
                {
                    Console.Error.WriteLine("Unable to auto-detect input stream format, please specify a value for --input-format");
                }
                var inputBomFormat = CLIUtils.DetectFileFormat(inputFile);
                if (inputBomFormat == BomFormat.Unsupported)
                {
                    Console.Error.WriteLine("Unable to auto-detect input format from input filename");
                }
                return inputBomFormat;
            }
            else
            {
                if (inputFormat == InputFormat.json)
                {
                    return BomFormat.Json;
                }
                else if (inputFormat == InputFormat.xml)
                {
                    return BomFormat.Xml;
                }
                else if (inputFormat == InputFormat.csv)
                {
                    return BomFormat.Csv;
                }
            }

            return BomFormat.Unsupported;
        }

        public static async Task<string> InputFileHelper(string inputFile)
        {
            string inputString = null;
            if (!string.IsNullOrEmpty(inputFile))
            {
                inputString = await File.ReadAllTextAsync(inputFile);
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
            else
            {
                Console.Error.WriteLine("You must specify a value for --input-file or pipe in content");
            }
            return inputString;
        }
    }
}

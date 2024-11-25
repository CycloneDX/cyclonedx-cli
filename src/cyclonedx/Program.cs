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
using System.Threading.Tasks;
using CycloneDX.Cli.Commands;

namespace CycloneDX.Cli
{
    public static class Program
    {
        private const string CycloneDx = @"
   ______           __                 ____ _  __    ________    ____
  / ____/_  _______/ /___  ____  ___  / __ \ |/ /   / ____/ /   /  _/
 / /   / / / / ___/ / __ \/ __ \/ _ \/ / / /   /   / /   / /    / /  
/ /___/ /_/ / /__/ / /_/ / / / /  __/ /_/ /   |   / /___/ /____/ /   
\____/\__, /\___/_/\____/_/ /_/\___/_____/_/|_|   \____/_____/___/   
     /____/                                                          
        ";

        public static async Task<int> Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine(CycloneDx);
            }

            RootCommand rootCommand = new RootCommand();
            
            AddCommand.Configure(rootCommand);
            AnalyzeCommand.Configure(rootCommand);
            ConvertCommand.Configure(rootCommand);
            DiffCommand.Configure(rootCommand);
            KeyGenCommand.Configure(rootCommand);
            MergeCommand.Configure(rootCommand);
            RenameEntityCommand.Configure(rootCommand);
            SignCommand.Configure(rootCommand);
            ValidateCommand.Configure(rootCommand);
            VerifyCommand.Configure(rootCommand);

            return await rootCommand.InvokeAsync(args).ConfigureAwait(false);
        }
    }
}

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
using System.Diagnostics.Contracts;
using System.CommandLine;
using CycloneDX.Cli.Commands.Sign;

namespace CycloneDX.Cli.Commands
{
    public static class SignCommand
    {
        public static void Configure(RootCommand rootCommand)
        {
            Contract.Requires(rootCommand != null);
            var subCommand = new Command("sign", "Sign a BOM or file");
            SignBomCommand.Configure(subCommand);
            SignFileCommand.Configure(subCommand);
            rootCommand.Add(subCommand);
        }
    }
}

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
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Snapshooter;
using Snapshooter.Xunit;
using CycloneDX.Cli.Commands.Sign;

namespace CycloneDX.Cli.Tests
{
    public class SignBomTests
    {
        [Fact]
        public async Task SignXmlBom()
        {
            using (var tempDirectory = new TempDirectory())
            {
                var testFilename = Path.Combine(tempDirectory.DirectoryPath, "bom.xml");
                File.Copy(Path.Combine("Resources", "bom-1.3.xml"), testFilename);

                var exitCode = await SignBomCommand.SignBom(new SignBomCommandOptions
                {
                    BomFile = testFilename,
                    KeyFile = Path.Combine("Resources", "private.key"),
                }).ConfigureAwait(false);
                
                Assert.Equal(ExitCode.Ok, (ExitCode)exitCode);

                var bom = File.ReadAllText(testFilename);
                Snapshot.Match(bom);
            }
        }
    }
}

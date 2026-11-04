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
#if NET8_0_OR_GREATER
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Snapshooter;
using Snapshooter.Xunit;
using CycloneDX.Cli.Commands;

namespace CycloneDX.Cli.Tests
{
    public class RenameEntityTests
    {
        [Theory]
        [InlineData("sbom1.json", CycloneDXBomFormat.autodetect, "sbom.json", CycloneDXBomFormat.autodetect)]
        [InlineData("sbom1.json", CycloneDXBomFormat.json, "sbom.xml", CycloneDXBomFormat.autodetect)]
        public async Task RenameEntity_RewritesIdentifierAndBackReferences(
            string inputFilename,
            CycloneDXBomFormat inputFormat,
            string outputFilename,
            CycloneDXBomFormat outputFormat
        )
        {
            using (var tempDirectory = new TempDirectory())
            {
                var fullOutputPath = Path.Join(tempDirectory.DirectoryPath, outputFilename);
                var options = new RenameEntityCommandOptions
                {
                    InputFile = Path.Combine("Resources", "RenameEntity", inputFilename),
                    InputFormat = inputFormat,
                    OutputFile = fullOutputPath,
                    OutputFormat = outputFormat,
                    OldRef = "lib-old",
                    NewRef = "lib-new",
                };

                var exitCode = await RenameEntityCommand.RenameEntity(options).ConfigureAwait(false);

                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(fullOutputPath);
                bom = Regex.Replace(bom, @"\s*""serialNumber"": "".*?"",\r?\n", ""); // json
                bom = Regex.Replace(bom, @"\s+serialNumber="".*?""", ""); // xml
                bom = Regex.Replace(bom, @"\s*""timestamp"": "".*?"",\r?\n", ""); // json
                bom = Regex.Replace(bom, @"\s+<timestamp>.*?</timestamp>", ""); // xml
                // The tools list embeds this build's assembly names/versions
                // (e.g. "testhost" under `dotnet test` vs. the real CLI
                // executable otherwise), which are environment-specific --
                // strip the whole block before snapshotting.
                bom = Regex.Replace(bom, @"\s*""tools"":\s*\[.*?\],?", "", RegexOptions.Singleline); // json
                bom = Regex.Replace(bom, @"\s*<tools>.*?</tools>", "", RegexOptions.Singleline); // xml

                Assert.DoesNotContain("lib-old", bom);
                Assert.Contains("lib-new", bom);
                Snapshot.Match(bom, SnapshotNameExtension.Create(inputFilename, inputFormat, outputFilename, outputFormat));
            }
        }

        [Fact]
        public async Task RenameEntity_NoOp_WhenOldRefNotPresent()
        {
            using (var tempDirectory = new TempDirectory())
            {
                var fullOutputPath = Path.Join(tempDirectory.DirectoryPath, "sbom.json");
                var options = new RenameEntityCommandOptions
                {
                    InputFile = Path.Combine("Resources", "RenameEntity", "sbom1.json"),
                    InputFormat = CycloneDXBomFormat.autodetect,
                    OutputFile = fullOutputPath,
                    OutputFormat = CycloneDXBomFormat.autodetect,
                    OldRef = "does-not-exist",
                    NewRef = "lib-new",
                };

                var exitCode = await RenameEntityCommand.RenameEntity(options).ConfigureAwait(false);

                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(fullOutputPath);
                Assert.Contains("lib-old", bom);
                Assert.DoesNotContain("lib-new", bom);
            }
        }
    }
}
#endif

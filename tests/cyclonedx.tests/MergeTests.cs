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
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Snapshooter;
using Snapshooter.Xunit;
using CycloneDX.Cli.Commands;

namespace CycloneDX.Cli.Tests
{
    public class MergeTests
    {
        [Theory]
        [InlineData(new[] { "sbom1.json", "sbom2.json"}, CycloneDXBomFormat.autodetect, "sbom.json", CycloneDXBomFormat.autodetect, null, true, null, "Thing", "1")]
        [InlineData(new[] { "sbom1.json", "sbom2.json"}, CycloneDXBomFormat.autodetect, "sbom.json", CycloneDXBomFormat.autodetect, null, false, null, null, null)]
        [InlineData(new[] { "sbom1.json", "sbom2.json"}, CycloneDXBomFormat.autodetect, "sbom.xml", CycloneDXBomFormat.autodetect, null, false, null, null, null)]
        [InlineData(new[] { "sbom1.json", "sbom2.json"}, CycloneDXBomFormat.json, "sbom.json", CycloneDXBomFormat.autodetect, null, false, null, null, null)]
        [InlineData(new[] { "sbom1.json", "sbom2.json" }, CycloneDXBomFormat.json, "sbom.json", CycloneDXBomFormat.autodetect, SpecificationVersion.v1_4, false, null, null, null)]
        [InlineData(new[] { "sbom1.xml", "sbom2.xml"}, CycloneDXBomFormat.autodetect, "sbom.xml", CycloneDXBomFormat.autodetect, null, false, null, null, null)]
        [InlineData(new[] { "sbom1.xml", "sbom2.xml" }, CycloneDXBomFormat.autodetect, "sbom.xml", CycloneDXBomFormat.autodetect, SpecificationVersion.v1_4, false, null, null, null)]
        [InlineData(new[] { "sbom1.xml", "sbom2.xml"}, CycloneDXBomFormat.autodetect, "sbom.json", CycloneDXBomFormat.autodetect, null, false, null, null, null)]
        [InlineData(new[] { "sbom1.xml", "sbom2.xml"}, CycloneDXBomFormat.xml, "sbom.xml", CycloneDXBomFormat.autodetect, null, false, null, null, null)]
        [InlineData(new[] { "sbom1.json", "sbom2.xml"}, CycloneDXBomFormat.autodetect, "sbom.xml", CycloneDXBomFormat.autodetect, null, false, null, null, null)]
        [InlineData(new[] { "sbom1.json", "sbom2.json"}, CycloneDXBomFormat.autodetect, "sbom.json", CycloneDXBomFormat.json, null, false, null, null, null)]
        [InlineData(new[] { "sbom1.json", "sbom2.json"}, CycloneDXBomFormat.autodetect, "sbom.xml", CycloneDXBomFormat.xml, null, false, null, null, null)]
        public async Task Merge(
            string[] inputFilenames,
            CycloneDXBomFormat inputFormat,
            string outputFilename, CycloneDXBomFormat outputFormat,
            SpecificationVersion? outputVersion,
            bool hierarchical,
            string group, string name, string version
        )
        {
            using (var tempDirectory = new TempDirectory())
            {
                var snapshotInputFilenames = string.Join('_', inputFilenames);
                var fullOutputPath = Path.Join(tempDirectory.DirectoryPath, outputFilename);
                var options = new MergeCommandOptions
                {
                    InputFiles = new List<string>(),
                    InputFormat = inputFormat,
                    OutputFile = fullOutputPath,
                    OutputFormat = outputFormat,
                    OutputVersion = outputVersion,
                    Hierarchical = hierarchical,
                    Group = group,
                    Name = name,
                    Version = version,
                };
                foreach (var inputFilename in inputFilenames)
                {
                    options.InputFiles.Add(Path.Combine("Resources", "Merge", inputFilename));
                }

                var exitCode = await MergeCommand.Merge(options).ConfigureAwait(true);

                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(fullOutputPath);
                bom = Regex.Replace(bom, @"\s*""serialNumber"": "".*?"",\r?\n", ""); // json
                bom = Regex.Replace(bom, @"\s+serialNumber="".*?""", ""); // xml
                Snapshot.Match(bom, SnapshotNameExtension.Create(hierarchical ? "Hierarchical" : "Flat", snapshotInputFilenames, inputFormat, outputFilename, outputFormat, outputVersion));
            }
        }
    }
}

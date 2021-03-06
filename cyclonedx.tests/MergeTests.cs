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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Snapshooter;
using Snapshooter.Xunit;

namespace CycloneDX.Cli.Tests
{
    public class MergeTests
    {
        [Theory]
        [InlineData(new string[] { "sbom1.json", "sbom2.json"}, StandardInputOutputBomFormat.autodetect, "sbom.json", StandardInputOutputBomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.json", "sbom2.json"}, StandardInputOutputBomFormat.autodetect, "sbom.xml", StandardInputOutputBomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.json", "sbom2.json"}, StandardInputOutputBomFormat.json, "sbom.json", StandardInputOutputBomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.xml", "sbom2.xml"}, StandardInputOutputBomFormat.autodetect, "sbom.xml", StandardInputOutputBomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.xml", "sbom2.xml"}, StandardInputOutputBomFormat.autodetect, "sbom.json", StandardInputOutputBomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.xml", "sbom2.xml"}, StandardInputOutputBomFormat.xml, "sbom.xml", StandardInputOutputBomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.json", "sbom2.xml"}, StandardInputOutputBomFormat.autodetect, "sbom.xml", StandardInputOutputBomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.json", "sbom2.json"}, StandardInputOutputBomFormat.autodetect, "sbom.json", StandardInputOutputBomFormat.json)]
        [InlineData(new string[] { "sbom1.json", "sbom2.json"}, StandardInputOutputBomFormat.autodetect, "sbom.xml", StandardInputOutputBomFormat.xml)]
        public async Task Merge(string[] inputFilenames, StandardInputOutputBomFormat inputFormat, string outputFilename, StandardInputOutputBomFormat outputFormat)
        {
            using (var tempDirectory = new TempDirectory())
            {
                var snapshotInputFilenames = string.Join('_', inputFilenames);
                var fullOutputPath = Path.Join(tempDirectory.DirectoryPath, outputFilename);
                var options = new MergeCommand.Options
                {
                    InputFiles = new List<string>(),
                    InputFormat = inputFormat,
                    OutputFile = fullOutputPath,
                    OutputFormat = outputFormat
                };
                foreach (var inputFilename in inputFilenames)
                {
                    options.InputFiles.Add(Path.Combine("Resources", "Merge", inputFilename));
                }
                
                var exitCode = await MergeCommand.Merge(options);
                
                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(fullOutputPath);
                Snapshot.Match(bom, SnapshotNameExtension.Create(snapshotInputFilenames, inputFormat, outputFilename, outputFormat));
            }
        }
    }
}

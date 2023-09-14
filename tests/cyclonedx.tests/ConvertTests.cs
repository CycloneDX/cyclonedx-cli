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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Snapshooter;
using Snapshooter.Xunit;
using CycloneDX.Cli.Commands;

namespace CycloneDX.Cli.Tests
{
    public class ConvertTests
    {
        [Theory]
        [InlineData("bom-1.0.xml", ConvertFormat.autodetect, "bom.xml", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.0.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.0.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.xml, SpecificationVersion.v1_1)]

        [InlineData("bom-1.1.xml", ConvertFormat.autodetect, "bom.xml", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.1.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.1.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.xml, SpecificationVersion.v1_1)]

        [InlineData("bom-1.2.xml", ConvertFormat.autodetect, "bom.xml", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.2.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.2.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.xml, null)]
        [InlineData("bom-1.2.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.xml, SpecificationVersion.v1_2)]

        [InlineData("bom-1.2.json", ConvertFormat.autodetect, "bom.json", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.2.json", ConvertFormat.json, "bom.json", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.2.json", ConvertFormat.json, "bom.json", ConvertFormat.json, SpecificationVersion.v1_2)]

        [InlineData("bom-1.3.xml", ConvertFormat.autodetect, "bom.xml", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.3.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.3.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.xml, null)]
        [InlineData("bom-1.3.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.xml, SpecificationVersion.v1_3)]

        [InlineData("bom-1.3.json", ConvertFormat.autodetect, "bom.json", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.3.json", ConvertFormat.json, "bom.json", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.3.json", ConvertFormat.json, "bom.json", ConvertFormat.json, null)]
        [InlineData("bom-1.3.json", ConvertFormat.json, "bom.json", ConvertFormat.json, SpecificationVersion.v1_3)]

        [InlineData("bom-1.4.xml", ConvertFormat.autodetect, "bom.xml", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.4.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.4.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.xml, null)]
        [InlineData("bom-1.4.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.xml, SpecificationVersion.v1_4)]

        [InlineData("bom-1.5.xml", ConvertFormat.autodetect, "bom.xml", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.5.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.5.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.xml, null)]
        [InlineData("bom-1.5.xml", ConvertFormat.xml, "bom.xml", ConvertFormat.xml, SpecificationVersion.v1_5)]

        [InlineData("bom-1.4.json", ConvertFormat.autodetect, "bom.json", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.4.json", ConvertFormat.json, "bom.json", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.4.json", ConvertFormat.json, "bom.json", ConvertFormat.json, null)]
        [InlineData("bom-1.4.json", ConvertFormat.json, "bom.json", ConvertFormat.json, SpecificationVersion.v1_4)]

        [InlineData("bom-1.5.json", ConvertFormat.autodetect, "bom.json", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.5.json", ConvertFormat.json, "bom.json", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.5.json", ConvertFormat.json, "bom.json", ConvertFormat.json, null)]
        [InlineData("bom-1.5.json", ConvertFormat.json, "bom.json", ConvertFormat.json, SpecificationVersion.v1_5)]

        [InlineData("bom.csv", ConvertFormat.autodetect, "bom.csv", ConvertFormat.autodetect, null)]
        [InlineData("bom.csv", ConvertFormat.csv, "bom.csv", ConvertFormat.autodetect, null)]
        [InlineData("bom.csv", ConvertFormat.csv, "bom.csv", ConvertFormat.csv, null)]

        [InlineData("bom-1.3.cdx", ConvertFormat.protobuf, "bom.cdx", ConvertFormat.autodetect, null)]
        [InlineData("bom-1.3.cdx", ConvertFormat.protobuf, "bom.json", ConvertFormat.json, SpecificationVersion.v1_3)]
        [InlineData("bom-1.3.json", ConvertFormat.json, "bom.cdx", ConvertFormat.protobuf, SpecificationVersion.v1_3)]
        public async Task Convert(string inputFilename, ConvertFormat inputFormat, string outputFilename, ConvertFormat outputFormat, SpecificationVersion? outputVersion)
        {
            using (var tempDirectory = new TempDirectory())
            {
                var fullOutputPath = Path.Join(tempDirectory.DirectoryPath, outputFilename);
                var exitCode = await ConvertCommand.Convert(new ConvertCommandOptions
                {
                    InputFile = Path.Combine("Resources", inputFilename),
                    OutputFile = fullOutputPath,
                    InputFormat = inputFormat,
                    OutputFormat = outputFormat,
                    OutputVersion = outputVersion,
                }).ConfigureAwait(false);
                
                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(fullOutputPath);
                Snapshot.Match(bom, SnapshotNameExtension.Create(inputFilename, inputFormat, outputFilename, outputFormat, outputVersion));
            }
        }

        [Theory]
        [InlineData(ConvertFormat.autodetect)]
        [InlineData(ConvertFormat.spdxjson)]
        public async Task ConvertToSpdxJson(ConvertFormat outputFormat)
        {
            using (var tempDirectory = new TempDirectory())
            {
                var outputFilename = Path.Combine(tempDirectory.DirectoryPath, "bom.spdx.json");
                var exitCode = await ConvertCommand.Convert(new ConvertCommandOptions
                {
                    InputFile = Path.Combine("Resources", "document.spdx.json"),
                    OutputFile = outputFilename,
                    InputFormat = ConvertFormat.autodetect,
                    OutputFormat = outputFormat
                    
                }).ConfigureAwait(false);
                
                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(outputFilename);
                bom = Regex.Replace(bom, @"Created"": .*\n", "");
                Snapshot.Match(bom, SnapshotNameExtension.Create(outputFormat));
            }
        }

        [Theory]
        [InlineData(ConvertFormat.autodetect)]
        [InlineData(ConvertFormat.spdxjson)]
        public async Task ConvertFromSpdxJson(ConvertFormat inputFormat)
        {
            using (var tempDirectory = new TempDirectory())
            {
                var outputFilename = Path.Combine(tempDirectory.DirectoryPath, "bom.spdx.json");
                var exitCode = await ConvertCommand.Convert(new ConvertCommandOptions
                {
                    InputFile = Path.Combine("Resources", "document.spdx.json"),
                    OutputFile = outputFilename,
                    InputFormat = inputFormat,
                    OutputFormat = ConvertFormat.xml,
                    
                }).ConfigureAwait(false);
                
                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(outputFilename);
                bom = Regex.Replace(bom, @"Created"": .*\n", "");
                Snapshot.Match(bom, SnapshotNameExtension.Create(inputFormat));
            }
        }
    }
}

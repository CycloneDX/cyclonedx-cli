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
using CycloneDX.Cli.Commands.Options;

namespace CycloneDX.Cli.Tests
{
    public class ConvertTests
    {
        [Theory]
        [InlineData("bom-1.0.xml", ConvertCommand.InputFormat.autodetect, "bom.xml", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom-1.0.xml", ConvertCommand.InputFormat.xml, "bom.xml", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom-1.0.xml", ConvertCommand.InputFormat.xml, "bom.xml", ConvertCommand.OutputFormat.xml_v1_1)]
        [InlineData("bom-1.1.xml", ConvertCommand.InputFormat.autodetect, "bom.xml", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom-1.1.xml", ConvertCommand.InputFormat.xml, "bom.xml", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom-1.1.xml", ConvertCommand.InputFormat.xml, "bom.xml", ConvertCommand.OutputFormat.xml_v1_1)]
        [InlineData("bom-1.2.xml", ConvertCommand.InputFormat.autodetect, "bom.xml", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom-1.2.xml", ConvertCommand.InputFormat.xml, "bom.xml", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom-1.2.xml", ConvertCommand.InputFormat.xml, "bom.xml", ConvertCommand.OutputFormat.xml)]
        [InlineData("bom-1.2.xml", ConvertCommand.InputFormat.xml, "bom.xml", ConvertCommand.OutputFormat.xml_v1_2)]
        [InlineData("bom-1.2.json", ConvertCommand.InputFormat.autodetect, "bom.json", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom-1.2.json", ConvertCommand.InputFormat.json, "bom.json", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom-1.2.json", ConvertCommand.InputFormat.json, "bom.json", ConvertCommand.OutputFormat.json_v1_2)]
        [InlineData("bom-1.3.xml", ConvertCommand.InputFormat.autodetect, "bom.xml", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom-1.3.xml", ConvertCommand.InputFormat.xml, "bom.xml", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom-1.3.xml", ConvertCommand.InputFormat.xml, "bom.xml", ConvertCommand.OutputFormat.xml)]
        [InlineData("bom-1.3.xml", ConvertCommand.InputFormat.xml, "bom.xml", ConvertCommand.OutputFormat.xml_v1_3)]
        [InlineData("bom-1.3.json", ConvertCommand.InputFormat.autodetect, "bom.json", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom-1.3.json", ConvertCommand.InputFormat.json, "bom.json", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom-1.3.json", ConvertCommand.InputFormat.json, "bom.json", ConvertCommand.OutputFormat.json)]
        [InlineData("bom-1.3.json", ConvertCommand.InputFormat.json, "bom.json", ConvertCommand.OutputFormat.json_v1_3)]
        [InlineData("bom.csv", ConvertCommand.InputFormat.autodetect, "bom.csv", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom.csv", ConvertCommand.InputFormat.csv, "bom.csv", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom.csv", ConvertCommand.InputFormat.csv, "bom.csv", ConvertCommand.OutputFormat.csv)]
        [InlineData("bom-1.3.cdx", ConvertCommand.InputFormat.protobuf, "bom.cdx", ConvertCommand.OutputFormat.autodetect)]
        [InlineData("bom-1.3.cdx", ConvertCommand.InputFormat.protobuf, "bom.json", ConvertCommand.OutputFormat.json_v1_3)]
        [InlineData("bom-1.3.json", ConvertCommand.InputFormat.json, "bom.cdx", ConvertCommand.OutputFormat.protobuf_v1_3)]
        public async Task Convert(string inputFilename, ConvertCommand.InputFormat inputFormat, string outputFilename, ConvertCommand.OutputFormat outputFormat)
        {
            using (var tempDirectory = new TempDirectory())
            {
                var fullOutputPath = Path.Join(tempDirectory.DirectoryPath, outputFilename);
                var exitCode = await ConvertCommand.Convert(new ConvertCommandOptions
                {
                    InputFile = Path.Combine("Resources", inputFilename),
                    OutputFile = fullOutputPath,
                    InputFormat = inputFormat,
                    OutputFormat = outputFormat
                });
                
                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(fullOutputPath);
                Snapshot.Match(bom, SnapshotNameExtension.Create(inputFilename, inputFormat, outputFilename, outputFormat));
            }
        }

        [Theory]
        [InlineData(ConvertCommand.OutputFormat.autodetect)]
        [InlineData(ConvertCommand.OutputFormat.spdxtag_v2_1)]
        [InlineData(ConvertCommand.OutputFormat.spdxtag_v2_2)]
        public async Task ConvertToSpdxTag(ConvertCommand.OutputFormat outputFormat)
        {
            using (var tempDirectory = new TempDirectory())
            {
                var outputFilename = Path.Combine(tempDirectory.DirectoryPath, "bom.spdx");
                var exitCode = await ConvertCommand.Convert(new ConvertCommandOptions
                {
                    InputFile = Path.Combine("Resources", "bom-1.2.xml"),
                    OutputFile = outputFilename,
                    InputFormat = ConvertCommand.InputFormat.autodetect,
                    OutputFormat = outputFormat
                    
                });
                
                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(outputFilename);
                bom = Regex.Replace(bom, @"Created: .*\n", "");
                Snapshot.Match(bom, SnapshotNameExtension.Create(outputFormat));
            }
        }
    }
}

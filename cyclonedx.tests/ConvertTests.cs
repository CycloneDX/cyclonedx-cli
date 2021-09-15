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
        [InlineData("bom-1.0.xml", ConvertInputFormat.autodetect, "bom.xml", ConvertOutputFormat.autodetect)]
        [InlineData("bom-1.0.xml", ConvertInputFormat.xml, "bom.xml", ConvertOutputFormat.autodetect)]
        [InlineData("bom-1.0.xml", ConvertInputFormat.xml, "bom.xml", ConvertOutputFormat.xml_v1_1)]
        [InlineData("bom-1.1.xml", ConvertInputFormat.autodetect, "bom.xml", ConvertOutputFormat.autodetect)]
        [InlineData("bom-1.1.xml", ConvertInputFormat.xml, "bom.xml", ConvertOutputFormat.autodetect)]
        [InlineData("bom-1.1.xml", ConvertInputFormat.xml, "bom.xml", ConvertOutputFormat.xml_v1_1)]
        [InlineData("bom-1.2.xml", ConvertInputFormat.autodetect, "bom.xml", ConvertOutputFormat.autodetect)]
        [InlineData("bom-1.2.xml", ConvertInputFormat.xml, "bom.xml", ConvertOutputFormat.autodetect)]
        [InlineData("bom-1.2.xml", ConvertInputFormat.xml, "bom.xml", ConvertOutputFormat.xml)]
        [InlineData("bom-1.2.xml", ConvertInputFormat.xml, "bom.xml", ConvertOutputFormat.xml_v1_2)]
        [InlineData("bom-1.2.json", ConvertInputFormat.autodetect, "bom.json", ConvertOutputFormat.autodetect)]
        [InlineData("bom-1.2.json", ConvertInputFormat.json, "bom.json", ConvertOutputFormat.autodetect)]
        [InlineData("bom-1.2.json", ConvertInputFormat.json, "bom.json", ConvertOutputFormat.json_v1_2)]
        [InlineData("bom-1.3.xml", ConvertInputFormat.autodetect, "bom.xml", ConvertOutputFormat.autodetect)]
        [InlineData("bom-1.3.xml", ConvertInputFormat.xml, "bom.xml", ConvertOutputFormat.autodetect)]
        [InlineData("bom-1.3.xml", ConvertInputFormat.xml, "bom.xml", ConvertOutputFormat.xml)]
        [InlineData("bom-1.3.xml", ConvertInputFormat.xml, "bom.xml", ConvertOutputFormat.xml_v1_3)]
        [InlineData("bom-1.3.json", ConvertInputFormat.autodetect, "bom.json", ConvertOutputFormat.autodetect)]
        [InlineData("bom-1.3.json", ConvertInputFormat.json, "bom.json", ConvertOutputFormat.autodetect)]
        [InlineData("bom-1.3.json", ConvertInputFormat.json, "bom.json", ConvertOutputFormat.json)]
        [InlineData("bom-1.3.json", ConvertInputFormat.json, "bom.json", ConvertOutputFormat.json_v1_3)]
        [InlineData("bom.csv", ConvertInputFormat.autodetect, "bom.csv", ConvertOutputFormat.autodetect)]
        [InlineData("bom.csv", ConvertInputFormat.csv, "bom.csv", ConvertOutputFormat.autodetect)]
        [InlineData("bom.csv", ConvertInputFormat.csv, "bom.csv", ConvertOutputFormat.csv)]
        [InlineData("bom-1.3.cdx", ConvertInputFormat.protobuf, "bom.cdx", ConvertOutputFormat.autodetect)]
        [InlineData("bom-1.3.cdx", ConvertInputFormat.protobuf, "bom.json", ConvertOutputFormat.json_v1_3)]
        [InlineData("bom-1.3.json", ConvertInputFormat.json, "bom.cdx", ConvertOutputFormat.protobuf_v1_3)]
        public async Task Convert(string inputFilename, ConvertInputFormat inputFormat, string outputFilename, ConvertOutputFormat outputFormat)
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
                }).ConfigureAwait(false);
                
                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(fullOutputPath);
                Snapshot.Match(bom, SnapshotNameExtension.Create(inputFilename, inputFormat, outputFilename, outputFormat));
            }
        }

        [Theory]
        [InlineData(ConvertOutputFormat.autodetect)]
        [InlineData(ConvertOutputFormat.spdxtag_v2_1)]
        [InlineData(ConvertOutputFormat.spdxtag_v2_2)]
        public async Task ConvertToSpdxTag(ConvertOutputFormat outputFormat)
        {
            using (var tempDirectory = new TempDirectory())
            {
                var outputFilename = Path.Combine(tempDirectory.DirectoryPath, "bom.spdx");
                var exitCode = await ConvertCommand.Convert(new ConvertCommandOptions
                {
                    InputFile = Path.Combine("Resources", "bom-1.2.xml"),
                    OutputFile = outputFilename,
                    InputFormat = ConvertInputFormat.autodetect,
                    OutputFormat = outputFormat
                    
                }).ConfigureAwait(false);
                
                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(outputFilename);
                bom = Regex.Replace(bom, @"Created: .*\n", "");
                Snapshot.Match(bom, SnapshotNameExtension.Create(outputFormat));
            }
        }
    }
}

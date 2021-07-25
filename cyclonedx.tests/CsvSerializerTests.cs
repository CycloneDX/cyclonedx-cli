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
using Xunit;
using Snapshooter;
using Snapshooter.Xunit;
using CycloneDX.Xml;

namespace CycloneDX.Cli.Tests
{
    public class CsvSerializerTests
    {
        [Theory]
        [InlineData("bom")]
        [InlineData("valid-component-hashes")]
        [InlineData("valid-component-swid")]
        [InlineData("valid-component-swid-full")]
        [InlineData("valid-component-types")]
        [InlineData("valid-license-expression")]
        [InlineData("valid-license-id")]
        [InlineData("valid-license-name")]
        public void SerializationTests(string filename)
        {
            using (var tempDirectory = new TempDirectory())
            {
                var resourceFilename = Path.Join("Resources", filename + "-1.2.xml");
                var inputBomString = File.ReadAllText(resourceFilename);
                var bom = Xml.Deserializer.Deserialize(inputBomString);

                var bomCsv = CsvSerializer.Serialize(bom);

                Snapshot.Match(bomCsv, SnapshotNameExtension.Create(filename));
            }
        }

        [Theory]
        [InlineData("bom")]
        [InlineData("bom-minimum-viable")]
        [InlineData("bom-lowercase-field-names")]
        [InlineData("valid-component-hashes")]
        [InlineData("valid-component-swid")]
        [InlineData("valid-component-swid-full")]
        [InlineData("valid-component-types")]
        [InlineData("valid-license-expression")]
        [InlineData("valid-license-id")]
        [InlineData("valid-license-name")]
        public void DeserializationTests(string filename)
        {
            using (var tempDirectory = new TempDirectory())
            {
                var resourceFilename = Path.Join("Resources", filename + ".csv");
                var inputBomString = File.ReadAllText(resourceFilename);

                var bom = CsvSerializer.Deserialize(inputBomString);

                var bomXml = Xml.Serializer.Serialize(bom);

                Snapshot.Match(bomXml, SnapshotNameExtension.Create(filename));
            }
        }
    }
}

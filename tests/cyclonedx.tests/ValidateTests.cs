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
using CycloneDX.Cli.Commands;

namespace CycloneDX.Cli.Tests
{
    public class ValidateTests
    {
        [Theory]
        [InlineData("bom-1.0.xml", ValidationBomFormat.autodetect, null,true)]
        [InlineData("bom-1.0.xml", ValidationBomFormat.xml, null, true)]
        [InlineData("bom-1.0.xml", ValidationBomFormat.xml, SpecificationVersion.v1_0, true)]
        [InlineData("bom-1.1.xml", ValidationBomFormat.autodetect, null, true)]
        [InlineData("bom-1.1.xml", ValidationBomFormat.xml, SpecificationVersion.v1_1, true)]
        [InlineData("bom-1.2.xml", ValidationBomFormat.autodetect, null, true)]
        [InlineData("bom-1.2.xml", ValidationBomFormat.xml, SpecificationVersion.v1_2, true)]
        [InlineData("bom-1.3.xml", ValidationBomFormat.autodetect, null, true)]
        [InlineData("bom-1.3.xml", ValidationBomFormat.xml, SpecificationVersion.v1_3, true)]
        [InlineData("bom-1.2.json", ValidationBomFormat.autodetect, null, true)]
        [InlineData("bom-1.2.json", ValidationBomFormat.json, null, true)]
        [InlineData("bom-1.2.json", ValidationBomFormat.json, SpecificationVersion.v1_2, true)]
        [InlineData("bom-1.3.json", ValidationBomFormat.autodetect, null, true)]
        [InlineData("bom-1.3.json", ValidationBomFormat.json, SpecificationVersion.v1_3, true)]
        // these two are currently failing due to the .NET library throwing an exception
        // [InlineData("bom-1.0.xml", CycloneDXFormat.json, false)]
        // [InlineData("bom-1.2.json", CycloneDXFormat.xml, false)]
        public async Task Validate(string inputFilename, ValidationBomFormat inputFormat, SpecificationVersion? inputVersion, bool valid)
        {
            var exitCode = await ValidateCommand.Validate(new ValidateCommandOptions
            {
                InputFile = Path.Combine("Resources", inputFilename),
                InputFormat = inputFormat,
                InputVersion = inputVersion,
            }).ConfigureAwait(false);
            
            if (valid)
            {
                Assert.Equal(ExitCode.Ok, (ExitCode)exitCode);
            }
            else
            {
                Assert.Equal(ExitCode.OkFail, (ExitCode)exitCode);
            }
        }
    }
}

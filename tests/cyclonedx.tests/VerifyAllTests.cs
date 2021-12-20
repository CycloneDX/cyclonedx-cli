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
using CycloneDX.Cli.Commands.Verify;

namespace CycloneDX.Cli.Tests
{
    public class VerifyAllTests
    {
        [Fact]
        public async Task OriginalBomSignatureVerifies()
        {
            var exitCode = await VerifyAllCommand.VerifyAll(new VerifyAllCommandOptions
            {
                BomFile = Path.Combine("Resources", "signed-bom.xml"),
                KeyFile = Path.Combine("Resources", "public.key")
            }).ConfigureAwait(false);
            
            Assert.Equal(ExitCode.Ok, (ExitCode)exitCode);
        }

        [Fact]
        public async Task ModifiedBomFailsSignatureVerification()
        {
            var exitCode = await VerifyAllCommand.VerifyAll(new VerifyAllCommandOptions
            {
                BomFile = Path.Combine("Resources", "signed-bom-modified.xml"),
                KeyFile = Path.Combine("Resources", "public.key")
            }).ConfigureAwait(false);
            
            Assert.Equal(ExitCode.SignatureFailedVerification, (ExitCode)exitCode);
        }
    }
}

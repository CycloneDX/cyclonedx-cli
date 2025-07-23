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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CycloneDX.Cli.Commands.Sign
{
    public static class SignFileCommand
    {
        public static void Configure(Command rootCommand)
        {
            Contract.Requires(rootCommand != null);
            var subCommand = new Command("file", "Sign arbitrary files and generate a PKCS1 RSA SHA256 signature file");
            subCommand.Add(new Argument<string>("file", "Filename of the file the signature will be created for"));
            subCommand.Add(new Option<string>("--key-file", "Signing key filename (RSA private key in PEM format, defaults to \"private.key\")"));
            subCommand.Add(new Option<string>("--signature-file", "Filename of the generated signature file (defaults to the filename with \".sig\" appended)"));
            subCommand.Handler = CommandHandler.Create<SignFileCommandOptions>(SignFile);
            rootCommand.Add(subCommand);
        }

        [SuppressMessage("Microsoft.Performance", "CA1835:PreferStreamAsyncMemoryOverloads")]
        public static async Task<int> SignFile(SignFileCommandOptions options)
        {
            Contract.Requires(options != null);

            var keyFilename = string.IsNullOrEmpty(options.KeyFile) ? "private.key" : options.KeyFile;
            var signatureFilename = string.IsNullOrEmpty(options.SignatureFile) ? options.File + ".sig" : options.SignatureFile;

            Console.WriteLine("Loading private key...");
            var privateKey = await File.ReadAllTextAsync(keyFilename).ConfigureAwait(false);
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKey.ToCharArray());

            Console.WriteLine("Generating signature...");
            using (var stream = File.OpenRead(options.File))
            {
                var sig = rsa.SignData(stream, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                Console.WriteLine("Writing signature file...");
                using (var sigStream = File.OpenWrite(signatureFilename))
                {
                    await sigStream.WriteAsync(sig, 0, sig.Length).ConfigureAwait(false);
                }
            }
            Console.WriteLine("Finished");
            return (int)ExitCode.Ok;
        }
    }
}

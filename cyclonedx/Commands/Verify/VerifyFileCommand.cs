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
using System.Diagnostics.Contracts;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CycloneDX.Cli.Commands.Verify
{
    public static class VerifyFileCommand
    {
        public static void Configure(Command rootCommand)
        {
            Contract.Requires(rootCommand != null);
            var subCommand = new Command("file", "Verifies a PKCS1 RSA SHA256 signature file for an abritrary file");
            subCommand.Add(new Argument<string>("file", "File the signature file is for"));
            subCommand.Add(new Option<string>("--key-file", "Public key filename (RSA public key in PEM format, defaults to \"public.key\")"));
            subCommand.Add(new Option<string>("--signature-file", "Signature file to be verified (defaults to the filename with \".sig\" appended)"));
            subCommand.Handler = CommandHandler.Create<VerifyFileCommandOptions>(VerifyFile);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> VerifyFile(VerifyFileCommandOptions options)
        {
            Contract.Requires(options != null);

            var keyFilename = string.IsNullOrEmpty(options.KeyFile) ? "public.key" : options.KeyFile;
            var signatureFilename = string.IsNullOrEmpty(options.SignatureFile) ? options.File + ".sig" : options.SignatureFile;

            Console.WriteLine("Loading public key...");
            var publicKey = await File.ReadAllTextAsync(keyFilename).ConfigureAwait(false);
            var rsa = RSA.Create();
            rsa.ImportFromPem(publicKey.ToCharArray());

            Console.WriteLine("Reading signature...");
            var signature = await File.ReadAllBytesAsync(signatureFilename).ConfigureAwait(false);

            Console.WriteLine("Verifying signature...");
            using (var stream = File.OpenRead(options.File))
            {
                var verified = rsa.VerifyData(stream, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                Console.WriteLine();
                if (verified)
                {
                    Console.WriteLine("Signature successfully verified");
                    return (int)ExitCode.Ok;
                }
                else
                {
                    Console.WriteLine("Signature failed verification");
                    return (int)ExitCode.SignatureFailedVerification;
                }
            }

        }
    }
}

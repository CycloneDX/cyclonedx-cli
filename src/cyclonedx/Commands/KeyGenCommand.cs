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
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CycloneDX.Cli.Commands
{
    internal static class KeyGenCommand
    {
        internal static void Configure(RootCommand rootCommand)
        {
            var subCommand = new Command("keygen", "Generates an RSA public/private key pair for BOM signing");
            subCommand.Add(new Option<string>("--private-key-file", "Filename for generated private key file (defaults to \"private.key\")"));
            subCommand.Add(new Option<string>("--public-key-file", "Filename for generated public key file (defaults to \"public.key\")"));
            subCommand.Handler = CommandHandler.Create<KeyGenCommandOptions>(KeyGen);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> KeyGen(KeyGenCommandOptions options)
        {
            Console.WriteLine("Generating new public/private key pair...");
            using (RSA rsa = new RSACryptoServiceProvider(2048))
            {
                var publicKeyFilename = string.IsNullOrEmpty(options.PublicKeyFile) ? "public.key" : options.PublicKeyFile;
                Console.WriteLine($"Saving public key to {publicKeyFilename}");
                byte[] pubKeyBytes = rsa.ExportSubjectPublicKeyInfo();
                char[] pubKeyPem = PemEncoding.Write("PUBLIC KEY", pubKeyBytes);
                await File.WriteAllTextAsync(publicKeyFilename, new string(pubKeyPem)).ConfigureAwait(false);

                var privateKeyFilename = string.IsNullOrEmpty(options.PrivateKeyFile) ? "private.key" : options.PrivateKeyFile;
                Console.WriteLine($"Saving private key to {privateKeyFilename}");
                byte[] privKeyBytes = rsa.ExportPkcs8PrivateKey();
                char[] privKeyPem = PemEncoding.Write("PRIVATE KEY", privKeyBytes);
                await File.WriteAllTextAsync(privateKeyFilename, new string(privKeyPem)).ConfigureAwait(false);
            }
            return (int)ExitCode.Ok;
        }
    }
}

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
using System.CommandLine.NamingConventionBinder;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Xml;

namespace CycloneDX.Cli.Commands.Verify
{
    public static class VerifyAllCommand
    {
        public static void Configure(Command rootCommand)
        {
            Contract.Requires(rootCommand != null);
            var subCommand = new Command("all", "Verify all signatures in a BOM");
            subCommand.Add(new Argument<string>("bom-file", "BOM filename"));
            subCommand.Add(new Option<string>("--key-file", "Public key filename (RSA public key in PEM format, defaults to \"public.key\")"));
            subCommand.Handler = CommandHandler.Create<VerifyAllCommandOptions>(VerifyAll);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> VerifyAll(VerifyAllCommandOptions options)
        {
            Contract.Requires(options != null);

            var keyFilename = string.IsNullOrEmpty(options.KeyFile) ? "public.key" : options.KeyFile;

            Console.WriteLine("Loading public key...");
            var publicKey = await File.ReadAllTextAsync(keyFilename).ConfigureAwait(false);
            var rsa = RSA.Create();
            rsa.ImportFromPem(publicKey.ToCharArray());

            if (options.BomFile.EndsWith(".xml", true, CultureInfo.InvariantCulture))
            {
                Console.WriteLine("Loading XML BOM...");
                var bom = new XmlDocument();
                bom.PreserveWhitespace = true;
                bom.Load(options.BomFile);

                Console.WriteLine("Reading signatures...");
                var signatures = bom.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");

                if (signatures.Count == 0)
                {
                    Console.WriteLine("No signatures found");
                    return (int)ExitCode.SignatureNotFound;
                }
                else
                {
                    Console.WriteLine($"Found {signatures.Count} signatures...");
                    var allSignaturesOk = true;
                    for (var i=0; i<signatures.Count; i++)
                    {
                        Console.Write($"Verifying signature {i+1}... ");
                        var signedXml = new SignedXml(bom);
                        signedXml.LoadXml((XmlElement)signatures[i]);
                        var valid = signedXml.CheckSignature(rsa);
                        if (valid)
                        {
                            Console.WriteLine("verified");
                        }
                        else
                        {
                            Console.WriteLine("failed verification");
                            allSignaturesOk = false;
                        }
                    }

                    Console.WriteLine();
                    if (allSignaturesOk)
                    {
                        Console.WriteLine("All signatures verified");
                        return (int)ExitCode.Ok;
                    }
                    else
                    {
                        Console.WriteLine("Signatures failed verification");
                        return (int)ExitCode.SignatureFailedVerification;
                    }
                }
            }
            else
            {
                Console.WriteLine("Only XML BOMs are supported for signature verification.");
                return (int)ExitCode.ParameterValidationError;
            }
        }
    }
}

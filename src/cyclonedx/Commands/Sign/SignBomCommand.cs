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

namespace CycloneDX.Cli.Commands.Sign
{
    public static class SignBomCommand
    {
        public static void Configure(Command rootCommand)
        {
            Contract.Requires(rootCommand != null);
            var subCommand = new Command("bom", "Sign the entire BOM document");
            subCommand.Add(new Argument<string>("bom-file", "BOM filename"));
            subCommand.Add(new Option<string>("--key-file", "Signing key filename (RSA private key in PEM format, defaults to \"private.key\")"));
            subCommand.Handler = CommandHandler.Create<SignBomCommandOptions>(SignBom);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> SignBom(SignBomCommandOptions options)
        {
            Contract.Requires(options != null);

            var keyFilename = string.IsNullOrEmpty(options.KeyFile) ? "private.key" : options.KeyFile;

            Console.WriteLine("Loading private key...");
            var privateKey = await File.ReadAllTextAsync(keyFilename).ConfigureAwait(false);
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKey.ToCharArray());

            if (options.BomFile.EndsWith(".xml", true, CultureInfo.InvariantCulture))
            {
                Console.WriteLine("Loading XML BOM...");
                var bom = new XmlDocument();
                bom.PreserveWhitespace = true;
                bom.Load(options.BomFile);

                Console.WriteLine("Generating signature...");
                var signedXml = new SignedXml(bom);
                signedXml.SigningKey = rsa;
                var reference = new Reference("");
                var envelope = new XmlDsigEnvelopedSignatureTransform();
                reference.AddTransform(envelope);
                signedXml.AddReference(reference);
                signedXml.ComputeSignature();

                Console.WriteLine("Saving signature...");
                var xmlDigitalSignature = signedXml.GetXml();
                bom.DocumentElement.AppendChild(bom.ImportNode(xmlDigitalSignature, true));
                bom.Save(options.BomFile);

                return (int)ExitCode.Ok;
            }
            else
            {
                Console.WriteLine("Only XML BOMs are currently supported for signing.");
                return (int)ExitCode.ParameterValidationError;
            }
        }
    }
}

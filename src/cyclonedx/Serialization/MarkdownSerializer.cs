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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using CycloneDX.Models;

namespace CycloneDX.Cli.Serialization
{
    public static class MarkdownSerializer
    {
        public static string Serialize(Bom bom)
        {
            Contract.Requires(bom != null);
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                if (bom.Metadata?.Component != null)
                {
                    writer.WriteLine("# " + bom.Metadata.Component.Name + " (" + bom.Metadata.Component.Type + ")");
                }
                writer.WriteLine("BOM " + bom.SerialNumber + " " + bom.BomFormat + " " + bom.SpecVersionString + " " + bom.Version + " " + bom.Metadata?.Timestamp);
                writer.WriteLine();

                if (bom.Metadata != null)
                {
                    var m = bom.Metadata;

                    // BOM tools or authors
                    if (m.Tools != null)
                    {
                        writer.Write("BOM done with tools: ");
                        foreach (var t in m.Tools)
                        {
                            writer.Write(t.Name + " " + t.Version + " (" + t.Vendor + "), ");
                        }
                        writer.WriteLine();
                    }
                    else if (m.Authors != null)
                    {
                        writer.Write("BOM authored by: ");
                        foreach (var a in m.Authors)
                        {
                            writer.Write(a.Name + ", ");
                        }
                        writer.WriteLine();
                    }

                    // Component
                    if (m.Component != null)
                    {
                        var c = m.Component;

                        writer.Write(">");
                        if (c.Group != null)
                        {
                            writer.Write(" _group:_ **" + c.Group + "**");
                        }
                        if (c.Name != null)
                        {
                            writer.Write(" _name:_ **" + c.Name + "**");
                        }
                        if (c.Version != null)
                        {
                            writer.Write(" _version:_ **" + c.Version + "**");
                        }
                        if (c.Name == null && c.Purl != null)
                        {
                            writer.Write(" _purl:_ **" + c.Purl + "**");
                        }
                        if (c.Cpe != null)
                        {
                            writer.Write(" _CPE:_ **" + c.Cpe + "**");
                        }
                        writer.WriteLine();
                        if (c.Description != null)
                        {
                            writer.WriteLine(">");
                            writer.WriteLine("> " + c.Description);
                        }
                        writer.WriteLine();

                        if (c.ExternalReferences != null)
                        {
                            writer.Write("Component external references: ");
                            foreach (var er in c.ExternalReferences)
                            {
                                writer.Write("[" + er.Type + "](" + er.Url + "), ");
                            }
                            writer.WriteLine();
                        }
                    }

                    if (bom.ExternalReferences != null)
                    {
                        writer.WriteLine("## ExternalReferences");
                        writer.WriteLine("not supported yet"); // how is it different from bom.Metadata.Component.ExternalReferences?
                        writer.WriteLine();
                    }

                    if (bom.Components != null)
                    {
                        writer.WriteLine("## Components");
                        foreach (var c in bom.Components)
                        {
                            writer.Write("1. " + c.Type);
                            if (c.Group != null)
                            {
                                writer.Write(" _group:_ **" + c.Group + "**");
                            }
                            if (c.Name != null)
                            {
                                writer.Write(" _name:_ **" + c.Name + "**");
                            }
                            if (c.Version != null)
                            {
                                writer.Write(" _version:_ **" + c.Version + "**");
                            }
                            if (c.Scope != null)
                            {
                                writer.Write(" _scope:_ " + c.Scope);
                            }
                            if (c.Purl != null)
                            {
                                writer.WriteLine(" \\");
                                writer.Write("   _purl:_ " + c.Purl);
                            }
                            writer.WriteLine();
                        }
                        writer.WriteLine();
                    }

                    if (bom.Services != null)
                    {
                        writer.WriteLine("## Services");
                        writer.WriteLine("not supported yet");
                        writer.WriteLine();
                    }

                    if (bom.Dependencies != null) {
                        writer.WriteLine("## Dependencies");
                        WriteDependencies(writer, bom.Dependencies, "");
                        writer.WriteLine();
                    }

                    if (bom.Compositions != null)
                    {
                        writer.WriteLine("## Compositions");
                        writer.WriteLine("not supported yet");
                        writer.WriteLine();
                    }

                    if (bom.Vulnerabilities != null)
                    {
                        writer.WriteLine("## Vulnerabilities");
                        writer.WriteLine("not supported yet");
                        writer.WriteLine();
                    }
                }
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private static void WriteDependencies(StreamWriter writer, List<Dependency> dependencies, string indent)
        {
            foreach (var d in dependencies)
            {
                writer.WriteLine(indent + "- " + d.Ref);
                if (d.Dependencies != null) {
                    WriteDependencies(writer, d.Dependencies, indent + "  ");
                }
            }
        }

        public static Bom Deserialize(string csv)
        {
            return null;
        }
    }
}

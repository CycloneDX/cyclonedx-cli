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
#if NET8_0_OR_GREATER
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using CycloneDX.Cli.Commands;

namespace CycloneDX.Cli.Tests
{
    public class AttachDanglingComponentsTests
    {
        [Fact]
        public async Task Merge_AttachDanglingComponents_ClosesTheDependencyGraph()
        {
            using (var tempDirectory = new TempDirectory())
            {
                var fullOutputPath = Path.Join(tempDirectory.DirectoryPath, "sbom.json");
                var options = new MergeCommandOptions
                {
                    InputFiles = new List<string> { Path.Combine("Resources", "AttachDanglingComponents", "sbom1.json") },
                    InputFormat = CycloneDXBomFormat.autodetect,
                    OutputFile = fullOutputPath,
                    OutputFormat = CycloneDXBomFormat.autodetect,
                    AttachDanglingComponents = true,
                };

                var exitCode = await MergeCommand.Merge(options).ConfigureAwait(false);

                Assert.Equal(0, exitCode);
                using var doc = JsonDocument.Parse(File.ReadAllText(fullOutputPath));
                var root = doc.RootElement;

                var componentRefs = new HashSet<string>();
                foreach (var c in root.GetProperty("components").EnumerateArray())
                {
                    componentRefs.Add(c.GetProperty("bom-ref").GetString());
                }

                // orphan-lib (required) and orphan-test-lib (excluded) had no
                // incoming dependsOn edge -- each should now have its own
                // scope-bucketed attachment component.
                Assert.Contains("unreferenced-components:scope=Required", componentRefs);
                Assert.Contains("unreferenced-components:scope=Excluded", componentRefs);

                var outgoing = new Dictionary<string, List<string>>();
                foreach (var d in root.GetProperty("dependencies").EnumerateArray())
                {
                    var list = new List<string>();
                    if (d.TryGetProperty("dependsOn", out var dependsOn))
                    {
                        foreach (var t in dependsOn.EnumerateArray()) list.Add(t.GetString());
                    }
                    outgoing[d.GetProperty("ref").GetString()] = list;
                }

                // Every component must now be reachable from the subject via
                // a directed walk of dependsOn edges.
                var seen = new HashSet<string> { "app" };
                var stack = new Stack<string>();
                stack.Push("app");
                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    if (!outgoing.TryGetValue(current, out var children)) continue;
                    foreach (var child in children)
                    {
                        if (seen.Add(child)) stack.Push(child);
                    }
                }

                foreach (var bomRef in componentRefs)
                {
                    Assert.True(seen.Contains(bomRef), $"'{bomRef}' is not reachable from the subject after AttachDanglingComponents");
                }
            }
        }

        [Fact]
        public async Task Merge_WithoutAttachDanglingComponents_LeavesGraphAsIs()
        {
            using (var tempDirectory = new TempDirectory())
            {
                var fullOutputPath = Path.Join(tempDirectory.DirectoryPath, "sbom.json");
                var options = new MergeCommandOptions
                {
                    InputFiles = new List<string> { Path.Combine("Resources", "AttachDanglingComponents", "sbom1.json") },
                    InputFormat = CycloneDXBomFormat.autodetect,
                    OutputFile = fullOutputPath,
                    OutputFormat = CycloneDXBomFormat.autodetect,
                    AttachDanglingComponents = false,
                };

                var exitCode = await MergeCommand.Merge(options).ConfigureAwait(false);

                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(fullOutputPath);
                Assert.DoesNotContain("unreferenced-components", bom);
            }
        }
    }
}
#endif

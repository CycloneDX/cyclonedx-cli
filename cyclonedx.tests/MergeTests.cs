using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Snapshooter;
using Snapshooter.Xunit;
using CycloneDX.CLI;
using CycloneDX.CLI.Models;

namespace CycloneDX.CLI.Tests
{
    public class MergeTests
    {
        [Theory]
        [InlineData(new string[] { "sbom1.json", "sbom2.json"}, StandardInputOutputSbomFormat.autodetect, "sbom.json", StandardInputOutputSbomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.json", "sbom2.json"}, StandardInputOutputSbomFormat.autodetect, "sbom.xml", StandardInputOutputSbomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.json", "sbom2.json"}, StandardInputOutputSbomFormat.json, "sbom.json", StandardInputOutputSbomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.xml", "sbom2.xml"}, StandardInputOutputSbomFormat.autodetect, "sbom.xml", StandardInputOutputSbomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.xml", "sbom2.xml"}, StandardInputOutputSbomFormat.autodetect, "sbom.json", StandardInputOutputSbomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.xml", "sbom2.xml"}, StandardInputOutputSbomFormat.xml, "sbom.xml", StandardInputOutputSbomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.json", "sbom2.xml"}, StandardInputOutputSbomFormat.autodetect, "sbom.xml", StandardInputOutputSbomFormat.autodetect)]
        [InlineData(new string[] { "sbom1.json", "sbom2.json"}, StandardInputOutputSbomFormat.autodetect, "sbom.json", StandardInputOutputSbomFormat.json)]
        [InlineData(new string[] { "sbom1.json", "sbom2.json"}, StandardInputOutputSbomFormat.autodetect, "sbom.xml", StandardInputOutputSbomFormat.xml)]
        public async Task Merge(string[] inputFilenames, StandardInputOutputSbomFormat inputFormat, string outputFilename, StandardInputOutputSbomFormat outputFormat)
        {
            using (var tempDirectory = new TempDirectory())
            {
                var snapshotInputFilenames = string.Join('_', inputFilenames);
                var fullOutputPath = Path.Join(tempDirectory.DirectoryPath, outputFilename);
                var options = new Program.MergeCommandOptions
                {
                    InputFormat = inputFormat,
                    OutputFile = fullOutputPath,
                    OutputFormat = outputFormat
                };
                foreach (var inputFilename in inputFilenames)
                {
                    options.InputFiles.Add(Path.Combine("Resources", "Merge", inputFilename));
                }
                
                var exitCode = await Program.Merge(options);
                
                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(fullOutputPath);
                Snapshot.Match(bom, SnapshotNameExtension.Create(snapshotInputFilenames, inputFormat, outputFilename, outputFormat));
            }
        }
    }
}

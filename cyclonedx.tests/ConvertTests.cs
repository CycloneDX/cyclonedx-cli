using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Snapshooter;
using Snapshooter.Xunit;
using CycloneDX.CLI;

namespace CycloneDX.CLI.Tests
{
    public class ConvertTests
    {
        [Fact]
        public async Task CanConvertFromXmlToJson()
        {
            using (var tempDirectory = new TempDirectory())
            {
                var outputFilename = Path.Combine(tempDirectory.DirectoryPath, "bom.json");
                var exitCode = await Program.Convert(
                    Path.Combine("Resources", "bom-1.2.xml"),
                    outputFilename,
                    Models.InputFormat.autodetect,
                    Commands.ConvertOutputFormat.autodetect);
                
                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(outputFilename);
                Snapshot.Match(bom);
            }
        }

        [Fact]
        public async Task CanConvertFromJsonToXml()
        {
            using (var tempDirectory = new TempDirectory())
            {
                var outputFilename = Path.Combine(tempDirectory.DirectoryPath, "bom.xml");
                var exitCode = await Program.Convert(
                    Path.Combine("Resources", "bom-1.2.json"),
                    outputFilename,
                    Models.InputFormat.autodetect,
                    Commands.ConvertOutputFormat.autodetect);
                
                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(outputFilename);
                Snapshot.Match(bom);
            }
        }

        [Fact]
        public async Task CanConvertToSpdxTag_v2_1()
        {
            using (var tempDirectory = new TempDirectory())
            {
                var outputFilename = Path.Combine(tempDirectory.DirectoryPath, "bom.txt");
                var exitCode = await Program.Convert(
                    Path.Combine("Resources", "bom-1.2.xml"),
                    outputFilename,
                    Models.InputFormat.autodetect,
                    Commands.ConvertOutputFormat.spdxtag_v2_1);
                
                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(outputFilename);
                bom = Regex.Replace(bom, @"Created: .*\n", "");
                Snapshot.Match(bom);
            }
        }

        [Fact]
        public async Task CanConvertToSpdxTag_v2_2()
        {
            using (var tempDirectory = new TempDirectory())
            {
                var outputFilename = Path.Combine(tempDirectory.DirectoryPath, "bom.txt");
                var exitCode = await Program.Convert(
                    Path.Combine("Resources", "bom-1.2.xml"),
                    outputFilename,
                    Models.InputFormat.autodetect,
                    Commands.ConvertOutputFormat.spdxtag_v2_2);
                
                Assert.Equal(0, exitCode);
                var bom = File.ReadAllText(outputFilename);
                bom = Regex.Replace(bom, @"Created: .*\n", "");
                Snapshot.Match(bom);
            }
        }
    }
}

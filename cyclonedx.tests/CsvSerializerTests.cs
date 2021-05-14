using System;
using System.IO;
using Xunit;
using Snapshooter;
using Snapshooter.Xunit;
using CycloneDX.Xml;

namespace CycloneDX.CLI.Tests
{
    public class CsvSerializerTests
    {
        [Theory]
        [InlineData("bom")]
        [InlineData("valid-component-hashes")]
        [InlineData("valid-component-swid")]
        [InlineData("valid-component-swid-full")]
        [InlineData("valid-component-types")]
        [InlineData("valid-license-expression")]
        [InlineData("valid-license-id")]
        [InlineData("valid-license-name")]
        public void SerializationTests(string filename)
        {
            using (var tempDirectory = new TempDirectory())
            {
                var resourceFilename = Path.Join("Resources", filename + "-1.2.xml");
                var inputBomString = File.ReadAllText(resourceFilename);
                var bom = Xml.Deserializer.Deserialize(inputBomString);

                var bomCsv = CsvSerializer.Serialize(bom);

                Snapshot.Match(bomCsv, SnapshotNameExtension.Create(filename));
            }
        }

        [Theory]
        [InlineData("bom")]
        [InlineData("bom-minimum-viable")]
        [InlineData("bom-lowercase-field-names")]
        [InlineData("valid-component-hashes")]
        [InlineData("valid-component-swid")]
        [InlineData("valid-component-swid-full")]
        [InlineData("valid-component-types")]
        [InlineData("valid-license-expression")]
        [InlineData("valid-license-id")]
        [InlineData("valid-license-name")]
        public void DeserializationTests(string filename)
        {
            using (var tempDirectory = new TempDirectory())
            {
                var resourceFilename = Path.Join("Resources", filename + ".csv");
                var inputBomString = File.ReadAllText(resourceFilename);

                var bom = CsvSerializer.Deserialize(inputBomString);

                var bomXml = Xml.Serializer.Serialize(bom);

                Snapshot.Match(bomXml, SnapshotNameExtension.Create(filename));
            }
        }
    }
}

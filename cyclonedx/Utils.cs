using System;
using System.IO;
using CycloneDX.Models;
using CycloneDX.Json;
using CycloneDX.Xml;

namespace CycloneDX.CLI
{
    static class Utils
    {
        public static FileFormat FileFormatFromFilename(string filename)
        {
            var fileExtension = Path.GetExtension(filename);
            if (fileExtension == ".json")
            {
                return FileFormat.Json;
            }
            else if (fileExtension == ".xml")
            {
                return FileFormat.Xml;
            }
            else
            {
                return FileFormat.Unsupported;
            }
        }

        public static Bom BomDeserializer(string bom, FileFormat format)
        {
            if (format == FileFormat.Json)
            {
                return JsonBomDeserializer.Deserialize(bom);
            }
            else if (format == FileFormat.Xml)
            {
                return XmlBomDeserializer.Deserialize(bom);
            }
            throw new UnsupportedFormatException("Unsupported SBOM file format");
        }

        public static string BomSerializer(Bom bom, FileFormat format)
        {
            if (format == FileFormat.Json)
            {
                return JsonBomSerializer.Serialize(bom);
            }
            else if (format == FileFormat.Xml)
            {
                return XmlBomSerializer.Serialize(bom);
            }
            throw new UnsupportedFormatException("Unsupported SBOM file format");
        }
    }
}
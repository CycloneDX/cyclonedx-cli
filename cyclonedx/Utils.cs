using System;
using System.IO;
using System.Text;
using CycloneDX.Models;
using CycloneDX.Json;
using CycloneDX.Xml;

namespace CycloneDX.CLI
{
    static class Utils
    {
        public static BomFormat DetectFileFormat(string filename)
        {
            var fileExtension = Path.GetExtension(filename);
            if (fileExtension == ".json")
            {
                return BomFormat.Json;
            }
            else if (fileExtension == ".xml")
            {
                return BomFormat.Xml;
            }
            else
            {
                return BomFormat.Unsupported;
            }
        }

        public static Bom BomDeserializer(string bom, BomFormat format)
        {
            if (format == BomFormat.Json)
            {
                return JsonBomDeserializer.Deserialize(bom);
            }
            else if (format == BomFormat.Xml)
            {
                return XmlBomDeserializer.Deserialize(bom);
            }
            throw new UnsupportedFormatException("Unsupported SBOM file format");
        }

        public static string BomSerializer(Bom bom, BomFormat format)
        {
            if (format == BomFormat.Json)
            {
                return JsonBomSerializer.Serialize(bom);
            }
            else if (format == BomFormat.Xml)
            {
                return XmlBomSerializer.Serialize(bom);
            }
            else if (format == BomFormat.SpdxTag)
            {
                return SpdxTagSerializer.Serialize(bom);
            }
            throw new UnsupportedFormatException("Unsupported SBOM file format");
        }
    }
}
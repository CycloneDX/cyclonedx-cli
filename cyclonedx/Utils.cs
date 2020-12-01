using System;
using System.IO;
using CycloneDX.Json;
using CycloneDX.Xml;
using CycloneDX.CLI.Models;

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
            else if (fileExtension == ".spdx")
            {
                return BomFormat.SpdxTag;
            }
            else
            {
                return BomFormat.Unsupported;
            }
        }

        public static CycloneDX.Models.v1_2.Bom BomDeserializer(string bom, BomFormat format)
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

        public static string BomSerializer(CycloneDX.Models.v1_2.Bom bom, BomFormat format)
        {
            if (format == BomFormat.Json || format == BomFormat.Json_v1_2)
            {
                return JsonBomSerializer.Serialize(bom);
            }
            else if (format == BomFormat.Xml || format == BomFormat.Xml_v1_2)
            {
                return XmlBomSerializer.Serialize(bom);
            }
            else if (format == BomFormat.Xml_v1_1)
            {
                return XmlBomSerializer.Serialize(new CycloneDX.Models.v1_1.Bom(bom));
            }
            else if (format == BomFormat.Xml_v1_0)
            {
                var v1_1_bom = new CycloneDX.Models.v1_1.Bom(bom);
                return XmlBomSerializer.Serialize(new CycloneDX.Models.v1_0.Bom(v1_1_bom));
            }
            else if (format == BomFormat.SpdxTag || format == BomFormat.SpdxTag_v2_2)
            {
                return SpdxTagSerializer.Serialize(bom, SpdxVersion.v2_2);
            }
            else if (format == BomFormat.SpdxTag_v2_1)
            {
                return SpdxTagSerializer.Serialize(bom, SpdxVersion.v2_1);
            }
            throw new UnsupportedFormatException("Unsupported SBOM file format");
        }
    }
}
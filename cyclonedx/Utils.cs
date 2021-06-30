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
// Copyright (c) Patrick Dwyer. All Rights Reserved.
using System;
using System.IO;
using System.Text;
using CycloneDX.Models.v1_3;

namespace CycloneDX.CLI
{
    public static class CLIUtils
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
            else if (fileExtension == ".cdx" || fileExtension == ".bin")
            {
                return BomFormat.Protobuf;
            }
            else if (fileExtension == ".spdx")
            {
                return BomFormat.SpdxTag;
            }
            else if (fileExtension == ".csv")
            {
                return BomFormat.Csv;
            }
            else
            {
                return BomFormat.Unsupported;
            }
        }

        public static CycloneDX.Models.v1_3.Bom BomDeserializer(Stream bomStream, BomFormat format)
        {
            if (format == BomFormat.Json || format == BomFormat.Csv)
            {
                var ms = new MemoryStream();
                bomStream.CopyTo(ms);
                var bomString = System.Text.Encoding.UTF8.GetString(ms.ToArray());
                if (format == BomFormat.Csv)
                    return CsvSerializer.Deserialize(bomString);
                else
                    return Json.Deserializer.Deserialize(bomString);
            }
            else if (format == BomFormat.Xml)
            {
                return Xml.Deserializer.Deserialize(bomStream);
            }
            else if (format == BomFormat.Protobuf)
            {
                return Protobuf.Deserializer.Deserialize(bomStream);
            }
            throw new UnsupportedFormatException("Unsupported BOM file format");
        }

        public static byte[] BomSerializer(Bom bom, BomFormat format)
        {
            if (format == BomFormat.Protobuf || format == BomFormat.Protobuf_v1_3)
            {
                return Protobuf.Serializer.Serialize(bom);
            }
            else if (format == BomFormat.Json || format == BomFormat.Json_v1_3)
            {
                return Encoding.UTF8.GetBytes(Json.Serializer.Serialize(bom));
            }
            else if (format == BomFormat.Json_v1_2)
            {
                return Encoding.UTF8.GetBytes(Json.Serializer.Serialize(new CycloneDX.Models.v1_2.Bom(bom)));
            }
            else if (format == BomFormat.Xml || format == BomFormat.Xml_v1_3)
            {
                return Encoding.UTF8.GetBytes(Xml.Serializer.Serialize(bom));
            }
            else if (format == BomFormat.Xml_v1_2)
            {
                return Encoding.UTF8.GetBytes(Xml.Serializer.Serialize(new CycloneDX.Models.v1_2.Bom(bom)));
            }
            else if (format == BomFormat.Xml_v1_1)
            {
                var v1_2_bom = new CycloneDX.Models.v1_2.Bom(bom);
                return Encoding.UTF8.GetBytes(Xml.Serializer.Serialize(new CycloneDX.Models.v1_1.Bom(v1_2_bom)));
            }
            else if (format == BomFormat.Xml_v1_0)
            {
                var v1_2_bom = new CycloneDX.Models.v1_2.Bom(bom);
                var v1_1_bom = new CycloneDX.Models.v1_1.Bom(v1_2_bom);
                return Encoding.UTF8.GetBytes(Xml.Serializer.Serialize(new CycloneDX.Models.v1_0.Bom(v1_1_bom)));
            }
            else if (format == BomFormat.SpdxTag || format == BomFormat.SpdxTag_v2_2)
            {
                return Encoding.UTF8.GetBytes(SpdxTagSerializer.Serialize(bom, SpdxVersion.v2_2));
            }
            else if (format == BomFormat.SpdxTag_v2_1)
            {
                return Encoding.UTF8.GetBytes(SpdxTagSerializer.Serialize(bom, SpdxVersion.v2_1));
            }
            else if (format == BomFormat.Csv)
            {
                return Encoding.UTF8.GetBytes(CsvSerializer.Serialize(bom));
            }
            throw new UnsupportedFormatException("Unsupported BOM file format");
        }

        public static string NullIfWhiteSpace(this string str) {
            return String.IsNullOrWhiteSpace(str) ? null : str;
        }
    }
}
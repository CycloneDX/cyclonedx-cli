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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CycloneDX.Models.v1_3;
using CycloneDX.Cli.Commands;
using CycloneDX.Cli.Serialization;

namespace CycloneDX.Cli
{
    internal static class CliUtils
    {
        public static BomFormat AutoDetectBomFormat(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return BomFormat.autodetect;
            
            var fileExtension = Path.GetExtension(filename);
            if (fileExtension == ".json")
            {
                return BomFormat.json;
            }
            else if (fileExtension == ".xml")
            {
                return BomFormat.xml;
            }
            else if (fileExtension == ".cdx" || fileExtension == ".bin")
            {
                return BomFormat.protobuf;
            }

            return BomFormat.autodetect;
        }

        public static ConvertInputFormat AutoDetectConvertCommandInputBomFormat(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return ConvertInputFormat.autodetect;
            
            var fileExtension = Path.GetExtension(filename);
            if (fileExtension == ".csv")
            {
                return ConvertInputFormat.csv;
            }
            else
            {
                return (ConvertInputFormat) AutoDetectBomFormat(filename);
            }
        }

        public static ConvertOutputFormat AutoDetectConvertCommandOutputBomFormat(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return ConvertOutputFormat.autodetect;
            
            var fileExtension = Path.GetExtension(filename);
            if (fileExtension == ".spdx")
            {
                return ConvertOutputFormat.spdxtag;
            }
            else
            {
                return (ConvertOutputFormat) AutoDetectConvertCommandInputBomFormat(filename);
            }
        }

        public static async Task<Bom> InputBomHelper(string filename, BomFormat format)
        {
            if (filename == null && format == BomFormat.autodetect)
            {
                await Console.Error.WriteLineAsync("Unable to auto-detect input stream format, please specify a value for --input-format");
                return null;
            }
            else if (format == BomFormat.autodetect)
            {
                format = AutoDetectBomFormat(filename);
                if (format == BomFormat.autodetect)
                {
                    await Console.Error.WriteLineAsync("Unable to auto-detect file format, please specify a value for --input-format");
                    return null;
                }
            }

            using var inputStream = filename == null ? Console.OpenStandardInput() : File.OpenRead(filename);
            
            if (format == BomFormat.json)
            {
                return await Json.Deserializer.DeserializeAsync(inputStream).ConfigureAwait(false);
            }
            else if (format == BomFormat.xml)
            {
                return Xml.Deserializer.Deserialize(inputStream);
            }
            else if (format == BomFormat.protobuf)
            {
                return Protobuf.Deserializer.Deserialize(inputStream);
            }

            return null;
        }
        
        public static async Task<Bom> InputBomHelper(string filename, ConvertInputFormat format)
        {
            if (filename == null && format == ConvertInputFormat.autodetect)
            {
                await Console.Error.WriteLineAsync("Unable to auto-detect input stream format, please specify a value for --input-format");
                return null;
            }
            else if (format == ConvertInputFormat.autodetect)
            {
                format = AutoDetectConvertCommandInputBomFormat(filename);
                if (format == ConvertInputFormat.autodetect)
                {
                    await Console.Error.WriteLineAsync("Unable to auto-detect file format, please specify a value for --input-format");
                    return null;
                }
            }

            
            if (format == ConvertInputFormat.csv)
            {
                using var inputStream = filename == null ? Console.OpenStandardInput() : File.OpenRead(filename);
                using var ms = new MemoryStream();
                await inputStream.CopyToAsync(ms);
                var bomCsv = Encoding.UTF8.GetString(ms.ToArray());
                return CsvSerializer.Deserialize(bomCsv);
            }
            else
            {
                return await InputBomHelper(filename, (BomFormat) format).ConfigureAwait(false);
            }
        }
        
        public static async Task<int> OutputBomHelper(Bom bom, BomFormat format, string filename)
        {
            if (filename == null && format == BomFormat.autodetect)
            {
                await Console.Error.WriteLineAsync("Unable to auto-detect output stream format, please specify a value for --output-format");
                return (int) ExitCode.ParameterValidationError;
            }
            else if (format == BomFormat.autodetect)
            {
                var detectedFormat = AutoDetectBomFormat(filename);
                if (detectedFormat == BomFormat.autodetect)
                {
                    await Console.Error.WriteLineAsync("Unable to auto-detect file format, please specify a value for --input-format");
                    return (int) ExitCode.ParameterValidationError;
                }
            }

            using var stream = filename == null ? Console.OpenStandardOutput() : File.OpenWrite(filename);

            if (format == BomFormat.protobuf)
            {
                Protobuf.Serializer.Serialize(bom, stream);
            }
            else if (format == BomFormat.json)
            {
                await Json.Serializer.SerializeAsync(bom, stream).ConfigureAwait(false);
            }
            else if (format == BomFormat.xml)
            {
                Xml.Serializer.Serialize(bom, stream);
            }
            
            return 0;
        }

        public static async Task<int> OutputBomHelper(Bom bom, ConvertOutputFormat format, string filename)
        {
            if (format == ConvertOutputFormat.autodetect
                || format == ConvertOutputFormat.json
                || format == ConvertOutputFormat.protobuf
                || format == ConvertOutputFormat.xml
            )
            {
                return await OutputBomHelper(bom, (BomFormat) format, filename).ConfigureAwait(false);
            }
            else
            {
                using var stream = filename == null ? Console.OpenStandardOutput() : File.OpenWrite(filename);

                if (format == ConvertOutputFormat.protobuf_v1_3)
                {
                    Protobuf.Serializer.Serialize(bom, stream);
                }
                else if (format == ConvertOutputFormat.json_v1_3)
                {
                    await Json.Serializer.SerializeAsync(bom, stream).ConfigureAwait(false);
                }
                else if (format == ConvertOutputFormat.json_v1_2)
                {
                    await Json.Serializer.SerializeAsync(new CycloneDX.Models.v1_2.Bom(bom), stream).ConfigureAwait(false);
                }
                else if (format == ConvertOutputFormat.xml_v1_3)
                {
                    Xml.Serializer.Serialize(bom, stream);
                }
                else if (format == ConvertOutputFormat.xml_v1_2)
                {
                    Xml.Serializer.Serialize(new CycloneDX.Models.v1_2.Bom(bom), stream);
                }
                else if (format == ConvertOutputFormat.xml_v1_1)
                {
                    var v1_2_bom = new CycloneDX.Models.v1_2.Bom(bom);
                    Xml.Serializer.Serialize(new CycloneDX.Models.v1_1.Bom(v1_2_bom), stream);
                }
                else if (format == ConvertOutputFormat.xml_v1_0)
                {
                    var v1_2_bom = new CycloneDX.Models.v1_2.Bom(bom);
                    var v1_1_bom = new CycloneDX.Models.v1_1.Bom(v1_2_bom);
                    Xml.Serializer.Serialize(new CycloneDX.Models.v1_0.Bom(v1_1_bom), stream);
                }
                else if (format == ConvertOutputFormat.spdxtag || format == ConvertOutputFormat.spdxtag_v2_2)
                {
                    var serializer = new SpdxTagSerializer(bom, SpdxVersion.v2_2);
                    var bomString = serializer.Serialize();
                    var bomBytes = Encoding.UTF8.GetBytes(bomString);
                    stream.Write(bomBytes);
                }
                else if (format == ConvertOutputFormat.spdxtag_v2_1)
                {
                    var serializer = new SpdxTagSerializer(bom, SpdxVersion.v2_1);
                    var bomString = serializer.Serialize();
                    var bomBytes = Encoding.UTF8.GetBytes(bomString);
                    stream.Write(bomBytes);
                }
                else if (format == ConvertOutputFormat.csv)
                {
                    var bomString = CsvSerializer.Serialize(bom);
                    var bomBytes = Encoding.UTF8.GetBytes(bomString);
                    stream.Write(bomBytes);
                }
                else
                {
                    Console.WriteLine($"Unimplemented output format {format}");
                    return (int) ExitCode.UnsupportedFormat;
                }

                return 0;
            }
        }

        public static string NullIfWhiteSpace(this string str) {
            return String.IsNullOrWhiteSpace(str) ? null : str;
        }
    }
}
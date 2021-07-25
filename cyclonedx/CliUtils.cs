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
using CycloneDX.Models.v1_3;

namespace CycloneDX.Cli
{
    internal static class CliUtils
    {
        public static StandardInputOutputBomFormat AutoDetectBomFormat(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return StandardInputOutputBomFormat.autodetect;
            
            var fileExtension = Path.GetExtension(filename);
            if (fileExtension == ".json")
            {
                return StandardInputOutputBomFormat.json;
            }
            else if (fileExtension == ".xml")
            {
                return StandardInputOutputBomFormat.xml;
            }
            else if (fileExtension == ".cdx" || fileExtension == ".bin")
            {
                return StandardInputOutputBomFormat.protobuf;
            }

            return StandardInputOutputBomFormat.autodetect;
        }

        public static ConvertCommand.InputFormat AutoDetectConvertCommandInputBomFormat(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return ConvertCommand.InputFormat.autodetect;
            
            var fileExtension = Path.GetExtension(filename);
            if (fileExtension == ".csv")
            {
                return ConvertCommand.InputFormat.csv;
            }
            else
            {
                return (ConvertCommand.InputFormat) AutoDetectBomFormat(filename);
            }
        }

        public static ConvertCommand.OutputFormat AutoDetectConvertCommandOutputBomFormat(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return ConvertCommand.OutputFormat.autodetect;
            
            var fileExtension = Path.GetExtension(filename);
            if (fileExtension == ".spdx")
            {
                return ConvertCommand.OutputFormat.spdxtag;
            }
            else
            {
                return (ConvertCommand.OutputFormat) AutoDetectConvertCommandInputBomFormat(filename);
            }
        }

        public static Bom InputBomHelper(string filename, StandardInputOutputBomFormat format)
        {
            if (filename == null && format == StandardInputOutputBomFormat.autodetect)
            {
                Console.Error.WriteLine("Unable to auto-detect input stream format, please specify a value for --input-format");
                return null;
            }
            else if (format == StandardInputOutputBomFormat.autodetect)
            {
                format = AutoDetectBomFormat(filename);
                if (format == StandardInputOutputBomFormat.autodetect)
                {
                    Console.Error.WriteLine("Unable to auto-detect file format, please specify a value for --input-format");
                    return null;
                }
            }

            using var inputStream = filename == null ? Console.OpenStandardInput() : File.OpenRead(filename);
            
            if (format == StandardInputOutputBomFormat.json)
            {
                return Json.Deserializer.Deserialize(inputStream);
            }
            else if (format == StandardInputOutputBomFormat.xml)
            {
                return Xml.Deserializer.Deserialize(inputStream);
            }
            else if (format == StandardInputOutputBomFormat.protobuf)
            {
                return Protobuf.Deserializer.Deserialize(inputStream);
            }

            return null;
        }
        
        public static Bom InputBomHelper(string filename, ConvertCommand.InputFormat format)
        {
            if (filename == null && format == ConvertCommand.InputFormat.autodetect)
            {
                Console.Error.WriteLine("Unable to auto-detect input stream format, please specify a value for --input-format");
                return null;
            }
            else if (format == ConvertCommand.InputFormat.autodetect)
            {
                format = AutoDetectConvertCommandInputBomFormat(filename);
                if (format == ConvertCommand.InputFormat.autodetect)
                {
                    Console.Error.WriteLine("Unable to auto-detect file format, please specify a value for --input-format");
                    return null;
                }
            }

            
            if (format == ConvertCommand.InputFormat.csv)
            {
                using var inputStream = filename == null ? Console.OpenStandardInput() : File.OpenRead(filename);
                using var ms = new MemoryStream();
                inputStream.CopyTo(ms);
                var bomCsv = Encoding.UTF8.GetString(ms.ToArray());
                return CsvSerializer.Deserialize(bomCsv);
            }
            else
            {
                return InputBomHelper(filename, (StandardInputOutputBomFormat) format);
            }
        }
        
        public static int OutputBomHelper(Bom bom, StandardInputOutputBomFormat format, string filename)
        {
            if (filename == null && format == StandardInputOutputBomFormat.autodetect)
            {
                Console.Error.WriteLine("Unable to auto-detect input stream format, please specify a value for --input-format");
                return (int) ExitCode.ParameterValidationError;
            }
            else if (format == StandardInputOutputBomFormat.autodetect)
            {
                var detectedFormat = AutoDetectBomFormat(filename);
                if (detectedFormat == StandardInputOutputBomFormat.autodetect)
                {
                    Console.Error.WriteLine("Unable to auto-detect file format, please specify a value for --input-format");
                    return (int) ExitCode.ParameterValidationError;
                }
            }

            using var stream = filename == null ? Console.OpenStandardOutput() : File.OpenWrite(filename);

            if (format == StandardInputOutputBomFormat.protobuf)
            {
                Protobuf.Serializer.Serialize(bom, stream);
            }
            else if (format == StandardInputOutputBomFormat.json)
            {
                Json.Serializer.Serialize(bom, stream);
            }
            else if (format == StandardInputOutputBomFormat.xml)
            {
                Xml.Serializer.Serialize(bom, stream);
            }
            
            if (filename == null) stream.SetLength(stream.Position);
            
            return 0;
        }

        public static int OutputBomHelper(Bom bom, ConvertCommand.OutputFormat format, string filename)
        {
            if (format == ConvertCommand.OutputFormat.autodetect
                || format == ConvertCommand.OutputFormat.json
                || format == ConvertCommand.OutputFormat.protobuf
                || format == ConvertCommand.OutputFormat.xml
            )
            {
                return OutputBomHelper(bom, (StandardInputOutputBomFormat) format, filename);
            }
            else
            {
                using var stream = filename == null ? Console.OpenStandardOutput() : File.OpenWrite(filename);

                if (format == ConvertCommand.OutputFormat.protobuf_v1_3)
                {
                    Protobuf.Serializer.Serialize(bom, stream);
                }
                else if (format == ConvertCommand.OutputFormat.json_v1_3)
                {
                    Json.Serializer.Serialize(bom, stream);
                }
                else if (format == ConvertCommand.OutputFormat.json_v1_2)
                {
                    Json.Serializer.Serialize(new CycloneDX.Models.v1_2.Bom(bom), stream);
                }
                else if (format == ConvertCommand.OutputFormat.xml_v1_3)
                {
                    Xml.Serializer.Serialize(bom, stream);
                }
                else if (format == ConvertCommand.OutputFormat.xml_v1_2)
                {
                    Xml.Serializer.Serialize(new CycloneDX.Models.v1_2.Bom(bom), stream);
                }
                else if (format == ConvertCommand.OutputFormat.xml_v1_1)
                {
                    var v1_2_bom = new CycloneDX.Models.v1_2.Bom(bom);
                    Xml.Serializer.Serialize(new CycloneDX.Models.v1_1.Bom(v1_2_bom), stream);
                }
                else if (format == ConvertCommand.OutputFormat.xml_v1_0)
                {
                    var v1_2_bom = new CycloneDX.Models.v1_2.Bom(bom);
                    var v1_1_bom = new CycloneDX.Models.v1_1.Bom(v1_2_bom);
                    Xml.Serializer.Serialize(new CycloneDX.Models.v1_0.Bom(v1_1_bom), stream);
                }
                else if (format == ConvertCommand.OutputFormat.spdxtag || format == ConvertCommand.OutputFormat.spdxtag_v2_2)
                {
                    var bomString = SpdxTagSerializer.Serialize(bom, SpdxVersion.v2_2);
                    var bomBytes = Encoding.UTF8.GetBytes(bomString);
                    stream.Write(bomBytes);
                }
                else if (format == ConvertCommand.OutputFormat.spdxtag_v2_1)
                {
                    var bomString = SpdxTagSerializer.Serialize(bom, SpdxVersion.v2_1);
                    var bomBytes = Encoding.UTF8.GetBytes(bomString);
                    stream.Write(bomBytes);
                }
                else if (format == ConvertCommand.OutputFormat.csv)
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
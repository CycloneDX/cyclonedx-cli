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
using CycloneDX.Models;
using CycloneDX.Spdx.Interop;
using CycloneDX.Cli.Commands;
using CycloneDX.Cli.Serialization;

namespace CycloneDX.Cli
{
    internal static class CliUtils
    {
        public static CycloneDXBomFormat AutoDetectBomFormat(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return CycloneDXBomFormat.autodetect;
            
            var fileExtension = Path.GetExtension(filename);
            if (fileExtension == ".json")
            {
                return CycloneDXBomFormat.json;
            }
            else if (fileExtension == ".xml")
            {
                return CycloneDXBomFormat.xml;
            }
            else if (fileExtension == ".cdx" || fileExtension == ".bin")
            {
                return CycloneDXBomFormat.protobuf;
            }

            return CycloneDXBomFormat.autodetect;
        }

        public static ConvertFormat AutoDetectConvertBomFormat(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return ConvertFormat.autodetect;
            
            var fileExtension = Path.GetExtension(filename);
            if (fileExtension == ".csv")
            {
                return ConvertFormat.csv;
            }
            else if (filename.ToLowerInvariant().EndsWith(".spdx.json", StringComparison.InvariantCulture))
            {
                return ConvertFormat.spdxjson;
            }
            else
            {
                return (ConvertFormat) AutoDetectBomFormat(filename);
            }
        }

        public static async Task<Bom> InputBomHelper(string filename, CycloneDXBomFormat format)
        {
            if (filename == null && format == CycloneDXBomFormat.autodetect)
            {
                await Console.Error.WriteLineAsync("Unable to auto-detect input stream format, please specify a value for --input-format").ConfigureAwait(false);
                return null;
            }
            else if (format == CycloneDXBomFormat.autodetect)
            {
                format = AutoDetectBomFormat(filename);
                if (format == CycloneDXBomFormat.autodetect)
                {
                    await Console.Error.WriteLineAsync("Unable to auto-detect file format, please specify a value for --input-format").ConfigureAwait(false);
                    return null;
                }
            }

            using var inputStream = filename == null ? Console.OpenStandardInput() : File.OpenRead(filename);
            
            switch (format)
            {
                case CycloneDXBomFormat.xml:
                    return Xml.Serializer.Deserialize(inputStream);
                case CycloneDXBomFormat.json:
                    return await Json.Serializer.DeserializeAsync(inputStream).ConfigureAwait(false);
                case CycloneDXBomFormat.protobuf:
                    return Protobuf.Serializer.Deserialize(inputStream);
                default:
                    return null;
            }
        }
        
        public static async Task<Bom> InputBomHelper(string filename, ConvertFormat format)
        {
            if (filename == null && format == ConvertFormat.autodetect)
            {
                await Console.Error.WriteLineAsync("Unable to auto-detect input stream format, please specify a value for --input-format").ConfigureAwait(false);
                return null;
            }
            else if (format == ConvertFormat.autodetect)
            {
                format = AutoDetectConvertBomFormat(filename);
                if (format == ConvertFormat.autodetect)
                {
                    await Console.Error.WriteLineAsync("Unable to auto-detect file format, please specify a value for --input-format").ConfigureAwait(false);
                    return null;
                }
            }

            
            if (format == ConvertFormat.csv)
            {
                using var inputStream = filename == null ? Console.OpenStandardInput() : File.OpenRead(filename);
                using var ms = new MemoryStream();
                await inputStream.CopyToAsync(ms).ConfigureAwait(false);
                var bomCsv = Encoding.UTF8.GetString(ms.ToArray());
                return CsvSerializer.Deserialize(bomCsv);
            }
            else if (format == ConvertFormat.spdxjson)
            {
                using var inputStream = filename == null ? Console.OpenStandardInput() : File.OpenRead(filename);
                var spdxDoc = await CycloneDX.Spdx.Serialization.JsonSerializer.DeserializeAsync(inputStream);
                return spdxDoc.ToCycloneDX();
            }
            else
            {
                return await InputBomHelper(filename, (CycloneDXBomFormat)format).ConfigureAwait(false);
            }
        }
        
        public static async Task<int> OutputBomHelper(Bom bom, CycloneDXBomFormat format, string filename)
        {
            if (filename == null && format == CycloneDXBomFormat.autodetect)
            {
                await Console.Error.WriteLineAsync("Unable to auto-detect output stream format, please specify a value for --output-format").ConfigureAwait(false);
                return (int) ExitCode.ParameterValidationError;
            }
            else if (format == CycloneDXBomFormat.autodetect)
            {
                var detectedFormat = AutoDetectBomFormat(filename);
                if (detectedFormat == CycloneDXBomFormat.autodetect)
                {
                    await Console.Error.WriteLineAsync("Unable to auto-detect file format, please specify a value for --input-format").ConfigureAwait(false);
                    return (int) ExitCode.ParameterValidationError;
                }
            }

            using var stream = filename == null ? Console.OpenStandardOutput() : File.Create(filename);

            switch (format)
            {
                case CycloneDXBomFormat.xml:
                    Xml.Serializer.Serialize(bom, stream);
                    break;
                case CycloneDXBomFormat.json:
                    await Json.Serializer.SerializeAsync(bom, stream).ConfigureAwait(false);
                    break;
                case CycloneDXBomFormat.protobuf:
                    Protobuf.Serializer.Serialize(bom, stream);
                    break;
                default:
                    return (int)ExitCode.ParameterValidationError;
            }

            return 0;
        }

        public static async Task<int> OutputBomHelper(Bom bom, ConvertFormat format, SpecificationVersion? outputVersion, string filename)
        {
            if (filename == null && format == ConvertFormat.autodetect)
            {
                await Console.Error.WriteLineAsync("Unable to auto-detect output stream format, please specify a value for --output-format").ConfigureAwait(false);
                return (int) ExitCode.ParameterValidationError;
            }
            else if (format == ConvertFormat.autodetect)
            {
                format = AutoDetectConvertBomFormat(filename);
                if (format == ConvertFormat.autodetect)
                {
                    await Console.Error.WriteLineAsync("Unable to auto-detect file format, please specify a value for --input-format").ConfigureAwait(false);
                    return (int) ExitCode.ParameterValidationError;
                }
            }

            bom.SpecVersion = outputVersion.HasValue ? outputVersion.Value : SpecificationVersionHelpers.CurrentVersion;

            using var stream = filename == null ? Console.OpenStandardOutput() : File.Create(filename);

            switch (format)
            {
                case ConvertFormat.xml:
                    Xml.Serializer.Serialize(bom, stream);
                    break;
                case ConvertFormat.json:
                    await Json.Serializer.SerializeAsync(bom, stream).ConfigureAwait(false);
                    break;
                case ConvertFormat.protobuf:
                    Protobuf.Serializer.Serialize(bom, stream);
                    break;
                case ConvertFormat.csv:
                    var bomString = CsvSerializer.Serialize(bom);
                    var bomBytes = Encoding.UTF8.GetBytes(bomString);
                    stream.Write(bomBytes);
                    break;
                case ConvertFormat.spdxjson:
                    var spdxDoc = bom.ToSpdx();
                    await CycloneDX.Spdx.Serialization.JsonSerializer.SerializeAsync(spdxDoc, stream);
                    break;
                default:
                    Console.WriteLine($"Unimplemented output format {format}");
                    return (int) ExitCode.UnsupportedFormat;
            }
            return 0;
        }

        public static string NullIfWhiteSpace(this string str) {
            return String.IsNullOrWhiteSpace(str) ? null : str;
        }
    }
}
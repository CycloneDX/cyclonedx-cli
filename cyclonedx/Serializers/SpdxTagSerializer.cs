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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CycloneDX.Models.v1_3;

namespace CycloneDX.Cli.Serializers
{
    public enum SpdxVersion
    {
        v2_1,
        v2_2
    }

    public class SpdxSerializationException : Exception
    {
        public SpdxSerializationException() {}
        public SpdxSerializationException(string message) : base(message) {}
        public SpdxSerializationException(string message, Exception innerException) : base(message, innerException) {}
    }

    public class SpdxTagSerializer
    {
        private Bom OriginalBom;
        private SpdxVersion Version;
        private List<License> NonSpdxLicenses;
        private StringBuilder Sb;

        public SpdxTagSerializer(Bom bom, SpdxVersion version)
        {
            OriginalBom = bom;
            Version = version;
        }

        public string Serialize()
        {
            NonSpdxLicenses = new List<License>();
            Sb = new StringBuilder();
            
            string bomSpdxRef;
            if (string.IsNullOrEmpty(OriginalBom.SerialNumber))
            {
                bomSpdxRef = System.Guid.NewGuid().ToString();
            }
            else if (OriginalBom.SerialNumber.StartsWith("urn:uuid:", StringComparison.InvariantCulture))
            {
                bomSpdxRef = OriginalBom.SerialNumber.Remove(0, 9);
            }
            else
            {
                bomSpdxRef = OriginalBom.SerialNumber;
            }

            Sb.Append("SPDXVersion: SPDX-");
            if (Version == SpdxVersion.v2_1)
                Sb.Append("2.1");
            else if (Version == SpdxVersion.v2_2)
                Sb.Append("2.2");
            Sb.AppendLine();
            // CC0-1.0 is a requirement when using the SPDX specification
            Sb.AppendLine("DataLicense: CC0-1.0");
            Sb.AppendLine($"SPDXID: SPDXRef-DOCUMENT");
            
            // SPDX: doesn't support majority of BOM metadata component information
            var documentRef = "Generated from CycloneDX BOM without metadata component specified";
            if (OriginalBom.Metadata?.Component?.Name != null)
            {
                documentRef = OriginalBom.Metadata.Component.Name;
                if (OriginalBom.Metadata.Component.Version != null)
                    documentRef += $"-{OriginalBom.Metadata.Component.Version}";
                if (OriginalBom.Metadata.Component.Group != null)
                    documentRef = $"{OriginalBom.Metadata.Component.Group}.{documentRef}";
            }
            Sb.AppendLine($"DocumentName: {documentRef}");
            Sb.AppendLine($"DocumentNamespace: http://spdx.org/spdxdocs/{documentRef}-{bomSpdxRef}");

            Sb.AppendLine("LicenseListVersion: 3.14");

            // SPDX: doesn't support author phone
            if (OriginalBom.Metadata?.Authors != null)
            foreach (var author in OriginalBom.Metadata.Authors)
            {
                Sb.AppendLine($"Creator: Person: {author.Name} ({author.Email ?? ""})");
            }

            // SPDX: does not support tool vendor and hashes
            if (OriginalBom.Metadata?.Tools != null)
            foreach (var tool in OriginalBom.Metadata.Tools)
            {
                Sb.AppendLine($"Tool: {tool.Name}-{tool.Version}");
            }
            Sb.AppendLine("Creator: Tool: CycloneDX-CLI");

            Sb.AppendLine($"Created: {DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")}");
            Sb.AppendLine("CreatorComment: <text>This SPDX document was created by conversion from a CycloneDX BOM.</text>");
            Sb.AppendLine("DocumentComment: <text>SPDX does not support all features of CycloneDX. Some information is missing from the original CycloneDX BOM.</text>");

            if (OriginalBom.Components != null)
            for (var componentIndex=0; componentIndex<OriginalBom.Components.Count; componentIndex++)
            {
                var component = OriginalBom.Components[componentIndex];
                if (component.Type == Component.Classification.File)
                {
                    WriteFileInformation(component, componentIndex);
                }
                else
                {
                    WritePackageInformation(component, componentIndex);
                }
            }

            for (var licenseIndex=0; licenseIndex<NonSpdxLicenses.Count; licenseIndex++)
            {
                Sb.AppendLine();
                var license = NonSpdxLicenses[licenseIndex];
                Sb.AppendLine($"LicenseID: LicenseRef-{licenseIndex+1}");
                if (license.Text != null && !string.IsNullOrEmpty(license.Text.Content))
                    Sb.AppendLine($"ExtractedText: <text>\"{license.Name}\": {WebUtility.HtmlEncode(license.Text.Content)}</text>");
                Sb.AppendLine($"LicenseName: {license.Name}");
                if (!string.IsNullOrEmpty(license.Url))
                    Sb.AppendLine($"LicenseCrossReference: {license.Url}");
            }

            return Sb.ToString();
        }

        private void WritePackageInformation(Component component, int componentIndex)
        {
            // if (component.Type == Classification.File) throw new Exception("Unsupported component type for this method.");

            string componentSpdxRef;
            if (string.IsNullOrEmpty(component.BomRef))
            {
                componentSpdxRef = (componentIndex + 1).ToString();
            }
            else
            {
                componentSpdxRef = SpdxIdString(component.BomRef);
            }

            Sb.AppendLine();
            Sb.Append("PackageName:");
            if (component.Group != null) Sb.Append($" {component.Group}");
            Sb.AppendLine($" {component.Name}");
            Sb.AppendLine($"SPDXID: SPDXRef-{componentSpdxRef}");
            Sb.AppendLine($"PackageVersion: {component.Version}");

            if (component.Supplier != null) Sb.AppendLine($"PackageSupplier: Organization: {component.Supplier.Name} ()");

            if (component.Author != null)
                Sb.AppendLine($"PackageOriginator: Person: {component.Author} ()");
            else if (component.Publisher != null)
                Sb.AppendLine($"PackageOriginator: Person: {component.Publisher} ()");

            Sb.AppendLine("PackageDownloadLocation: NOASSERTION");
            Sb.AppendLine("FilesAnalyzed: false");

            // SPDX: Does not support all hash algorithms
            if (component.Hashes != null)
            foreach(var hash in component.Hashes)
            {
                string algStr = null;

                switch (hash.Alg)
                {
                    case Hash.HashAlgorithm.MD5:
                        algStr = "MD5";
                        break;
                    case Hash.HashAlgorithm.SHA_1:
                        algStr = "SHA1";
                        break;
                    case Hash.HashAlgorithm.SHA_256:
                        algStr = "SHA256";
                        break;
                }
                if (Version == SpdxVersion.v2_2)
                switch (hash.Alg)
                {
                    case Hash.HashAlgorithm.SHA_384:
                        algStr = "SHA384";
                        break;
                    case Hash.HashAlgorithm.SHA_512:
                        algStr = "SHA512";
                        break;
                }

                if (algStr != null)
                {
                    Sb.AppendLine($"PackageChecksum: {algStr}: {hash.Content}");
                }
            }

            var homepage = component.ExternalReferences?.FirstOrDefault(er => er.Type == ExternalReference.ExternalReferenceType.Website);
            if (homepage != null) Sb.AppendLine($"PackageHomePage: {homepage.Url}");

            Sb.AppendLine("PackageLicenseConcluded: NOASSERTION");

            if (component.Licenses?.Count > 0)
            {
                Sb.Append("PackageLicenseDeclared: ");
                if (component.Licenses.Count > 1) Sb.Append('(');
                for (var licenseIndex=0; licenseIndex<component.Licenses.Count; licenseIndex++)
                {
                    var componentLicense = component.Licenses[licenseIndex];
                    if (licenseIndex != 0) Sb.Append(" AND ");
                    if (componentLicense.License != null)
                    {
                        if (!string.IsNullOrEmpty(componentLicense.License.Id))
                        {
                            Sb.Append(componentLicense.License.Id);
                        }
                        else
                        {
                            NonSpdxLicenses.Add(componentLicense.License);
                            Sb.Append($"LicenseRef-{NonSpdxLicenses.Count}");
                        }
                    }
                    else
                    {
                        Sb.Append($"({componentLicense.Expression})");
                    }
                }
                if (component.Licenses.Count > 1) Sb.Append(')');
                Sb.AppendLine();
            }
            else
            {
                Sb.AppendLine("PackageLicenseDeclared: NOASSERTION");
            }

            if (string.IsNullOrEmpty(component.Copyright))
            {
                Sb.AppendLine("PackageCopyrightText: NOASSERTION");
            }
            else
            {
                Sb.AppendLine($"PackageCopyrightText: <text>{WebUtility.HtmlEncode(component.Copyright)}</text>");
            }

            if (!string.IsNullOrEmpty(component.Purl))
                Sb.AppendLine($"ExternalRef: PACKAGE-MANAGER purl {component.Purl}");
            
            if (!string.IsNullOrEmpty(component.Cpe))
            {
                if (component.Cpe.ToLowerInvariant().StartsWith("cpe:2.2:", StringComparison.InvariantCulture))
                {
                    Sb.Append("ExternalRef: SECURITY cpe22Type ");
                    Sb.AppendLine(component.Cpe);
                }
                else if (component.Cpe.ToLowerInvariant().StartsWith("cpe:2.3:", StringComparison.InvariantCulture))
                {
                    Sb.Append("ExternalRef: SECURITY cpe23Type ");
                    Sb.AppendLine(component.Cpe);
                }
            }

            // SPDX: does not support most external reference types
        }

        private void WriteFileInformation(Component component, int componentIndex)
        {
            // if (component.Type != Classification.File) throw new Exception("Unsupported component type for this method.");
            WritePackageInformation(component, componentIndex);
        }

        private static string SpdxIdString(string value)
        {
            var rgx = new Regex(@"[^a-zA-Z0-9\.\-]");
            var result = rgx.Replace(value, "-");
            return result;
        }
    }
}

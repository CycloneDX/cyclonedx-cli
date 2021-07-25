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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CycloneDX.Models.v1_3;

namespace CycloneDX.Cli
{
    public enum SpdxVersion
    {
        v2_1,
        v2_2
    }

    public class SpdxSerializationException : Exception
    {
        public SpdxSerializationException(string message) : base(message) {}
    }

    public static class SpdxTagSerializer
    {
        public static string Serialize(Bom bom, SpdxVersion version)
        {
            Contract.Requires(bom != null);
            
            var nonSpdxLicenses = new List<License>();
            string bomSpdxRef;
            if (string.IsNullOrEmpty(bom.SerialNumber))
            {
                bomSpdxRef = System.Guid.NewGuid().ToString();
            }
            else if (bom.SerialNumber.StartsWith("urn:uuid:", StringComparison.InvariantCulture))
            {
                bomSpdxRef = bom.SerialNumber.Remove(0, 9);
            }
            else
            {
                bomSpdxRef = bom.SerialNumber;
            }

            var sb = new StringBuilder();
            var componentSb = new StringBuilder();
            sb.Append("SPDXVersion: SPDX-");
            if (version == SpdxVersion.v2_1)
                sb.Append("2.1");
            else if (version == SpdxVersion.v2_2)
                sb.Append("2.2");
            sb.AppendLine();
            // CC0-1.0 is a requirement when using the SPDX specification
            sb.AppendLine("DataLicense: CC0-1.0");
            sb.AppendLine($"SPDXID: SPDXRef-DOCUMENT");
            
            var documentRef = "Generated from CycloneDX BOM without top level component metadata";
            if (bom.Metadata?.Component?.Name != null)
            {
                documentRef = bom.Metadata.Component.Name;
                if (bom.Metadata?.Component?.Version != null)
                    documentRef += $"-{bom.Metadata.Component.Version}";
            }
            sb.AppendLine($"DocumentName: {documentRef}");
            sb.AppendLine($"DocumentNamespace: http://spdx.org/spdxdocs/{documentRef}-{bomSpdxRef}");
            if (bom.Metadata?.Authors != null)
            foreach (var author in bom.Metadata.Authors)
            {
                sb.AppendLine($"Creator: Person: {author.Name} ({author.Email ?? ""})");
            }
            sb.AppendLine("Creator: Tool: CycloneDX-CLI");
            sb.AppendLine($"Created: {DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")}");

            if (bom.Components != null)
            for (var componentIndex=0; componentIndex<bom.Components.Count; componentIndex++)
            {
                var component = bom.Components[componentIndex];
                string componentSpdxRef;
                if (string.IsNullOrEmpty(component.BomRef))
                {
                    componentSpdxRef = (componentIndex + 1).ToString();
                }
                else
                {
                    componentSpdxRef = SpdxIdString(component.BomRef);
                }

                sb.AppendLine();
                sb.AppendLine($"PackageName: {component.Name}");
                sb.AppendLine($"SPDXID: SPDXRef-{componentSpdxRef}");
                sb.AppendLine($"PackageVersion: {component.Version}");

                if (component.Supplier != null)
                    sb.AppendLine($"PackageSupplier: Organization: {component.Supplier.Name} ()");
                if (component.Author != null)
                    sb.AppendLine($"PackageOriginator: Person: {component.Author} ()");
                else if (component.Publisher != null)
                    sb.AppendLine($"PackageOriginator: Person: {component.Publisher} ()");

                sb.AppendLine("PackageDownloadLocation: NOASSERTION");
                sb.AppendLine("FilesAnalyzed: false");

                if (component.Hashes != null)
                foreach(var hash in component.Hashes)
                {
                    string algStr = null;

                    switch (hash.Alg)
                    {
                        case Hash.HashAlgorithm.SHA_1:
                            algStr = "SHA1";
                            break;
                        case Hash.HashAlgorithm.SHA_256:
                            algStr = "SHA256";
                            break;
                    }
                    if (version == SpdxVersion.v2_2)
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
                        sb.AppendLine($"PackageChecksum: {algStr}: {hash.Content}");
                    }
                }

                sb.AppendLine("PackageLicenseConcluded: NOASSERTION");

                if (component.Licenses?.Count > 0)
                {
                    sb.Append("PackageLicenseDeclared: ");
                    if (component.Licenses.Count > 1) sb.Append('(');
                    for (var licenseIndex=0; licenseIndex<component.Licenses.Count; licenseIndex++)
                    {
                        var componentLicense = component.Licenses[licenseIndex];
                        if (licenseIndex != 0) sb.Append(" AND ");
                        if (componentLicense.License != null)
                        {
                            if (!string.IsNullOrEmpty(componentLicense.License.Id))
                            {
                                sb.Append(componentLicense.License.Id);
                            }
                            else
                            {
                                nonSpdxLicenses.Add(componentLicense.License);
                                sb.Append($"LicenseRef-{nonSpdxLicenses.Count}");
                            }
                        }
                        else
                        {
                            sb.Append($"({componentLicense.Expression})");
                        }
                    }
                    if (component.Licenses.Count > 1) sb.Append(')');
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("PackageLicenseDeclared: NOASSERTION");
                }

                if (string.IsNullOrEmpty(component.Copyright))
                {
                    sb.AppendLine("PackageCopyrightText: NOASSERTION");
                }
                else
                {
                    sb.AppendLine($"PackageCopyrightText: <text>{WebUtility.HtmlEncode(component.Copyright)}</text>");
                }

                if (!string.IsNullOrEmpty(component.Purl))
                    sb.AppendLine($"External-Ref: PACKAGE-MANAGER purl {component.Purl}");
                
                if (!string.IsNullOrEmpty(component.Cpe))
                {
                    if (component.Cpe.ToLowerInvariant().StartsWith("cpe:2.2:", StringComparison.InvariantCulture))
                    {
                        sb.Append("ExternalRef: SECURITY cpe22Type ");
                        sb.AppendLine(component.Cpe);
                    }
                    else if (component.Cpe.ToLowerInvariant().StartsWith("cpe:2.3:", StringComparison.InvariantCulture))
                    {
                        sb.Append("ExternalRef: SECURITY cpe23Type ");
                        sb.AppendLine(component.Cpe);
                    }
                }
            }

            for (var licenseIndex=0; licenseIndex<nonSpdxLicenses.Count; licenseIndex++)
            {
                sb.AppendLine();
                var license = nonSpdxLicenses[licenseIndex];
                sb.AppendLine($"LicenseID: LicenseRef-{licenseIndex+1}");
                if (license.Text != null && !string.IsNullOrEmpty(license.Text.Content))
                    sb.AppendLine($"ExtractedText: <text>\"{license.Name}\": {WebUtility.HtmlEncode(license.Text.Content)}</text>");
                sb.AppendLine($"LicenseName: {license.Name}");
                if (!string.IsNullOrEmpty(license.Url))
                    sb.AppendLine($"LicenseCrossReference: {license.Url}");
            }

            return sb.ToString();
        }

        private static string SpdxIdString(string value)
        {
            var rgx = new Regex(@"[^a-zA-Z0-9\.\-]");
            var result = rgx.Replace(value, "-");
            return result;
        }
    }
}

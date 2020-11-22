using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CycloneDX.CLI
{
    public static class SpdxTagSerializer
    {
        public class SpdxSerializationException : Exception
        {
            public SpdxSerializationException(string message) : base(message) {}
        }

        public static string Serialize(CycloneDX.Models.v1_2.Bom bom)
        {
            if (bom.Metadata?.Component?.Name == null || bom.Metadata?.Component?.Version == null)
                throw new SpdxSerializationException("For SPDX output top level component name and version are required in the BOM metadata");
            
            var nonSpdxLicenses = new List<CycloneDX.Models.v1_2.License>();
            string bomSpdxRef;
            if (string.IsNullOrEmpty(bom.SerialNumber))
            {
                bomSpdxRef = System.Guid.NewGuid().ToString();
            }
            else if (bom.SerialNumber.StartsWith("urn:uuid:"))
            {
                bomSpdxRef = bom.SerialNumber.Remove(0, 9);
            }
            else
            {
                bomSpdxRef = bom.SerialNumber;
            }

            var sb = new StringBuilder();
            var componentSb = new StringBuilder();
            sb.AppendLine("SPDXVersion: SPDX-2.1");
            // CC0-1.0 is a requirement when using the SPDX specification
            sb.AppendLine("DataLicense: CC0-1.0");
            sb.AppendLine($"SPDXID: SPDXRef-DOCUMENT");
            sb.AppendLine($"DocumentName: {bom.Metadata.Component.Name}-{bom.Metadata.Component.Version}");
            sb.AppendLine($"DocumentNamespace: http://spdx.org/spdxdocs/{bom.Metadata.Component.Name}-{bom.Metadata.Component.Version}-{bomSpdxRef}");
            if (bom.Metadata.Authors != null)
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
                    componentSpdxRef = component.BomRef;
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
                        case CycloneDX.Models.v1_2.Hash.HashAlgorithm.SHA_1:
                            algStr = "SHA1";
                            break;
                        case CycloneDX.Models.v1_2.Hash.HashAlgorithm.SHA_256:
                            algStr = "SHA256";
                            break;
                        // following algorithms only supported in v2.2
                        // case Hash.HashAlgorithm.SHA_384:
                        //     algStr = "SHA384";
                        //     break;
                        // case Hash.HashAlgorithm.SHA_512:
                        //     algStr = "SHA512";
                        //     break;
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
                    if (component.Licenses.Count > 1) sb.Append("(");
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
                    if (component.Licenses.Count > 1) sb.Append(")");
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
                    if (component.Cpe.ToLowerInvariant().StartsWith("cpe:2.2:"))
                    {
                        sb.Append("ExternalRef: SECURITY cpe22Type ");
                        sb.AppendLine(component.Cpe);
                    }
                    else if (component.Cpe.ToLowerInvariant().StartsWith("cpe:2.3:"))
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
                sb.AppendLine($"ExtractedText: <text>\"{license.Name}\": {WebUtility.HtmlEncode(license.Text.Content)}</text>");
                sb.AppendLine($"LicenseName: {license.Name}");
                if (!string.IsNullOrEmpty(license.Url))
                    sb.AppendLine($"LicenseCrossReference: {license.Url}");
            }

            return sb.ToString();
        }

    }
}
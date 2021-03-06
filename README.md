[![Build Status](https://github.com/CycloneDX/cyclonedx-cli/workflows/.NET%20Core%20CI/badge.svg)](https://github.com/CycloneDX/cyclonedx-cli/actions?workflow=.NET+Core+CI)
[![Docker Image](https://img.shields.io/badge/docker-image-brightgreen?style=flat&logo=docker)](https://hub.docker.com/r/cyclonedx/cyclonedx-cli)
[![License](https://img.shields.io/badge/license-Apache%202.0-brightgreen.svg)](LICENSE)
[![Website](https://img.shields.io/badge/https://-cyclonedx.org-blue.svg)](https://cyclonedx.org/)
[![Slack Invite](https://img.shields.io/badge/Slack-Join-blue?logo=slack&labelColor=393939)](https://cyclonedx.org/slack/invite)
[![Group Discussion](https://img.shields.io/badge/discussion-groups.io-blue.svg)](https://groups.io/g/CycloneDX)
[![Twitter](https://img.shields.io/twitter/url/http/shields.io.svg?style=social&label=Follow)](https://twitter.com/CycloneDX_Spec)

```
   ______           __                 ____ _  __    ________    ____
  / ____/_  _______/ /___  ____  ___  / __ \ |/ /   / ____/ /   /  _/
 / /   / / / / ___/ / __ \/ __ \/ _ \/ / / /   /   / /   / /    / /
/ /___/ /_/ / /__/ / /_/ / / / /  __/ /_/ /   |   / /___/ /____/ /
\____/\__, /\___/_/\____/_/ /_/\___/_____/_/|_|   \____/_____/___/
     /____/

Usage:
  cyclonedx [options] [command]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  analyze                       Analyze an SBOM file
  convert                       Convert between different SBOM formats
  diff <from-file> <to-file>    Generate an SBOM diff
  merge                         Merge two or more SBOMs
  validate                      Validate an SBOM
```

The CycloneDX CLI tool currently supports SBOM analysis, diffing, merging and format conversions.

Conversion from all CycloneDX SBOM versions and CSV is supported.

Conversion to all CycloneDX SBOM versions, CSV, SPDX tag/value v2.1 and v2.2 is supported.

Binaries can be downloaded from the [releases page](https://github.com/CycloneDX/cyclonedx-cli/releases).

# Commands

## Analyze Command

```
analyze:
  Analyze an SBOM file

Usage:
  cyclonedx analyze [options]

Options:
  --input-file <input-file>                   Input SBOM filename, will read from stdin if no value provided.
  --input-format <autodetect|csv|json|xml>    Specify input file format.
  --output-format <json|text>                 Specify output format (defaults to text).
  --multiple-component-versions               Report components that have multiple versions in use.
```

### Examples

Reporting on components that are included multiple times with different versions:  
`cyclonedx-cli analyze --input-file sbom.xml --multiple-component-versions`

## Convert Command

```
convert:
  Convert between different SBOM formats

Usage:
  cyclonedx convert [options]

Options:
  --input-file <input-file>                                                                                           Input SBOM filename, will read from stdin if no value provided.
  --output-file <output-file>                                                                                         Output SBOM filename, will write to stdout if no value provided.
  --input-format <autodetect|csv|json|xml>                                                                            Specify input file format.
  --output-format <autodetect|csv|json|json_v1_2|spdxtag|spdxtag_v2_1|spdxtag_v2_2|xml|xml_v1_0|xml_v1_1|xml_v1_2>    Specify output file format.
```

Examples

Converting from XML to JSON format:  
`cyclonedx-cli convert --input-file sbom.xml --output-file sbom.json`

Converting from XML to JSON format and piping output to additional tools:  
`cyclonedx-cli convert --input-file sbom.xml --output-format json | grep "somthing"`

### CSV Format

The CSV format is a limited representation of the list of components in an SBOM.

The intention is to provide a simple way for users to produce and consume SBOMs
for simple use cases. Including simple data migration use cases.

The only required fields are the component `name` and `version` fields. Others
can be left blank or the columns omitted.

[example.csv](example.csv)

## Diff Command

```
diff:
  Generate an SBOM diff

Usage:
  cyclonedx diff [options] <from-file> <to-file>

Arguments:
  <from-file>    From SBOM filename.
  <to-file>      To SBOM filename.

Options:
  --from-format <autodetect|csv|json|xml>    Specify from file format.
  --to-format <autodetect|csv|json|xml>      Specify to file format.
  --output-format <json|text>                Specify output format (defaults to text).
  --component-versions                       Report component versions that have been added, removed or modified.
```

### Examples

Reporting on components with version changes:  
`cyclonedx-cli diff sbom-from.xml sbom-to.xml --component-versions`

## Validate Command

```
validate:
  Validate an SBOM

Usage:
  cyclonedx validate [options]

Options:
  --input-file <input-file>                                                    Input SBOM filename, will read from stdin if no value provided.
  --input-format <autodetect|json|json_v1_2|xml|xml_v1_0|xml_v1_1|xml_v1_2>    Specify input file format.
  --fail-on-errors                                                             Fail on validation errors (return a non-zero exit code)
```

### Examples

Validate SBOM and return non-zero exit code (handy for automatically "breaking" a build, etc)  
`cyclonedx-cli validate --input-file sbom.xml --fail-on-errors`

# Docker Image

The CycloneDX CLI tool can also be run using docker `docker run cyclonedx/cyclonedx-cli`.

# Supported Platforms

Officially supported builds are available for these platforms:

- Windows x64 (win-x64)
- Linux x64 (linux-x64)
- Linux musl x64 (linux-musl-x64, includes Alpine Linux)
- MacOS x64 (osx-x64)

Community supported builds are available for these platforms:

- Windows x86 (win-x86)
- Windows ARM (win-arm)
- Windows ARM x64 (win-arm64)
- Linux ARM (linux-arm)
- Linux ARM x64 (linux-arm64)

.NET Core runtime dependencies are required.

For Windows these should be preinstalled.

For Ubuntu these are libc6 libgcc1 libgssapi-krb5-2 libicu66 libssl1.1 libstdc++6 zlib1g.

# Using gron for adhoc searching and analysis

gron transforms JSON into discrete assignments to make it easier to grep for what you want and see the absolute 'path' to it.

For convenience, gron is included in the CycloneDX CLI Docker image.

Example usage that lists all component names and versions

```
$ gron bom-1.2.json | grep -E "(components\[[[:digit:]]*\].name)|(components\[[[:digit:]]*\].version)"

json.components[0].name = "tomcat-catalina";
json.components[0].version = "9.0.14";
json.components[1].name = "mylibrary";
json.components[1].version = "1.0.0";
```

Or the same using an XML format SBOM

```
$ cyclonedx convert --input-file bom.xml --output-format json | gron | grep -E "(components\[[[:digit:]]*\].name)|(components\[[[:digit:]]*\].version)"

json.components[0].name = "tomcat-catalina";
json.components[0].version = "9.0.14";
json.components[1].name = "mylibrary";
json.components[1].version = "1.0.0";
```

For more details on gron usage refer to the [gron project page](https://github.com/TomNomNom/gron).

For more details on grep usage refer to the [grep man page](https://www.man7.org/linux/man-pages/man1/grep.1.html).

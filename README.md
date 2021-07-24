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
  analyze                       Analyze a BOM file
  convert                       Convert between different BOM formats
  diff <from-file> <to-file>    Generate a BOM diff
  merge                         Merge two or more BOMs
  validate                      Validate a BOM
```

The CycloneDX CLI tool currently supports BOM analysis, diffing, merging and format conversions.

Conversion from all CycloneDX BOM versions and CSV is supported.

Conversion to all CycloneDX BOM versions, CSV, SPDX tag/value v2.1 and v2.2 is supported.

Binaries can be downloaded from the [releases page](https://github.com/CycloneDX/cyclonedx-cli/releases).

# Commands

## Analyze Command

```
analyze:
  Analyze a BOM file

Usage:
  cyclonedx analyze [options]

Options:
  --input-file <input-file>                            Input BOM filename, will read from stdin if no value provided.
  --input-format <autodetect|csv|json|protobuf|xml>    Specify input file format.
  --output-format <json|text>                          Specify output format (defaults to text).
  --multiple-component-versions                        Report components that have multiple versions in use.
```

### Examples

Reporting on components that are included multiple times with different versions:  
`cyclonedx-cli analyze --input-file sbom.xml --multiple-component-versions`

## Convert Command

```
convert:
  Convert between different BOM formats

Usage:
  cyclonedx convert [options]

Options:
  --input-file <input-file>                                                                                                               Input BOM filename, will read from stdin if no value provided.
  --output-file <output-file>                                                                                                             Output BOM filename, will write to stdout if no value provided.
  --input-format <autodetect|csv|json|protobuf|xml>                                                                                       Specify input file format.
  --output-format <autodetect|csv|json|json_v1_2|json_v1_3|protobuf|protobuf_v1_3|spdxtag|spdxtag_v2_1|spdxtag_v2_2|xml|xml_v1_0|xml_v    Specify output file format.
  1_1|xml_v1_2|xml_v1_3>
```

### Examples

Converting from XML to JSON format:  
`cyclonedx-cli convert --input-file sbom.xml --output-file sbom.json`

Converting from XML to JSON format and piping output to additional tools:  
`cyclonedx-cli convert --input-file sbom.xml --output-format json | grep "somthing"`

### CSV Format

The CSV format is a limited representation of the list of components in a BOM.

The intention is to provide a simple way for users to produce and consume BOMs
for simple use cases. Including simple data migration use cases.

The only required fields are the component `name` and `version` fields. Others
can be left blank or the columns omitted.

[example.csv](example.csv)

## Diff Command

```
diff:
  Generate a BOM diff

Usage:
  cyclonedx diff [options] <from-file> <to-file>

Arguments:
  <from-file>    From BOM filename.
  <to-file>      To BOM filename.

Options:
  --from-format <autodetect|csv|json|protobuf|xml>    Specify from file format.
  --to-format <autodetect|csv|json|protobuf|xml>      Specify to file format.
  --output-format <json|text>                         Specify output format (defaults to text).
  --component-versions                                Report component versions that have been added, removed or modified.
```

### Examples

Reporting on components with version changes:  
`cyclonedx-cli diff sbom-from.xml sbom-to.xml --component-versions`


## Merge Command

```
merge:
  Merge two or more BOMs

Usage:
  cyclonedx merge [options]

Options:
  --input-files <input-files>                       Input BOM filenames (separate filenames with a space).
  --output-file <output-file>                       Output BOM filename, will write to stdout if no value provided.
  --input-format <autodetect|json|protobuf|xml>     Specify input file format.
  --output-format <autodetect|json|protobuf|xml>    Specify output file format.
  --hierarchical                                    Perform a hierarchical merge.
```

Note: To perform a hierarchical merge all BOMs need the subject of the BOM
described in the metadata component element.

### Examples

Merge two XML formatted BOMs:  
`cyclonedx-cli merge --input-files sbom1.xml sbom2.xml --output-file sbom_all.xml`

Merging two BOMs and piping output to additional tools:  
`cyclonedx-cli merge --input-files sbom1.xml sbom2.xml --output-format json | grep "something"`


## Validate Command

```
validate:
  Validate a BOM

Usage:
  cyclonedx validate [options]

Options:
  --input-file <input-file>                                                                       Input BOM filename, will read from stdin if no value provided.
  --input-format <autodetect|json|json_v1_2|json_v1_3|xml|xml_v1_0|xml_v1_1|xml_v1_2|xml_v1_3>    Specify input file format.
  --fail-on-errors                                                                                Fail on validation errors (return a non-zero exit code)
```

### Examples

Validate BOM and return non-zero exit code (handy for automatically "breaking" a build, etc)  
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

Or the same using an XML format BOM

```
$ cyclonedx convert --input-file bom.xml --output-format json | gron | grep -E "(components\[[[:digit:]]*\].name)|(components\[[[:digit:]]*\].version)"

json.components[0].name = "tomcat-catalina";
json.components[0].version = "9.0.14";
json.components[1].name = "mylibrary";
json.components[1].version = "1.0.0";
```

For more details on gron usage refer to the [gron project page](https://github.com/TomNomNom/gron).

For more details on grep usage refer to the [grep man page](https://www.man7.org/linux/man-pages/man1/grep.1.html).

## License

Permission to modify and redistribute is granted under the terms of the Apache 2.0 license. See the [LICENSE] file for the full license.

[License]: https://github.com/CycloneDX/cyclonedx-cli/blob/main/LICENSE

## Contributing

Pull requests are welcome. But please read the
[CycloneDX contributing guidelines](https://github.com/CycloneDX/.github/blob/master/CONTRIBUTING.md) first.

To build and test the solution locally you should have .NET 5
installed. Standard commands like `dotnet build` and `dotnet test` work.

It is generally expected that pull requests will include relevant tests.
Tests are automatically run on Windows, MacOS and Linux for every pull request.
And build warnings will break the build.

If you are having trouble debugging a test that is failing for a platform you
don't have access to please us know.
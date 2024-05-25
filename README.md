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
  cyclonedx [command] [options]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  add                         Add information to a BOM (currently supports files)
  analyze                     Analyze a BOM file
  convert                     Convert between different BOM formats
  diff <from-file> <to-file>  Generate a BOM diff
  keygen                      Generates an RSA public/private key pair for BOM signing
  merge                       Merge two or more BOMs
  sign                        Sign a BOM or file
  validate                    Validate a BOM
  verify                      Verify signatures in a BOM
```

The CycloneDX CLI tool currently supports BOM analysis, modification, diffing, merging, format conversion, signing and verification.

Conversion is supported between CycloneDX XML, JSON, Protobuf, CSV, and SPDX JSON v2.2.

Binaries can be downloaded from the [releases page](https://github.com/CycloneDX/cyclonedx-cli/releases).

Note: The CycloneDX CLI tool is built for automation use cases. Any commands that have the `--input-file` option also support feeding input from stdin. Likewise, any commands that have the `--output-file` option support output to stdout. However, you will need to supply the input/output formats.

For example:  
`cat bom.json | cyclonedx-cli convert --input-format json --output-format xml > bom.xml`

# Commands

## Add Command

### Add File Subcommand

```
files
  Add files to a BOM

Usage:
  cyclonedx add files [options]

Options:
  --input-file <input-file>                       Input BOM filename.
  --no-input                                      Use this option to indicate that there is no input BOM.
  --output-file <output-file>                     Output BOM filename, will write to stdout if no value provided.
  --input-format <autodetect|json|protobuf|xml>   Specify input file format.
  --output-format <autodetect|json|protobuf|xml>  Specify output file format.
  --base-path <base-path>                         Base path for directory to process (defaults to current working directory if omitted).
  --include <include>                             Apache Ant style path and file patterns to specify what to include (defaults to all files, separate patterns with a space).
  --exclude <exclude>                             Apache Ant style path and file patterns to specify what to exclude (defaults to none, separate patterns with a space).
```

#### Examples

Generating a source code BOM, excluding Git repository directory:  
`cyclonedx-cli add files --no-input --output-format json --exclude /.git/**`

Adding build output files, from `bin` directory, to existing BOM:  
`cyclonedx-cli add files --input-file bom.json --output-format json --base-path bin`

## Analyze Command

```
analyze
  Analyze a BOM file

Usage:
  cyclonedx analyze [options]

Options:
  --input-file <input-file>                      Input BOM filename, will read from stdin if no value provided.
  --input-format <autodetect|json|protobuf|xml>  Specify input file format.
  --output-format <json|text>                    Specify output format (defaults to text).
  --multiple-component-versions                  Report components that have multiple versions in use.
```

### Examples

Reporting on components that are included multiple times with different versions:  
`cyclonedx-cli analyze --input-file sbom.xml --multiple-component-versions`

## Convert Command

```
convert
  Convert between different BOM formats

Usage:
  cyclonedx convert [options]

Options:
  --input-file <input-file>                                    Input BOM filename, will read from stdin if no value provided.
  --output-file <output-file>                                  Output BOM filename, will write to stdout if no value provided.
  --input-format <autodetect|csv|json|protobuf|spdxjson|xml>   Specify input file format.
  --output-format <autodetect|csv|json|protobuf|spdxjson|xml>  Specify output file format.
  --output-version <v1_0|v1_1|v1_2|v1_3|v1_4>                  Specify output BOM specification version. (ignored for CSV and SPDX formats)  
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

### SPDX Format

Converting between SPDX and CycloneDX formats can result in the loss of some
information. The conversion functionality is provided by the
`CycloneDX.Spdx.Interop` library, which is part of the CycloneDX .NET library
project.

For more details on what information is lost refer to the
[CycloneDX .NET Library project page](https://github.com/CycloneDX/cyclonedx-dotnet-library).

## Diff Command

```
diff
  Generate a BOM diff

Usage:
  cyclonedx diff <from-file> <to-file> [options]

Arguments:
  <from-file>  From BOM filename.
  <to-file>    To BOM filename.

Options:
  --from-format <autodetect|json|protobuf|xml>  Specify from file format.
  --to-format <autodetect|json|protobuf|xml>    Specify to file format.
  --output-format <json|text>                   Specify output format (defaults to text).
  --component-versions                          Report component versions that have been added, removed or modified.
```

### Examples

Reporting on components with version changes:  
`cyclonedx-cli diff sbom-from.xml sbom-to.xml --component-versions`

## Keygen Command

```
keygen
  Generates an RSA public/private key pair for BOM signing

Usage:
  cyclonedx keygen [options]

Options:
  --private-key-file <private-key-file>  Filename for generated private key file (defaults to "private.key")
  --public-key-file <public-key-file>    Filename for generated public key file (defaults to "public.key")
```

## Merge Command

```
merge
  Merge two or more BOMs

Usage:
  cyclonedx merge [options]

Options:
  --input-files <input-files>                     Input BOM filenames (separate filenames with a space).
  --input-files-list <input-files-list-files>     One or more text file(s) with input BOM filenames (one per line).
  --input-files-nul-list <input-files-list-files> One or more text-like file(s) with input BOM filenames (separated by 0x00 characters).
  --output-file <output-file>                     Output BOM filename, will write to stdout if no value provided.
  --input-format <autodetect|json|protobuf|xml>   Specify input file format.
  --output-format <autodetect|json|protobuf|xml>  Specify output file format.
  --hierarchical                                  Perform a hierarchical merge.
  --group <group>                                 Provide the group of software the merged BOM describes.
  --name <name>                                   Provide the name of software the merged BOM describes (required for hierarchical merging).
  --version <version>                             Provide the version of software the merged BOM describes (required for hierarchical merging).
```

Note: To perform a hierarchical merge all BOMs need the subject of the BOM
described in the metadata component element.

The `--input-files-list` option can be useful if you have so many filenames to
merge that your shell interpreter command-line limit is exceeded if you list
them all as `--input-files`, or if your path names have spaces.

The related `--input-files-nul-list` is intended for lists prepared by commands
like `find ... -print0` and makes sense on filesystems where carriage-return
and/or line-feed characters may validly be present in a path name component.
Note: behavior with multi-byte encodings (Unicode family) where a 0x00 byte
can be part of a character may be undefined.

If you specify several of these options, the effective file lists will be
concatenated before the actual merge (first the individual `--input-files`,
then the contents of `--input-files-list`, and finally the contents of
`--input-files-nul-list`). If you have a document crafted to describe the
root of your product hierarchy tree, it is recommended to list it as the
first of individual `--input-files` (or otherwise on first line among used
lists).

### Examples

Merge two XML formatted BOMs:  
`cyclonedx-cli merge --input-files sbom1.xml sbom2.xml --output-file sbom_all.xml`

Merging two BOMs and piping output to additional tools:  
`cyclonedx-cli merge --input-files sbom1.xml sbom2.xml --output-format json | grep "something"`

## Sign Command

Sign a BOM or file

### Sign Bom Subcommand

```
bom
  Sign the entire BOM document

Usage:
  cyclonedx sign bom <bom-file> [options]

Arguments:
  <bom-file>  BOM filename

Options:
  --key-file <key-file>  Signing key filename (RSA private key in PEM format, defaults to "private.key")
```

### Sign File Subcommand

```
file
  Sign arbitrary files and generate a PKCS1 RSA SHA256 signature file

Usage:
  cyclonedx sign file <file> [options]

Arguments:
  <file>  Filename of the file the signature will be created for

Options:
  --key-file <key-file>              Signing key filename (RSA private key in PEM format, defaults to "private.key")
  --signature-file <signature-file>  Filename of the generated signature file (defaults to the filename with ".sig" appended)
```

## Validate Command

```
validate
  Validate a BOM

Usage:
  cyclonedx validate [options]

Options:
  --input-file <input-file>                   Input BOM filename, will read from stdin if no value provided.
  --input-format <autodetect|json|xml>        Specify input file format.
  --input-version <v1_0|v1_1|v1_2|v1_3|v1_4>  Specify input file specification version (defaults to v1.4)
  --fail-on-errors                            Fail on validation errors (return a non-zero exit code)
```

### Examples

Validate BOM and return non-zero exit code (handy for automatically "breaking" a build, etc)  
`cyclonedx-cli validate --input-file sbom.xml --fail-on-errors`

## Verify Command

Verify signatures for BOMs and files

### Verify All Subcommand

```
all
  Verify all signatures in a BOM

Usage:
  cyclonedx verify all <bom-file> [options]

Arguments:
  <bom-file>  BOM filename

Options:
  --key-file <key-file>  Public key filename (RSA public key in PEM format, defaults to "public.key")
```

### Verify File Subcommand

```
file
  Verifies a PKCS1 RSA SHA256 signature file for an arbitrary file

Usage:
  cyclonedx verify file <file> [options]

Arguments:
  <file>  File the signature file is for

Options:
  --key-file <key-file>              Public key filename (RSA public key in PEM format, defaults to "public.key")
  --signature-file <signature-file>  Signature file to be verified (defaults to the filename with ".sig" appended)
```

# Docker Image

The CycloneDX CLI tool can also be run using docker `docker run cyclonedx/cyclonedx-cli`.

# Homebrew

For Linux and MacOS, the CLI can be installed via the [CycloneDX Homebrew tap](https://github.com/CycloneDX/homebrew-cyclonedx):

```shell
brew install cyclonedx/cyclonedx/cyclonedx-cli
```

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
- MacOS ARM x64 (osx-arm64)

.NET Core runtime dependencies are required.

For Windows these should be preinstalled.

For Ubuntu these are libc6 libgcc1 libgssapi-krb5-2 libicu66 libssl1.1 libstdc++6 zlib1g.

# Using gron for adhoc searching and analysis

_gron transforms JSON into discrete assignments to make it easier to grep for what you want and see the absolute 'path' to it._

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

To build and test the solution locally you should have .NET 6
installed. Standard commands like `dotnet build` and `dotnet test` work.

It is generally expected that pull requests will include relevant tests.
Tests are automatically run on Windows, MacOS and Linux for every pull request.
Build warnings will break the build.

Please let us know if you are having trouble debugging a test that is failing
for a platform that you don't have access to.

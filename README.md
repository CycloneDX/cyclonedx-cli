```
   ______           __                 ____ _  __    ________    ____
  / ____/_  _______/ /___  ____  ___  / __ \ |/ /   / ____/ /   /  _/
 / /   / / / / ___/ / __ \/ __ \/ _ \/ / / /   /   / /   / /    / /
/ /___/ /_/ / /__/ / /_/ / / / /  __/ /_/ /   |   / /___/ /____/ /
\____/\__, /\___/_/\____/_/ /_/\___/_____/_/|_|   \____/_____/___/
     /____/
```

This is a preview version of the CycloneDX CLI tool.

It currently supports SBOM analysis and format conversions.

Conversion from all CycloneDX SBOM versions and CSV is supported.

Conversion to all CycloneDX SBOM versions, CSV, SPDX tag/value v2.1 and v2.2 is supported.

Binaries can be downloaded from the [releases page](https://github.com/CycloneDX/cyclonedx-cli/releases).

## Usage

Basic usage:  
`cyclonedx [command] [options]`

Show help and usage information:  
`cyclonedx --help`

## Analyze Command

Analyze an SBOM file

Usage:
  `cyclonedx analyze [options]`

Options:

| Option | Description |
| --- | --- |
| `--input-file <input-file>` | Input SBOM filename, will read from stdin if no value provided. |
| `--input-format <autodetect \| csv \| json \| xml>` | Specify input file format. |
| `--output-format <text \| json>` | Specify output file format (defaults to text). |
| `--multiple-component-versions` | Report components that have multiple versions in use. |

### Multiple Component Versions

This option will include a list of all components that have multiple versions in
use.

Matching is based on component name.

## Convert Command

Convert between different SBOM formats

Usage:
  `cyclonedx convert [options]`

Options:

| Option | Description |
| --- | --- |
| `--input-file <input-file>` | Input SBOM filename, will read from stdin if no value provided. |
| `--output-file <output-file>` | Output SBOM filename, will write to stdout if no value provided. |
| `--input-format <autodetect \| csv \| json \| xml>` | Specify input file format. |
| `--output-format <autodetect \| csv \| json\|json_v1_2 \| spdxtag\|spdxtag_v2_1\|spdxtag_v2_2 \| xml\|xml_v1_0\|xml_v1_1\|xml_v1_2>` | Specify output file format. |

### CSV Format

The CSV format is a limited representation of the list of components in an SBOM.

The intention is to provide a simple way for users to produce and consume SBOMs
for simple use cases. Including simple data migration use cases.

The only required fields are the component `name` and `version` fields. Others
can be left blank or the columns omitted.

[example.csv](example.csv)

## Docker Image

The CycloneDX CLI tool can also be run using docker `docker run cyclonedx/cyclonedx-cli`.

## Supported Platforms

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

# CycloneDX CLI

This is a preview version of the CycloneDX CLI tool.

It currently supports converting between JSON and XML SBOM formats.

## Docker Image

The CycloneDX CLI tool can be run using docker `docker run cyclonedx/cyclonedx-cli`.

## Supported Platforms

Officially supported builds are planned for these platforms:

- Windows x64 (win-x64)
- Linux x64 (linux-x64)
- Linux musl x64 (linux-musl-x64, includes Alpine Linux)
- MacOS x64 (osx-x64)

Community supported builds are planned for these platforms:

- Windows x86 (win-x86)
- Windows ARM (win-arm)
- Windows ARM x64 (win-arm64)
- Linux ARM (linux-arm)
- Linux ARM x64 (linux-arm64)

.NET Core runtime dependencies are required.

For Windows these should be preinstalled.

For Ubuntu these are libc6 libgcc1 libgssapi-krb5-2 libicu66 libssl1.1 libstdc++6 zlib1g.
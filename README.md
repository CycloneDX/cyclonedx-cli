# CycloneDX CLI

This is a preview version of the CycloneDX CLI tool.

It currently supports converting between JSON and XML SBOM formats.

## Local Docker Image Build

After cloning this repo locally and running `local-docker-build.sh` you can use
`docker run cyclonedx/cyclonedx-cli`.

## Supported Platforms

Supported official builds are planned for these platforms:

- Windows x64 (win-x64)
- Linux x64 (linux-x64)
- Linux musl x64 (linux-musl-x64, includes Alpine Linux)
- MacOS x64 (osx-x64)

Unsupported official builds are planned for these platforms:

- Windows x86 (win-x86)
- Windows ARM (win-arm)
- Windows ARM x64 (win-arm64)
- Linux ARM (linux-arm)
- Linux ARM x64 (linux-arm64)
#!/usr/bin/env bash
dotnet publish src/cyclonedx/cyclonedx.csproj -r linux-x64 --configuration Release --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesInSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --output bin/linux-x64
docker build . --tag cyclonedx/cyclonedx-cli
docker run cyclonedx/cyclonedx-cli
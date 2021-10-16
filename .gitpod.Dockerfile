ARG BASE_IMAGE=gitpod/workspace-full:latest
FROM ${BASE_IMAGE}

ARG USER=gitpod

USER root
# Install .NET runtime dependencies
RUN apt-get update \
    && apt-get install -y \
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu60 \
        libssl1.1 \
        libstdc++6 \
        zlib1g \
    && rm -rf /var/lib/apt/lists/*

USER ${user}

# Install .NET SDK
# Source: https://docs.microsoft.com/dotnet/core/install/linux-scripted-manual#scripted-install
RUN mkdir -p "${HOME}/dotnet" \
    && wget --output-document="${HOME}/dotnet/dotnet-install.sh" https://dot.net/v1/dotnet-install.sh \
    && chmod +x "${HOME}/dotnet/dotnet-install.sh"
RUN "${HOME}/dotnet/dotnet-install.sh" --channel 5.0 --install-dir "${HOME}/dotnet"

ENV DOTNET_ROOT="${HOME}/dotnet"
ENV PATH=$PATH:"${HOME}/dotnet"

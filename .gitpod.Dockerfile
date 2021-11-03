ARG BASE_IMAGE=gitpod/workspace-full-vnc:latest
ARG USERNAME=gitpod

FROM $BASE_IMAGE

USER root
# Install .NET runtime dependencies and some dev tools
RUN apt-get update \
    && apt-get install -y \
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu66 \
        libssl1.1 \
        libstdc++6 \
        zlib1g \
        git-gui \
        meld \
        kdiff3 \
    && rm -rf /var/lib/apt/lists/*

USER $USERNAME

# Install .NET SDK
# Source: https://docs.microsoft.com/dotnet/core/install/linux-scripted-manual#scripted-install
# RUN mkdir -p "$HOME/dotnet" \
#     && wget --output-document="$HOME/dotnet/dotnet-install.sh" https://dot.net/v1/dotnet-install.sh \
#     && chmod +x "$HOME/dotnet/dotnet-install.sh"
# RUN "$HOME/dotnet/dotnet-install.sh" --channel 5.0 --install-dir "$HOME/dotnet"

# ENV DOTNET_ROOT="$HOME/dotnet"
# ENV PATH=$PATH:"$HOME/dotnet"

# messy handling for https://github.com/gitpod-io/gitpod/issues/5090
ENV DOTNET_ROOT="/workspace/.dotnet"
ENV PATH=$PATH:$DOTNET_ROOT
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true

RUN mkdir -p "$DOTNET_ROOT" \
    && wget --output-document="$DOTNET_ROOT/dotnet-install.sh" https://dot.net/v1/dotnet-install.sh \
    && chmod +x "$DOTNET_ROOT/dotnet-install.sh"
RUN "$DOTNET_ROOT/dotnet-install.sh" --channel 5.0 --install-dir "$DOTNET_ROOT"

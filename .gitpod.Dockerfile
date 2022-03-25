FROM gitpod/workspace-full:latest

USER gitpod

# Install .NET SDK
# Source: https://docs.microsoft.com/dotnet/core/install/linux-scripted-manual#scripted-install
#RUN mkdir -p /home/gitpod/dotnet && curl -fsSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 6.0 --install-dir /home/gitpod/dotnet
#ENV DOTNET_ROOT=/home/gitpod/dotnet
#ENV PATH=$PATH:/home/gitpod/dotnet

# Refer to for reasons https://github.com/gitpod-io/template-dotnet-core-cli-csharp/commit/9d01b88fa900c7802103a13ca0cc18b2b02c4752
ENV DOTNET_ROOT=/workspace/local/dotnet
ENV PATH=$DOTNET_ROOT:$PATH
ENV PATH=/workspace/local/bin:$PATH

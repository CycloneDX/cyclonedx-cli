FROM mcr.microsoft.com/dotnet/runtime-deps:5.0

COPY bin/linux-x64/cyclonedx /cyclonedx

ADD https://github.com/tomnomnom/gron/releases/download/v0.6.1/gron-linux-amd64-0.6.1.tgz /tmp/gron.tgz

RUN tar xzf /tmp/gron.tgz \
    && mv ./gron /usr/local/bin/ \
    && rm /tmp/gron.tgz

ENTRYPOINT [ "/cyclonedx" ]
CMD [ "--help" ]
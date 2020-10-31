FROM mcr.microsoft.com/dotnet/runtime-deps:5.0

COPY bin/linux-x64/cyclonedx /cyclonedx

ENTRYPOINT [ "/cyclonedx" ]
CMD [ "--help" ]
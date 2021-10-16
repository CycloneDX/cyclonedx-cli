#!/usr/bin/env bash

docker build --tag cyclonedx-cli-development --build-arg BASE_IMAGE=ghcr.io/coderpatros/openvscode-server --build-arg USERNAME=openvscode-server --file .gitpod.Dockerfile .
docker run -it --init -p 3000:3000 -v "$(pwd):/workspace:cached" cyclonedx-cli-development

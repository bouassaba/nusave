#!/bin/bash
arch="$1"
version="$2"
publishDir=./bin/Release/net5.0/${arch}/publish
rm -rf ${publishDir}/*
cd ./src/CommandLine && \
dotnet publish -c Release -r ${arch} -p:PublishSingleFile=true --self-contained true && \
mkdir -p ../../dist && \
cd ${publishDir} && \
if test -f ./NuSave.CommandLine; then
    mv NuSave.CommandLine nusave
fi
if test -f ./NuSave.CommandLine.exe; then
    mv NuSave.CommandLine.exe nusave.exe
fi
zip -r ../../../../../../../dist/nusave-${version}-${arch}.zip . && \
cd ../../../../../../../
#!/bin/bash
:<<"::CMDLITERAL"
@ECHO OFF
CLS
.\bin\win-x64\amethyst %*
exit /b 1
::CMDLITERAL
ARCH="$(uname -m)"
if [ "$ARCH" = "aarch64" ]; then
    ./bin/linux-arm64/amethyst "$@"
elif [ "$ARCH" = "x86_64" ]; then
    ./bin/linux-x64/amethyst "$@"
else
    echo "Unsupported architecture '$ARCH'"
fi

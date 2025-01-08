#!/bin/bash
:<<"::CMDLITERAL"
@ECHO OFF
CLS
.\win-x64\amethyst %*
exit /b 1
::CMDLITERAL
ARCH="$(uname -m)"
if [ "$ARCH" = "aarch64" ]; then
    ./linux-arm64/amethyst "$@"
elif [ "$ARCH" = "x86_64" ]; then
    ./linux-x64/amethyst "$@"
else
    echo "Unsupported architecture '$ARCH'"
fi

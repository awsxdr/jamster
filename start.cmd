#!/bin/bash
:<<"::CMDLITERAL"
@ECHO OFF
CLS
.\bin\win-x64\jamster.engine --root-path=.\ %*
exit /b 1
::CMDLITERAL
ARCH="$(uname -m)"
if [ "$ARCH" = "aarch64" ]; then
    ./bin/linux-arm64/jamster.engine --root-path=./ "$@"
elif [ "$ARCH" = "x86_64" ]; then
    ./bin/linux-x64/jamster.engine --root-path=./ "$@"
else
    echo "Unsupported architecture '$ARCH'"
fi

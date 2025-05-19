#!/bin/sh

SCRIPT_DIR=$(dirname "$(realpath "$0")")

if [ -f "$SCRIPT_DIR/Hyjinx" ]; then
    HYJINX_BIN="Hyjinx"
fi

if [ -z "$HYJINX_BIN" ]; then
    exit 1
fi

COMMAND="env DOTNET_EnableAlternateStackCheck=1"

if command -v gamemoderun > /dev/null 2>&1; then
    COMMAND="$COMMAND gamemoderun"
fi

exec $COMMAND "$SCRIPT_DIR/$HYJINX_BIN" "$@"

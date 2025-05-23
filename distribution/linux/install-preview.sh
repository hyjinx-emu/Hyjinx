#!/bin/bash

# Config
EMULATOR_NAME="Hyjinx"
REPO="hyjinx-emu/Hyjinx"
INSTALL_DIR="$HOME/Applications/$EMULATOR_NAME"
TMP_DIR="/tmp/$EMULATOR_NAME"
CONFIG_DIR="$HOME/.config/$EMULATOR_NAME"
VERSION_FILE="$CONFIG_DIR/.version"

# Get latest prerelease version tag from GitHub
echo "🔍 Checking for latest prerelease..."
LATEST_VERSION=$(curl -s "https://api.github.com/repos/$REPO/releases" \
  | jq -r '[.[] | select(.prerelease)][0].tag_name')

if [[ -z "$LATEST_VERSION" || "$LATEST_VERSION" == "null" ]]; then
    echo "❌ Failed to get latest prerelease version from GitHub."
    exit 1
fi

echo "🔖 Latest version: $LATEST_VERSION"

# Check installed version
if [[ -f "$VERSION_FILE" ]]; then
    INSTALLED_VERSION=$(cat "$VERSION_FILE")
    if [[ "$INSTALLED_VERSION" == "$LATEST_VERSION" ]]; then
        echo "✅ Hyjinx $INSTALLED_VERSION is already installed."
        exit 0
    fi
    echo "↪️ Updating from $INSTALLED_VERSION to $LATEST_VERSION..."
else
    echo "📥 No version installed yet. Proceeding with install..."
fi

# Set up asset URL and name
ASSET_NAME="hyjinx-${LATEST_VERSION}-linux_x64.tar.gz"
ASSET_URL="https://github.com/$REPO/releases/download/$LATEST_VERSION/$ASSET_NAME"

# Clean up
rm -rf "$INSTALL_DIR" "$TMP_DIR"
mkdir -p "$TMP_DIR" "$CONFIG_DIR"

# Download
echo "⬇️ Downloading $ASSET_NAME..."
curl -L -o "$TMP_DIR/$ASSET_NAME" "$ASSET_URL"
if [ $? -ne 0 ]; then
    echo "❌ Failed to download $ASSET_NAME"
    exit 1
fi

# Extract
echo "📦 Installing to $INSTALL_DIR..."
mkdir -p "$INSTALL_DIR"
tar -xzf "$TMP_DIR/$ASSET_NAME" -C "$INSTALL_DIR"

# Find executable
EXECUTABLE="$INSTALL_DIR/$EMULATOR_NAME"
chmod +x "$EXECUTABLE"

# Create .desktop shortcut
echo "🖥 Creating desktop shortcut..."
mkdir -p "$HOME/.local/share/applications"
cat > "$HOME/.local/share/applications/${EMULATOR_NAME}.desktop" <<EOF
[Desktop Entry]
Name=$EMULATOR_NAME
Exec=$EXECUTABLE.sh
Icon=$INSTALL_DIR/Logo.png
Type=Application
Categories=Game;
EOF

# Save installed version
echo "$LATEST_VERSION" > "$VERSION_FILE"

echo "✅ Hyjinx $LATEST_VERSION installed!"

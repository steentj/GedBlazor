#!/bin/bash

echo "Bruger dotnet version:"
dotnet --version

set -e

APP_NAME="GedBlazor"
OUTPUT_DIR="upload_ged"

echo "🧹 Sletter gamle build-mapper..."
rm -rf bin obj "$OUTPUT_DIR"

echo "📦 Bygger Blazor WASM til undermappe /ged/..."
dotnet publish "GedBlazor.csproj" -c Release 

PUBLISH_DIR="bin/Release/net9.0/publish/wwwroot"

if [ ! -d "$PUBLISH_DIR" ]; then
    echo "❌ Kunne ikke finde publish wwwroot mappe. Tjek build-output."
    exit 1
fi

echo "📂 Kopierer filer til $OUTPUT_DIR..."
mkdir -p "$OUTPUT_DIR"
cp -r "$PUBLISH_DIR"/* "$OUTPUT_DIR/"

echo "✅ Klar! Upload indholdet af $OUTPUT_DIR til thrane-jacobsen.dk/ged/"
#!/bin/bash

echo "Bruger dotnet version:"
dotnet --version

set -e

APP_NAME="GedBlazor"
OUTPUT_DIR="upload_ged"

echo "🧹 Sletter gamle build-mapper..."
rm -rf bin obj "$OUTPUT_DIR"

echo "📦 Bygger Blazor WASM til undermappe /ged/..."
dotnet publish "GedBlazor.csproj" -c Release \
  -o "bin/Release/net9.0/publish" \
  -p:StaticWebAssetBasePath=/ged \
  -p:PublishTrimmed=true \
  -p:TrimMode=link \
  -p:BlazorEnableCompression=true \
  -p:RunAOTCompilation=false \
  -p:InvariantGlobalization=true \
  -p:BlazorWebAssemblyIOMode=lazy \
  -p:WasmEnableSIMD=false \
  -p:EmccVerbose=false

PUBLISH_DIR="bin/Release/net9.0/publish/wwwroot"

if [ ! -d "$PUBLISH_DIR" ]; then
    echo "❌ Kunne ikke finde publish wwwroot mappe. Tjek build-output."
    exit 1
fi

echo "📂 Kopierer filer til $OUTPUT_DIR..."
mkdir -p "$OUTPUT_DIR"
cp -r "$PUBLISH_DIR"/* "$OUTPUT_DIR/"

# Verify theme trimming worked
THEMES_SIZE=$(du -sh "$OUTPUT_DIR/_content/Syncfusion.Blazor.Themes" | cut -f1)
echo "📊 Size of Syncfusion themes folder: $THEMES_SIZE"

echo "✅ Klar! Upload indholdet af $OUTPUT_DIR til thrane-jacobsen.dk/ged/"
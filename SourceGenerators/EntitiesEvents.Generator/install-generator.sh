#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

dotnet build "$SCRIPT_DIR/EntitiesEvents.Generator.csproj" -c Release
cp "$SCRIPT_DIR/bin/Release/netstandard2.0/EntitiesEventsGenerator.dll" \
  "$REPO_ROOT/Assets/EntitiesEvents/Generator/EntitiesEventsGenerator.dll"

echo "Installed Assets/EntitiesEvents/Generator/EntitiesEventsGenerator.dll"
echo "Keep the RoslynAnalyzer label in EntitiesEventsGenerator.dll.meta."

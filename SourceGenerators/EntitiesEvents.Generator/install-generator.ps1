$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Resolve-Path (Join-Path $ScriptDir "../..")

dotnet build (Join-Path $ScriptDir "EntitiesEvents.Generator.csproj") -c Release
Copy-Item `
  (Join-Path $ScriptDir "bin/Release/netstandard2.0/EntitiesEventsGenerator.dll") `
  (Join-Path $RepoRoot "Assets/EntitiesEvents/Generator/EntitiesEventsGenerator.dll") `
  -Force

Write-Host "Installed Assets/EntitiesEvents/Generator/EntitiesEventsGenerator.dll"
Write-Host "Keep the RoslynAnalyzer label in EntitiesEventsGenerator.dll.meta."

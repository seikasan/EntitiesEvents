@echo off
setlocal
set SCRIPT_DIR=%~dp0
for %%I in ("%SCRIPT_DIR%..\..") do set REPO_ROOT=%%~fI

dotnet build "%SCRIPT_DIR%EntitiesEvents.Generator.csproj" -c Release
if errorlevel 1 exit /b %errorlevel%

copy /Y "%SCRIPT_DIR%bin\Release\netstandard2.0\EntitiesEventsGenerator.dll" "%REPO_ROOT%\Assets\EntitiesEvents\Generator\EntitiesEventsGenerator.dll"
if errorlevel 1 exit /b %errorlevel%

echo Installed Assets\EntitiesEvents\Generator\EntitiesEventsGenerator.dll
echo Keep the RoslynAnalyzer label in EntitiesEventsGenerator.dll.meta.

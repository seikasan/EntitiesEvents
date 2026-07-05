# EntitiesEventsGenerator

This project contains the Unity 6+ source for `Assets/EntitiesEvents/Generator/EntitiesEventsGenerator.dll`.

Unity 6 source generators must target .NET Standard 2.0 and use `Microsoft.CodeAnalysis.CSharp` 4.3.0. This project intentionally does not support the older Unity 2022.3 / Roslyn 3.8 path.

Build and install:

```bash
SourceGenerators/EntitiesEvents.Generator/install-generator.sh
```

On Windows PowerShell:

```powershell
SourceGenerators/EntitiesEvents.Generator/install-generator.ps1
```

Keep the `RoslynAnalyzer` label on `Assets/EntitiesEvents/Generator/EntitiesEventsGenerator.dll.meta`, and keep all plugin platforms disabled.

The generator is implemented as an incremental source generator and emits unmanaged `ISystem` event cleanup systems. It deliberately avoids `SystemAPI` in generated code so it does not depend on Unity.Entities source generators re-processing generated files.

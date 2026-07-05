# Entities Events

Entities Events provides lightweight inter-system messaging for Unity ECS. This fork targets Unity 6 and Entities 1.4, uses a Roslyn source generator, and keeps the runtime cleanup path on unmanaged `ISystem` only.

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

[日本語版READMEはこちら](README_JA.md)

## Requirements

- Unity 6.0 / 6000.0 or newer
- Entities 1.4.7 or newer

## Installation

Open Package Manager, choose `Add package from git URL`, and use:

```text
https://github.com/seikasan/EntitiesEvents.git?path=Assets/EntitiesEvents
```

Or add the package to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.seikasan.entities-events": "https://github.com/seikasan/EntitiesEvents.git?path=Assets/EntitiesEvents"
  }
}
```

## Basic usage

Events are keyed by unmanaged struct type. Register every event type once at assembly scope so the source generator can emit the cleanup system and generic component registration.

```cs
using EntitiesEvents;

public struct MyEvent
{
    public int Value;
}

[assembly: RegisterEvent(typeof(MyEvent))]
```

Cache writers and readers in `OnCreate`. `EventReader<T>` stores its own read position, so recreating it every frame can cause duplicated reads.

```cs
using EntitiesEvents;
using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct WriteEventSystem : ISystem
{
    private EventWriter<MyEvent> _writer;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _writer = state.GetEventWriter<MyEvent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _writer.Write(new MyEvent { Value = 1 });
    }
}

public partial struct ReadEventSystem : ISystem
{
    private EventReader<MyEvent> _reader;

    public void OnCreate(ref SystemState state)
    {
        _reader = state.GetEventReader<MyEvent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var eventData in _reader.Read())
        {
            // Handle eventData.
        }
    }
}
```

## Parallel writing

Parallel writers intentionally never resize the backing buffer. Reserve enough space before scheduling jobs, then call `WriteNoResize` from worker threads.

```cs
using EntitiesEvents;
using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct ParallelWriteEventSystem : ISystem
{
    private EventParallelWriter<MyEvent> _writer;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EnsureEventCapacity<MyEvent>(1024);
        _writer = state.GetEventParallelWriter<MyEvent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Pass _writer to a job and call WriteNoResize inside Execute.
    }
}
```

`EnsureEventCapacity<T>(capacity)` guarantees an absolute capacity for the current and next frame buffers. `EnsureAdditionalEventCapacity<T>(additionalCapacity)` reserves space relative to the current frame write count. Direct `Events<T>` usage also exposes `Capacity`, `CurrentFrameCount`, `RemainingCurrentFrameCapacity`, `EnsureCapacity`, and `EnsureAdditionalCapacity`.

## Event lifetime

A written event can be read during the same frame and the following frame. After two `Update` calls, unread events are discarded. This design lets receiver systems run before sender systems with a one-frame delay, but it also means readers should run every frame if events must not be lost. If same-frame delivery is required, specify system order with `UpdateBefore` or `UpdateAfter`. `EventReader<T>.Read()` returns a stable snapshot bounded at the moment it is called, so events written afterwards are read on the next call.

## Events<T>

`Events<T>` is the underlying native container. Use it when you want to manage event lifetime manually instead of relying on generated ECS cleanup systems.

```cs
using EntitiesEvents;
using Unity.Collections;

var events = new Events<MyEvent>(32, Allocator.Temp);
var writer = events.GetWriter();
var reader = events.GetReader();

writer.Write(new MyEvent { Value = 1 });

foreach (var eventData in reader.Read())
{
    // Handle eventData.
}

events.Update();
events.Dispose();
```

## Source generator maintenance

The source for `Assets/EntitiesEvents/Generator/EntitiesEventsGenerator.dll` lives in `SourceGenerators/EntitiesEvents.Generator`. This fork targets Unity 6+ only, so the generator is fixed to `Microsoft.CodeAnalysis.CSharp` 4.3.0 and no longer keeps the Unity 2022.3 / Roslyn 3.8 compatibility path.

After changing the generator source, rebuild and copy the DLL back into the package:

```bash
SourceGenerators/EntitiesEvents.Generator/install-generator.sh
```

On Windows, use:

```bat
SourceGenerators\EntitiesEvents.Generator\install-generator.cmd
```

Keep the `RoslynAnalyzer` label and the disabled plugin platform settings in `Assets/EntitiesEvents/Generator/EntitiesEventsGenerator.dll.meta`.

## Tests and samples

Runtime tests live in `Assets/EntitiesEvents/Tests/Runtime`. The package sample lives in `Assets/EntitiesEvents/Samples~/BasicUsage` and is exposed through the package manifest.

## License

[MIT License](LICENSE)

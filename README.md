# Entities Events
Provides inter-system messaging functionality to Unity ECS

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

[日本語版READMEはこちら](README_JA.md)

## Overview

Entities Events is a library that adds event functionality to Unity's Entity Component System (ECS). It allows for easy implementation of messaging between systems using EventWriter/EventReader.

## Features

* Natural inter-system messaging using EventWriter/EventReader
* Creating a custom event system using Events<T>

### Requirements

* Unity 6.0 / 6000.0 or higher
* Entities 1.3.15 or higher

### Installation

1. Open the Package Manager from Window > Package Manager.
2. Click the "+" button and select "Add package from git URL."
3. Enter the following URL:

```
https://github.com/AnnulusGames/EntitiesEvents.git?path=Assets/EntitiesEvents
```

Alternatively, open Packages/manifest.json and add the following to the dependencies block:

```json
{
    "dependencies": {
        "com.annulusgames.entities-events": "https://github.com/AnnulusGames/EntitiesEvents.git?path=Assets/EntitiesEvents"
    }
}
```

## Basic Usage

In Entities Events, you perform event writing/reading based on the type of event. First, define a structure to be used for events. The structure used for events cannot contain reference types and must be an unmanaged type.

```cs
public struct MyEvent { }
```

The type of event you want to use must be registered in advance using the `RegisterEvent` attribute. Adding this attribute generates code that includes the necessary System and assembly attributes during compilation.

```cs
using EntitiesEvents;

// Add RegisterEvent attribute to the assembly
[assembly: RegisterEvent(typeof(MyEvent))]
```

In the sending System, use `EventWriter` to publish events.

```cs
using Unity.Burst;
using Unity.Entities;
using EntitiesEvents;

[BurstCompile]
public partial struct WriteEventSystem : ISystem
{
    // Cache the obtained EventWriter within the System
    EventWriter<MyEvent> eventWriter;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Obtain the EventWriter with GetEventWriter
        eventWriter = state.GetEventWriter<MyEvent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Publish the event using Write
        eventWriter.Write(new MyEvent());
    }
}
```

In the receiving System, use `EventReader` to read the published events.

```cs
using Unity.Burst;
using Unity.Entities;
using EntitiesEvents;

[BurstCompile]
public partial struct ReadEventSystem : ISystem
{
    // Cache the obtained EventReader within the System
    EventReader<MyEvent> eventReader;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Obtain the EventReader with GetEventReader
        eventReader = state.GetEventReader<MyEvent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Read unread events with eventReader.Read()
        foreach (var eventData in eventReader.Read())
        {
            Debug.Log("received!");
        }
    }
}
```

For parallel jobs, obtain an `EventParallelWriter` and reserve enough capacity before scheduling the job. Parallel writes never resize the backing buffer.

```cs
using Unity.Burst;
using Unity.Entities;
using EntitiesEvents;

[BurstCompile]
public partial struct ParallelWriteEventSystem : ISystem
{
    EventParallelWriter<MyEvent> eventWriter;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EnsureEventCapacity<MyEvent>(1024);
        eventWriter = state.GetEventParallelWriter<MyEvent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Pass eventWriter to a job and call WriteNoResize from worker threads.
    }
}
```

> **Warning**
> Always obtain EventWriter/EventReader and cache it in OnCreate. In particular, EventReader records the count of unread events for each reader, so calling `state.GetEventReader()` each time you read can lead to duplicated event reads. Do not use `EventWriter.Write` from parallel jobs; use `EventParallelWriter.WriteNoResize` instead.

## Event Mechanism

Entities Events generates a singleton Entity and an unmanaged `ISystem` for each type registered with the `RegisterEvent` attribute to hold event buffers and update the buffers. The generated EventSystem is executed within `EventSystemGroup` and clears the event buffers every frame.

However, events are held for one additional frame after being sent. This means that even if the receiving System is executed before the sending System, there will be a one-frame delay. To prevent this, you can explicitly specify the execution order between Systems using the `UpdateBefore` and `UpdateAfter` attributes.

Also, events have a lifespan of two frames, so if you do not read events every frame, there is a risk of events being lost. If you want to manually update the buffer, you can create your own EventSystem using `Events<T>` as described below.

## Events<T>

A custom NativeContainer `Events<T>` is provided as a collection to store event information.

```cs
using Unity.Collections;
using EntitiesEvents;

// Create a new Events
var events = new Events<MyEvent>(32, Allocator.Temp);
```

You can call `Update` to update the container, which swaps the internal buffer and removes the oldest buffer to prevent memory consumption due to event accumulation. It is recommended to perform this update every frame.

```cs
// Call Update to clear and swap the buffer
events.Update();
```

Writing and reading are done through `EventWriter/EventReader`, which can be obtained using `GetWriter/GetReader`. If you need to write from multiple worker threads, use `EventWriter.AsParallelWriter()` and call `EnsureEventCapacity` before scheduling the job.

```cs
// Obtain EventWriter and write on a single thread
var eventWriter = events.GetWriter();
eventWriter.Write(new MyEvent());

// Obtain EventParallelWriter for parallel jobs
var parallelWriter = eventWriter.AsParallelWriter();
parallelWriter.WriteNoResize(new MyEvent());

// Obtain EventReader and read
var eventReader = events.GetReader();
```

After use, like other NativeContainers, you must release the memory using `Dispose`. Forgetting to do this can lead to memory leaks.

```cs
// Dispose to release the container and free memory
events.Dispose();
```

## License

[MIT License](LICENSE)
## Source Generator Maintenance

The source for `Assets/EntitiesEvents/Generator/EntitiesEventsGenerator.dll` lives in `SourceGenerators/EntitiesEvents.Generator`. This fork targets Unity 6+ only, so the generator is fixed to `Microsoft.CodeAnalysis.CSharp` 4.3.0 and no longer keeps the Unity 2022.3 / Roslyn 3.8 compatibility path. After changing the generator source, run `SourceGenerators/EntitiesEvents.Generator/install-generator.cmd` on Windows or `install-generator.sh` on macOS/Linux to rebuild and copy the DLL back to `Assets/EntitiesEvents/Generator/EntitiesEventsGenerator.dll`. Keep the `RoslynAnalyzer` label plus disabled plugin platforms in the `.dll.meta` file. The generator is an incremental source generator and emits unmanaged `ISystem` cleanup systems for registered event types.

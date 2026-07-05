# Basic Usage

This sample registers a simple unmanaged event type, writes it from one `ISystem`, and reads it from another cached `EventReader`.

Parallel jobs should use `EventParallelWriter.WriteNoResize` after reserving enough capacity with `EnsureEventCapacity` or `EventWriter.EnsureCapacity`.

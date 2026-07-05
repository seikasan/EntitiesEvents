# Changelog

## 4.2.0

- Made `EventReader<T>.Read()` return a stable snapshot bounded by the event counter at call time, so events written after `Read()` are consumed by the next read instead of leaking into the current iterator.
- Cached the selected current-frame parallel writer when `EventParallelWriter<T>` is created, avoiding the write-buffer branch and `AsParallelWriter()` construction on every parallel write.
- Added a runtime test for stable read snapshots.

## 4.1.0

- Removed the obsolete `SystemBase` cleanup path and kept the generated unmanaged `ISystem` path as the only runtime path.
- Added capacity inspection and reservation APIs to `Events<T>`, `UnsafeEvents<T>`, and `EventWriter<T>`.
- Added `EnsureAdditionalEventCapacity` helper methods.
- Removed the stale Unity Package Manager lock file from the repository.
- Added package metadata for documentation, changelog, license, samples, and tests.
- Updated source generator validation for nested generic event types and removed unused generated code.

## 4.0.0

- Fork modernized for Unity 6 and Entities 1.4.
- Source generator updated to emit unmanaged `ISystem` cleanup systems.

# Changelog

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

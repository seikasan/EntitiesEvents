using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using EntitiesEvents.Internal;

namespace EntitiesEvents
{
    [NativeContainer]
    public readonly unsafe struct EventWriter<T>
        where T : unmanaged
    {
        public EventWriter(in Events<T> events)
        {
            _buffer = events.GetBuffer();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckGetSecondaryDataPointerAndThrow(events.Safety);
            var ash = events.Safety;
            AtomicSafetyHandle.UseSecondaryVersion(ref ash);
            _safety = ash;
#endif
        }

        [NativeDisableUnsafePtrRestriction] readonly EventsData<T>* _buffer;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private readonly AtomicSafetyHandle _safety;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(in T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(_safety);
#endif
            _buffer->Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventParallelWriter<T> AsParallelWriter()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(_safety);
#endif
            return new EventParallelWriter<T>(_buffer
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                , _safety
#endif
            );
        }
    }

    [NativeContainer]
    [NativeContainerIsAtomicWriteOnly]
    public readonly unsafe struct EventParallelWriter<T>
        where T : unmanaged
    {
        internal EventParallelWriter(EventsData<T>* buffer
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            , AtomicSafetyHandle safety
#endif
        )
        {
            _buffer = buffer;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            _safety = safety;
#endif
        }

        [NativeDisableUnsafePtrRestriction] readonly EventsData<T>* _buffer;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private readonly AtomicSafetyHandle _safety;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNoResize(in T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(_safety);
#endif
            _buffer->WriteNoResize(value);
        }
    }
}

namespace EntitiesEvents.LowLevel.Unsafe
{
    public readonly unsafe struct UnsafeEventWriter<T>
        where T : unmanaged
    {
        public UnsafeEventWriter(in UnsafeEvents<T> events)
        {
            _buffer = events.Buffer;
        }

        [NativeDisableUnsafePtrRestriction] readonly EventsData<T>* _buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(in T value)
        {
            _buffer->Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeEventParallelWriter<T> AsParallelWriter()
        {
            return new UnsafeEventParallelWriter<T>(_buffer);
        }
    }

    public readonly unsafe struct UnsafeEventParallelWriter<T>
        where T : unmanaged
    {
        internal UnsafeEventParallelWriter(EventsData<T>* buffer)
        {
            _buffer = buffer;
        }

        [NativeDisableUnsafePtrRestriction] readonly EventsData<T>* _buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNoResize(in T value)
        {
            _buffer->WriteNoResize(value);
        }
    }
}

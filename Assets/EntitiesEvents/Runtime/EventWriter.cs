using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using EntitiesEvents.Internal;

namespace EntitiesEvents
{
    [NativeContainer]
    public unsafe struct EventWriter<T>
        where T : unmanaged
    {
        public EventWriter(in Events<T> events)
        {
            buffer = events.GetBuffer();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckGetSecondaryDataPointerAndThrow(events.m_Safety);
            var ash = events.m_Safety;
            AtomicSafetyHandle.UseSecondaryVersion(ref ash);
            m_Safety = ash;
#endif
        }

        [NativeDisableUnsafePtrRestriction] EventsData<T>* buffer;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(in T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            buffer->Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventParallelWriter<T> AsParallelWriter()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            return new EventParallelWriter<T>(buffer
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                , m_Safety
#endif
            );
        }
    }

    [NativeContainer]
    [NativeContainerIsAtomicWriteOnly]
    public unsafe struct EventParallelWriter<T>
        where T : unmanaged
    {
        internal EventParallelWriter(EventsData<T>* buffer
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            , AtomicSafetyHandle safety
#endif
        )
        {
            this.buffer = buffer;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Safety = safety;
#endif
        }

        [NativeDisableUnsafePtrRestriction] EventsData<T>* buffer;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNoResize(in T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            buffer->WriteNoResize(value);
        }
    }
}

namespace EntitiesEvents.LowLevel.Unsafe
{
    public unsafe struct UnsafeEventWriter<T>
        where T : unmanaged
    {
        public UnsafeEventWriter(in UnsafeEvents<T> events)
        {
            buffer = events.buffer;
        }

        [NativeDisableUnsafePtrRestriction] EventsData<T>* buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(in T value)
        {
            buffer->Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeEventParallelWriter<T> AsParallelWriter()
        {
            return new UnsafeEventParallelWriter<T>(buffer);
        }
    }

    public unsafe struct UnsafeEventParallelWriter<T>
        where T : unmanaged
    {
        internal UnsafeEventParallelWriter(EventsData<T>* buffer)
        {
            this.buffer = buffer;
        }

        [NativeDisableUnsafePtrRestriction] EventsData<T>* buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNoResize(in T value)
        {
            buffer->WriteNoResize(value);
        }
    }
}

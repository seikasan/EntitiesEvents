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

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(_safety);
#endif
                return _buffer->Capacity;
            }
        }

        public int CurrentFrameCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(_safety);
#endif
                return _buffer->CurrentFrameCount;
            }
        }

        public int RemainingCurrentFrameCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(_safety);
#endif
                return _buffer->RemainingCurrentFrameCapacity;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(_safety);
#endif
            _buffer->EnsureCapacity(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureAdditionalCapacity(int additionalCapacity)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(_safety);
#endif
            _buffer->EnsureAdditionalCapacity(additionalCapacity);
        }

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
    public unsafe struct EventParallelWriter<T>
        where T : unmanaged
    {
        internal EventParallelWriter(EventsData<T>* buffer
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            , AtomicSafetyHandle safety
#endif
        )
        {
            _writer = buffer->AsParallelWriter();
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            _safety = safety;
#endif
        }

        EventsDataParallelWriter<T> _writer;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private readonly AtomicSafetyHandle _safety;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNoResize(in T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(_safety);
#endif
            _writer.WriteNoResize(value);
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

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer->Capacity;
        }

        public int CurrentFrameCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer->CurrentFrameCount;
        }

        public int RemainingCurrentFrameCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer->RemainingCurrentFrameCapacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity)
        {
            _buffer->EnsureCapacity(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureAdditionalCapacity(int additionalCapacity)
        {
            _buffer->EnsureAdditionalCapacity(additionalCapacity);
        }

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

    public unsafe struct UnsafeEventParallelWriter<T>
        where T : unmanaged
    {
        internal UnsafeEventParallelWriter(EventsData<T>* buffer)
        {
            _writer = buffer->AsParallelWriter();
        }

        EventsDataParallelWriter<T> _writer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNoResize(in T value)
        {
            _writer.WriteNoResize(value);
        }
    }
}

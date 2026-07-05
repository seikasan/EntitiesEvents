using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using EntitiesEvents.Internal;

namespace EntitiesEvents
{
    [NativeContainer]
    [NativeContainerIsReadOnly]
    public unsafe struct EventReader<T>
        where T : unmanaged
    {
        public EventReader(in Events<T> events)
        {
            _buffer = events.GetBuffer();
            _eventCounter = _buffer->PrevEventCounter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckGetSecondaryDataPointerAndThrow(events.Safety);
            var ash = events.Safety;
            AtomicSafetyHandle.UseSecondaryVersion(ref ash);
            _safety = ash;
#endif
        }

        [NativeDisableUnsafePtrRestriction] readonly EventsData<T>* _buffer;
        int _eventCounter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private readonly AtomicSafetyHandle _safety;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventsDataIterator<T> Read()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(_safety);
#endif
            var endEventCounter = _buffer->EventCounter;
            var itr = new EventsDataIterator<T>(_buffer, _eventCounter, endEventCounter);
            _eventCounter = endEventCounter;
            return itr;
        }
    }
}

namespace EntitiesEvents.LowLevel.Unsafe
{
    public unsafe struct UnsafeEventReader<T>
        where T : unmanaged
    {
        public UnsafeEventReader(in UnsafeEvents<T> events)
        {
            _buffer = events.Buffer;
            _eventCounter = _buffer->PrevEventCounter;
        }

        [NativeDisableUnsafePtrRestriction] readonly EventsData<T>* _buffer;
        int _eventCounter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventsDataIterator<T> Read()
        {
            var endEventCounter = _buffer->EventCounter;
            var itr = new EventsDataIterator<T>(_buffer, _eventCounter, endEventCounter);
            _eventCounter = endEventCounter;
            return itr;
        }
    }
}
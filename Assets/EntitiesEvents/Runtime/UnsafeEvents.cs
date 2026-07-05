using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using EntitiesEvents.Internal;

namespace EntitiesEvents.LowLevel.Unsafe
{
    public unsafe struct UnsafeEvents<T> : IDisposable
        where T : unmanaged
    {
        public UnsafeEvents(int initialCapacity, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (allocator <= Allocator.None) throw new ArgumentException("Allocator must be Temp, TempJob, Persistent or registered custom allocator", nameof(allocator));
            if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity), "InitialCapacity must be >= 0");
#endif

            var size = UnsafeUtility.SizeOf<EventsData<T>>();
            Buffer = (EventsData<T>*)UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<EventsData<T>>(), allocator, 1);
            UnsafeUtility.MemClear(Buffer, size);

            var data = new EventsData<T>(initialCapacity, allocator);
            UnsafeUtility.CopyStructureToPtr(ref data, Buffer);

            _allocator = allocator;
        }

        [NativeDisableUnsafePtrRestriction] internal EventsData<T>* Buffer;
        readonly Allocator _allocator;

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Buffer != null;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckBuffer();
                return Buffer->Capacity;
            }
        }

        public int CurrentFrameCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckBuffer();
                return Buffer->CurrentFrameCount;
            }
        }

        public int RemainingCurrentFrameCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckBuffer();
                return Buffer->RemainingCurrentFrameCapacity;
            }
        }

        public void EnsureCapacity(int capacity)
        {
            CheckBuffer();
            Buffer->EnsureCapacity(capacity);
        }

        public void EnsureAdditionalCapacity(int additionalCapacity)
        {
            CheckBuffer();
            Buffer->EnsureAdditionalCapacity(additionalCapacity);
        }

        public void Dispose()
        {
            if (Buffer == null) return;
            Buffer->Dispose();
            UnsafeUtility.FreeTracked(Buffer, _allocator);
            Buffer = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update()
        {
            CheckBuffer();
            Buffer->Update();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeEventWriter<T> GetWriter()
        {
            CheckBuffer();
            return new UnsafeEventWriter<T>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeEventReader<T> GetReader()
        {
            CheckBuffer();
            return new UnsafeEventReader<T>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckBuffer()
        {
            if (Buffer == null) throw new InvalidOperationException("The Events container has not been created or has already been disposed.");
        }
    }
}

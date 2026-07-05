using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using EntitiesEvents.LowLevel.Unsafe;
using EntitiesEvents.Internal;
using Unity.Burst;

namespace EntitiesEvents
{
    [NativeContainer]
    public struct Events<T> : IDisposable
        where T : unmanaged
    {
        public Events(int initialCapacity, Allocator allocator)
        {
            _container = new UnsafeEvents<T>(initialCapacity, allocator);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Safety = CollectionHelper.CreateSafetyHandle(allocator);
            CollectionHelper.SetStaticSafetyId<Events<T>>(ref Safety, ref StaticSafetyId.Data); 
            if (UnsafeUtility.IsNativeContainerType<T>()) AtomicSafetyHandle.SetNestedContainer(Safety, true);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(Safety, true);
#endif
        }

        UnsafeEvents<T> _container;

        internal readonly unsafe EventsData<T>* GetBuffer() => _container.Buffer;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle Safety;
        private static readonly SharedStatic<int> StaticSafetyId = SharedStatic<int>.GetOrCreate<Events<T>>();
#endif

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _container.IsCreated;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(Safety);
#endif
            _container.Update();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventWriter<T> GetWriter()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckExistsAndThrow(Safety);
#endif
            return new EventWriter<T>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventReader<T> GetReader()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckExistsAndThrow(Safety);
#endif
            return new EventReader<T>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            CollectionHelper.DisposeSafetyHandle(ref Safety);
#endif
            _container.Dispose();
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EntitiesEvents.Internal
{
    public readonly struct EventInstance<T> where T : unmanaged
    {
        public readonly T value;
        public readonly int id;

        public EventInstance(in T value, int id)
        {
            this.value = value;
            this.id = id;
        }
    }

    public struct EventsData<T> : IDisposable
        where T : unmanaged
    {
        internal UnsafeList<EventInstance<T>> buffer1;
        internal UnsafeList<EventInstance<T>> buffer2;
        internal int eventCounter;
        internal int prevEventCounter;
        bool state;

        internal UnsafeList<EventInstance<T>> GetWriteBuffer() => state ? buffer2 : buffer1;
        internal UnsafeList<EventInstance<T>> GetReadBuffer() => state ? buffer1 : buffer2;

        public EventsData(int capacity, Allocator allocator)
        {
            buffer1 = new UnsafeList<EventInstance<T>>(capacity, allocator);
            buffer2 = new UnsafeList<EventInstance<T>>(capacity, allocator);
            eventCounter = 0;
            prevEventCounter = 0;
            state = false;
        }

        public void Update()
        {
            state = !state;
            if (state) buffer2.Clear();
            else buffer1.Clear();

            prevEventCounter = eventCounter;
        }

        public void Write(in T value)
        {
            if (state) buffer2.Add(new EventInstance<T>(value, eventCounter));
            else buffer1.Add(new EventInstance<T>(value, eventCounter));
            eventCounter = unchecked(eventCounter + 1);
        }

        public void WriteNoResize(in T value)
        {
            var id = unchecked(Interlocked.Increment(ref eventCounter) - 1);
            if (state) buffer2.AsParallelWriter().AddNoResize(new EventInstance<T>(value, id));
            else buffer1.AsParallelWriter().AddNoResize(new EventInstance<T>(value, id));
        }

        public void Dispose()
        {
            if (buffer1.IsCreated) buffer1.Dispose();
            if (buffer2.IsCreated) buffer2.Dispose();
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity <= 0) return;

            var newCapacity = Math.Max(1, Math.Max(buffer1.Capacity, buffer2.Capacity));
            while (newCapacity < capacity) newCapacity <<= 1;

            if (buffer1.Capacity < newCapacity) buffer1.SetCapacity(newCapacity);
            if (buffer2.Capacity < newCapacity) buffer2.SetCapacity(newCapacity);
        }
    }

    public readonly unsafe ref struct EventsDataIterator<T> where T : unmanaged
    {
        public EventsDataIterator(EventsData<T>* buffer, int eventCounter)
        {
            this.buffer = buffer;
            this.eventCounter = eventCounter;
        }
        readonly EventsData<T>* buffer;
        readonly int eventCounter;

        public Enumerator GetEnumerator()
        {
            return new Enumerator(buffer->GetReadBuffer(), buffer->GetWriteBuffer(), eventCounter);
        }

        public struct Enumerator : IEnumerator<T>
        {
            public Enumerator(UnsafeList<EventInstance<T>> buffer1, UnsafeList<EventInstance<T>> buffer2, int eventCounter)
            {
                reader1 = buffer1.AsParallelReader();
                reader2 = buffer2.AsParallelReader();
                this.eventCounter = eventCounter;
                current = default;
                offset = default;
                readFirstReader = default;
            }

            readonly UnsafeList<EventInstance<T>>.ParallelReader reader1;
            readonly UnsafeList<EventInstance<T>>.ParallelReader reader2;
            readonly int eventCounter;
            T current;
            int offset;
            bool readFirstReader;

            public T Current => current;
            object IEnumerator.Current => current;

            public void Dispose() { }

            public bool MoveNext()
            {
                while (true)
                {
                    var reader = readFirstReader ? reader2 : reader1;
                    while (reader.Ptr != null && reader.Length > offset)
                    {
                        ref var instance = ref *(reader.Ptr + offset);
                        offset++;

                        if (IsOlder(instance.id, eventCounter)) continue;

                        current = instance.value;
                        return true;
                    }

                    if (readFirstReader) return false;

                    readFirstReader = true;
                    offset = 0;
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            static bool IsOlder(int id, int threshold)
            {
                return unchecked(id - threshold) < 0;
            }
        }
    }
}
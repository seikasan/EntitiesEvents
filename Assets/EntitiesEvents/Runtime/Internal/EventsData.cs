using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EntitiesEvents.Internal
{
    internal readonly struct EventInstance<T> where T : unmanaged
    {
        public readonly T Value;
        public readonly int ID;

        public EventInstance(in T value, int id)
        {
            Value = value;
            ID = id;
        }
    }

    internal struct EventsData<T> : IDisposable
        where T : unmanaged
    {
        private UnsafeList<EventInstance<T>> _buffer1;
        private UnsafeList<EventInstance<T>> _buffer2;
        internal int EventCounter;
        internal int PrevEventCounter;
        bool _state;

        internal UnsafeList<EventInstance<T>> GetWriteBuffer() => _state ? _buffer2 : _buffer1;
        internal UnsafeList<EventInstance<T>> GetReadBuffer() => _state ? _buffer1 : _buffer2;

        public int Capacity => Math.Min(_buffer1.Capacity, _buffer2.Capacity);
        public int CurrentFrameCount => GetWriteBuffer().Length;
        public int RemainingCurrentFrameCapacity => Math.Max(0, Capacity - CurrentFrameCount);

        public EventsData(int capacity, Allocator allocator)
        {
            _buffer1 = new UnsafeList<EventInstance<T>>(capacity, allocator);
            _buffer2 = new UnsafeList<EventInstance<T>>(capacity, allocator);
            EventCounter = 0;
            PrevEventCounter = 0;
            _state = false;
        }

        public void Update()
        {
            _state = !_state;
            if (_state) _buffer2.Clear();
            else _buffer1.Clear();

            PrevEventCounter = EventCounter;
        }

        public void Write(in T value)
        {
            int id = EventCounter;
            if (_state) _buffer2.Add(new EventInstance<T>(value, id));
            else _buffer1.Add(new EventInstance<T>(value, id));
            EventCounter = unchecked(id + 1);
        }

        public void WriteNoResize(in T value)
        {
            AsParallelWriter().WriteNoResize(value);
        }

        public unsafe EventsDataParallelWriter<T> AsParallelWriter()
        {
            return new EventsDataParallelWriter<T>(
                GetWriteBuffer().AsParallelWriter(),
                (int*)UnsafeUtility.AddressOf(ref EventCounter));
        }

        public void Dispose()
        {
            if (_buffer1.IsCreated) _buffer1.Dispose();
            if (_buffer2.IsCreated) _buffer2.Dispose();
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity <= 0) return;

            int newCapacity = Math.Max(_buffer1.Capacity, _buffer2.Capacity);
            if (newCapacity >= capacity) return;

            newCapacity = Math.Max(1, newCapacity);
            while (newCapacity < capacity)
            {
                if (newCapacity > int.MaxValue / 2)
                {
                    newCapacity = capacity;
                    break;
                }

                newCapacity <<= 1;
            }

            if (_buffer1.Capacity < newCapacity) _buffer1.SetCapacity(newCapacity);
            if (_buffer2.Capacity < newCapacity) _buffer2.SetCapacity(newCapacity);
        }

        public void EnsureAdditionalCapacity(int additionalCapacity)
        {
            if (additionalCapacity <= 0) return;
            EnsureCapacity(CurrentFrameCount + additionalCapacity);
        }
    }

    internal unsafe struct EventsDataParallelWriter<T> where T : unmanaged
    {
        UnsafeList<EventInstance<T>>.ParallelWriter _writer;
        [NativeDisableUnsafePtrRestriction] readonly int* _eventCounter;

        public EventsDataParallelWriter(UnsafeList<EventInstance<T>>.ParallelWriter writer, int* eventCounter)
        {
            _writer = writer;
            _eventCounter = eventCounter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNoResize(in T value)
        {
            var id = unchecked(Interlocked.Increment(ref UnsafeUtility.AsRef<int>(_eventCounter)) - 1);
            _writer.AddNoResize(new EventInstance<T>(value, id));
        }
    }

    public readonly unsafe ref struct EventsDataIterator<T> where T : unmanaged
    {
        internal EventsDataIterator(EventsData<T>* buffer, int startEventCounter, int endEventCounter)
        {
            _buffer = buffer;
            _startEventCounter = startEventCounter;
            _endEventCounter = endEventCounter;
        }

        readonly EventsData<T>* _buffer;
        readonly int _startEventCounter;
        readonly int _endEventCounter;

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_buffer->GetReadBuffer(), _buffer->GetWriteBuffer(), _startEventCounter, _endEventCounter);
        }

        public struct Enumerator
        {
            internal Enumerator(UnsafeList<EventInstance<T>> buffer1, UnsafeList<EventInstance<T>> buffer2, int startEventCounter, int endEventCounter)
            {
                _reader1 = buffer1.AsParallelReader();
                _reader2 = buffer2.AsParallelReader();
                _startEventCounter = startEventCounter;
                _endEventCounter = endEventCounter;
                _current = default;
                _offset = 0;
                _readFirstReader = false;
            }

            readonly UnsafeList<EventInstance<T>>.ParallelReader _reader1;
            readonly UnsafeList<EventInstance<T>>.ParallelReader _reader2;
            readonly int _startEventCounter;
            readonly int _endEventCounter;
            T _current;
            int _offset;
            bool _readFirstReader;

            public T Current => _current;

            public bool MoveNext()
            {
                while (true)
                {
                    var reader = _readFirstReader ? _reader2 : _reader1;
                    while (reader.Ptr != null && reader.Length > _offset)
                    {
                        ref var instance = ref *(reader.Ptr + _offset);
                        _offset++;

                        if (!IsInReadRange(instance.ID, _startEventCounter, _endEventCounter)) continue;

                        _current = instance.Value;
                        return true;
                    }

                    if (_readFirstReader) return false;

                    _readFirstReader = true;
                    _offset = 0;
                }
            }

            public void Dispose() { }

            static bool IsInReadRange(int id, int start, int end)
            {
                return unchecked(id - start) >= 0 && unchecked(end - id) > 0;
            }
        }
    }
}

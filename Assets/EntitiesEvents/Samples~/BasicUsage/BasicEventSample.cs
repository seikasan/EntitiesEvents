using EntitiesEvents;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[assembly: RegisterEvent(typeof(EntitiesEvents.Samples.BasicUsage.MyEvent))]

namespace EntitiesEvents.Samples.BasicUsage
{
    public struct MyEvent
    {
        public int Value;
    }

    [BurstCompile]
    public partial struct WriteEventSystem : ISystem
    {
        EventWriter<MyEvent> _writer;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _writer = state.GetEventWriter<MyEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _writer.Write(new MyEvent { Value = 1 });
        }
    }

    public partial struct ReadEventSystem : ISystem
    {
        EventReader<MyEvent> _reader;

        public void OnCreate(ref SystemState state)
        {
            _reader = state.GetEventReader<MyEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var eventData in _reader.Read())
            {
                Debug.Log($"Received MyEvent: {eventData.Value}");
            }
        }
    }
}

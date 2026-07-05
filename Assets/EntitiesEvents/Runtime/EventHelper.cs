using EntitiesEvents.Internal;
using Unity.Collections;
using Unity.Entities;

namespace EntitiesEvents
{
    public static class EventHelper
    {
        public static EventWriter<T> GetEventWriter<T>(this ref SystemState state)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(ref state).Events.GetWriter();
        }

        public static EventWriter<T> GetEventWriter<T>(this EntityManager entityManager)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(entityManager).Events.GetWriter();
        }

        public static EventParallelWriter<T> GetEventParallelWriter<T>(this ref SystemState state)
            where T : unmanaged
        {
            return GetEventWriter<T>(ref state).AsParallelWriter();
        }

        public static EventParallelWriter<T> GetEventParallelWriter<T>(this EntityManager entityManager)
            where T : unmanaged
        {
            return entityManager.GetEventWriter<T>().AsParallelWriter();
        }

        public static EventReader<T> GetEventReader<T>(this ref SystemState state)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(ref state).Events.GetReader();
        }

        public static EventReader<T> GetEventReader<T>(this EntityManager entityManager)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(entityManager).Events.GetReader();
        }

        public static unsafe void EnsureEventCapacity<T>(this ref SystemState state, int capacity)
            where T : unmanaged
        {
            var events = GetOrCreateSingleton<T>(ref state).Events;
            events.GetBuffer()->EnsureCapacity(capacity);
        }

        public static unsafe void EnsureEventCapacity<T>(this EntityManager entityManager, int capacity)
            where T : unmanaged
        {
            var events = GetOrCreateSingleton<T>(entityManager).Events;
            events.GetBuffer()->EnsureCapacity(capacity);
        }

        static EventSingleton<T> GetOrCreateSingleton<T>(EntityManager entityManager)
            where T : unmanaged
        {
            using var query = entityManager.CreateEntityQuery(ComponentType.ReadWrite<EventSingleton<T>>());
            if (query.TryGetSingleton<EventSingleton<T>>(out var singleton)) return singleton;

            singleton = new EventSingleton<T>
            {
                Events = new Events<T>(512, Allocator.Persistent)
            };
            entityManager.CreateSingleton(singleton);
            return singleton;
        }

        static EventSingleton<T> GetOrCreateSingleton<T>(ref SystemState state)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(state.EntityManager);
        }
    }
}

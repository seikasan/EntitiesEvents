using EntitiesEvents.Internal;
using Unity.Collections;
using Unity.Entities;

namespace EntitiesEvents
{
    public static class EventHelper
    {
        public const int DefaultInitialCapacity = 512;

        public static EventWriter<T> GetEventWriter<T>(this ref SystemState state)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(ref state).Events.GetWriter();
        }

        public static EventWriter<T> GetEventWriter<T>(this ref SystemState state, int initialCapacity)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(ref state, initialCapacity).Events.GetWriter();
        }

        public static EventWriter<T> GetEventWriter<T>(this EntityManager entityManager)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(entityManager).Events.GetWriter();
        }

        public static EventWriter<T> GetEventWriter<T>(this EntityManager entityManager, int initialCapacity)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(entityManager, initialCapacity).Events.GetWriter();
        }

        public static EventParallelWriter<T> GetEventParallelWriter<T>(this ref SystemState state)
            where T : unmanaged
        {
            return GetEventWriter<T>(ref state).AsParallelWriter();
        }

        public static EventParallelWriter<T> GetEventParallelWriter<T>(this ref SystemState state, int initialCapacity)
            where T : unmanaged
        {
            return GetEventWriter<T>(ref state, initialCapacity).AsParallelWriter();
        }

        public static EventParallelWriter<T> GetEventParallelWriter<T>(this EntityManager entityManager)
            where T : unmanaged
        {
            return entityManager.GetEventWriter<T>().AsParallelWriter();
        }

        public static EventParallelWriter<T> GetEventParallelWriter<T>(this EntityManager entityManager, int initialCapacity)
            where T : unmanaged
        {
            return entityManager.GetEventWriter<T>(initialCapacity).AsParallelWriter();
        }

        public static EventReader<T> GetEventReader<T>(this ref SystemState state)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(ref state).Events.GetReader();
        }

        public static EventReader<T> GetEventReader<T>(this ref SystemState state, int initialCapacity)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(ref state, initialCapacity).Events.GetReader();
        }

        public static EventReader<T> GetEventReader<T>(this EntityManager entityManager)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(entityManager).Events.GetReader();
        }

        public static EventReader<T> GetEventReader<T>(this EntityManager entityManager, int initialCapacity)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(entityManager, initialCapacity).Events.GetReader();
        }

        public static void EnsureEventCapacity<T>(this ref SystemState state, int capacity)
            where T : unmanaged
        {
            GetOrCreateSingleton<T>(ref state).Events.EnsureCapacity(capacity);
        }

        public static void EnsureEventCapacity<T>(this EntityManager entityManager, int capacity)
            where T : unmanaged
        {
            GetOrCreateSingleton<T>(entityManager).Events.EnsureCapacity(capacity);
        }

        public static void EnsureAdditionalEventCapacity<T>(this ref SystemState state, int additionalCapacity)
            where T : unmanaged
        {
            GetOrCreateSingleton<T>(ref state).Events.EnsureAdditionalCapacity(additionalCapacity);
        }

        public static void EnsureAdditionalEventCapacity<T>(this EntityManager entityManager, int additionalCapacity)
            where T : unmanaged
        {
            GetOrCreateSingleton<T>(entityManager).Events.EnsureAdditionalCapacity(additionalCapacity);
        }

        static EventSingleton<T> GetOrCreateSingleton<T>(EntityManager entityManager, int initialCapacity = DefaultInitialCapacity)
            where T : unmanaged
        {
            using var query = entityManager.CreateEntityQuery(ComponentType.ReadWrite<EventSingleton<T>>());
            if (query.TryGetSingleton<EventSingleton<T>>(out var singleton))
            {
                singleton.Events.EnsureCapacity(initialCapacity);
                return singleton;
            }

            singleton = new EventSingleton<T>
            {
                Events = new Events<T>(initialCapacity, Allocator.Persistent)
            };
            entityManager.CreateSingleton(singleton);
            return singleton;
        }

        static EventSingleton<T> GetOrCreateSingleton<T>(ref SystemState state, int initialCapacity = DefaultInitialCapacity)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(state.EntityManager, initialCapacity);
        }
    }
}

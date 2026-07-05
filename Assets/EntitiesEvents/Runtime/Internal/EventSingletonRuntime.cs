using Unity.Entities;

namespace EntitiesEvents.Internal
{
    public static class EventSingletonRuntime
    {
        public static void Update<T>(EntityQuery query)
            where T : unmanaged
        {
            var singleton = query.GetSingleton<EventSingleton<T>>();
            singleton.Events.Update();
        }

        public static void Dispose<T>(EntityQuery query)
            where T : unmanaged
        {
            if (!query.TryGetSingleton<EventSingleton<T>>(out var singleton)) return;

            singleton.Events.Dispose();
        }
    }
}
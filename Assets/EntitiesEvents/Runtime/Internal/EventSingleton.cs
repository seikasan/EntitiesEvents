using Unity.Entities;

namespace EntitiesEvents.Internal
{
    public struct EventSingleton<T> : IComponentData
        where T : unmanaged
    {
        internal Events<T> Events;
    }
}
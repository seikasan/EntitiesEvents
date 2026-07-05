# Entities Events

Entities Events is a Unity 6+ ECS event helper package. Register event types with `[assembly: RegisterEvent(typeof(T))]`, cache `EventWriter<T>` and `EventReader<T>` in `OnCreate`, and read events every frame because each event lives for the sending frame plus one additional frame. `EventReader<T>.Read()` returns a snapshot bounded at call time.

For parallel jobs, call `EnsureEventCapacity<T>` or `EventWriter<T>.EnsureCapacity` before scheduling the job. `EventParallelWriter<T>.WriteNoResize` intentionally never grows the backing buffer.

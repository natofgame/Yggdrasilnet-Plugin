using System;

namespace Yggdrasil.Plugin.Event;

[AttributeUsage(AttributeTargets.Method)]
public sealed class SubscribeEventAttribute : Attribute {
    public EventPriority Priority { get; }

    public SubscribeEventAttribute(EventPriority priority = EventPriority.Normal) => Priority = priority;
}

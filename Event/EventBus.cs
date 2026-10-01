using System;
using System.Collections.Generic;
using System.Reflection;

namespace Yggdrasil.Plugin.Event;

public sealed class EventBus : IEventBus {
    private sealed class Subscription {
        public readonly object Listener;
        public readonly EventPriority Priority;
        public readonly Action<PluginEvent> Invoke;

        public Subscription(object listener, EventPriority priority, Action<PluginEvent> invoke) {
            Listener = listener;
            Priority = priority;
            Invoke = invoke;
        }
    }

    private readonly Dictionary<Type, List<Subscription>> _subscriptions = new();
    private readonly Func<PluginContext> _contextProvider;
    private readonly Action<Exception> _onError;

    public EventBus(Func<PluginContext> contextProvider, Action<Exception> onError) {
        _contextProvider = contextProvider;
        _onError = onError;
    }

    public void Register(object listener) {
        var type = listener.GetType();
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods) {
            var attr = method.GetCustomAttribute<SubscribeEventAttribute>();
            if (attr == null) {
                continue;
            }

            var parameters = method.GetParameters();
            if (parameters.Length != 1 || !typeof(PluginEvent).IsAssignableFrom(parameters[0].ParameterType)) {
                _onError(new InvalidOperationException(
                    $"{type.Name}.{method.Name} : invalid signature for [SubscribeEvent] " +
                    "(expected: void Method(XxxEvent e))"));
                continue;
            }

            var eventType = parameters[0].ParameterType;
            var invoker = BuildInvoker(listener, method, eventType);

            if (!_subscriptions.TryGetValue(eventType, out var list)) {
                list = new List<Subscription>();
                _subscriptions[eventType] = list;
            }

            list.Add(new Subscription(listener, attr.Priority, invoker));
            list.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }
    }

    public void Unregister(object listener) {
        foreach (var list in _subscriptions.Values) {
            list.RemoveAll(s => ReferenceEquals(s.Listener, listener));
        }
    }

    public void Post(PluginEvent evt) {
        evt.Context = _contextProvider();

        if (!_subscriptions.TryGetValue(evt.GetType(), out var list) || list.Count == 0) {
            return;
        }

        var snapshot = list.ToArray();
        for (var i = 0; i < snapshot.Length; i++) {
            try {
                snapshot[i].Invoke(evt);
            } catch (Exception ex) {
                _onError(ex);
            }
        }
    }

    private static Action<PluginEvent> BuildInvoker(object listener, MethodInfo method, Type eventType) {
        var factory = typeof(EventBus)
            .GetMethod(nameof(CreateTypedInvoker), BindingFlags.NonPublic | BindingFlags.Static)
            ?.MakeGenericMethod(eventType);

        if (factory == null) {
            throw new InvalidOperationException("Unable to create event invoker factory.");
        }

        return (Action<PluginEvent>)factory.Invoke(null, new object[] { listener, method })!;
    }

    private static Action<PluginEvent> CreateTypedInvoker<TEvent>(object listener, MethodInfo method)
        where TEvent : PluginEvent {
        var typed = (Action<TEvent>)Delegate.CreateDelegate(typeof(Action<TEvent>), listener, method);
        return evt => typed((TEvent)evt);
    }
}

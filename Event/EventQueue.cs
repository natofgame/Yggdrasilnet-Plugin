using System.Collections.Generic;

namespace Yggdrasil.Plugin.Event;

internal sealed class EventQueue {
    private readonly Queue<PluginEvent> _queue = new();
    private bool _flushing;

    public void Enqueue(PluginEvent evt) => _queue.Enqueue(evt);

    public void Flush(EventBus bus) {
        if (_flushing) {
            return;
        }

        _flushing = true;
        try {
            while (_queue.Count > 0) {
                bus.Post(_queue.Dequeue());
            }
        } finally {
            _flushing = false;
        }
    }
}

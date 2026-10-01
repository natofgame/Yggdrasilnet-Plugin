using System;

namespace Yggdrasil.Plugin.Event;

public sealed class PluginContext {
    private readonly Action<string> _log;

    public string ServerName { get; }
    public string ServerVersion { get; }
    public IEventBus Events { get; }

    public PluginContext(string serverName, string serverVersion, IEventBus events, Action<string> log) {
        ServerName = serverName;
        ServerVersion = serverVersion;
        Events = events;
        _log = log;
    }

    public void Log(string message) => _log(message);
}

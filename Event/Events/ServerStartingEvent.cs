namespace Yggdrasil.Plugin.Event.Events;

public sealed class ServerStartingEvent : PluginEvent {
    public int Port { get; }
    public int TickRate { get; }

    public ServerStartingEvent(int port, int tickRate) {
        Port = port;
        TickRate = tickRate;
    }
}

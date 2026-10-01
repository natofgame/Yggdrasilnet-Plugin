namespace Yggdrasil.Plugin.Event;

public abstract class PluginEvent {
    public PluginContext Context { get; internal set; } = null!;
}

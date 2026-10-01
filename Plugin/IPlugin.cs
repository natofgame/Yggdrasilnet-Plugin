namespace Yggdrasil.Plugin.Plugin;

public interface IPlugin {
    string Id { get; }
    void OnLoad(Event.PluginContext context);
    void OnEnable();
    void OnDisable();
}

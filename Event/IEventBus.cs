namespace Yggdrasil.Plugin.Event;

public interface IEventBus {
    void Register(object listener);
    void Unregister(object listener);
}

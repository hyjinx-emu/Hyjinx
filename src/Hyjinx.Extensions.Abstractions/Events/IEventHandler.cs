namespace Hyjinx.Extensions.Abstractions.Events;

public interface IEventHandler<T> where T : Event
{
    void Handle(T evnt);
}
using Hyjinx.Extensions.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hyjinx.Ava.UI.Messaging;

/// <summary>
/// A mechanism used for signaling events which occur during the application lifetime.
/// </summary>
internal abstract class Messenger
{
    /// <summary>
    /// The default messenger.
    /// </summary>
    /// <param name="applicationServices">The service provider containing application services.</param>
    public class Default(IServiceProvider applicationServices) : Messenger
    {
        public override void Send<T>(T evnt)
        {
            var handlers = applicationServices.GetServices<IEventHandler<T>>();

            foreach (var handler in handlers)
            {
                handler.Handle(evnt);
            }
        }
    }

    /// <summary>
    /// Sends the event.
    /// </summary>
    /// <typeparam name="T">The event type.</typeparam>
    /// <param name="evnt">The event data.</param>
    public abstract void Send<T>(T evnt) where T : Event;
}
using System;
using System.Collections.Generic;
using System.Linq;
using SGS29.Utilities;

namespace Events
{
    public class EventManager : MonoSingleton<EventManager>
    {
        private readonly Dictionary<Type, List<object>> EventListeners = new();

        public void RegisterListener<T>(Action<T> listener) where T: IEvent
        {
            var type = typeof(T);

            Action addAction = () =>
            {
                if (!EventListeners.ContainsKey(type))
                {
                    EventListeners[type] = new List<object>();
                }

                EventListeners[type].Add(listener);
            };

            addAction.Invoke();
        }

        public void UnregisterListener<T>(Action<T> listener) where T: IEvent
        {
            var type = typeof(T);

            Action removeAction = () =>
            {
                if (EventListeners.TryGetValue(type, out var eventListener))
                {
                    eventListener?.Remove(listener);
                }
            };
            
            removeAction.Invoke();
        }

        public void DispatchEvent<T>(T eventToDispatch) where T: IEvent
        {
            var eventType = eventToDispatch.GetType();
            var handlers = EventListeners.Where(kvp => kvp.Key.IsAssignableFrom(eventType));

            foreach (var handler in handlers)
            {
                handler.Value.ForEach(e => ((Action<T>)e).Invoke(eventToDispatch));
            }
        }
    }
}
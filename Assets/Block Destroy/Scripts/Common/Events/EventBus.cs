using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central event bus for game-wide event communication.
/// Uses the Singleton pattern to provide global access.
/// </summary>
public class EventBus : Singleton<EventBus>
{
    private Dictionary<Type, List<Delegate>> eventListeners = new Dictionary<Type, List<Delegate>>();

    /// <summary>
    /// Subscribe to an event type.
    /// </summary>
    /// <typeparam name="T">Event type derived from GameEvent</typeparam>
    /// <param name="listener">Callback to invoke when event is published</param>
    public void Subscribe<T>(Action<T> listener) where T : GameEvent
    {
        Type eventType = typeof(T);

        if (!eventListeners.ContainsKey(eventType))
        {
            eventListeners[eventType] = new List<Delegate>();
        }

        if (!eventListeners[eventType].Contains(listener))
        {
            eventListeners[eventType].Add(listener);
        }
    }

    /// <summary>
    /// Unsubscribe from an event type.
    /// </summary>
    /// <typeparam name="T">Event type derived from GameEvent</typeparam>
    /// <param name="listener">Callback to remove</param>
    public void Unsubscribe<T>(Action<T> listener) where T : GameEvent
    {
        Type eventType = typeof(T);

        if (eventListeners.ContainsKey(eventType))
        {
            eventListeners[eventType].Remove(listener);
        }
    }

    /// <summary>
    /// Publish an event to all subscribers.
    /// </summary>
    /// <typeparam name="T">Event type derived from GameEvent</typeparam>
    /// <param name="gameEvent">Event instance to publish</param>
    public void Publish<T>(T gameEvent) where T : GameEvent
    {
        Type eventType = typeof(T);

        if (eventListeners.ContainsKey(eventType))
        {
            // Create a copy to avoid issues if listeners modify the list during iteration
            List<Delegate> listeners = new List<Delegate>(eventListeners[eventType]);

            foreach (var listener in listeners)
            {
                try
                {
                    (listener as Action<T>)?.Invoke(gameEvent);
                }
                catch (Exception e)
                {
                }
            }
        }
    }

    /// <summary>
    /// Clear all event subscriptions.
    /// Useful when changing scenes to prevent memory leaks.
    /// </summary>
    public void Clear()
    {
        eventListeners.Clear();
    }

    /// <summary>
    /// Clear subscriptions for a specific event type.
    /// </summary>
    /// <typeparam name="T">Event type to clear</typeparam>
    public void Clear<T>() where T : GameEvent
    {
        Type eventType = typeof(T);
        if (eventListeners.ContainsKey(eventType))
        {
            eventListeners[eventType].Clear();
        }
    }

    /// <summary>
    /// Get the number of subscribers for a specific event type.
    /// Useful for debugging.
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <returns>Number of subscribers</returns>
    public int GetSubscriberCount<T>() where T : GameEvent
    {
        Type eventType = typeof(T);
        if (eventListeners.ContainsKey(eventType))
        {
            return eventListeners[eventType].Count;
        }
        return 0;
    }

    /// <summary>
    /// Check if there are any subscribers for a specific event type.
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <returns>True if there are subscribers</returns>
    public bool HasSubscribers<T>() where T : GameEvent
    {
        Type eventType = typeof(T);
        return eventListeners.ContainsKey(eventType) && eventListeners[eventType].Count > 0;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Debug method to log all active subscriptions.
    /// Only available in Unity Editor.
    /// </summary>
    public void LogAllSubscriptions()
    {
        foreach (var kvp in eventListeners)
        {
        }
    }
#endif
}

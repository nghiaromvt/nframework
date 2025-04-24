using System;
using System.Collections.Generic;

namespace NFramework
{
    public class EventDispatcher
    {
        private static readonly Dictionary<string, Action<BaseEventData>> _eventDic = new();

        public static void Register(string eventId, Action<BaseEventData> action)
        {
            if (!_eventDic.ContainsKey(eventId))
                _eventDic.Add(eventId, new Action<BaseEventData>(eventData => { }));

            _eventDic[eventId] += action;
        }

        public static void Unregister(string eventId, Action<BaseEventData> action)
        {
            if (_eventDic.ContainsKey(eventId))
                _eventDic[eventId] -= action;
        }

        public static void ClearEvent(string eventId)
        {
            if (_eventDic.ContainsKey(eventId))
                _eventDic.Remove(eventId);
        }

        public static void TriggerEvent(string eventId, BaseEventData eventData)
        {
            if (_eventDic.TryGetValue(eventId, out var value))
                value?.Invoke(eventData);
            else
                NLogger.LogError($"[{nameof(EventDispatcher)}] Not found event to trigger - eventId:{eventId}");
        }
    }
    
    [Serializable]
    public class BaseEventData
    {
    }
}
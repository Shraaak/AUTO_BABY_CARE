using System;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// 事件中心
/// </summary>
public class EventCenter
{
    public static EventCenter _instance;
    public static EventCenter Instance
    {
        get
        {
            if(_instance == null)
                _instance = new EventCenter();
            return _instance;
        }
    }

    private EventCenter(){}

    //存储所有事件
    private Dictionary<string, IEventInfo> _eventDic = new Dictionary<string, IEventInfo>();

    /// <summary>
    /// 添加无参事件的监听
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void AddEventListener(string name, Action action)
    {
        //检查是否已存在
        if (_eventDic.ContainsKey(name))
        {
            (_eventDic[name] as EventInfo).actions += action;
        }
        else
        {
            _eventDic.Add(name, new EventInfo(action));
        }
    }

    /// <summary>
    /// 移除无参事件的监听
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void RemoveEventListener(string name, Action action)
    {
        if (_eventDic.ContainsKey(name))
        {
            (_eventDic[name] as EventInfo).actions -= action;
        }
    }

    public void AddEventListener<T>(string name, Action<T> action)
    {
        if (_eventDic.ContainsKey(name))
        {
            (_eventDic[name] as EventInfo<T>).actions += action;
        }
        else
        {
            _eventDic.Add(name, new EventInfo<T>(action));
        }
    }

    /// <summary>
    /// 触发事件
    /// </summary>
    /// <param name="name"></param>
    public void EventTrigger(string name)
    {
        if (_eventDic.ContainsKey(name))
        {
            EventInfo eventInfo = _eventDic[name] as EventInfo;

            if(eventInfo.actions!=null)
                eventInfo.actions.Invoke();
        }
    }

    public void EventTrigger<T>(string name, T arg)
    {
        if (_eventDic.ContainsKey(name))
        {
            EventInfo<T> eventInfo = _eventDic[name] as EventInfo<T>;
            if (eventInfo.actions != null)
                eventInfo.actions.Invoke(arg);
        }
    }

    public void AddEventListener<T1, T2>(string name, Action<T1, T2> action)
    {
        if (_eventDic.ContainsKey(name))
        {
            (_eventDic[name] as EventInfo<T1, T2>).actions += action;
        }
        else
        {
            _eventDic.Add(name, new EventInfo<T1, T2>(action));
        }
    }

    public void RemoveEventListener<T1, T2>(string name, Action<T1, T2> action)
    {
        if (_eventDic.ContainsKey(name))
        {
            (_eventDic[name] as EventInfo<T1, T2>).actions -= action;
        }
    }

    public void EventTrigger<T1, T2>(string name, T1 t1, T2 t2)
    {
        if (_eventDic.ContainsKey(name))
        {
            EventInfo<T1, T2> eventInfo = _eventDic[name] as EventInfo<T1, T2>;
            if (eventInfo.actions != null)
                eventInfo.actions.Invoke(t1, t2);
        }
    }

    public void Clear()
    {
        _eventDic.Clear();
    }
}

public interface IEventInfo {}

public class EventInfo : IEventInfo
{
    public Action actions; 

    public EventInfo(Action action)
    {
        actions += action;        
    }
}

public class EventInfo<T> : IEventInfo
{
    public Action<T> actions; // 存储带参委托

    public EventInfo(Action<T> action)
    {
        actions += action;
    }
}

public class EventInfo<T1, T2> : IEventInfo
{
    public Action<T1, T2> actions;
    public EventInfo(Action<T1, T2> action)
    {
        actions += action;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTimer
{
    private class TimedAction
    {
        public Action handler = null;
        public float timer = 0f;
        public float period = 0f;
        public TimedAction(Action handler, float period)
        {
            this.handler = handler;
            this.period = period;
        }
    }
    List<TimedAction> actions = new List<TimedAction>();

    float prevTime = -1f;
    public void Update(float time)
    {
        if (prevTime < 0)
        {
            prevTime = time;
        }
        float deltaTime = time - prevTime;
        foreach (TimedAction action in actions)
        {
            action.timer += deltaTime;
            if (action.timer >= action.period)
            {
                action.timer -= action.period;
                action.handler.Invoke();
            }
        }
        prevTime = time;
    }

    // add timer handler Action
    public void Schedule(Action handler, float period)
    {
        if (actions.Exists(x => x.handler == handler))
        {
            throw new InvalidOperationException("Handler is already registered.");
        }
        actions.Add(new TimedAction(handler, period));
    }

    // remove action
    public void Deschedule(Action handler)
    {
        if (!actions.Exists(x => x.handler == handler))
        {
            throw new InvalidOperationException("Handler does not exist.");
        }
        actions.RemoveAll(x => x.handler == handler);
    }
}
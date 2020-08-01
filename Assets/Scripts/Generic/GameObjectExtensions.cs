using UnityEngine;
using System.Collections.Generic;

public static class GameObjectExtensions
{
    // http://answers.unity.com/answers/1433854/view.html
    public static T[] GetComponentsInDirectChildren<T>(this GameObject gameObject) where T : Component
    {
        List<T> components = new List<T>();
        for (int i = 0; i < gameObject.transform.childCount; ++i)
        {
            T component = gameObject.transform.GetChild(i).GetComponent<T>();
            if (component != null)
                components.Add(component);
        }

        return components.ToArray();
    }
}
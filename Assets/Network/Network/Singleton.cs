using UnityEngine;
public class Singleton<T> : MonoBehaviour where T : Component {
    private static T instance = null;

    public static T Instance {
        get {
            if (instance == null)
                instance = FindObjectOfType<T>();
            return instance;
        }
    }

}
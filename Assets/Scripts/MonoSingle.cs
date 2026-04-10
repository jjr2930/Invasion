using UnityEngine;


public class MonoSingle<T> : MonoBehaviour
    where T : MonoSingle<T>
{
    static T instance = null;
    public static T Instance
    {
        get
        {
            if (null == instance)
            {

                var go = new GameObject(typeof(T).Name);
                instance = go.AddComponent<T>();
                DontDestroyOnLoad(go);
            }

            return instance;
        }
    }
}
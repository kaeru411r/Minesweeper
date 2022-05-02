using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonoBehaviourを継承しつつSingletonを実現したいときに継承する
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>シングルトンクラスのインスタンス</summary>
    private static T instance;

    /// <summary>シングルトンクラスのインスタンス</summary>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType(typeof(T));

                if (instance == null)
                {
                    Debug.LogError(typeof(T) + "is nothing");
                }
            }
            return instance;
        }
    }

    protected void Awake()
    {
        CheckInstance();
    }

    /// <summary>このインスタンスがSingleton化されたインスタンスか否かを返す
    /// もし否ならこのインスタンスを破棄する</summary>
    /// <returns>true シングルトン化されたインスタンス : false 二つ目以降のインスタンス</returns>
    protected bool CheckInstance()
    {
        if (this == Instance)
        {
            return true;
        }
        Destroy(this);
        return false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonScript<T> : MonoBehaviour where T : MonoBehaviour
{
    //permite que vc acesse outras classes globalmente sem precisar criar variáveis específicas
    private static T instance;

    public static T Instance
    {
        get
        {
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

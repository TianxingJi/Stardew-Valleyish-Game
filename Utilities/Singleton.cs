using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Define a generic class Singleton that extends MonoBehaviour.
// T is a type parameter constrained to be a subclass of Singleton<T>.
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    // Static variable instance which will hold the singleton instance.
    private static T instance;

    // Public property to access the instance. It's read-only outside the class.
    public static T Instance
    {
        get => instance; // Return the instance
    }

    // Awake is called when the script instance is being loaded.
    protected virtual void Awake()
    {
        // Check if instance already exists
        if (instance != null)
        {
            // If instance exists and it's not this instance, destroy this to enforce singleton property
            Destroy(gameObject);
        }
        else
        {
            // If no instance exists, assign this object to instance
            instance = (T)this;
        }
    }

    // OnDestroy will be called when the object is being destroyed.
    protected virtual void OnDestroy()
    {
        // When the singleton instance is destroyed, clean up the instance variable.
        if (instance == this)
        {
            instance = null;
        }
    }

}

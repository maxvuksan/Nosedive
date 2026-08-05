using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A container to hold spawned gameobjects which should be removed when gameplay is stopped (e.g. load scene mode is entered) 
/// </summary>
public static class TemporaryGameobjectContainer{

    private static List<GameObject> s_gameObjects = new();

    /// <summary>
    /// Clears the container, destroying the gameobjects
    /// </summary>
    public static void Clear()
    {
        foreach(var entry in s_gameObjects)
        {
            Object.Destroy(entry);
        }
        
        s_gameObjects.Clear();
    }

    /// <summary>
    /// Registers an object in the container, when .Clear() is called all registered objects will be deleted
    /// </summary>
    /// <param name="">The gameobject to add</param>
    public static void Register(GameObject newObject)
    {
        s_gameObjects.Add(newObject);
    }

}
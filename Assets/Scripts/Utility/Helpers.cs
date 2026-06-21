using System;
using UnityEngine;

public class Helpers : MonoBehaviour
{
    private static Helpers _singleton;
    public static Helpers Singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindObjectOfType<Helpers>();
            }
            return _singleton;
        }
    }

    public bool DebugMode;
    public LayerMask GroundLayerMask;
    public Material BezierCurveWireMaterial;

    [Header("UI")]
    public Color UiIdleColour;
    public Color UiSelectedColour;
    public Color UiDisabledColour;
    public float UiJoltStrength = 4f;
    public float UiJoltSpeed = 1f;
    public float UiHorizontalJoltStrength = 4f;
    public float UiHorizontalJoltSpeed = 1f;
    public string UiBlipSubmitSoundLabel = "UiBlip_Submit";
    public string UiBlipUpSoundLabel = "UiBlip_Up";
    public string UiBlipDownSoundLabel = "UiBlip_Down";
    
    void Awake()
    {
        Application.targetFrameRate = 300;
        QualitySettings.vSyncCount = 0;
    }


    /// <summary>
    /// A utility function for creating and enforcing Singleton behaviour on a class
    /// </summary>
    /// <typeparam name="T">The class type to create a singleton from</typeparam>
    /// <param name="Singleton">The Singleton static variable</param>
    /// <param name="callingClass">The instance of the Singleton we wish to promote</param>
    public static void CreateSingleton<T>(ref T Singleton, T callingClass) where T : MonoBehaviour
    {
        if (Singleton != null)
        {
            Debug.LogWarning("Could not create Singleton (" + Singleton.name + ") because another instance already exists");
            Destroy(Singleton.gameObject);
            return;
        }

        Singleton = callingClass;
        DontDestroyOnLoad(Singleton.gameObject);
    }

    public static float EaseInOutQuint(float t)
    {
        return t < 0.5f
            ? 16f * t * t * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 5f) / 2f;
    }

    /// <summary>
    /// Calls .SetActive() on every element of an array
    /// </summary>
    public static void SetActiveGameObjectArray(GameObject[] array, bool state)
    {
        for(int i = 0; i < array.Length; i++)
        {
            array[i].SetActive(state);
        }
    }

    public static void SetActiveMonoBehaviourArray(MonoBehaviour[] array, bool state)
    {
        for(int i = 0; i < array.Length; i++)
        {
            array[i].gameObject.SetActive(state);
        }
    }

    /// <summary>
    /// Invokes an action, catching an exceptions raised by subscribed events
    /// </summary>
    public static void SafeInvoke(Action action, string functionLabel)
    {
        if (action == null){
            return;
        }

        foreach (var subscriber in action.GetInvocationList())
        {
            // Skip if target is a disabled MonoBehaviour
            if (subscriber.Target is MonoBehaviour behaviour && !behaviour.enabled)
            {
                continue;
            }

            try
            {
                ((Action)subscriber).Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"Exception during {functionLabel} from " +
                    $"{subscriber.Target?.GetType().Name}: {ex}");
            }
        }
    }
}

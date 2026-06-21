using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the jolting animation of UI elements
/// </summary>
public class UIJoltManager : MonoBehaviour
{
    private static UIJoltManager _instance;
    public static UIJoltManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("UIJoltManager");
                _instance = go.AddComponent<UIJoltManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private List<UIElementJolt> _activeJolts = new List<UIElementJolt>();

    private void Update()
    {
        // Only update elements that are actually jolting
        for (int i = _activeJolts.Count - 1; i >= 0; i--)
        {
            if (!_activeJolts[i].UpdateJolt())
            {
                // Jolt finished, remove from active list
                _activeJolts.RemoveAt(i);
            }
        }
    }

    public void RegisterJolt(UIElementJolt element)
    {
        if (!_activeJolts.Contains(element))
        {
            _activeJolts.Add(element);
        }
    }

    public void UnregisterJolt(UIElementJolt element)
    {
        _activeJolts.Remove(element);
    }
}
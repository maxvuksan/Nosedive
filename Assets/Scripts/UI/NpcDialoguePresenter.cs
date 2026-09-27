using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Manages presentation of npc dialogue popups 
/// </summary>
public class NpcDialoguePresenter : MonoBehaviour
{
    public static NpcDialoguePresenter Singleton;

    [SerializeField] private RectTransform _textBackPanel;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _typeSpeed;
    
    private NpcDialogueLine? _activeDialogueLine;
    
    private int _trackedCharIndex;
    private float _trackedTypeSpeed;
    
    public void Awake()
    {
        Helpers.CreateSingleton(ref Singleton, this);
    }

    public void Start()
    {
        _trackedCharIndex = 0;
        _trackedTypeSpeed = 0;
        UnloadLine();
    }

    /// <summary>
    /// Loads a new line to be displayed, this resets the letter index to 0
    /// </summary>
    public void LoadLine(NpcDialogueLine line)
    {
        UnloadLine();
        _activeDialogueLine = line;
        print("load line");
    }

    /// <summary>
    /// Clears any loaded line
    /// </summary>
    public void UnloadLine()
    {
        _trackedCharIndex = 0;
        _trackedTypeSpeed = 0;
        _activeDialogueLine = null;
        DynamicUIBackPanel.SetWidth(0);
        RefreshVisual();
    }

    private void Update()
    {
        if (_activeDialogueLine == null)
        {
            return;
        }
        
        _trackedTypeSpeed +=  Time.deltaTime * _typeSpeed;
        if (_trackedTypeSpeed > 1)
        {
            _trackedCharIndex++;

            if (_trackedCharIndex >= _activeDialogueLine.Value.Content.Length)
            {
                // We have reached the end of the line, don't do anything
                return;
            }
            
            _trackedTypeSpeed = 0;
            RefreshVisual();
        }
    }

    private void RefreshVisual()
    {
        if (_activeDialogueLine == null)
        {
            _text.text = "";
            return;
        }
        
        _text.text = _activeDialogueLine.Value.Content.Substring(0, _trackedCharIndex + 1);
        
        // Force bounds to be recalculated for the updated text
        _text.ForceMeshUpdate();

        DynamicUIBackPanel.SetColour(PanelColour.White);
        DynamicUIBackPanel.SetWidth(_text.textBounds.size.x);
        DynamicUIBackPanel.SetPosition(_text.transform.position);
    }
    
}

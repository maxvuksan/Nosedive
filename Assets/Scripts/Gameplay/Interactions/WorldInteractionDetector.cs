using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the players interactions with interactable in the world, this should be placed at the cameras position / player head
/// </summary>
public class WorldInteractionDetector : MonoBehaviour
{
    public static WorldInteractionDetector Singleton;
    
    /// <summary>
    /// How far the interaction ray casts from this transforms position
    /// </summary>
    [SerializeField] private float _interactionDistance = 10;

    [SerializeField] private TextMeshProUGUI _interactionText;

    /// <summary>
    /// The interactable the ray is currently hitting
    /// </summary>
    [CanBeNull] private InteractableBase _focusedInteractable;

    /// <summary>
    /// The interaction which has taken focus, this will block other interactions from occuring
    /// </summary>
    [CanBeNull] private InteractableBase _activeInteraction;

    public static Action OnPostInteractionSearch;
    
    public bool HasActiveInteraction => _hasActiveInteraction;

    private bool _hasActiveInteraction;
    private bool _hasFocusedInteraction;

    private void Awake()
    {
        Helpers.CreateSingleton(ref Singleton, this);
        
        _hasActiveInteraction = false;
        _hasFocusedInteraction = false;
        _activeInteraction = null;
        _focusedInteractable = null;
    }

    
    public void AssignActiveInteraction(InteractableBase interactable)
    {
        _activeInteraction = interactable;
        _hasActiveInteraction = true; 
    }

    public void ReleaseActiveInteraction()
    {
        _hasActiveInteraction = false;
        _activeInteraction = null;
    }
    
    
    private void Update()
    {
        if(!InCorrectGameState())
        {
            ReleaseActiveInteraction();
            return;
        }
        
        if (InputManager.InteractInputOnPress())
        {
            if (_hasActiveInteraction)
            {
                _focusedInteractable.OnInteractWhenActive(this);
            }
            else if (_hasFocusedInteraction)
            {
                _focusedInteractable.OnInteract(this);
            }
        }
    }
    
    private void FixedUpdate()
    {
        SetInteractionFocusText("");

        if(InCorrectGameState() && !_hasActiveInteraction)
        {
            SearchForFocusedInteractable();
        }
        
        // Tell other scripts we are finished search for interactions
        OnPostInteractionSearch?.Invoke();
    }

    private bool InCorrectGameState()
    {
        // Only search for interactions when the player is in the world (play mode)
        if(GameStateManager.CurrentState == GameStateManager.GameState.Playing)
        {
            return true;
        }
        return false;
    }

    private void SetInteractionFocusText(string text)
    {
        // Force bounds to be recalculated for the updated text

        if (text != _interactionText.text)
        {
            _interactionText.text = text;
            _interactionText.ForceMeshUpdate();

            DynamicUIBackPanel.SetColour(PanelColour.Black);
            DynamicUIBackPanel.SetWidth(_interactionText.textBounds.size.x);
            DynamicUIBackPanel.SetPosition(_interactionText.transform.position);
        }
    }
    
    /// <summary>
    /// Shoots a ray to find an interactable object
    /// </summary>
    private void SearchForFocusedInteractable()
    {
        _focusedInteractable = null; 
        _hasFocusedInteraction = false;
        HeldInteractable.Singleton.PowerBoxAttractSpot = null;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, _interactionDistance))
        {
            if (hitInfo.collider.TryGetComponent<PowerBoxAttractSpot>(out var powerBoxAttractSpot))
            {
                HeldInteractable.Singleton.PowerBoxAttractSpot = powerBoxAttractSpot;
            }
            else if (hitInfo.collider.TryGetComponent<InteractableBase>(out var interactable))
            {
                // Ensure the interactable isn't currently being held 
                if (HeldInteractable.Singleton.PowerBox != interactable && interactable.CanBeInteractedWith)
                {
                    SetInteractionFocusText(interactable.InteractionText);
                    
                    _focusedInteractable = interactable;
                    _hasFocusedInteraction = true;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Set the color dynamically based on whether we currently have a valid interaction target
        if (_focusedInteractable != null)
        {
            Gizmos.color = Color.green;
            
            Vector3 targetPosition = transform.position + (transform.forward * _interactionDistance);
            
            if (Application.isPlaying && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _interactionDistance))
            {
                targetPosition = hit.point;
                Gizmos.DrawWireSphere(targetPosition, 0.1f);
            }
            
            Gizmos.DrawLine(transform.position, targetPosition);
        }
        else
        {
            // Red line when there is no active target or when editing the scene
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (transform.forward * _interactionDistance));
        }
    }

}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;

/// <summary>
/// Reusable class for configuring events
/// </summary>
public class TriggeredEvent : MonoBehaviour
{
    [System.Serializable]
    public class AnimationTrigger
    {
        /// <summary>
        /// The animation to trigger an event on
        /// </summary>
        public Animator Animator;
        
        /// <summary>
        /// The name of the trigger to call on the animator (through .SetTrigger)
        /// </summary>
        public string TriggerName;
    }
    
    /// <summary>
    /// An array 
    /// </summary>
    public AnimationTrigger[] AnimationTriggers;

    /// <summary>
    /// Events to invoke on trigger
    /// </summary>
    public UnityEvent[] UnityEvents;

    public void TriggerEvent()
    {
        foreach (var trigger in AnimationTriggers)
        {
            trigger.Animator.SetTrigger(trigger.TriggerName);
        }
        
        foreach(var unityEvent in UnityEvents)
        {
            unityEvent?.Invoke();
        }
    }
    
}

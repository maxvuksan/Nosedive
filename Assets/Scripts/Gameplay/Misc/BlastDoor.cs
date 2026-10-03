using UnityEngine;

/// <summary>
/// Manages the animation for the blast door in the incineration chambers 
/// </summary>
public class BlastDoor : MonoBehaviour
{
    private Animator _animator;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    
    public void PlayAnimationOpenToClose()
    {
        _animator.SetTrigger("OpenToClose");
    }

    public void PlayAnimationCloseToOpen()
    {
        _animator.SetTrigger("CloseToOpen");
    }
}

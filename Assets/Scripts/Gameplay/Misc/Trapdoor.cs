using UnityEngine;


public class Trapdoor : PressurePlateListener
{
    [SerializeField] Animator _animator;

    public override void OnSwitchState(bool openState)
    {
        if (openState)
        {
            _animator.SetTrigger("Open");
        }
    }
}

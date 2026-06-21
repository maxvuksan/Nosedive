using UnityEngine;

public class PinholeDoorFrame : PressurePlateListener
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public override void OnSwitchState(bool pressurePlateState)
    {
        _animator.SetBool("Open", pressurePlateState);
    }
}

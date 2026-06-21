using UnityEngine;

public class BirdAI_StateSwitchTrigger : MonoBehaviour
{
    private BirdAI _bird;

    void Start()
    {
        _bird = GetComponentInParent<BirdAI>();
    }

    /// <summary>
    /// signals to the bird that the animation is done, and we may go to the next random idle state
    /// </summary>
    public void OnIdleAnimationFinish()
    {
        _bird.OnIdleAnimationFinish();
    }

}

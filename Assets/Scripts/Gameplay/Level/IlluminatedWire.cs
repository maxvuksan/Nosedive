using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MaterialOverrides))]
public class IlluminatedWire : MonoBehaviour
{
    private MaterialOverrides _materialOverrides;

    /// <summary>
    /// Function to invoke when blend value reaches 1
    /// </summary>
    public UnityEvent OnFullyBlended;

    private void Awake() 
    {
        _materialOverrides = GetComponent<MaterialOverrides>();
    }

    public void SetBlendLerpT(float lerpT, bool invokeFunctionWhenFullyBlended = false)
    {
        lerpT = Mathf.Clamp01(lerpT);

        _materialOverrides.AssignFloatOverride("_Blend", lerpT);

        if (lerpT >= 0.99f && invokeFunctionWhenFullyBlended)
        {
            OnFullyBlended?.Invoke();
        }
    }

}

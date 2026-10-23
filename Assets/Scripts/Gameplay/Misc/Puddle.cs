using JetBrains.Annotations;
using UnityEngine;

public class Puddle : MaterialOverrides
{
    /// <summary>
    /// The reflection probe the puddle should use for reflections
    /// </summary>
    [SerializeField] [CanBeNull] private ReflectionProbe _reflectionProbe;

    [SerializeField] [Range(0,1)] private float _rippleStrength;
    public override void AdditionalReapply()
    {
        if (_reflectionProbe != null && propertyBlock != null)
        {
            Cubemap probeCubemap = _reflectionProbe.bakedTexture as Cubemap;

            if (probeCubemap == null)
            {
                return;
            }
            
            propertyBlock.SetTexture("_ReflectionProbe", probeCubemap);
            propertyBlock.SetFloat("_RippleStrength", _rippleStrength);
        }
    }
}

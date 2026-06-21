using UnityEngine;


/// <summary>
/// Physical materials objects can be made of, apply to specific object by attaching the ApplyMaterial component to that object
/// </summary>
public enum MaterialTypes
{
    Conrete,
    Metal,
    WoodBoard,
    Glass,
}

/// <summary>
/// Manages the properties of each material type
/// </summary>
public class MaterialManager : MonoBehaviour
{
    [System.Serializable]
    public struct MaterialProperties {
        public string PlayerFootstepSoundLeft;
        public string PlayerFootstepSoundRight;
    }

    /// <summary>
    /// The properties of the associated material, this list should match the ordering of MaterialTypes enum
    /// </summary>
    public MaterialProperties[] Properties;

    public static MaterialManager Singleton;

    void Awake()
    {
        Helpers.CreateSingleton<MaterialManager>(ref Singleton, this);
    }

}

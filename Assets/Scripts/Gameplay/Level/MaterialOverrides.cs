using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;


/// <summary>
/// Provides functionality for overriding material variables on a per mesh renderer basis
/// </summary>
public class MaterialOverrides : MonoBehaviour
{
    /// <summary>
    /// All the mesh renderers to apply the overrides to
    /// </summary>
    public MeshRenderer[] MeshRenderers;

    /// <summary>
    /// Should this override only apply to a specific material index on each mesh renderer
    /// </summary>
    public bool TargetSpecificMaterialIndex;
    [ShowIf("TargetSpecificMaterialIndex")] public int TargetMaterialIndex;


    protected MaterialPropertyBlock propertyBlock;   

    /// <summary>
    /// A dictionary to to mapped by
    /// The variable string to set the colour override on
    /// This is generally prefixed with an underscore, e.g. _ColourVariable
    /// </summary>
    private Dictionary<string, Color> _colourOverrides = new();

    private Dictionary<string, float> _floatOverrides = new();

    #region Modify Overrides

    public void AssignColourOverride(string variableName, Color newColour)
    {
        _colourOverrides[variableName] = newColour;
        ApplyAll();
    }

    public void AssignFloatOverride(string variableName, float newFloat)
    {
        _floatOverrides[variableName] = newFloat;
        ApplyAll();
    }

    #endregion

    #region Internal Functionality
    
    private void Awake()
    {
        ApplyAll();
    }
    /// <summary>
    /// Resets all the renderers' MaterialPropertyBlocks by setting them to null
    /// </summary>
    private void ResetRenderers()
    {
        foreach (Renderer renderer in MeshRenderers)
        {
            renderer.SetPropertyBlock(null);
        }
    }

    /// <summary>
    /// Sets all the renderers' MaterialPropertyBlocks to the desired
    /// material property block
    /// </summary>
    private void SetRenderers(MaterialPropertyBlock materialPropertyBlock)
    {
        foreach (Renderer renderer in MeshRenderers)
        {   
            // Only apply overrides to a specific material index
            if (TargetSpecificMaterialIndex)
            {
                renderer.SetPropertyBlock(materialPropertyBlock, TargetMaterialIndex);
            }
            else
            {
                renderer.SetPropertyBlock(materialPropertyBlock);
            }
        }
    }

    /// <summary>
    /// Apply the base and emmisive colours to the material
    /// </summary>
    protected void ApplyOverrides()
    {
        ResetRenderers(); 

        propertyBlock.Clear();

        // Apply each colour override
        foreach(var entry in _colourOverrides)
        {
            propertyBlock.SetColor(entry.Key, entry.Value);
        }

        // Apply each float override
        foreach(var entry in _floatOverrides)
        {
            propertyBlock.SetFloat(entry.Key, entry.Value);
        }

    }

    private void OnValidate()
    {
        ApplyAll();
    }

    void ApplyAll()
    {
        propertyBlock = new();
        ApplyOverrides();
        
        AdditionalReapply();
        
        SetRenderers(propertyBlock); 
    }
    
    public virtual void AdditionalReapply()
    {
        // Additional logic on reapply
    }

    #endregion
}

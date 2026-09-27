using UnityEngine;

public class CustomBlit : MonoBehaviour
{
    public RenderTexture sourceRT;
    public RenderTexture destinationRT;
    public Material blitMaterial;

    void Update()
    {
        var texSize = new Vector4(destinationRT.width, destinationRT.height, 0, 0);
        blitMaterial.SetVector("_SourceTexSize", texSize);
        Graphics.Blit(sourceRT, destinationRT, blitMaterial);
    }
}

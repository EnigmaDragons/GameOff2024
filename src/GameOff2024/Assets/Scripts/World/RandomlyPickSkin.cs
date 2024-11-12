
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class RandomlyPickSkin : MonoBehaviour
{
    public Mesh[] skins;

    void Awake()
    {
        if (skins == null || skins.Length == 0) return;
        
        // Get the SkinnedMeshRenderer component
        SkinnedMeshRenderer meshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (meshRenderer == null) return;

        // Pick a random skin from the array
        int randomIndex = Random.Range(0, skins.Length);
        meshRenderer.sharedMesh = skins[randomIndex];
    }
}
using UnityEngine;

public class GpuInstanceMaterial : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer[] meshRenderers;

    private void Start()
    {
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            Debug.LogWarning("No SkinnedMeshRenderers assigned to GpuInstanceMaterial");
            return;
        }

        foreach (SkinnedMeshRenderer renderer in meshRenderers)
        {
            if (renderer != null)
            {
                Material[] materials = renderer.materials;
                foreach (Material material in materials)
                {
                    if (material != null)
                    {
                        material.enableInstancing = true;
                    }
                }
            }
        }
    }
}

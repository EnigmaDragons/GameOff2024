using UnityEngine;

public class GpuInstanceEverything : MonoBehaviour
{
    private void Start()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning("No Renderers found in GpuInstanceEverything");
            return;
        }

        foreach (Renderer renderer in renderers)
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

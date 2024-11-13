using DunGen.Adapters;
using UnityEngine;

public class DisableAfterCarving : MonoBehaviour
{
    private void Start()
    {
        UnityNavMeshAdapter.instance.OnNavmeshBaked += Instance_OnNavmeshBaked;
    }

    private void Instance_OnNavmeshBaked(object sender, System.EventArgs e)
    {
        gameObject.SetActive(false);
    }
}

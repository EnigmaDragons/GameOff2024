using UnityEngine;
using UnityEngine.AI;

public class EnableTargetAfterNavMeshBake : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;

    private void OnEnable()
    {
        if (DunGen.Adapters.UnityNavMeshAdapter.instance != null)
        {
            targetObject.SetActive(false);
            DunGen.Adapters.UnityNavMeshAdapter.instance.OnNavmeshBaked += OnNavmeshBaked;
        } else {
            targetObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (DunGen.Adapters.UnityNavMeshAdapter.instance != null)
        {
            DunGen.Adapters.UnityNavMeshAdapter.instance.OnNavmeshBaked -= OnNavmeshBaked;
        }
    }

    private void OnNavmeshBaked(object sender, System.EventArgs e)
    {
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }
}

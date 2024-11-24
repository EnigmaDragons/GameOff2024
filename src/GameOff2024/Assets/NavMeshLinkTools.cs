using DunGen.Adapters;
using UnityEngine;
using System;
using Unity.AI.Navigation;
using DunGen;

public class NavMeshLinkTools : MonoBehaviour
{
    [SerializeField] private bool refreshOnMeshBake = true;
    private NavMeshLink m_link;
    [SerializeField] float min_cost = 0.75f;
    [SerializeField] float max_cost = 2.5f;
    
    private void Awake()
    {
        m_link = GetComponent<NavMeshLink>();
    }
    
    private void OnEnable()
    {
        transform.parent = GetComponentInParent<Tile>().transform;
    
        if (UnityNavMeshAdapter.instance != null)
            UnityNavMeshAdapter.instance.OnNavmeshBaked += Instance_OnNavmeshBaked;
    }

    private void OnDisable()
    {
        if (UnityNavMeshAdapter.instance != null)
            UnityNavMeshAdapter.instance.OnNavmeshBaked -= Instance_OnNavmeshBaked;
    }

    private void Instance_OnNavmeshBaked(object sender, EventArgs e)
    {
        m_link.enabled = true;
        m_link.costModifier = UnityEngine.Random.Range(min_cost, max_cost);
    }
}

using DunGen.Adapters;
using UnityEngine;
using System;
using Unity.AI.Navigation;

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
    private void Start()
    {
        UnityNavMeshAdapter.instance.OnNavmeshBaked += Instance_OnNavmeshBaked;
    }

    private void Instance_OnNavmeshBaked(object sender, EventArgs e)
    {
        m_link.enabled = false;
        m_link.enabled = true;
        m_link.costModifier = UnityEngine.Random.Range(min_cost, max_cost);
    }

}

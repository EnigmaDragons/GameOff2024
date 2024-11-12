using UnityEngine;
using System.Collections.Generic;

public class NpcVariation : MonoBehaviour
{
    [SerializeField] private GameObject[] elements;
    
    void Awake()
    {
        VaryNpcScales();
    }

    public void VaryNpcScales()
    {
        foreach (GameObject npc in elements)
        {
            if (npc != null)
            {
                // Vary height (y scale) by ±15%
                float heightVariation = Random.Range(0.85f, 1.15f);
                
                // Vary width/depth (x & z scale) by ±20%
                float widthDepthVariation = Random.Range(0.8f, 1.2f);
                
                Vector3 newScale = new Vector3(
                    widthDepthVariation,
                    heightVariation,
                    widthDepthVariation
                );
                
                npc.transform.localScale = newScale;
            }
        }
    }
}
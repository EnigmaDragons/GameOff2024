using UnityEngine;
using DunGen;
using NeoFPS.CharacterMotion;
public class TileTriggerVolume : MonoBehaviour
{
    Tile m_tile;
    BoxCollider m_boxCollider;
    private void Awake()
    {
        m_tile = GetComponentInParent<Tile>();
        m_boxCollider = GetComponent<BoxCollider>();
    }
    private void Start()
    {
        m_boxCollider.size = 2*m_tile.Bounds.extents;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            TileManager.instance.SetNewActiveTile(m_tile, true);
        }
        else if(other.gameObject.GetComponent<SpyController>() != null)
        {
            TileManager.instance.SetNewActiveTile(m_tile, false);
        }
    }
}

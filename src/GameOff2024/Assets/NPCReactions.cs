using System.Collections;
using UnityEngine;

public class NPCReactions : MonoBehaviour
{
    SkinnedNpc npc;
    public bool isTripping;
    [SerializeField] float rotateSpeed = 5f;
    private void Awake()
    {
        npc = GetComponentInChildren<SkinnedNpc>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out RecordAsPlayerTransformOnStart player))
        {
            ReactToPlayer(player.transform);
        }
        else if(collision.gameObject.TryGetComponent(out SpyController spy))
        {
            ReactToSpy(spy.transform);
        }
    }

    void ReactToPlayer(Transform player)
    {
        isTripping = true;
        npc.SetTrigger("Stumble");
    }
    void ReactToSpy(Transform spy)
    {
        isTripping = true;
        npc.SetTrigger("Stumble");
    }

    public void Reset()
    {
        isTripping =false;
    }
}

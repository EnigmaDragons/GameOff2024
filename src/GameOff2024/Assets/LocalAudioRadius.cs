using UnityEngine;

public class LocalAudioRadius : MonoBehaviour
{
    [SerializeField] float radius;

    private void Start()
    {
        GetComponent<SphereCollider>().radius = radius;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Physics_Audio audioComp))
        {
            audioComp.ToggleCollision(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Physics_Audio audioComp))
        {
            audioComp.ToggleCollision(false);
        }
    }
}

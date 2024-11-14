using UnityEngine;

public class UnparentOnAwake : MonoBehaviour
{
    private void Start()
    {
        transform.parent = null;
    }
}

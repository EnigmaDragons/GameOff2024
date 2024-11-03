using UnityEngine;

public class RandomCardinalRotation : MonoBehaviour
{
    private void Awake()
    {
        int randomRotation = Random.Range(0, 4) * 90;
        transform.rotation = Quaternion.Euler(0, randomRotation, 0);
    }
}

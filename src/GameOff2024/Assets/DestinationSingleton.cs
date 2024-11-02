using UnityEngine;

public class DestinationSingleton : MonoBehaviour
{
    public static DestinationSingleton instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
}

using UnityEngine;

public class RandomlySelectOnlyOneChild : MonoBehaviour
{
    void Awake()
    {
        // Get all child objects
        int childCount = transform.childCount;
        
        if (childCount == 0) return;

        // Pick a random child index
        int selectedIndex = Random.Range(0, childCount);

        // Disable all children except the randomly selected one
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.gameObject.SetActive(i == selectedIndex);
        }
    }
}

using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText; // Reference to the TextMeshPro text component
    private float elapsedTime;                  // Time in seconds that has elapsed
    private bool isTicking;                     // Whether the timer is running

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the timer
        elapsedTime = 0f;
        isTicking = true; // Start ticking immediately
    }

    // Update is called once per frame
    void Update()
    {
        if (isTicking)
        {
            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Convert elapsed time to minutes and seconds
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            // Update the text component in MM:SS format
            timerText.text = $"{minutes:D2}:{seconds:D2}";
        }
    }

    // Method to start the timer
    public void StartTimer()
    {
        isTicking = true;
    }

    // Method to stop the timer
    public void StopTimer()
    {
        isTicking = false;
    }

    // Method to reset the timer
    public void ResetTimer()
    {
        elapsedTime = 0f;
        timerText.text = "00:00";
    }
}

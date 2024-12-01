using UnityEngine;
using System.IO;

public class EzScreen : MonoBehaviour
{
    [SerializeField] private string filename;
    
    private static int _counter;

    private void Awake()
    {
        string path = Path.Combine(Application.persistentDataPath, "Screenshots");
        Directory.CreateDirectory(path);
        
        while (File.Exists(Path.Combine(path, $"{filename}_{_counter}.png")))
            _counter++;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F12) || Input.GetKeyDown(KeyCode.Backslash))
        {
            string path = Path.Combine(Application.persistentDataPath, "Screenshots");
            var n = Path.Combine(path, $"{filename}_{_counter++}.png");
            ScreenCapture.CaptureScreenshot(n);
            Debug.Log($"Captured screenshot: {n}");
        }
#endif
    }
}

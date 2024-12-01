using UnityEngine;
using System.IO;

public class EzScreen : CrossSceneSingleInstance
{
    [SerializeField] private string filename;
    
    protected override string UniqueTag => "Screenshots";
    private static int _counter;

    protected override void OnAwake()
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

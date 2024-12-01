using UnityEngine;

[CreateAssetMenu]
public sealed class Navigator : ScriptableObject
{
    [SerializeField] private bool loggingEnabled;
    
    public void NavigateToMainMenu() => NavigateTo("MainMenu");
    public void NavigateToIntroScene() => NavigateTo("IntroCutscene");
    public void NavigateToGameScene() => NavigateTo("GameScene");
    public void NavigateToCreditsScene() => NavigateTo("CreditsScene");
    public void NavigateToScene(string sceneName) => NavigateTo(sceneName);

    private void NavigateTo(string sceneName)
    {
        if (loggingEnabled)
            Log.Info($"Navigating to {sceneName}");
        Message.Publish(new NavigateToSceneRequested(sceneName, sceneName == "GameScene"));
    }

    public void NavigateInstantly(string sceneName)
    {
        if (loggingEnabled)
            Log.Info($"Navigating instantly to {sceneName}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}

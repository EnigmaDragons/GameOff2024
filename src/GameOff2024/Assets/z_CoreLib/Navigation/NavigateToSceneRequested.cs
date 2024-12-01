
public class NavigateToSceneRequested
{
    public string SceneName { get; set; }
    public bool isGameScene;

    public NavigateToSceneRequested(string sceneName, bool gameScene)
    {
        SceneName = sceneName;
        isGameScene = gameScene;
    }
}

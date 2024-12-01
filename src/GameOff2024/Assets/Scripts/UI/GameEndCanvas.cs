using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using FMODUnity;
using NeoFPS;
using NeoFPS.Samples;

public class GameEndCanvas : OnMessage<GameStateChanged>
{
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] private EventReference[] gameOverLines;

    [SerializeField] float GameOverDelay;

    bool gameDecided;

    bool showCanvas;
    [SerializeField] private CanvasGroup loadUi;

    InGameMenu menuParent;

    [SerializeField] Navigator navigator;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        menuParent = GetComponentInParent<InGameMenu>();
    }
    protected override void Execute(GameStateChanged msg)
    {
        if(msg != null && !gameDecided)
        {
            if (msg.State.gameLost)
            {
                gameDecided = true;
                menuParent.ShowGameOverPanel();
                StartCoroutine(TriggerGameOver());
            }
        }
    }

    IEnumerator TriggerGameOver()
    {
        float alpha = 0;
        NeoFpsTimeScale.FreezeTime();

        if (gameOverLines.Length > 0)
        {
            int randomIndex = Random.Range(0, gameOverLines.Length);
            RuntimeManager.PlayOneShot(gameOverLines[randomIndex]);
        }

        gameOverPanel.SetActive(true);

        while (alpha < 1)
        {
            alpha += 0.5f * Time.unscaledDeltaTime;
            loadUi.alpha = alpha;
            yield return null;
        }

        //Cursor.visible = true;
        //Cursor.lockState = CursorLockMode.None;
    }

    public void ResetScene()
    {
        Debug.Log("Reset Scene");
        NeoFpsTimeScale.ResumeTime();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        loadUi.alpha = 0;
        gameOverPanel.SetActive(false);
        menuParent.ResetGameOver();

        navigator.NavigateToGameScene();
    }

}

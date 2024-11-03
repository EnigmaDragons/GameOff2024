using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameEndCanvas : OnMessage<GameStateChanged>
{
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject youWonSubpanel;
    [SerializeField] GameObject youLostSubpanel;
    [SerializeField] Button playAgainButton;

    [SerializeField] float GameOverDelay;

    bool gameDecided;

    private void Start()
    {
        playAgainButton.onClick.AddListener(()=>SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        youWonSubpanel.SetActive(false);
        youLostSubpanel.SetActive(false);
    }
    protected override void Execute(GameStateChanged msg)
    {
        if(msg != null && !gameDecided)
        {
            if (msg.State.gameWon)
            {
                gameDecided = true;
                StartCoroutine(TriggerGameOver(true));
            }
            else if (msg.State.gameLost)
            {
                gameDecided = true;
                StartCoroutine(TriggerGameOver(false));
            }
        }
    }

    IEnumerator TriggerGameOver(bool gameWon)
    {
        yield return new WaitForSeconds(GameOverDelay);
        gameOverPanel.SetActive(true);
        youWonSubpanel.SetActive(gameWon);
        youLostSubpanel.SetActive(!gameWon);
    }
}

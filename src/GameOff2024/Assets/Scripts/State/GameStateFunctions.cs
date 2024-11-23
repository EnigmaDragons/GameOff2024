using UnityEngine;

[CreateAssetMenu(menuName = "GameStateFunctions")]
public class GameStateFunctions : ScriptableObject
{
    public void SetShowIntroCutscene(bool value)
    {
        CurrentGameState.Init();
        CurrentGameState.UpdateState(state => state.shouldShowIntroCutscene = value);
    }

    public void SetGameWon(bool value)
    {
        CurrentGameState.UpdateState(state =>
        {
            state.gameLost = false;
            state.gameWon = true;
        });
    }
}

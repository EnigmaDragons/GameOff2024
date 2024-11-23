using UnityEngine;

[CreateAssetMenu(menuName = "GameStateFunctions")]
public class GameStateFunctions : ScriptableObject
{
    public void SetShowIntroCutscene(bool value) => CurrentGameState.UpdateState(state => state.shouldShowIntroCutscene = value);
}

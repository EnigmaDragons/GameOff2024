using UnityEngine;

public class Briefcase : MonoBehaviour
{
    public void OnBriefcaseGrabbed()
    {
        CurrentGameState.UpdateState(gs =>
        {
            gs.gameWon = true;
        });
    }
}

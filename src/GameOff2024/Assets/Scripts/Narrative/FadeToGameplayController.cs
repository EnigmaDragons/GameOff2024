using UnityEngine;

public class FadeToGameplayController : MonoBehaviour
{
    [SerializeField] private FadeOutImage canvasFader;
    [SerializeField] private CameraEyeVignette vignette;
    
    public void TriggerFadeToGameplay()
    {
        canvasFader.StartFade(false);
        vignette.TriggerOpenAnimation();
    }
}

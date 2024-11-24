using UnityEngine;

public class FadeToGameplayController : MonoBehaviour
{
    [SerializeField] private FadeOutImage canvasFader;
    [SerializeField] private CameraEyeVignette vignette;
    [SerializeField] private float drunkBlurFadeDelay = 2f;
    
    public void TriggerFadeToGameplay()
    {
        canvasFader.StartFade(false);
        vignette.TriggerOpenAnimation();
        this.ExecuteAfterDelay(() => Message.Publish(new SetIntoxicationLevel(0.75f)), drunkBlurFadeDelay);
        this.ExecuteAfterDelay(() => Message.Publish(new SetIntoxicationLevel(0.50f)), drunkBlurFadeDelay * 2);
        this.ExecuteAfterDelay(() => Message.Publish(new SetIntoxicationLevel(0.25f)), drunkBlurFadeDelay * 3);
        this.ExecuteAfterDelay(() => Message.Publish(new SetIntoxicationLevel(0f)), drunkBlurFadeDelay * 4);
    }
}

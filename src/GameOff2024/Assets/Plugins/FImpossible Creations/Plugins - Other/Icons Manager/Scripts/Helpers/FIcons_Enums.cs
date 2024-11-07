namespace FIMSpace.FIcons
{
    /// <summary> Defining style of animating sprite after being loaded </summary>
    public enum FE_IconLoadingAnimation { None, FadeIn, SpinnerWithFade, FastFade }
    public enum FE_TextureLoadState { None, Loading, Loaded, Unloaded, Error }
    public enum FE_SpriteLoadState { Unloaded, Loading, Loaded }
    public enum FE_TextureTaskState { Idle, Waiting, Finished, Error }
    public enum FE_RequestState { None, Complete, Error }

#if ADDRESSABLES_IMPORTED
    public enum FE_LoadingPath { StreamingAssets, WWW, Resources, OtherTexture, Adressable }
#else
    public enum FE_LoadingPath { StreamingAssets, WWW, Resources, OtherTexture }
#endif

}

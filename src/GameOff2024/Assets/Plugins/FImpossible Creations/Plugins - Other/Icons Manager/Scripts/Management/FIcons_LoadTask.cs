using UnityEngine;
using UnityEngine.UI;

namespace FIMSpace.FIcons
{
    /// <summary>
    /// FM: Class helping managing icon loading logics, each task is assigned to single UI Image to assign sprite image for it.
    /// Tasks are subordinated to texture containers.
    /// </summary>
    public class FIcons_LoadTask
    {
        /// <summary> State of loading for the task </summary>
        public FE_TextureTaskState State { get; private set; }

        public int Width { get; private set; }
        public int OriginalCallWidth { get; private set; }
        public int Height { get; private set; }
        public int OriginalCallHeight { get; private set; }
        public FilterMode FilterMode { get; private set; }

        public FE_IconLoadingAnimation AnimationStyle { get; private set; }
        public FIcon_TextureContainer TargetContainer { get; private set; }
        public FE_LoadingPath LoadingPath { get; private set; }
        public bool ScaleDownSupport { get; private set; }
        public readonly int TimeInitiated;

        public FIcon_LoadingAnimator LoadingAnimator { get; private set; }
        public FIcon_GeneratedSprite GeneratedSprite { get; private set; }
        public Image TargetImage { get; private set; }
        public bool Aborted { get; private set; }
        private readonly bool setNativeSize;

        public bool LossyScaleSync { get; private set; }
        public System.Action<Sprite> Callback { get; private set; }

        public FIcons_LoadTask(FIcon_TextureContainer container, Image targetImage, int width, int height, FilterMode filterMode = FilterMode.Point, bool setNativeSize = true, FE_IconLoadingAnimation animationStyle = FE_IconLoadingAnimation.FadeIn, FE_LoadingPath loadingPath = FE_LoadingPath.StreamingAssets, bool scaleDownSupport = false, bool lossyScaleSync = true, System.Action<Sprite> callback = null)
        {
            TargetContainer = container;

            this.TargetImage = targetImage;

            OriginalCallWidth = width;
            OriginalCallHeight = height;

            if (lossyScaleSync)
            {
                width = (int)Mathf.Abs(targetImage.rectTransform.lossyScale.x * width);
                height = (int)Mathf.Abs(targetImage.rectTransform.lossyScale.y * height);
            }

            Width = width;
            Height = height;
            FilterMode = filterMode;
            this.setNativeSize = setNativeSize;
            AnimationStyle = animationStyle;
            LoadingPath = loadingPath;
            TimeInitiated = Time.frameCount;
            ScaleDownSupport = scaleDownSupport;
            Aborted = false;
            LossyScaleSync = lossyScaleSync;
            Callback = callback;

            FIcon_Reference.AddReferenceFromTask(this);

            State = FE_TextureTaskState.Idle;
        }

        /// <summary>
        /// Setting waiting flag to help awaiting for sprite to load
        /// </summary>
        public void Wait()
        {
            State = FE_TextureTaskState.Waiting;
        }

        /// <summary>
        /// Assigning monobehvaiour which is animating target image to fade in or animate loading spinner
        /// </summary>
        public void AssignLoadingAnimator(FIcon_LoadingAnimator animator, FE_IconLoadingAnimation animationStyle)
        {
            if (LoadingAnimator != null) return;
            if (animator == null) return;
            if (Aborted) return;

            LoadingAnimator = animator;
            animator.AssignToImage(TargetImage, animationStyle, FIcons_Manager.Get.SpinningSpeed);
        }

        /// <summary>
        /// Applying task's loaded or readed sprite source to the target image and assigning sprite reference class
        /// </summary>
        internal void FinishTaskWith(FIcon_GeneratedSprite spriteSource, int spriteId)
        {
            if (Aborted) return;

            if (TargetImage) // If task's target image is still existing so sprite can be assigned
            {
                if (LoadingAnimator) // Letting loading animator finish it's work and assign new sprite
                {
                    LoadingAnimator.OnFinishedLoading(spriteSource.GetSprite(), setNativeSize);
                }
                else // If there is no animator we setting sprite image immadietely
                {
                    TargetImage.sprite = spriteSource.GetSprite();
                    if (setNativeSize) TargetImage.SetNativeSize();
                }

                State = FE_TextureTaskState.Finished;

                // Adding sprite reference MonoBehaviour to be able to detect if Image is removed, not needed etc. so we can unload memory etc.
                FIcon_Reference reference = TargetImage.GetComponent<FIcon_Reference>();
                if (!reference) reference = TargetImage.gameObject.AddComponent<FIcon_Reference>();
                reference.SetData(this, spriteSource, TargetImage, spriteId, AnimationStyle, setNativeSize);
                reference.TaskFinished(this);

                if (LossyScaleSync)
                {
                    if (setNativeSize)
                    {
                        Vector2 newScale = TargetImage.rectTransform.sizeDelta;
                        newScale.x /= TargetImage.rectTransform.lossyScale.x;
                        newScale.y /= TargetImage.rectTransform.lossyScale.y;
                        TargetImage.rectTransform.sizeDelta = newScale;
                    }
                }

                if (Callback != null) Callback.Invoke(spriteSource.GeneratedSprite);
            }
            else // Loading probably took to long and Image is not existing anymore so we can't assign loaded sprite
            {
                Debug.LogWarning("[ICONS MANAGER] Target Image component for sprite at path " + TargetContainer.PathToSourceTexture + " doesn't exist anymore!");
                State = FE_TextureTaskState.Error;
            }
        }

        /// <summary>
        /// Aborting executing this task
        /// </summary>
        internal void Abort()
        {
            if (TargetContainer != null)
            {
                if (GeneratedSprite != null) GeneratedSprite.AbortTask(this);
            }

            Aborted = true;
            State = FE_TextureTaskState.Finished;

            if (LoadingAnimator)
            {
                LoadingAnimator.OnAbort();
            }
        }

        /// <summary>
        /// Refreshing task variables with texture loaded to target container
        /// </summary>
        internal void CheckCorrectness()
        {
            if (TargetContainer != null)
                if (TargetContainer.SourceWidth != 0)
                    if (Height <= 0)
                        Height = FIcons_Methods.AspectRatioHeight(TargetContainer.SourceWidth, TargetContainer.SourceHeight, Width);
        }

        /// <summary>
        /// When task gets own generated sprite
        /// </summary>
        internal void OnSpriteAcquired(FIcon_GeneratedSprite generatedSprite)
        {
            GeneratedSprite = generatedSprite;
        }

        /// <summary>
        /// When some error occured we view error sprite and aborting task
        /// </summary>
        internal void ErrorOccured()
        {
            TargetImage.sprite = FIcons_Manager.Get.ErrorSprite;
            Abort();
        }
    }
}
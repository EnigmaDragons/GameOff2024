using System;
using UnityEngine;
using UnityEngine.UI;

namespace FIMSpace.FIcons
{
    /// <summary>
    /// FM: Class to animate spinner when image is waiting for load and fade in loaded image
    /// Component is working with object pooling when used with icons manager
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Hidden/FC IS_Animator")]
    public class FIcon_LoadingAnimator : MonoBehaviour
    {
        public Image Spinner;

        /// <summary> Image to which should be assigned new sprite when loaded </summary>
        public Image TargetImage;

        public Sprite LoadedSprite;

        internal FIcons_LoadingAnimator_ObjectPool PoolOwner { get; private set; }
        public FE_IconLoadingAnimation LoadingStyle = FE_IconLoadingAnimation.None;

        private readonly float originalAlpha = 1f;
        private float spinnerAlpha = 0f;
        private float targetAlpha = 0f;
        private float timeOfLoading = 0f;
        private float spinningSpeed = 300f;
        private static readonly Color transparent = new Color(1f, 1f, 1f, 0f);
        private bool aborted = false;
        internal void ResetToInit()
        {
            spinnerAlpha = 0f;
            targetAlpha = 0f;
            timeOfLoading = 0f;

            TargetImage = null;
            LoadedSprite = null;

            Spinner.transform.rotation = Quaternion.identity;
            Spinner.color = transparent;

            LoadingStyle = FE_IconLoadingAnimation.None;
        }


        /// <summary>
        /// Assigning objects pool parent to the animator
        /// </summary>
        public void AssignPool(FIcons_LoadingAnimator_ObjectPool pool)
        {
            if (PoolOwner != null) if (pool != PoolOwner) pool.PurgeFromPool(this);
            PoolOwner = pool;
        }


        /// <summary>
        /// Assignign loading animation to the UI image
        /// </summary>
        public void AssignToImage(Image targetImage, FE_IconLoadingAnimation animationStyle, float spinningSpeed = 1f)
        {
            this.spinningSpeed = spinningSpeed * 350f;

            try
            {
                LoadingStyle = animationStyle;

                TargetImage = targetImage;

                targetImage.color = targetImage.color * transparent;
                Spinner.color = transparent;

                Spinner.rectTransform.sizeDelta = TargetImage.rectTransform.sizeDelta * 0.8f;

                transform.SetParent(targetImage.transform.parent);

                transform.localScale = Vector3.one;
                transform.position = targetImage.transform.position;

                if (LoadingStyle == FE_IconLoadingAnimation.None)
                {
                    enabled = false;
                    Spinner.enabled = false;
                }
                else
                {
                    enabled = true;
                    if (!aborted) if (LoadingStyle == FE_IconLoadingAnimation.SpinnerWithFade) Spinner.enabled = true; else Spinner.enabled = false;
                }

                //SetSpinnerSpriteForSize(targetImage.rectTransform.sizeDelta.x * targetImage.rectTransform.lossyScale.x);
            }
            catch (Exception exc)
            {
                Debug.LogWarning("[ICONS MANAGER] There was problem during assigning sprite to new image " + exc);
            }
        }


        /// <summary>
        /// Getting loading spinner with nearest size to target scale for smooth spinner look
        /// </summary>
        private void SetSpinnerSpriteForSize(float scale, Sprite spr32, Sprite spr64, Sprite spr128, Sprite spr256)
        {
            if (scale < 0) scale = 64;
            int nearest = 32;
            int lowestDiff = int.MaxValue;

            for (int i = 32; i <= 256; i *= 2)
            {
                int diff = (int)Mathf.Abs(scale * 1.25f - i);
                if (diff < lowestDiff) { nearest = i; lowestDiff = diff; }
            }

            Sprite newSpinner = null;

            switch (nearest)
            {
                case 32: newSpinner = spr32; break;
                case 64: newSpinner = spr64; break;
                case 128: newSpinner = spr128; break;
                case 256: newSpinner = spr256; break;
            }

            if (newSpinner != null) Spinner.sprite = newSpinner;
        }


        /// <summary>
        /// We assigning new sprite to target image when loading finishes
        /// </summary>
        public void OnFinishedLoading(Sprite loaded, bool setNativeSize = true)
        {
            LoadedSprite = loaded;
            TargetImage.sprite = loaded;
            if (setNativeSize) TargetImage.SetNativeSize();
            if (LoadingStyle == FE_IconLoadingAnimation.None)
            {
                OnFinishedJob();
            }
        }


        /// <summary>
        /// Animating loading image and fading item
        /// </summary>
        void Update()
        {
            float delta = Time.unscaledDeltaTime;
            float deltaSpeed = 1f;
            if (FIcons_Manager.Get.FadesSpeed < 1f)
            {
                deltaSpeed = delta * Mathf.Lerp(2f, 10f, FIcons_Manager.Get.FadesSpeed);
                if (LoadingStyle == FE_IconLoadingAnimation.FastFade) deltaSpeed *= 2f;
            }

            timeOfLoading += delta;

            if (LoadedSprite == null)
            {
                if (Spinner.enabled)
                    if (timeOfLoading > FIcons_Manager.Get.SpinnerAfter)
                    {
                        spinnerAlpha = Mathf.Min(originalAlpha, spinnerAlpha + deltaSpeed);
                        Spinner.color = new Color(1f, 1f, 1f, spinnerAlpha);
                        Spinner.rectTransform.Rotate(0f, 0f, delta * -spinningSpeed);
                    }
            }
            else
            {
                spinnerAlpha = Mathf.Max(0f, spinnerAlpha - deltaSpeed);
                targetAlpha = Mathf.Min(originalAlpha, targetAlpha + deltaSpeed);

                if (targetAlpha >= 1f)
                {
                    OnFinishedJob();
                }
                else
                {
                    if (Spinner.enabled)
                    {
                        Spinner.color = new Color(1f, 1f, 1f, spinnerAlpha);
                        Spinner.rectTransform.Rotate(0f, 0f, delta * -spinningSpeed * (0.25f + spinnerAlpha * 0.75f));
                    }

                    TargetImage.color = new Color(TargetImage.color.r, TargetImage.color.g, TargetImage.color.b, targetAlpha);
                }
            }
        }

        /// <summary>
        /// Giving animator back to pool or destroying
        /// </summary>
        public void OnFinishedJob(bool setImage = true)
        {
            if (setImage)
                if (TargetImage != null) TargetImage.color = new Color(TargetImage.color.r, TargetImage.color.g, TargetImage.color.b, 1f);

            Spinner.color = transparent;
            aborted = false;

            if (PoolOwner != null)
            {
                PoolOwner.GiveBackObject(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Refreshing some variables to correctly abort task for already loaded sprite
        /// </summary>
        public void OnAbort()
        {
            if (LoadingStyle == FE_IconLoadingAnimation.None)
                OnFinishedJob();
            else
            {
                Spinner.enabled = false; 
                aborted = true;
            }
        }

    }
}
using UnityEngine;
using UnityEngine.UI;

namespace FIMSpace.FIcons
{
    [AddComponentMenu("FImpossible Creations/Icons/FIcon Loader")]
    public class FIcon_Loader : MonoBehaviour
    {
        public Image ToReplace;
        public FE_LoadingPath LoadingPath = FE_LoadingPath.StreamingAssets;
        [Tooltip("[Streaming Assets] Just path to target file with it's extension WITHOUT writing Assets/StreamingAssets\n\n[WWW] Path should start with 'http://' then write full adress of target file with it's extension\n\n[Resources] You should write path with the same manner like with StreamingAssets but WITHOUT extension" )]
        public string PathTo = "FIconsManagerExample/Mask 1.png";
        public Texture2D OtherTexture;
        public int NewWidth = 128;
        
        [HideInInspector]
        public bool Advanced = false;

        [HideInInspector]
        public int NewHeight = 0;
        [HideInInspector]
        [Tooltip("When you want to rescale image or move smoother on the scene use Bilinear filter (image will loose some sharpness)")]
        public FilterMode TargetFilter = FilterMode.Point;
        [HideInInspector]
        public FE_IconLoadingAnimation LoadingAnimation = FE_IconLoadingAnimation.FadeIn;
        [HideInInspector]
        [Tooltip("Generating lower resolution textures to support scalling down better - size in memory will be bigger by half of total size of the image (Use Bilinear filter with this option)")]
        public bool ScaleDownSupport = false;

        [HideInInspector]
        public bool LoadOnStart = true;
        [Tooltip("Changing size of rect transform to exacly fit image size after target image is loaded (Set Native Size)")]
        public bool AutoSetWidthHeight = true;
        [HideInInspector]
        [Tooltip("If image is scaled (or it's parent) loaded image dimensions will also be scaled with this facor (enable when you use scaled canvas and point filter)")]
        public bool LossyScaleSync = true;


        private void Reset()
        {
            GetImage();

            if (ToReplace != null) NewWidth = (int)ToReplace.rectTransform.sizeDelta.x;
        }


        private void Start()
        {
            if (LoadOnStart) Refresh();
        }


        private void GetImage()
        {
            if (ToReplace == null) ToReplace = GetComponentInChildren<Image>();
        }


        public void Refresh()
        {
            GetImage();

            if (ToReplace.sprite == null) ToReplace.sprite = FIcons_Manager.Get.DefaultLoadSprite;

            if (LoadingPath == FE_LoadingPath.OtherTexture)
                FIcons_Manager.Get.LoadSpriteManaged(OtherTexture, ToReplace, NewWidth, NewHeight, TargetFilter, AutoSetWidthHeight, LoadingAnimation, ScaleDownSupport, LossyScaleSync);
            else
                FIcons_Manager.Get.LoadSpriteManaged(PathTo, ToReplace, NewWidth, NewHeight, TargetFilter, AutoSetWidthHeight, LoadingAnimation, LoadingPath, ScaleDownSupport, LossyScaleSync);
        }


        private void OnValidate()
        {
            if (NewWidth < 1) NewWidth = 1;
            if (NewHeight < 0) NewHeight = 0;
        }
    }

}
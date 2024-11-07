using FIMSpace.FTex;
using UnityEngine;

namespace FIMSpace.FIcons
{
    public class FIcon_ScaleLanczosAsync : FThread
    {
        public Color32[] ScaledPixels { get; private set; }

        private Color32[] textureBytes;
        private readonly int sourceWidth;
        private readonly int sourceHeight;
        private readonly int targetWidth;
        private readonly int targetHeight;

        public FIcon_ScaleLanczosAsync(Color32[] textureBytes, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
        {
            ScaledPixels = null;
            this.textureBytes = textureBytes;
            this.sourceWidth = sourceWidth;
            this.sourceHeight = sourceHeight;
            this.targetWidth = targetWidth;
            this.targetHeight = targetHeight;
        }

        protected override void ThreadOperations()
        {
            ScaledPixels = FTex_ScaleLanczos.ScaleTexture(textureBytes, sourceWidth, sourceHeight, targetWidth, targetHeight);
            textureBytes = null;
        }

        public void ClearPixels()
        {
            ScaledPixels = null;
        }
    }
}

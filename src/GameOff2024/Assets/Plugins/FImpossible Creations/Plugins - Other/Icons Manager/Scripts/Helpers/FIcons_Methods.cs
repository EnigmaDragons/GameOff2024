using FIMSpace.FTex;
using System.Collections;
using UnityEngine;

#if UNITY_2017_1_OR_NEWER
using UnityEngine.Networking;
#endif

#if ADDRESSABLES_IMPORTED
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace FIMSpace.FIcons
{
    public class FIcons_Methods
    {
        /// <summary>
        /// Loading image from the streaming assets folder, scalling it with algorithm to new size and delivering it in 'Action' as Sprite type
        /// </summary>
        /// <param name="assetPath"> Image inside "Assets/StreamingAssets" directory, write here for example "FIconScaler/Mask1.png" ! REMEMBER ABOUT EXTENSION like '.png' !</param>
        /// <param name="newWidth"> New width of loaded sprite, UI image should have the same dimensions (Like 'Set Native Size')</param>
        /// <param name="newHeight"> (set 0 for auto-Preserve Aspect) New height of loaded sprite, UI image should have the same dimensions (Like 'Set Native Size')</param>
        /// <param name="deliverAsset"> Action to which will be forwarded loaded sprite after load </param>
        /// <param name="otherTexture"> If you using FE_LoadingPath.OtherTexture you put here texture image you want to get but rescaled, then assetPath can be empty </param>
        /// <param name="quality"> Sample count for scaling algorithm, it's recommended to use low values like 1-4 (max 8), if you want scale image up to bigger size than original, you can try use higher quality value - then lower value will give a bit pixelate effect </param>
        /// <param name="supportScaling"> Supporting better look of sprite when scaling it down ! use Bilinear filter with this option ! (image will not be very sharp now)</param>
        public static IEnumerator LoadScaledSprite(string assetPath, System.Action<Sprite> deliverAsset, int newWidth, int newHeight = 0, FilterMode filterMode = FilterMode.Point, FE_LoadingPath pathType = FE_LoadingPath.StreamingAssets, Texture2D otherTexture = null, int quality = 4, bool supportScaling = false)
        {
            string path = "";

            Texture2D sourceTexture = null;
            Texture2D processedTexture = null;
            bool source = true;

#region Supporting different types of texture source

            switch (pathType)
            {
                case FE_LoadingPath.StreamingAssets:
                case FE_LoadingPath.WWW:

                    if (pathType == FE_LoadingPath.StreamingAssets)
                        path = System.IO.Path.Combine(Application.streamingAssetsPath, assetPath);
                    else
                        path = assetPath;

#if UNITY_2017_1_OR_NEWER
                    UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);
                    yield return request.SendWebRequest();

                    if (request.isDone && string.IsNullOrEmpty(request.error))
                    {
                        processedTexture = DownloadHandlerTexture.GetContent(request);
#else
                    WWW request = new WWW(path);
                    yield return request;

                    if (request.bytes.Length != 0)
                    {
                        processedTexture = request.texture;
#endif

                        sourceTexture = processedTexture;

                        // Automatic keeping aspect ratio
                        if (newHeight <= 0) newHeight = AspectRatioHeight(sourceTexture.width, sourceTexture.height, newWidth);

                        source = false;
                        request.Dispose();
                    }
                    else
                    {
                        Debug.LogError("[ICONS MANAGER] Failed to load icon from path: " + assetPath + " !");
                        request.Dispose();
                        yield break;
                    }


                    break;

                case FE_LoadingPath.Resources:
                    path = assetPath;

                    ResourceRequest resRequest = Resources.LoadAsync<Texture>(path);
                    yield return resRequest;

                    if (resRequest.asset != null)
                        sourceTexture = resRequest.asset as Texture2D;
                    else
                    {
                        Debug.LogError("[ICONS MANAGER] Failed to load texture from Resources path: " + path + " !");
                        yield break;
                    }

                    // Automatic keeping aspect ratio
                    if (newHeight <= 0) newHeight = AspectRatioHeight(sourceTexture.width, sourceTexture.height, newWidth);

                    processedTexture = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);

                    break;

                case FE_LoadingPath.OtherTexture:

                    if (otherTexture == null)
                    {
                        Debug.LogError("[ICONS MANAGER] Failed to load texture !");
                        yield break;
                    }

                    sourceTexture = otherTexture;

                    // Automatic keeping aspect ratio
                    if (newHeight <= 0) newHeight = AspectRatioHeight(sourceTexture.width, sourceTexture.height, newWidth);

                    processedTexture = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);

                    break;
            }

#endregion

            if (processedTexture != null)
            {
                processedTexture.filterMode = FilterMode.Point;

                if (processedTexture.width != newWidth && !source || source) // If new texture target size is different than original size we have to scale it
                {
                    Color32[] lanczosedPixels;

                    try
                    {
                        lanczosedPixels = FTex_ScaleLanczos.ScaleTexture(sourceTexture.GetPixels32(), sourceTexture.width, sourceTexture.height, newWidth, newHeight, quality);
                    }
                    catch (System.Exception)
                    {
                        if (source)
                        {
#region Rendering texture (skipping Read/Write enabled requirement)

                            // Pre calculations etc.
                            FilterMode preFilter = sourceTexture.filterMode;
                            sourceTexture.filterMode = FilterMode.Point;

                            RenderTexture preActive = RenderTexture.active;

                            // Rendering source texture onto new readable texture
                            RenderTexture renderingTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height);
                            renderingTexture.filterMode = FilterMode.Point;
                            RenderTexture.active = renderingTexture;
                            Graphics.Blit(sourceTexture, renderingTexture);

                            // Getting pixels on texture and applying them
                            Texture2D renderedReadableTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
                            renderedReadableTexture.ReadPixels(new Rect(0, 0, sourceTexture.width, sourceTexture.height), 0, 0);
                            renderedReadableTexture.Apply();
                            sourceTexture.filterMode = preFilter;

                            // Getting desired data and finishing loading texture job
                            lanczosedPixels = FTex_ScaleLanczos.ScaleTexture(renderedReadableTexture.GetPixels32(), sourceTexture.width, sourceTexture.height, newWidth, newHeight);

                            // Cleaning
                            RenderTexture.ReleaseTemporary(renderingTexture);
                            RenderTexture.active = preActive;

                            GameObject.Destroy(renderedReadableTexture);
                            GameObject.Destroy(renderingTexture);

#endregion
                        }
                        else
                        {
                            lanczosedPixels = null;
                            Debug.LogError("[ICONS MANAGER] Failed to load icon from path: " + assetPath + " !");
                            yield break;
                        }
                    }


                    if (!source) processedTexture.Reinitialize(newWidth, newHeight); // Setting new size for the target sprite texture
                    processedTexture.SetPixels32(lanczosedPixels); // Applying new pixels to the texture
                    processedTexture.Apply(supportScaling, true);

                    processedTexture.filterMode = filterMode;

                    //if ( pathType == FE_LoadingPath.Resources ) GameObject.Destroy(sourceTexture);
                }
            }
            else
            {
                Debug.LogError("[ICONS MANAGER] Failed to load icon from path: " + assetPath + " !");
                yield break;
            }

            Sprite sprite = Sprite.Create(processedTexture as Texture2D, new Rect(0, 0, processedTexture.width, processedTexture.height), Vector2.one / 2f);

            if (sprite)
            {
                sprite.name = assetPath;

#if UNITY_EDITOR
                // We naming sprites inside unity edtitor for debugging purposes
                sprite.name = "NOT MANAGED_" + System.IO.Path.GetFileName(assetPath) + " " + newWidth + "x" + newHeight + " F: " + filterMode;
#endif

                if (deliverAsset != null) deliverAsset(sprite);
            }
            else
            {
                Debug.LogError("[ICONS MANAGER] Failed to load icon from path: " + assetPath + " !");
            }

            yield return null;
        }


        /// <summary>
        /// Loading image from the streaming assets folder without scalling it and resizing, just source texture as base for rescalling
        /// <param name="streamingAssetsPath"> Image inside "Assets/StreamingAssets" directory, write here for example "FIconScaler/Mask1.png" ! REMEMBER ABOUT EXTENSION like '.png' !</param>
        /// <param name="deliverAsset"> Action to which will be forwarded loaded texture after load </param>
        /// </summary>
        public static IEnumerator LoadTexture(string streamingAssetsPath, System.Action<Texture2D> deliverAsset)
        {
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, streamingAssetsPath);

#if UNITY_2017_1_OR_NEWER
            UnityWebRequest request = UnityWebRequestTexture.GetTexture("file://" + path);
            yield return request.SendWebRequest();

            if (request.isDone && string.IsNullOrEmpty(request.error))
            {
                Texture2D processedTexture = DownloadHandlerTexture.GetContent(request);
#else
            WWW request = new WWW("file://" + path);
            yield return request;

            if (request.bytes.Length != 0)
            {
                Texture2D processedTexture = request.texture;
#endif
                if (processedTexture != null)
                {
                    processedTexture.filterMode = FilterMode.Point;
                    if (deliverAsset != null) deliverAsset(processedTexture);
                }
                else
                {
                    Debug.LogError("[ICONS MANAGER] Failed to load icon from path: " + streamingAssetsPath + " !");
                    yield break;
                }
            }
            else
            {
                Debug.LogError("[ICONS MANAGER] Failed to load icon from path: " + streamingAssetsPath + " !");
                yield break;
            }

            yield break;
        }


        /// <summary>
        /// Downloading image from the url without scalling it and resizing, just source texture as base for rescalling
        /// <param name="textureUrl"> Image www url, write here for example "http://filipmoeglich.pl/FIconsManagerExample/Apple.png" ! REMEMBER ABOUT EXTENSION like '.png' !</param>
        /// <param name="deliverAsset"> Action to which will be forwarded loaded texture after load </param>
        /// </summary>
        public static IEnumerator DownloadTexture(string textureUrl, System.Action<Texture2D> deliverAsset)
        {
#if UNITY_2017_1_OR_NEWER
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(textureUrl);
            yield return request.SendWebRequest();

            if (request.isDone && string.IsNullOrEmpty(request.error))
            {
                Texture2D processedTexture = DownloadHandlerTexture.GetContent(request);
#else
            WWW request = new WWW(textureUrl);
            yield return request;

            if (request.bytes.Length != 0)
            {
                Texture2D processedTexture = request.texture;
#endif
                if (processedTexture != null)
                {
                    processedTexture.filterMode = FilterMode.Point;
                    if (deliverAsset != null) deliverAsset(processedTexture);
                }
                else
                {
                    Debug.LogError("[ICONS MANAGER] Failed to load icon from path: " + textureUrl + " !");
                    yield break;
                }
            }
            else
            {
                Debug.LogError("[ICONS MANAGER] Failed to load icon from path: " + textureUrl + " !");
                yield break;
            }

            yield break;
        }


        /// <summary>
        /// Loading image from the resources path without scalling it and resizing, just source texture as base for rescalling
        /// <param name="path"> Image resources path, write here for example "FIconsManagerExample/Apple" ! DON'T USE EXTENSIONS like '.png' !</param>
        /// <param name="deliverAsset"> Action to which will be forwarded loaded texture after load </param>
        /// </summary>
        public static IEnumerator LoadTextureResources(string path, System.Action<Texture2D> deliverAsset)
        {
            ResourceRequest request = Resources.LoadAsync<Texture>(path);
            yield return request;

            if (request.asset != null)
            {
                Texture2D loadedTexture = request.asset as Texture2D;

                if (loadedTexture != null)
                {
                    loadedTexture.filterMode = FilterMode.Point;
                    if (deliverAsset != null) deliverAsset(loadedTexture);
                }
                else
                {
                    Debug.LogError("[ICONS MANAGER] Failed to load icon from path: " + path + " !");
                    yield break;
                }
            }
            else
            {
                Debug.LogError("[ICONS MANAGER] Failed to load icon from path: " + path + " !");
                yield break;
            }

            yield break;
        }


#if ADDRESSABLES_IMPORTED

        /// <summary>
        /// Loading image from the addressable path without scalling it and resizing, just source texture as base for rescalling
        /// <param name="addressablePath"> Image addressable path, write here for example "Assets/FImpossible Games/Icons Manager/Demo - Icons Manager/Sprites/Boots.png" after setting this asset addresable </param>
        /// <param name="deliverAsset"> Action to which will be forwarded loaded texture after load </param>
        /// </summary>
        public static IEnumerator LoadTextureAddressable(string addressablePath, System.Action<Texture2D> deliverAsset)
        {
            // Addressables
            AsyncOperationHandle request = Addressables.LoadAssetAsync<Texture2D>(addressablePath);
            yield return request;

            if (request.Result != null)
            {
                Texture2D loadedTexture = request.Result as Texture2D;

                if (loadedTexture != null)
                {
                    loadedTexture.filterMode = FilterMode.Point;
                    if (deliverAsset != null) deliverAsset(loadedTexture);
                }
                else
                {
                    Debug.LogError("[ICONS MANAGER] Failed to load icon from path: " + addressablePath + " !");
                    yield break;
                }
            }
            else
            {
                Debug.LogError("[ICONS MANAGER] Failed to load icon from path: " + addressablePath + " !");
                yield break;
            }

            yield break;
        }

#endif

        /// <summary>
        /// Change height value to fit aspect ratio of image's width
        /// </summary>
        public static int AspectRatioHeight(int sourceWidth, int sourceHeight, int targetWidth)
        {
            float ratio = (float)sourceWidth / (float)targetWidth;
            return (int)Mathf.Ceil(sourceHeight / ratio);
        }

    }
}
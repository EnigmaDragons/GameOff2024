using FIMSpace.FTex;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FIcons
{
    /// <summary>
    /// FM: Class which is holding reference to source loaded texture and servicing scaling it to different sizes and generating sprites
    /// </summary>
    public class FIcon_TextureContainer : IDisposable
    {
        /// <summary> Path to asset </summary>
        public string PathToSourceTexture { get; private set; }

        /// <summary> Hash created out of texture path to identify collections with certain textures </summary>
        public int TextureHash = -1;

        /// <summary> On what state of loading is current progress for this texture </summary>
        public FE_TextureLoadState SourceTextureLoadState { get; private set; }

        //public Texture2D SourceTexture { get; private set; }
        public int SourceWidth { get; private set; }
        public int SourceHeight { get; private set; }
        public Color32[] SourcePixels { get; private set; }

        /// <summary> Size of source texture and all generated sprites associated with this container </summary>
        public float TotalSizeInMB { get; private set; }

        /// <summary> Last time when container was used </summary>
        public int LastUse { get; private set; }

        /// <summary> If something can be unloaded from this container </summary>
        public bool UnloadPossibility { get; private set; }

        /// <summary> Requests for texture container to give sprites </summary>
        public List<FIcon_LoadRequest> Requests { get; private set; }

        /// <summary> How we are loading source texture </summary>
        public FE_LoadingPath LoadingPathType { get; private set; }

        /// <summary> Used if we are loading source texture using other texture </summary>
        public Texture2D OtherOriginalTexture { get; private set; }

        /// <summary> Generated sprites in different scales by this texture, when generated sprites count is zero and texture was not loaded from project files, texture can be unloaded </summary>
        private Dictionary<int, FIcon_GeneratedSprite> generatedSprites;

        /// <summary> Sprites which are waiting or are being generated </summary>
        private List<int> waitingToStartLoadingSprites;

        /// <summary> Sprite which was lately generated </summary>
        private Sprite newestGeneratedSprite;



        public FIcon_TextureContainer(string path, FE_LoadingPath loadingPath)
        {
            PathToSourceTexture = path;
            TextureHash = PathToSourceTexture.GetHashCode();
            SourcePixels = null;

            SourceTextureLoadState = FE_TextureLoadState.None;
            LoadingPathType = loadingPath;

            InitContainer();
        }

        public FIcon_TextureContainer(Texture2D originTexture)
        {
            PathToSourceTexture = originTexture.name;
            TextureHash = PathToSourceTexture.GetHashCode();
            OtherOriginalTexture = originTexture;

            SourceTextureLoadState = FE_TextureLoadState.None;
            LoadingPathType = FE_LoadingPath.OtherTexture;
            InitContainer();
        }

        private void InitContainer()
        {
            Disposed = false;
            SourcePixels = null;

            SourceWidth = 0;
            SourceHeight = 0;

            Requests = new List<FIcon_LoadRequest>();

            generatedSprites = new Dictionary<int, FIcon_GeneratedSprite>();
            waitingToStartLoadingSprites = new List<int>();

            UnloadPossibility = false;
            LastUse = Time.frameCount;
        }


        #region Source Texture Loading



        private bool viewedReadWriteInfo = false;

        /// <summary>
        /// Setting loading texture flag
        /// </summary>
        internal void LoadTexture()
        {
            SourceTextureLoadState = FE_TextureLoadState.Loading;
        }

        /// <summary>
        /// Getting pixels from other texture, rendering readable texture if provided is not read/writeable
        /// </summary>
        internal void GetFromTexture(Texture2D tex)
        {
            if (OtherOriginalTexture == null) OtherOriginalTexture = tex;

            bool renderTexApproach = true;
            if (OtherOriginalTexture.isReadable) renderTexApproach = false;

            if (renderTexApproach == false)
            {
                try
                {
                    SourcePixels = OtherOriginalTexture.GetPixels32();
                    SourceWidth = OtherOriginalTexture.width;
                    SourceHeight = OtherOriginalTexture.height;
                    OnLoadTextureFinish(OtherOriginalTexture, false);
                }
                catch (UnityException)
                // Generating readable temporary texture if texture is not readable
                {
                    renderTexApproach = true;
                }
            }

            if (renderTexApproach)
            // Generating readable temporary texture if texture is not readable
            {
                #region Editor Warning info about render texture
#if UNITY_EDITOR
                if (!viewedReadWriteInfo)
                    Debug.LogWarning("[ICONS MANAGER - Log not viewed in build] Texture which you're trying to rescale don't have read/write enabled, so algorithm will use render texture to get it's pixels");
                viewedReadWriteInfo = true;
#endif
                #endregion

                // Pre calculations etc.
                FilterMode preFilter = OtherOriginalTexture.filterMode;
                OtherOriginalTexture.filterMode = FilterMode.Point;

                RenderTexture preActive = RenderTexture.active;

                SourceWidth = tex.width;
                SourceHeight = tex.height;

                // Rendering source texture onto new readable texture
                RenderTexture renderingTexture = RenderTexture.GetTemporary(OtherOriginalTexture.width, OtherOriginalTexture.height);
                renderingTexture.filterMode = FilterMode.Point;
                RenderTexture.active = renderingTexture;
                Graphics.Blit(OtherOriginalTexture, renderingTexture);

                // Getting pixels on texture and applying them
                Texture2D renderedReadableTexture = new Texture2D(OtherOriginalTexture.width, OtherOriginalTexture.height);
                renderedReadableTexture.ReadPixels(new Rect(0, 0, OtherOriginalTexture.width, OtherOriginalTexture.height), 0, 0);
                renderedReadableTexture.Apply();
                OtherOriginalTexture.filterMode = preFilter;

                // Cleaning
                RenderTexture.ReleaseTemporary(renderingTexture);
                RenderTexture.active = preActive;

                // Getting desired data and finishing loading texture job
                SourcePixels = renderedReadableTexture.GetPixels32();
                OnLoadTextureFinish(renderedReadableTexture);
            }
        }


        /// <summary>
        /// Assigning new loaded texture to container
        /// </summary>
        internal void OnLoadTextureFinish(Texture2D texture, bool destroyTextureAfter = true)
        {
            try
            {
                //SourceTexture = texture;
                if (SourcePixels == null)
                {
                    SourcePixels = texture.GetPixels32();
                    SourceWidth = texture.width;
                    SourceHeight = texture.height;
                }

                if (destroyTextureAfter)
                {
                    // Unloading texture after loading needed data (we need only pixels array)
                    // loaded texture would need 'Read/Write' enabled which is taking big amount of memory so we get rid of it
                    GameObject.Destroy(texture);
                }

                TotalSizeInMB = GetTotalUsedMemory();
                SourceTextureLoadState = FE_TextureLoadState.Loaded;
                LastUse = Time.frameCount;
            }
            catch (Exception exc)
            {
                SourceTextureLoadState = FE_TextureLoadState.Error;
                Debug.LogError("[ICONS MANAGER] Error occured during loading texture for the container from path " + PathToSourceTexture + " exception: " + exc);
            }

            OtherOriginalTexture = null;
        }


        /// <summary>
        /// Setting reload flag
        /// </summary>
        internal void CanBeReloaded()
        {
            SourceTextureLoadState = FE_TextureLoadState.None;
        }


        #endregion


        /// <summary>
        /// Requesting for sprite to be generated by texture container
        /// </summary>
        public void AskForSprite(FIcons_LoadTask task, int newWidth, int newHeight = 0, FilterMode filterMode = FilterMode.Point, bool scaleDownSupport = false)
        {
            bool exists = false;
            int targetId = FIcon_GeneratedSprite.GetIdFrom(newWidth, newHeight, filterMode);

            // Checking if we already intending to load such sprite
            for (int i = 0; i < Requests.Count; i++) if (Requests[i].Id == targetId) { exists = true; break; }

            if (!exists)
            {
                Requests.Add(new FIcon_LoadRequest(newWidth, newHeight, filterMode, targetId, this, task, scaleDownSupport));
                waitingToStartLoadingSprites.Add(targetId);
            }
        }


        /// <summary>
        /// Generating scaled sprite for provided request
        /// </summary>
        public Sprite ExecuteRequest(FIcon_LoadRequest request)
        {
            #region Remove sprite from waiting state

            for (int i = 0; i < waitingToStartLoadingSprites.Count; i++)
                if (waitingToStartLoadingSprites[i] == request.Id)
                {
                    waitingToStartLoadingSprites.RemoveAt(i);
                    break;
                }

            #endregion

            #region Checking if source texture exists

            if (SourcePixels == null)
            {
                if (generatedSprites.Count > 0)
                {
                    if (newestGeneratedSprite)
                    {
                        Debug.LogWarning("[ICONS MANAGER] There is no source texture but sprite request occured, returning last generated sprite");
                        return newestGeneratedSprite;
                    }
                    else
                    {
                        Debug.LogError("[ICONS MANAGER] There is no source texture and no sprites in texture container!");
                        return FIcons_Manager.Get.ErrorSprite;
                    }
                }
                else
                {
                    Debug.LogError("[ICONS MANAGER] There is no source texture and no sprites in texture container!");
                    return FIcons_Manager.Get.ErrorSprite;
                }
            }

            #endregion

            #region Checking if requested sprite already exists


            FIcon_GeneratedSprite generatedSprite = null;

            if (generatedSprites.TryGetValue(request.Id, out generatedSprite))
            {
                if (generatedSprite.LoadState == FE_SpriteLoadState.Loaded)
                {
                    Sprite genSpriteRef = generatedSprite.GetSprite();
                    request.SetState(FE_RequestState.Complete);
                    //RedefineOldest();
                    //request.Complete = true;

                    request.Task.OnSpriteAcquired(generatedSprite);
                    return genSpriteRef;
                }
            }

            #endregion

            try
            {
                // If we scaling asynchronously, we delaying executing task until sprite is scaled and generated with enough time to not lag main thread
                if (FIcons_Manager.Get.ScaleAsync)
                {

                }

                // Scalling original texture with lanczos
                Color32[] scaledPixels = FTex_ScaleLanczos.ScaleTexture(SourcePixels, SourceWidth, SourceHeight, request.Width, request.Height);

                // Assigning scaled texture to new texture which will be used by sprite
                Texture2D newTexture = new Texture2D(request.Width, request.Height, TextureFormat.RGBA32, request.ScaleDown);
                newTexture.SetPixels32(scaledPixels);
                newTexture.Apply(request.ScaleDown, true); // Making texture not readable again (memory optimization)

                newTexture.filterMode = request.Filter;

                // Creating sprite and assigning scaled texture
                Sprite sprite = Sprite.Create(newTexture, new Rect(0, 0, request.Width, request.Height), new Vector2(0.5f, 0.5f));

                #region Editor Debug Naming

#if UNITY_EDITOR
                // We naming sprites inside unity edtitor for debugging purposes
                sprite.name = System.IO.Path.GetFileName(PathToSourceTexture) + " " + request.Width + "x" + request.Height + " F: " + request.Filter;
#endif

                #endregion

                if (generatedSprite == null)
                {
                    generatedSprite = new FIcon_GeneratedSprite(request);
                    generatedSprites.Add(request.Id, generatedSprite);
                }

                generatedSprite.AssignSprite(sprite, this);
                //RedefineOldest();
                TotalSizeInMB = GetTotalUsedMemory();
                newestGeneratedSprite = sprite;

                request.Task.OnSpriteAcquired(generatedSprite);
                request.SetState(FE_RequestState.Complete);

                if (sprite != null) return sprite;
            }
            catch (Exception exc)
            {
                Debug.LogError("[ICONS MANAGER] There was error during generating sprite from texture at path " + PathToSourceTexture + " | Exception: " + exc);
            }

            return FIcons_Manager.Get.ErrorSprite;
        }


        /// <summary>
        /// Delivering scaled sprite for provided request, if it not exists running generating scaled sprite algorithm
        /// </summary>
        public void ServiceRequest(FIcon_LoadRequest request)
        {
            #region Remove sprite from wait to start loading state

            for (int i = 0; i < waitingToStartLoadingSprites.Count; i++)
                if (waitingToStartLoadingSprites[i] == request.Id)
                {
                    waitingToStartLoadingSprites.RemoveAt(i);
                    break;
                }

            #endregion


            #region Checking if requested sprite already exists

            FIcon_GeneratedSprite generatedSprite = null;

            if (generatedSprites.TryGetValue(request.Id, out generatedSprite))
            {

                if (generatedSprite.LoadState == FE_SpriteLoadState.Loading)
                {
                    if (generatedSprite.GeneratedSprite == null) generatedSprite.CheckLoadState();

                    if (generatedSprite.ScalingThread != null)
                        if (generatedSprite.ScalingThread.IsDone)
                        {
                            OnReceiveScaledPixels(request, generatedSprite, generatedSprite.ScalingThread.ScaledPixels, request.ScaleDown);
                            generatedSprite.ScalingThread.ClearPixels();
                            generatedSprite.SetThread(null);
                            return;
                        }
                }

                if (generatedSprite.LoadState == FE_SpriteLoadState.Loaded)
                {
                    request.Task.OnSpriteAcquired(generatedSprite);
                    request.SetState(FE_RequestState.Complete);
                    return;
                }
            }

            #endregion


            #region Checking if source texture exists

            if (SourcePixels == null)
            {
                if (generatedSprites.Count > 0)
                {
                    if (newestGeneratedSprite)
                    {
                        Debug.LogWarning("[ICONS MANAGER] There is no source texture but sprite request occured, returning last generated sprite");
                        request.SetState(FE_RequestState.Error);
                        return;
                    }
                    else
                    {
                        Debug.LogError("[ICONS MANAGER] There is no source texture and no sprites in texture container!");
                        request.SetState(FE_RequestState.Error);
                        return;
                    }
                }
                else
                {
                    Debug.LogError("[ICONS MANAGER] There is no source texture and no sprites in texture container!");
                    request.SetState(FE_RequestState.Error);
                    return;
                }
            }

            #endregion


            // Running generating sprite stuff
            if (generatedSprite == null || generatedSprite.LoadState == FE_SpriteLoadState.Unloaded)
            {
                try
                {
                    if (generatedSprite == null)
                    {
                        generatedSprite = new FIcon_GeneratedSprite(request);
                        generatedSprites.Add(request.Id, generatedSprite);
                    }

                    if (!FIcons_Manager.Get.ScaleAsync)
                    {
                        // Scalling original texture with lanczos
                        Color32[] scaledPixels = FTex_ScaleLanczos.ScaleTexture(SourcePixels, SourceWidth, SourceHeight, request.Width, request.Height);
                        OnReceiveScaledPixels(request, generatedSprite, scaledPixels, request.ScaleDown);
                    }
                    else
                    {
                        // If we scaling asynchronously we running algorithm in other thread
                        generatedSprite.SetThread(new FIcon_ScaleLanczosAsync(SourcePixels, SourceWidth, SourceHeight, request.Width, request.Height));
                    }
                }
                catch (Exception exc)
                {
                    Debug.LogError("[ICONS MANAGER] There was error during generating sprite from texture at path " + PathToSourceTexture + " | Exception: " + exc);
                    //generatedSprite.AssignSprite(FIcons_Manager.Get.ErrorSprite, this);
                    //request.Task.OnSpriteAcquired(generatedSprite);
                    request.SetState(FE_RequestState.Error);
                    return;
                }
            }
        }


        /// <summary>
        /// Finishing creating generatedSprite class with actual scaled sprite
        /// </summary>
        private void OnReceiveScaledPixels(FIcon_LoadRequest request, FIcon_GeneratedSprite generatedSprite, Color32[] pixels, bool scaleDown)
        {
            // Assigning scaled texture to new texture which will be used by sprite
            Texture2D newTexture = new Texture2D(request.Width, request.Height, TextureFormat.RGBA32, scaleDown);

            newTexture.SetPixels32(pixels);
            newTexture.Apply(scaleDown, true); // Making texture not readable again (memory optimization)

            newTexture.filterMode = request.Filter;

            // Creating sprite and assigning scaled texture
            Sprite sprite = Sprite.Create(newTexture, new Rect(0, 0, request.Width, request.Height), new Vector2(0.5f, 0.5f));

            #region Editor Debug Naming

#if UNITY_EDITOR
            // We naming sprites inside unity edtitor for debugging purposes
            sprite.name = System.IO.Path.GetFileName(PathToSourceTexture) + " " + request.Width + "x" + request.Height + " F: " + request.Filter;
#endif

            #endregion

            generatedSprite.AssignSprite(sprite, this);
            TotalSizeInMB = GetTotalUsedMemory();
            newestGeneratedSprite = sprite;

            request.Task.OnSpriteAcquired(generatedSprite);
            request.SetState(FE_RequestState.Complete);
        }


        /// <summary>
        /// Returning sprite of this texture with provided size, if not it returns null
        /// </summary>
        public Sprite GetSprite(int width, int height, FilterMode filter)
        {
            return GetSprite(FIcon_GeneratedSprite.GetIdFrom(width, height, filter));
        }

        /// <summary>
        /// Returning sprite of this texture with provided id, if not it returns null
        /// </summary>
        public Sprite GetSprite(int id)
        {
            FIcon_GeneratedSprite generated = GetGeneratedSprite(id);
            if (generated != null) return generated.GetSprite();
            return null;
        }

        /// <summary>
        /// Returning generated sprite source container class of this texture container with provided id, if not it returns null
        /// </summary>
        public FIcon_GeneratedSprite GetGeneratedSprite(int id)
        {
            FIcon_GeneratedSprite generated;

            if (generatedSprites.TryGetValue(id, out generated))
            {
                LastUse = Time.frameCount;
                return generated;
            }

            return null;
        }


        /// <summary>
        /// Checking if task to load certain sprite is launched
        /// </summary>
        public bool IsSpriteWaiting(int width, int height, FilterMode filter)
        {
            return IsSpriteWaiting(FIcon_GeneratedSprite.GetIdFrom(width, height, filter));
        }

        /// <summary>
        /// Checking if task to load certain sprite is launched
        /// </summary>
        public bool IsSpriteWaiting(int id)
        {
            return waitingToStartLoadingSprites.Contains(id);
        }


        /// <summary>
        /// Trying to unload not needed references from memory, one per try, each time the oldest one
        /// </summary>
        /// <returns> Returning cleaned amount of memory in MB </returns>
        public float SafeUnload()
        {
            float unloaded = 0f;

            // If there is no generated sprites for the container - already unloaded and we trying unload texture in second loop of unloading
            if (!GeneratedSpritesExists())
            {
                if (SourcePixels != null)
                {
                    if (!FIcons_Manager.Get.AllowUnloadSources)
                    {
                        UnloadPossibility = false;
                        return unloaded;
                    }
                    else
                    {
                        // Try to unload source texture and container
                        unloaded = CalculateUsedMemory(SourceWidth, SourceHeight);
                        SourcePixels = null;
                        FIcons_Manager.Get.PurgeContainer(this);
                    }
                }
                else
                    UnloadPossibility = false;

                return unloaded;
            }

            // Getting sprites which can be unloaded (references doesn't exist)
            List<FIcon_GeneratedSprite> canBeUnloaded = new List<FIcon_GeneratedSprite>();

            foreach (var generated in generatedSprites.Values)
            {
                if (generated.CanBeUnloaded()) canBeUnloaded.Add(generated);
            }

            // If we can't unload any sprite then we don't do anything
            if (canBeUnloaded.Count == 0)
            {
                return unloaded;
            }

            // If we can unload only one sprite, let's do this
            if (canBeUnloaded.Count == 1)
            {
                unloaded += canBeUnloaded[0].SizeInMB;
                canBeUnloaded[0].PurgeGeneratedSprite();
                TotalSizeInMB = GetTotalUsedMemory();
                // We keep generatedSprite class in the dictionary to reload sprites in references when sprite will be required to view again (unload deactivated option)
            }
            else // If we can unload multiple sprites
            {
                for (int i = 1; i < canBeUnloaded.Count; i++)
                {
                    unloaded += canBeUnloaded[i].SizeInMB;
                    canBeUnloaded[i].PurgeGeneratedSprite();
                }

                TotalSizeInMB = GetTotalUsedMemory();

                //Debug.Log("[ICONS MANAGER] few generated sprite unloaded " + unloaded);
            }

            if (canBeUnloaded.Count == generatedSprites.Count)
            {
                if (!FIcons_Manager.Get.AllowUnloadSources) UnloadPossibility = false;
            }

            return unloaded;
        }

        /// <summary>
        /// Updating info if container can unload something
        /// </summary>
        public void UnloadingInformation(bool spriteCanUnload)
        {
            LastUse = Time.frameCount;

            if (generatedSprites.Count == 0) // If there is no sprites generated
            {
                if (SourcePixels != null) // If there is source texture
                {
                    if (FIcons_Manager.Get.AllowUnloadSources)
                        UnloadPossibility = true;
                    else
                        UnloadPossibility = false;
                }
                else UnloadPossibility = false;
            }
            else // If any sprite is generated
            {
                if (spriteCanUnload) // If information is letting unloading one of the sprites
                {
                    UnloadPossibility = true;
                }
                else // If information is not letting unloading sprite
                {
                    UnloadPossibility = false;

                    if (generatedSprites.Count > 1)
                        foreach (var generated in generatedSprites.Values) // We checking if any other sprite is letting unloading
                        {
                            if (generated.CanBeUnloaded())
                            {
                                UnloadPossibility = true;
                                break;
                            }
                        }
                }
            }
        }

        /// <summary>
        /// Unloading source texture (it takes the biggest space in memory), when new sprite will try to generate source texture will be loaded again
        /// </summary>
        /// <returns> Returning cleaned amount of memory in MB </returns>
        internal float UnloadJustSourceTexture()
        {
            float unloaded = 0;

            if (SourcePixels != null)
            {
                unloaded = CalculateUsedMemory(SourceWidth, SourceHeight);
                SourcePixels = null;
                SourceTextureLoadState = FE_TextureLoadState.Unloaded;
                TotalSizeInMB = GetTotalUsedMemory();
            }

            return unloaded;
        }


        #region Utility Methods

        /// <summary>
        /// If there is any sprite generated in the container
        /// </summary>
        public bool GeneratedSpritesExists()
        {
            if (generatedSprites.Count == 0) return false;

            foreach (var generated in generatedSprites.Values)
                if (generated.GeneratedSprite != null) return true; // If any sprite exists that means there is still something which needs container

            return false;
        }

        /// <summary>
        /// Checking if sprite with given id exists and is loaded in texture container generated list
        /// </summary>
        internal bool GeneratedSpriteAvailable(int targetId)
        {
            FIcon_GeneratedSprite generated = null;
            if (generatedSprites.TryGetValue(targetId, out generated))
            {
                if (generated.LoadState == FE_SpriteLoadState.Unloaded) return false;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Checking if source texture can be unloaded
        /// </summary>
        private bool CanBeUnloaded()
        {
            if (!FIcons_Manager.Get.AllowUnloadSources) return false;

            bool can = true;

            int fullScaleId = FIcon_GeneratedSprite.GetIdFrom(SourceWidth, SourceHeight, FilterMode.Point);
            FIcon_GeneratedSprite fullScale;

            // Checking different filter sprites, if they are generated and can be unloaded
            if (generatedSprites.TryGetValue(fullScaleId, out fullScale))
            {
                if (!fullScale.CanBeUnloaded()) can = false;
                if (can)
                {
                    fullScaleId = FIcon_GeneratedSprite.GetIdFrom(SourceWidth, SourceHeight, FilterMode.Bilinear);
                    if (generatedSprites.TryGetValue(fullScaleId, out fullScale))
                    {
                        if (!fullScale.CanBeUnloaded()) can = false;
                        if (can)
                        {
                            fullScaleId = FIcon_GeneratedSprite.GetIdFrom(SourceWidth, SourceHeight, FilterMode.Trilinear);
                            if (generatedSprites.TryGetValue(fullScaleId, out fullScale)) if (!fullScale.CanBeUnloaded()) can = false;
                        }
                    }
                }
            }

            return can;
        }


        /// <summary>
        /// Computing size of texture and associated sprites in memory in MB
        /// </summary>
        public float GetTotalUsedMemory()
        {
            float size = 0f;

            if (SourcePixels != null) size += CalculateUsedMemory(SourceWidth, SourceHeight);
            foreach (var generated in generatedSprites.Values) size += generated.SizeInMB;

            return size;
        }


        /// <summary>
        /// Computing size of associated sprites in memory in MB
        /// </summary>
        public float GetSpritesUsedMemory()
        {
            float size = 0f;
            foreach (var generated in generatedSprites.Values) size += generated.SizeInMB;
            return size;
        }


        /// <summary>
        /// Count of generated sprites references with different dimensions and filters out of source texture
        /// </summary>
        public int GetSpritesCount()
        {
            int count = 0;
            foreach (var generated in generatedSprites.Values) if (generated.LoadState == FE_SpriteLoadState.Loaded) count++;
            return count;
            //return generatedSprites.Count;
        }


        /// <summary>
        /// Returning dictionary of all generated sprites with this texture container
        /// </summary>
        public Dictionary<int, FIcon_GeneratedSprite> GetGeneratedSprites()
        {
            return generatedSprites;
        }


        /// <summary>
        /// Returning count of components which are using different sprites from this container
        /// </summary>
        internal int GetUsersCount()
        {
            int count = 0;

            foreach (var generated in generatedSprites.Values)
            {
                count += generated.References.Count;
            }

            return count;
        }

        /// <summary>
        /// Returning count of component which are using generated sprites' references
        /// </summary>
        public int GetSharedCount()
        {
            int count = 0;

            foreach (var generated in generatedSprites.Values)
            {
                count += generated.References.Count > 1 ? generated.References.Count - 1 : 0;
            }

            return count;
        }

        /// <summary>
        /// Returning count of sprites which can be unloaded from the memory (unactive, without references etc.)
        /// </summary>
        public int GetReadyToUnloadCount()
        {
            int count = 0;

            foreach (var generated in generatedSprites.Values)
                if (generated.CanBeUnloaded()) count++;

            return count;
        }


        /// <summary>
        /// Calculating approximate used memory in MB of image (basing on unity's profiler deep profile injunctions)
        /// </summary>
        public static float CalculateUsedMemory(int width, int height)
        {
            return (((float)(width * height * 8)) / 1024f) / 1024f;
        }


        /// <summary>
        /// Setting reference to this class as removed
        /// </summary>
        public bool Disposed { get; private set; }
        public void Dispose()
        {
            Disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
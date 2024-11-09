using FIMSpace.FTex;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace FIMSpace.FIcons
{
    /// <summary>
    /// FM: Class which will manage loading, unloading icons, managing memory, making queues etc.
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Icons/Ficons Manager")]
    public class FIcons_Manager : MonoBehaviour, UnityEngine.EventSystems.IDropHandler, IFHierarchyIcon
    {
        #region Get manager stuff and initialization

        public string EditorIconPath { get { return "FIcons/Icons Manager Icon"; } }
        public void OnDrop(UnityEngine.EventSystems.PointerEventData data) { }

        private static FIcons_Manager _get;
        public static FIcons_Manager Get
        {
            get { if (_get == null) GenerateIconsManager(); return _get; }
            private set { _get = value; }
        }

        private static void GenerateIconsManager()
        {
            if (!Application.isPlaying) return;
            FIcons_Manager manager = FindObjectOfType<FIcons_Manager>();

            if (!manager)
            {
                GameObject managerObject = new GameObject("Generated Icons Manager");
                managerObject.transform.SetAsFirstSibling();
                manager = managerObject.AddComponent<FIcons_Manager>();
            }

            Get = manager;
            Get.Init();
        }

        private bool initialized = false;
        private void Awake()
        {
            if (!Application.isPlaying) return;
            Init();
        }

        #endregion

        [Tooltip("Viewing only main variables needed in the most cases")]
        [FPD_Width(145)]
        public bool ShowSimpleSettings = true;

        [Tooltip("(DontDestroyOnLoad)\n!DISABLED ONLY FOR DEMO PURPOSES!\nWith this option enabled, manager will never be destroyed, even during changing scenes, also manager will move in hierarchy view to Unity's generated scene 'Dont Destroy On Load'")]
        [FPD_Width(145)]
        public bool ExistThroughScenes = true;

        [Space(5f)]
        [Tooltip("Maximum memory used by sprites. When exceed - the last used sprites which are not existing on scene anymore will be cleaned from the memory. When set to 0 all not needed sprites will be unloaded immedietely when not used.")]
        [FPD_Width(145)]
        public float MaxMemoryUsage = 100f;
        [Tooltip("If Max Memory Usage is exceed, not only not existing sprites will be cleaned from memory but also ones with Image's game objects deactivated")]
        [FPD_Width(145)]
        public bool UnloadDeactivated = false;
        [Tooltip("Allowing to unload source textures for scaled sprites (source is the biggest texture used for scaling) when memory limit is exceeded and no sprites using this textures exists, it will be loaded again when needed")]
        [FPD_Width(145)]
        public bool AllowUnloadSources = false;
        [Tooltip("Calling Resources.UnloadUnusedAssets and GarbageCollector.Collect every time when 25% of limit memory is being unloaded")]
        [FPD_Width(145)]
        public bool CallUnloadUnused = true;

        [Tooltip("When loading (not scaling) multiple textures takes more than this time in seconds in one frame, loader will skip loading to next frame")]
        [FPD_Width(145)]
        public float MaxLoadTimeDelay = 0.01f;

        [Tooltip("Sample count for scaling algorithm, it's recommended to use low values like 1-4 (lower then faster and difference is almost not noticable), if you want scale image up to bigger size than original, you can try use higher quality value - then lower value will give a bit pixelate effect")]
        [Range(1, 8)]
        public int ScalingQuality = 2;

        [FPD_Width(145)]
        [Tooltip("Making sprites scaling algorithm execute asynchronously so there are no frame lags. Highly recommended when your source icon files are larger than 256x256")]
        public bool ScaleAsync = true;

        [Space(5f)]
        [Tooltip("Animation speed factor of icons fading in when being loaded")]
        [Range(0f, 1f)]
        public float FadesSpeed = .75f;
        [Tooltip("Animation speed factor for rotation of loading spinner")]
        [Range(-2.5f, 2.5f)]
        public float SpinningSpeed = 1f;
        [Tooltip("(In Seconds) For load animation style 'SpinnerWithFade' time of loading before spinner should show up to inform that sprite is loaded in certain place in UI")]
        [Range(0f, 5f)]
        public float SpinnerAfter = 0.5f;


        #region References to needed objects for inspector window

        public GameObject LoadAnimatorPrefab;
        [Space(5f)]
        public Sprite DefaultLoadSprite;
        public Sprite ErrorSprite;

        #endregion


        /// <summary> Object pool of loading helper prefabs </summary>
        private FIcons_LoadingAnimator_ObjectPool loadingAnimatorsPool;

        /// <summary> Class which is managing memory controll over loaded and manager sprites and textures with icon loader </summary>
        private FIcons_ManagerStorage storage;


        /// <summary>
        /// Initializing needed stuff
        /// </summary>
        public void Init()
        {
            if (initialized) return;

            if (Get != null)
            {
                if (Get != this)
                {
                    Debug.LogError("[ICONS MANAGER] There was duplicate of icons manager! I removed it! (" + name + ") main manager will be still " + Get.name);
                    GameObject.Destroy(this);
                    return;
                }
            }

            Get = this;

            if (ExistThroughScenes) DontDestroyOnLoad(gameObject);

            if (loadingAnimatorsPool == null)
            {
                loadingAnimatorsPool = new FIcons_LoadingAnimator_ObjectPool(ExistThroughScenes);
                loadingAnimatorsPool.GenerateObjectsToPool(5);
            }

            storage = new FIcons_ManagerStorage();
            storage.Init(this);
            storage.StartUpdate();

            initialized = true;
        }


        private void OnEnable()
        {
            if (Application.isPlaying) if ( storage != null ) storage.StartUpdate();
        }


        #region Loading Methods For Programmers


        /// <summary>
        /// Loading texture from path, scaling it and assigning to provided image.
        /// ! Sprite will not be serviced by icons manager to keep memory clean !
        /// You can do with this sprite whatever you want.
        /// </summary>
        /// <param name="path"> Path to image inside StreamingAssets directory, put images in Assets/StreamingAssets/YourDirectoryOrImage then PATH SHOULD LOOK LIKE: 'YourDirectory/YourImage.png' </param>
        public void LoadSpriteFree(string path, Image targetImage, int newWidth, int newHeight = 0, FilterMode imageFilter = FilterMode.Point, bool setNativeSize = true, FE_IconLoadingAnimation loadingStyle = FE_IconLoadingAnimation.FadeIn, FE_LoadingPath loadingPath = FE_LoadingPath.StreamingAssets)
        {
            FIcon_LoadingAnimator spinner = null;
            if (loadingStyle != FE_IconLoadingAnimation.None)
            {
                spinner = GetLoadingAnimator();
                spinner.TargetImage = targetImage;
                spinner.LoadingStyle = loadingStyle;

                spinner.transform.SetParent(targetImage.transform);
                spinner.transform.position = targetImage.transform.position;
            }

            switch (loadingPath)
            {
                case FE_LoadingPath.StreamingAssets:
                    StartCoroutine(FIcons_Methods.LoadScaledSprite("file://" + path, spr => { OnSpriteLoadedFree(spinner, spr, setNativeSize); }, newWidth, newHeight, imageFilter));
                    break;

                case FE_LoadingPath.WWW:
                    StartCoroutine(FIcons_Methods.LoadScaledSprite(path, spr => { OnSpriteLoadedFree(spinner, spr, setNativeSize); }, newWidth, newHeight, imageFilter));
                    break;

                case FE_LoadingPath.Resources:
                    StartCoroutine(FIcons_Methods.LoadScaledSprite(path, spr => { OnSpriteLoadedFree(spinner, spr, setNativeSize); }, newWidth, newHeight, imageFilter));
                    break;
            }

        }


        /// <summary>
        /// Putting request to load this sprite in manager's queue, sprite will be loaded asynchronously and serviced in memory by manager
        /// If same image with the same size is already loaded it will not be loaded again but returned immediately
        /// </summary>
        /// <param name="path"> Path to image inside StreamingAssets directory, put images in Assets/StreamingAssets/YourDirectoryOrImage then PATH SHOULD LOOK LIKE: 'YourDirectory/YourImage.png' </param>
        /// <param name="lossyScaleSync"> Adjusting target image newWidth and newHeight to it's parent scale multiplier (when you have resizable canvas it's recommended to use) </param>
        public void LoadSpriteManaged(string path, Image targetImage, int newWidth, int newHeight = 0, FilterMode imageFilter = FilterMode.Point, bool setNativeSize = true, FE_IconLoadingAnimation loadingStyle = FE_IconLoadingAnimation.FadeIn, FE_LoadingPath loadingPath = FE_LoadingPath.StreamingAssets, bool scaleDownSupport = false, bool lossyScaleSync = true, System.Action<Sprite> callback = null)
        {
            storage.AddNewTask(path, targetImage, newWidth, newHeight, imageFilter, setNativeSize, loadingStyle, loadingPath, scaleDownSupport, lossyScaleSync, callback);
        }


        /// <summary>
        /// Putting request to load this sprite in manager's queue, sprite will be loaded asynchronously and serviced in memory by manager
        /// If same image with the same size is already loaded it will not be loaded again but returned immediately
        /// </summary>
        /// <param name="texture"> Texture which you want to rescale </param>
        /// <param name="lossyScaleSync"> Adjusting target image newWidth and newHeight to it's parent scale multiplier (when you have resizable canvas it's recommended to use) </param>
        public void LoadSpriteManaged(Texture2D texture, Image targetImage, int newWidth, int newHeight = 0, FilterMode imageFilter = FilterMode.Point, bool setNativeSize = true, FE_IconLoadingAnimation loadingStyle = FE_IconLoadingAnimation.FadeIn, bool scaleDownSupport = false, bool lossyScaleSync = false, System.Action<Sprite> callback = null)
        {
            storage.AddNewTask(texture, targetImage, newWidth, newHeight, imageFilter, setNativeSize, loadingStyle, scaleDownSupport, lossyScaleSync, callback);
        }


        /// <summary>
        /// Putting request to load this sprite in manager's queue, sprite will be loaded asynchronously and serviced in memory by manager
        /// If same image with the same size is already loaded it will not be loaded again but returned immediately
        /// </summary>
        /// <param name="projectSprite"> The biggest sprite as base for rescaling </param>
        /// <param name="lossyScaleSync"> Adjusting target image newWidth and newHeight to it's parent scale multiplier (when you have resizable canvas it's recommended to use) </param>
        public void LoadSpriteManaged(Sprite projectSprite, Image targetImage, int newWidth, int newHeight = 0, FilterMode imageFilter = FilterMode.Point, bool setNativeSize = true, FE_IconLoadingAnimation loadingStyle = FE_IconLoadingAnimation.FadeIn, bool scaleDownSupport = false, bool lossyScaleSync = false, System.Action<Sprite> callback = null)
        {
            if (projectSprite.texture)
                storage.AddNewTask(projectSprite.texture, targetImage, newWidth, newHeight, imageFilter, setNativeSize, loadingStyle, scaleDownSupport, lossyScaleSync, callback);
            else
                Debug.LogWarning("[ICONS MANAGER] No texture inside " + projectSprite.name + " sprite");
        }


        #endregion


        #region Additional Methods for Advanced Programmers


        /// <summary>
        /// Generating new texture scaled to new size
        /// </summary>
        /// <param name="noLongerEditable"> Texture will be no longer avilable for rescaling etc. but will take much less memory in RAM </param>
        public Texture2D RescaleTexture(Texture2D textureToBeScaled, int newWidth, int newHeight, FilterMode filter = FilterMode.Bilinear, bool supportScalingDown = false, bool noLongerEditable = true)
        {
            Color32[] pixels = FTex_ScaleLanczos.ScaleTexture(textureToBeScaled.GetPixels32(), textureToBeScaled.width, textureToBeScaled.height, newWidth, newHeight);

            Texture2D newTex = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
            newTex.SetPixels32(pixels);
            newTex.Apply(supportScalingDown, noLongerEditable);
            newTex.filterMode = filter;

            return newTex;
        }


        /// <summary>
        /// Unloading all source textures from texture containers (generated sprites will remain)
        /// You can do it if you don't want to keep texture files in memory, textures are needed to generate new scaled sprites
        /// If you are sure you will not generate new scaled sprites feel free to call this method
        /// </summary>
        public void UnloadSourceTextures()
        {
            if (storage != null) storage.UnloadSourceTextures();
        }


        #endregion


        #region Utilities


        /// <summary>
        /// Assigning UI loading animator from object pool
        /// </summary>
        internal void AssignAnimatorTo(FIcons_LoadTask task)
        {
            if (task.LoadingAnimator == null)
                task.AssignLoadingAnimator(GetLoadingAnimator(), task.AnimationStyle);
        }


        /// <summary>
        /// Returning memory used by sprites in MB
        /// </summary>
        public float GetMemoryInUse(bool withTextures = true)
        {
            if (storage == null) return 0;

            float totalMemory = 0;
            if (withTextures)
                foreach (var container in storage.TextureContainers.Values) totalMemory += container.TotalSizeInMB;
            else
                foreach (var container in storage.TextureContainers.Values) totalMemory += container.GetSpritesUsedMemory();

            return totalMemory;
        }

        /// <summary>
        /// Returning count of each original texture loaded to memory, from original textures we are creating scaled ones which are assigned to sprites
        /// </summary>
        public int GetTexturesCount()
        {
            if (storage == null) return 0;

            int textures = 0;
            foreach (var container in storage.TextureContainers.Values) if (container.SourceTextureLoadState == FE_TextureLoadState.Loaded) textures += 1;
            return textures;
        }

        /// <summary>
        /// Returning count of different sprites created out of source textures, there is bigger number when you create different scaled sprites out of one type of texture
        /// </summary>
        public int GetSpritesCount()
        {
            if (storage == null) return 0;

            int sprites = 0;
            foreach (var container in storage.TextureContainers.Values) sprites += container.GetSpritesCount();
            return sprites;
        }

        /// <summary>
        /// Returning count of objects which are using the same sprite allocated once in the memory
        /// </summary>
        public int GetSharedSpritesUseCount()
        {
            if (storage == null) return 0;

            int shared = 0;

            foreach (var container in storage.TextureContainers.Values)
            {
                shared += container.GetSharedCount();
            }

            return shared;
        }

        /// <summary>
        /// Returning count of objects which are using the sprites with Icons Manager manager
        /// </summary>
        public int GetImagesUseCount()
        {
            if (storage == null) return 0;

            int useCount = 0;

            foreach (var container in storage.TextureContainers.Values)
            {
                useCount += container.GetUsersCount();
            }

            return useCount;
        }

        /// <summary>
        /// Returning count of all sprite load requests in every texture container
        /// </summary>
        public int GetContainersRequestsCount()
        {
            if (storage == null) return 0;

            int requestsCount = 0;
            foreach (var container in storage.TextureContainers.Values) requestsCount += container.Requests.Count;
            return requestsCount;
        }

        /// <summary>
        /// Number of textures which waiting to be loaded for containers
        /// </summary>
        public int GetTexturesToLoadCount()
        {
            if (storage == null) return 0;
            return storage.ContainersToLoad;
        }

        /// <summary>
        /// Number of sprites which waiting to be assigned to target images
        /// </summary>
        public int GetSpritesToAssignCount()
        {
            if (storage == null) return 0;
            return storage.SpritesToLoad;
        }

        /// <summary>
        /// Returning dictionary of containers in use by manager's storage
        /// </summary>
        public Dictionary<int, FIcon_TextureContainer> GetContainers()
        {
            if (storage == null) return null;
            return storage.TextureContainers;
        }

        /// <summary>
        /// Removes container from existence in game session
        /// </summary>
        internal void PurgeContainer(FIcon_TextureContainer container)
        {
            if (storage.TextureContainers.ContainsKey(container.TextureHash))
            {
                storage.TextureContainers.Remove(container.TextureHash);
                container.Dispose();
            }
        }

        /// <summary>
        /// Returning count of load tasks for any sprite
        /// </summary>
        public int GetLoadTasksCount()
        {
            if (storage == null) return 0;
            return storage.LoadTasks.Count;
        }

        /// <summary>
        /// Returning count of sprites whichs' references aren't existing or are deactivated (if manager allows to unload deactivated references)
        /// </summary>
        public int GetReadyToUnloadCount()
        {
            if (storage == null) return 0;
            int count = 0;

            foreach (var container in storage.TextureContainers.Values)
                count += container.GetReadyToUnloadCount();

            return count;
        }

        #endregion


        #region Helpers


        private void OnValidate()
        {
            if (MaxLoadTimeDelay < 0f) MaxLoadTimeDelay = 0;
            //if (SkipDuration < 0.0001f) SkipDuration = 0.0001f;
            //if (SkipDuration > 5f) SkipDuration = 5f;
            if (MaxMemoryUsage < 0) MaxMemoryUsage = 0;
        }


        private bool editorIsQuitting = false;
        private void OnApplicationQuit() { editorIsQuitting = true; }
        public bool IsEditorQuitting() { return editorIsQuitting; }


        /// <summary>
        /// Getting loading animator from object pool
        /// </summary>
        private FIcon_LoadingAnimator GetLoadingAnimator()
        {
            if (editorIsQuitting) return null;
            if (LoadAnimatorPrefab == null) return null;
            return loadingAnimatorsPool.GetObjectFromPool();
        }


        /// <summary>
        /// Event to assign sprite to target image with free loading
        /// </summary>
        private void OnSpriteLoadedFree(FIcon_LoadingAnimator animator, Sprite loadedSprite, bool setNativeSize = true)
        {
            if (!animator) return;
            animator.OnFinishedLoading(loadedSprite);
            if (setNativeSize) animator.TargetImage.SetNativeSize();
        }


        #endregion

    }
}
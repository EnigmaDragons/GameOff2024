using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FIMSpace.FIcons
{
    /// <summary>
    /// FM: Class which is managing sprites loaded by manager and keeping all under controll
    /// </summary>
    public class FIcons_ManagerStorage
    {
        /// <summary> Reference to main icons loader manager </summary>
        public FIcons_Manager Manager { get; private set; }

        /// <summary> Tasks for loading sprites onto UI images </summary>
        public List<FIcons_LoadTask> LoadTasks { get; private set; }

        /// <summary> Texture containers for each type of icon - it generates sprites with different dimensions out of one source texture </summary>
        public Dictionary<int, FIcon_TextureContainer> TextureContainers { get; private set; }

        public int ContainersToLoad { get; private set; }
        public int SpritesToLoad { get; private set; }
        public float TotalUnloaded { get; private set; }


        #region Initialization

        /// <summary> Initialization method controll flag </summary>
        private bool initialized = false;

        /// <summary>
        /// Method to initialize needed references
        /// </summary>
        public void Init(FIcons_Manager manager)
        {
            if (initialized) return;

            Manager = manager;

            TextureContainers = new Dictionary<int, FIcon_TextureContainer>();
            LoadTasks = new List<FIcons_LoadTask>();

            ContainersToLoad = 0;
            SpritesToLoad = 0;
            TotalUnloaded = 0f;

            initialized = true;
        }


        public void StartUpdate()
        {
            Manager.StopAllCoroutines();
            Manager.StartCoroutine(HandleTasks());
            Manager.StartCoroutine(HandleLoadingTextures());
            Manager.StartCoroutine(HandleSpriteRequests());
            Manager.StartCoroutine(HandleUnloading());
        }


        #endregion


        private float unloadedBeforeGC = 0f;
        //private WaitForSeconds waitInterval;
        //private float lastSkipDuration = 0f;

        /// <summary>
        /// Adding new loading task to storage, generating new texture container if needed
        /// </summary>
        public void AddNewTask(string path, Image targetImage, int newWidth, int newHeight = 0, FilterMode imageFilter = FilterMode.Point, bool setNativeSize = true, FE_IconLoadingAnimation loadingStyle = FE_IconLoadingAnimation.FadeIn, FE_LoadingPath loadingPath = FE_LoadingPath.StreamingAssets, bool scaleDownSupport = false, bool lossyScaleSync = true, System.Action<Sprite> callback = null)
        {
            path = CheckPathCorrectness(path, loadingPath);

            FIcon_TextureContainer container;
            if (TextureContainers.TryGetValue(path.GetHashCode(), out container))
            { }
            else
            {
                container = new FIcon_TextureContainer(path, loadingPath);
                TextureContainers.Add(path.GetHashCode(), container);
                ContainersToLoad++;
            }

            FIcons_LoadTask newTask = new FIcons_LoadTask(container, targetImage, newWidth, newHeight, imageFilter, setNativeSize, loadingStyle, loadingPath, scaleDownSupport, lossyScaleSync, callback);

            LoadTasks.Add(newTask);
            SpritesToLoad++;
        }

        /// <summary>
        /// Adding new loading task to storage basing on already existing Texture2D, generating new texture container if needed
        /// </summary>
        public void AddNewTask(Texture2D originTexture, Image targetImage, int newWidth, int newHeight = 0, FilterMode imageFilter = FilterMode.Point, bool setNativeSize = true, FE_IconLoadingAnimation loadingStyle = FE_IconLoadingAnimation.FadeIn, bool scaleDownSupport = false, bool lossyScaleSync = true, System.Action<Sprite> callback = null)
        {

            if (originTexture == null)
            {
                Debug.LogError("[ICONS MANAGER] Null texture is tried to be used as source texture for containers");
                return;
            }

            if (Manager.LoadAnimatorPrefab == null) loadingStyle = FE_IconLoadingAnimation.None;

            FIcon_TextureContainer container;
            if (TextureContainers.TryGetValue(originTexture.name.GetHashCode(), out container))
            { }
            else
            {
                container = new FIcon_TextureContainer(originTexture);
                TextureContainers.Add(originTexture.name.GetHashCode(), container);
                ContainersToLoad++;
            }

            if (newHeight == 0) newHeight = newWidth;
            FIcons_LoadTask newTask = new FIcons_LoadTask(container, targetImage, newWidth, newHeight, imageFilter, setNativeSize, loadingStyle, FE_LoadingPath.OtherTexture, scaleDownSupport, lossyScaleSync, callback);

            LoadTasks.Add(newTask);
            SpritesToLoad++;
        }

        private static readonly string handlingTasks = "Handling Tasks";
        private static readonly string genSpritesLoadName = "Generating Sprites";

        /// <summary>
        /// Coroutine to handle tasks, things like assigning new sprites to images, requesting loading textures/sprites etc.
        /// </summary>
        private IEnumerator HandleTasks()
        {
            /// Measuring load time to avoid lags when loading takes too much time
            System.Diagnostics.Stopwatch timeLimiter = new System.Diagnostics.Stopwatch();
            timeLimiter.Start();

            //waitInterval = new WaitForSeconds(Manager.SkipDuration);
            //lastSkipDuration = Manager.SkipDuration;

            while (true)
            {
                // Tiny optimization for garbage collector
                //if (Manager.SkipDuration != lastSkipDuration) waitInterval = new WaitForSeconds(Manager.SkipDuration);
                //lastSkipDuration = Manager.SkipDuration;
                timeLimiter.Reset(); timeLimiter.Start();

                for (int i = 0; i < LoadTasks.Count; i++)
                {
                    // Getting first task from list and trying to handle it
                    FIcons_LoadTask task = LoadTasks[i];

                    if (task.Aborted) { SpritesToLoad--; continue; }

                    if (task.TargetContainer != null)
                    {
                        // Servicing loading animation if wanted
                        if (task.AnimationStyle != FE_IconLoadingAnimation.None)
                        {
                            Manager.AssignAnimatorTo(task);
                        }

                        bool askFor = false;
                        if (task.TargetContainer.SourceTextureLoadState == FE_TextureLoadState.Loaded)
                        {
                            task.CheckCorrectness();
                            askFor = true;
                        }
                        else
                        {
                            // Checking if source is unloaded but needed sprite exists
                            if (task.TargetContainer.SourceTextureLoadState == FE_TextureLoadState.Unloaded)
                            {
                                task.CheckCorrectness();
                                if (task.TargetContainer.GeneratedSpriteAvailable(FIcon_GeneratedSprite.GetIdFrom(task))) askFor = true;
                            }
                        }

                        // If task's container have already loaded texture we can check if sprite is generated
                        if (askFor)
                        {
                            int targetId = FIcon_GeneratedSprite.GetIdFrom(task.Width, task.Height, task.FilterMode);
                            FIcon_GeneratedSprite generatedSprite = task.TargetContainer.GetGeneratedSprite(targetId);

                            if (generatedSprite == null || generatedSprite.LoadState != FE_SpriteLoadState.Loaded)
                            {
                                task.Wait();

                                // If there is no needed sprite and is not waiting to be generated, we requesting to generate it
                                if (!task.TargetContainer.IsSpriteWaiting(targetId))
                                {
                                    task.TargetContainer.AskForSprite(task, task.Width, task.Height, task.FilterMode, task.ScaleDownSupport);
                                }
                            }
                            else
                            {
                                task.FinishTaskWith(generatedSprite, targetId);
                                SpritesToLoad--;
                            }
                        }
                        else
                        {
                            if (task.TargetContainer.SourceTextureLoadState == FE_TextureLoadState.Unloaded)
                            {
                                task.TargetContainer.CanBeReloaded();
                                ContainersToLoad++;
                            }
                        }
                    }

                    // Skip frame if handling tasks takes too long
                    if (CheckTimeLimit(timeLimiter, handlingTasks)) { yield return null; /*waitInterval;*/ timeLimiter.Reset(); timeLimiter.Start(); }
                }

                // Cleaning tasks list in the safe way
                for (int i = LoadTasks.Count - 1; i >= 0; i--)
                {
                    if (LoadTasks[i].State == FE_TextureTaskState.Finished) LoadTasks.RemoveAt(i);
                }

                yield return null;
            }
        }


        /// <summary>
        /// Coroutine to handle loading textures and generating sprites procedures
        /// </summary>
        private IEnumerator HandleLoadingTextures()
        {
            while (true)
            {
                // Doing nothing when there are no textures to load
                if (ContainersToLoad > 0)
                    foreach (var container in TextureContainers.Values)
                    {
                        // If source texture is not loaded yet let's do it
                        if (container.SourceTextureLoadState == FE_TextureLoadState.None)
                        {
                            container.LoadTexture();

                            #region Supporting different loading sources

                            switch (container.LoadingPathType)
                            {
                                case FE_LoadingPath.StreamingAssets:
                                    yield return FIcons_Methods.LoadTexture(container.PathToSourceTexture, t => { container.OnLoadTextureFinish(t); });
                                    break;

                                case FE_LoadingPath.WWW:
                                    yield return FIcons_Methods.DownloadTexture(container.PathToSourceTexture, t => { container.OnLoadTextureFinish(t); });
                                    break;

                                case FE_LoadingPath.Resources:
                                    yield return FIcons_Methods.LoadTextureResources(container.PathToSourceTexture, t => { container.GetFromTexture(t); });
                                    break;

                                case FE_LoadingPath.OtherTexture: container.GetFromTexture(container.OtherOriginalTexture); break;

#if ADDRESSABLES_IMPORTED
                                    case FE_LoadingPath.Adressable:
                                    yield return FIcons_Methods.LoadTextureAddressable(container.PathToSourceTexture, t => { container.GetFromTexture(t); });
                                    break;
#endif
                            }

                            #endregion

                            ContainersToLoad--;
                        }
                    }

                yield return null;
            }
        }


        /// <summary>
        /// Coroutine to handle generating sprites in texture containers
        /// </summary>
        private IEnumerator HandleSpriteRequests()
        {
            /// Measuring load time to avoid lags when loading takes too much time
            System.Diagnostics.Stopwatch timeLimiter = new System.Diagnostics.Stopwatch();
            timeLimiter.Start();

            while (true)
            {
                // Doing nothing if no sprites to load
                if (SpritesToLoad > 0)
                {
                    timeLimiter.Reset(); timeLimiter.Start();

                    foreach (var container in TextureContainers.Values)
                    {
                        // Executing requests one by one and delaying loading by frame if it takes too long
                        for (int i = 0; i < container.Requests.Count; i++)
                        {
                            if (container.Requests[i].State == FE_RequestState.Complete) continue;
                            container.ServiceRequest(container.Requests[i]);
                            //if (CheckTimeLimit(timeLimiter, "Handling Tasks")) { yield return waitInterval; timeLimiter.Reset(); timeLimiter.Start(); }
                        }

                        // Cleaning requests list in the safe way
                        for (int i = container.Requests.Count - 1; i >= 0; i--)
                        {
                            if (container.Requests[i].State == FE_RequestState.Complete) { container.Requests.RemoveAt(i); continue; }

                            if (container.Requests[i].State == FE_RequestState.Error)
                            {
                                container.Requests[i].Task.ErrorOccured();
                                container.Requests.RemoveAt(i);
                                continue;
                            }
                        }

                        // Skip frame if generating sprites took too long (only when not async)
                        if (!Manager.ScaleAsync) if (CheckTimeLimit(timeLimiter, genSpritesLoadName)) { yield return null; /*waitInterval;*/ timeLimiter.Reset(); timeLimiter.Start(); }
                    }
                }

                yield return null;
            }
        }


        /// <summary>
        /// Coroutine to handle unloading sprites, managing memory when max memory limit exceeds
        /// </summary>
        private IEnumerator HandleUnloading()
        {
            while (true)
            {
                if (Manager.GetMemoryInUse() > Manager.MaxMemoryUsage) // Checking containers only when memory exceeds
                {
                    // How much memory should be cleaned
                    float memoryToClean = Manager.GetMemoryInUse() - Manager.MaxMemoryUsage + Manager.MaxMemoryUsage * 0.1f; // Trying to clean 10% more if we can

                    List<FIcon_TextureContainer> sortedContainers = new List<FIcon_TextureContainer>();

                    // Collecting list of the oldest containers and check what can be removed to the point
                    // when their size in memory will leave enough space, then purging allowed data at once
                    foreach (var container in TextureContainers.Values)
                        if (container.UnloadPossibility)
                            sortedContainers.Add(container);

                    if (sortedContainers.Count == 0)
                    {
                        //Debug.Log("[ICONS MANAGER] We can't remove anything from containers, sprites are needed");
                    }
                    else
                    {
                        sortedContainers.OrderBy(c => c.LastUse);

                        // Going one by one from the oldest container to newest and cleaning allowed data from the memory until we reach enough free space
                        for (int i = 0; i < sortedContainers.Count; i++)
                        {
                            float unload = sortedContainers[i].SafeUnload();
                            memoryToClean -= unload;
                            TotalUnloaded += unload;
                            unloadedBeforeGC += unload;
                            if (memoryToClean <= 0) break;
                        }
                    }
                }

                if (Manager.CallUnloadUnused)
                    if (unloadedBeforeGC > Manager.MaxMemoryUsage / 4)
                    {
                        unloadedBeforeGC = 0f;
                        yield return Resources.UnloadUnusedAssets();
                        GC.Collect();
                        yield return null;
                    }

                yield return null;
            }
        }


        /// <summary>
        /// Unloading source textures from containers
        /// </summary>
        internal void UnloadSourceTextures()
        {
            foreach (var container in TextureContainers.Values)
            {
                float unload = container.UnloadJustSourceTexture();
                unloadedBeforeGC += unload;
                TotalUnloaded += unload;
            }
        }

        /// <summary>
        /// Checking if path to icon file is correct
        /// </summary>
        private string CheckPathCorrectness(string path, FE_LoadingPath loadingPath)
        {
            #region Extension check

            bool ext = Path.HasExtension(path);
            if (loadingPath == FE_LoadingPath.Resources)
            {
                if (ext)
                {
#if UNITY_EDITOR
                    Debug.LogError("[ICONS MANAGER] Resources file path SHOULDN'T have extension! (remove .png from path or something)");
#endif
                    path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                }
            }
            else
            {
#if UNITY_EDITOR
                if (!ext)
                    if (loadingPath == FE_LoadingPath.StreamingAssets || loadingPath == FE_LoadingPath.WWW)
                    {
                        Debug.LogError("[ICONS MANAGER] WWW or StreamingAssets file path SHOULD have extension! (missing .png or something?)");
                    }
#endif
            }

            #endregion



            return path;
        }


        /// <summary>
        /// Return true if time limit exceed
        /// </summary>
        public static bool CheckTimeLimit(System.Diagnostics.Stopwatch watch, string operationType = "")
        {
            if (watch.ElapsedMilliseconds * 0.001f >= FIcons_Manager.Get.MaxLoadTimeDelay)
            {
                watch.Stop();
                //Debug.Log("[ICONS MANAGER] Skip at " + operationType + " " + watch.ElapsedTicks + "t " + watch.ElapsedMilliseconds + "ms");
                return true;
            }

            return false;
        }

    }
}
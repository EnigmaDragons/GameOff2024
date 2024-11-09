using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FIcons
{
    /// <summary>
    /// FM: Generated sprite, scaled source texture from container with algorithm reference class
    /// Containing list with all images using this scaled generated sprite helping with memory management
    /// </summary>
    public class FIcon_GeneratedSprite
    {
        /// <summary> Id of the sprite in texture container </summary>
        public int Id { get; private set; }

        /// <summary> Path from container source texture </summary>
        public string TexturePath { get; private set; }

        /// <summary> Approximate size of image in the RAM memory </summary>
        public float SizeInMB { get; private set; }

        /// <summary> Full path to source texture file </summary>
        //public string FullPath { get; private set; }
        public FIcon_TextureContainer OwnerContainer { get; private set; }

        /// <summary> If sprite was loaded but not yes assigned to waiting object </summary>
        public bool WaitingFor { get; private set; }

        /// <summary> Last time when image was readed/activated or deactivated</summary>
        public int LastUse { get; private set; }

        /// <summary> Width of the scaled sprite </summary>
        public int Width { get; private set; }
        public int OriginalCallWidth { get; private set; }

        /// <summary> Height of the scaled sprite </summary>
        public int Height { get; private set; }
        public int OriginalCallHeight { get; private set; }
        /// <summary> Filter interpolation mode of the sprite </summary>
        public FilterMode Filter { get; private set; }
        /// <summary> Supporting scaling down additional maps generation </summary>
        public bool ScaleDownSupport { get; private set; }
        public bool LossyScaleSync { get; private set; }

        /// <summary> All components using this scaled sprite, when references count is zero, sprite can be unloaded from memory </summary>
        public List<FIcon_Reference> References { get; private set; }
        public FE_SpriteLoadState LoadState { get; private set; }

        /// <summary> Generated scaled sprite </summary>
        public Sprite GeneratedSprite { get; private set; }

        /// <summary> Count of active components using this generated sprite </summary>
        public int ActiveUsers { get; private set; }
        public bool Reloading { get; private set; }

        /// <summary> Asynchronous scaling thread </summary>
        public FIcon_ScaleLanczosAsync ScalingThread { get; private set; }


        public FIcon_GeneratedSprite(FIcon_LoadRequest request)
        {
            TexturePath = "";
            Width = request.Width;
            Height = request.Height;
            Filter = request.Filter;
            ScaleDownSupport = request.ScaleDown;
            Id = request.Id;
            LossyScaleSync = request.Task.LossyScaleSync;
            OriginalCallWidth = request.Task.OriginalCallWidth;
            OriginalCallHeight = request.Task.OriginalCallHeight;

            LoadState = FE_SpriteLoadState.Unloaded;
            SizeInMB = 0;
            WaitingFor = false;
            ActiveUsers = 0;
            Reloading = false;

            LastUse = Time.frameCount;

            References = new List<FIcon_Reference>();
        }


        /// <summary>
        /// Assigning desired sprite to be spread in the game and setting parent texture container
        /// </summary>
        public void AssignSprite(Sprite generatedSprite, FIcon_TextureContainer container)
        {
            if (generatedSprite == null)
            {
                Debug.LogError("[ICONS MANAGER] Trying to assign null sprite!");
                return;
            }

            TexturePath = container.PathToSourceTexture;
            GeneratedSprite = generatedSprite;
            OwnerContainer = container;
            WaitingFor = true;
            Reloading = false;

            SizeInMB = FIcon_TextureContainer.CalculateUsedMemory(Width, Height);
            if (generatedSprite.texture) if (generatedSprite.texture.mipmapCount > 1) SizeInMB *= 1.4f;
            LoadState = FE_SpriteLoadState.Loaded;
            LastUse = Time.frameCount;
        }


        /// <summary>
        /// Returning scaled sprite reference
        /// </summary>
        public Sprite GetSprite()
        {
            LastUse = Time.frameCount;

            if (GeneratedSprite != null)
            {
                if (WaitingFor) { WaitingFor = false; }
            }
            else Debug.Log("[ICONS MANAGER] Trying get null sprite");

            return GeneratedSprite;
        }


        /// <summary>
        /// Refreshing array of references to be correct, clearing from nulls etc.
        /// </summary>
        public void RefreshReferences()
        {
            for (int i = References.Count - 1; i >= 0; i--)
            {
                if (References[i] == null) References.RemoveAt(i);
            }
        }


        /// <summary>
        /// When new component started to using this generated sprite (ofater created, onActivated also is triggered by unity MonoBehaviour)
        /// </summary>
        internal void OnReferenceCreated(FIcon_Reference reference)
        {
            if (!References.Contains(reference)) References.Add(reference);
        }


        /// <summary>
        /// When sprite reference is activated we informing that something is using it, if sprite already unloaded we try to load it again
        /// </summary>
        internal void OnReferenceActivated()
        {
            LastUse = Time.frameCount;
            ActiveUsers++;
            OwnerContainer.UnloadingInformation(false);

            if (LoadState == FE_SpriteLoadState.Unloaded) ReloadSprite();
        }

        /// <summary>
        /// Informing about aborted task
        /// </summary>
        internal void AbortTask(FIcons_LoadTask task)
        {
            WaitingFor = false;
        }


        /// <summary>
        /// When sprite reference is activated, maybe we will want to load sprite or other stuff
        /// </summary>
        internal void OnReferenceDeactivated()
        {
            ActiveUsers--;
            if (CanBeUnloaded()) OwnerContainer.UnloadingInformation(true);
        }


        /// <summary>
        /// Cleaning references from references list
        /// </summary>
        internal void OnReferenceRemoved(FIcon_Reference reference)
        {
            References.Remove(reference);
            RefreshReferences();

            if (CanBeUnloaded()) OwnerContainer.UnloadingInformation(true);
        }


        /// <summary>
        /// Checking if generated sprite can be unloaded from memory (nothing is using this generated sprite)
        /// </summary>
        public bool CanBeUnloaded()
        {
            if (SizeInMB == 0) return false;
            if (WaitingFor) return false;

            if (FIcons_Manager.Get.UnloadDeactivated) if (ActiveUsers == 0) return true;
            if (References.Count == 0) return true;

            return false;
        }


        /// <summary>
        /// Removing generated sprite from the memory
        /// </summary>
        public void PurgeGeneratedSprite()
        {
            GameObject.Destroy(GeneratedSprite);
            GeneratedSprite = null;
            WaitingFor = false;

            LoadState = FE_SpriteLoadState.Unloaded;
            SizeInMB = 0;
            ActiveUsers = 0;

            for (int i = References.Count - 1; i >= 0; i--)
            {
                if (References[i] == null) { References.RemoveAt(i); continue; }
                References[i].ResetReference();
            }
        }


        /// <summary>
        /// Reloading sprites for all references
        /// </summary>
        public void ReloadSprite()
        {
            LoadState = FE_SpriteLoadState.Loading;

            for (int i = References.Count - 1; i >= 0; i--)
            {
                if (References[i] == null) { References.RemoveAt(i); continue; }
                FIcons_Manager.Get.LoadSpriteManaged(OwnerContainer.PathToSourceTexture, References[i].TargetImage, OriginalCallWidth, OriginalCallHeight, Filter, References[i].SettedNativeSize, References[i].LoadingAnimation, OwnerContainer.LoadingPathType, ScaleDownSupport, LossyScaleSync);
                Reloading = true;
            }
        }


        /// <summary>
        /// Generating id of image (in source texture space) basing on its width height and filter
        /// </summary>
        public static int GetIdFrom(int width, int height, FilterMode filter)
        {
            return (width + height) * ((int)filter + 1111);
        }

        internal static int GetIdFrom(FIcons_LoadTask task)
        {
            return GetIdFrom(task.Width, task.Height, task.FilterMode);
        }

        /// <summary>
        /// Setting async loading thread
        /// </summary>
        internal void SetThread(FIcon_ScaleLanczosAsync scalingThread)
        {
            if (scalingThread == null)
            {
                ScalingThread = null;
            }
            else
            {
                ScalingThread = scalingThread;
                scalingThread.Start();
                LoadState = FE_SpriteLoadState.Loading;
            }
        }

        internal void CheckLoadState()
        {
            if (GeneratedSprite == null)
            {
                if (Reloading)
                    LoadState = FE_SpriteLoadState.Unloaded;
            }
        }
    }
}
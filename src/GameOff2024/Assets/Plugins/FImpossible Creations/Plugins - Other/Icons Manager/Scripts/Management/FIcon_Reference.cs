using System;
using UnityEngine;
using UnityEngine.UI;

namespace FIMSpace.FIcons
{
    /// <summary>
    /// FM: Simple MonoBehaviour to controll management of loaded sprites with icons loader, letting to unload sprites not needed in game etc.
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Hidden/FC IS_Reference")]
    public class FIcon_Reference : MonoBehaviour
    {
        public FIcon_GeneratedSprite SpriteSource { get; private set; }
        public FIcon_TextureContainer Container { get; private set; }
        public Sprite Sprite { get; private set; }
        public Image TargetImage;
        public int SpriteId { get; private set; }

        public FE_IconLoadingAnimation LoadingAnimation { get; private set; }
        public bool SettedNativeSize { get; private set; }
        public bool SentUser { get; private set; }
        public FIcons_LoadTask CurrentTask { get; private set; }


        public void SetData(FIcons_LoadTask task, FIcon_GeneratedSprite spriteSource, Image targetImage, int spriteId, FE_IconLoadingAnimation animation, bool settedNativeSize)
        {
            bool referenced = false;

            if (Container != null)
            {
                // Removing user if we assigned new sprite to the image
                if (SpriteSource != null)
                {
                    SentUser = true;
                    if (spriteSource != SpriteSource) { SpriteSource.OnReferenceRemoved(this);}
                    if (CurrentTask != null) CurrentTask.Abort();
                    RemoveUser(true);
                    referenced = true;
                }

                if (Container.Disposed) ResetReference();
            }

            SpriteSource = spriteSource;
            Container = SpriteSource.OwnerContainer;
            Sprite = spriteSource.GetSprite();
            TargetImage = targetImage;
            SpriteId = spriteId;
            SpriteSource.OnReferenceCreated(this);
            LoadingAnimation = animation;
            SettedNativeSize = settedNativeSize;
            CurrentTask = task;
            SendUser(referenced);
        }


        /// <summary>
        /// Resetting reference when sprite is unloaded
        /// </summary>
        public void ResetReference()
        {
            SentUser = false;
        }

        /// <summary>
        /// Sending info to add flag of one user of target SpriteSource
        /// </summary>
        private void SendUser(bool referenced = false)
        {
            if (SpriteSource == null) return;
            if (SentUser) return;

            SpriteSource.OnReferenceActivated();
            SentUser = true;
        }

        /// <summary>
        /// Sending info to remove flag of one user of target SpriteSource
        /// </summary>
        private void RemoveUser(bool referenced = false)
        {
            if (SpriteSource == null) return;
            if (!SentUser) return;

            SpriteSource.OnReferenceDeactivated();
            SentUser = false;
        }

        /// <summary>
        /// Adding icon reference to image from the task
        /// </summary>
        internal static void AddReferenceFromTask(FIcons_LoadTask task)
        {
            FIcon_Reference reference = task.TargetImage.gameObject.GetComponent<FIcon_Reference>();
            if (!reference)
            {
                reference = task.TargetImage.gameObject.AddComponent<FIcon_Reference>();
                reference.SetTask(task);
            }
            else
            {
                reference.SetTask(task);
            }
        }


        /// <summary>
        /// Handling if few tasks are trying to load sprite for this image
        /// </summary>
        /// <param name="task"></param>
        private void SetTask(FIcons_LoadTask task)
        {
            if (CurrentTask == null) CurrentTask = task;
            else
            {
                if (task.TimeInitiated > CurrentTask.TimeInitiated)
                {
                    CurrentTask.Abort();
                    CurrentTask = task;
                }
                else
                {
                    task.Abort();
                }
            }
        }


        /// <summary>
        /// Clearing task after finishing loading for this image
        /// </summary>
        public void TaskFinished(FIcons_LoadTask task)
        {
            if (CurrentTask == task)
            {
                CurrentTask = null;
            }
        }


        /// <summary>
        /// Handling detection if sprite user component is being activated
        /// </summary>
        private void OnEnable()
        {
            SendUser();
        }


        /// <summary>
        /// Handling detection if sprite is being deactivated
        /// </summary>
        private void OnDisable()
        {
            if (isQuitting) return;
            RemoveUser();
        }


        /// <summary>
        /// Handling detection if sprite user component is being destroyed
        /// </summary>
        private void OnDestroy()
        {
            if (!isQuitting) Purge();
        }


        /// <summary>
        /// Unloading current used sprite
        /// </summary>
        public void Unload()
        {
            RemoveUser();
            Sprite = null;
            SpriteSource = null;
            Container = null;
            SettedNativeSize = false;
        }


        /// <summary>
        /// Removing reference
        /// </summary>
        private void Purge()
        {
            if (SpriteSource != null) SpriteSource.OnReferenceRemoved(this);
            Destroy(this);
        }


        private bool isQuitting = false;
        private void OnApplicationQuit()
        {
            isQuitting = true;
        }
    }


#if UNITY_EDITOR
    /// <summary>
    /// FM: Editor class component to enchance controll over component from inspector window
    /// </summary>
    [UnityEditor.CustomEditor(typeof(FIcon_Reference))]
    public class FIcon_ReferenceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            FIcon_Reference targetScript = (FIcon_Reference)target;
            DrawDefaultInspector();

            GUILayout.Space(4f);

            if (targetScript.CurrentTask == null)
                if (Application.isPlaying)
                    if (GUILayout.Button("Unload")) targetScript.Unload();
        }
    }
#endif

}
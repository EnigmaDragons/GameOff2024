using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTex
{
    /// <summary>
    /// FM: Base class for object pooling
    /// </summary>
    public class FObjectsPool<T> where T : MonoBehaviour
    {
        /// <summary> Now used objects from this pool </summary>
        protected List<T> ActiveObjects;

        /// <summary> Not used objects from this pool </summary>
        protected List<T> UnactiveObjects;

        /// <summary> Used only inside editor to keep hierarchy clean, on build it's not needed </summary>
        protected Transform PoolObjectsContainer;

        protected readonly string defaultName = "PoolObject";
        protected bool dontDestroyOnLoad = false;

        public FObjectsPool(bool dontDestroyOnLoad = false)
        {
            ActiveObjects = new List<T>();
            UnactiveObjects = new List<T>();

            this.dontDestroyOnLoad = dontDestroyOnLoad;

            // We keep parental order only inside unity editor to see everything clear, then on build we don't need this
            // and also this way (no parental order on vuild) it will work more optimal, changing parents takes some time to compute for unity
#if UNITY_EDITOR
            PoolObjectsContainer = new GameObject("FObjectsPool-Container").transform;
            PoolObjectsContainer.transform.SetAsFirstSibling();

            if (this.dontDestroyOnLoad)
                GameObject.DontDestroyOnLoad(PoolObjectsContainer);
#endif

            Init();
        }


        /// <summary>
        /// Method colled when object pool is created
        /// </summary>
        protected virtual void Init()
        {

        }

        /// <summary>
        /// Getting object with own gameObject from object pool, if need it is generating another one to the pool
        /// </summary>
        public virtual T GetObjectFromPool()
        {
            if (UnactiveObjects.Count == 0) GenerateObject();

            T nextObject = UnactiveObjects[0];

            if (nextObject == null) CheckNulls();

            nextObject = UnactiveObjects[0];

            if (nextObject == null) nextObject = GenerateObject();

            UnactiveObjects.Remove(nextObject);

            ActiveObjects.Add(nextObject);

            nextObject.gameObject.SetActive(true);

            return nextObject;
        }

        /// <summary>
        /// Creates gameObject and component then deactivating it and putting to unactive objects list
        /// </summary>
        protected virtual T GenerateObject()
        {
            GameObject poolObject = new GameObject(GetName());

            T poolComponent = poolObject.AddComponent<T>();

            PutObjectInContainer(poolComponent);

            poolObject.SetActive(false);

            UnactiveObjects.Add(poolComponent);

            return poolComponent;
        }


        /// <summary>
        /// Getting name for generated object to pool
        /// </summary>
        protected virtual string GetName()
        {
            string name = defaultName;

#if UNITY_EDITOR
            // On build we don't need to make visual friendly names
            name = "FPool-Object-" + (UnactiveObjects.Count + ActiveObjects.Count);
#endif

            return name;
        }


        /// <summary>
        /// Putting object in container when deactivated (only in editor to keep hierachy clean)
        /// </summary>
        protected virtual void PutObjectInContainer(T poolComponent)
        {
#if UNITY_EDITOR
            if (dontDestroyOnLoad)
                GameObject.DontDestroyOnLoad(poolComponent.gameObject);

            // On build we don't need to put pool objects onto other transform container
            poolComponent.transform.SetParent(PoolObjectsContainer);
#endif
        }


        /// <summary>
        /// If we encounter null in our lists, check whole list and remove this elements
        /// </summary>
        protected void CheckNulls()
        {
            for (int i = UnactiveObjects.Count - 1; i >= 0; i--)
            {
                if (UnactiveObjects[i] == null) UnactiveObjects.RemoveAt(i);
            }

            for (int i = ActiveObjects.Count - 1; i >= 0; i--)
            {
                if (ActiveObjects[i] == null) ActiveObjects.RemoveAt(i);
            }
        }

        /// <summary>
        /// Generating initial objects in pool, you don't need to do this, because new objects are generated if there is too small amount of them in pool
        /// </summary>
        public void GenerateObjectsToPool(int count = 15)
        {
            for (int i = 0; i < count; i++) GenerateObject();
        }

        /// <summary>
        /// Removing object from this pool
        /// </summary>
        public virtual void PurgeFromPool(T poolComponent)
        {
            if (ActiveObjects.Contains(poolComponent)) ActiveObjects.Remove(poolComponent);
            else
                if ((UnactiveObjects.Contains(poolComponent))) UnactiveObjects.Remove(poolComponent);
        }

        /// <summary>
        /// Deactivating object and moving it back to pool's reserve list
        /// </summary>
        public virtual void GiveBackObject(T poolComponent)
        {
            ActiveObjects.Remove(poolComponent);

#if UNITY_EDITOR
            // On build we don't need to put pool objects onto other transform container
            poolComponent.transform.SetParent(PoolObjectsContainer);
#endif

            poolComponent.gameObject.SetActive(false);
            UnactiveObjects.Add(poolComponent);
        }
    }
}
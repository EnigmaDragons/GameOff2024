using FIMSpace.FTex;
using UnityEngine;

namespace FIMSpace.FIcons
{
    /// <summary>
    /// FM: Object pool for loading animators
    /// </summary>
    public class FIcons_LoadingAnimator_ObjectPool : FObjectsPool<FIcon_LoadingAnimator>
    {
        public FIcons_LoadingAnimator_ObjectPool(bool existThroughScenes):base(existThroughScenes)
        {
#if UNITY_EDITOR
            if ( PoolObjectsContainer)
                PoolObjectsContainer.name = "FObjectsPool-LoadingAnimations & Spinners";
#endif
        }

        protected override FIcon_LoadingAnimator GenerateObject()
        {
            GameObject loadingAnim = FIcons_Manager.Get.LoadAnimatorPrefab;
            if (!loadingAnim) return null;

            GameObject poolObject = GameObject.Instantiate(FIcons_Manager.Get.LoadAnimatorPrefab);
            poolObject.name = GetName();

            FIcon_LoadingAnimator poolComponent = poolObject.GetComponent<FIcon_LoadingAnimator>();
            PutObjectInContainer(poolComponent);
            poolComponent.AssignPool(this);

            poolObject.SetActive(false);

            UnactiveObjects.Add(poolComponent);

            return poolComponent;
        }

        public override void GiveBackObject(FIcon_LoadingAnimator poolComponent)
        {
            base.GiveBackObject(poolComponent);
            poolComponent.ResetToInit();
        }
    }
}
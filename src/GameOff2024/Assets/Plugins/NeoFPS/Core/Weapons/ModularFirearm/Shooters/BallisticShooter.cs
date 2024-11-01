﻿using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-ballisticshooter.html")]
    public class BallisticShooter : BaseShooterBehaviour, IUseCameraAim
    {
        [Header("Shooter Settings")]

        [SerializeField, NeoPrefabField(typeof(IProjectile), required = true), Tooltip("The projectile to spawn.")]
        private PooledObject m_ProjectilePrefab = null;

        [SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("The position and direction the projectile is spawned.")]
        private Transform m_MuzzleTip = null;

        [SerializeField, Tooltip("The speed of the projectile.")]
        private float m_MuzzleSpeed = 100f;

        [SerializeField, Tooltip("The layers bullets will collide with.")]
        private LayerMask m_Layers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Tooltip("The minimum accuracy spread (in degrees) of the weapon")]
        private float m_MinimumSpread = 0f;

        [SerializeField, Tooltip("The maximum accuracy spread (in degrees) of the weapon")]
        private float m_MaximumSpread = 5f;

        [SerializeField, Tooltip("The gravity for the projectile.")]
        private float m_Gravity = 9.8f;

        [FormerlySerializedAs("m_UseCameraForward")] // Remove this
        [SerializeField, Tooltip("When set to use camera aim, the gun first casts from the FirstPersonCamera's aim transform, and then from the muzzle tip to that point to get more accurate firing.")]
        private UseCameraAim m_UseCameraAim = UseCameraAim.HipFireOnly;
        
        const float k_MaxDistance = 1000f;

        private RaycastHit m_Hit = new RaycastHit();
        private ITargetingSystem m_TargetingSystem = null;

#if UNITY_EDITOR
        protected void OnValidate()
        {
            // Check shell prefab is valid
            if (m_ProjectilePrefab != null && m_ProjectilePrefab.GetComponent<IProjectile>() == null)
            {
                Debug.Log("Projectile prefab must have IProjectile component attached (such as a BallisticProjectile): " + m_ProjectilePrefab.name);
                m_ProjectilePrefab = null;
            }

            if (m_MuzzleSpeed < 1f)
                m_MuzzleSpeed = 1f;
            if (m_Gravity < 0f)
                m_Gravity = 0f;
        }
#endif

        public LayerMask collisionLayers
        {
            get { return m_Layers; }
            set { m_Layers = value; }
        }

        public override bool isModuleValid
        {
            get
            {
                return
                    m_ProjectilePrefab != null &&
                    m_MuzzleTip != null &&
                    m_Layers != 0;
            }
        }

        public UseCameraAim useCameraAim
        {
            get { return m_UseCameraAim; }
            set { m_UseCameraAim = value; }
        }

        protected override void Awake()
        {
            base.Awake();
            m_TargetingSystem = GetComponentInChildren<ITargetingSystem>();
        }

        protected virtual float GetModifiedSpread(float unmodified)
        {
            return unmodified;
        }

        public override void Shoot(float accuracy, IAmmoEffect effect)
        {
            if (m_ProjectilePrefab != null)
            {
                IProjectile projectile = PoolManager.GetPooledObject<IProjectile>(m_ProjectilePrefab, false);
                InitialiseProjectile(projectile);

                bool useCamera = false;
                if (firearm.wielder != null)
                {
                    switch (m_UseCameraAim)
                    {
                        case UseCameraAim.HipAndAimDownSights:
                            useCamera = true;
                            break;
                        case UseCameraAim.AimDownSightsOnly:
                            if (firearm.aimer != null)
                                useCamera = firearm.aimer.isAiming;
                            break;
                        case UseCameraAim.HipFireOnly:
                            if (firearm.aimer != null)
                                useCamera = !firearm.aimer.isAiming;
                            else
                                useCamera = true;
                            break;
                    }
                }
                if (useCamera)
                {
                    Transform aimTransform = firearm.wielder.fpCamera.aimTransform;
                    Vector3 startPosition = aimTransform.position;
                    Vector3 forwardVector = aimTransform.forward;

                    Transform ignoreRoot = GetRootTransform();

                    // Get the direction (with accuracy offset)
                    Vector3 rayDirection = forwardVector;
                    float spread = GetModifiedSpread(Mathf.Lerp(m_MinimumSpread, m_MaximumSpread, 1f - accuracy));
                    if (spread > 0.0001f)
                    {
                        Quaternion randomRot = UnityEngine.Random.rotationUniform;
                        rayDirection = Quaternion.Slerp(Quaternion.identity, randomRot, spread / 360f) * forwardVector;
                    }

                    Ray ray = new Ray(startPosition, rayDirection);
                    Vector3 hitPoint;
                    if (PhysicsExtensions.RaycastNonAllocSingle(ray, out m_Hit, k_MaxDistance, m_Layers, ignoreRoot, QueryTriggerInteraction.Ignore))
                        hitPoint = m_Hit.point;
                    else
                        hitPoint = startPosition + (rayDirection * k_MaxDistance);

                    Vector3 muzzlePosition = m_MuzzleTip.position;
                    Vector3 newDirection = hitPoint - muzzlePosition;
                    newDirection.Normalize();
                    projectile.Fire(muzzlePosition, newDirection * m_MuzzleSpeed, m_Gravity, effect, firearm.wielder.gameObject.transform, m_Layers, firearm as IDamageSource);
                    projectile.gameObject.SetActive(true);
                }
                else
                {
                    // Get the direction (with accuracy offset)
                    Vector3 rayDirection = m_MuzzleTip.forward;
                    float spread = GetModifiedSpread(Mathf.Lerp(m_MinimumSpread, m_MaximumSpread, 1f - accuracy));
                    if (spread > 0.0001f)
                    {
                        Quaternion randomRot = UnityEngine.Random.rotationUniform;
                        rayDirection = Quaternion.Slerp(Quaternion.identity, randomRot, spread / 360f) * rayDirection;
                    }

                    Transform ignoreRoot = (firearm.wielder == null) ? null : firearm.wielder.gameObject.transform;
                    projectile.Fire(m_MuzzleTip.position, rayDirection * m_MuzzleSpeed, m_Gravity, effect, ignoreRoot, m_Layers, firearm as IDamageSource);
                    projectile.gameObject.SetActive(true);
                }
            }

            base.Shoot(accuracy, effect);
        }

        protected virtual void InitialiseProjectile(IProjectile projectile)
        {
            if (m_TargetingSystem != null)
            {
                var tracker = projectile.gameObject.GetComponent<ITargetTracker>();
                if (tracker != null)
                    m_TargetingSystem.RegisterTracker(tracker);
            }
        }

        Transform GetRootTransform()
        {
            var t = transform;
            while (t.parent != null)
                t = t.parent;
            return t;
        }

        private static readonly NeoSerializationKey k_LayersKey = new NeoSerializationKey("layers");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);
            writer.WriteValue(k_LayersKey, m_Layers);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);
            int layers = m_Layers;
            if (reader.TryReadValue(k_LayersKey, out layers, layers))
                collisionLayers = layers;

        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using UnityEngine.PlayerLoop;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Parameters/GetWallHeightBehaviour", "GetWallHeightBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-modifyfloatparameterbehaviour.html")]
    public class GetWallHeightBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("How far in front of the player to check from.")]
        private float distanceForwardParameter = 1f;

        [SerializeField, Tooltip("How far above the player to check from.")]
        private float checkHeightParameter = 5f;

        [SerializeField, Tooltip("The radius of the spherecast")]
        private float spherecastRadius = 1f;

        [SerializeField, Tooltip("When should the modification happen.")]
        private When m_When = When.OnEnter;

        [SerializeField, Tooltip("The wall's normal vector.")]
        private VectorParameter m_WallNormalParameter = null;
        [SerializeField, Tooltip("The parameter to write to.")]
        private FloatParameter m_WallHeightParameter = null;

        [SerializeField, Tooltip("Layermask for the spherecast")]
        private LayerMask m_WallCollisionMask = 0;

        [SerializeField, Tooltip("Animator variable to write to")]
        private string m_ParameterName;
        private int m_ParameterHash;
        public enum When
        {
            OnEnter,
            OnExit,
            OnUpdate,
            Both
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            if (controller.bodyAnimator != null && !string.IsNullOrWhiteSpace(m_ParameterName))
                m_ParameterHash = Animator.StringToHash(m_ParameterName);
            else
                enabled = false;
        }

        public override void OnEnter()
        {
            if (m_WallHeightParameter != null && controller.transform != null && (m_When == When.OnEnter || m_When == When.Both))
            {
                RaycastHit hit;
                Ray detectionRay = new Ray(controller.transform.position + Vector3.up * checkHeightParameter - m_WallNormalParameter.value * distanceForwardParameter, Vector3.down);
                if (Physics.SphereCast(detectionRay, spherecastRadius, out hit, checkHeightParameter, m_WallCollisionMask))
                {
                    float height = hit.point.y - controller.transform.position.y;
                    if (height > 0)
                    {
                        m_WallHeightParameter.value = height;
                        controller.bodyAnimator.SetFloat(m_ParameterHash, height);
                    }
                }
                else
                {
                    m_WallHeightParameter.value = 5;
                    controller.bodyAnimator.SetFloat(m_ParameterHash, 5);
                }
            }
        }

        public override void OnExit()
        {
            if (m_WallHeightParameter != null && controller.transform != null && (m_When == When.OnExit || m_When == When.Both))
            {
                RaycastHit hit;
                Ray detectionRay = new Ray(controller.transform.position + Vector3.up * checkHeightParameter + controller.transform.forward * distanceForwardParameter, Vector3.down);
                if (Physics.SphereCast(detectionRay, spherecastRadius, out hit, checkHeightParameter, m_WallCollisionMask))
                {
                    float height = hit.point.y - controller.transform.position.y;
                    if (height > 0)
                    {
                        m_WallHeightParameter.value = height;
                    }
                }
                else
                {
                    m_WallHeightParameter.value = 5;
                    controller.bodyAnimator.SetFloat(m_ParameterHash, 5);
                }
            }
        }
        
        

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_WallHeightParameter = map.Swap(m_WallHeightParameter);
        }
    }
}
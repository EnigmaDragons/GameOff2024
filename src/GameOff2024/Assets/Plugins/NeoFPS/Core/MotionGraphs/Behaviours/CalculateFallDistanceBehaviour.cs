using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Parameters/CalculateFallDistance", "CalculateFallDistanceParameter")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-modifyfloatparameterbehaviour.html")]
    public class CalculateFallDistanceBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("The parameter to read from for initial y position.")]
        private FloatParameter m_Parameter_Read = null;

        [SerializeField, Tooltip("The parameter to write to for distance fallen.")]
        private FloatParameter m_Distance_Fallen_Parameter = null;

        [SerializeField, Tooltip("When should the modification happen.")]
        private When m_When = When.OnEnter;

        [SerializeField, Tooltip("The value to set to, add or subtract based on the \"What\" parameter.")]
        private float m_Value = 0f;

        public enum When
        {
            OnEnter,
            OnExit,
            Both
        }

        public override void OnEnter()
        {
            if (m_Parameter_Read != null && m_Distance_Fallen_Parameter != null && (m_When == When.OnEnter || m_When == When.Both))
            {
                m_Distance_Fallen_Parameter.value = controller.GetComponent<MotionController>().distanceFallen; // m_Parameter_Read.value - controller.transform.position.y;
            }
        }

        public override void OnExit()
        {
            if (m_Parameter_Read != null && m_Distance_Fallen_Parameter != null && (m_When == When.OnExit || m_When == When.Both))
            {
                m_Distance_Fallen_Parameter.value = controller.GetComponent<MotionController>().distanceFallen;
            }
        }
        

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Distance_Fallen_Parameter = map.Swap(m_Distance_Fallen_Parameter);
        }
    }
}
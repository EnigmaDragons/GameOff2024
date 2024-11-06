#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(CalculateFallDistanceBehaviour))]
    public class CalculateDistanceFallenBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(
                owner.container,
                serializedObject.FindProperty("m_Parameter_Read"),
                new GUIContent("Initial Height")
                );
            MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(
                owner.container,
                serializedObject.FindProperty("m_Distance_Fallen_Parameter"),
                new GUIContent("Write To")
                );

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));

        }
    }
}

#endif

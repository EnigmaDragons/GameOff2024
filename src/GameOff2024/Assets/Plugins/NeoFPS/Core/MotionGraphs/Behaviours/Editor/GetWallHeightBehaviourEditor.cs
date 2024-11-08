#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;
using System.ComponentModel;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(GetWallHeightBehaviour))]
    public class GetWallHeightBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Mantling Properties", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("distanceForwardParameter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("checkHeightParameter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spherecastRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_WallCollisionMask"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ParameterName"));

            MotionGraphEditorGUI.ParameterDropdownField<FloatParameter>(
                owner.container,
                serializedObject.FindProperty("m_WallHeightParameter"),
                new GUIContent("Parameter to write To")
                );

            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(
                owner.container,
                serializedObject.FindProperty("m_WallNormalParameter"),
                new GUIContent("Wall's normal vector")
                );

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));

        }
    }
}

#endif

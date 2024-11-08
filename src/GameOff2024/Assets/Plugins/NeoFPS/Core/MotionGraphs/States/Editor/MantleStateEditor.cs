#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(MantleState))]
    //[HelpURL("")]
    public class MantleStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(container, serializedObject.FindProperty("m_WallNormal"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_WallCheckDistance"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_ClimbSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_TopSpeed"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mantling Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_WallCollisionMask"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StartingSpeedMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EndingSpeedMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OvershootDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("horizontalVelocityCurveMidpoint"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("horizontalVelocityCurveSteepness"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("horizontalVelocityCurveMaxvalue"));
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(
                container,
                serializedObject.FindProperty("m_PreviousVelocity"),
                new GUIContent("Previous Velocity")
                );
        }
    }
}

#endif
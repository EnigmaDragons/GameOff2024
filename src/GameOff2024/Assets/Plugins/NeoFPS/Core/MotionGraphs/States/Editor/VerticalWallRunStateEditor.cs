#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(VerticalWallRunState))]
    //[HelpURL("")]
    public class VerticalWallRunStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            // Transform parameter
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(container, serializedObject.FindProperty("m_WallNormal"));

            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_UpBoost"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_MaxBoostSpeed"));
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_GravityMultiplier"));


            /*
            EditorGUILayout.LabelField("Wall Height Tracking Properties", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("distanceForwardParameter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("checkHeightParameter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spherecastRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_WallCollisionMask"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_WallHeightParameter"));
            */
        }
    }
}

#endif
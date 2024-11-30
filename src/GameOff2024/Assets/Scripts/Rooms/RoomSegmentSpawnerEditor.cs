#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomSegmentSpawner))]
public class RoomSegmentSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomSegmentSpawner spawner = (RoomSegmentSpawner)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Auto-Assign Slots"))
        {
            var slotsComponents = spawner.GetComponentsInChildren<RoomSegmentSlot>();
            if (slotsComponents != null && slotsComponents.Length > 0)
            {
                var serializedObject = new SerializedObject(spawner);
                var slotsProperty = serializedObject.FindProperty("slots");
                slotsProperty.arraySize = slotsComponents.Length;
                
                for (int i = 0; i < slotsComponents.Length; i++)
                {
                    var element = slotsProperty.GetArrayElementAtIndex(i);
                    element.objectReferenceValue = slotsComponents[i];
                }
                
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(spawner);
            }
            else
            {
                Debug.LogWarning("No RoomSegmentSlot components found in children");
            }
        }
    }
}
#endif
